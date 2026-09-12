"""New service panels must have editable English/German templates and matching arguments."""
import re
import json
import unittest
from validate_locales import GAME, read_catalog, validate_pair

class GameplayPanelCatalogTests(unittest.TestCase):
    def test_complete_panel_slice(self):
        catalogs = [{k:v for k,v in read_catalog(GAME / f"locale/{lang}.json").items()
                     if k.startswith("panel.")} for lang in ("en", "de")]
        validate_pair(*catalogs)
        self.assertEqual(len(catalogs[0]), 80)

    def test_literal_fallbacks(self):
        keys = {}
        for source in (GAME / "src").rglob("*.cs"):
            if "Tests" in source.parts:
                continue
            for key, value in re.findall(r'\bT\("(panel\.[^"\n]+)",\s*"((?:[^"\\]|\\.)*)"', source.read_text(encoding="utf-8")):
                text = json.loads('"' + value + '"')
                if key in keys: self.assertEqual(keys[key], text, key)
                keys[key] = text
        english = {k:v for k,v in read_catalog(GAME / "locale/en.json").items() if k.startswith("panel.")}
        self.assertEqual(keys, english)
