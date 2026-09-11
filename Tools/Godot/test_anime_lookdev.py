"""Network-free staging/evidence safety tests. These do not pretend to render shaders."""
import json
import struct
import tempfile
from pathlib import Path
import unittest

import anime_lookdev as lab


class LookDevRunnerTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)

    def source(self):
        for name in ("src/Visuals/Lab.cs", "src/App/GraphicsQualityProfile.cs",
                     "shaders/characters/anime_basic.gdshader", "scenes/dev/AnimeLookDev.tscn"):
            file = self.root / lab.GAME / name
            file.parent.mkdir(parents=True, exist_ok=True)
            file.write_text("fixture", encoding="utf-8")
        forbidden = self.root / lab.GAME / "src/App/GameRoot.cs"
        forbidden.write_text("must not be staged", encoding="utf-8")

    def evidence(self):
        result = dict(schema=1, passed=True, failures=0, run_id="test", renderer="forward_plus",
                      source_commit="abc", checks=[dict(name=f"check{i}", passed=True) for i in range(40)],
                      captures=[dict(file=name, width=640, height=480) for name in sorted(lab.EXPECTED_CAPTURES)])
        for name in lab.EXPECTED_CAPTURES:
            (self.root / name).write_bytes(b"\x89PNG\r\n\x1a\n" + struct.pack(">I", 13) + b"IHDR" + struct.pack(">II", 640, 480) + b"x" * 100)
        self.write_result(result)
        return result

    def write_result(self, result):
        (self.root / "results.json").write_text(json.dumps(result), encoding="utf-8")

    def test_stage_is_whitelisted_and_has_no_autoload(self):
        self.source()
        target = self.root / "host"
        lab.stage(self.root, target, "forward_plus", False)
        self.assertFalse((target / "src/App/GameRoot.cs").exists())
        project = (target / "project.godot").read_text()
        self.assertNotIn("[autoload]", project)
        self.assertIn("AnimeLookDevChecks", project)
        self.assertIn("12.0", (target / "AnimeLookDevValidation.csproj").read_text())

    def test_stage_refuses_overwrite(self):
        self.source()
        target = self.root / "host"
        target.mkdir()
        with self.assertRaises(FileExistsError):
            lab.stage(self.root, target, "forward_plus", False)

    def test_stage_requires_source(self):
        (self.root / lab.GAME).mkdir()
        with self.assertRaises(ValueError):
            lab.stage(self.root, self.root / "host", "forward_plus", False)

    def test_stage_rejects_unknown_renderer(self):
        with self.assertRaises(ValueError):
            lab.stage(self.root, self.root / "host", "dummy", False)

    def test_stage_refuses_symlink(self):
        self.source()
        outside = self.root / "secret"
        outside.write_text("not source")
        try:
            (self.root / lab.GAME / "src/Visuals/Escape.cs").symlink_to(outside)
        except OSError:
            self.skipTest("Symlinks unavailable")
        with self.assertRaises(ValueError):
            lab.stage(self.root, self.root / "host", "forward_plus", False)

    def test_interactive_host_is_still_autoload_free(self):
        self.source()
        target = self.root / "host"
        lab.stage(self.root, target, "gl_compatibility", True)
        project = (target / "project.godot").read_text()
        self.assertNotIn("Checks.tscn", project)
        self.assertNotIn("[autoload]", project)

    def test_environment_isolates_directories(self):
        env = lab.isolated_environment(self.root, "forward_plus", "test", "abc")
        self.assertEqual((self.root / "evidence/owner.txt").read_text(), "test")
        for key in ("APPDATA", "LOCALAPPDATA", "XDG_DATA_HOME", "XDG_CONFIG_HOME", "XDG_CACHE_HOME"):
            self.assertTrue(Path(env[key]).is_relative_to(self.root))
        with self.assertRaises(FileExistsError):
            lab.isolated_environment(self.root, "forward_plus", "other", "abc")

    def test_runtime_error_is_never_allowed(self):
        message = 'ERROR: EditorSettings not instantiated yet when getting setting "export/android/shutdown_adb_on_exit"'
        self.assertTrue(lab.errors_in_log(message))
        self.assertFalse(lab.errors_in_log(message, importing=True))
        self.assertTrue(lab.errors_in_log("SCRIPT ERROR: Shader compilation failed", importing=True))

    def test_good_evidence_and_hash_manifest(self):
        self.evidence()
        lab.validate_evidence(self.root, "forward_plus", "test", "abc")
        hashes = json.loads((self.root / "sha256.json").read_text())
        self.assertIn("results.json", hashes)
        self.assertEqual(len(hashes["neutral-high.png"]), 64)

    def test_stale_evidence_is_refused(self):
        self.evidence()
        for renderer, run_id, commit in (("gl_compatibility", "test", "abc"), ("forward_plus", "old", "abc"), ("forward_plus", "test", "def")):
            with self.assertRaises(ValueError):
                lab.validate_evidence(self.root, renderer, run_id, commit)

    def test_failing_check_is_refused(self):
        result = self.evidence()
        result["checks"][0]["passed"] = False
        self.write_result(result)
        with self.assertRaises(ValueError):
            lab.validate_evidence(self.root, "forward_plus", "test", "abc")

    def test_bad_or_missing_capture_is_refused(self):
        result = self.evidence()
        result["captures"][0]["file"] = "../escape.png"
        self.write_result(result)
        with self.assertRaises(ValueError):
            lab.validate_evidence(self.root, "forward_plus", "test", "abc")

    def test_fake_image_is_refused(self):
        self.evidence()
        (self.root / "neutral-high.png").write_text("not a rendered image")
        with self.assertRaises(ValueError):
            lab.validate_evidence(self.root, "forward_plus", "test", "abc")

    def test_wrong_image_dimensions_are_refused(self):
        result = self.evidence()
        result["captures"][0]["width"] = 42
        self.write_result(result)
        with self.assertRaises(ValueError):
            lab.validate_evidence(self.root, "forward_plus", "test", "abc")


if __name__ == "__main__":
    unittest.main()
