"""Translator errors fail validation without changing game or personal profile data."""
import tempfile
import unittest
from pathlib import Path

from validate_locales import format_slots, read_catalog, validate_pair, validate_repository


class LocaleCatalogTests(unittest.TestCase):
    def test_shipped_world_catalogs(self):
        self.assertGreaterEqual(validate_repository(), 150)

    def test_reordered_placeholders_and_escaped_braces(self):
        validate_pair({"message": "{{Value}} {0} / {1:0.0}"},
                      {"message": "{1:0.0} / {{Wert}} {0}"})
        self.assertEqual(format_slots("{0,-12} {1:+0;-0;0}"), {0, 1})

    def test_dropped_or_extra_arguments_rejected(self):
        for value in ("Only {0}", "{0} {2}", "{0} {1} {3}"):
            with self.subTest(value=value), self.assertRaises(ValueError):
                validate_pair({"key": "{0} {1}"}, {"key": value})

    def test_broken_format_or_named_fields_rejected(self):
        for value in ("{", "Unescaped }", "{name}", "{0!r}", "{0:{1}}", "{}"):
            with self.subTest(value=value), self.assertRaises(ValueError):
                format_slots(value)

    def test_different_key_sets_rejected(self):
        with self.assertRaises(ValueError):
            validate_pair({"one": "One"}, {"two": "Zwei"})

    def test_blank_null_duplicate_and_non_object_catalogs_rejected(self):
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder) / "sample.json"
            for text in ('{"key":""}', '{"key":null}', '{"key":9}', '[]', '{"key":"one","key":"two"}', '{"":"text"}'):
                path.write_text(text, encoding="utf-8")
                with self.subTest(text=text), self.assertRaises(ValueError):
                    read_catalog(path)

    def test_oversized_catalog_rejected(self):
        with tempfile.TemporaryDirectory() as folder:
            path = Path(folder) / "sample.json"
            path.write_text('{"key":"' + "x" * (512 * 1024) + '"}', encoding="utf-8")
            with self.assertRaises(ValueError):
                read_catalog(path)


if __name__ == "__main__":
    unittest.main()
