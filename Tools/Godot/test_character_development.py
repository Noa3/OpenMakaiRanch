"""Localization and pinned original-source receipts; mechanics execute in Godot smoke."""
import hashlib
import re
import subprocess
import unittest
from pathlib import Path
from validate_locales import GAME, read_catalog, validate_pair

ORIGINAL = GAME.parent / "eraMakaiRanch-game-eng-translation"


class CharacterDevelopmentCatalogTests(unittest.TestCase):
    def test_keys_and_slots(self):
        en = {k: v for k, v in read_catalog(GAME / "locale/en.json").items() if k.startswith("character.")}
        de = {k: v for k, v in read_catalog(GAME / "locale/de.json").items() if k.startswith("character.")}
        validate_pair(en, de)
        self.assertEqual(len(en), 18)

    def test_literal_keys(self):
        catalog = read_catalog(GAME / "locale/en.json")
        keys = set()
        for name in ("CharacterProtectionService.cs", "CharacterDevelopmentService.cs"):
            source = (GAME / "src/Gameplay" / name).read_text(encoding="utf-8")
            keys.update(re.findall(r'T\("(character\.[^"\n]+)"\s*,', source))
        self.assertEqual(keys, {k for k in catalog if k.startswith("character.")})


@unittest.skipUnless(ORIGINAL.is_dir(), "Review-source artifact does not include the read-only original")
class OriginalCharacterSourceReceipts(unittest.TestCase):
    def check_blob(self, path, expected):
        # Git object bytes, not a Windows CRLF-converted checkout. Never alter the source.
        process = subprocess.run(
            ["git", "-C", str(GAME.parent), "show", "HEAD:eraMakaiRanch-game-eng-translation/" + path],
            stdout=subprocess.PIPE, stderr=subprocess.PIPE, check=True, timeout=20,
        )
        raw = process.stdout
        self.assertEqual((ORIGINAL / path).read_text(encoding="utf-8-sig"),
                         raw.decode("utf-8-sig").replace("\r\n", "\n"))
        digest = hashlib.sha1(b"blob " + str(len(raw)).encode("ascii") + b"\0" + raw).hexdigest()
        self.assertEqual(digest, expected, "Original reference changed: review the audit, do not rewrite the source")
        return raw.decode("utf-8-sig")

    def test_ward_uses_spirit_not_mana(self):
        text = self.check_blob("ERB/○TRAIN/処女結界保護フラグ.ERB", "9c7197fd10f8abd4c26bc4aa5e3241942d4ed34d")
        self.assertIn("BASE:霊力 <= MAXBASE:霊力 /2", text)
        self.assertNotIn("BASE:魔力", text)

    def test_battle_arithmetic_source(self):
        text = self.check_blob("ERB/戦闘/あなた戦闘_キャラ選択.ERB", "725c1ebe5665ffb23a3383679d33e549d3d29e7d")
        self.assertIn("BASE:霊力 * 10 <= BASE:魔力 && BASE:レベル < 30", text)
        self.assertIn("BASE:戦闘力 = BASE:戦闘力 * BP_CALC / 100", text)
        self.assertIn("BASE:戦闘力 = BASE:戦闘力 * BASE:戦闘適性 / 100", text)
