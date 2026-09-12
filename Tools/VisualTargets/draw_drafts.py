#!/usr/bin/env python3
"""Draw editable UI layout studies, never AI images or rendered-game evidence.
Only writes References/VisualTargets/ui and plans, with explicit --write.
"""
from __future__ import annotations
import argparse
from html import escape
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PACK = ROOT / 'References/VisualTargets'
P = {'bg':'#0e201f','panel':'#152b2a','surface':'#213e39','text':'#f5f2e9','muted':'#c5d3cb','gold':'#efd19b','green':'#31564b'}

def rect(x,y,w,h,fill='panel',radius=14,stroke=None):
    return f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="{radius}" fill="{P.get(fill,fill)}"'+(f' stroke="{P.get(stroke,stroke)}" stroke-width="2"' if stroke else '')+'/>'
def txt(x,y,s,size=24,fill='text',weight=400):
    return f'<text x="{x}" y="{y}" font-size="{size}" fill="{P.get(fill,fill)}" font-weight="{weight}">{escape(s)}</text>'
def line(x,y,x2,y2):
    return f'<path d="M{x},{y}L{x2},{y2}" stroke="{P["green"]}" stroke-width="2"/>'
def button(x,y,w,s,selected=False):
    return rect(x,y,w,56,'gold' if selected else 'surface',10,'gold' if selected else None)+txt(x+20,y+36,s,24,'bg' if selected else 'text',600)
def wrap(x,y,s,width=48,size=24,fill='muted'):
    import textwrap
    return ''.join(txt(x,y+i*34,t,size,fill) for i,t in enumerate(textwrap.wrap(s,width)))
def box(x,y,w,h,title,subtitle=''):
    return rect(x,y,w,h)+txt(x+24,y+42,title,28,'text',600)+(txt(x+24,y+78,subtitle,22,'muted') if subtitle else '')
def begin(title,w=1600,h=900):
    return f'<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" viewBox="0 0 {w} {h}" role="img" aria-label="{escape(title)}"><title>{escape(title)} — UI layout draft, not game render</title><g font-family="DejaVu Sans, sans-serif">'+rect(0,0,w,h,'bg',0)
def finish():return '</g></svg>\n'
def header(s):
    return txt(48,49,'OPEN MAKAI RANCH  /  VISUAL TARGET ATLAS',20,'muted',600)+txt(48,113,s['title'],44,'text',600)+txt(1552,45,'',20)+txt(48,857,f"{s['id']}  /  LAYOUT DRAFT • NOT IN-ENGINE • SAMPLE COPY / VALUES",20,'gold')
def backdrop(x,y,w,h,label):
    # Deliberately diagrammatic; do not substitute this linework for target landscape artwork.
    a=rect(x,y,w,h,'#466456')
    a+=f'<path d="M{x},{y+h*.68} Q{x+w*.25},{y+h*.29} {x+w*.52},{y+h*.66} T{x+w},{y+h*.52} L{x+w},{y+h}H{x}Z" fill="#314f40"/>'
    a+=f'<path d="M{x+w*.25},{y+h}Q{x+w*.59},{y+h*.70} {x+w*.5},{y+h*.52}" stroke="#b7b79a" fill="none" stroke-width="16"/>'
    a+=rect(x+20,y+20,w-40,48,'panel',8)+txt(x+38,y+51,label,21,'muted')
    return a

