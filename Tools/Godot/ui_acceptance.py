"""Run the opt-in UI acceptance scene in a verified disposable Windows/Linux profile."""
from __future__ import annotations

import argparse
import json
import os
import struct
from pathlib import Path
import sys
import tempfile

import launch


def isolated_environment(run_dir: Path, source: dict[str, str], platform: str) -> dict[str, str]:
    env = dict(source)
    if platform == "win32":
        keys = ("APPDATA", "LOCALAPPDATA")
    elif platform.startswith("linux"):
        keys = ("XDG_DATA_HOME", "XDG_CONFIG_HOME", "XDG_CACHE_HOME")
    else:
        raise RuntimeError("UI profile isolation is supported only on Windows and Linux.")
    for key in keys:
        path = run_dir / key.lower()
        path.mkdir(parents=True, exist_ok=True)
        env[key] = str(path.resolve())
    env["OMR_EXPECTED_USER_ROOT"] = str(run_dir.resolve())
    env["OMR_UI_EVIDENCE_DIR"] = str(run_dir.resolve())
    return env


def valid_evidence(run_dir: Path, code: int, text: str, require_rendered: bool) -> bool:
    lines = text.splitlines()
    if code or lines.count("UI ACCEPTANCE PASS") != 1:
        return False
    # Unlike the full smoke suite this UI scenario deliberately submits no invalid-save fixtures.
    # A C# exception inside a Godot signal may be logged without propagating into the test task.
    if any(line.lstrip().startswith(("ERROR:", "SCRIPT ERROR:", "UI FAIL ")) for line in lines):
        return False
    report_path = run_dir / "results.json"
    if not report_path.is_file():
        return False
    report = json.loads(report_path.read_text(encoding="utf-8"))
    if not isinstance(report, dict) or report.get("passed") is not True:
        return False
    checks = report.get("checks")
    if not isinstance(checks, list) or not checks or not all(
        isinstance(check, dict) and check.get("Passed") is True for check in checks
    ):
        return False
    if require_rendered:
        captures = report.get("captures")
        if report.get("rendered") is not True or not isinstance(captures, list) or not captures:
            return False
        for name in captures:
            if not isinstance(name, str) or Path(name).name != name or "\\" in name or not name.endswith(".png"):
                return False
            image_path = run_dir / name
            if not image_path.is_file():
                return False
            with image_path.open("rb") as image:
                header = image.read(24)
            if len(header) != 24 or header[:8] != b"\x89PNG\r\n\x1a\n" or header[12:16] != b"IHDR":
                return False
            if not all(0 < side <= 16384 for side in struct.unpack(">II", header[16:24])):
                return False
    return True


def main(argv=None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--rendered", action="store_true", help="Require a real renderer and PNG captures (no headless fallback).")
    parser.add_argument("--timeout", type=int, default=240)
    args = parser.parse_args(argv)
    if not 1 <= args.timeout <= 900:
        parser.error("--timeout must be between 1 and 900 seconds")
    executable, version = launch.resolve_godot(launch.REPO, dict(os.environ))
    launch.assert_port_available(9501)
    artifacts = launch.REPO / ".artifacts" / "godot"
    artifacts.mkdir(parents=True, exist_ok=True)
    run_dir = Path(tempfile.mkdtemp(prefix="ui-", dir=artifacts))
    env = isolated_environment(run_dir, dict(os.environ), sys.platform)
    code, text = launch.invoke(
        [executable, "--headless", "--path", launch.PROJECT,
         "--script", Path(__file__).with_name("check_user_data.gd")],
        env, 30, run_dir / "profile-check.log")
    if code or "USER_DATA_ISOLATION_PASS" not in text.splitlines():
        raise RuntimeError(f"UI profile isolation failed; acceptance was not started: {run_dir}")
    (run_dir / "ui-acceptance.marker").write_text("isolated-ui-acceptance-v1\n", encoding="utf-8")
    command = [executable, "--path", launch.PROJECT, "--log-file", run_dir / "engine.log",
               "--audio-driver", "Dummy", "--resolution", "1280x720"]
    if args.rendered:
        command += ["--rendering-method", "gl_compatibility", "--rendering-driver", "opengl3"]
    else:
        command += ["--headless"]
    command += ["res://scenes/dev/UiLayoutAcceptance.tscn", "--", "--ui-layout-acceptance"]
    if args.rendered:
        command += ["--require-ui-captures"]
    print(f"UI evidence: {run_dir}; engine: {version}", flush=True)
    code, text = launch.invoke(command, env, args.timeout, run_dir / "console.log")
    return 0 if valid_evidence(run_dir, code, text, args.rendered) else 1


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except (OSError, RuntimeError, ValueError) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        raise SystemExit(1)
