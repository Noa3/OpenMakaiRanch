"""Local Qwen-2511 reference edits; explicit native template values, review-only output."""
import argparse
import hashlib
import json
from pathlib import Path
import time
import uuid
import requests
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
BASE = 'http://127.0.0.1:7860'
MODEL = 'qwen_image_edit_2511_fp8mixed.safetensors'
LORA = 'Qwen-Image-Edit-2511-Lightning-4steps-V1.0-bf16.safetensors'
TEMPLATE = Path('D:/ComfyUI/python_embeded/Lib/site-packages/comfyui_workflow_templates_json/templates/image_qwen_image_edit_2511.json')


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def api(path, payload=None):
    r = requests.get(BASE + path, timeout=30) if payload is None else requests.post(BASE + path, json=payload, timeout=30)
    r.raise_for_status()
    return r.json()


def upload(path):
    with path.open('rb') as f:
        r = requests.post(BASE+'/upload/image', files={'image': ('OMR-'+sha(path)[:20]+'.png', f, 'image/png')}, data={'type':'input','overwrite':'false'}, timeout=45)
    r.raise_for_status()
    data = r.json()
    return '/'.join(filter(None, [data.get('subfolder'), data['name']]))


def main():
    p = argparse.ArgumentParser()
    p.add_argument('source',type=Path)
    p.add_argument('prompt',type=Path)
    p.add_argument('--reference',type=Path)
    p.add_argument('--name',default='W01-qwen')
    p.add_argument('--seed',type=int,default=26091201)
    p.add_argument('--full',action='store_true')
    args = p.parse_args()
    run = ROOT/'.dream-loop'/('generation-'+args.name+'-'+uuid.uuid4().hex[:12])
    run.mkdir()
    positive = args.prompt.read_text(encoding='utf-8')
    w = {
        '1':{'class_type':'UNETLoader','inputs':{'unet_name':MODEL,'weight_dtype':'default'}},
        '2':{'class_type':'CLIPLoader','inputs':{'clip_name':'qwen_2.5_vl_7b_fp8_scaled.safetensors','type':'qwen_image','device':'default'}},
        '3':{'class_type':'VAELoader','inputs':{'vae_name':'qwen_image_vae.safetensors'}},
        '4':{'class_type':'LoadImage','inputs':{'image':upload(args.source)}},
        '5':{'class_type':'ImageScale','inputs':{'image':['4',0],'upscale_method':'lanczos','width':1536,'height':864,'crop':'disabled'}},
        '6':{'class_type':'ModelSamplingAuraFlow','inputs':{'model':['1',0],'shift':3.1}},
        '7':{'class_type':'CFGNorm','inputs':{'model':['6',0],'strength':1.0}},
        '8':{'class_type':'LoraLoaderModelOnly','inputs':{'model':['7',0],'lora_name':LORA,'strength_model':1.0}},
        '9':{'class_type':'TextEncodeQwenImageEditPlus','inputs':{'clip':['2',0],'vae':['3',0],'image1':['5',0],'prompt':positive}},
        '10':{'class_type':'TextEncodeQwenImageEditPlus','inputs':{'clip':['2',0],'vae':['3',0],'image1':['5',0],'prompt':''}},
        '11':{'class_type':'VAEEncode','inputs':{'pixels':['5',0],'vae':['3',0]}},
        '12':{'class_type':'KSampler','inputs':{'model':['7' if args.full else '8',0],'positive':['9',0],'negative':['10',0],'latent_image':['11',0],'seed':args.seed,'steps':40 if args.full else 4,'cfg':4.0 if args.full else 1.0,'sampler_name':'euler','scheduler':'simple','denoise':1.0}},
        '13':{'class_type':'VAEDecode','inputs':{'samples':['12',0],'vae':['3',0]}},
        '14':{'class_type':'SaveImage','inputs':{'images':['13',0],'filename_prefix':'OMR_VisualTargets/'+run.name+'/candidate'}}
    }
    if args.reference:
        w['15']={'class_type':'LoadImage','inputs':{'image':upload(args.reference)}}
        for n in ('9','10'):
            w[n]['inputs']['image2']=['15',0]
    info = api('/object_info')
    missing = sorted(set(n['class_type'] for n in w.values())-info.keys())
    if missing:
        raise RuntimeError('Missing native nodes: '+str(missing))
    (run/'system_stats.json').write_text(json.dumps(api('/system_stats'),indent=2),encoding='utf-8')
    (run/'workflow.json').write_text(json.dumps(w,indent=2),encoding='utf-8')
    (run/'prompt.txt').write_text(positive,encoding='utf-8')
    inputs = [{'path':str(v.resolve()),'sha256':sha(v)} for v in (args.source,args.reference) if v]
    provenance = {'provider':'local ComfyUI','model':MODEL,'lora':None if args.full else LORA,'template_path':str(TEMPLATE),'template_sha256':sha(TEMPLATE),'adaptations':['Single explicit sampler mode instead of lazy switches','1536x864 exact 16:9 ImageScale instead of FluxKontextImageScale','Official Comfy-Org repack: no optional third-party latent method override'],'inputs':inputs,'seed':args.seed,'status':'candidate_not_approved'}
    (run/'provenance.json').write_text(json.dumps(provenance,indent=2),encoding='utf-8')
    job = api('/prompt',{'prompt':w,'client_id':run.name})
    (run/'submission.json').write_text(json.dumps(job,indent=2),encoding='utf-8')
    if job.get('node_errors') or not job.get('prompt_id'):
        raise RuntimeError('Server rejected workflow: '+str(job))
    pid=job['prompt_id']
    print('SUBMITTED',pid,run,flush=True)
    deadline=time.monotonic()+1500
    while time.monotonic()<deadline:
        history=api('/history/'+pid)
        record=history.get(pid)
        if record:
            (run/'history.json').write_text(json.dumps(record,indent=2),encoding='utf-8')
            if record.get('status',{}).get('status_str')=='error':
                raise RuntimeError('Generation failed: '+str(run/'history.json'))
            images=record.get('outputs',{}).get('14',{}).get('images',[])
            if images:
                if len(images)!=1:
                    raise RuntimeError('Unexpected output count')
                r=requests.get(BASE+'/view',params=images[0],timeout=90)
                r.raise_for_status()
                output=run/'candidate.png'
                output.write_bytes(r.content)
                with Image.open(output) as im:
                    im.load()
                    if im.size!=(1536,864):
                        raise RuntimeError('Unexpected output size '+str(im.size))
                provenance.update(output_sha256=sha(output),dimensions=[1536,864],prompt_id=pid,workflow_sha256=sha(run/'workflow.json'))
                (run/'receipt.json').write_text(json.dumps(provenance,indent=2),encoding='utf-8')
                print('GENERATED',output,flush=True)
                return
        time.sleep(3)
    raise TimeoutError('Job remains unresolved; inspect '+pid+' before resubmitting. No global interrupt issued.')


if __name__=='__main__':
    main()
