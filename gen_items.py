#!/usr/bin/env python3
"""
森林手账 - 道具图标生成器
30个道具图标
"""
from PIL import Image, ImageDraw
import os

# 道具定义
ITEMS = {
    # 消耗品
    'food_berry': {'name': '浆果', 'color': (180, 80, 100), 'type': 'consumable'},
    'food_meat': {'name': '烤肉', 'color': (150, 100, 60), 'type': 'consumable'},
    'food_fish': {'name': '烤鱼', 'color': (120, 160, 200), 'type': 'consumable'},
    'food_fruit': {'name': '水果', 'color': (200, 150, 50), 'type': 'consumable'},
    
    # 药水
    'potion_health': {'name': '生命药水', 'color': (255, 80, 80), 'type': 'consumable'},
    'potion_antidote': {'name': '解毒药', 'color': (100, 200, 100), 'type': 'consumable'},
    'potion_energy': {'name': '精力药水', 'color': (255, 200, 50), 'type': 'consumable'},
    
    # 材料
    'mat_wood': {'name': '木材', 'color': (139, 90, 60), 'type': 'material'},
    'mat_stone': {'name': '石头', 'color': (128, 128, 128), 'type': 'material'},
    'mat_herb': {'name': '草药', 'color': (76, 140, 70), 'type': 'material'},
    'mat_fiber': {'name': '纤维', 'color': (180, 160, 120), 'type': 'material'},
    'mat_ore': {'name': '矿石', 'color': (100, 100, 140), 'type': 'material'},
    'mat_bone': {'name': '兽骨', 'color': (220, 220, 200), 'type': 'material'},
    'mat_soul': {'name': '魂精华', 'color': (150, 100, 200), 'type': 'material'},
    
    # 武器
    'weapon_staff': {'name': '魔杖', 'color': (100, 80, 60), 'type': 'weapon'},
    'weapon_bow': {'name': '弓', 'color': (139, 90, 60), 'type': 'weapon'},
    'weapon_sword': {'name': '剑', 'color': (180, 180, 200), 'type': 'weapon'},
    'weapon_dagger': {'name': '匕首', 'color': (160, 160, 170), 'type': 'weapon'},
    'weapon_axe': {'name': '斧', 'color': (100, 80, 60), 'type': 'weapon'},
    
    # 防具
    'armor_cloth': {'name': '布甲', 'color': (180, 160, 140), 'type': 'armor'},
    'armor_leather': {'name': '皮甲', 'color': (120, 80, 60), 'type': 'armor'},
    'armor_mail': {'name': '锁甲', 'color': (140, 140, 150), 'type': 'armor'},
    'armor_plate': {'name': '板甲', 'color': (100, 100, 120), 'type': 'armor'},
    
    # 特殊
    'special_map': {'name': '地图', 'color': (220, 200, 160), 'type': 'special'},
    'special_compass': {'name': '罗盘', 'color': (200, 180, 100), 'type': 'special'},
    'special_torch': {'name': '火把', 'color': (255, 150, 50), 'type': 'special'},
    'special_rope': {'name': '绳索', 'color': (180, 150, 100), 'type': 'special'},
    'special_key': {'name': '钥匙', 'color': (255, 200, 50), 'type': 'special'},
    'special_diary': {'name': '日记', 'color': (80, 60, 40), 'type': 'special'},
    'special_trap': {'name': '陷阱', 'color': (100, 80, 60), 'type': 'special'},
    'special_bait': {'name': '诱饵', 'color': (200, 150, 100), 'type': 'special'},
}

