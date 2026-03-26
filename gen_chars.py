#!/usr/bin/env python3
"""
森林手账 - 角色精灵生成器
5职业 × 8方向 × 4状态 = 160帧
"""
from PIL import Image, ImageDraw
import random
import os

# 角色配色方案
CLASS_COLORS = {
    'herbalist': {  # 草药师 - 绿色系
        'primary': (76, 140, 70),
        'secondary': (120, 180, 100),
        'skin': (255, 200, 160),
        'hair': (60, 90, 50),
        'cloak': (40, 100, 60)
    },
    'hunter': {  # 猎人 - 棕色系
        'primary': (139, 90, 60),
        'secondary': (180, 120, 80),
        'skin': (255, 210, 170),
        'hair': (80, 50, 30),
        'cloak': (100, 70, 50)
    },
    'warrior': {  # 战士 - 红色系
        'primary': (180, 60, 60),
        'secondary': (220, 100, 80),
        'skin': (255, 190, 150),
        'hair': (40, 30, 20),
        'cloak': (140, 40, 40)
    },
    'poet': {  # 诗人 - 紫色系
        'primary': (140, 80, 160),
        'secondary': (180, 120, 200),
        'skin': (255, 220, 180),
        'hair': (60, 40, 80),
        'cloak': (100, 60, 120)
    },
    'elf': {  # 精灵 - 蓝色系
        'primary': (60, 120, 180),
        'secondary': (100, 160, 220),
        'skin': (255, 240, 220),
        'hair': (200, 220, 100),
        'cloak': (40, 80, 140)
    }
}

# 方向定义 (从上往下看8个方向)
DIRECTIONS = ['N', 'NE', 'E', 'SE', 'S', 'SW', 'W', 'NW']

# 状态
STATES = ['idle', 'walk', 'attack', 'hurt']

