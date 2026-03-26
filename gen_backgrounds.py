#!/usr/bin/env python3
"""
森林手账 - 场景背景生成器
5个区域背景
"""
from PIL import Image, ImageDraw
import random
import os

# 区域定义
REGIONS = {
    'mist_forest': {
        'name': '迷雾森林',
        'sky': (140, 160, 180),  # 灰蓝色
        'ground': (60, 80, 50),   # 深绿色
        'trees': (40, 60, 35),     # 暗绿色
        'fog': (180, 190, 200, 80),  # 雾色
        'accent': (100, 120, 100)
    },
    'fog_front': {
        'name': '黑雾前沿',
        'sky': (80, 70, 100),     # 深紫灰
        'ground': (50, 45, 60),    # 暗紫
        'trees': (30, 25, 45),     # 更暗
        'fog': (100, 80, 120, 120),  # 黑雾
        'accent': (150, 100, 180)   # 紫光
    },
    'ancient_ruins': {
        'name': '古老废墟',
        'sky': (120, 130, 140),    # 蓝灰
        'ground': (90, 85, 75),    # 土色
        'trees': (70, 90, 60),     # 苔绿
        'fog': (200, 200, 190, 60),
        'accent': (180, 160, 120)   # 石头色
    },
    'dark_valley': {
        'name': '幽暗山谷',
        'sky': (60, 70, 80),       # 深蓝灰
        'ground': (40, 50, 40),    # 暗绿
        'trees': (25, 40, 30),     # 很暗
        'fog': (100, 110, 120, 100),
        'accent': (80, 150, 100)    # 幽光
    },
    'forest_heart': {
        'name': '森林之心',
        'sky': (100, 180, 150),    # 翠绿
        'ground': (80, 140, 100),  # 亮绿
        'trees': (60, 120, 80),    # 中绿
        'fog': (200, 255, 220, 40),
        'accent': (255, 255, 200)   # 金光
    }
}

def draw_tree(draw, x, y, height, color, dark=False):
    """绘制一棵树"""
    # 树干
    tw = max(2, height // 8)
    draw.rectangle([x - tw // 2, y, x + tw // 2, y + height // 3], fill=(80, 60, 40))
    
    # 树冠
    if dark:
        crown_color = (color[0] - 10, color[1] - 15, color[2] - 10)
    else:
        crown_color = color
    
    # 多层树冠
    for i in range(3):
        cy = y - height // 3 + i * height // 6
        cw = height // 2 - i * height // 10
        draw.ellipse([x - cw, cy - cw // 2, x + cw, cy + cw // 2], fill=crown_color)

def draw_rock(draw, x, y, size, color):
    """绘制岩石"""
    # 不规则多边形
    points = [
        (x - size, y),
        (x - size // 2, y - size),
        (x + size // 3, y - size * 1.2),
        (x + size, y - size // 2),
        (x + size // 2, y),
    ]
    draw.polygon(points, fill=color)
    # 高光
    highlight = (min(255, color[0] + 30), min(255, color[1] + 30), min(255, color[2] + 30))
    draw.polygon(points[:3], fill=highlight)

def draw_region(region_id, region, width=320, height=180):
    """绘制区域背景"""
    img = Image.new('RGB', (width, height), region['sky'])
    draw = ImageDraw.Draw(img)
    
    # 地面
    ground_y = height * 2 // 3
    draw.rectangle([0, ground_y, width, height], fill=region['ground'])
    
    # 远景树木
    for i in range(width // 20):
        x = i * 20 + random.randint(-5, 5)
        h = random.randint(40, 70)
        draw_tree(draw, x, ground_y, h, region['trees'], dark=True)
    
    # 中景元素
    if region_id == 'ancient_ruins':
        # 废墟石头
        for i in range(5):
            rx = 30 + i * 60 + random.randint(-10, 10)
            ry = ground_y - random.randint(10, 30)
            rs = random.randint(15, 25)
            draw_rock(draw, rx, ry, rs, region['accent'])
        # 断裂的石柱
        for i in range(3):
            sx = 50 + i * 100
            draw.rectangle([sx, ground_y - 50, sx + 15, ground_y], fill=(120, 115, 100))
            draw.rectangle([sx + 3, ground_y - 35, sx + 12, ground_y - 30], fill=(100, 95, 80))
    elif region_id == 'dark_valley':
        # 山谷两侧
        draw.polygon([(0, 0), (0, ground_y), (80, ground_y + 20)], fill=(50, 60, 55))
        draw.polygon([(width, 0), (width, ground_y), (width - 80, ground_y + 20)], fill=(50, 60, 55))
    elif region_id == 'forest_heart':
        # 发光的花
        for i in range(15):
            fx = random.randint(0, width)
            fy = ground_y + random.randint(5, 30)
            draw.ellipse([fx - 2, fy - 2, fx + 2, fy + 2], fill=region['accent'])
    
    # 近景树木
    for i in range(width // 35):
        x = i * 35 + random.randint(0, 15)
        h = random.randint(50, 80)
        draw_tree(draw, x, ground_y, h, region['trees'])
    
    # 雾层
    if region['fog'][3] > 0:
        fog_layer = Image.new('RGBA', (width, height), region['fog'])
        img.paste(fog_layer, (0, 0), fog_layer)
    
    # 前景草/细节
    if region_id not in ['ancient_ruins', 'fog_front']:
        for i in range(width // 8):
            x = i * 8 + random.randint(0, 4)
            h = random.randint(5, 12)
            grass_color = (region['ground'][0] - 10, region['ground'][1], region['ground'][2] - 5)
            draw.line([x, height, x, height - h], fill=grass_color, width=2)
    
    # 特殊效果
    if region_id == 'fog_front':
        # 黑雾涌动效果
        for i in range(8):
            fx = random.randint(0, width)
            fy = random.randint(height // 2, height)
            fr = random.randint(20, 50)
            draw.ellipse([fx - fr, fy - fr // 2, fx + fr, fy + fr // 2], fill=(60, 50, 80, 100))
    
    elif region_id == 'forest_heart':
        # 光芒效果
        for i in range(5):
            sx = width // 2 + random.randint(-100, 100)
            sy = random.randint(20, height // 2)
            draw.ellipse([sx - 5, sy - 20, sx + 5, sy], fill=(255, 255, 200, 150))
    
    return img

def generate_all_backgrounds(output_dir):
    """生成所有区域背景"""
    os.makedirs(output_dir, exist_ok=True)
    
    for region_id, region in REGIONS.items():
        img = draw_region(region_id, region, 320, 180)
        img = img.resize((640, 360), Image.NEAREST)
        
        filename = f"bg_{region_id}.png"
        img.save(os.path.join(output_dir, filename))
        
        # 生成缩略图
        thumb = img.resize((160, 90), Image.NEAREST)
        thumb.save(os.path.join(output_dir, f"thumb_{region_id}.png"))
        
        print(f"生成 {region['name']} 背景")
    
    print(f"共生成 {len(REGIONS)} 个区域背景")

if __name__ == '__main__':
    output = '/root/.openclaw/workspace/games/森林手账/Unity_Full/assets/backgrounds'
    generate_all_backgrounds(output)
    print("背景生成完成!")
