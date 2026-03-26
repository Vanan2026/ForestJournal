#!/usr/bin/env python3
"""
森林手账 - 怪物精灵生成器
8种怪物 × 4状态 = 32帧
"""
from PIL import Image, ImageDraw
import os

# 怪物定义
MONSTERS = {
    'shadow_wolf': {
        'name': '阴影狼',
        'color': (60, 50, 80),
        'secondary': (100, 80, 120),
        'eye_color': (255, 100, 100),
        'size': (24, 16),
        'type': 'beast'
    },
    'poison_spider': {
        'name': '毒蜘蛛',
        'color': (80, 60, 100),
        'secondary': (120, 80, 140),
        'eye_color': (100, 255, 100),
        'size': (20, 14),
        'type': 'beast'
    },
    'fog_beast': {
        'name': '黑雾兽',
        'color': (40, 30, 60),
        'secondary': (80, 60, 100),
        'eye_color': (200, 100, 255),
        'size': (28, 24),
        'type': 'demon'
    },
    'wild_boar': {
        'name': '野猪',
        'color': (100, 70, 50),
        'secondary': (140, 100, 70),
        'eye_color': (50, 50, 50),
        'size': (22, 16),
        'type': 'beast'
    },
    'bat': {
        'name': '蝙蝠',
        'color': (60, 50, 70),
        'secondary': (100, 80, 100),
        'eye_color': (255, 200, 0),
        'size': (16, 10),
        'type': 'beast'
    },
    'mist_spirit': {
        'name': '雾精灵',
        'color': (150, 180, 200),
        'secondary': (200, 220, 240),
        'eye_color': (100, 150, 200),
        'size': (16, 24),
        'type': 'spirit'
    },
    'mutant_tree': {
        'name': '变异树妖',
        'color': (80, 100, 60),
        'secondary': (120, 140, 80),
        'eye_color': (255, 100, 100),
        'size': (24, 28),
        'type': 'plant'
    },
    'swamp_frog': {
        'name': '沼泽蛙',
        'color': (60, 100, 60),
        'secondary': (80, 140, 80),
        'eye_color': (255, 255, 0),
        'size': (18, 12),
        'type': 'beast'
    }
}

STATES = ['idle', 'walk', 'attack', 'hurt']

def draw_shadow_wolf(mon, state, size=32):
    """绘制阴影狼"""
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size // 2, size // 2 + 4
    
    # 身体
    body_color = mon['color']
    if state == 'hurt':
        body_color = (150, 50, 50)
    
    # 狼的身体（椭圆形）
    bx, by = cx - 12, cy - 4
    draw.ellipse([bx, by, bx + 24, by + 12], fill=body_color)
    
    # 头部
    hx, hy = cx + 8, cy - 8
    draw.ellipse([hx, hy, hx + 10, hy + 10], fill=body_color)
    
    # 耳朵
    draw.polygon([(hx + 1, hy), (hx + 3, hy - 4), (hx + 5, hy)], fill=mon['secondary'])
    draw.polygon([(hx + 5, hy), (hx + 7, hy - 4), (hx + 9, hy)], fill=mon['secondary'])
    
    # 眼睛
    draw.ellipse([hx + 2, hy + 3, hx + 4, hy + 5], fill=mon['eye_color'])
    draw.ellipse([hx + 6, hy + 3, hx + 8, hy + 5], fill=mon['eye_color'])
    
    # 腿
    leg_color = mon['secondary']
    if state == 'walk':
        # 行走姿态
        draw.rectangle([cx - 8, cy + 6, cx - 4, cy + 12], fill=leg_color)
        draw.rectangle([cx + 2, cy + 4, cx + 6, cy + 10], fill=leg_color)
    else:
        draw.rectangle([cx - 8, cy + 6, cx - 4, cy + 12], fill=leg_color)
        draw.rectangle([cx + 2, cy + 6, cx + 6, cy + 12], fill=leg_color)
    
    # 尾巴
    if state == 'attack':
        # 攻击姿态 - 尾巴下垂
        draw.line([cx - 12, cy, cx - 16, cy + 6], fill=mon['secondary'], width=2)
    else:
        draw.line([cx - 12, cy, cx - 18, cy - 2], fill=mon['secondary'], width=2)
    
    return img

