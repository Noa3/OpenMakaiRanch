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

def bridge_frame(data):
    bridge=data['region']['connection']['bridge'];a=bridge['start'];b=bridge['end']
    length=math.hypot(b[0]-a[0],b[2]-a[2]);ux=(b[0]-a[0])/length;uz=(b[2]-a[2])/length
    return bridge,a,b,length,ux,uz

def bridge_coordinates(x,z,data):
    bridge,a,b,length,ux,uz=bridge_frame(data)
    return (x-a[0])*ux+(z-a[2])*uz, -(x-a[0])*uz+(z-a[2])*ux

def on_valley_bridge(x,z,data,margin=0.):
    bridge,a,b,length,ux,uz=bridge_frame(data);along,side=bridge_coordinates(x,z,data)
    return -margin-1e-7<=along<=length+margin+1e-7 and abs(side)<=bridge['deck_width']/2+margin+1e-7

def walking_height(area,x,z,data,grid):
    wx,_,wz=world(area,[x,0,z],data)
    if area=='ranch' and on_valley_bridge(wx,wz,data):
        return data['region']['connection']['bridge']['start'][1]-data['region']['areas'][area]['origin'][1]
    return sample(grid,x,z)

def validate_export_routes(data):
    """Sample full lane width; only the explicit deck may cover carved banks."""
    r=data['region'];stream=r['stream']['points'];widths=r['stream']['widths'];lane=r['connection']
    bridge,a,b,length,ux,uz=bridge_frame(data)
    assert lane['points'][bridge['segment_index']]==a and lane['points'][bridge['segment_index']+1]==b
    assert bridge['clear_width']>=lane['width'] and a[1]==b[1]
    validate_bridge_rail_clearance(data)
    for a,b in zip(lane['points'],lane['points'][1:]):
        dx,dz=b[0]-a[0],b[2]-a[2];length=math.hypot(dx,dz);n=math.ceil(length*4)
        for k in range(n+1):
            t=k/n
            for side in [-lane['width']/2,0,lane['width']/2]:
                x=a[0]+t*dx-dz/length*side;z=a[2]+t*dz+dx/length*side
                if on_valley_bridge(x,z,data):continue
                d,y,i,u=nearest(x,z,stream);half=(widths[i]*(1-u)+widths[i+1]*u)/2
                if d<half+1:
                    raise ValueError(f'VALLEY_LANE unbridged stream/bank at ({x:.3f},{z:.3f})')

def segment_rect_distance(a,b,rect):
    """Exact 2D distance from a closed segment to an axis-aligned filled rectangle."""
    x0,x1,y0,y1=rect
    lo,hi=0.,1.
    for origin,delta,lower,upper in [(a[0],b[0]-a[0],x0,x1),(a[1],b[1]-a[1],y0,y1)]:
        if abs(delta)<1e-12:
            if not lower<=origin<=upper:break
        else:
            enter,leave=sorted(((lower-origin)/delta,(upper-origin)/delta))
            lo,hi=max(lo,enter),min(hi,leave)
            if lo>hi:break
    else:return 0.
    def endpoint(p):return math.hypot(max(x0-p[0],0,p[0]-x1),max(y0-p[1],0,p[1]-y1))
    def corner(p):
        dx,dy=b[0]-a[0],b[1]-a[1];s=dx*dx+dy*dy
        t=max(0.,min(1.,((p[0]-a[0])*dx+(p[1]-a[1])*dy)/s)) if s else 0.
        return math.hypot(p[0]-a[0]-t*dx,p[1]-a[1]-t*dy)
    return min(endpoint(a),endpoint(b),*(corner((x,y)) for x in [x0,x1] for y in [y0,y1]))

def validate_bridge_rail_clearance(data):
    """Keep the entire lane radius clear of rails, including turns at both ends."""
    bridge,a,b,length,ux,uz=bridge_frame(data);lane=data['region']['connection']
    points=[bridge_coordinates(p[0],p[2],data) for p in lane['points']]
    distances=[]
    for side in [-1,1]:
        inner=side*bridge['clear_width']/2;outer=side*bridge['deck_width']/2
        rect=(0.,length,min(inner,outer),max(inner,outer))
        for index,(p,q) in enumerate(zip(points,points[1:])):
            clearance=segment_rect_distance(p,q,rect)
            distances.append(clearance)
            if clearance+1e-6<lane['width']/2:
                raise ValueError(f'VALLEY_LANE rail clearance at segment {index}, rail {side}: {clearance:.6f} < {lane["width"]/2:.6f}')
    return min(distances)

def build_grids(data,sha):
    out={'version':1,'source_sha256':sha,'areas':{}}
    for area,a in data['region']['areas'].items():
        x,z,w,d=a['terrain_bounds']; cols=w+1; rows=d+1; rr=routes(area,data)
        heights=[round(authored_height(area,x+i,z+j,data,rr),6) for j in range(rows) for i in range(cols)]
        modules=[f'res://assets/3d/coastal_region/{area}_{k}.glb' for k in ['terrain','paths','water','safety']]
        modules.append(f'res://assets/3d/coastal_region/{area}_{"bridge" if area=="ranch" else "pier"}.glb')
        if area=='ranch': modules.append('res://assets/3d/coastal_region/'+data['region']['connection']['bridge']['module'])
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
