"""Validate and draw the shared regional plan; not runtime walking evidence."""
from __future__ import annotations

import argparse
import hashlib
import json
import math
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'OpenMakaiRanchGame/data/world/organic_layout.json'


def regional(area, point):
    x, y, z = point
    c, s = math.cos(area['yaw']), math.sin(area['yaw'])
    ox, oy, oz = area['origin']
    return [ox + c*x + s*z, oy+y, oz-s*x+c*z]


def samples(points, spacing=0.4):
    """Catmull-Rom in plan view; 3D heights interpolate monotonically per leg."""
    padded = [points[0]] + list(points) + [points[-1]]
    result = []
    for k in range(1, len(padded)-2):
        a, b, c, d = padded[k-1:k+3]
        steps = max(4, math.ceil(math.dist(b, c)/spacing))
        for i in range(steps):
            t = i/steps
            value = [.5*(2*b[q]+(-a[q]+c[q])*t+(2*a[q]-5*b[q]+4*c[q]-d[q])*t*t+(-a[q]+3*b[q]-3*c[q]+d[q])*t*t*t) for q in range(len(b))]
            if len(value) == 3:
                value[1] = b[1] + (c[1]-b[1])*t
            result.append(value)
    return result + [list(points[-1])]


def on_segment(point, a, b):
    x, z = point
    cross = (x-a[0])*(b[1]-a[1])-(z-a[1])*(b[0]-a[0])
    return abs(cross) < 1e-7 and min(a[0], b[0])-1e-7 <= x <= max(a[0], b[0])+1e-7 and min(a[1], b[1])-1e-7 <= z <= max(a[1], b[1])+1e-7


def inside(point, polygon):
    x, z = point
    result = False
    for a, b in zip(polygon, polygon[1:]+polygon[:1]):
        if on_segment(point, a, b):
            return True
        if (a[1] > z) != (b[1] > z) and x < (b[0]-a[0])*(z-a[1])/(b[1]-a[1])+a[0]:
            result = not result
    return result


def ribbon(points, width):
    """Full swept width including round caps, not only centreline controls."""
    ps = samples(points)
    for i, p in enumerate(ps):
        a, b = ps[max(0, i-1)], ps[min(len(ps)-1, i+1)]
        dx, dz = b[0]-a[0], b[-1]-a[-1]
        length = math.hypot(dx, dz)
        if length <= 1e-9:
            continue
        for side in (-1, 1):
            yield [p[0]-side*dz/length*width/2, p[-1]+side*dx/length*width/2]
        yield [p[0], p[-1]]
    for p in (ps[0], ps[-1]):
        for i in range(24):
            angle = i*math.tau/24
            yield [p[0]+math.cos(angle)*width/2, p[-1]+math.sin(angle)*width/2]


def envelope(plot):
    c, s = abs(math.cos(plot['yaw'])), abs(math.sin(plot['yaw']))
    w, d = (v+1 for v in plot['footprint'])
    x, z = plot['center']
    ex, ez = (c*w+s*d)/2, (s*w+c*d)/2
    return [x-ex, z-ez, x+ex, z+ez]


def path_length(points):
    return sum(math.dist(a,b) for a,b in zip(points,points[1:]))


def intersects_plot(point, plot):
    # Test the rotated envelope, not its much larger axis-aligned bounding box.
    # The latter is useful for canopy reservation but reports false road collisions
    # alongside angled facades. Keep the same half-metre overhang allowance.
    dx, dz = point[0]-plot['center'][0], point[1]-plot['center'][1]
    c, s = math.cos(plot['yaw']), math.sin(plot['yaw'])
    return abs(c*dx-s*dz) < (plot['footprint'][0]+1)/2 and abs(s*dx+c*dz) < (plot['footprint'][1]+1)/2