def draw_poison_spider(mon, state, size=32):
    """绘制毒蜘蛛"""
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size // 2, size // 2
    
    body_color = mon['color']
    if state == 'hurt':
        body_color = (150, 50, 50)
    
    # 身体（两个椭圆）
    # 头部
    draw.ellipse([cx - 4, cy - 6, cx + 4, cy - 2], fill=body_color)
    # 腹部
    draw.ellipse([cx - 8, cy - 2, cx + 8, cy + 8], fill=mon['secondary'])
    
    # 眼睛（8只）
    eye_positions = [(-3, -5), (-1, -5), (1, -5), (3, -5)]
    for ex, ey in eye_positions:
        draw.ellipse([cx + ex - 1, cy + ey - 1, cx + ex + 1, cy + ey + 1], fill=mon['eye_color'])
    
    # 腿（8条）
    leg_starts = [-4, -2, 2, 4]
    for i, ls in enumerate(leg_starts):
        # 左边
        lx = cx - 8
        ly = cy + ls
        if state == 'walk':
            offset = (i % 2) * 2
            draw.line([lx, ly, lx - 6, ly - 4 + offset], fill=body_color, width=1)
            draw.line([lx - 6, ly - 4 + offset, lx - 10, ly - 2 + offset], fill=body_color, width=1)
        else:
            draw.line([lx, ly, lx - 8, ly - 4], fill=body_color, width=1)
            draw.line([lx - 8, ly - 4, lx - 12, ly - 2], fill=body_color, width=1)
        # 右边
        lx = cx + 8
        draw.line([lx, ly, lx + 8, ly - 4], fill=body_color, width=1)
        draw.line([lx + 8, ly - 4, lx + 12, ly - 2], fill=body_color, width=1)
    
    return img

def draw_fog_beast(mon, state, size=32):
    """绘制黑雾兽"""
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size // 2, size // 2
    
    body_color = mon['color']
    if state == 'hurt':
        body_color = (100, 50, 80)
    
    # 雾状身体
    for i in range(5):
        offset = i * 2
        alpha = 200 - i * 30
        color = (*body_color[:3], alpha)
        draw.ellipse([cx - 14 + offset, cy - 10 + offset, 
                      cx + 14 - offset, cy + 10 - offset], 
                     fill=color)
    
    # 核心
    draw.ellipse([cx - 6, cy - 6, cx + 6, cy + 6], fill=mon['secondary'])
    
    # 眼睛
    draw.ellipse([cx - 5, cy - 3, cx - 2, cy], fill=mon['eye_color'])
    draw.ellipse([cx + 2, cy - 3, cx + 5, cy], fill=mon['eye_color'])
    
    # 角/触须
    draw.line([cx - 8, cy - 10, cx - 10, cy - 16], fill=mon['secondary'], width=2)
    draw.line([cx + 8, cy - 10, cx + 10, cy - 16], fill=mon['secondary'], width=2)
    
    return img

def draw_mist_spirit(mon, state, size=32):
    """绘制雾精灵"""
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size // 2, size // 2
    
    body_color = mon['color']
    if state == 'hurt':
        body_color = (100, 100, 120)
    
    # 透明身体
    for i in range(4):
        offset = i * 2
        alpha = 150 - i * 30
        color = (*body_color[:3], alpha)
        draw.ellipse([cx - 10 + offset, cy - 12 + offset,
                      cx + 10 - offset, cy + 8 - offset],
                     fill=color)
    
    # 眼睛
    draw.ellipse([cx - 5, cy - 4, cx - 2, cy - 1], fill=mon['eye_color'])
    draw.ellipse([cx + 2, cy - 4, cx + 5, cy - 1], fill=mon['eye_color'])
    
    # 尾部
    for i in range(3):
        ty = cy + 8 + i * 2
        alpha = 100 - i * 30
        draw.line([cx - 4 + i * 4, ty, cx - 2 + i * 4, ty + 4], 
                  fill=(*body_color[:3], alpha), width=2)
    
    return img

