"""Bounded local-only image generation. Outputs remain unapproved proposals."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import sys
import time
import uuid
import requests
from PIL import Image, ImageOps, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / 'Tools/VisualTargets'))
import atlas
BASE = 'http://127.0.0.1:7860'
MODEL = os.environ.get('OMR_IMAGE_CHECKPOINT', 'albedobaseXL_V31Large.safetensors')

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def api(path, payload=None):
    response = requests.get(BASE + path, timeout=30) if payload is None else requests.post(BASE + path, json=payload, timeout=30)
    response.raise_for_status()
    return response.json()

def generate(run, name, source, positive, negative, width, height, denoise, seed):
    with source.open('rb') as f:
        response = requests.post(BASE + '/upload/image', files={'image': (name+'-'+sha(source)[:12]+'.png',f,'image/png')},
                                 data={'type':'input','overwrite':'false'}, timeout=30)
    response.raise_for_status()
    uploaded = response.json()
    remote = '/'.join(filter(None, [uploaded.get('subfolder'), uploaded['name']]))
    workflow = {
        '1': {'class_type':'CheckpointLoaderSimple','inputs':{'ckpt_name':MODEL}},
        '2': {'class_type':'LoadImage','inputs':{'image':remote}},
        '3': {'class_type':'ImageScale','inputs':{'image':['2',0],'upscale_method':'lanczos','width':width,'height':height,'crop':'disabled'}},
        '4': {'class_type':'VAEEncode','inputs':{'pixels':['3',0],'vae':['1',2]}},
        '5': {'class_type':'CLIPTextEncode','inputs':{'clip':['1',1],'text':positive}},
        '6': {'class_type':'CLIPTextEncode','inputs':{'clip':['1',1],'text':negative}},
        '7': {'class_type':'KSampler','inputs':{'model':['1',0],'positive':['5',0],'negative':['6',0], 'latent_image':['4',0],
              'seed':seed,'steps':30 if MODEL.startswith('raMix') else 32,'cfg':5.0 if MODEL.startswith('raMix') else 6.0,
              'sampler_name':'euler_ancestral' if MODEL.startswith('raMix') else 'dpmpp_2m','scheduler':'karras','denoise':denoise}},
        '8': {'class_type':'VAEDecode','inputs':{'samples':['7',0],'vae':['1',2]}},
        '9': {'class_type':'SaveImage','inputs':{'images':['8',0],'filename_prefix':'OMR_VisualTargets/'+run.name+'/'+name}}
    }
    (run/(name+'-workflow.json')).write_text(json.dumps(workflow,indent=2),encoding='utf-8')
    job = api('/prompt', {'prompt':workflow,'client_id':run.name})
    (run/(name+'-submission.json')).write_text(json.dumps(job,indent=2),encoding='utf-8')
    if job.get('node_errors') or not job.get('prompt_id'):
        raise RuntimeError('ComfyUI validation failed: '+str(job))
    prompt_id = job['prompt_id']
    print('SUBMITTED',name,prompt_id,flush=True)
    deadline = time.monotonic() + 360
    while time.monotonic() < deadline:
        history = api('/history/'+prompt_id)
        record = history.get(prompt_id)
        if record:
            (run/(name+'-history.json')).write_text(json.dumps(record,indent=2),encoding='utf-8')
            if record.get('status',{}).get('status_str') == 'error':
                raise RuntimeError('ComfyUI execution error; see '+name+'-history.json')
            images = record.get('outputs',{}).get('9',{}).get('images',[])
            if images:
                if len(images)!=1:raise RuntimeError('Unexpected image count')
                img = images[0]
                r = requests.get(BASE+'/view',params=img,timeout=60);r.raise_for_status()
                dest = run/(name+'.png');dest.write_bytes(r.content)
                if atlas.png_size(dest)!=(width,height):raise RuntimeError('Output dimensions differ')
                with Image.open(dest) as check:check.load()
                receipt = {'provider':'local ComfyUI','model':MODEL,'prompt_id':prompt_id,'source':str(source),
                           'source_sha256':sha(source),'output':dest.name,'output_sha256':sha(dest),
                           'positive':positive,'negative':negative,'width':width,'height':height,'denoise':denoise,'seed':seed,
                           'workflow_sha256':sha(run/(name+'-workflow.json')),'status':'candidate_not_approved'}
                (run/(name+'-receipt.json')).write_text(json.dumps(receipt,indent=2),encoding='utf-8')
                print('GENERATED',dest,flush=True)
                return dest
        time.sleep(2)
    raise TimeoutError('Generation timeout. Do not resubmit automatically; inspect prompt '+prompt_id)


def main():
    parser=argparse.ArgumentParser();parser.add_argument('shot',choices=['C01','W01']);parser.add_argument('source',type=Path);args=parser.parse_args()
    run=ROOT/'.dream-loop'/('generation-'+args.shot+'-'+uuid.uuid4().hex[:12]);run.mkdir()
    info=api('/object_info/CheckpointLoaderSimple')
    if MODEL not in info['CheckpointLoaderSimple']['input']['required']['ckpt_name'][0]:raise RuntimeError('Model not installed')
    (run/'system_stats.json').write_text(json.dumps(api('/system_stats'),indent=2),encoding='utf-8')
    (run/'atlas-brief.md').write_text(atlas.prompt(atlas.PACK,args.shot),encoding='utf-8')
    (run/'source-record.json').write_text(json.dumps({'source':str(args.source),'sha256':sha(args.source)},indent=2),encoding='utf-8')
    neg='text, watermark, logo, blurry, depth of field, photograph, oversaturated, oversharpened, plastic, primitive shapes, toy, chibi, child, teenager, nude, underwear, lingerie, cleavage, fetish, deformed, extra limbs, malformed hands, hands in pockets, hidden hands, floating islands, purple sky, volcano, gothic roofs'
    if args.shot=='W01':
        positive='Polished real-time 3D farming RPG screenshot, elevated three-quarter view of the same small starter ranch, preserve the paths and positions from the image. An ordinary lush green meadow in gentle countryside, blue sky, familiar broadleaf trees with richly shaped canopies, fine grass tufts along winding ochre gravel paths, natural wood fencing, subtle warm stone lanterns. Humble rural house, practical kitchen and pasture shelter, timber framing and creamy lime plaster, weathered terracotta tiled gabled roofs, believable rough stone bases. Other plots are inactive surveyed plots with wooden stakes and modest derelict shells, not equipped production buildings. Naturally flowing small stream outside the ranch boundary, no new bridge or plot. Detailed authored foliage and subtle surface texture, soft anime game aesthetic, clear readable geometry, restrained morning sun, soft ambient occlusion, deep focus, fully rendered high quality game assets.'
        generate(run,'W01-v1',args.source,positive,neg,1536,864,0.68,726154)
        generate(run,'W01-v2',args.source,positive,neg,1536,864,0.82,726154)
    else:
        # A real current material-study render supplies only pose/color initialization.
        # Its toy proportions and unnamed identity are explicitly not design constraints.
        with Image.open(args.source) as image:
            crop=image.crop((550,110,730,420)).convert('RGB')
            fitted=ImageOps.contain(crop,(400,960))
            init=Image.new('RGB',(640,1152),(205,210,205));init.paste(fitted,((640-fitted.width)//2,(1152-fitted.height)//2))
            init.save(run/'C01-initialization.png')
        descriptions=[
            ('REF_A','one adult woman in her thirties, mature angular soft anime face, almond grey eyes, ash grey hair tied into a low ponytail with shaped hair clumps, reserved researcher, practical olive canvas waistcoat over ivory high-collar long-sleeve blouse, tailored dark brown trousers, brown leather ankle boots, slim adult build, neutral attentive expression, hands relaxed and visible'),
            ('REF_B','one adult woman in her thirties, visibly mature strong soft anime face, warm tan skin and freckles, copper red braided hair, lively ranch worker, green work overalls over fully buttoned cream rolled-sleeve shirt, heavy brown work boots, muscular sturdy adult build, friendly slight smile, hands relaxed and visible'),
            ('REF_C','one adult man in his forties, composed mature soft anime face, warm brown skin, neat dark brown short hair, dark eyebrows, ordinary charcoal long coat over high-collared teal waistcoat and ivory shirt, dark tailored trousers, leather shoes, tall lean adult build, neutral thoughtful expression, hands relaxed and visible'),
            ('REF_D','one adult woman in her fifties, warm visibly mature soft anime face, subtle age lines, dark brown skin, short silver grey hair in shaped soft waves, muted sage knitted cardigan over fully buttoned cream blouse and rust brown full length skirt, practical leather shoes, full sturdy mature build, kind calm expression, hands relaxed and visible')]
        common='High quality stylized soft-anime 3D game character, full body from head to boots, front-facing orthographic studio reference, standing upright, realistic adult eight-head body proportions, small head, long adult legs, original character, completely clothed modest everyday rural clothing. '
        ending='. Clean sculpted face and expressive eyes, precise shaped hair, matte woven fabric with seams and subtle folds, controlled highlights, soft neutral daylight, grounded contact shadow, muted pale grey studio background, no props, no text, not a drawing, not a photograph, polished real-time 3D render.'
        paths=[]
        for index,(name,description) in enumerate(descriptions):
            paths.append(generate(run,name,run/'C01-initialization.png',common+description+ending,neg,640,1152,0.95,371600+index))
        sheet=Image.new('RGB',(2560,1440),(224,226,221));draw=ImageDraw.Draw(sheet)
        for index,path in enumerate(paths):
            with Image.open(path) as image:sheet.paste(image,(640*index,144))
            draw.text((640*index+24,1350),descriptions[index][0],fill=(32,49,41),font_size=40)
        draw.text((28,42),'C01 | ORIGINAL ADULT REFERENCES | CANDIDATE — NOT APPROVED',fill=(32,49,41),font_size=38)
        sheet.save(run/'C01-lineup.png')
        print('COMPOSITED',run/'C01-lineup.png',flush=True)
    print('RUN',run,flush=True)

if __name__=='__main__':main()
