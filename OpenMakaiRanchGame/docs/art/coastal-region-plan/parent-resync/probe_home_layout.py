"""Read-only candidate probe; does not alter layout, exports or provenance."""
import sys, json, math
from pathlib import Path
ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT/'Tools/WorldDesign'))
import coastal_geometry as geometry
import coastal_plan as plan

data, _ = geometry.load()
for p in data['ranch']['plots']:
    if p['id'] == 'ranch_house': p['center'] = [-10,-11]
    if p['id'] == 'pasture': p['center'] = [-2,-34]
    if p['id'] == 'pet_care': p['center'] = [-13,-34]
for p in data['ranch']['paths']:
    if p['id'] == 'home_lane': p['points'] = [[1,9],[-2,6],[-10,6]]
    if p['id'] == 'farm_lane': p['points'] = [[0,20],[0,13],[1,9],[5,5],[5,0],[4,-5],[4,-25],[-2,-29],[-13,-29]]
    if p['id'] == 'quiet_garden': p['points'] = [[1,9],[-2,11],[-4.5,15],[-6.5,15]]
data['ranch']['approaches']['ranch_house'] = [[-10,6],[-10,4]]
data['ranch']['approaches']['pasture'] = [[4,-25],[-2,-29]]
data['ranch']['approaches']['pet_care'] = [[-2,-29],[-13,-29]]
errors = plan.validate(data)['errors']
def intersects(a,b):
    return a[0]<b[2] and b[0]<a[2] and a[1]<b[3] and b[1]<a[3]
for p in data['ranch']['plots']:
    bounds = plan.envelope(p)
    if intersects(bounds,[-1.6,7,1.6,19.4]): errors.append(p['id']+': gate')
    for q in data['ranch']['plots']:
        if p is q: continue
        c,s = math.cos(q['yaw']),math.sin(q['yaw'])
        x=q['center'][0]+s*(q['footprint'][1]/2+1.3)
        z=q['center'][1]+c*(q['footprint'][1]/2+1.3)
        w=abs(c)*2.2+abs(s)*2.6;d=abs(s)*2.2+abs(c)*2.6
        if intersects(bounds,plan.envelope(q)) or intersects(bounds,[x-w/2,z-d/2,x+w/2,z+d/2]):
            errors.append(p['id']+' vs '+q['id'])
# Synthetic candidate grid, never saved or represented as canonical export.
grids = geometry.build_grids(data,'SYNTHETIC_CANDIDATE_NOT_PROVENANCE')
worst = {}
for area in ('ranch','town'):
    for p in data[area]['plots']:
        vals=[]
        for u in [-p['footprint'][0]/2,0,p['footprint'][0]/2]:
            for v in [-p['footprint'][1]/2,0,p['footprint'][1]/2+1.5]:
                c,s=math.cos(p['yaw']),math.sin(p['yaw'])
                x=p['center'][0]+c*u+s*v;z=p['center'][1]-s*u+c*v
                vals.append(abs(geometry.sample(grids['areas'][area],x,z)))
        worst[area+'/'+p['id']]=max(vals)
        if max(vals)>=.015: errors.append(area+'/'+p['id']+': terrain')
print(json.dumps({'candidate':'home origin [-10,0,-5]; north services at Z-34','errors':errors,'maximum_envelope_errors_m':worst},indent=2))
raise SystemExit(bool(errors))
