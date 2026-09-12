"""Regression checks for the opt-in capture wrapper; no Godot process is started."""
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest
from unittest import mock

spec = importlib.util.spec_from_file_location('visual_capture', Path(__file__).with_name('capture.py'))
capture = importlib.util.module_from_spec(spec)
spec.loader.exec_module(capture)


class CaptureWrapperTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.run = self.root / '.dream-loop' / 'capture-test'
        self.run.mkdir(parents=True)
        self.commit = 'a' * 40
        patches = [
            mock.patch.object(capture, 'ROOT', self.root),
            mock.patch.object(capture.sys, 'platform', 'win32'),
            mock.patch.object(capture.launch, 'resolve_godot', return_value=(Path('test-engine.exe'), '4.7.2')),
            mock.patch.object(capture.launch, 'assert_port_available'),
            mock.patch.object(capture.tempfile, 'mkdtemp', return_value=str(self.run)),
            mock.patch.object(capture.subprocess, 'check_output', return_value=self.commit+'\n'),
            mock.patch.object(capture.launch, 'invoke'),
            mock.patch.object(capture, 'source_snapshot', return_value={'sha256': 's'*64, 'files': []}, create=True),
            mock.patch.object(capture, 'file_sha', return_value='d'*64, create=True),
            mock.patch('atlas.png_size', return_value=(1600, 900)),
        ]
        # capture imports atlas lazily; make the test directory importable independently of cwd.
        import sys
        with mock.patch.object(sys, 'path', [str(Path(__file__).parent), *sys.path]):
            import atlas
        self.invoke = None
        for patcher in patches:
            result = patcher.start()
            self.addCleanup(patcher.stop)
            if patcher.attribute == 'invoke':
                self.invoke = result
            if patcher.attribute == 'png_size':
                self.png_size = result
            if patcher.attribute == 'source_snapshot':
                self.snapshot = result
            if patcher.attribute == 'file_sha':
                self.file_sha = result
        self.context = {'source_commit': self.commit, 'renderer': 'forward_plus'}
        evidence = self.run / 'evidence'
        evidence.mkdir()
        self.context_file = evidence / 'W01-context.json'
        self.context_file.write_text(json.dumps(self.context), encoding='utf-8')

    def succeeds(self):
        self.invoke.side_effect = [(0, 'USER_DATA_ISOLATION_PASS\n'), (0, 'build ok\n'), (0, 'VISUAL CAPTURE PASS\n')]

    def test_success_stays_inside_disposable_profile(self):
        self.succeeds()
        self.assertEqual(capture.main(), 0)
        self.assertEqual(self.invoke.call_count, 3)
        self.assertEqual(self.invoke.call_args_list[1].args[0][:2], ['dotnet', 'build'])
        command, env, timeout, log = self.invoke.call_args.args
        self.assertIn('--visual-target-capture', command)
        self.assertNotIn('--headless', command)
        self.assertEqual(env['OMR_EXPECTED_USER_ROOT'], str(self.run.resolve()))
        self.assertEqual(env['OMR_VISUAL_TARGET_OUTPUT'], str(self.run / 'evidence'))
        self.assertTrue((self.run / 'visual-target.marker').is_file())
        self.assertLessEqual(timeout, 120)
        context = json.loads(self.context_file.read_text(encoding='utf-8'))
        self.assertEqual(context['source_state']['sha256'], 's'*64)
        self.assertEqual(context['build']['assembly_sha256'], 'd'*64)

    def test_failed_isolation_never_starts_render_or_writes_marker(self):
        self.invoke.return_value = (1, 'USER_DATA_ISOLATION_FAIL\n')
        with self.assertRaisesRegex(RuntimeError, 'isolation'):
            capture.main()
        self.assertEqual(self.invoke.call_count, 1)
        self.assertFalse((self.run / 'visual-target.marker').exists())

    def test_missing_isolation_receipt_is_rejected_even_with_zero_exit(self):
        self.invoke.return_value = (0, '')
        with self.assertRaisesRegex(RuntimeError, 'isolation'):
            capture.main()
        self.assertEqual(self.invoke.call_count, 1)

    def test_render_errors_cannot_be_hidden_by_success_line(self):
        for text in ('VISUAL CAPTURE PASS\nERROR: failure\n', 'VISUAL CAPTURE PASS\nSCRIPT ERROR: failure\n', 'VISUAL CAPTURE PASS\nVISUAL CAPTURE FAIL: failure\n'):
            with self.subTest(text=text):
                self.invoke.side_effect = [(0, 'USER_DATA_ISOLATION_PASS\n'), (0, 'build ok\n'), (0, text)]
                with self.assertRaisesRegex(RuntimeError, 'Render failed'):
                    capture.main()

    def test_missing_duplicate_or_nonzero_completion_is_rejected(self):
        for result in ((0, ''), (0, 'VISUAL CAPTURE PASS\nVISUAL CAPTURE PASS\n'), (1, 'VISUAL CAPTURE PASS\n')):
            with self.subTest(result=result):
                self.invoke.side_effect = [(0, 'USER_DATA_ISOLATION_PASS\n'), (0, 'build ok\n'), result]
                with self.assertRaisesRegex(RuntimeError, 'Render failed'):
                    capture.main()

    def test_wrong_dimensions_are_rejected(self):
        self.succeeds()
        self.png_size.return_value = (800, 450)
        with self.assertRaisesRegex(RuntimeError, 'dimensions'):
            capture.main()

    def test_stale_commit_or_wrong_renderer_is_rejected(self):
        for update in ({'source_commit': 'b'*40}, {'renderer': 'gl_compatibility'}):
            with self.subTest(update=update):
                self.succeeds()
                self.context_file.write_text(json.dumps(self.context | update), encoding='utf-8')
                with self.assertRaisesRegex(RuntimeError, 'mismatch'):
                    capture.main()

    def test_failed_build_never_starts_capture(self):
        self.invoke.side_effect = [(0, 'USER_DATA_ISOLATION_PASS\n'), (1, 'build failed\n')]
        with self.assertRaisesRegex(RuntimeError, 'Build failed'):
            capture.main()
        self.assertEqual(self.invoke.call_count, 2)
        self.assertFalse((self.run / 'visual-target.marker').exists())

    def test_source_change_during_capture_is_rejected(self):
        self.succeeds()
        self.snapshot.side_effect = [{'sha256': 'a'*64}, {'sha256': 'b'*64}]
        with self.assertRaisesRegex(RuntimeError, 'Source changed'):
            capture.main()

    def test_assembly_replacement_during_capture_is_rejected(self):
        self.succeeds()
        self.file_sha.side_effect = ['a'*64, 'b'*64]
        with self.assertRaisesRegex(RuntimeError, 'Assembly changed'):
            capture.main()


    def okachi_fixture(self):
        # Synthetic wrapper data only; real engine evidence is produced by capture.py.
        review = {'passed': True, 'renderer': 'forward_plus', 'unique_architecture': ['base', 'annex', 'cap'],
                  'variants': [{'state': state, 'front_clear_ray': True, 'four_room_clear_rays': True,
                                'rear_blocked_ray': state == 'base'} for state in ('base', 'expanded')],
                  'shots': [{'name': f'okachi-{state}-{view}'} for state in ('base', 'expanded')
                            for view in ('exterior', 'cutaway', 'reception')]}
        return review, self.run / 'evidence/okachi-assets.json'

    @mock.patch.dict(capture.os.environ, {'OMR_OKACHI_ASSET_REVIEW': '1'})
    def test_okachi_complete_review_records_image_provenance(self):
        review, path = self.okachi_fixture()
        path.write_text(json.dumps(review), encoding='utf-8')
        self.succeeds()
        self.assertEqual(capture.main(), 0)
        saved = json.loads(self.context_file.read_text(encoding='utf-8'))
        self.assertEqual(len(saved['okachi_asset_review']['images']), 6)

    @mock.patch.dict(capture.os.environ, {'OMR_OKACHI_ASSET_REVIEW': '1'})
    def test_okachi_retained_invisible_cap_is_rejected(self):
        review, path = self.okachi_fixture()
        review['variants'][1]['rear_blocked_ray'] = True
        path.write_text(json.dumps(review), encoding='utf-8')
        self.succeeds()
        with self.assertRaisesRegex(RuntimeError, 'Okachi asset review'):
            capture.main()

    @mock.patch.dict(capture.os.environ, {'OMR_OKACHI_ASSET_REVIEW': '1'})
    def test_okachi_missing_variant_image_is_rejected(self):
        review, path = self.okachi_fixture()
        review['shots'].pop()
        path.write_text(json.dumps(review), encoding='utf-8')
        self.succeeds()
        with self.assertRaisesRegex(RuntimeError, 'Okachi asset review'):
            capture.main()


    def market_fixture(self):
        # Synthetic wrapper contract only; no imported geometry or real screenshots.
        return {'passed': True, 'renderer': 'forward_plus',
                'unique_assets': ['canopy.glb', 'counter.glb', 'scaffold_bay.glb', 'material_stack.glb', 'barrier.glb'],
                'variants': [{'state': s, 'bypass_clear_volume': True, 'central_clear_volume': True,
                              'front_work_barrier_ray': s == 'work', 'visible_roof_meshes': 17 if s == 'finished' else 0}
                             for s in ('base', 'work', 'finished')],
                'shots': [{'name': f'market-{s}-{v}'} for s in ('base', 'work', 'finished') for v in ('overview', 'eye')]}

    @mock.patch.dict(capture.os.environ, {'OMR_MARKET_ASSET_REVIEW': '1'})
    def test_market_complete_evidence(self):
        (self.run / 'evidence/market-assets.json').write_text(json.dumps(self.market_fixture()))
        self.succeeds()
        self.assertEqual(capture.main(), 0)
        saved = json.loads(self.context_file.read_text())
        self.assertEqual(len(saved['market_asset_review']['images']), 6)

    @mock.patch.dict(capture.os.environ, {'OMR_MARKET_ASSET_REVIEW': '1'})
    def test_market_blocked_bypass_fails(self):
        payload = self.market_fixture()
        payload['variants'][1]['bypass_clear_volume'] = False
        (self.run / 'evidence/market-assets.json').write_text(json.dumps(payload))
        self.succeeds()
        with self.assertRaisesRegex(RuntimeError, 'collision/roof-state'):
            capture.main()

    @mock.patch.dict(capture.os.environ, {'OMR_MARKET_ASSET_REVIEW': '1'})
    def test_market_premature_roof_fails(self):
        payload = self.market_fixture()
        payload['variants'][1]['visible_roof_meshes'] = 17
        (self.run / 'evidence/market-assets.json').write_text(json.dumps(payload))
        self.succeeds()
        with self.assertRaisesRegex(RuntimeError, 'collision/roof-state'):
            capture.main()


    def town_walk_fixture(self):
        # Synthetic validator input only; actual movement is verified in Godot.
        return {'passed': True, 'error': None,
                'checks': [{'ok': True, 'label': k} for k in capture.TOWN_WALK_REQUIRED],
                'trace': [{'frame': i+1, 'player': [i*.1, 0, 0], 'escort': [i*.1, 0, 1]} for i in range(2)],
                'shots': [{'name': f'town-core-{s}'} for s in ('overview', 'reception', 'market', 'milestones', 'shop')]}

    @mock.patch.dict(capture.os.environ, {'OMR_TOWN_CORE_REVIEW': '1'})
    def test_town_walk_complete_evidence(self):
        (self.run/'evidence/town-core-walk.json').write_text(json.dumps(self.town_walk_fixture()))
        self.succeeds()
        self.assertEqual(capture.main(), 0)

    @mock.patch.dict(capture.os.environ, {'OMR_TOWN_CORE_REVIEW': '1'})
    def test_town_walk_missing_escort_arrival_is_rejected(self):
        data = self.town_walk_fixture()
        data['checks'] = [c for c in data['checks'] if c['label'] != 'escort reached market bypass exit on foot']
        (self.run/'evidence/town-core-walk.json').write_text(json.dumps(data))
        self.succeeds()
        with self.assertRaisesRegex(RuntimeError, 'Town walk checks'):
            capture.main()

    @mock.patch.dict(capture.os.environ, {'OMR_TOWN_CORE_REVIEW': '1'})
    def test_town_walk_teleport_is_rejected(self):
        data = self.town_walk_fixture()
        data['trace'][1]['escort'] = [20, 0, 1]
        (self.run/'evidence/town-core-walk.json').write_text(json.dumps(data))
        self.succeeds()
        with self.assertRaisesRegex(RuntimeError, 'trajectory contains a jump'):
            capture.main()


if __name__ == '__main__':
    unittest.main()