def render(s):
    a=begin(s['title'])+header(s); layout=s['layout']
    tabs=s['tabs']
    if layout=='title':
        a+=backdrop(620,152,932,656,'2D BACKGROUND ART SLOT — final illustration pending')
        a+=txt(74,243,'A place to come home to.',30,'gold')
        for n,t in enumerate(tabs):a+=button(72,280+n*72,460,t,n==0)
        a+=txt(672,735,'Green valley • ordinary trees • quiet mana light',23,'text')
    elif layout in ('hud','dialogue','combat'):
        a+=backdrop(48,152,1504,656,'WORLD VIEW SLOT — actual scene render required')
        if layout=='hud':
            a+=box(72,228,350,106,'Spring  •  Morning','Weather from canonical state')
            a+=box(1136,228,390,174,'Ranch check','1 tracked task — example')
            a+=txt(1160,346,'Kitchen: review team  →',23,'gold')
            a+=box(72,668,320,104,'Stamina','Current / maximum')
            a+=button(570,704,460,'E  Speak with resident',True)
        elif layout=='dialogue':
            a+=box(128,544,1344,242,'Resident name','Subtitles stay in editable UI, not the generated image.')
            a+=txt(156,668,'“Shall we take a break after this?”',28)
            a+=button(824,634,590,'Ask about their day',True)+button(824,708,590,'Leave the conversation')
        else:
            a+=box(72,238,360,114,'Player turn','Health • target • status')+box(1170,238,356,114,'Opponent','Health and readable intent')
            a+=rect(72,632,1454,140)
            for n,t in enumerate(['Action','Skill','Item','Target']):a+=button(96+n*347,676,320,t,n==0)
    elif layout in ('creator','resident','wardrobe','pets'):
        a+=backdrop(48,156,688,650,'CHARACTER / PET RENDER SLOT — not an approved identity')
        a+=box(768,156,784,650,s['title'],'Context and identity from the selected actor')
        for n,t in enumerate(tabs[:5]):a+=button(796+n%3*244,262+n//3*70,228,t,n==0)
        a+=txt(802,468,'Selection details',30,'text',600)
        a+=wrap(802,510,'Readable information, one decision at a time. Preview changes before applying. Cancel preserves the original state.',45)
        a+=button(802,704,460,s['primary_action'],True)+button(1278,704,234,'Back')
    elif layout in ('station','house'):
        a+=backdrop(48,156,690,650,'ADDRESSED PLACE REMAINS VISIBLE')
        a+=box(770,156,782,650,'Kitchen' if layout=='station' else 'Ranch house','Local interaction • no remote ranch-wide commands')
        for n,t in enumerate(tabs[:3]):a+=button(798+n*244,258,228,t,n==0)
        a+=txt(802,383,'Current situation',30,'text',600)
        a+=wrap(802,426,'One relevant status and one next action. Detailed output and costs are available on demand.',44)
        a+=rect(800,520,716,125,'surface')+txt(824,562,'Before → After',26,'gold')+txt(824,603,'Quote, upkeep, wallet and availability',23)
        a+=button(802,704,460,s['primary_action'],True)+button(1278,704,234,'Back')
    elif layout in ('settings','pause','save'):
        a+=box(48,156,396,650,'Options' if layout=='settings' else s['title'])
        for n,t in enumerate(tabs[:6]):a+=button(72,250+n*76,348,t,n==0)
        a+=box(476,156,1076,650,'Display settings' if layout=='settings' else 'Selected option','One focused pane • keyboard and controller navigation')
        values= [('Graphics quality','High'),('Render scale','100%'),('UI text scale','150%'),('Reduced motion','On')] if layout=='settings' else [('Selection','Current slot / action'),('Last saved','Recorded timestamp'),('Location','Recorded location'),('Safety','Confirm before overwrite')]
        for n,(label,value) in enumerate(values):
            a+=rect(506,270+n*87,1016,72,'surface')+txt(530,315+n*87,label,25)+txt(1174,315+n*87,value,24,'gold')
        a+=button(506,704,580,s['primary_action'],True)+button(1106,704,416,'Discard / Back')
    elif layout=='report':
        a+=box(48,156,1504,146,'Day complete','Committed results — not a forecast and never a second payment')
        for n,(t,v) in enumerate([('Income','Recorded income'),('Costs','Recorded costs'),('Balance','Reconciled net')]):
            a+=box(48+n*510,330,484,160,t)+txt(74+n*510,441,v,29,'gold')
        a+=box(48,518,992,286,'Tomorrow','Resident condition • completed work • one next step')
        a+=wrap(76,634,'Show the most important change first. Expand the existing ledger for details. No fabricated positive result.',56)
        a+=button(1070,704,482,s['primary_action'],True)
    else:
        # Shared workspace structure: the route map retains every original operation.
        a+=box(48,156,326,650,'Browse')
        for n,t in enumerate(tabs[:5]):a+=button(70,244+n*76,282,t,n==0)
        a+=box(402,156,666,650,'Available entries','Search / filter — using canonical data')
        for n,t in enumerate(['Selected entry','Another available entry','Unavailable entry']):
            a+=rect(428,270+n*126,612,106,'green' if n==0 else 'surface',12,'gold' if n==0 else None)+txt(452,313+n*126,t,26)+txt(452,352+n*126,'Details on selection' if n<2 else 'Requirement shown, not color alone',21,'muted')
        a+=box(1096,156,456,650,'Details','One selected entry')
        a+=wrap(1124,297,'Name, description and truthful requirements. Show the reason when unavailable.',23)
        a+=line(1124,469,1524,469)+txt(1124,518,'Cost / effect / quantity',25,'gold')
        a+=wrap(1124,565,'Illustrative copy only. Never infer gameplay values from this image.',23,22)
        a+=button(1124,704,400,s['primary_action'],True)
    return a+finish()

def plan(style):
    a=begin('Fixed ranch plot plan',1600,1000)
    a+=txt(48,52,'RANCH CONTINUITY PLAN',22,'gold',600)+txt(48,114,'Keep the place. Improve its craft.',42,'text',600)
    a+=rect(48,154,994,796,'#3d5d4a',4)
    # Original plot centers; planar draft only. Yaw is documented in source, not guessed here.
    ox,oy,k=545,548,19
    a+=rect(ox-1.6*k,oy+7*k,3.2*k,12.4*k,'#c1b895',0)+txt(ox-140,926,'Protected town-gate approach',19,'bg',600)
    for plot in style['plots']:
        x,z=plot['center_xz'];w,h=plot['footprint_xz']
        px,py=ox+x*k,oy+z*k
        a+=rect(px-w*k/2,py-h*k/2,w*k,h*k,'panel',7,'gold')
        a+=txt(px-w*k/2+7,py-3,plot['id'],18)+txt(px-w*k/2+7,py+26,f'{w:g} × {h:g} m',18,'gold')
    a+=txt(1090,222,'Fixed across W01–W03',28,'gold',600)
    for n,t in enumerate(['Same plot centers','Same doors and clearances','Same gate and public route','Same named tree landmarks','Same river course once authored']):a+=txt(1090,280+n*56,t,23)
    a+=wrap(1090,614,'Planning diagram: footprints are shown unrotated for labeling, not a construction or collision map. Read the source yaw and reserved envelopes before building.',29,22)
    a+=txt(48,983,'LAYOUT DIAGRAM • NOT FINAL WORLD ART • proposed finish stages do not create gameplay unlocks',19,'gold')
    return a+finish()

def atlas(manifest):
    # The overview uses symbols to share the entire large world-space background.
    a=begin('Menu coverage atlas',2000,1540)
    a+=txt(40,54,'OPEN MAKAI RANCH / 25 UI FAMILIES',30,'gold',600)
    a+=txt(40,89,'Layout studies only. Not generated target art or game screenshots.',20,'muted')
    a+='<defs><g id="tile">'+rect(0,0,368,244,'panel',8)+rect(14,43,340,24,'surface',4)+rect(14,83,97,137,'surface',4)+rect(123,83,231,87,'surface',4)+rect(123,188,231,32,'gold',4)+'</g></defs>'
    for j,s in enumerate(x for x in manifest['shots'] if x['category']=='ui'):
        x=40+(j%5)*388;y=122+(j//5)*278
        a+=f'<g id="{s["id"]}" transform="translate({x},{y})"><use href="#tile"/>'
        a+=txt(14,29,s['id']+' / '+s['title'],18,'text',600)
        a+=txt(24,60,s['layout'].upper(),13,'muted')
        for k,tab in enumerate(s['tabs'][:4]):a+=txt(23,105+k*31,tab[:15],12,'text')
        a+=txt(136,109,'Context before action',16,'gold')
        a+=txt(136,141,'Real text / source data',14,'muted')
        a+=txt(134,209,s['primary_action'],14,'bg',600)+'</g>'
    return a+finish()

def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('--write',action='store_true');a=p.parse_args()
    if not a.write:p.error('Pass --write to create/update the owned SVG draft files.')
    manifest=json.loads((PACK/'manifest.json').read_text(encoding='utf-8'));style=json.loads((PACK/'style-lock.json').read_text(encoding='utf-8'))
    for s in manifest['shots']:
        if s['category']=='ui':
            out=PACK/'ui'/f"{s['id']}.svg";out.parent.mkdir(parents=True,exist_ok=True);out.write_text(render(s),encoding='utf-8')
    out=PACK/'plans/ranch-layout.svg';out.parent.mkdir(parents=True,exist_ok=True);out.write_text(plan(style),encoding='utf-8')
    (PACK/'ui/UI_ATLAS.svg').write_text(atlas(manifest),encoding='utf-8')
    print('Wrote 25 labeled UI layout drafts, an atlas and one plot diagram; no target screenshots generated.')
if __name__=='__main__':main()