def validate(data):
    r = data['region']
    errors = []
    if r['units'] != 'metres' or r['sea_level'] != 0:
        errors.append('regional units or sea datum changed')
    stream = samples(r['stream']['points'])
    if any(b[1] >= a[1] for a,b in zip(stream,stream[1:])):
        errors.append('stream must descend throughout the sampled curve')
    if abs(stream[-1][1]-r['sea_level']) > 1e-7:
        errors.append('stream mouth does not meet sea level')
    areas = r['areas']
    seam = [regional(areas[key], areas[key]['seam']) for key in ('ranch','town')]
    seam_gap = math.dist(*seam)
    if seam_gap > 1e-5:
        errors.append(f'regional scene seam gap: {seam_gap:.6f} m')
    paths = []
    for area_name in ('ranch','town'):
        area = areas[area_name]
        core = area['core_bounds']
        if [core[2]/2,core[3]/2] != data[area_name]['half_extents']:
            errors.append(area_name+': conflicting area extents')
        for plot in data[area_name]['plots']:
            x0,z0,x1,z1 = envelope(plot)
            if not all(inside(p,area['walkable_boundary']) for p in ([x0,z0],[x1,z0],[x1,z1],[x0,z1])):
                errors.append(area_name+': plot outside actual walkable boundary: '+plot['id'])
        paths.extend((area_name,p['id'],p['points'],p['width']) for p in data[area_name]['paths'])
    # The connection is already regional; convert to ranch-local plan coordinates.
    connection = r['connection']
    points = [[p[0]-areas['ranch']['origin'][0], p[2]-areas['ranch']['origin'][2]] for p in connection['points']]
    paths.append(('ranch','VALLEY_LANE',points,connection['width']))
    paths.append(('town','SHORE_LANE',r['coast']['shore_route'],r['coast']['shore_route_width']))
    path_reports = []
    for area,name,points,width in paths:
        swept = list(ribbon(points,width))
        outside = next((p for p in swept if not inside(p,areas[area]['walkable_boundary'])),None)
        if outside is not None:
            errors.append(f'{area}/{name}: swept path leaves walkable boundary at {outside}')
        blocked = []
        for plot in data[area]['plots']:
            if any(intersects_plot(point,plot) for point in swept):
                blocked.append(plot['id'])
        if blocked:
            errors.append(f'{area}/{name}: swept path intersects reserved envelope: {blocked}')
        path_reports.append({'area':area,'id':name,'sampled_length_m':path_length(samples(points)),'width_m':width,'outside':outside,'blocked_envelopes':blocked})
    return {'source':'shared regional plan, not a gameplay run','errors':errors,'seam_gap_m':seam_gap,
            'connection_control_length_m':path_length(connection['points']),
            'connection_smoothed_length_m':path_length(samples(connection['points'])),
            'stream_monotonic':all(b[1]<a[1] for a,b in zip(stream,stream[1:])),
            'max_connection_grade':max(abs(b[1]-a[1])/math.hypot(b[0]-a[0],b[2]-a[2]) for a,b in zip(connection['points'],connection['points'][1:])),
            'paths':path_reports}


