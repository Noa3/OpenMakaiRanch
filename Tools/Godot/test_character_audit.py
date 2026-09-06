"""Read-only audit evidence regressions using synthetic, non-explicit fixtures."""
import hashlib
import json
import os
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest

import verify_character_audit as audit


class CharacterAuditTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix="omr-character-audit-")
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name) / "repo with spaces"
        self.source = self.root / "eraMakaiRanch-game-eng-translation/CSV/Chara1_test.csv"
        self.source.parent.mkdir(parents=True)
        self.source.write_text("番号,1\n呼び名,Test\nフラグ,外見年齢,13\n", encoding="utf-8-sig")
        self.code = self.root / "OpenMakaiRanchGame/src/Example.cs"
        self.code.parent.mkdir(parents=True)
        self.code.write_text("// synthetic fixture\n", encoding="utf-8")
        self.report = self.root / "audit.json"
        source_file = self.source.relative_to(self.root).as_posix()
        self.data = {
            "schema_version": 1,
            "counts": {"source_files": 1},
            "source_characters": [{
                "source_id": 1,
                "source_file": source_file,
                "source_sha256": self.digest(self.source),
                "metadata": {
                    "source_id": [{"value": 1, "source_file": source_file,
                                   "line": 1, "source_key": "番号"}],
                    "display_name": [{"value": "Test", "source_file": source_file,
                                      "line": 2, "source_key": "呼び名"}],
                    "apparent_age": [{"value": 13, "source_file": source_file,
                                      "line": 3, "source_key": "フラグ,外見年齢"}],
                },
            }],
            "input_manifest": [{"file": self.code.relative_to(self.root).as_posix(),
                                "sha256": self.digest(self.code)}],
        }
        self.save_report()

    @staticmethod
    def digest(path):
        return hashlib.sha256(path.read_bytes()).hexdigest()

    def save_report(self):
        self.report.write_text(json.dumps(self.data, ensure_ascii=False), encoding="utf-8")

    def run_verifier(self, optimized=False):
        # Override only the repository/report paths; execute the real verifier.
        script = (
            "from pathlib import Path; import verify_character_audit as audit; "
            f"audit.ROOT = Path({str(self.root)!r}); "
            f"audit.REPORT = Path({str(self.report)!r}); audit.verify()"
        )
        env = dict(os.environ, PYTHONPATH=str(Path(audit.__file__).parent), PYTHONIOENCODING="utf-8")
        return subprocess.run([sys.executable, *(["-O"] if optimized else []), "-c", script],
                              capture_output=True, text=True, encoding="utf-8", env=env, timeout=15)

    def test_optimized_python_rejects_stale_input(self):
        self.code.write_text("// changed fixture\n", encoding="utf-8")
        result = self.run_verifier(optimized=True)
        self.assertNotEqual(result.returncode, 0, result.stdout)
        self.assertNotIn("AUDIT_EVIDENCE_PASS", result.stdout)

    def test_valid_evidence_passes_without_executing_embedded_code(self):
        self.data["extraction_python"] = "raise RuntimeError('must not execute')"
        self.save_report()
        result = self.run_verifier()
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn("1 sources, 3 field citations, 1 code/data hashes", result.stdout)
        self.assertIn("does not certify", result.stdout)

    def test_citation_line_zero_cannot_alias_last_line(self):
        self.data["source_characters"][0]["metadata"]["apparent_age"][0]["line"] = 0
        self.save_report()
        result = self.run_verifier()
        self.assertNotEqual(result.returncode, 0, result.stdout)

    def test_citation_cannot_claim_a_different_file(self):
        field = self.data["source_characters"][0]["metadata"]["apparent_age"][0]
        field["source_file"] = "eraMakaiRanch-game-eng-translation/CSV/Chara99_wrong.csv"
        self.save_report()
        result = self.run_verifier()
        self.assertNotEqual(result.returncode, 0, result.stdout)

    def test_row_identity_must_match_cited_source_id(self):
        self.data["source_characters"][0]["source_id"] = 99
        self.save_report()
        result = self.run_verifier()
        self.assertNotEqual(result.returncode, 0, result.stdout)

    def test_manifest_path_cannot_leave_repository(self):
        outside = self.root.parent / "outside.cs"
        outside.write_text("// outside fixture", encoding="utf-8")
        self.data["input_manifest"] = [{"file": "../outside.cs", "sha256": self.digest(outside)}]
        self.save_report()
        result = self.run_verifier()
        self.assertNotEqual(result.returncode, 0, result.stdout)

    def test_empty_evidence_cannot_pass(self):
        self.data["source_characters"][0]["metadata"] = {}
        self.save_report()
        result = self.run_verifier()
        self.assertNotEqual(result.returncode, 0, result.stdout)

    def test_duplicate_manifest_paths_cannot_inflate_count(self):
        self.data["input_manifest"].append(dict(self.data["input_manifest"][0]))
        self.save_report()
        result = self.run_verifier()
        self.assertNotEqual(result.returncode, 0, result.stdout)

    def test_wrong_schema_is_rejected(self):
        self.data["schema_version"] = 2
        self.save_report()
        result = self.run_verifier()
        self.assertNotEqual(result.returncode, 0, result.stdout)

    def run_cli(self, *args):
        return subprocess.run([sys.executable, str(Path(audit.__file__).resolve()),
                               "--root", str(self.root), "--report", "audit.json", *args],
                              capture_output=True, text=True, encoding="utf-8",
                              env=dict(os.environ, PYTHONIOENCODING="utf-8"), timeout=15)

    def test_cli_accepts_relocated_repository(self):
        result = self.run_cli()
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn("AUDIT_EVIDENCE_PASS 1 sources", result.stdout)

    def test_source_only_explicitly_skips_stale_code(self):
        self.code.write_text("// changed fixture\n", encoding="utf-8")
        result = self.run_cli("--sources-only")
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertIn("AUDIT_SOURCE_EVIDENCE_PASS 1 sources, 3 field citations", result.stdout)
        self.assertIn("Code/data snapshot NOT CHECKED", result.stdout)
        self.assertNotIn("AUDIT_EVIDENCE_PASS", result.stdout)

    def test_source_only_still_rejects_changed_source(self):
        self.source.write_text("番号,1\n", encoding="utf-8-sig")
        result = self.run_cli("--sources-only")
        self.assertEqual(result.returncode, 1, result.stderr)
        self.assertIn("Source hash mismatch", result.stderr)

    def test_cli_malformed_report_fails_without_traceback(self):
        self.report.write_text("{ broken", encoding="utf-8")
        result = self.run_cli()
        self.assertEqual(result.returncode, 1)
        self.assertIn("AUDIT_EVIDENCE_FAIL", result.stderr)
        self.assertNotIn("Traceback", result.stderr)

    def test_cli_reports_all_stale_inputs_without_traceback(self):
        second = self.code.with_name("Second.cs")
        second.write_text("// second fixture\n", encoding="utf-8")
        self.data["input_manifest"].append({"file": second.relative_to(self.root).as_posix(),
                                            "sha256": self.digest(second)})
        self.save_report()
        self.code.write_text("// changed fixture\n", encoding="utf-8")
        second.write_text("// changed second fixture\n", encoding="utf-8")
        result = self.run_cli()
        self.assertEqual(result.returncode, 1)
        self.assertIn("Example.cs", result.stderr)
        self.assertIn("Second.cs", result.stderr)
        self.assertNotIn("Traceback", result.stderr)

    def test_invalid_citation_line_types_and_bounds(self):
        field = self.data["source_characters"][0]["metadata"]["apparent_age"][0]
        for value in (True, -1, 4, "3", None):
            with self.subTest(value=value):
                field["line"] = value
                self.save_report()
                result = self.run_cli()
                self.assertEqual(result.returncode, 1)
                self.assertIn("Invalid citation line", result.stderr)
                self.assertNotIn("Traceback", result.stderr)

    def test_new_source_missing_from_report_is_rejected(self):
        self.source.with_name("CHARA2_EXTRA.CSV").write_text("番号,2\n", encoding="utf-8-sig")
        result = self.run_cli()
        self.assertEqual(result.returncode, 1)
        self.assertIn("Source count mismatch", result.stderr)

    def test_missing_manifest_file_is_rejected(self):
        self.code.unlink()
        result = self.run_cli()
        self.assertEqual(result.returncode, 1)
        self.assertIn("Evidence file missing", result.stderr)
        self.assertNotIn("Traceback", result.stderr)

    def test_invalid_manifest_shapes_are_actionable(self):
        for value in (None, [], {}, [None]):
            with self.subTest(value=value):
                self.data["input_manifest"] = value
                self.save_report()
                result = self.run_cli()
                self.assertEqual(result.returncode, 1)
                self.assertIn("AUDIT_EVIDENCE_FAIL", result.stderr)
                self.assertNotIn("Traceback", result.stderr)

    def test_digest_tokens_are_not_silently_normalized(self):
        digest = self.digest(self.code)
        for value in (digest.upper(), digest + " ", "", None):
            with self.subTest(value=value):
                self.data["input_manifest"][0]["sha256"] = value
                self.save_report()
                result = self.run_cli()
                self.assertEqual(result.returncode, 1)
                self.assertIn("Expected SHA-256", result.stderr)

    def test_verification_preserves_all_input_bytes(self):
        before = {p: p.read_bytes() for p in (self.source, self.code, self.report)}
        result = self.run_cli()
        self.assertEqual(result.returncode, 0, result.stderr)
        self.assertEqual(before, {p: p.read_bytes() for p in before})

    def test_duplicate_json_fields_are_rejected(self):
        text = self.report.read_text(encoding="utf-8")
        self.report.write_text(text.replace('"schema_version": 1',
                                           '"schema_version": 2, "schema_version": 1'), encoding="utf-8")
        result = self.run_cli()
        self.assertEqual(result.returncode, 1, result.stdout)
        self.assertIn("Duplicate JSON field", result.stderr)

    def test_citation_must_match_metadata_field_meaning(self):
        metadata = self.data["source_characters"][0]["metadata"]
        metadata["display_name"] = [dict(metadata["source_id"][0])]
        self.save_report()
        result = self.run_cli()
        self.assertEqual(result.returncode, 1, result.stdout)
        self.assertIn("Metadata field/key mismatch", result.stderr)

    def test_cli_oversized_json_integer_is_actionable(self):
        self.report.write_text('{"value":' + '9' * 5000 + '}', encoding="utf-8")
        result = self.run_cli()
        self.assertEqual(result.returncode, 1)
        self.assertIn("AUDIT_EVIDENCE_FAIL", result.stderr)
        self.assertNotIn("Traceback", result.stderr)

    def test_cli_excessive_json_nesting_is_actionable(self):
        self.report.write_text('{"value":' + '[' * 2000 + '0' + ']' * 2000 + '}', encoding="utf-8")
        result = self.run_cli()
        self.assertEqual(result.returncode, 1)
        self.assertIn("AUDIT_EVIDENCE_FAIL", result.stderr)
        self.assertNotIn("Traceback", result.stderr)


if __name__ == "__main__":
    unittest.main()