def draw_item(item_id, item, size=32):
    """绘制道具图标"""
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size // 2, size // 2
    color = item['color']
    itype = item['type']
    
    # 背景框
    draw.rectangle([2, 2, size - 3, size - 3], outline=(80, 80, 80), width=1)
    
    if itype == 'consumable' or itype == 'material':
        # 圆形物品
        r = size // 3
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=color)
        # 高光
        draw.ellipse([cx - r + 2, cy - r + 2, cx - 2, cy - 2], fill=(255, 255, 255, 100))
        
        if item_id.startswith('potion_'):
            # 药水瓶
            draw.rectangle([cx - 4, cy - 6, cx + 4, cy + 6], fill=color)
            draw.rectangle([cx - 2, cy - 8, cx + 2, cy - 6], fill=(200, 180, 150))
            
    elif itype == 'weapon':
        if 'staff' in item_id:
            # 魔杖
            draw.rectangle([cx - 2, cy - 12, cx + 2, cy + 12], fill=color)
            draw.ellipse([cx - 4, cy - 14, cx + 4, cy - 8], fill=(100, 200, 150))
        elif 'bow' in item_id:
            # 弓
            draw.arc([cx - 8, cy - 10, cx + 8, cy + 10], 0, 180, fill=color, width=2)
            draw.line([cx - 8, cy, cx + 8, cy], fill=(200, 180, 150), width=1)
        elif 'sword' in item_id:
            # 剑
            draw.rectangle([cx - 2, cy - 12, cx + 2, cy + 8], fill=(180, 180, 200))
            draw.rectangle([cx - 4, cy + 6, cx + 4, cy + 10], fill=(150, 100, 50))
        elif 'dagger' in item_id:
            # 匕首
            draw.polygon([(cx, cy - 10), (cx + 3, cy + 4), (cx - 3, cy + 4)], fill=(180, 180, 200))
            draw.rectangle([cx - 3, cy + 4, cx + 3, cy + 8], fill=(100, 80, 60))
        elif 'axe' in item_id:
            # 斧
            draw.rectangle([cx - 2, cy - 4, cx + 2, cy + 12], fill=(139, 90, 60))
            draw.polygon([(cx - 8, cy - 8), (cx, cy - 4), (cx + 8, cy - 8), (cx, cy - 2)], fill=(140, 140, 150))
            
    elif itype == 'armor':
        # 防具 - 简化人形
        # 身体
        draw.rectangle([cx - 6, cy - 4, cx + 6, cy + 10], fill=color)
        # 肩膀
        draw.rectangle([cx - 10, cy - 6, cx - 6, cy - 2], fill=color)
        draw.rectangle([cx + 6, cy - 6, cx + 10, cy - 2], fill=color)
        # 头盔
        draw.ellipse([cx - 5, cy - 10, cx + 5, cy - 4], fill=color)
        
    elif itype == 'special':
        if 'map' in item_id:
            # 地图
            draw.rectangle([cx - 10, cy - 8, cx + 10, cy + 8], fill=(240, 220, 180))
            draw.rectangle([cx - 8, cy - 6, cx + 8, cy + 6], outline=(150, 130, 100), width=1)
            # 线条
            for i in range(-6, 7, 4):
                draw.line([cx - 6, cy + i, cx + 6, cy + i], fill=(180, 160, 130), width=1)
        elif 'compass' in item_id:
            # 罗盘
            draw.ellipse([cx - 8, cy - 8, cx + 8, cy + 8], fill=(220, 200, 150), outline=(100, 80, 60))
            draw.polygon([(cx, cy - 6), (cx + 2, cy), (cx, cy + 6), (cx - 2, cy)], fill=(200, 60, 60))
            draw.ellipse([cx - 2, cy - 2, cx + 2, cy + 2], fill=(220, 200, 150))
        elif 'torch' in item_id:
            # 火把
            draw.rectangle([cx - 2, cy, cx + 2, cy + 12], fill=(139, 90, 60))
            # 火焰
            draw.ellipse([cx - 4, cy - 6, cx + 4, cy + 2], fill=(255, 150, 50))
            draw.ellipse([cx - 2, cy - 4, cx + 2, cy], fill=(255, 255, 100))
        elif 'key' in item_id:
            # 钥匙
            draw.ellipse([cx - 4, cy - 10, cx + 4, cy - 4], outline=(200, 180, 80), width=2)
            draw.rectangle([cx - 2, cy - 4, cx + 2, cy + 8], fill=(200, 180, 80))
            draw.rectangle([cx - 4, cy + 2, cx, cy + 5], fill=(200, 180, 80))
            draw.rectangle([cx + 1, cy + 4, cx + 4, cy + 7], fill=(200, 180, 80))
        elif 'diary' in item_id:
            # 日记
            draw.rectangle([cx - 8, cy - 10, cx + 8, cy + 8], fill=(80, 60, 40))
            draw.rectangle([cx - 6, cy - 8, cx + 6, cy + 6], fill=(240, 230, 200))
            # 文字线
            for i in range(-4, 7, 3):
                draw.line([cx - 4, cy + i, cx + 4, cy + i], fill=(100, 100, 100), width=1)
        elif 'trap' in item_id:
            # 陷阱
            draw.arc([cx - 8, cy - 4, cx + 8, cy + 12], 0, 180, fill=(139, 90, 60), width=2)
            draw.line([cx - 6, cy + 2, cx + 6, cy + 2], fill=(100, 80, 60), width=1)
        elif 'bait' in item_id:
            # 诱饵
            draw.ellipse([cx - 6, cy - 4, cx + 6, cy + 4], fill=(200, 150, 100))
            draw.ellipse([cx - 3, cy - 2, cx + 3, cy + 2], fill=(220, 180, 130))
        elif 'rope' in item_id:
            # 绳索
            for i in range(3):
                y = cy - 8 + i * 6
                draw.arc([cx - 6, y, cx + 6, y + 8], 0, 180, fill=(180, 150, 100), width=2)
    
    return img

def generate_all_items(output_dir):
    """生成所有道具图标"""
    os.makedirs(output_dir, exist_ok=True)
    
    for item_id, item in ITEMS.items():
        img = draw_item(item_id, item, 32)
        img = img.resize((48, 48), Image.NEAREST)
        
        filename = f"{item_id}.png"
        img.save(os.path.join(output_dir, filename))
    
    print(f"生成 {len(ITEMS)} 个道具图标")
    
    # 生成物品清单JSON
    import json
    items_json = {}
    for item_id, item in ITEMS.items():
        items_json[item_id] = {
            'name': item['name'],
            'type': item['type'],
            'stackable': item['type'] in ['consumable', 'material']
        }
    
    with open(os.path.join(output_dir, 'items.json'), 'w', encoding='utf-8') as f:
        json.dump(items_json, f, ensure_ascii=False, indent=2)
    
    print("生成物品清单 items.json")

if __name__ == '__main__':
    output = '/root/.openclaw/workspace/games/森林手账/Unity_Full/assets/items'
    generate_all_items(output)
    print("道具生成完成!")
