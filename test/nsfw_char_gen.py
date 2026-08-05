"""
NSFW RPG Maker pixel art character spritesheet generator v2.
- 48x112 frames, 3 cols x 4 rows = 144x448 sheet
- Proper walk cycle with body bob + arm swing
- NSFW layers: breast sizes 0-6, pubic, 12 hairs, 50+ cloths, 6 skins
- GIF preview for visual verification
"""

from PIL import Image
import os

OUT = os.path.dirname(os.path.abspath(__file__))

# === COLOR PALETTES ===
SKINS = [
    [(255, 210, 180), (245, 195, 165), (255, 225, 200), (160, 100, 60), (255, 200, 170), (210, 160, 110)],  # base
    [(220, 175, 145), (210, 160, 130), (220, 190, 165), (125, 70, 35), (220, 165, 135), (175, 125, 80)],   # shadow
    [(180, 140, 115), (170, 125, 100), (180, 155, 130), (95, 50, 25), (180, 130, 105), (140, 95, 60)],     # dark
    [(200, 140, 130), (190, 130, 120), (205, 150, 140), (120, 60, 50), (195, 135, 125), (160, 100, 85)],   # blush
]
NIPPLES = [(220, 160, 150), (210, 150, 140), (225, 170, 160), (140, 75, 60), (215, 155, 145), (180, 115, 100)]
AREOLAE = [(200, 140, 130), (190, 130, 120), (205, 150, 140), (120, 60, 50), (195, 135, 125), (160, 100, 85)]

HAIRS = [
    (60, 50, 45), (180, 140, 80), (200, 80, 60), (140, 60, 140),
    (60, 120, 180), (220, 200, 170), (30, 30, 30), (80, 180, 140),
    (255, 160, 180), (160, 100, 60), (200, 180, 100), (100, 60, 180),
]
HAIR_SH = [(a-20, b-18, c-17) for a,b,c in HAIRS]
HAIR_HL = [(min(a+30,255), min(b+30,255), min(c+30,255)) for a,b,c in HAIRS]

CLOTHS = [
    (50, 120, 200), (200, 60, 60), (60, 180, 100), (180, 60, 140),
    (200, 160, 60), (60, 60, 60), (180, 120, 200), (60, 180, 180),
    (200, 140, 60), (100, 60, 180), (50, 150, 180), (180, 80, 80),
    (80, 140, 80), (160, 80, 120), (180, 160, 80), (80, 80, 80),
    (160, 100, 180), (80, 160, 160), (180, 120, 80), (120, 80, 160),
    (70, 130, 160), (160, 100, 100), (100, 120, 100), (140, 100, 120),
    (160, 140, 100), (100, 100, 100), (140, 120, 160), (100, 140, 140),
    (160, 130, 100), (140, 100, 140), (40, 100, 160), (180, 80, 120),
    (60, 120, 80), (160, 60, 100), (180, 140, 80), (60, 60, 80),
    (140, 80, 160), (60, 140, 120), (180, 120, 80), (100, 60, 120),
]
CLOTH_SH = [(a-15, b-15, c-15) for a,b,c in CLOTHS[:10]] * 4
CLOTH_HL = [(a+15, b+15, c+15) for a,b,c in CLOTHS[:10]] * 4

OUTLINE = (25, 20, 15)
TRANS = (0, 0, 0, 0)
EYE = (40, 30, 20)
MOUTH = (180, 80, 80)
WHITE = (255, 255, 255)


