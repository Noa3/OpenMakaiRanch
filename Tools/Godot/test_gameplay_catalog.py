"""Gameplay strings share the existing locale loader and export path, not UI layout files."""
import re
import unittest
from validate_locales import GAME, read_catalog, validate_pair


class GameplayCatalogTests(unittest.TestCase):
    def test_keys_and_slots(self):
        english = {k: v for k, v in read_catalog(GAME / "locale/en.json").items() if k.startswith("gameplay.")}
        german = {k: v for k, v in read_catalog(GAME / "locale/de.json").items() if k.startswith("gameplay.")}
        validate_pair(english, german)
        self.assertEqual(len(english), 13)
        for identity in ("restored_home", "home_cooking", "balanced_shift", "small_crew", "time_to_breathe"):
            for suffix in ("title", "requirement"):
                self.assertIn(f"gameplay.achievement.{identity}.{suffix}", english)

    def test_literal_gameplay_keys(self):
        catalog = read_catalog(GAME / "locale/en.json")
        for path in (GAME / "src/Gameplay").glob("*.cs"):
            for key in re.findall(r'T\("(gameplay\.[^"\n]+)"\s*,', path.read_text(encoding="utf-8")):
                self.assertIn(key, catalog, str(path))
