#!/usr/bin/env python3
"""Stage an autoload-free material lab; never launch tests against a player's game/profile.

Use a desktop display, or xvfb-run on Linux. Godot and .NET must already be installed.
Only whitelisted rendering source is copied. This tool does not download engines or assets.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import struct
import subprocess
import sys
import uuid

GAME = "OpenMakaiRanchGame"
EXPECTED_CAPTURES = {
    "neutral-high.png", "neutral-matte.png", "daylight-high.png", "interior-high.png",
    "sunset-high.png", "night-high.png", "neutral-low.png", "neutral-medium.png",
    "neutral-ultra.png", "portrait-high.png", "portrait-rotated-high.png",
    "portrait-blue-iris-high.png", "portrait-unmasked-high.png", "portrait-uv-shift-high.png",
    "portrait-profile-high.png", "portrait-night-high.png",
    "presence-neutral.png", "presence-blink.png", "presence-warm.png",
    "makai-off.png", "makai-night.png",
}
PASTORAL_CAPTURES = {
    "pastoral-day.png", "pastoral-night.png", "pastoral-moon-only.png", "pastoral-overcast.png",
    "pastoral-low.png", "pastoral-lantern-on.png", "pastoral-lantern-off.png",
}
PROJECT = '''config_version=5
[application]
config/name="OpenMakaiRanchLookDevValidation"
run/main_scene="res://scenes/dev/{scene}.tscn"
config/features=PackedStringArray("4.7", "C#", "Forward Plus")
[display]
window/size/viewport_width=1280
window/size/viewport_height=720
[dotnet]
project/assembly_name="AnimeLookDevValidation"
[rendering]
renderer/rendering_method="{renderer}"
'''
CSPROJ = '''<Project Sdk="Godot.NET.Sdk/4.7.0">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <AssemblyName>AnimeLookDevValidation</AssemblyName>
    <Nullable>enable</Nullable><LangVersion>12.0</LangVersion>
    <EnableDynamicLoading>true</EnableDynamicLoading>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
    <GenerateTargetFrameworkAttribute>false</GenerateTargetFrameworkAttribute>
    <DefaultItemExcludes>$(DefaultItemExcludes);.godot/**;.mono/**</DefaultItemExcludes>
  </PropertyGroup>
  <ItemGroup><Compile Remove=".godot/**/*.cs"/><Compile Remove=".mono/**/*.cs"/></ItemGroup>
</Project>
'''


def stage(root: Path, destination: Path, renderer: str, interactive: bool, *, pastoral: bool = False) -> None:
    """Create a new host, refusing overwrite and all source-tree escape/symlink cases."""
    if renderer not in {"forward_plus", "gl_compatibility"}:
        raise ValueError("Unsupported renderer")
    source = (root / GAME).resolve(strict=True)
    patterns = ("src/Visuals/*.cs", "src/App/GraphicsQualityProfile.cs",
                "shaders/characters/anime_*.gdshader*", "scenes/dev/AnimeLookDev*.tscn")
    files: set[Path] = set()
    for pattern in patterns:
        found = list(source.glob(pattern))
        if not found:
            raise ValueError(f"Missing rendering source: {pattern}")
        for file in found:
            if not file.is_file() or file.is_symlink() or not file.resolve().is_relative_to(source):
                raise ValueError(f"Unsafe source: {file}")
            files.add(file)
    destination.mkdir(parents=True, exist_ok=False)
    for file in sorted(files):
        target = destination / file.relative_to(source)
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(file, target)
    scene = ("AnimeLookDevPastoral" if interactive else "AnimeLookDevPastoralChecks") if pastoral else ("AnimeLookDev" if interactive else "AnimeLookDevChecks")
    (destination / "project.godot").write_text(PROJECT.format(
        scene=scene, renderer=renderer), encoding="utf-8")
    (destination / "AnimeLookDevValidation.csproj").write_text(CSPROJ, encoding="utf-8")


def isolated_environment(run: Path, renderer: str, run_id: str, commit: str) -> dict[str, str]:
    env = dict(os.environ)
    for key, suffix in (("APPDATA", "roaming"), ("LOCALAPPDATA", "local"),
                        ("XDG_DATA_HOME", "data"), ("XDG_CONFIG_HOME", "config"),
                        ("XDG_CACHE_HOME", "cache")):
        path = run / "profile" / suffix
        path.mkdir(parents=True, exist_ok=True)
        env[key] = str(path)
    output = run / "evidence"
    output.mkdir(exist_ok=False)
    (output / "owner.txt").write_text(run_id, encoding="utf-8")
    env.update(OMR_LOOKDEV_OUTPUT=str(output), OMR_LOOKDEV_RUN_ID=run_id,
               OMR_LOOKDEV_RENDERER=renderer, OMR_LOOKDEV_SOURCE_COMMIT=commit)
    return env


def errors_in_log(text: str, *, importing: bool = False) -> list[str]:
    errors = []
    for line in text.splitlines():
        if re.search(r"(?:^|\s)(?:ERROR:|SCRIPT ERROR:|SHADER ERROR:)", line, re.I):
            # Existing editor shutdown issue is NOT an allowed runtime/shader error.
            if importing and 'EditorSettings not instantiated' in line and 'export/android/shutdown_adb_on_exit' in line:
                continue
            errors.append(line)
    return errors


def run_command(args: list[str], cwd: Path, env: dict[str, str], log: Path, timeout: int,
                *, importing: bool = False) -> None:
    print("+ " + " ".join(args), flush=True)
    with log.open("w", encoding="utf-8") as stream:
        try:
            completed = subprocess.run(args, cwd=cwd, env=env, stdout=stream,
                                       stderr=subprocess.STDOUT, text=True, timeout=timeout, check=False)
        except subprocess.TimeoutExpired as exc:
            raise RuntimeError(f"Timed out after {timeout}s; full log: {log}") from exc
    text = log.read_text(encoding="utf-8", errors="replace")
    print(text[-16000:], flush=True)
    if completed.returncode or errors_in_log(text, importing=importing):
        raise RuntimeError(f"Command failed ({completed.returncode}); full unfiltered log: {log}")


def validate_evidence(output: Path, renderer: str, run_id: str, commit: str,
                      *, expected_captures: set[str] | None = None) -> dict:
    expected = EXPECTED_CAPTURES if expected_captures is None else expected_captures
    result = json.loads((output / "results.json").read_text(encoding="utf-8"))
    if (result.get("schema") != 1 or result.get("passed") is not True or result.get("failures") != 0
            or result.get("run_id") != run_id or result.get("renderer") != renderer
            or result.get("source_commit") != commit):
        raise ValueError("Failed, stale or mismatched rendered results")
    checks = result.get("checks", [])
    if len(checks) < 40 or not all(check.get("passed") is True for check in checks):
        raise ValueError("Missing or failing material checks")
    captures = result.get("captures", [])
    if len(captures) != len(expected) or {c.get("file") for c in captures} != expected:
        raise ValueError("Missing, duplicate or unexpected captures")
    for capture in captures:
        path = output / capture["file"]
        if path.is_symlink() or path.parent != output:
            raise ValueError("Unsafe evidence path")
        raw = path.read_bytes()
        if len(raw) < 100 or raw[:8] != b"\x89PNG\r\n\x1a\n" or raw[12:16] != b"IHDR":
            raise ValueError(f"Invalid PNG: {path.name}")
        width, height = struct.unpack(">II", raw[16:24])
        if width < 320 or height < 240 or (width, height) != (capture.get("width"), capture.get("height")):
            raise ValueError(f"Mismatched PNG dimensions: {path.name}")
    hashes = {path.name: hashlib.sha256(path.read_bytes()).hexdigest()
              for path in sorted(output.iterdir()) if path.is_file() and path.name != "sha256.json"}
    (output / "sha256.json").write_text(json.dumps(hashes, indent=2), encoding="utf-8")
    return result


def runtime_arguments(engine: str, project: Path, renderer: str) -> list[str]:
    # This is a silent rendering lab, not an audio test. CI has no physical sound device.
    # Select Dummy explicitly instead of tolerating an ALSA initialization ERROR in the log.
    return [engine, "--path", str(project), "--rendering-method", renderer,
            "--audio-driver", "Dummy", "--disable-vsync"]


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--godot", default=os.environ.get("GODOT_BIN"))
    parser.add_argument("--renderer", choices=("forward_plus", "gl_compatibility"), default="forward_plus")
    parser.add_argument("--timeout", type=int, default=420)
    parser.add_argument("--interactive", action="store_true", help="Open the lab instead of running acceptance; no game autoloads")
    parser.add_argument("--pastoral", action="store_true", help="Study the revised green-ranch sky and mana-stone lamps")
    args = parser.parse_args(argv)
    if args.timeout < 10:
        parser.error("--timeout must be at least 10 seconds")
    root = Path(__file__).resolve().parents[2]
    engine = args.godot or shutil.which("godot") or shutil.which("godot-mono")
    if not engine or not shutil.which("dotnet"):
        parser.error("Install the existing Godot 4.7 stable Mono and .NET SDK; set GODOT_BIN or --godot")
    version = subprocess.run([engine, "--version"], capture_output=True, text=True, timeout=30, check=True).stdout.strip()
    if not re.match(r"^4\.7(?:\.\d+)?\.stable\.mono\b", version):
        parser.error(f"Expected stable Godot 4.7 Mono, found {version!r}; no engine upgrade is performed")
    commit_proc = subprocess.run(["git", "rev-parse", "HEAD"], cwd=root, capture_output=True, text=True, check=True)
    commit = commit_proc.stdout.strip()
    run_id = uuid.uuid4().hex
    run = root / ".artifacts" / "anime-lookdev" / (("pastoral-" if args.pastoral else "") + args.renderer + "-" + run_id)
    run.mkdir(parents=True, exist_ok=False)
    stage(root, run / "project", args.renderer, args.interactive, pastoral=args.pastoral)
    env = isolated_environment(run, args.renderer, run_id, commit)
    output = run / "evidence"
    (output / "source.json").write_text(json.dumps({"source_commit": commit, "engine": version,
        "run_id": run_id, "renderer": args.renderer, "host": "autoload-free whitelist"}, indent=2), encoding="utf-8")
    project = run / "project"
    run_command(["dotnet", "build", "AnimeLookDevValidation.csproj"], project, env, output / "build.log", args.timeout)
    run_command([engine, "--headless", "--editor", "--path", str(project), "--import"], project, env,
                output / "import.log", args.timeout, importing=True)
    run_command(runtime_arguments(engine, project, args.renderer),
                project, env, output / "runtime.log", args.timeout)
    if not args.interactive:
        result = validate_evidence(output, args.renderer, run_id, commit,
                                   expected_captures=PASTORAL_CAPTURES if args.pastoral else None)
        print(f"PASS: {len(result['checks'])} checks, {len(result['captures'])} rendered PNGs; {output}")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except (OSError, ValueError, RuntimeError, subprocess.SubprocessError) as error:
        print(f"LOOKDEV FAILED: {error}", file=sys.stderr)
        raise SystemExit(1)
