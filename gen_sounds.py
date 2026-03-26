#!/usr/bin/env python3
"""
森林手账 - 音效生成器
使用Python生成WAV格式的音效
"""
import struct
import math
import random
import os
import json

SAMPLE_RATE = 22050
OUTPUT_DIR = '/root/.openclaw/workspace/games/森林手账/Unity_Full/assets/sfx'

def write_wav(filename, data, duration_sec):
    """写入WAV文件"""
    with open(filename, 'wb') as f:
        # WAV header
        f.write(b'RIFF')
        f.write(struct.pack('<I', 36 + len(data)))
        f.write(b'WAVE')
        f.write(b'fmt ')
        f.write(struct.pack('<I', 16))  # chunk size
        f.write(struct.pack('<H', 1))   # PCM
        f.write(struct.pack('<H', 1))   # mono
        f.write(struct.pack('<I', SAMPLE_RATE))
        f.write(struct.pack('<I', SAMPLE_RATE * 2))  # byte rate
        f.write(struct.pack('<H', 2))   # block align
        f.write(struct.pack('<H', 16))  # bits per sample
        f.write(b'data')
        f.write(struct.pack('<I', len(data)))
        f.write(data)

def generate_attack_sound():
    """攻击音效 - 快速的短促声音"""
    duration = 0.15
    samples = int(SAMPLE_RATE * duration)
    data = bytearray()
    
    for i in range(samples):
        t = i / SAMPLE_RATE
        # 快速衰减的正弦波 + 噪声
        env = math.exp(-t * 30)
        freq = 200 + 400 * math.exp(-t * 20)
        sample = int(32767 * env * math.sin(2 * math.pi * freq * t))
        # 添加噪声
        noise = random.randint(-2000, 2000) * env
        sample = max(-32768, min(32767, int(sample + noise)))
        data.extend(struct.pack('<h', sample))
    
    return bytes(data)

def generate_hit_sound():
    """受击音效 -沉闷的撞击"""
    duration = 0.2
    samples = int(SAMPLE_RATE * duration)
    data = bytearray()
    
    for i in range(samples):
        t = i / SAMPLE_RATE
        env = math.exp(-t * 20)
        # 低频撞击 + 高频噪声
        low = math.sin(2 * math.pi * 80 * t)
        noise = (random.random() * 2 - 1) * 0.3
        sample = env * (low * 0.7 + noise)
        sample = int(32767 * sample * 0.8)
        data.extend(struct.pack('<h', sample))
    
    return bytes(data)

def generate_death_sound():
    """死亡音效 - 较长的衰减"""
    duration = 0.5
    samples = int(SAMPLE_RATE * duration)
    data = bytearray()
    
    for i in range(samples):
        t = i / SAMPLE_RATE
        env = math.exp(-t * 8)
        freq = 150 - 50 * t
        sample = int(32767 * env * math.sin(2 * math.pi * freq * t))
        # 添加下滑效果
        sample = int(sample * (1 - t * 0.5))
        data.extend(struct.pack('<h', sample))
    
    return bytes(data)

def generate_collect_sound():
    """采集音效 - 轻盈的高音"""
    duration = 0.25
    samples = int(SAMPLE_RATE * duration)
    data = bytearray()
    
    for i in range(samples):
        t = i / SAMPLE_RATE
        env = math.exp(-t * 15)
        # 上升音调
        freq = 400 + 600 * t
        sample = int(32767 * env * math.sin(2 * math.pi * freq * t))
        # 添加谐波
        sample2 = int(32767 * env * 0.3 * math.sin(2 * math.pi * freq * 2 * t))
        sample = max(-32768, min(32767, sample + sample2))
        data.extend(struct.pack('<h', sample))
    
    return bytes(data)

def generate_footstep_sound():
    """脚步声"""
    duration = 0.1
    samples = int(SAMPLE_RATE * duration)
    data = bytearray()
    
    for i in range(samples):
        t = i / SAMPLE_RATE
        env = math.exp(-t * 40)
        noise = (random.random() * 2 - 1) * env
        sample = int(32767 * noise * 0.4)
        data.extend(struct.pack('<h', sample))
    
    return bytes(data)

def generate_ui_click_sound():
    """UI点击音效"""
    duration = 0.08
    samples = int(SAMPLE_RATE * duration)
    data = bytearray()
    
    for i in range(samples):
        t = i / SAMPLE_RATE
        env = math.exp(-t * 50)
        freq = 800
        sample = int(32767 * env * math.sin(2 * math.pi * freq * t))
        data.extend(struct.pack('<h', sample))
    
    return bytes(data)

