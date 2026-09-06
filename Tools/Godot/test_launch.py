"""Regression checks for discovery, version policy and launch failures."""
import os
from pathlib import Path
import subprocess
import tempfile
import unittest
from unittest.mock import patch

import launch


class LauncherTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix="omr-launch-")
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name) / "repo with spaces"
        self.root.mkdir()
        self.binary = self.root / launch.NAMES[0]
        self.binary.touch()

    def test_repo_relative_baseline(self):
        path, version = launch.resolve_godot(self.root, {"PATH": ""}, lambda _: "4.7.stable.mono.official.test")
        self.assertEqual(path, self.binary.resolve())
        self.assertTrue(version.startswith("4.7."))

    def test_explicit_configuration_has_priority(self):
        other = self.root / "custom.exe"
        other.touch()
        path, _ = launch.resolve_godot(self.root, {"GODOT_BIN": str(other)}, lambda _: "4.7.2.stable.mono.test")
        self.assertEqual(path, other.resolve())

    def test_invalid_explicit_path_does_not_fall_back(self):
        with self.assertRaisesRegex(RuntimeError, "file does not exist"):
            launch.resolve_godot(self.root, {"GODOT_BIN": str(self.root / "missing.exe")})

    def test_rejects_incompatible_versions(self):
        for version in ("4.6.3.stable.mono.test", "4.7.stable.official.test", "4.8.dev.mono.test", "4.7.rc1.mono.test"):
            with self.subTest(version=version), self.assertRaisesRegex(RuntimeError, "No compatible Godot"):
                launch.resolve_godot(self.root, {"GODOT_BIN": str(self.binary)}, lambda _: version)

    def test_hung_version_check_fails_closed(self):
        def timeout(_):
            raise subprocess.TimeoutExpired("godot --version", 10)
        with self.assertRaisesRegex(RuntimeError, "timed out"):
            launch.resolve_godot(self.root, {"GODOT_BIN": str(self.binary)}, timeout)

    def test_godot_home(self):
        home = self.root / "installed"
        home.mkdir()
        (home / launch.NAMES[0]).touch()
        path, _ = launch.resolve_godot(self.root, {"GODOT_HOME": str(home), "PATH": ""}, lambda _: "4.7.stable.mono.test")
        self.assertEqual(path.parent, home)

    def test_path_fallback(self):
        empty = self.root / "empty"
        empty.mkdir()
        # root.parent is also searched, so use a separate nested root.
        nested = empty / "nested"
        nested.mkdir()
        with patch("launch.shutil.which", return_value=str(self.binary)):
            path, _ = launch.resolve_godot(nested, {"PATH": "fake"}, lambda _: "4.7.stable.mono.test")
        self.assertEqual(path, self.binary)

    def test_port_collision_is_actionable(self):
        with launch.socket.socket() as server:
            server.bind(("127.0.0.1", 0))
            server.listen()
            with self.assertRaisesRegex(RuntimeError, "already in use"):
                launch.assert_port_available(server.getsockname()[1])

    def test_child_exit_code_and_log_are_preserved(self):
        output = self.root / "log.txt"
        code, text = launch.invoke([launch.sys.executable, "-c", "print('expected failure'); raise SystemExit(7)"],
                                   dict(os.environ), 10, output)
        self.assertEqual(code, 7)
        self.assertIn("expected failure", text)
        self.assertEqual(output.read_text(), text)


if __name__ == "__main__":
    unittest.main()