class Char2:
    W, H = 48, 112
    COLS, ROWS = 3, 4

    def __init__(self, skin=0, hair=0, cloth=0, breast=3, body=0):
        self.skin = skin
        self.hair = hair
        self.cloth = cloth
        self.breast = min(breast, 6)
        self.body = body  # 0=big, 1=small

    def sk(self, idx=0):
        return SKINS[idx][self.skin % 6]

    def hair_c(self):
        return HAIRS[self.hair % 12]

    def hair_sh(self):
        return HAIR_SH[self.hair % 12]

    def hair_hl(self):
        return HAIR_HL[self.hair % 12]

    def cloth_c(self):
        return CLOTHS[self.cloth % 40]

    def cloth_sh(self):
        return CLOTH_SH[self.cloth % len(CLOTH_SH)]

    def cloth_hl(self):
        return CLOTH_HL[self.cloth % len(CLOTH_HL)]

    def _blank(self):
        return [[TRANS] * self.W for _ in range(self.H)]

    def _set(self, img, y, x, color):
        if 0 <= x < self.W and 0 <= y < self.H:
            img[y][x] = color

    def _rect(self, img, y1, x1, y2, x2, color, exclude=None):
        for y in range(y1, y2 + 1):
            for x in range(x1, x2 + 1):
                if exclude and exclude(x, y):
                    continue
                self._set(img, y, x, color)

    def _vline(self, img, x, y1, y2, color):
        for y in range(y1, y2 + 1):
            self._set(img, y, x, color)

    def _hline(self, img, y, x1, x2, color):
        for x in range(x1, x2 + 1):
            self._set(img, y, x, color)

    def _outline(self, img):
        result = [row[:] for row in img]
        for y in range(self.H):
            for x in range(self.W):
                if img[y][x] != TRANS:
                    for dx, dy in [(0,-1),(0,1),(-1,0),(1,0)]:
                        nx, ny = x+dx, y+dy
                        if 0 <= nx < self.W and 0 <= ny < self.H and img[ny][nx] == TRANS:
                            result[y][x] = OUTLINE
                            break
        return result

    def _to_pil(self, img):
        pil = Image.new('RGBA', (self.W, self.H))
        px = pil.load()
        for y in range(self.H):
            for x in range(self.W):
                c = img[y][x]
                px[x, y] = c if len(c) == 4 else (*c, 255)
        return pil

    def build_front(self, ft='stand'):
        """Nude front view body with walk cycle."""
        img = self._blank()
        s = self.sk
        ss = lambda: self.sk(1)
        sd = lambda: self.sk(2)
        sb = lambda: self.sk(3)

        leg_off = 0
        body_bob = 0
        arm_swing = 0
        if ft == 'walk1':
            leg_off = 2; body_bob = -1; arm_swing = 1
        elif ft == 'walk2':
            leg_off = -2; body_bob = -1; arm_swing = -1

        by = body_bob

        # Head
        self._rect(img, 4+by, 16, 19+by, 32, s())

        # Hair back
        self._rect(img, 2+by, 14, 7+by, 34, self.hair_c())
        # Hair sides
        self._rect(img, 6+by, 12, 17+by, 15, self.hair_c())
        self._rect(img, 6+by, 33, 17+by, 36, self.hair_c())
        # Hair top
        self._rect(img, 4+by, 12, 7+by, 36, self.hair_c())
        # Hair bangs
        self._rect(img, 6+by, 17, 11+by, 31, self.hair_c())
        self._rect(img, 11+by, 19, 15+by, 29, self.hair_sh())

        # Eyes
        self._set(img, 12+by, 19, EYE); self._set(img, 12+by, 20, WHITE)
        self._set(img, 12+by, 28, EYE); self._set(img, 12+by, 29, WHITE)
        # Eyebrows
        self._hline(img, 10+by, 18, 22, self.hair_c())
        self._hline(img, 10+by, 27, 31, self.hair_c())

        # Mouth
        self._set(img, 16+by, 23, MOUTH); self._set(img, 16+by, 24, MOUTH)
        # Blush
        self._rect(img, 14+by, 16, 16+by, 18, sb())
        self._rect(img, 14+by, 30, 16+by, 32, sb())

        # Neck
        self._rect(img, 20+by, 19, 23+by, 28, s())
        self._set(img, 21+by, 18, ss()); self._set(img, 21+by, 29, ss())

        # Shoulders
        self._rect(img, 22+by, 15, 26+by, 33, s())

        # Torso
        self._rect(img, 26+by, 16, 44+by, 32, s())
        # Side shading
        self._rect(img, 24+by, 14, 44+by, 16, ss())
        self._rect(img, 24+by, 32, 44+by, 34, ss())

        # BREASTS
        if self.breast > 0:
            bsz = self.breast  # 1-6
            bw = 3 + bsz // 2
            bh = 3 + bsz // 2
            lx = 19 - bsz // 3
            rx = 25 + bsz // 3 - bw
            ly = 28 + by

            # Left
            self._rect(img, ly, lx, ly+bh-1, lx+bw-1, s())
            self._rect(img, ly+1, lx+bw, ly+bh-2, lx+bw+1, ss())
            # Right
            self._rect(img, ly, rx, ly+bh-1, rx+bw-1, s())
            self._rect(img, ly+1, rx-2, ly+bh-2, rx-1, ss())
            # Cleavage
            self._rect(img, ly+1, lx+bw-1, ly+bh-2, rx+1, ss())

            # Nipples
            ny = ly + bh // 2
            nx_l = lx + bw // 2
            nx_r = rx + bw // 2
            if bsz >= 3:
                self._set(img, ny, nx_l, AREOLAE[self.skin % 6])
                self._set(img, ny, nx_l+1, NIPPLES[self.skin % 6])
                self._set(img, ny+1, nx_l, AREOLAE[self.skin % 6])
                self._set(img, ny, nx_r, AREOLAE[self.skin % 6])
                self._set(img, ny, nx_r-1, NIPPLES[self.skin % 6])
                self._set(img, ny+1, nx_r, AREOLAE[self.skin % 6])
            else:
                self._set(img, ny, nx_l, NIPPLES[self.skin % 6])
                self._set(img, ny, nx_r, NIPPLES[self.skin % 6])

            # Underboob
            if bsz >= 5:
                self._hline(img, ly+bh, lx, lx+bw-1, sd())
                self._hline(img, ly+bh, rx, rx+bw-1, sd())

        # PUBIC AREA
        pubic = self.hair_c()
        # Triangle
        self._rect(img, 48+by, 20, 49+by, 27, pubic)
        self._rect(img, 49+by, 19, 50+by, 28, pubic)
        self._rect(img, 50+by, 19, 55+by, 29, pubic)
        self._rect(img, 55+by, 20, 57+by, 28, pubic)
        # Labia slit
        self._vline(img, 23, 51+by, 57+by, sd())
        self._vline(img, 24, 51+by, 57+by, sd())

        # Belly / Waist
        self._rect(img, 44+by, 18, 54+by, 30, s())
        self._set(img, 48+by, 23, sd()); self._set(img, 48+by, 24, sd())

        # LEGS
        ll = 18 + leg_off
        lr = 26 - leg_off
        self._rect(img, 56+by, ll, 80+by, ll+5, s())
        self._rect(img, 56+by, lr, 80+by, lr+5, s())
        # Inner thigh
        self._rect(img, 56+by, ll, 70+by, ll+1, ss())
        self._rect(img, 56+by, lr+4, 70+by, lr+5, ss())
        # Crotch
        self._rect(img, 54+by, 22, 58+by, 25, sd())

        # FEET
        if ft == 'stand':
            self._rect(img, 80+by, 17, 90+by, 23, s())
            self._rect(img, 80+by, 25, 90+by, 31, s())
            # Sole shadow
            self._hline(img, 90+by, 17, 31, sd())
        elif ft == 'walk1':
            self._rect(img, 80+by, 16, 90+by, 21, s())
            self._rect(img, 80+by, 28, 90+by, 33, s())
            self._hline(img, 90+by, 16, 33, sd())
        elif ft == 'walk2':
            self._rect(img, 80+by, 15, 90+by, 20, s())
            self._rect(img, 80+by, 27, 90+by, 32, s())
            self._hline(img, 90+by, 15, 32, sd())

        # ARMS
        arm_fwd = arm_swing
        arm_back = -arm_swing
        # Left arm
        for y in range(24+by, 46+by):
            ax = 11 + (arm_fwd if y > 30+by else 0)
            self._set(img, y, ax, s()); self._set(img, y, ax+1, s())
            self._set(img, y, ax-1, ss())
        # Right arm
        for y in range(24+by, 46+by):
            ax = 35 + (arm_back if y > 30+by else 0)
            self._set(img, y, ax, s()); self._set(img, y, ax-1, s())
            self._set(img, y, ax+1, ss())
        # Hands
        self._set(img, 46+by, 11+arm_fwd, s()); self._set(img, 46+by, 12+arm_fwd, s())
        self._set(img, 46+by, 35+arm_back, s()); self._set(img, 46+by, 36+arm_back, s())

        return self._outline(img)

    def build_back(self, ft='stand'):
        """Back view nude body."""
        img = self._blank()
        s = self.sk; ss = lambda: self.sk(1); sd = lambda: self.sk(2)

        leg_off = 0; body_bob = 0; arm_swing = 0
        if ft == 'walk1': leg_off = 2; body_bob = -1; arm_swing = 1
        elif ft == 'walk2': leg_off = -2; body_bob = -1; arm_swing = -1
        by = body_bob

        # Head back
        self._rect(img, 4+by, 16, 19+by, 32, s())
        # Hair back (full cover)
        self._rect(img, 2+by, 14, 21+by, 34, self.hair_c())
        self._rect(img, 18+by, 16, 35+by, 32, self.hair_c())
        # Hair highlights
        self._rect(img, 4+by, 20, 18+by, 28, self.hair_hl())

        # Neck
        self._rect(img, 20+by, 20, 23+by, 28, s())

        # Back
        self._rect(img, 22+by, 16, 44+by, 32, s())
        self._rect(img, 24+by, 14, 44+by, 16, ss())
        self._rect(img, 24+by, 32, 44+by, 34, ss())

        # Spine
        self._vline(img, 23, 28+by, 48+by, sd())
        self._vline(img, 24, 28+by, 48+by, sd())

        # Buttocks (back view)
        self._rect(img, 50+by, 18, 64+by, 30, s())
        self._rect(img, 52+by, 22, 62+by, 24, sd())  # Cleft
        # Cheek shading
        self._rect(img, 50+by, 16, 62+by, 18, ss())
        self._rect(img, 50+by, 30, 62+by, 32, ss())

        # Waist
        self._rect(img, 44+by, 18, 56+by, 30, s())

        # Legs
        ll = 18 + leg_off; lr = 26 - leg_off
        self._rect(img, 56+by, ll, 80+by, ll+5, s())
        self._rect(img, 56+by, lr, 80+by, lr+5, s())

        # Feet
        if ft == 'stand':
            self._rect(img, 80+by, 17, 90+by, 23, s())
            self._rect(img, 80+by, 25, 90+by, 31, s())
        elif ft == 'walk1':
            self._rect(img, 80+by, 16, 90+by, 21, s())
            self._rect(img, 80+by, 28, 90+by, 33, s())
        elif ft == 'walk2':
            self._rect(img, 80+by, 15, 90+by, 20, s())
            self._rect(img, 80+by, 27, 90+by, 32, s())

        # Arms
        af = arm_swing; ab = -arm_swing
        for y in range(24+by, 46+by):
            ax = 11 + (af if y > 30+by else 0)
            self._set(img, y, ax, s()); self._set(img, y, ax+1, s())
        for y in range(24+by, 46+by):
            ax = 35 + (ab if y > 30+by else 0)
            self._set(img, y, ax, s()); self._set(img, y, ax-1, s())

        return self._outline(img)

    def build_side(self, direction, ft='stand'):
        """Side view body."""
        img = self._blank()
        s = self.sk; ss = lambda: self.sk(1); sd = lambda: self.sk(2)
        mir = direction == 'right'

        leg_off = 0; body_bob = 0; arm_swing = 0
        if ft == 'walk1': leg_off = 3; body_bob = -1
        elif ft == 'walk2': leg_off = -1; body_bob = -1
        by = body_bob

        # Head side
        if mir:
            self._rect(img, 4+by, 18, 19+by, 32, s())
            self._set(img, 12+by, 33, s())  # Nose
            self._set(img, 13+by, 33, s())
        else:
            self._rect(img, 4+by, 16, 19+by, 30, s())
            self._set(img, 12+by, 15, s())
            self._set(img, 13+by, 15, s())

        # Hair (side view)
        self._rect(img, 2+by, 13, 21+by, 35, self.hair_c())
        self._rect(img, 18+by, 15, 34+by, 33, self.hair_c())
        self._rect(img, 4+by, 19, 18+by, 29, self.hair_hl())

        # Eye
        if mir:
            self._set(img, 12+by, 24, EYE); self._set(img, 12+by, 25, WHITE)
        else:
            self._set(img, 12+by, 22, WHITE); self._set(img, 12+by, 23, EYE)

        # Neck
        self._rect(img, 20+by, 20, 23+by, 28, s())

        # Torso
        self._rect(img, 22+by, 18, 44+by, 30, s())
        self._rect(img, 24+by, 16, 44+by, 18, ss())
        self._rect(img, 24+by, 30, 44+by, 32, ss())

        # Side breast
        if self.breast > 0:
            bx = 21 if mir else 26
            bs = 3 + self.breast // 2 + self.breast % 2
            self._rect(img, 28+by, bx, 28+bs+by, bx+3, s())
            ny = 30 + by + bs//2
            self._set(img, ny, bx+1, NIPPLES[self.skin % 6])
            self._set(img, ny, bx+2, NIPPLES[self.skin % 6])

        # Waist
        self._rect(img, 44+by, 20, 54+by, 28, s())

        # Legs
        lx = 20 + leg_off
        self._rect(img, 54+by, lx, 80+by, lx+7, s())

        # Foot
        fx = 19 + leg_off
        self._rect(img, 80+by, fx, 90+by, fx+9, s())

        # Arms
        if mir:
            self._rect(img, 24+by, 30, 46+by, 32, s())
            self._set(img, 46+by, 30, s()); self._set(img, 46+by, 31, s())
        else:
            self._rect(img, 24+by, 16, 46+by, 18, s())
            self._set(img, 46+by, 16, s()); self._set(img, 46+by, 17, s())

        return self._outline(img)

    def render_frame(self, dir_name, ft):
        if dir_name == 'down':
            return self._to_pil(self.build_front(ft))
        elif dir_name == 'up':
            return self._to_pil(self.build_back(ft))
        elif dir_name == 'left':
            return self._to_pil(self.build_side('left', ft))
        elif dir_name == 'right':
            return self._to_pil(self.build_side('right', ft))

    def render_sheet(self):
        sheet = Image.new('RGBA', (self.W * 3, self.H * 4), TRANS)
        dirs = ['down', 'left', 'right', 'up']
        fts = ['stand', 'walk1', 'walk2']

        for di, dn in enumerate(dirs):
            for fi, ft in enumerate(fts):
                frame = self.render_frame(dn, ft)
                x = fi * self.W
                y = di * self.H
                sheet.paste(frame, (x, y), frame)

        return sheet


