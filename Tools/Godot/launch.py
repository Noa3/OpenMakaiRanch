"""Discover and run the project's stable Godot 4.7 .NET baseline (stdlib only)."""
from __future__ import annotations

import argparse
import json
import os
from pathlib import Path
import re
import shutil
import socket
import subprocess
import sys
import tempfile

REPO = Path(__file__).resolve().parents[2]
PROJECT = REPO / "OpenMakaiRanchGame"
VERSION = re.compile(r"^4\.7(?:\.\d+)?\.stable\.mono(?:\.|$)")
NAMES = ("Godot_v4.7-stable_mono_win64_console.exe", "Godot_v4.7-stable_mono_win64.exe")


def version_of(executable: Path) -> str:
    result = subprocess.run([str(executable), "--version"], capture_output=True,
                            text=True, encoding="utf-8", errors="replace", timeout=10)
    if result.returncode:
        raise RuntimeError(f"Version check failed ({result.returncode}): {executable}")
    return result.stdout.strip()


# A real Godot mono win64 build is ~150-200 MB. A file far smaller than this is a
# truncated/partial download (a "stub") that passes --version but crashes Mono init
# with "Assemblies not found". Reject it so discovery continues to a valid engine.
MIN_ENGINE_BYTES = 50 * 1024 * 1024  # 50 MB


def _version_tuple(version: str):
    parts = []
    for token in version.split("."):
        if token.isdigit():
            parts.append(int(token))
        else:
            break
    return tuple(parts)


def candidates(root: Path, environ: dict[str, str]):
    # Explicit paths fail closed. A typo must not silently select another engine.
    configured = environ.get("GODOT_BIN") or environ.get("GODOT_PATH")
    if configured:
        yield Path(configured).expanduser()
        return
    directories = []
    if environ.get("GODOT_HOME"):
        directories.append(Path(environ["GODOT_HOME"]))
    directories.extend((root, root / "tools" / "godot", root.parent))
    for key, suffix in (("LOCALAPPDATA", "Programs"), ("ProgramFiles", "")):
        base = Path(environ[key]) / suffix if environ.get(key) else None
        if base and base.is_dir():
            directories.extend(sorted(p for p in base.glob("*Godot*") if p.is_dir()))
    seen = set()
    for directory in directories:
        paths = [directory / name for name in NAMES]
        if directory.is_dir():
            # Nested extracted installs are common; discovery is deliberately bounded.
            paths += sorted(directory.glob("Godot_v4.7*-stable_mono_win64*.exe"))
            for child in sorted(directory.glob("Godot*")):
                if child.is_dir():
                    paths += sorted(child.glob("Godot_v4.7*-stable_mono_win64*.exe"))
        for path in paths:
            key = str(path.resolve()).casefold()
            if key not in seen and path.is_file():
                seen.add(key)
                yield path.resolve()
    for name in ("godot_mono", "godot", "Godot_v4.7-stable_mono_win64_console.exe"):
        found = shutil.which(name, path=environ.get("PATH", ""))
        if found and found.casefold() not in seen:
            seen.add(found.casefold())
            yield Path(found)


def resolve_godot(root: Path, environ: dict[str, str], probe=version_of):
    explicit = environ.get("GODOT_BIN") or environ.get("GODOT_PATH")
    rejected = []
    valid = {}
    for candidate in candidates(root, environ):
        try:
            if not candidate.is_file():
                raise RuntimeError("file does not exist")
            size = candidate.stat().st_size
            if size < MIN_ENGINE_BYTES:
                raise RuntimeError(
                    f"stub/partial download ({size} bytes < {MIN_ENGINE_BYTES}); "
                    "passes --version but crashes Mono init")
            version = probe(candidate)
            if not VERSION.match(version):
                raise RuntimeError(f"expected stable Godot 4.7 .NET, got {version!r}")
            key = str(candidate.resolve()).casefold()
            if key not in valid:
                valid[key] = (candidate.resolve(), version)
        except (OSError, RuntimeError, subprocess.TimeoutExpired) as error:
            rejected.append(f"{candidate}: {error}")
    if explicit:
        # Fail closed: an explicit path must be the one we run.
        for key, (path, version) in valid.items():
            if key == str(Path(explicit).expanduser().resolve()).casefold():
                return path, version
        raise RuntimeError(
            f"Explicit Godot {explicit} was rejected.\n" + "\n".join(rejected))
    if not valid:
        raise RuntimeError(
            "No compatible Godot found. Set GODOT_BIN to a stable 4.7 .NET executable.\n"
            + "\n".join(rejected))
    # Prefer the newest engine (e.g. 4.7.2 over a broken 4.7.0 in the repo root).
    best = max(valid.values(), key=lambda item: _version_tuple(item[1]))
    return best[0], best[1]


