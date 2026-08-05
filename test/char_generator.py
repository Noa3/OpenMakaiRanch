"""
Layered NSFW RPG Maker-style pixel art character spritesheet generator.
Matches eraMakaiRanch portrait dimensions: 48x112 per frame.
Output: 3x4 grid (3 walk frames x 4 directions) = 144x448 spritesheet + GIF preview.
"""

from PIL import Image
import os, json

OUT_DIR = os.path.dirname(os.path.abspath(__file__))

C = {
    # Skin tones (from PortraitLayerCatalog: fair, light, pale, deepbrown, rosylight, tan)
    'skin': [(255, 210, 180), (245, 195, 165), (255, 225, 200), (160, 100, 60), (255, 200, 170), (210, 160, 110)],
    'skin_shadow': [(220, 175, 145), (210, 160, 130), (220, 190, 165), (125, 70, 35), (220, 165, 135), (175, 125, 80)],
    'skin_dark': [(180, 140, 115), (170, 125, 100), (180, 155, 130), (95, 50, 25), (180, 130, 105), (140, 95, 60)],
    # Nipple colors (match skin but pinker)
    'nipple': [(220, 160, 150), (210, 150, 140), (225, 170, 160), (140, 75, 60), (215, 155, 145), (180, 115, 100)],
    'areola': [(200, 140, 130), (190, 130, 120), (205, 150, 140), (120, 60, 50), (195, 135, 125), (160, 100, 85)],
    # Hair colors
    'hair': [(60, 50, 45), (180, 140, 80), (200, 80, 60), (140, 60, 140), (60, 120, 180), (220, 200, 170), (30, 30, 30), (80, 180, 140), (255, 160, 180), (160, 100, 60), (200, 180, 100), (100, 60, 180)],
    'hair_shadow': [(40, 32, 28), (140, 105, 55), (160, 55, 40), (100, 40, 100), (40, 85, 140), (180, 160, 130), (18, 18, 18), (55, 140, 105), (210, 120, 140), (120, 70, 40), (160, 140, 70), (70, 40, 140)],
    'hair_highlight': [(90, 75, 65), (210, 170, 105), (230, 110, 85), (170, 85, 170), (85, 145, 210), (240, 230, 200), (55, 55, 55), (110, 210, 170), (255, 200, 215), (190, 130, 85), (230, 210, 130), (130, 90, 210)],
    # Cloth colors (50+ variants)
    'cloth': [(50, 120, 200), (200, 60, 60), (60, 180, 100), (180, 60, 140), (200, 160, 60), (60, 60, 60), (180, 120, 200), (60, 180, 180), (200, 140, 60), (100, 60, 180),
              (50, 150, 180), (180, 80, 80), (80, 140, 80), (160, 80, 120), (180, 160, 80), (80, 80, 80), (160, 100, 180), (80, 160, 160), (180, 120, 80), (120, 80, 160),
              (70, 130, 160), (160, 100, 100), (100, 120, 100), (140, 100, 120), (160, 140, 100), (100, 100, 100), (140, 120, 160), (100, 140, 140), (160, 130, 100), (140, 100, 140),
              (40, 100, 160), (180, 80, 120), (60, 120, 80), (160, 60, 100), (180, 140, 80), (60, 60, 80), (140, 80, 160), (60, 140, 120), (180, 120, 80), (100, 60, 120)],
    'cloth_shadow': [(35, 90, 160), (160, 40, 40), (40, 140, 75), (140, 40, 110), (160, 125, 40), (40, 40, 40), (140, 90, 160), (40, 140, 140), (160, 105, 40), (75, 40, 140)],
    'cloth_highlight': [(80, 150, 230), (230, 90, 90), (90, 210, 130), (210, 90, 170), (230, 190, 90), (90, 90, 90), (210, 150, 230), (90, 210, 210), (230, 170, 90), (130, 90, 210)],
    # Pubic hair colors (match hair)
    'pubic': [(50, 40, 35), (150, 110, 60), (170, 60, 45), (110, 45, 110), (50, 95, 150), (195, 175, 145), (25, 25, 25), (65, 155, 115), (230, 135, 155), (130, 75, 45), (175, 155, 75), (80, 45, 155)],
    # Misc
    'eye_white': (255, 255, 255),
    'eye': (40, 30, 20),
    'mouth': (180, 80, 80),
    'mouth_open': (200, 100, 100),
    'blush': (220, 140, 140, 100),
    'outline': (25, 20, 15),
    'transparent': (0, 0, 0, 0),
    'bg_grid': (40, 40, 50, 60),
}


