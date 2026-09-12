"""Review-only projected labels derived from the captured camera and canonical plot coordinates."""
import json
import math
from pathlib import Path
from PIL import Image, ImageDraw
import numpy as np
root=Path(__file__).resolve().parent
source=root/'capture-0ghaskkk/evidence'
c=json.loads((source/'W01-context.json').read_text())
eye=np.array(c['camera']['position'],dtype=float)
forward=np.array([0.,0.,-1.])-eye;forward/=np.linalg.norm(forward)
right=np.cross(forward,[0,1,0]);right/=np.linalg.norm(right)
up=np.cross(right,forward)
width,height=c['viewport'];f=height/(2*math.tan(math.radians(c['camera']['fov'])/2))
def project(v):
    d=np.array(v)-eye;z=np.dot(d,forward)
    return (width/2+f*np.dot(d,right)/z,height/2-f*np.dot(d,up)/z)
image=Image.open(source/'W01-current.png').convert('RGB');draw=ImageDraw.Draw(image)
for p in c['plots']:
    x,z=p['center'];sx,sz=p['footprint'];yaw=p['yaw'];corners=[]
    for a,b in [(-1,-1),(-1,1),(1,1),(1,-1)]:
        dx=a*sx/2;dz=b*sz/2
        corners.append(project([x+dx*math.cos(yaw)+dz*math.sin(yaw),0.1,z-dx*math.sin(yaw)+dz*math.cos(yaw)]))
    draw.line(corners+[corners[0]],fill=(255,210,67),width=3)
    pos=project([x,3.2,z])
    state='HOUSE' if p['Id']=='ranch_house' else 'LEVEL '+str(c['progression']['facilities'].get(p['Id'],0))
    label=p['Id']+' | '+state
    box=draw.textbbox(pos,label,font_size=18,anchor='mm')
    draw.rectangle((box[0]-6,box[1]-4,box[2]+6,box[3]+4),fill=(15,40,35))
    draw.text(pos,label,font_size=18,fill=(255,244,210),anchor='mm')
draw.text((24,24),'SOURCE GEOMETRY GUIDE — NOT TARGET ART — EDIT OUT LABELS',font_size=25,fill=(255,255,255),stroke_width=2,stroke_fill=(10,32,27))
image.save(root/'W01-geometry-guide.png')
print(root/'W01-geometry-guide.png')