def draw_pixel_char(clazz, direction, state, size=32):
    """绘制一个像素角色"""
    colors = CLASS_COLORS[clazz]
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size // 2, size // 2
    
    # 根据方向调整身体比例
    dir_idx = DIRECTIONS.index(direction)
    is_side = dir_idx in [1, 2, 3, 5, 6, 7]  # 非正面/背面
    is_back = dir_idx in [0, 1, 7]  # 背面
    
    # 基础尺寸
    body_w = 10 if is_side else 12
    body_h = 14
    
    # 身体
    bx = cx - body_w // 2
    by = cy - 2
    
    # 斗篷/衣服
    draw.rectangle([bx, by, bx + body_w, by + body_h], fill=colors['cloak'])
    
    # 头部
    head_size = 8
    hx = cx - head_size // 2
    hy = by - head_size + 2
    draw.rectangle([hx, hy, hx + head_size, hy + head_size], fill=colors['skin'])
    
    # 头发
    hair_offset = 2 if is_back else 0
    draw.rectangle([hx - 1, hy - 2 + hair_offset, hx + head_size + 1, hy + 2 + hair_offset], 
                   fill=colors['hair'])
    
    # 眼睛
    eye_y = hy + 4
    if is_side:
        eye_x = hx + (4 if dir_idx in [2, 3, 5] else 2)
        draw.rectangle([eye_x, eye_y, eye_x + 1, eye_y + 1], fill=(20, 20, 20))
    else:
        draw.rectangle([hx + 2, eye_y, hx + 3, eye_y + 1], fill=(20, 20, 20))
        draw.rectangle([hx + 5, eye_y, hx + 6, eye_y + 1], fill=(20, 20, 20))
    
    # 手臂
    arm_y = by + 4
    if is_side:
        # 侧面 - 一只手
        if dir_idx in [2, 3]:
            draw.rectangle([bx + body_w, arm_y, bx + body_w + 3, arm_y + 6], fill=colors['skin'])
        else:
            draw.rectangle([bx - 3, arm_y, bx, arm_y + 6], fill=colors['skin'])
    else:
        # 正面/背面 - 双手
        draw.rectangle([bx - 3, arm_y, bx, arm_y + 6], fill=colors['skin'])
        draw.rectangle([bx + body_w, arm_y, bx + body_w + 3, arm_y + 6], fill=colors['skin'])
    
    # 腿
    leg_y = by + body_h
    leg_h = 8
    if is_side:
        # 侧面 - 一条腿
        lx = cx - 2
        draw.rectangle([lx, leg_y, lx + 4, leg_y + leg_h], fill=colors['primary'])
    else:
        # 双腿
        draw.rectangle([bx + 1, leg_y, bx + 4, leg_y + leg_h], fill=colors['primary'])
        draw.rectangle([bx + 6, leg_y, bx + 9, leg_y + leg_h], fill=colors['primary'])
    
    # 武器（根据职业）
    if clazz == 'herbalist':
        # 魔杖
        wx = bx - 5 if dir_idx not in [2, 3] else bx + body_w + 2
        draw.rectangle([wx, by, wx + 2, by + 12], fill=(100, 80, 60))
        # 顶部发光
        draw.rectangle([wx, by - 3, wx + 2, by], fill=(100, 255, 150))
    elif clazz == 'hunter':
        # 弓
        wx = bx - 4 if dir_idx not in [2, 3] else bx + body_w + 1
        draw.rectangle([wx, by + 2, wx + 4, by + 14], outline=colors['secondary'], width=1)
    elif clazz == 'warrior':
        # 剑
        wx = bx - 6 if dir_idx not in [2, 3] else bx + body_w + 3
        draw.rectangle([wx, by - 2, wx + 2, by + 14], fill=(180, 180, 200))  # 剑身
        draw.rectangle([wx - 1, by + 6, wx + 3, by + 8], fill=(150, 100, 50))  # 剑柄
    elif clazz == 'poet':
        # 书
        wx = bx - 4 if dir_idx not in [2, 3] else bx + body_w + 1
        draw.rectangle([wx, by + 4, wx + 3, by + 10], fill=(80, 60, 100))
    elif clazz == 'elf':
        # 法杖
        wx = bx - 5 if dir_idx not in [2, 3] else bx + body_w + 2
        draw.rectangle([wx, by - 4, wx + 2, by + 12], fill=(150, 200, 255))
        draw.ellipse([wx - 2, by - 6, wx + 4, by - 2], fill=(150, 200, 255))
    
    # 状态效果
    if state == 'walk':
        # 行走动画 - 腿部偏移
        pass  # 已在腿部分体现
    elif state == 'attack':
        # 攻击姿态 - 手抬起
        pass
    elif state == 'hurt':
        # 受伤 - 红色调
        for y in range(size):
            for x in range(size):
                p = img.getpixel((x, y))
                if p[3] > 0:
                    r = min(255, p[0] + 60)
                    img.putpixel((x, y), (r, p[1] // 2, p[2] // 2, p[3]))
    
    return img

def generate_all_chars(output_dir):
    """生成所有角色精灵"""
    os.makedirs(output_dir, exist_ok=True)
    
    classes = list(CLASS_COLORS.keys())
    
    for clazz in classes:
        class_dir = os.path.join(output_dir, clazz)
        os.makedirs(class_dir, exist_ok=True)
        
        for direction in DIRECTIONS:
            for state in STATES:
                img = draw_pixel_char(clazz, direction, state)
                
                # 缩放到2倍 (32x32 -> 64x64)
                img = img.resize((64, 64), Image.NEAREST)
                
                filename = f"{clazz}_{direction}_{state}.png"
                img.save(os.path.join(class_dir, filename))
    
    print(f"生成 {len(classes) * len(DIRECTIONS) * len(STATES)} 个角色精灵")

if __name__ == '__main__':
    output = '/root/.openclaw/workspace/games/森林手账/Unity_Full/assets/chars'
    generate_all_chars(output)
    print("角色生成完成!")