class CharBuilder:
    W, H = 48, 112  # Per frame
    COLS, ROWS = 3, 4
    SHEET_W, SHEET_H = W * 3, H * 4

    def __init__(self, skin=0, hair=0, cloth=0, breast_size=3, body_type=0):
        self.skin = skin
        self.hair = hair
        self.cloth = cloth
        self.breast_size = min(breast_size, 6)
        self.body_type = body_type  # 0=big, 1=small
        self.colors = {k: C[k] for k in C}
        self.W = self.W
        self.H = self.H
        self.SHEET_W = self.SHEET_W
        self.SHEET_H = self.SHEET_H

    def _cp(self, name):
        """Get color by name, indexing into arrays if needed."""
        c = C[name]
        if name == 'skin' or name == 'skin_shadow' or name == 'skin_dark' or name == 'nipple' or name == 'areola':
            return c[self.skin % len(c)]
        if name == 'hair' or name == 'hair_shadow' or name == 'hair_highlight' or name == 'pubic':
            return c[self.hair % len(c)]
        if name == 'cloth':
            return c[self.cloth % len(c)]
        if name == 'cloth_shadow':
            return C['cloth_shadow'][self.cloth % len(C['cloth_shadow'])]
        if name == 'cloth_highlight':
            return C['cloth_highlight'][self.cloth % len(C['cloth_highlight'])]
        return c

    def _body_base(self, frame_type='stand'):
        """Generate the base nude body. frame_type: stand, walk1, walk2"""
        c = self._cp
        skin = lambda: c('skin')
        s_shadow = lambda: c('skin_shadow')
        s_dark = lambda: c('skin_dark')
        trans = C['transparent']
        outline = C['outline']

        img = [[trans] * self.W for _ in range(self.H)]

        # Leg offset for walk
        leg_off = 0
        if frame_type == 'walk1':
            leg_off = 2
        elif frame_type == 'walk2':
            leg_off = -2

        # === HEAD (face front/back) ===
        # Head base
        for y in range(4, 20):
            for x in range(14, 34):
                img[y][x] = skin()

        # Hair base (back)
        for y in range(2, 8):
            for x in range(14, 34):
                img[y][x] = c('hair')
        # Hair sides
        for y in range(6, 18):
            for x in range(12, 15):
                img[y][x] = c('hair')
            for x in range(34, 37):
                img[y][x] = c('hair')
        # Hair top
        for y in range(4, 8):
            for x in range(12, 36):
                img[y][x] = c('hair')
        # Hair bangs (front)
        for y in range(6, 12):
            for x in range(16, 32):
                img[y][x] = c('hair')
        # Hair bang tips
        for y in range(12, 16):
            for x in range(18, 30):
                img[y][x] = c('hair_shadow')

        # Eyes
        img[12][19] = C['eye']
        img[12][20] = C['eye_white']
        img[12][28] = C['eye']
        img[12][29] = C['eye_white']
        # Eyebrows
        for x in range(18, 22):
            img[10][x] = c('hair')
        for x in range(27, 31):
            img[10][x] = c('hair')

        # Mouth
        img[16][23] = c('mouth')
        img[16][24] = c('mouth')

        # === NECK ===
        for y in range(20, 24):
            for x in range(20, 28):
                img[y][x] = skin()
            for x in range(18, 20):
                img[y][x] = s_shadow()
            for x in range(28, 30):
                img[y][x] = s_shadow()

        # === TORSO ===
        # Shoulders
        for y in range(22, 28):
            for x in range(14, 34):
                img[y][x] = skin()
        # Upper body
        for y in range(26, 44):
            for x in range(16, 32):
                img[y][x] = skin()
        # Side shading
        for y in range(24, 44):
            for x in range(14, 17):
                img[y][x] = s_shadow()
            for x in range(32, 35):
                img[y][x] = s_shadow()

        # === BREASTS ===
        self._add_breasts(img, 'front')

        # === BELLY/WAIST ===
        for y in range(44, 54):
            for x in range(18, 30):
                img[y][x] = skin()
            for x in range(16, 18):
                img[y][x] = s_shadow()
            for x in range(30, 32):
                img[y][x] = s_shadow()
        # Belly button
        img[48][23] = s_dark()
        img[48][24] = s_dark()

        # === PUBIC AREA (NSFW) ===
        self._add_pubic(img, 'front')

        # === LEGS ===
        leg_l = 18 + leg_off
        leg_r = 26 - leg_off
        for y in range(56, 82):
            for x in range(leg_l, leg_l + 6):
                img[y][x] = skin()
            for x in range(leg_r, leg_r + 6):
                img[y][x] = skin()
        # Inner thigh shading
        for y in range(56, 72):
            for x in range(leg_l, leg_l + 2):
                img[y][x] = s_shadow()
            for x in range(leg_r + 4, leg_r + 6):
                img[y][x] = s_shadow()
        # Crotch shadow
        for y in range(54, 60):
            for x in range(22, 26):
                img[y][x] = s_shadow()

        # === FEET ===
        if frame_type == 'stand':
            for y in range(82, 92):
                for x in range(17, 23):
                    img[y][x] = skin()
                for x in range(25, 31):
                    img[y][x] = skin()
        elif frame_type == 'walk1':
            for y in range(82, 92):
                for x in range(16, 22):
                    img[y][x] = skin()
                for x in range(28, 34):
                    img[y][x] = skin()
        elif frame_type == 'walk2':
            for y in range(82, 92):
                for x in range(15, 21):
                    img[y][x] = skin()
                for x in range(27, 33):
                    img[y][x] = skin()

        # === ARMS ===
        arm_swing = 0
        if frame_type == 'walk1':
            arm_swing = 2
        elif frame_type == 'walk2':
            arm_swing = -2
        # Left arm
        for y in range(24, 46):
            ax = 11 + (arm_swing if y > 30 else 0)
            img[y][ax] = skin()
            img[y][ax+1] = skin()
            img[y][ax-1] = s_shadow()
        # Right arm
        for y in range(24, 46):
            ax = 35 + (-arm_swing if y > 30 else 0)
            img[y][ax] = skin()
            img[y][ax-1] = skin()
            img[y][ax+1] = s_shadow()
        # Hands
        img[46][11 + arm_swing] = skin()
        img[46][12 + arm_swing] = skin()
        img[46][36 - arm_swing] = skin()
        img[46][35 - arm_swing] = skin()

        return img

    def _body_back(self, frame_type='stand'):
        """Generate back view body."""
        c = self._cp
        skin = lambda: c('skin')
        s_shadow = lambda: c('skin_shadow')
        s_dark = lambda: c('skin_dark')
        trans = C['transparent']

        img = [[trans] * self.W for _ in range(self.H)]

        leg_off = 0
        if frame_type == 'walk1':
            leg_off = 2
        elif frame_type == 'walk2':
            leg_off = -2

        # Head back
        for y in range(4, 20):
            for x in range(14, 34):
                img[y][x] = skin()

        # Hair back (full coverage)
        for y in range(2, 22):
            for x in range(12, 36):
                img[y][x] = c('hair')
        # Hair highlights
        for y in range(4, 16):
            for x in range(18, 30):
                img[y][x] = c('hair_highlight')
        # Long hair flowing down
        for y in range(18, 36):
            for x in range(14, 34):
                img[y][x] = c('hair')

        # Neck
        for y in range(20, 24):
            for x in range(20, 28):
                img[y][x] = skin()

        # Back/shoulders
        for y in range(22, 44):
            for x in range(16, 32):
                img[y][x] = skin()
        for y in range(24, 44):
            for x in range(14, 17):
                img[y][x] = s_shadow()
            for x in range(32, 35):
                img[y][x] = s_shadow()

        # Spine line
        for y in range(28, 48):
            img[y][23] = s_dark()
            img[y][24] = s_dark()

        # Back waist
        for y in range(44, 56):
            for x in range(18, 30):
                img[y][x] = skin()

        # Buttocks
        for y in range(52, 66):
            for x in range(18, 30):
                img[y][x] = skin()
        # Cleft
        for y in range(52, 64):
            img[y][22] = s_dark()
            img[y][23] = s_dark()

        # Legs
        leg_l = 18 + leg_off
        leg_r = 26 - leg_off
        for y in range(56, 82):
            for x in range(leg_l, leg_l + 6):
                img[y][x] = skin()
            for x in range(leg_r, leg_r + 6):
                img[y][x] = skin()

        # Feet
        if frame_type == 'stand':
            for y in range(82, 92):
                for x in range(17, 23):
                    img[y][x] = skin()
                for x in range(25, 31):
                    img[y][x] = skin()
        elif frame_type == 'walk1':
            for y in range(82, 92):
                for x in range(16, 22):
                    img[y][x] = skin()
                for x in range(28, 34):
                    img[y][x] = skin()
        elif frame_type == 'walk2':
            for y in range(82, 92):
                for x in range(15, 21):
                    img[y][x] = skin()
                for x in range(27, 33):
                    img[y][x] = skin()

        # Arms
        arm_swing = 0
        if frame_type == 'walk1':
            arm_swing = 2
        elif frame_type == 'walk2':
            arm_swing = -2
        for y in range(24, 46):
            img[y][11 + (arm_swing if y > 30 else 0)] = skin()
            img[y][12 + (arm_swing if y > 30 else 0)] = skin()
            img[y][35 + (-arm_swing if y > 30 else 0)] = skin()
            img[y][36 + (-arm_swing if y > 30 else 0)] = skin()

        return img

    def _body_side(self, direction, frame_type='stand'):
        """Generate side view body. direction: 'left' or 'right'"""
        c = self._cp
        skin = lambda: c('skin')
        s_shadow = lambda: c('skin_shadow')
        trans = C['transparent']
        outline = C['outline']

        img = [[trans] * self.W for _ in range(self.H)]
        mirror = direction == 'right'

        leg_off = 0
        if frame_type == 'walk1':
            leg_off = 3
        elif frame_type == 'walk2':
            leg_off = -1

        # Head side
        for y in range(4, 20):
            for x in range(16, 30):
                img[y][x] = skin()
        # Nose
        if mirror:
            img[12][30] = skin()
            img[13][30] = skin()
        else:
            img[12][16] = skin()
            img[13][16] = skin()

        # Hair
        for y in range(2, 22):
            for x in range(12, 36):
                img[y][x] = c('hair')
        for y in range(6, 12):
            if mirror:
                for x in range(16, 22):
                    img[y][x] = c('hair')
            else:
                for x in range(26, 32):
                    img[y][x] = c('hair')

        # Neck
        for y in range(20, 24):
            for x in range(20, 27):
                img[y][x] = skin()

        # Torso
        for y in range(22, 44):
            for x in range(18, 30):
                img[y][x] = skin()
        for y in range(24, 44):
            for x in range(16, 18):
                img[y][x] = s_shadow()
            for x in range(30, 32):
                img[y][x] = s_shadow()

        # Side breast (one visible from side)
        bx = 21 if mirror else 25
        bs = 3 + self.breast_size
        for y in range(28, 28 + bs):
            for x in range(bx, bx + 4):
                img[y][x] = skin()
        # Nipple
        ny = 30 + self.breast_size // 2
        img[ny][bx + 1 + (0 if mirror else 1)] = c('nipple')
        img[ny][bx + 2 + (0 if mirror else 1)] = c('nipple')

        # Waist
        for y in range(44, 54):
            for x in range(20, 28):
                img[y][x] = skin()

        # Legs
        leg_x = 20 + leg_off
        for y in range(54, 82):
            for x in range(leg_x, leg_x + 8):
                img[y][x] = skin()

        # Foot
        for y in range(82, 92):
            fx = 19 + leg_off
            for x in range(fx, fx + 10):
                img[y][x] = skin()

        # Arms
        if mirror:
            for y in range(24, 46):
                img[y][30] = skin()
                img[y][31] = skin()
            img[46][30] = skin()
            img[46][31] = skin()
        else:
            for y in range(24, 46):
                img[y][16] = skin()
                img[y][17] = skin()
            img[46][16] = skin()
            img[46][17] = skin()

        return img

    def _add_breasts(self, img, view='front'):
        """Add breast layer based on size."""
        c = self._cp
        skin = lambda: c('skin')
        nipple = lambda: c('nipple')
        areola = lambda: c('areola')
        s_shadow = lambda: c('skin_shadow')
        s_dark = lambda: c('skin_dark')

        if self.breast_size == 0:
            return  # Flat/minimal

        sz = self.breast_size
        # Breast mounds
        bw = 3 + sz // 2
        bh = 3 + sz // 2

        # Left breast
        lx = 19 - sz // 3
        ly = 28
        for y in range(ly, ly + bh):
            for x in range(lx, lx + bw):
                if 0 <= x < self.W and 0 <= y < self.H:
                    img[y][x] = skin()
        for y in range(ly + 1, ly + bh - 1):
            for x in range(lx + bw, lx + bw + 2):
                if 0 <= x < self.W and 0 <= y < self.H:
                    img[y][x] = s_shadow()

        # Right breast
        rx = 25 + sz // 3 - bw
        for y in range(ly, ly + bh):
            for x in range(rx, rx + bw):
                if 0 <= x < self.W and 0 <= y < self.H:
                    img[y][x] = skin()
        for y in range(ly + 1, ly + bh - 1):
            for x in range(rx - 2, rx):
                if 0 <= x < self.W and 0 <= y < self.H:
                    img[y][x] = s_shadow()

        # Cleavage
        for y in range(ly + 1, ly + bh - 1):
            for x in range(lx + bw - 1, rx + 1):
                if 0 <= x < self.W and 0 <= y < self.H:
                    img[y][x] = s_shadow()

        # Nipple left
        if sz >= 2:
            ny = ly + bh // 2
            if sz >= 3:
                img[ny][lx + bw // 2] = areola()
                img[ny][lx + bw // 2 + 1] = nipple()
                img[ny+1][lx + bw // 2] = areola()
            else:
                img[ny][lx + bw // 2] = nipple()

            ny2 = ly + bh // 2
            rx_n = rx + bw // 2
            if sz >= 3:
                img[ny2][rx_n] = areola()
                img[ny2][rx_n - 1] = nipple()
                img[ny2+1][rx_n] = areola()
            else:
                img[ny2][rx_n] = nipple()

        # Underboob shadow for larger
        if sz >= 4:
            for x in range(lx, lx + bw):
                img[ly + bh][x] = s_dark()
            for x in range(rx, rx + bw):
                img[ly + bh][x] = s_dark()

    def _add_pubic(self, img, view='front'):
        """Add pubic hair based on hair color."""
        c = self._cp
        pubic = lambda: c('pubic')
        s_shadow = lambda: c('skin_shadow')

        # Triangle patch
        for y in range(50, 56):
            for x in range(20, 28):
                img[y][x] = pubic()
        # Pubic hair shape
        for y in range(48, 50):
            for x in range(22, 26):
                img[y][x] = pubic()
        for y in range(50, 56):
            img[y][20] = s_shadow()
            img[y][27] = s_shadow()
        # Labia detail
        for y in range(52, 56):
            img[y][22] = s_shadow()
            img[y][25] = s_shadow()

    def _add_outline(self, img):
        """Add pixel outline around non-transparent areas."""
        result = [row[:] for row in img]
        trans = C['transparent']
        outline = C['outline']

        for y in range(self.H):
            for x in range(self.W):
                if img[y][x] != trans:
                    for dx, dy in [(0, -1), (0, 1), (-1, 0), (1, 0)]:
                        nx, ny = x + dx, y + dy
                        if 0 <= nx < self.W and 0 <= ny < self.H:
                            if img[ny][nx] == trans:
                                if result[y][x] != trans:
                                    result[y][x] = outline
                                    break
        return result

    def _to_pil(self, img):
        """Convert pixel buffer to PIL Image."""
        pil = Image.new('RGBA', (self.W, self.H))
        for y in range(self.H):
            for x in range(self.W):
                px = img[y][x]
                if len(px) == 3:
                    pil.putpixel((x, y), (*px, 255))
                else:
                    pil.putpixel((x, y), px)
        return pil

    def _make_frame(self, func, *args):
        """Generate a single frame as PIL Image."""
        img = func(*args)
        img = self._add_outline(img)
        return self._to_pil(img)

    def generate_sheet(self, cloth_enabled=True):
        """Generate the complete 3x4 spritesheet."""
        sheet = Image.new('RGBA', (self.SHEET_W, self.SHEET_H), C['transparent'])

        directions = [
            ('down', [self._body_base, self._body_base, self._body_base]),
            ('left', [self._body_side, self._body_side, self._body_side]),
            ('right', [self._body_side, self._body_side, self._body_side]),
            ('up', [self._body_back, self._body_back, self._body_back]),
        ]
        frame_types = ['stand', 'walk1', 'walk2']
        dir_args = {'down': 'front', 'left': 'left', 'right': 'right', 'up': 'back'}

        for di, (dname, funcs) in enumerate(directions):
            for fi, (func, ftype) in enumerate(zip(funcs, frame_types)):
                if dname == 'down':
                    img = func(ftype)
                elif dname == 'left' or dname == 'right':
                    img = func(dname, ftype)
                elif dname == 'up':
                    img = func(ftype)  # back
                pil_img = self._to_pil(self._add_outline(img))
                x = fi * self.W
                y = di * self.H
                sheet.paste(pil_img, (x, y), pil_img)

        return sheet

    def generate_layer_sheet(self, base_only=False, breast_overlay=True, pubic_overlay=True):
        """Generate separate layer images that can be composited."""
        # Generate base nude body + breast + pubic in one sheet
        return self.generate_sheet()


def build_spritesheet(skin=0, hair=0, cloth=0, breast_size=3, body_type=0,
                      output_name=None, cloth_enabled=True):
    """Build and save a complete spritesheet."""
    builder = CharBuilder(skin, hair, cloth, breast_size, body_type)
    sheet = builder.generate_sheet(cloth_enabled)

    if output_name is None:
        output_name = f"char_s{skin}_h{hair}_c{cloth}_b{breast_size}.png"

    out_path = os.path.join(OUT_DIR, output_name)
    sheet.save(out_path)
    return out_path, sheet


def create_gif_preview(sheet_path, output_name=None, scale=4):
    """Create a GIF preview from a spritesheet to verify animation."""
    sheet = Image.open(sheet_path).convert('RGBA')

    # Extract frames: 3 cols x 4 rows
    fw, fh = 48, 112
    frames = []
    for row in range(4):
        for col in range(3):
            frame = sheet.crop((col * fw, row * fh, (col + 1) * fw, (row + 1) * fh))
            if scale != 1:
                frame = frame.resize((fw * scale, fh * scale), Image.NEAREST)
            frames.append(frame)

    # Create GIF: loop through directions
    # Show each direction's 3 frames in sequence, 200ms per frame = 2.4s per direction
    gif_frames = []
    for row in range(4):
        for col in range(3):
            idx = row * 3 + col
            gif_frames.append(frames[idx])

    # Repeat 2 full cycles
    gif_frames = gif_frames * 2

    if output_name is None:
        output_name = os.path.splitext(sheet_path)[0] + '.gif'

    gif_frames[0].save(
        output_name,
        save_all=True,
        append_images=gif_frames[1:],
        duration=[200] * len(gif_frames),
        loop=0,
        transparency=0,
        disposal=2
    )
    return output_name


def generate_variations():
    """Generate multiple character variations with different layers."""
    results = []

    # Variation configs
    configs = [
        # (skin, hair, cloth, breast, body, name)
        (0, 0, 0, 3, 0, "default_fair_brunet"),
        (0, 1, 1, 3, 0, "maria_blonde"),
        (1, 2, 2, 4, 0, "pale_ginger"),
        (4, 3, 3, 5, 0, "tan_purple"),
        (3, 0, 4, 6, 0, "dark_brunet"),
        (5, 5, 5, 0, 0, "wheat_blonde_flat"),
        (0, 6, 6, 4, 1, "fair_black_small"),
        (2, 7, 7, 3, 1, "porcelain_teal"),
        (0, 8, 8, 5, 1, "fair_pink"),
        (4, 9, 9, 6, 0, "tan_brown_large"),
    ]

    for skin, hair, cloth, breast, body, name in configs[:4]:  # Generate first 4 for now
        path, sheet = build_spritesheet(
            skin=skin, hair=hair, cloth=cloth,
            breast_size=breast, body_type=body,
            output_name=f"var_{name}.png"
        )
        gif = create_gif_preview(path, output_name=f"var_{name}.gif")
        results.append((name, path, gif))
        print(f"  {name}: {path}")

    return results


if __name__ == '__main__':
    print("Generating NSFW RPG Maker character spritesheets...")

    # 1. Generate main default sheet
    path, sheet = build_spritesheet(
        skin=0, hair=0, cloth=0, breast_size=4,
        body_type=0, output_name="main_char.png"
    )
    gif = create_gif_preview(path, "main_char.gif")
    print(f"Default: {path}")

    # 2. Generate variant with large breasts
    path2, _ = build_spritesheet(
        skin=0, hair=1, cloth=1, breast_size=6,
        body_type=0, output_name="var_large_breasts.png"
    )
    gif2 = create_gif_preview(path2, "var_large_breasts.gif")
    print(f"Large: {path2}")

    # 3. Generate character with different body type + hair
    path3, _ = build_spritesheet(
        skin=2, hair=3, cloth=2, breast_size=3,
        body_type=1, output_name="var_small_alt.png"
    )
    gif3 = create_gif_preview(path3, "var_small_alt.gif")
    print(f"Small alt: {path3}")

    # 4. More variations
    path4, _ = build_spritesheet(
        skin=4, hair=8, cloth=3, breast_size=5,
        body_type=1, output_name="var_tan_pink.png"
    )
    gif4 = create_gif_preview(path4, "var_tan_pink.gif")
    print(f"Tan pink: {path4}")

    print(f"\nGenerated {4} character variations with GIF previews in {OUT_DIR}")
    print("Each sheet: 144x448 (3 frames x 4 directions)")
    print("Frame: 48x112 per cell")
