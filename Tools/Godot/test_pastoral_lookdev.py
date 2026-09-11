"""Network-free checks for the additional, still autoload-free pastoral study."""
from pathlib import Path
import tempfile
import unittest
import anime_lookdev as lab


class PastoralRunnerTests(unittest.TestCase):
    def test_original_and_pastoral_evidence_are_distinct(self):
        self.assertEqual(len(lab.PASTORAL_CAPTURES), 7)
        self.assertFalse(lab.PASTORAL_CAPTURES & lab.EXPECTED_CAPTURES)

    def test_pastoral_stage_remains_isolated(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            for name in ("src/Visuals/Lab.cs", "src/App/GraphicsQualityProfile.cs",
                         "shaders/characters/anime_basic.gdshader", "scenes/dev/AnimeLookDevPastoral.tscn"):
                file = root / lab.GAME / name
                file.parent.mkdir(parents=True, exist_ok=True)
                file.write_text("fixture", encoding="utf-8")
            forbidden = root / lab.GAME / "src/App/GameRoot.cs"
            forbidden.write_text("not allowed", encoding="utf-8")
            target = root / "pastoral"
            lab.stage(root, target, "forward_plus", True, pastoral=True)
            self.assertIn("AnimeLookDevPastoral.tscn", (target / "project.godot").read_text())
            self.assertNotIn("[autoload]", (target / "project.godot").read_text())
            self.assertFalse((target / "src/App/GameRoot.cs").exists())
