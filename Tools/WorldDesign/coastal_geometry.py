"""Deterministic coastal blockout geometry; Godot XYZ, metres, stdlib only."""
import hashlib
import json
import math
from pathlib import Path
ROOT = Path(__file__).resolve().parents[2]
GAME = ROOT / 'OpenMakaiRanchGame'
SOURCE = GAME / 'data/world/organic_layout.json'

def load():
    raw = SOURCE.read_bytes()
    return json.loads(raw), hashlib.sha256(raw).hexdigest()

def smooth(t):
    t = max(0., min(1., t))
    return t*t*(3-2*t)

def nearest(x,z,points):
    best = (float('inf'), 0., 0, 0.)
    for i,(a,b) in enumerate(zip(points,points[1:])):
        dx,dz=b[0]-a[0],b[2]-a[2]
        t=max(0.,min(1.,((x-a[0])*dx+(z-a[2])*dz)/(dx*dx+dz*dz)))
        dist=math.hypot(x-a[0]-t*dx,z-a[2]-t*dz)
        if dist<best[0]: best=(dist,a[1]+t*(b[1]-a[1]),i,t)
    return best

def local(area,p,data):
    a=data['region']['areas'][area]; o=a['origin']; c=math.cos(a['yaw']); s=math.sin(a['yaw'])
    x,z=p[0]-o[0],p[2]-o[2]
    return [c*x-s*z,p[1]-o[1],s*x+c*z]

def world(area,p,data):
    a=data['region']['areas'][area]; o=a['origin']; c=math.cos(a['yaw']); s=math.sin(a['yaw'])
    return [o[0]+c*p[0]+s*p[2],o[1]+p[1],o[2]-s*p[0]+c*p[2]]

def inside(x,z,poly):
    hit=False
    for a,b in zip(poly,poly[1:]+poly[:1]):
        if (a[1]>z)!=(b[1]>z) and x<(b[0]-a[0])*(z-a[1])/(b[1]-a[1])+a[0]: hit=not hit
    return hit

def door(plot):
    d=plot['footprint'][1]/2+0.8
    return [plot['center'][0]+math.sin(plot['yaw'])*d,0.,plot['center'][1]+math.cos(plot['yaw'])*d]

def routes(area,data):
    result=[]
    for p in data[area]['paths']:
        result.append((p['id'],p['width'],[[x,0.,z] for x,z in p['points']]))
    for p in data[area]['plots']:
        pts=[[x,0.,z] for x,z in data[area]['approaches'][p['id']]]+[door(p)]
        result.append(('door_'+p['id'],1.8,pts))
    r=data['region']
    if area=='ranch':
        for key in ['connection','ranch_side_route']:
            result.append((key,r[key]['width'],[local(area,p,data) for p in r[key]['points']]))
    else:
        pts=[]
        for x,z in r['coast']['shore_route']:
            y=-2.6*smooth((-x-27)/23)
            pts.append([x,y,z])
        pts.append([-54,-2.6,-24])
        result.append(('shore_route',r['coast']['shore_route_width'],pts))
        result.append(('seam_lane',3.2,[[0,0,15],[0,0,35],[0,0,42]]))
    return result

def plot_distance(x,z,p):
    dx,dz=x-p['center'][0],z-p['center'][1]; c=math.cos(p['yaw']);s=math.sin(p['yaw'])
    u,v=c*dx-s*dz,s*dx+c*dz
    # Full shell, entrance and outside landing, not merely shell centre.
    return max(abs(u)-p['footprint'][0]/2-1.5,-v-p['footprint'][1]/2-1.5,v-p['footprint'][1]/2-3.)

def authored_height(area,x,z,data,cached_routes=None):
    r=data['region']; wx,_,wz=world(area,[x,0,z],data)
    if area=='ranch':
        h=12+8*smooth((-wx-27)/33)+15*smooth((-wz-27)/83)
        if z>25: h=min(h,nearest(wx,wz,r['connection']['points'])[1]+3*smooth(abs(wx-25)/55))
        h-=12
    else:
        h=-3.1*smooth((-x-29)/25)
        hc=r['coast']['headland']['center']; radius=r['coast']['headland']['radius']
        h+=(hc[1]-h)*(1-smooth(math.hypot(x-hc[0],z-hc[2])/radius))
        if x>30: h+=5*smooth((x-30)/60)
    # Broad, graded full-width paths.
    for _,width,pts in (cached_routes or routes(area,data)):
        d,y,_,_=nearest(x,z,pts)
        h=h+(y-h)*(1-smooth((d-width/2-1.5)/5))
    # Preserve complete service envelopes exactly at datum.
    for p in data[area]['plots']:
        d=plot_distance(x,z,p)
        h=h*(smooth(d/4))
    # Hydrology wins outside protected building envelopes; conflicts tested.
    stream=[local(area,p,data) for p in r['stream']['points']]
    d,y,i,t=nearest(x,z,stream); widths=r['stream']['widths']; width=widths[i]*(1-t)+widths[i+1]*t
    half=width/2
    if d<half+2.5:
        bed=y-r['stream']['bed_depth']; bank=max(h,y+r['stream']['bank_height'])
        if d<=half: h=bed
        elif d<half+1: h=bed+(bank-bed)*smooth(d-half)
        else: h=bank+(h-bank)*smooth((d-half-1)/1.5)
    return h

