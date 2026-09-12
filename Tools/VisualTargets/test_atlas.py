import copy
import importlib.util
import json
from pathlib import Path
import struct
import tempfile
import unittest
import zlib
import zipfile
import hashlib

spec=importlib.util.spec_from_file_location('atlas',Path(__file__).with_name('atlas.py'))
a=importlib.util.module_from_spec(spec);spec.loader.exec_module(a)

def png(path,w=320,h=240):
    def chunk(k,v):return struct.pack('>I',len(v))+k+v+struct.pack('>I',zlib.crc32(k+v)&0xffffffff)
    raw=b'\x89PNG\r\n\x1a\n'+chunk(b'IHDR',struct.pack('>IIBBBBB',w,h,8,2,0,0,0))
    raw+=chunk(b'IDAT',zlib.compress((b'\0'+b'\x60\x80\xa0'*w)*h))+chunk(b'IEND',b'')
    path.write_bytes(raw);return path

class RegistryTests(unittest.TestCase):
    def setUp(self):
        self.temp=tempfile.TemporaryDirectory();self.addCleanup(self.temp.cleanup);self.p=Path(self.temp.name)
        self.m={'schema':1,'base_commit':'a'*40,'legacy_routes':['title'],'shots':[{'id':'U01','category':'ui','title':'Title','status':'awaiting_generation','target':None,'approval':None,'brief':'A UI','camera_contract':'camera','state_contract':'fixture','routes':['title'],'asset_ids':[],'acceptance':['readable'],'baseline_key':'base'}]}
        (self.p/'PROMPT_TEMPLATE.md').write_text('Shared image prompt');self.save()
    def save(self): (self.p/'manifest.json').write_text(json.dumps(self.m))
    def import_one(self):a.import_target(self.p,'U01',png(self.p/'input.png'),'test-provider','test-model','test-only fixture, not generated art')
    def test_pending_manifest_is_valid(self):self.assertEqual(a.validate(self.p),[])
    def test_route_loss_is_reported(self):self.m['legacy_routes'].append('room_assign');self.save();self.assertIn('room_assign',' '.join(a.validate(self.p)))
    def test_duplicate_id_is_reported(self):self.m['shots']*=2;self.save();self.assertTrue(a.validate(self.p))
    def test_duplicate_route_is_reported(self):s=copy.deepcopy(self.m['shots'][0]);s['id']='U02';self.m['shots'].append(s);self.save();self.assertIn('mapped twice',' '.join(a.validate(self.p)))
    def test_path_traversal_rejected(self):
        for p in ['../escape','/etc/passwd','a/../../b','a\\b']:
            with self.assertRaises(ValueError):a.within(self.p,p)
    def test_symlink_rejected(self):
        png(self.p/'input.png');(self.p/'linked.png').symlink_to(self.p/'input.png')
        with self.assertRaises(ValueError):a.within(self.p,'linked.png')
    def test_png_header_and_checksum(self):self.assertEqual(a.png_size(png(self.p/'i.png')),(320,240))
    def test_png_corruption_rejected(self):
        p=png(self.p/'i.png');b=bytearray(p.read_bytes());b[50]^=1;p.write_bytes(b)
        with self.assertRaises(ValueError):a.png_size(p)
    def test_renamed_text_rejected(self):
        p=self.p/'i.png';p.write_text('no image')
        with self.assertRaises(ValueError):a.png_size(p)
    def test_truncated_png_rejected(self):
        p=png(self.p/'i.png');p.write_bytes(p.read_bytes()[:-8])
        with self.assertRaises(ValueError):a.png_size(p)
    def test_small_image_rejected(self):
        with self.assertRaises(ValueError):a.png_size(png(self.p/'i.png',8,8))
    def test_missing_image_cannot_be_approved(self):
        with self.assertRaises(ValueError):a.approve(self.p,'U01','reviewer','decision')
    def test_import_is_candidate_only(self):
        self.import_one();s=a.read_json(self.p/'manifest.json')['shots'][0];self.assertEqual(s['status'],'candidate');self.assertIsNone(s['approval']);self.assertEqual(a.validate(self.p),[])
    def test_import_requires_provenance(self):
        with self.assertRaises(ValueError):a.import_target(self.p,'U01',png(self.p/'i.png'),'','','')
    def test_approval_requires_actual_reviewer_record(self):
        self.import_one()
        with self.assertRaises(ValueError):a.approve(self.p,'U01','','')
    def test_approved_targets_are_immutable(self):
        self.import_one();a.approve(self.p,'U01','Named reviewer','explicit user decision');self.assertEqual(a.validate(self.p),[])
        with self.assertRaises(ValueError):self.import_one()
    def test_changed_target_invalidates_approval(self):
        self.import_one();a.approve(self.p,'U01','Named reviewer','explicit user decision');m=a.read_json(self.p/'manifest.json');p=self.p/m['shots'][0]['target']['path'];png(p,640,480);self.assertIn('changed',' '.join(a.validate(self.p)))
    def test_unknown_shot_rejected(self):
        with self.assertRaises(ValueError):a.prompt(self.p,'W99')
    def test_complete_context_can_differ_in_commit_only(self):
        x={k:'same' for k in a.CONTEXT_KEYS};x['source_commit']='a'*40;y=dict(x,source_commit='b'*40);a.compare_context(x,y)
    def test_camera_drift_rejected(self):
        x={k:'same' for k in a.CONTEXT_KEYS};x['source_commit']='a'*40
        with self.assertRaises(ValueError):a.compare_context(x,dict(x,camera='changed'))
    def test_missing_context_rejected(self):
        with self.assertRaises(ValueError):a.compare_context({}, {})
    def test_source_commit_required(self):
        x={k:'same' for k in a.CONTEXT_KEYS}
        with self.assertRaises(ValueError):a.compare_context(x,x)
    def test_real_atlas_has_complete_route_coverage(self):self.assertEqual(a.validate(),[])
    def baseline_archive(self):
        image=png(self.p/'historical.png'); archive=self.p/'evidence.zip'
        with zipfile.ZipFile(archive,'w') as z:z.write(image,'run/capture.png')
        (self.p/'baselines').mkdir()
        records=[{'archive_sha256':a.sha(archive),'member':'run/capture.png','file':'baselines/old.png','sha256':a.sha(image)}]
        (self.p/'baselines/receipts.json').write_text(json.dumps(records))
        return archive
    def test_pinned_baseline_import_is_idempotent(self):
        z=self.baseline_archive();self.assertEqual(a.import_baselines(self.p,z),1);self.assertEqual(a.import_baselines(self.p,z),1)
    def test_unrecognized_baseline_archive_is_refused(self):
        z=self.baseline_archive();z.write_bytes(z.read_bytes()+b'extra')
        with self.assertRaises(ValueError):a.import_baselines(self.p,z)
    def test_changed_baseline_never_overwritten(self):
        z=self.baseline_archive();a.import_baselines(self.p,z);(self.p/'baselines/old.png').write_bytes(b'changed')
        with self.assertRaises(ValueError):a.import_baselines(self.p,z)
        self.assertEqual((self.p/'baselines/old.png').read_bytes(),b'changed')
    def test_baseline_path_cannot_escape_pack(self):
        z=self.baseline_archive();p=self.p/'baselines/receipts.json';r=a.read_json(p);r[0]['file']='../escape.png';p.write_text(json.dumps(r))
        with self.assertRaises(ValueError):a.import_baselines(self.p,z)
    def test_registry_counts_cover_every_shot(self):
        m=a.read_json(a.PACK/'manifest.json');self.assertEqual(sum(sum(s['status']==v for s in m['shots']) for v in a.STATES),len(m['shots']))

if __name__=='__main__':unittest.main()
