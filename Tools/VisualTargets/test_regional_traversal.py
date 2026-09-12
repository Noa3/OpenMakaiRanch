"""Synthetic validator tests only; never engine/traversal evidence."""
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch
from regional_traversal import verify_recording


class RegionalRecordingTests(unittest.TestCase):
    def check_recording(self, *, engine_error=False, gap=False, changed=False, claim_pass=False):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            evidence = root / 'evidence'
            evidence.mkdir()
            data = dict(passed=claim_pass, error='' if claim_pass else 'test obstruction', frames=2,
                        source_commit='synthetic', renderer='forward_plus', shots=[dict(name='synthetic')], segment='test')
            (evidence / 'regional-traversal.json').write_text(json.dumps(data))
            trace = [dict(frame=i+1, physics_frame=10+i+(1 if gap and i else 0),
                          player=[i*.1, 0, 0], escort=[i*.04, 0, 0], collisions=[], area='ranch', transitions=0)
                     for i in range(2)]
            (evidence / 'regional-trace.jsonl').write_text('\n'.join(map(json.dumps, trace)))
            (evidence / 'synthetic.png').write_bytes(b'explicitly mocked PNG dimensions, not image evidence')
            source = dict(sha256='synthetic')
            with patch('regional_traversal.png_size', return_value=(1600, 900)):
                result = verify_recording(root, 0, 'REGIONAL TRAVERSAL RECORDED\n' + ('ERROR: test\n' if engine_error else ''),
                                          {'OMR_VISUAL_SOURCE_COMMIT': 'synthetic'}, source,
                                          {} if changed else source, 'dll', 'dll')
            return result, json.loads((evidence / 'regional-verification.json').read_text())

    def test_failed_walk_retains_verified_recording_but_exits_failure(self):
        result, report = self.check_recording()
        self.assertEqual(result, 1)
        self.assertTrue(report['recording_verified'])
        self.assertFalse(report['route_passed'])
        self.assertFalse(report['accepted'])

    def test_engine_error_rejected(self):
        self.assertFalse(self.check_recording(engine_error=True)[1]['recording_verified'])

    def test_physics_gap_rejected(self):
        self.assertFalse(self.check_recording(gap=True)[1]['recording_verified'])

    def test_source_change_rejected(self):
        self.assertFalse(self.check_recording(changed=True)[1]['recording_verified'])

    def test_incomplete_pass_rejected(self):
        self.assertFalse(self.check_recording(claim_pass=True)[1]['route_passed'])


if __name__ == '__main__':
    unittest.main()
