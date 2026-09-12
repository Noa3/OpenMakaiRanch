"""Production-town static contracts. Not Godot loading, navigation or walked-route evidence."""
import hashlib
import json
import math
from pathlib import Path
import re
import struct
import unittest

import coastal_geometry as geometry
import coastal_plan as plan

ROOT = Path(__file__).resolve().parents[2]
GAME = ROOT / 'OpenMakaiRanchGame'
DATA = json.loads((GAME / 'data/world/organic_layout.json').read_text())
TOWN = DATA['town']
SCENE = (GAME / 'scenes/dev/TownGreybox.tscn').read_text()
CIVIC = (GAME / 'scenes/world/OkachiCivicHouse.tscn').read_text()


def overlap(a, b):
    return a[0] < b[2] - 1e-6 and a[2] > b[0] + 1e-6 and a[1] < b[3] - 1e-6 and a[3] > b[1] + 1e-6


def entrance_bounds(p):
    c, s = abs(math.cos(p['yaw'])), abs(math.sin(p['yaw']))
    x, z = p['center']; distance = p['footprint'][1]/2 + 1.3
    x += math.sin(p['yaw'])*distance; z += math.cos(p['yaw'])*distance
    ex, ez = (c*2.2+s*2.6)/2, (s*2.2+c*2.6)/2
    return [x-ex, z-ez, x+ex, z+ez]


def node_position(source, name):
    block = re.search(r'\[node name="'+re.escape(name)+r'"[^\n]*\]\n(.*?)(?=\n\[|\Z)', source, re.S).group(1)
    return tuple(map(float, re.search(r'position = Vector3\(([^)]+)\)', block).group(1).split(',')))


def collision_boxes(path):
    raw = path.read_bytes()
    doc = json.loads(raw[20:20+struct.unpack_from('<I', raw, 12)[0]])
    boxes = []
    for node in doc['nodes']:
        if not node.get('name', '').endswith('-col'): continue
        if any(k in node for k in ('children','rotation','matrix','scale')):
            raise AssertionError('Collision transform needs explicit decoding: '+node['name'])
        t = node.get('translation', [0,0,0])
        for primitive in doc['meshes'][node['mesh']]['primitives']:
            a = doc['accessors'][primitive['attributes']['POSITION']]
            boxes.append((node['name'], [a['min'][i]+t[i] for i in range(3)], [a['max'][i]+t[i] for i in range(3)]))
    return boxes