def draw_mutant_tree(mon, state, size=32):
    """绘制变异树妖"""
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size // 2, size // 2
    
    body_color = mon['color']
    if state == 'hurt':
        body_color = (80, 50, 50)
    
    # 树干身体
    draw.rectangle([cx - 6, cy - 8, cx + 6, cy + 12], fill=mon['secondary'])
    
    # 树冠
    draw.ellipse([cx - 12, cy - 16, cx + 12, cy - 4], fill=body_color)
    
    # 眼睛（嵌在树皮里）
    draw.ellipse([cx - 4, cy - 6, cx - 1, cy - 3], fill=mon['eye_color'])
    draw.ellipse([cx + 1, cy - 6, cx + 4, cy - 3], fill=mon['eye_color'])
    
    # 树枝（攻击时伸展）
    if state == 'attack':
        draw.line([cx - 6, cy - 4, cx - 14, cy - 10], fill=mon['secondary'], width=3)
        draw.line([cx + 6, cy - 4, cx + 14, cy - 10], fill=mon['secondary'], width=3)
    else:
        draw.line([cx - 6, cy - 4, cx - 10, cy - 8], fill=mon['secondary'], width=2)
        draw.line([cx + 6, cy - 4, cx + 10, cy - 8], fill=mon['secondary'], width=2)
    
    # 根
    draw.line([cx - 4, cy + 12, cx - 8, cy + 16], fill=mon['secondary'], width=2)
    draw.line([cx + 4, cy + 12, cx + 8, cy + 16], fill=mon['secondary'], width=2)
    
    return img

def draw_generic_beast(mon, state, size=32):
    """通用兽类怪物"""
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size // 2, size // 2 + 4
    
    body_color = mon['color']
    if state == 'hurt':
        body_color = (150, 50, 50)
    
    # 身体
    draw.ellipse([cx - 10, cy - 4, cx + 10, cy + 8], fill=body_color)
    
    # 头
    hx, hy = cx + 6, cy - 6
    draw.ellipse([hx, hy, hx + 8, hy + 8], fill=body_color)
    
    # 眼睛
    draw.ellipse([hx + 2, hy + 2, hx + 4, hy + 4], fill=mon['eye_color'])
    draw.ellipse([hx + 5, hy + 2, hx + 7, hy + 4], fill=mon['eye_color'])
    
    # 腿
    draw.rectangle([cx - 6, cy + 6, cx - 3, cy + 12], fill=mon['secondary'])
    draw.rectangle([cx + 2, cy + 6, cx + 5, cy + 12], fill=mon['secondary'])
    
    return img

DRAW_FUNCS = {
    'shadow_wolf': draw_shadow_wolf,
    'poison_spider': draw_poison_spider,
    'fog_beast': draw_fog_beast,
    'mutant_tree': draw_mutant_tree,
    'mist_spirit': draw_mist_spirit,
    'wild_boar': draw_generic_beast,
    'bat': draw_generic_beast,
    'swamp_frog': draw_generic_beast,
}

def generate_all_monsters(output_dir):
    """生成所有怪物精灵"""
    os.makedirs(output_dir, exist_ok=True)
    
    for mon_id, mon in MONSTERS.items():
        mon_dir = os.path.join(output_dir, mon_id)
        os.makedirs(mon_dir, exist_ok=True)
        
        draw_func = DRAW_FUNCS.get(mon_id, draw_generic_beast)
        
        for state in STATES:
            img = draw_func(mon, state, 32)
            img = img.resize((48, 48), Image.NEAREST)
            
            filename = f"{mon_id}_{state}.png"
            img.save(os.path.join(mon_dir, filename))
    
    print(f"生成 {len(MONSTERS) * len(STATES)} 个怪物精灵")

if __name__ == '__main__':
    output = '/root/.openclaw/workspace/games/森林手账/Unity_Full/assets/monsters'
    generate_all_monsters(output)
    print("怪物生成完成!")
