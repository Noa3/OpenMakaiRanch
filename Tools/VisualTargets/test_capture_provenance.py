"""Real temporary-Git checks for capture provenance; no engine or personal files."""
import importlib.util
from pathlib import Path
import subprocess
import tempfile
import unittest

spec = importlib.util.spec_from_file_location('capture_provenance', Path(__file__).with_name('capture.py'))
capture = importlib.util.module_from_spec(spec)
spec.loader.exec_module(capture)


class SourceProvenanceTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        subprocess.run(['git', 'init', '-q', str(self.root)], check=True, capture_output=True)
        self.source = self.root / 'OpenMakaiRanchGame/src/Test.cs'
        self.source.parent.mkdir(parents=True)
        self.source.write_text('// original fixture\n', encoding='utf-8')
        subprocess.run(['git', 'add', 'OpenMakaiRanchGame/src/Test.cs'], cwd=self.root, check=True, capture_output=True)

    def snapshot(self):
        return capture.source_snapshot(self.root)

    def test_local_tracked_edit_changes_fingerprint(self):
        before = self.snapshot()
        self.source.write_text('// changed fixture\n', encoding='utf-8')
        self.assertNotEqual(before['sha256'], self.snapshot()['sha256'])

    def test_untracked_source_is_included(self):
        before = self.snapshot()
        extra = self.source.with_name('New.cs')
        extra.write_text('// untracked fixture\n', encoding='utf-8')
        after = self.snapshot()
        self.assertNotEqual(before['sha256'], after['sha256'])
        self.assertIn('OpenMakaiRanchGame/src/New.cs', [f['path'] for f in after['files']])

    def test_deleted_source_is_recorded(self):
        before = self.snapshot()
        self.source.unlink()
        after = self.snapshot()
        self.assertNotEqual(before['sha256'], after['sha256'])
        self.assertIsNone(after['files'][0]['sha256'])

    def test_ignored_cache_and_out_of_scope_notes_do_not_change_fingerprint(self):
        (self.root / '.gitignore').write_text('OpenMakaiRanchGame/src/cache/\n', encoding='utf-8')
        before = self.snapshot()
        cache = self.source.parent / 'cache/temp.bin'
        cache.parent.mkdir()
        cache.write_bytes(b'test cache')
        (self.root / 'unrelated.md').write_text('unrelated fixture', encoding='utf-8')
        self.assertEqual(before, self.snapshot())

    def test_secret_named_input_fails_before_reading(self):
        path = self.source.parent / '.env'
        path.write_text('FAKE_TEST_VALUE=not-a-secret', encoding='utf-8')
        with self.assertRaisesRegex(RuntimeError, 'Secret-like'):
            self.snapshot()


if __name__ == '__main__':
    unittest.main()