def make_sheet(skin=0, hair=0, cloth=0, breast=3, body=0, name=None):
    if name is None:
        name = f"nsfw_s{skin}_h{hair}_c{cloth}_b{breast}.png"
    c = Char2(skin, hair, cloth, breast, body)
    sheet = c.render_sheet()
    path = os.path.join(OUT, name)
    sheet.save(path)
    return path, sheet


def make_gif(sheet_path, name=None, scale=4):
    sheet = Image.open(sheet_path).convert('RGBA')
    fw, fh = 48, 112
    # Extract frames in display order: cycle through directions, 3 frames each
    gif = []
    for row in range(4):
        for col in range(3):
            f = sheet.crop((col*fw, row*fh, (col+1)*fw, (row+1)*fh))
            if scale != 1:
                f = f.resize((fw*scale, fh*scale), Image.NEAREST)
            gif.append(f)

    # Two full cycles
    gif = gif * 2

    if name is None:
        name = os.path.splitext(sheet_path)[0] + '.gif'

    gif[0].save(name, save_all=True, append_images=gif[1:],
                duration=[200]*len(gif), loop=0, transparency=0, disposal=2)
    return name


if __name__ == '__main__':
    print("=== NSFW RPG Maker Spritesheet Generator v2 ===")
    results = []

    configs = [
        # (skin, hair, cloth, breast, body, label)
        (0, 0, 0, 3, 0, "default"),
        (0, 1, 1, 5, 0, "blonde_large"),
        (1, 2, 2, 6, 0, "pale_redhead_huge"),
        (2, 5, 3, 0, 0, "porcelain_blonde_flat"),
        (4, 3, 4, 4, 0, "tan_purple_med"),
        (3, 6, 5, 6, 1, "dark_black_huge"),
        (0, 8, 8, 2, 1, "fair_pink_small"),
        (5, 7, 9, 5, 0, "wheat_teal_large"),
    ]

    for skin, hair, cloth, breast, body, label in configs:
        fn = f"nsfw_{label}.png"
        p, s = make_sheet(skin, hair, cloth, breast, body, fn)
        g = make_gif(p, f"nsfw_{label}.gif")
        results.append((label, p, g))
        print(f"  {label}: sheet={p}, gif={g}")

    print(f"\nGenerated {len(results)} character variations")
    print("48x112 per frame | 144x448 per sheet | 3 cols x 4 rows")
    print("GIF = 3 frames/direction cycling, 200ms each, 2 loops")