def draw(data, report, destination):
    from PIL import Image, ImageDraw, ImageFont
    image = Image.new('RGB',(1600,1390),'#f5f1e7')
    d = ImageDraw.Draw(image)
    font_path = 'C:/Windows/Fonts/segoeui.ttf'
    def font(size):
        return ImageFont.truetype(font_path,size) if Path(font_path).is_file() else ImageFont.load_default()
    title, heading, body, small = font(32),font(23),font(19),font(16)
    d.text((38,23),'OKACHI · TAL UND KÜSTENBUCHT',font=title,fill='#253b36')
    d.text((40,68),'Räumlicher Blockout-Entwurf · keine freigegebene Zielgrafik · keine Laufzeitmessung',font=body,fill='#5b665d')
    r=data['region']; scale=2.5
    def screen(x,z):return (310+x*scale,435+z*scale)
    def points(ps):return [screen(p[0],p[-1]) for p in ps]
    def line(ps,color,width=3):d.line(points(ps),fill=color,width=width,joint='curve')
    def label(x,z,text,color='#263b34',size=small):
        p=screen(x,z); box=d.textbbox(p,text,font=size);d.rounded_rectangle((box[0]-4,box[1]-2,box[2]+4,box[3]+3),3,fill='#f8f3e5');d.text(p,text,font=size,fill=color)
    # Coherent valley shoulders, drawn as broad bands rather than random rock dots.
    for poly,color in [([[-88,-132],[7,-132],[28,-74],[7,-35],[-40,40],[-83,117]],'#aec19c'),
                       ([[-74,-132],[-4,-132],[11,-83],[-17,-28],[-50,63],[-70,120]],'#819d7b'),
                       ([[-61,-132],[-15,-132],[-7,-88],[-33,-25],[-56,65]],'#668669')]:
        d.polygon(points(poly),fill=color)
    town=r['areas']['town']
    shore=[regional(town,p) for p in r['coast']['shoreline']]
    sea_poly=[shore[0]]+shore[1:]+[[360,0,400],[360,0,115]]
    d.polygon(points(sea_poly),fill='#77b8c5')
    for i in range(6):
        x=158+i*20;line([[x,165],[x+5,170],[x+17,165]],'#c1e1e0',2)
    # Explicit playable regions, including corridor and dry pier projection.
    for name in ('ranch','town'):
        area=r['areas'][name]
        ps=[regional(area,[p[0],0,p[1]]) for p in area['walkable_boundary']]
        d.polygon(points(ps),fill='#d6dfbc')
        line(ps+[ps[0]],'#647c59',3)
    line(shore,'#e7cf9b',10)
    line(samples(r['stream']['points']),'#3184a1',6)
    for p in r['stream']['points']:
        x,y,z=p
        d.ellipse((screen(x,z)[0]-4,screen(x,z)[1]-4,screen(x,z)[0]+4,screen(x,z)[1]+4),fill='#164e70')
        label(x+3,z-3,f'{y:g} m','#165d79')
    for name in ('ranch','town'):
        area=r['areas'][name]
        for path in data[name]['paths']:
            ps=[regional(area,[p[0],0,p[1]]) for p in samples(path['points'])]
            line(ps,'#ab7954',max(2,round(path['width']*scale)))
        for plot in data[name]['plots']:
            x,z=plot['center'];w,h=plot['footprint'];c,s=math.cos(plot['yaw']),math.sin(plot['yaw'])
            corners=[[x+c*dx+s*dz,0,z-s*dx+c*dz] for dx,dz in [(-w/2,-h/2),(w/2,-h/2),(w/2,h/2),(-w/2,h/2)]]
            d.polygon(points([regional(area,p) for p in corners]),fill='#905c49',outline='#f9e7c3')
    line(samples(r['connection']['points']),'#ab7954',8)
    coast_path=[regional(town,[p[0],0,p[1]]) for p in samples(r['coast']['shore_route'])]
    line(coast_path,'#c19056',6)
    side=r['ranch_side_route'];line(samples(side['points']),'#ab7954',6)
    pier=r['coast']['pier'];x,z,w,h=pier['deck_bounds'];d.polygon(points([regional(town,[a,0,b]) for a,b in [(x,z),(x+w,z),(x+w,z+h),(x,z+h)]]),fill='#826442')
    label(-82,-141,'N ↑',size=heading)
    label(-18,-95,'Quellhügel / Wald',size=heading)
    label(16,-44,'RANCH · 12 m',size=heading)
    label(17,-32,'120 × 100 m + Talweg')
    label(-59,33,'Westweide / Fußbrücke')
    label(43,76,'Optionaler Talweg',size=heading)
    label(43,88,f"{report['connection_smoothed_length_m']:.1f} m geplant")
    label(74,178,'Szenennaht · 4 m')
    label(2,261,'ORT · Marktterrasse 4 m',size=heading)
    label(2,274,'180 × 130 m inkl. Küstenrand')
    label(137,242,'Bucht / offenes Meer',size=heading)
    label(137,255,'Meeresspiegel 0 m')
    label(109,202,'Bachmündung')
    label(132,269,'Strand + Anleger 1,4 m')
    label(143,296,'Felsvorsprung')
    d.line((70,1290,195,1290),fill='#273d35',width=4);d.text((70,1301),'50 m',font=small,fill='#273d35')
    # Sidebar with honest measured-vs-proposed separation.
    d.rounded_rectangle((1110,115,1562,1320),16,fill='#eae6d9',outline='#c6ccb9',width=2)
    x=1133;y=141
    entries=[('GEMEINSAMER HÖHENBEZUG',heading),('Quelle 31 m → Bach → Meer 0 m',body),('Ranch: ruhige Terrasse auf 12 m',body),('Markt: geschützte Terrasse auf 4 m',body),('Trockener Strand: mindestens 0,75 m',body),('Anleger: 1,4 m; keine Schwimmroute',body),('',body),
             ('ALLTAG BLEIBT KOMPAKT',heading),('Vorhandene Gebäudegrößen bleiben.',body),('Hofgruppen und Dienste im Kern.',body),('Zusatzfläche: Weiden, Höfe, Natur.',body),('Talweg freiwillig; Schnellreise bleibt.',body),('',body),
             ('ORIENTIERUNG',heading),('Nord = oben, Ost = rechts.',body),('Stadt lokal um 180° gedreht.',body),('Beide lokalen South Gates bleiben.',body),('Übergangspunkte regional deckungsgleich.',body),('',body),
             ('LEGENDE',heading),('Grüne Kontur: begehbare Grenze',body),('Braun: Wege / dunkler: Gebäude',body),('Blau: abfallender Bach und Meer',body),('Sand: begrenzter Küstenzugang',body),('',body),
             ('NOCH ZU PRÜFEN',heading),('Physik, Kamera, Begleitung an Hängen.',body),('Ganze Wegbreiten und Türzugänge.',body),('Echte Laufzeiten bei Standardbewegung.',body),('Save/Load, Reisen, Forward+, Qualität.',body),('',body),
             ('STAND DIESER GRAFIK',heading),('Plan aus gemeinsamer JSON-Datenquelle.',body),('Keine behauptete Spielaufnahme.',body),('Kein Nachweis durch Teleport.',body)]
    for text,f in entries:
        d.text((x,y),text,font=f,fill='#30473c');y+=33 if f==heading else 28
    d.text((42,1350),'Bachhöhe wird entlang seiner ganzen Kurve monoton interpoliert. Figuren, Uhr und Bewegung bleiben unverändert.',font=small,fill='#52634e')
    destination.parent.mkdir(parents=True,exist_ok=True)
    image.save(destination)


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--out',type=Path,default=ROOT/'OpenMakaiRanchGame/docs/art/coastal-region-plan')
    args=parser.parse_args()
    raw=SOURCE.read_bytes();data=json.loads(raw)
    report=validate(data);report['source_sha256']=hashlib.sha256(raw).hexdigest()
    args.out.mkdir(parents=True,exist_ok=True)
    (args.out/'validation.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
    draw(data,report,args.out/'overview.png')
    print(json.dumps(report,indent=2));print('OVERVIEW',args.out/'overview.png')
    return 1 if report['errors'] else 0


if __name__=='__main__':
    raise SystemExit(main())
