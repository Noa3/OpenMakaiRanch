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
        expected={f'{a}_{k}.glb' for a in ['ranch','town'] for k in ['terrain','paths','water','safety']}|{'ranch_bridge.glb','ranch_valley_bridge.glb','town_pier.glb'}
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
                    self.assertLess(abs(walking_height(area,px,pz,self.data,g)-(a[1]+t*(b[1]-a[1]))),.12,(px,pz))
                y=walking_height(area,x,z,self.data,g)
                if previous:self.assertLess(abs(y-previous[2])/math.hypot(x-previous[0],z-previous[1]),.25)
                previous=(x,z,y)
    def test_valley_real_deck_coverage_guards_and_clearance(self):
        validate_export_routes(self.data)
        bridge,a,b,length,ux,uz=bridge_frame(self.data)
        self.assertAlmostEqual(length,12.,places=6)
        self.assertAlmostEqual(ux*41+uz*28,0.,places=6)
        doc,blob=glb(OUT/bridge['module']);nodes={n['name']:n for n in doc['nodes']}
        def verts(name):
            return [v for p in doc['meshes'][nodes[name]['mesh']]['primitives'] for v in positions(doc,blob,p)]
        deck=verts('valley_footbridge_deck-col');top=max(v[1] for v in deck)
        self.assertAlmostEqual(top,a[1]-12,delta=.00001)
        corners=[bridge_coordinates(v[0],v[2],self.data) for v in deck if abs(v[1]-top)<.00001]
        # Coverage against the actual exported deck rectangle, not the carved grid.
        for k in range(121):
            for side in [-1.6,-.8,0,.8,1.6]:
                along=k*length/120
                self.assertGreaterEqual(along,min(p[0] for p in corners)-.00002)
                self.assertLessEqual(along,max(p[0] for p in corners)+.00002)
                self.assertGreaterEqual(side,min(p[1] for p in corners))
                self.assertLessEqual(side,max(p[1] for p in corners))
        for side in [-1,1]:
            vv=verts(f'valley_rail_{side}-col');coords=[bridge_coordinates(v[0],v[2],self.data) for v in vv]
            self.assertGreaterEqual(min(abs(p[1]) for p in coords),1.7-.00002)
            self.assertLessEqual(min(p[0] for p in coords),.00002)
            self.assertGreaterEqual(max(p[0] for p in coords),length-.00002)
            self.assertGreaterEqual(max(v[1] for v in vv)-top,1.3-.00002)
        self.assertEqual(len([n for n in nodes if 'support' in n]),4)
        x,z=(a[0]+b[0])/2,(a[2]+b[2])/2
        water=nearest(x,z,self.data['region']['stream']['points'])[1]
        self.assertGreater(top+12-bridge['deck_thickness']-water,.6)
        self.assertLess(sample(self.grids['areas']['ranch'],x,z)+12,water-.2)
        safety,sblob=glb(OUT/'ranch_safety.glb')
        for n in safety['nodes']:
            if '_bank_' not in n['name']:continue
            for p in safety['meshes'][n['mesh']]['primitives']:
                vv=positions(safety,sblob,p)
                for v in vv:self.assertFalse(on_valley_bridge(v[0],v[2],self.data),n['name'])

    def test_full_lane_does_not_enter_actual_exported_rails(self):
        doc,blob=glb(OUT/self.data['region']['connection']['bridge']['module'])
        points=[bridge_coordinates(p[0],p[2],self.data) for p in self.data['region']['connection']['points']]
        for node in doc['nodes']:
            if not node.get('name','').startswith('valley_rail_'):continue
            coordinates=[]
            for primitive in doc['meshes'][node['mesh']]['primitives']:
                for vertex in positions(doc,blob,primitive):
                    p=world('ranch',vertex,self.data)
                    coordinates.append(bridge_coordinates(p[0],p[2],self.data))
            rect=(min(p[0] for p in coordinates),max(p[0] for p in coordinates),
                  min(p[1] for p in coordinates),max(p[1] for p in coordinates))
            for index,(a,b) in enumerate(zip(points,points[1:])):
                self.assertGreaterEqual(segment_rect_distance(a,b,rect)+.0001,
                    self.data['region']['connection']['width']/2,(index,node['name'],'lane crosses rail'))

    def test_reject_sideways_bridge_entry(self):
        import copy
        data=copy.deepcopy(self.data);bridge,a,b,length,ux,uz=bridge_frame(data)
        data['region']['connection']['points']=[[a[0]-uz*4,a[1],a[2]+ux*4],a,b]
        bridge['segment_index']=1
        with self.assertRaisesRegex(ValueError,'rail clearance'):validate_export_routes(data)

    def test_segment_rectangle_distance(self):
        rect=(0,12,1.7,1.9)
        self.assertAlmostEqual(segment_rect_distance((-5,0),(0,0),rect),1.7)
        self.assertEqual(segment_rect_distance((0,5),(0,0),rect),0)
        self.assertEqual(segment_rect_distance((3,1.8),(3,1.8),rect),0)
        self.assertAlmostEqual(segment_rect_distance((-3,-2),(-3,-2),rect),math.hypot(3,3.7))

    def test_all_lane_lateral_grades(self):
        g=self.grids['areas']['ranch'];pts=self.data['region']['connection']['points']
        for a,b in zip(pts,pts[1:]):
            dx,dz=b[0]-a[0],b[2]-a[2];length=math.hypot(dx,dz);n=math.ceil(length*4)
            for side in [-1.6,-.8,0,.8,1.6]:
                previous=None
                for k in range(n+1):
                    t=k/n;x=a[0]+t*dx-dz/length*side;z=a[2]+t*dz+dx/length*side
                    y=walking_height('ranch',x,z,self.data,g)
                    self.assertLess(abs(y+12-a[1]-t*(b[1]-a[1])),.12)
                    if previous is not None:self.assertLess(abs(y-previous)/(length/n),.25)
                    previous=y

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
