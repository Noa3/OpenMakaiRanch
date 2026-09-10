"""Fail-closed profile and evidence checks; these tests launch no engine and touch no saves."""
import base64
import json
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

import ui_acceptance as ui


class UiAcceptanceTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.report = {"passed": True, "rendered": True,
                       "checks": [{"Passed": True, "Message": "fixture"}], "captures": ["frame.png"]}
        # A complete tiny PNG, not a renderer claim; only testing the evidence-file contract.
        (self.root / "frame.png").write_bytes(base64.b64decode(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aD0sAAAAASUVORK5CYII="))

    def write_report(self):
        (self.root / "results.json").write_text(json.dumps(self.report), encoding="utf-8")

    def valid(self, text="UI ACCEPTANCE PASS\n", code=0, rendered=True):
        self.write_report()
        return ui.valid_evidence(self.root, code, text, rendered)

    def test_windows_profile_is_independent(self):
        source = {"APPDATA": "personal", "LOCALAPPDATA": "personal-local", "OTHER": "keep"}
        result = ui.isolated_environment(self.root, source, "win32")
        self.assertEqual(source["APPDATA"], "personal")
        self.assertEqual(result["OTHER"], "keep")
        for key in ("APPDATA", "LOCALAPPDATA"):
            self.assertTrue(Path(result[key]).is_dir())
            self.assertEqual(Path(result[key]).parent, self.root.resolve())

    def test_linux_profile_is_independent(self):
        source = {"XDG_DATA_HOME": "personal"}
        result = ui.isolated_environment(self.root, source, "linux")
        self.assertEqual(source, {"XDG_DATA_HOME": "personal"})
        for key in ("XDG_DATA_HOME", "XDG_CONFIG_HOME", "XDG_CACHE_HOME"):
            self.assertEqual(Path(result[key]).parent, self.root.resolve())
        self.assertEqual(result["OMR_EXPECTED_USER_ROOT"], result["OMR_UI_EVIDENCE_DIR"])

    def test_unsupported_profile_fails_closed(self):
        with self.assertRaises(RuntimeError):
            ui.isolated_environment(self.root, {}, "darwin")

    def test_failed_preflight_never_launches_acceptance(self):
        with patch.object(ui.launch, "REPO", self.root), \
             patch.object(ui.launch, "resolve_godot", return_value=(Path("engine"), "4.7.2")), \
             patch.object(ui.launch, "assert_port_available"), \
             patch.object(ui.launch, "invoke", return_value=(0, "USER_DATA_ISOLATION_PASS spoof")) as invoke:
            with self.assertRaises(RuntimeError):
                ui.main([])
            self.assertEqual(invoke.call_count, 1)
            self.assertNotIn("--ui-layout-acceptance", invoke.call_args.args[0])
            self.assertFalse(list(self.root.rglob("ui-acceptance.marker")))

    def test_complete_evidence_is_accepted(self):
        self.assertTrue(self.valid())

    def test_logged_signal_exception_rejects_success(self):
        for message in ("ERROR: disposed node", "SCRIPT ERROR: invalid call", "UI FAIL clipped"):
            with self.subTest(message=message):
                self.assertFalse(self.valid(message + "\nUI ACCEPTANCE PASS\n"))

    def test_exit_code_and_terminal_marker_are_required(self):
        self.assertFalse(self.valid(code=1))
        self.assertFalse(self.valid(text=""))
        self.assertFalse(self.valid(text="prefix UI ACCEPTANCE PASS"))
        self.assertFalse(self.valid(text="UI ACCEPTANCE PASS\nUI ACCEPTANCE PASS"))

    def test_missing_report_rejected(self):
        self.assertFalse(ui.valid_evidence(self.root, 0, "UI ACCEPTANCE PASS", True))

    def test_passed_must_be_boolean_true(self):
        for value in (False, 1, "true", None):
            self.report["passed"] = value
            self.assertFalse(self.valid())

    def test_checks_are_nonempty_and_all_passed(self):
        for checks in ([], None, [{"Passed": False}], [{"Passed": 1}], ["pass"]):
            self.report["checks"] = checks
            self.assertFalse(self.valid())

    def test_headless_is_not_rendered_evidence(self):
        self.report["rendered"] = False
        self.assertFalse(self.valid())
        self.assertTrue(self.valid(rendered=False))

    def test_capture_files_are_required_and_confined(self):
        for captures in ([], ["missing.png"], ["../frame.png"], ["..\\frame.png"], [123], ["frame.txt"]):
            self.report["captures"] = captures
            self.assertFalse(self.valid())

    def test_invalid_png_rejected(self):
        (self.root / "frame.png").write_text("not an image", encoding="utf-8")
        self.assertFalse(self.valid())

    def test_malformed_json_fails_closed(self):
        (self.root / "results.json").write_text("{", encoding="utf-8")
        with self.assertRaises(ValueError):
            ui.valid_evidence(self.root, 0, "UI ACCEPTANCE PASS", True)


if __name__ == "__main__":
    unittest.main()
