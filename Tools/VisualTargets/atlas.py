#!/usr/bin/env python3
"""Offline visual-reference registry. Never generates an image, pays an API or runs Godot.
A candidate is not an approved target. A comparison package is not a visual pass.
"""
from __future__ import annotations
import argparse
import hashlib
import json
import re
import shutil
import struct
import sys
import zlib
import zipfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PACK = ROOT / 'References/VisualTargets'
CATEGORIES = {'progression','world','building_sheet','interior','character','interaction','ui'}
STATES = {'awaiting_generation','candidate','approved'}
CONTEXT_KEYS = ('scene','camera','viewport','renderer','quality','phase','weather','progression','identities','locale')

def sha(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()

def read_json(path: Path):
    return json.loads(path.read_text(encoding='utf-8'))

def within(root: Path, rel: str) -> Path:
    if not isinstance(rel,str) or not rel or '\\' in rel or Path(rel).is_absolute() or '..' in Path(rel).parts:
        raise ValueError('Path must be a relative path inside the reference pack')
    candidate = root / rel
    if not candidate.resolve().is_relative_to(root.resolve()):
        raise ValueError('Path escapes reference pack')
    for p in (candidate, *candidate.parents):
        if p == root.parent: break
        if p.is_symlink(): raise ValueError('Symlinks are not reference files')
    return candidate

def png_size(path: Path) -> tuple[int,int]:
    # Full chunk CRCs are checked, but this is not a semantic/art-quality check or a PNG decoder.
    if path.is_symlink() or not path.is_file() or path.stat().st_size > 32*1024*1024:
        raise ValueError('Expected a regular PNG up to 32 MiB')
    raw = path.read_bytes()
    if raw[:8] != b'\x89PNG\r\n\x1a\n': raise ValueError('Expected a PNG, not a renamed file')
    at=8; dims=None; saw_data=False; ended=False; chunk_index=0
    while at+12<=len(raw):
        n=struct.unpack('>I',raw[at:at+4])[0]; kind=raw[at+4:at+8]; end=at+12+n
        if end>len(raw): raise ValueError('Truncated PNG chunk')
        payload=raw[at+8:at+8+n]
        crc=struct.unpack('>I',raw[at+8+n:end])[0]
        if zlib.crc32(kind+payload)&0xffffffff != crc: raise ValueError('Invalid PNG checksum')
        if chunk_index==0 and kind!=b'IHDR': raise ValueError('IHDR must be first')
        if kind==b'IHDR':
            if dims is not None or n!=13: raise ValueError('Invalid PNG header')
            dims=struct.unpack('>II',payload[:8])
            if not(320<=dims[0]<=8192 and 240<=dims[1]<=8192): raise ValueError('PNG dimensions outside review limits')
        if kind==b'IDAT' and n: saw_data=True
        if kind==b'IEND':
            if n!=0 or end!=len(raw): raise ValueError('Invalid PNG end')
            ended=True;break
        at=end;chunk_index+=1
    if not dims or not saw_data or not ended: raise ValueError('Incomplete PNG')
    return dims

def validate(pack: Path = PACK) -> list[str]:
    m=read_json(pack/'manifest.json'); errors=[]
    if m.get('schema')!=1: errors.append('Unsupported manifest schema')
    if not re.fullmatch(r'[a-f0-9]{40}',m.get('base_commit','')): errors.append('Missing pinned base commit')
    ids=set();covered=set()
    for s in m.get('shots',[]):
        i=s.get('id','')
        if not re.fullmatch(r'[WBCIAU]\d{2}',i) or i in ids: errors.append('Invalid/duplicate shot ID: '+i)
        ids.add(i)
        if s.get('category') not in CATEGORIES or s.get('status') not in STATES: errors.append(i+': invalid category/status')
        if not s.get('brief') or not s.get('camera_contract') or not s.get('state_contract'): errors.append(i+': missing brief/context')
        for route in s.get('routes',[]):
            if route in covered: errors.append('Route mapped twice: '+route)
            covered.add(route)
        for field in ('draft_svg',):
            if s.get(field):
                try:
                    f=within(pack,s[field])
                    if not f.is_file() or f.suffix!='.svg': errors.append(i+': missing SVG draft')
                except ValueError as e:errors.append(i+': '+str(e))
        target=s.get('target')
        if s.get('status')=='awaiting_generation':
            if target or s.get('approval'):errors.append(i+': pending target cannot have artwork/approval')
        elif not target: errors.append(i+': candidate/approved requires a target')
        else:
            try:
                f=within(pack,target['path'])
                dims=png_size(f)
                if target['sha256']!=sha(f) or list(dims)!=target['dimensions']:errors.append(i+': target bytes changed')
                if not all(target.get(k) for k in ('provider','model','prompt_sha256','provenance')):errors.append(i+': missing provenance')
            except (ValueError,OSError,KeyError) as e:errors.append(i+': '+str(e))
            if s.get('status')=='approved':
                approval=s.get('approval') or {}
                if not approval.get('reviewer') or not approval.get('decision_reference') or approval.get('target_sha256')!=target.get('sha256'):
                    errors.append(i+': missing/stale approval')
    missing=set(m.get('legacy_routes',[]))-covered
    if missing:errors.append('Unmapped legacy routes: '+', '.join(sorted(missing)))
    if not ids:errors.append('No shots')
    return errors

def find_shot(m,shot_id):
    return next((s for s in m['shots'] if s['id']==shot_id),None) or (_ for _ in ()).throw(ValueError('Unknown shot '+shot_id))

def prompt(pack:Path,shot_id:str) -> str:
    m=read_json(pack/'manifest.json');s=find_shot(m,shot_id)
    text=(pack/'PROMPT_TEMPLATE.md').read_text(encoding='utf-8')
    return text+'\n\nSHOT '+s['id']+' — '+s['title']+'\n'+s['brief']+'\n\nCamera: '+s['camera_contract']+'\nState: '+s['state_contract']+'\nBaseline: '+str(s['baseline_key'])+'\nAssets: '+', '.join(s['asset_ids'])+'\nChecks:\n'+'\n'.join('- '+t for t in s['acceptance'])+'\n'

def write_manifest(pack,m):
    p=pack/'manifest.json'
    if p.is_symlink():raise ValueError('Refusing symlink manifest')
    # x-mode temp avoids following an existing link or silently overwriting another pending write.
    tmp=p.with_suffix('.json.pending')
    with tmp.open('x',encoding='utf-8') as f:f.write(json.dumps(m,ensure_ascii=False,indent=2)+'\n')
    tmp.replace(p)

def import_target(pack:Path,shot_id:str,image:Path,provider:str,model:str,provenance:str):
    if not all(t.strip() for t in (provider,model,provenance)):raise ValueError('Provider, model and provenance are required')
    m=read_json(pack/'manifest.json');s=find_shot(m,shot_id)
    if s['status']=='approved':raise ValueError('Approved targets are immutable; create a reviewed new version')
    dims=png_size(image);digest=sha(image); rel=f'targets/{shot_id}-{digest[:12]}.png';dest=within(pack,rel)
    dest.parent.mkdir(parents=True,exist_ok=True)
    if dest.exists() and sha(dest)!=digest:raise ValueError('Conflicting target filename')
    if not dest.exists():
        with dest.open('xb') as f:f.write(image.read_bytes())
    s.update(status='candidate',approval=None,target={'path':rel,'sha256':digest,'dimensions':list(dims),'provider':provider,'model':model,'provenance':provenance,'prompt_sha256':hashlib.sha256(prompt(pack,shot_id).encode()).hexdigest()})
    write_manifest(pack,m)

def approve(pack:Path,shot_id:str,reviewer:str,decision:str):
    if not reviewer.strip() or not decision.strip():raise ValueError('Record the real reviewer and decision reference')
    errors=validate(pack)
    if errors:raise ValueError('; '.join(errors))
    m=read_json(pack/'manifest.json');s=find_shot(m,shot_id)
    if s['status']!='candidate':raise ValueError('Only a real candidate can be approved')
    s['status']='approved';s['approval']={'reviewer':reviewer,'decision_reference':decision,'target_sha256':s['target']['sha256']}
    write_manifest(pack,m)

def compare_context(a:dict,b:dict):
    missing=[k for k in CONTEXT_KEYS if k not in a or k not in b]
    if missing:raise ValueError('Incomplete capture context: '+', '.join(missing))
    changed=[k for k in CONTEXT_KEYS if a[k]!=b[k]]
    if changed:raise ValueError('Different state/camera: '+', '.join(changed))
    for c in (a,b):
        if not re.fullmatch(r'[a-f0-9]{40}',c.get('source_commit','')):raise ValueError('Capture requires an exact source commit')

def import_baselines(pack:Path, archive:Path):
    records=read_json(pack/'baselines/receipts.json')
    if archive.is_symlink() or not archive.is_file() or archive.stat().st_size>32*1024*1024:
        raise ValueError('Expected a regular, bounded evidence archive')
    digest=sha(archive); selected=[r for r in records if r['archive_sha256']==digest]
    if not selected:raise ValueError('Archive is not a pinned baseline artifact')
    writes=[]
    with zipfile.ZipFile(archive) as z:
        for r in selected:
            info=z.getinfo(r['member'])
            if info.file_size>32*1024*1024:raise ValueError('Unbounded archive member')
            raw=z.read(info)
            if hashlib.sha256(raw).hexdigest()!=r['sha256']:raise ValueError('Baseline hash mismatch')
            dest=within(pack,r['file'])
            if dest.exists() and sha(dest)!=r['sha256']:raise ValueError('Refusing to overwrite a changed baseline')
            writes.append((dest,raw))
    for dest,raw in writes:
        dest.parent.mkdir(parents=True,exist_ok=True)
        if not dest.exists():
            with dest.open('xb') as f:f.write(raw)
    return len(writes)

def main(argv=None):
    p=argparse.ArgumentParser(description=__doc__);sp=p.add_subparsers(dest='command',required=True)
    sp.add_parser('validate');sp.add_parser('status')
    c=sp.add_parser('import-baselines');c.add_argument('archive',type=Path)
    c=sp.add_parser('prompt');c.add_argument('shot')
    c=sp.add_parser('import-target');c.add_argument('shot');c.add_argument('image',type=Path);c.add_argument('--provider',required=True);c.add_argument('--model',required=True);c.add_argument('--provenance',required=True)
    c=sp.add_parser('approve');c.add_argument('shot');c.add_argument('--reviewer',required=True);c.add_argument('--decision-reference',required=True)
    c=sp.add_parser('check-context');c.add_argument('reference',type=Path);c.add_argument('capture',type=Path)
    args=p.parse_args(argv)
    if args.command=='validate':
        errors=validate()
        if errors:raise ValueError('\n'.join(errors))
        print('Reference manifest valid. This does not certify generated imagery or runtime quality.')
    elif args.command=='status':
        m=read_json(PACK/'manifest.json')
        for state in sorted(STATES):print(state, sum(s['status']==state for s in m['shots']))
    elif args.command=='import-baselines':print('Imported historical baseline captures:',import_baselines(PACK,args.archive))
    elif args.command=='prompt':print(prompt(PACK,args.shot))
    elif args.command=='import-target':import_target(PACK,args.shot,args.image,args.provider,args.model,args.provenance);print('Imported as candidate; not approved.')
    elif args.command=='approve':approve(PACK,args.shot,args.reviewer,args.decision_reference);print('Recorded the supplied approval. Semantic truth of this decision must be reviewed.')
    elif args.command=='check-context':compare_context(read_json(args.reference),read_json(args.capture));print('Capture contexts match. No perceptual score was calculated.')
    return 0
if __name__=='__main__':
    try:raise SystemExit(main())
    except (OSError,ValueError,KeyError,TypeError,json.JSONDecodeError) as e:print('VISUAL TARGET ERROR:',e,file=sys.stderr);raise SystemExit(1)