def generate_success_sound():
    """成功音效"""
    duration = 0.4
    samples = int(SAMPLE_RATE * duration)
    data = bytearray()
    
    for i in range(samples):
        t = i / SAMPLE_RATE
        # 上升和弦
        env = math.exp(-t * 5) if t > 0.2 else 1
        f1 = 523  # C5
        f2 = 659  # E5
        f3 = 784  # G5
        s1 = math.sin(2 * math.pi * f1 * t)
        s2 = math.sin(2 * math.pi * f2 * t)
        s3 = math.sin(2 * math.pi * f3 * t)
        sample = int(32767 * env * (s1 + s2 + s3) / 3 * 0.5)
        data.extend(struct.pack('<h', sample))
    
    return bytes(data)

def generate_failure_sound():
    """失败音效"""
    duration = 0.4
    samples = int(SAMPLE_RATE * duration)
    data = bytearray()
    
    for i in range(samples):
        t = i / SAMPLE_RATE
        env = math.exp(-t * 6)
        # 下降音调
        freq = 400 - 200 * t
        sample = int(32767 * env * math.sin(2 * math.pi * freq * t))
        data.extend(struct.pack('<h', sample))
    
    return bytes(data)

def generate_rain_sound():
    """雨声"""
    duration = 2.0
    samples = int(SAMPLE_RATE * duration)
    data = bytearray()
    
    for i in range(samples):
        # 白噪声 + 低通滤波效果
        noise = random.random() * 2 - 1
        # 简单的低通滤波
        if i > 0:
            prev = data[-1] / 32767 if len(data) >= 2 else 0
            noise = noise * 0.3 + prev * 0.7
        sample = int(32767 * noise * 0.3)
        data.extend(struct.pack('<h', sample))
    
    return bytes(data)

def generate_wind_sound():
    """风声"""
    duration = 2.0
    samples = int(SAMPLE_RATE * duration)
    data = bytearray()
    
    for i in range(samples):
        t = i / SAMPLE_RATE
        # 调制的噪声
        mod = math.sin(2 * math.pi * 0.5 * t) * 0.5 + 0.5
        noise = (random.random() * 2 - 1) * mod
        # 添加低频呼啸
        low = math.sin(2 * math.pi * 100 * t) * 0.2
        sample = int(32767 * (noise * 0.4 + low))
        data.extend(struct.pack('<h', sample))
    
    return bytes(data)

def generate_thunder_sound():
    """雷声"""
    duration = 1.5
    samples = int(SAMPLE_RATE * duration)
    data = bytearray()
    
    for i in range(samples):
        t = i / SAMPLE_RATE
        if t < 0.1:
            # 初始雷鸣
            env = t / 0.1
        else:
            # 衰减
            env = math.exp(-(t - 0.1) * 3)
        
        noise = random.random() * 2 - 1
        # 低频成分
        low = math.sin(2 * math.pi * 60 * t) * 0.5
        sample = int(32767 * env * (noise * 0.6 + low * 0.4))
        data.extend(struct.pack('<h', sample))
    
    return bytes(data)

def generate_forest_ambient():
    """森林环境音"""
    duration = 3.0
    samples = int(SAMPLE_RATE * duration)
    data = bytearray()
    
    for i in range(samples):
        t = i / SAMPLE_RATE
        # 基础风声
        wind = (random.random() * 2 - 1) * 0.1
        # 鸟鸣（周期性）
        bird = 0
        if int(t * 3) % 7 == 0:
            bird_freq = 2000 + math.sin(t * 20) * 500
            bird = math.sin(2 * math.pi * bird_freq * t) * 0.15
        # 树叶沙沙
        leaf = (random.random() * 2 - 1) * 0.05
        sample = int(32767 * (wind + bird + leaf))
        data.extend(struct.pack('<h', sample))
    
    return bytes(data)

def generate_night_ambient():
    """夜晚环境音"""
    duration = 3.0
    samples = int(SAMPLE_RATE * duration)
    data = bytearray()
    
    for i in range(samples):
        t = i / SAMPLE_RATE
        # 更安静的风声
        wind = (random.random() * 2 - 1) * 0.05
        # 蟋蟀声
        cricket = math.sin(2 * math.pi * 4000 * t) * math.sin(2 * math.pi * 5 * t) * 0.1
        # 猫头鹰（偶尔）
        owl = 0
        if int(t * 0.5) % 11 == 0:
            owl = math.sin(2 * math.pi * 300 * t) * 0.2
        sample = int(32767 * (wind + cricket + owl))
        data.extend(struct.pack('<h', sample))
    
    return bytes(data)

