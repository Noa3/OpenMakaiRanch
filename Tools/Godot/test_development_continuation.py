"""Catalog receipts for gameplay feedback. C# behavior runs in the existing Godot smoke suite."""
import re
import unittest
from validate_locales import GAME, read_catalog, validate_pair


class DevelopmentFeedbackCatalogTests(unittest.TestCase):
    def test_translated_keys_and_slots(self):
        catalogs = [{k: v for k, v in read_catalog(GAME / f"locale/{locale}.json").items()
                     if k.startswith("development.")} for locale in ("en", "de")]
        self.assertEqual(len(catalogs[0]), 19)
        validate_pair(*catalogs)

    def test_every_literal_has_a_translator_entry(self):
        source = (GAME / "src/Gameplay/CharacterDevelopmentFeedback.cs").read_text(encoding="utf-8")
        literals = dict(re.findall(r'T\("(development\.[^"\n]+)",\s*"([^"\n]*)"', source))
        en = {k: v for k, v in read_catalog(GAME / "locale/en.json").items() if k.startswith("development.")}
        self.assertEqual(literals, en)


if __name__ == "__main__":
    unittest.main()