def assert_port_available(port: int):
    with socket.socket() as connection:
        connection.settimeout(0.5)
        if connection.connect_ex(("127.0.0.1", port)) == 0:
            raise RuntimeError(f"Bridge port {port} is already in use; inspect its owner before launching another instance.")


def invoke(command, env, timeout, output: Path):
    print("Command: " + subprocess.list2cmdline([str(x) for x in command]), flush=True)
    try:
        result = subprocess.run([str(x) for x in command], env=env, cwd=REPO,
                                stdout=subprocess.PIPE, stderr=subprocess.STDOUT,
                                encoding="utf-8", errors="replace", timeout=timeout)
    except subprocess.TimeoutExpired as error:
        text = error.stdout or b""
        output.write_bytes(text.encode("utf-8") if isinstance(text, str) else text)
        raise RuntimeError(f"Godot timed out after {timeout}s; captured output: {output}") from error
    output.write_text(result.stdout, encoding="utf-8")
    print(result.stdout, end="", flush=True)
    return result.returncode, result.stdout


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--mode", choices=("check", "editor", "runtime", "import", "smoke"), default="editor")
    parser.add_argument("--godot", type=Path, help="Explicit executable; overrides GODOT_BIN/GODOT_PATH")
    parser.add_argument("--isolated", action="store_true", help="Use a fresh test profile; mandatory and automatic for smoke")
    parser.add_argument("--timeout", type=int, default=180, help="Timeout for import/smoke, in seconds")
    args = parser.parse_args(argv)
    if args.timeout <= 0:
        parser.error("--timeout must be positive")
    env = dict(os.environ)
    if args.godot:
        env["GODOT_BIN"] = str(args.godot.resolve())
    executable, version = resolve_godot(REPO, env)
    if not (PROJECT / "project.godot").is_file():
        raise RuntimeError(f"Missing project.godot: {PROJECT}")
    if args.mode == "check":
        print(json.dumps({"executable": str(executable), "version": version, "project": str(PROJECT)}, indent=2))
        return 0
    print(f"Godot: {executable}\nVersion: {version}\nProject: {PROJECT}", flush=True)
    assert_port_available(9500 if args.mode in ("editor", "import") else 9501)
    artifacts = REPO / ".artifacts" / "godot"
    artifacts.mkdir(parents=True, exist_ok=True)
    run_dir = Path(tempfile.mkdtemp(prefix=args.mode + "-", dir=artifacts))
    isolated = args.isolated or args.mode == "smoke"
    if isolated:
        if sys.platform != "win32":
            raise RuntimeError("Isolated profile currently validated on Windows only; refusing unsafe smoke run.")
        for key in ("APPDATA", "LOCALAPPDATA"):
            path = run_dir / key.lower()
            path.mkdir()
            env[key] = str(path)
        env["OMR_EXPECTED_USER_ROOT"] = str(run_dir)
        # Engine-resolved user:// is checked BEFORE any test receives --run-smoke-tests.
        code, text = invoke([executable, "--headless", "--path", PROJECT,
                             "--script", Path(__file__).with_name("check_user_data.gd")],
                            env, 30, run_dir / "profile-check.log")
        if code or "USER_DATA_ISOLATION_PASS" not in text:
            raise RuntimeError(f"Test profile isolation failed; no smoke tests were started. See {run_dir}")
    engine_args = [executable, "--path", PROJECT, "--log-file", run_dir / "engine.log"]
    if args.mode == "editor":
        engine_args += ["--editor", "res://scenes/Bootstrap.tscn"]
    elif args.mode == "import":
        engine_args += ["--headless", "--import"]
    elif args.mode == "smoke":
        engine_args += ["--headless", "--", "--run-smoke-tests"]
    print(f"Evidence: {run_dir}", flush=True)
    code, text = invoke(engine_args, env, None if args.mode in ("editor", "runtime") else args.timeout,
                        run_dir / "console.log")
    if args.mode == "smoke":
        passed = "SMOKE PASS" in text.splitlines()
        if not passed or "SMOKE FAIL" in text:
            return code or 1
    return code


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except (OSError, RuntimeError) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        raise SystemExit(1)
