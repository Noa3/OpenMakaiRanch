from pathlib import Path
from PIL import Image, ImageDraw
import json, hashlib, zipfile, xml.etree.ElementTree as ET
import numpy as np
ROOT=Path(__file__).resolve().parents[3]; SRC=Path(__file__).parent; OUT=ROOT/'OpenMakaiRanchGame/assets/materials/ranch_home'
specs={'warm_lime_plaster':2.,'painted_warm_oak':1.,'natural_linen':.25}
report={'resolution':[512,512],'channels':{'albedo':'sRGB RGB','normal':'linear tangent-space OpenGL +Y, +Z','roughness':'linear grayscale; exact ORM G','orm':'linear R=AO (255), G=roughness, B=metallic (0)'},'materials':{}}
sheet=Image.new('RGB',(1024,840),'#282b2c'); draw=ImageDraw.Draw(sheet)
for row,(name,metres) in enumerate(specs.items()):
 for channel in ['albedo','normal','orm']:
  p=OUT/f'{name}_{channel}.png'; im=Image.open(p).convert('RGB'); im=im.resize((512,512),Image.Resampling.LANCZOS)
  if channel=='normal':
   a=np.asarray(im).astype(float)/255*2-1; a/=np.linalg.norm(a,axis=2,keepdims=True); im=Image.fromarray(np.clip((a+1)*127.5,0,255).astype('uint8'))
  im.save(p)
 Image.open(OUT/f'{name}_orm.png').getchannel('G').save(OUT/f'{name}_roughness.png')
 entry={'tile_metres':[metres,metres],'texels_per_metre':512/metres,'uv_rule':f'UV = surface metres / {metres}; oak grain runs along V','maps':{}}
 for col,ch in enumerate(['albedo','normal','roughness','orm']):
  p=OUT/f'{name}_{ch}.png'; im=Image.open(p); a=np.asarray(im).astype(float); assert im.size==(512,512)
  if ch=='normal': assert a[:,:,2].min()>180
  if ch=='orm': assert np.all(a[:,:,0]==255) and np.all(a[:,:,2]==0)
  edge=max(float(abs(a[:,0]-a[:,-1]).mean()),float(abs(a[0]-a[-1]).mean()))
  assert edge<16,(name,ch,edge)
  entry['maps'][ch]={'file':p.name,'sha256':hashlib.sha256(p.read_bytes()).hexdigest(),'range':im.getextrema(),'opposite_edge_mean_abs_delta_255':edge}
  sheet.paste(im.convert('RGB').resize((256,256)),(col*256,row*280+24)); draw.text((col*256+8,row*280+6),name+' / '+ch,fill='white')
 report['materials'][name]=entry
# Assert Krita produced an actual multilayer native document and roundtrip export.
with zipfile.ZipFile(SRC/'house_accent_palette.kra') as z:
 xml=z.read('maindoc.xml').decode(); report['krita']={'native':'house_accent_palette.kra','paint_layers':xml.count('nodetype="paintlayer"'),'roundtrip_png':str(Image.open(SRC/'house_accent_palette.png').size)}
 assert report['krita']['paint_layers']==3
sheet.save(SRC/'material_contact_sheet.png')
(SRC/'verification.json').write_text(json.dumps(report,indent=2))
(OUT/'material_manifest.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report,indent=2))