class TownCoreIntegration(unittest.TestCase):
    def test_strict_full_width_plan(self):
        self.assertEqual([], plan.validate(DATA)['errors'])

    def test_plot_and_entrance_envelopes(self):
        for a in TOWN['plots']:
            for b in TOWN['plots']:
                if a is b: continue
                self.assertFalse(overlap(plan.envelope(a), plan.envelope(b)), (a['id'],b['id']))
                self.assertFalse(overlap(plan.envelope(a), entrance_bounds(b)), (a['id'],b['id'],'entrance'))

    def test_full_parcels_dry_and_flat(self):
        rr = geometry.routes('town', DATA)
        parcels = [plan.envelope(p) for p in TOWN['plots']] + [[-16,2,-2,14],[20,0,30,10]]
        for x0,z0,x1,z1 in parcels:
            for i in range(math.ceil((x1-x0)*2)+1):
                for j in range(math.ceil((z1-z0)*2)+1):
                    x,z=min(x1,x0+i*.5),min(z1,z0+j*.5)
                    self.assertAlmostEqual(0, geometry.authored_height('town',x,z,DATA,rr), delta=.015, msg=str((x,z)))

    def test_court_yard_and_gate_clearance(self):
        court, yard = [-16,2,-2,14], [20,0,30,10]
        for p in TOWN['plots']:
            if p['id'] != 'general_store': self.assertFalse(overlap(court,plan.envelope(p)),p['id'])
            self.assertFalse(overlap(yard,plan.envelope(p)),p['id'])
            self.assertFalse(overlap([-1.6,7,1.6,19.4],plan.envelope(p)),p['id'])
        # Two real BASE counter footprints, not the walkable paving, against protected main circulation.
        for x in [-11,-7]: self.assertFalse(overlap([x-.9,7.6,x+.9,8.4],[-2.5,2,2.5,34]))
        self.assertEqual((0,.7,12),node_position(SCENE,'TravelToRanch'))
        self.assertEqual([90,65],TOWN['half_extents'])
        self.assertEqual([-90,-65,180,130],DATA['region']['areas']['town']['core_bounds'])

    def test_approaches_do_not_cross_other_buildings(self):
        for p in TOWN['plots']:
            points=TOWN['approaches'][p['id']]+[[geometry.door(p)[0],geometry.door(p)[2]]]
            for other in TOWN['plots']:
                if p is other: continue
                self.assertFalse(any(plan.intersects_plot(q,other) for q in plan.ribbon(points,1.8)),(p['id'],other['id']))

    def test_permanent_bypass_connects_at_both_ends(self):
        bypass=next(p for p in TOWN['paths'] if p['id']=='market_bypass')
        self.assertEqual(1.8,bypass['width'])
        self.assertTrue(all(p[0]==-14 for p in bypass['points']))
        self.assertEqual([16,0],[bypass['points'][0][1],bypass['points'][-1][1]])
        for end in [bypass['points'][0],bypass['points'][-1]]:
            self.assertTrue(any(end in p['points'] for p in TOWN['paths'] if p is not bypass))
        # +/- .9m ribbon fits the authored 2m strip; both counters remain east of it.
        self.assertGreater(-11-.9,-13)

    def test_actual_scene_base_only_and_markers(self):
        self.assertIn('res://scenes/world/OkachiCivicHouse.tscn',SCENE)
        self.assertIn('res://scenes/world/OkachiMarketBase.tscn',SCENE)
        self.assertNotRegex(SCENE,r'OkachiMarket(?:Work|Finished)|OkachiSupplyHouseExpanded')
        groups={g['id']:g for g in DATA['development']['town_groups']}
        self.assertEqual(tuple(groups['ADMINISTRATION_SUPPLY']['main_origin']),node_position(SCENE,'CivicHouse'))
        market=node_position(SCENE,'Market')
        self.assertEqual(tuple(groups['MARKET_COURT']['center']),(market[0],market[2]))
        for name,marker in [('TownHall','ReceptionApproach'),('PlanningBoard','PlanningApproach')]:
            pos=node_position(CIVIC,marker); origin=node_position(SCENE,'CivicHouse')
            self.assertEqual(tuple(round(a+b,4) for a,b in zip(pos,origin)),node_position(SCENE,name))
        self.assertEqual((-11,.7,9),tuple(a+b for a,b in zip(market,node_position(SCENE,'ShopApproach'))))
        self.assertEqual(2,SCENE.count('size = Vector3(180, 1, 130)'))

    def test_service_identity_and_existing_lock(self):
        pairs=dict(re.findall(r'ServiceId = "([^"]+)"\nScreenId = "([^"]+)"',SCENE))
        self.assertEqual({'general_store':'shop','adventure_guild':'adventure','research_office':'research','tavern':'roster','bathhouse':'bond','town_hall':'milestones','planning_board':'town'},pairs)
        self.assertEqual(1,SCENE.count('RequiredFacilityId = "workshop"'))
        self.assertEqual(set(pairs)-{'planning_board'},{p['id'] for p in TOWN['plots']})

    def test_civic_authored_collision_clearance_samples(self):
        boxes=collision_boxes(GAME/'assets/3d/okachi_supply_house/base_shell.glb')+collision_boxes(GAME/'assets/3d/okachi_supply_house/rear_connection_cap.glb')
        self.assertEqual(64,len(boxes))
        sizes={name:tuple(map(float,s.split(','))) for name,s in re.findall(r'\[sub_resource type="BoxShape3D" id="([^"]+)"\]\nsize = Vector3\(([^)]+)\)',CIVIC)}
        bodies=re.findall(r'\[node name="([^"]+)" type="StaticBody3D" parent="FurnitureCollision"\]\nposition = Vector3\(([^)]+)\).*?shape = SubResource\("([^"]+)"\)',CIVIC,re.S)
        self.assertEqual(9,len(bodies))
        for name,p,size in bodies:
            center=tuple(map(float,p.split(','))); half=[v/2 for v in sizes[size]]
            boxes.append((name,[a-b for a,b in zip(center,half)],[a+b for a,b in zip(center,half)]))
        # Conservative upright 0.64m-wide / 1.6m-tall swept boxes. No navigation/physics or movement claim.
        paths=[[(0,6),(0,2.2),(-2,2.2),(-2,3),(-3.25,3)],[(0,6),(0,-2.7),(2,-2.7),(2,-2.3),(3.35,-2.3)]]
        for path in paths:
            for a,b in zip(path,path[1:]):
                count=math.ceil(math.dist(a,b)*10)
                for k in range(count+1):
                    x,z=[u+(v-u)*k/count for u,v in zip(a,b)]
                    for name,lo,hi in boxes:
                        if hi[1] <= .02 or lo[1]>=1.6:continue
                        self.assertFalse(overlap([x-.32,z-.32,x+.32,z+.32],[lo[0],lo[2],hi[0],hi[2]]),(name,x,z))

    def test_no_generated_civic_or_automatic_entry(self):
        adapter=(GAME/'src/World/OkachiCivicHouse.cs').read_text()
        controller=(GAME/'src/World/TownWorldController.cs').read_text()
        self.assertIn('public override void Build() { }',adapter)
        self.assertNotRegex(adapter,r'Instantiate|new Node|TryDeliver|Spend\(|StateChanged|ShowScreen')
        self.assertIn('mesh.Visible = !IsCutaway',adapter)
        self.assertIn('WorldShelterVolume.cs',CIVIC)
        self.assertIn('CanApproachService(service.ServiceId, _player.GlobalPosition)',controller)
        self.assertIn('!InputGate.WorldInputEnabled',controller)
        self.assertIn('GetTree().Paused',controller)
        self.assertIn('UpdateNearbyTargets();',controller[controller.index('public bool TryInteract()'):])
        self.assertIn('ServiceScreenRequested?.Invoke(_nearbyService.ScreenId)',controller)
        # The known planning route remains an explicit blocker, not a silently opened remote hub.
        context=(GAME/'src/Ui/UiShellController.ServiceContext.cs').read_text()
        known=context.split('internal static bool IsKnownService',1)[1].split(';',1)[0]
        self.assertNotIn('"town"',known)


if __name__ == '__main__': unittest.main()
