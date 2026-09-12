"""Stdlib geometry + real GLB receipts tests. python -m unittest discover -s Tools/WorldDesign -p test_coastal_geometry.py"""
import unittest,json,math,struct,hashlib
from coastal_geometry import *
OUT=GAME/'assets/3d/coastal_region'

def glb(path):
    raw=path.read_bytes();n,t=struct.unpack_from('<II',raw,12)
    assert raw[:4]==b'glTF' and t==0x4e4f534a
    doc=json.loads(raw[20:20+n]);off=20+n;nb,tb=struct.unpack_from('<II',raw,off)
    return doc,raw[off+8:off+8+nb]

def positions(doc,blob,primitive):
    acc=doc['accessors'][primitive['attributes']['POSITION']];view=doc['bufferViews'][acc['bufferView']]
    offset=view.get('byteOffset',0)+acc.get('byteOffset',0);stride=view.get('byteStride',12)
    return [struct.unpack_from('<fff',blob,offset+i*stride) for i in range(acc['count'])]

class CoastalGeometryTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.data,cls.sha=load();cls.grids=json.loads((GAME/'data/world/coastal_heightfields.json').read_text())
    def test_provenance_finite_envelopes_seam(self):
        validate(self.data,self.grids)
    def test_contract_modules_receipts(self):
        receipt=json.loads((OUT/'generation_receipt.json').read_text());self.assertEqual(receipt['source_sha256'],self.sha)
        expected={f'{a}_{k}.glb' for a in ['ranch','town'] for k in ['terrain','paths','water','safety']}|{'ranch_bridge.glb','town_pier.glb'}
        self.assertEqual({r['module'] for r in receipt['modules']},expected)
        for r in receipt['modules']:
            p=OUT/r['module'];self.assertEqual(hashlib.sha256(p.read_bytes()).hexdigest(),r['sha256']);doc,_=glb(p)
            self.assertGreater(len(doc['meshes']),0);self.assertEqual(len(doc['meshes']),r['meshes'])
            if '_terrain' in p.name:self.assertTrue(all(n.get('name','').endswith('-col') for n in doc['nodes']))
            if '_safety' in p.name:self.assertTrue(all(n.get('name','').endswith('-colonly') for n in doc['nodes']))
        self.assertGreater((OUT/'coastal_region.blend').stat().st_size,100000)
    def test_exported_full_width_path_contact(self):
        for area,g in self.grids['areas'].items():
            doc,blob=glb(OUT/(area+'_paths.glb'))
            for me in doc['meshes']:
                for p in me['primitives']:
                    for x,y,z in positions(doc,blob,p):
                        self.assertAlmostEqual(y-sample(g,x,z),.018,delta=.0001,msg=(area,me['name'],x,z))
    def test_connection_width_polygon_and_grade(self):
        area='ranch';g=self.grids['areas'][area];poly=self.data['region']['areas'][area]['walkable_boundary']
        pts=[local(area,p,self.data) for p in self.data['region']['connection']['points']]
        for a,b in zip(pts,pts[1:]):
            dx,dz=b[0]-a[0],b[2]-a[2];length=math.hypot(dx,dz);previous=None
            for k in range(math.ceil(length)*2+1):
                t=k/(math.ceil(length)*2);x=a[0]+t*dx;z=a[2]+t*dz
                for side in [-1.6,0,1.6]:
                    px,pz=x-dz/length*side,z+dx/length*side
                    self.assertTrue(inside(px,pz,poly),(px,pz,'lane leaves polygon'))
                    self.assertLess(abs(sample(g,px,pz)-(a[1]+t*(b[1]-a[1]))),.12)
                y=sample(g,x,z)
                if previous:self.assertLess(abs(y-previous[2])/math.hypot(x-previous[0],z-previous[1]),.25)
                previous=(x,z,y)
    def test_beach_dry(self):
        g=self.grids['areas']['town'];x,z,w,d=self.data['region']['coast']['beach_bounds']
        for i in range(w+1):
            for j in range(d+1):self.assertGreaterEqual(sample(g,x+i,z+j)+4,.75)
    def test_bridge_gap_and_guards(self):
        doc,blob=glb(OUT/'ranch_safety.glb')
        for node in doc['nodes']:
            if '_bank_' not in node.get('name',''):continue
            for p in doc['meshes'][node['mesh']]['primitives']:
                vv=positions(doc,blob,p);center=sum(v[2] for v in vv)/len(vv)
                self.assertFalse(22.2<=center<=25.8,'Bank blocks bridge')
        for module,expected in [('ranch_bridge.glb','ranch_footbridge_deck-col'),('town_pier.glb','pier_seaward_guard-col')]:
            doc,_=glb(OUT/module);self.assertIn(expected,[n.get('name') for n in doc['nodes']])
    def test_stream_monotonic_and_bed(self):
        r=self.data['region'];points=r['stream']['points']
        for a,b in zip(points,points[1:]):
            self.assertGreater(a[1],b[1])
            for k in range(21):
                t=k/20;p=[a[j]+t*(b[j]-a[j]) for j in range(3)]
                for area,g in self.grids['areas'].items():
                    x,y,z=local(area,p,self.data);ox,oz,w,d=g['bounds']
                    if ox+1<x<ox+w-1 and oz+1<z<oz+d-1:
                        self.assertLess(sample(g,x,z),y-.2,(area,p,'bed above water'))

if __name__=='__main__':unittest.main()