def generate_open_sound():
    """打开/发现音效"""
    duration = 0.3
    samples = int(SAMPLE_RATE * duration)
    data = bytearray()
    
    for i in range(samples):
        t = i / SAMPLE_RATE
        env = math.exp(-t * 10)
        freq = 300 + 400 * t
        sample = int(32767 * env * math.sin(2 * math.pi * freq * t))
        # 谐波
        sample2 = int(32767 * env * 0.5 * math.sin(2 * math.pi * freq * 2 * t))
        sample = max(-32768, min(32767, sample + sample2))
        data.extend(struct.pack('<h', sample))
    
    return bytes(data)

def generate_levelup_sound():
    """升级音效"""
    duration = 0.6
    samples = int(SAMPLE_RATE * duration)
    data = bytearray()
    
    for i in range(samples):
        t = i / SAMPLE_RATE
        # 上升的音阶
        notes = [523, 659, 784, 1047]  # C5, E5, G5, C6
        note_idx = min(int(t * 8), 3)
        freq = notes[note_idx]
        env = 1 if t < 0.5 else math.exp(-(t - 0.5) * 10)
        sample = int(32767 * env * math.sin(2 * math.pi * freq * t))
        data.extend(struct.pack('<h', sample))
    
    return bytes(data)

def generate_heal_sound():
    """治疗音效"""
    duration = 0.5
    samples = int(SAMPLE_RATE * duration)
    data = bytearray()
    
    for i in range(samples):
        t = i / SAMPLE_RATE
        env = math.sin(math.pi * t / 0.5)  # 先升后降
        # 柔和的高频
        freq = 800 + 200 * math.sin(math.pi * t / 0.5)
        sample = int(32767 * env * math.sin(2 * math.pi * freq * t) * 0.5)
        # 添加泛音
        sample2 = int(32767 * env * math.sin(2 * math.pi * freq * 2 * t) * 0.3)
        sample = max(-32768, min(32767, sample + sample2))
        data.extend(struct.pack('<h', sample))
    
    return bytes(data)

def generate_splash_sound():
    """水花音效"""
    duration = 0.4
    samples = int(SAMPLE_RATE * duration)
    data = bytearray()
    
    for i in range(samples):
        t = i / SAMPLE_RATE
        env = math.exp(-t * 12)
        # 噪声 + 低频
        noise = random.random() * 2 - 1
        low = math.sin(2 * math.pi * 150 * t)
        sample = int(32767 * env * (noise * 0.5 + low * 0.5) * 0.6)
        data.extend(struct.pack('<h', sample))
    
    return bytes(data)

def generate_all_sounds():
    """生成所有音效"""
    os.makedirs(OUTPUT_DIR, exist_ok=True)
    
    sounds = {
        # 战斗
        'battle_attack': generate_attack_sound(),
        'battle_hit': generate_hit_sound(),
        'battle_death': generate_death_sound(),
        'battle_victory': generate_success_sound,
        
        # UI
        'ui_click': generate_ui_click_sound(),
        'ui_success': generate_success_sound(),
        'ui_failure': generate_failure_sound(),
        'ui_levelup': generate_levelup_sound(),
        
        # 交互
        'collect': generate_collect_sound(),
        'footstep': generate_footstep_sound(),
        'open': generate_open_sound(),
        'heal': generate_heal_sound(),
        'splash': generate_splash_sound(),
        
        # 环境
        'rain': generate_rain_sound(),
        'wind': generate_wind_sound(),
        'thunder': generate_thunder_sound(),
        'forest': generate_forest_ambient(),
        'night': generate_night_ambient(),
    }
    
    for name, data in sounds.items():
        if callable(data):
            data = data()
        filename = f"{name}.wav"
        write_wav(os.path.join(OUTPUT_DIR, filename), data, len(data) / SAMPLE_RATE / 2)
    
    print(f"生成 {len(sounds)} 个音效文件")
    
    # 生成音效清单
    sfx_list = {
        'battle': ['attack', 'hit', 'death', 'victory'],
        'ui': ['click', 'success', 'failure', 'levelup'],
        'interaction': ['collect', 'footstep', 'open', 'heal', 'splash'],
        'ambient': ['rain', 'wind', 'thunder', 'forest', 'night']
    }
    
    with open(os.path.join(OUTPUT_DIR, 'sfx_list.json'), 'w') as f:
        json.dump(sfx_list, f, indent=2)
    
    print("音效清单 sfx_list.json")

if __name__ == '__main__':
    generate_all_sounds()
    print("音效生成完成!")