def validate_export_routes(data):
    """Reject uncrossed wet routes before exporting; do not hide plan conflicts."""
    r=data['region'];stream=r['stream']['points'];widths=r['stream']['widths']
    lane=r['connection'];clearance=lane['width']/2
    for a,b in zip(lane['points'],lane['points'][1:]):
        n=math.ceil(math.hypot(b[0]-a[0],b[2]-a[2])*2)
        for k in range(n+1):
            t=k/n;x=a[0]+t*(b[0]-a[0]);z=a[2]+t*(b[2]-a[2])
            d,y,i,u=nearest(x,z,stream)
            half=(widths[i]*(1-u)+widths[i+1]*u)/2
            if d<half+clearance+1:
                raise ValueError(f'VALLEY_LANE full width intersects stream/bank near regional ({x:.3f},{z:.3f}); add an explicitly planned crossing or reroute before export')

def build_grids(data,sha):
    out={'version':1,'source_sha256':sha,'areas':{}}
    for area,a in data['region']['areas'].items():
        x,z,w,d=a['terrain_bounds']; cols=w+1; rows=d+1; rr=routes(area,data)
        heights=[round(authored_height(area,x+i,z+j,data,rr),6) for j in range(rows) for i in range(cols)]
        modules=[f'res://assets/3d/coastal_region/{area}_{k}.glb' for k in ['terrain','paths','water','safety']]
        modules.append(f'res://assets/3d/coastal_region/{area}_{"bridge" if area=="ranch" else "pier"}.glb')
        out['areas'][area]={'bounds':[x,z,w,d],'step':1.0,'columns':cols,'rows':rows,'heights':heights,'modules':modules}
    return out

def sample(grid,x,z):
    ox,oz,w,d=grid['bounds']; fx=max(0,min(w-1e-9,x-ox));fz=max(0,min(d-1e-9,z-oz))
    i,j=int(fx),int(fz);u,v=fx-i,fz-j;c=grid['columns']; h=grid['heights'];a=h[j*c+i];b=h[j*c+i+1];cc=h[(j+1)*c+i];dd=h[(j+1)*c+i+1]
    return a+(b-a)*u+(dd-b)*v if u>=v else a+(dd-cc)*u+(cc-a)*v

def validate(data,grids):
    assert grids['source_sha256']==hashlib.sha256(SOURCE.read_bytes()).hexdigest()
    ys=[p[1] for p in data['region']['stream']['points']]
    assert all(a>b for a,b in zip(ys,ys[1:])), 'Uphill stream'
    seams=[]
    for area,g in grids['areas'].items():
        assert len(g['heights'])==g['columns']*g['rows'] and all(math.isfinite(h) for h in g['heights'])
        a=data['region']['areas'][area];seams.append(world(area,a['seam'],data))
        assert inside(a['seam'][0],a['seam'][2],a['walkable_boundary'])
        assert abs(sample(g,a['seam'][0],a['seam'][2])-(a['seam'][1]-.8))<.015, 'Seam ground mismatch'
        for p in data[area]['plots']:
            for u in [-p['footprint'][0]/2,0,p['footprint'][0]/2]:
                for v in [-p['footprint'][1]/2,0,p['footprint'][1]/2+1.5]:
                    c,s=math.cos(p['yaw']),math.sin(p['yaw']);x=p['center'][0]+c*u+s*v;z=p['center'][1]-s*u+c*v
                    assert abs(sample(g,x,z))<.015,(area,p['id'],'nonflat envelope',sample(g,x,z))
    assert math.dist(*seams)<1e-6,'Noncoincident seam'
    return {'finite_grids':True,'plot_envelopes':True,'downhill_water':True,'seam_world':seams[0]}
