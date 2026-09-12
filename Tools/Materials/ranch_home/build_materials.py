"""Original house family using Material Maker stock nodes; run with Python/Pillow."""
from pathlib import Path
import json, copy, subprocess, zipfile, io
from PIL import Image, ImageDraw
ROOT=Path('E:/OpenMakaiRanch')
SRC=ROOT/'Tools/Materials/ranch_home'
OUT=ROOT/'OpenMakaiRanchGame/assets/materials/ranch_home'
MM=Path('E:/material-maker-src')
GODOT='E:/GodotEditor/Godot_v4.7.2-stable_mono_win64_console.exe'
SRC.mkdir(parents=True,exist_ok=True); OUT.mkdir(parents=True,exist_ok=True)
def node(name,typ,params,x,y):
 return dict(name=name,type=typ,parameters=params,node_position=dict(x=x,y=y),seed=317,seed_locked=True)
def ramp(name,low,high,x,y):
 return node(name,'colorize',{'gradient':{'type':'Gradient','interpolation':1,'points':[dict(pos=i,a=1,r=c[0],g=c[1],b=c[2]) for i,c in enumerate([low,high])]}},x,y)
def edge(a,b,p): return dict(from_port=0,to_port=p,**{'from':a,'to':b})
specs=[('warm_lime_plaster','perlin',dict(scale_x=6,scale_y=6,iterations=2,persistence=.22),(.73,.68,.56),(.84,.79,.68),.84,.91,.10,2.0),('painted_warm_oak','perlin',dict(scale_x=32,scale_y=2,iterations=2,persistence=.25),(.56,.43,.28),(.69,.57,.40),.64,.76,.12,1.0),('natural_linen','weave',dict(columns=32,rows=32,width=.97),(.62,.56,.44),(.76,.70,.58),.89,.96,.065,.25)]
for name,typ,params,low,high,rl,rh,strength,metres in specs:
 nodes=[node('surface',typ,params,-500,0),ramp('paint',low,high,-180,-160),ramp('roughness',(rl,)*3,(rh,)*3,-180,40),node('normal','normal_map',dict(param0=9,param1=strength,param2=1,param4=0),-180,230),node('Material','material',dict(size=9,albedo_color=dict(type='Color',r=1,g=1,b=1,a=1),metallic=0,roughness=1,normal_scale=1,ao_light_affect=1,depth_scale=0,emission_energy=0),180,0)]
 # Inline the installed analytic normal shader, excluding its eager buffer node.
 normal=copy.deepcopy(next(n for n in json.loads((MM/'addons/material_maker/nodes/normal_map.mmg').read_text())['nodes'] if n['name']=='edge_detect_1'))
 normal.update(name='normal',node_position=dict(x=-180,y=230),parameters=dict(size=9,amount=strength,format=0))
 nodes[3]=normal
 graph=dict(type='graph',name=name,label=name.replace('_',' ').title(),node_position=dict(x=0,y=0),parameters={},nodes=nodes,connections=[edge('surface','paint',0),edge('surface','roughness',0),edge('surface','normal',0),edge('paint','Material',0),edge('roughness','Material',2),edge('normal','Material',4)])
 (SRC/(name+'.ptex')).write_text(json.dumps(graph,indent=2))
(SRC/'MATERIAL_MAKER_LICENSE.md').write_text((MM/'LICENSE.md').read_text())
# OpenRaster is a supported editable layered interchange file, then Krita converts it.
colors=[('Muted teal',(76,113,109)),('Terracotta',(172,105,78)),('Warm cream',(220,206,174))]
merged=Image.new('RGBA',(768,256),(0,0,0,0)); layers=[]
for i,(name,c) in enumerate(colors):
 im=Image.new('RGBA',(768,256),(0,0,0,0)); ImageDraw.Draw(im).rectangle((i*256,0,(i+1)*256-1,255),fill=c+(255,)); merged.alpha_composite(im); layers.append((name,im))
def png(im):
 b=io.BytesIO(); im.save(b,format='PNG'); return b.getvalue()
with zipfile.ZipFile(SRC/'house_accent_palette.ora','w') as z:
 z.writestr('mimetype','image/openraster'); z.writestr('mergedimage.png',png(merged))
 xml='<image version="0.0.3" w="768" h="256" name="Ranch house accents"><stack>'
 for i,(name,im) in enumerate(reversed(layers)):
  path=f'data/layer{i}.png'; z.writestr(path,png(im)); xml+=f'<layer name="{name}" src="{path}" opacity="1.0" visibility="visible" composite-op="svg:src-over" x="0" y="0"/>'
 z.writestr('stack.xml',xml+'</stack></image>')
if __name__=='__main__':
 for name,*_ in specs:
  cmd=[GODOT,'--path',str(MM),'--export-material',str(SRC/(name+'.ptex')),'--target','Godot/Godot 4 Standard','-o',str(OUT),'--size','512']
  r=subprocess.run(cmd,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,timeout=100)
  (SRC/(name+'_export.log')).write_bytes(r.stdout)
  print(name,r.returncode,r.stdout.decode(errors='replace')[-1800:],flush=True)
