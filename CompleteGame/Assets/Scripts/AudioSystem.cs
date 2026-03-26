using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// 音效与音乐系统 v1.0
/// 背景音乐 / 战斗BGM / 环境音 / 音效播放
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioSystem : MonoBehaviour
{
    public static AudioSystem instance { get; private set; }

    [Header("音频源")]
    public AudioSource bgmSource;   // 背景音乐
    public AudioSource sfxSource;   // 音效

    [Header("音量")]
    [Range(0f, 1f)]
    public float bgmVolume = 0.7f;
    [Range(0f, 1f)]
    public float sfxVolume = 0.8f;

    [Header("BGM 片段")]
    public AudioClip bgmTitle;      // 标题画面
    public AudioClip bgmForest;     // 森林探索
    public AudioClip bgmCombat;      // 战斗
    public AudioClip bgmNight;      // 夜晚
    public AudioClip bgmEnding;    // 结局

    [Header("环境音")]
    public AudioClip sfxWind;       // 风声
    public AudioClip sfxRain;       // 雨声
    public AudioClip sfxFire;       // 篝火
    public AudioClip sfxBirds;      // 鸟鸣

    [Header("战斗音效")]
    public AudioClip sfxAttack;     // 攻击
    public AudioClip sfxDefend;     // 防御
    public AudioClip sfxSkill;     // 技能
    public AudioClip sfxHit;        // 受伤
    public AudioClip sfxVictory;   // 胜利
    public AudioClip sfxDefeat;    // 失败

    [Header("UI音效")]
    public AudioClip sfxClick;      // 点击
    public AudioClip sfxCard;      // 出牌
    public AudioClip sfxGather;    // 采集
    public AudioClip sfxCraft;     // 制作

    [Header("状态")]
    public bool isMuted = false;
    public string currentBGM = "";

    // 简单合成音效（程序化生成，不依赖外部文件）
    private System.Collections.Generic.Dictionary<string, AudioClip> synthesizedClips =
        new System.Collections.Generic.Dictionary<string, AudioClip>();

    void Awake()
    {
        instance = this;
        SetupAudioSources();

        // 程序化生成所有音效
        SynthesizeAllSounds();
    }

    void SetupAudioSources()
    {
        // BGM 源（循环）
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = bgmVolume;

        // SFX 源（不循环）
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;
    }

    // ====================
    // 程序化音效合成
    // ====================

    void SynthesizeAllSounds()
    {
        // 战斗音效
        synthesizedClips["attack"] = SynthesizeAttackSound();
        synthesizedClips["defend"] = SynthesizeDefendSound();
        synthesizedClips["skill"] = SynthesizeSkillSound();
        synthesizedClips["hit"] = SynthesizeHitSound();
        synthesizedClips["victory"] = SynthesizeVictorySound();
        synthesizedClips["defeat"] = SynthesizeDefeatSound();

        // UI音效
        synthesizedClips["click"] = SynthesizeClickSound();
        synthesizedClips["card"] = SynthesizeCardSound();
        synthesizedClips["gather"] = SynthesizeGatherSound();
        synthesizedClips["craft"] = SynthesizeCraftSound();

        // 环境音（循环片段）
        synthesizedClips["wind"] = SynthesizeWindSound();
        synthesizedClips["rain"] = SynthesizeRainSound();
        synthesizedClips["fire"] = SynthesizeFireSound();
        synthesizedClips["birds"] = SynthesizeBirdSound();

        // 背景音乐（简单旋律循环）
        synthesizedClips["bgm_forest"] = SynthesizeForestBGM();
        synthesizedClips["bgm_combat"] = SynthesizeCombatBGM();
        synthesizedClips["bgm_night"] = SynthesizeNightBGM();
    }

    /// <summary>
    /// 攻击音效：短促的冲击波
    /// </summary>
    AudioClip SynthesizeAttackSound()
    {
        int sampleRate = 11025;
        float duration = 0.12f;
        int samples = (int)(sampleRate * duration);
        var clip = AudioClip.Create("attack", samples, 1, sampleRate, false);
        var data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float freq = 200f - (t * 800f);  // 频率下降
            float env = 1f - (t / duration); // 包络衰减
            data[i] = Mathf.Clamp01(env * Mathf.Sin(2 * Mathf.PI * freq * t) * 0.6f);
        }
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>
    /// 防御音效：金属铛声
    /// </summary>
    AudioClip SynthesizeDefendSound()
    {
        int sampleRate = 11025;
        float duration = 0.25f;
        int samples = (int)(sampleRate * duration);
        var clip = AudioClip.Create("defend", samples, 1, sampleRate, false);
        var data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * 15f);
            float wave = Mathf.Sin(2 * Mathf.PI * 800f * t) * 0.5f +
                         Mathf.Sin(2 * Mathf.PI * 1200f * t) * 0.3f +
                         Mathf.Sin(2 * Mathf.PI * 1600f * t) * 0.2f;
            data[i] = Mathf.Clamp(env * wave, -1f, 1f);
        }
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>
    /// 技能音效：上升的光芒音
    /// </summary>
    AudioClip SynthesizeSkillSound()
    {
        int sampleRate = 11025;
        float duration = 0.5f;
        int samples = (int)(sampleRate * duration);
        var clip = AudioClip.Create("skill", samples, 1, sampleRate, false);
        var data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float env = t < 0.1f ? t * 10f : 1f - (t - 0.1f) / 0.4f;
            float freq = 300f + (t * 600f);  // 频率上升
            data[i] = Mathf.Clamp01(env * (Mathf.Sin(2 * Mathf.PI * freq * t) * 0.6f +
                     Mathf.Sin(2 * Mathf.PI * freq * 1.5f * t) * 0.3f));
        }
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>
    /// 受伤音效：沉闷的冲击
    /// </summary>
    AudioClip SynthesizeHitSound()
    {
        int sampleRate = 11025;
        float duration = 0.2f;
        int samples = (int)(sampleRate * duration);
        var clip = AudioClip.Create("hit", samples, 1, sampleRate, false);
        var data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float env = Mathf.Exp(-t * 20f);
            float noise = UnityEngine.Random.Range(-1f, 1f) * 0.3f;
            float tone = Mathf.Sin(2 * Mathf.PI * 100f * t) * 0.5f;
            data[i] = Mathf.Clamp(env * (noise + tone), -1f, 1f);
        }
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>
    /// 胜利音效：上升的三和弦
    /// </summary>
    AudioClip SynthesizeVictorySound()
    {
        int sampleRate = 11025;
        float duration = 0.8f;
        int samples = (int)(sampleRate * duration);
        var clip = AudioClip.Create("victory", samples, 1, sampleRate, false);
        var data = new float[samples];

        float[] notes = { 523f, 659f, 784f }; // C5, E5, G5
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float env = t < 0.05f ? t * 20f : 1f - (t - 0.05f) / 0.75f;
            float wave = 0f;
            foreach (float freq in notes)
                wave += Mathf.Sin(2 * Mathf.PI * freq * t);
            data[i] = Mathf.Clamp01(env * wave / notes.Length * 0.8f);
        }
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>
    /// 失败音效：下沉的悲剧音
    /// </summary>
    AudioClip SynthesizeDefeatSound()
    {
        int sampleRate = 11025;
        float duration = 1.0f;
        int samples = (int)(sampleRate * duration);
        var clip = AudioClip.Create("defeat", samples, 1, sampleRate, false);
        var data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float env = 1f - t / 1.0f;
            float freq = 300f - (t * 200f);  // 频率下降
            data[i] = Mathf.Clamp(env * Mathf.Sin(2 * Mathf.PI * freq * t) * 0.7f, -1f, 1f);
        }
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>
    /// 点击音效：清脆的咔哒
    /// </summary>
    AudioClip SynthesizeClickSound()
    {
        int sampleRate = 11025;
        float duration = 0.05f;
        int samples = (int)(sampleRate * duration);
        var clip = AudioClip.Create("click", samples, 1, sampleRate, false);
        var data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float env = 1f - t / 0.05f;
            data[i] = Mathf.Clamp01(env * UnityEngine.Random.Range(-0.2f, 0.2f));
        }
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>
    /// 出牌音效：纸牌滑动的沙沙声
    /// </summary>
    AudioClip SynthesizeCardSound()
    {
        int sampleRate = 11025;
        float duration = 0.15f;
        int samples = (int)(sampleRate * duration);
        var clip = AudioClip.Create("card", samples, 1, sampleRate, false);
        var data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float env = t < 0.02f ? t * 50f : 1f - (t - 0.02f) / 0.13f;
            float noise = UnityEngine.Random.Range(-1f, 1f) * 0.15f;
            float tone = Mathf.Sin(2 * Mathf.PI * 2000f * t) * 0.05f;
            data[i] = Mathf.Clamp(env * (noise + tone), -1f, 1f);
        }
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>
    /// 采集音效：轻快的收获音
    /// </summary>
    AudioClip SynthesizeGatherSound()
    {
        int sampleRate = 11025;
        float duration = 0.3f;
        int samples = (int)(sampleRate * duration);
        var clip = AudioClip.Create("gather", samples, 1, sampleRate, false);
        var data = new float[samples];

        float[] notes = { 880f, 1100f, 1320f }; // A5, C#6, E6
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float env = t < 0.05f ? t * 20f : 1f - (t - 0.05f) / 0.25f;
            float wave = 0f;
            foreach (float freq in notes)
                wave += Mathf.Sin(2 * Mathf.PI * freq * t);
            data[i] = Mathf.Clamp01(env * wave / notes.Length * 0.5f);
        }
        clip.SetData(data, 0);
        return clip;
    }

    /// <summary>
    /// 制作音效：锤子敲击声
    /// </summary>
    AudioClip SynthesizeCraftSound()
    {
        int sampleRate = 11025;
        float duration = 0.4f;
        int samples = (int)(sampleRate * duration);
        var clip = AudioClip.Create("craft", samples, 1, sampleRate, false);
        var data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            // 两下敲击
            float env1 = t < 0.02f ? t * 50f : 1f - (t - 0.02f) / 0.08f;
            float env2 = t > 0.15f && t < 0.17f ? (t - 0.15f) * 50f :
                         t >= 0.17f ? 1f - (t - 0.17f) / 0.23f : 0f;
            float wave1 = UnityEngine.Random.Range(-1f, 1f) * 0.6f * env1;
            float wave2 = UnityEngine.Random.Range(-1f, 1f) * 0.6f * env2;
            data[i] = Mathf.Clamp(wave1 + wave2, -1f, 1f);
        }
        clip.SetData(data, 0);
        return clip;
    }

    // ===== 环境音 =====

    AudioClip SynthesizeWindSound()
    {
        int sampleRate = 11025;
        float duration = 3.0f;
        int samples = (int)(sampleRate * duration);
        var clip = AudioClip.Create("wind", samples, 1, sampleRate, false);
        var data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float env = 0.3f + 0.2f * Mathf.Sin(t * 0.5f);
            float noise = UnityEngine.Random.Range(-1f, 1f);
            // 低通滤波效果
            data[i] = noise * env * 0.15f;
        }
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip SynthesizeRainSound()
    {
        int sampleRate = 11025;
        float duration = 4.0f;
        int samples = (int)(sampleRate * duration);
        var clip = AudioClip.Create("rain", samples, 1, sampleRate, false);
        var data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float noise = UnityEngine.Random.Range(-1f, 1f);
            // 雨滴随机性
            float drop = UnityEngine.Random.value > 0.998f ? UnityEngine.Random.Range(0.3f, 0.8f) : 0f;
            data[i] = (noise * 0.1f + drop) * 0.5f;
        }
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip SynthesizeFireSound()
    {
        int sampleRate = 11025;
        float duration = 2.0f;
        int samples = (int)(sampleRate * duration);
        var clip = AudioClip.Create("fire", samples, 1, sampleRate, false);
        var data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float noise = UnityEngine.Random.Range(-1f, 1f);
            float crackle = UnityEngine.Random.value > 0.99f ? UnityEngine.Random.Range(0.4f, 0.9f) : 0f;
            float env = 0.4f + 0.1f * Mathf.Sin(t * 3f);
            data[i] = (noise * 0.08f + crackle) * env;
        }
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip SynthesizeBirdSound()
    {
        int sampleRate = 11025;
        float duration = 2.0f;
        int samples = (int)(sampleRate * duration);
        var clip = AudioClip.Create("birds", samples, 1, sampleRate, false);
        var data = new float[samples];

        // 简单的鸟鸣音调
        float[] birdNotes = { 1200f, 1400f, 1600f, 1800f };
        int noteLen = samples / 4;
        for (int i = 0; i < samples; i++)
        {
            int noteIdx = i / noteLen;
            float t = (float)(i % noteLen) / noteLen;
            float freq = birdNotes[noteIdx % birdNotes.Length];
            float env = t < 0.1f ? t * 10f : 1f - (t - 0.1f) / 0.9f;
            float chirp = Mathf.Sin(2 * Mathf.PI * freq * t) * 0.3f +
                          Mathf.Sin(2 * Mathf.PI * freq * 1.2f * t) * 0.2f;
            data[i] = chirp * env * (noteIdx < 4 ? 1f : 0f);
        }
        clip.SetData(data, 0);
        return clip;
    }

    // ===== BGM =====

    AudioClip SynthesizeForestBGM()
    {
        int sampleRate = 11025;
        float duration = 8.0f;  // 8秒循环
        int samples = (int)(sampleRate * duration);
        var clip = AudioClip.Create("bgm_forest", samples, 1, sampleRate, false);
        var data = new float[samples];

        // 简单的和声进行：C - Am - F - G
        float[,] chords = {
            { 261f, 329f, 392f },  // C
            { 220f, 261f, 329f },  // Am
            { 174f, 220f, 261f },  // F
            { 196f, 246f, 293f }   // G
        };
        int chordLen = samples / 4;

        for (int i = 0; i < samples; i++)
        {
            int chordIdx = i / chordLen;
            float t = (float)(i % chordLen) / sampleRate;
            float env = t < 0.3f ? t / 0.3f : 1f - (t - (float)chordLen / sampleRate - 0.3f) / ((float)chordLen / sampleRate - 0.3f);
            env = Mathf.Clamp01(env);

            float wave = 0f;
            for (int n = 0; n < 3; n++)
            {
                float freq = chords[chordIdx, n];
                wave += Mathf.Sin(2 * Mathf.PI * freq * t) * 0.15f;
            }
            data[i] = wave * env;
        }
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip SynthesizeCombatBGM()
    {
        int sampleRate = 11025;
        float duration = 4.0f;  // 4秒急促循环
        int samples = (int)(sampleRate * duration);
        var clip = AudioClip.Create("bgm_combat", samples, 1, sampleRate, false);
        var data = new float[samples];

        // 战斗节奏：下行低音 + 紧张和弦
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float beat = Mathf.Repeat(t * 2f, 1f); // 2拍/秒
            float env = 1f - beat; // 矩形波包络

            float bass = Mathf.Sin(2 * Mathf.PI * 110f * t) * 0.3f;
            float chord = (Mathf.Sin(2 * Mathf.PI * 220f * t) +
                          Mathf.Sin(2 * Mathf.PI * 277f * t) +
                          Mathf.Sin(2 * Mathf.PI * 330f * t)) * 0.1f;

            // 16分音符的紧张感
            float note16 = Mathf.Repeat(t * 8f, 1f);
            float noteEnv = 1f - note16;
            float accent = Mathf.Sin(2 * Mathf.PI * 440f * t) * 0.15f * noteEnv;

            data[i] = (bass + chord + accent) * env;
        }
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip SynthesizeNightBGM()
    {
        int sampleRate = 11025;
        float duration = 12.0f;  // 12秒缓慢循环
        int samples = (int)(sampleRate * duration);
        var clip = AudioClip.Create("bgm_night", samples, 1, sampleRate, false);
        var data = new float[samples];

        // 夜晚：缓慢的低音 drone + 轻柔旋律
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;

            // 低音 drone
            float drone = Mathf.Sin(2 * Mathf.PI * 65f * t) * 0.15f +
                          Mathf.Sin(2 * Mathf.PI * 98f * t) * 0.08f;

            // 缓慢的旋律（每4秒一个音）
            float melodyT = Mathf.Repeat(t, 4f);
            float note = melodyT < 0.5f ? 330f : 294f; // E5 / D5
            float melEnv = melodyT < 0.1f ? melodyT * 10f :
                           melodyT > 3.5f ? (4f - melodyT) / 0.5f : 1f;
            float melody = Mathf.Sin(2 * Mathf.PI * note * t) * 0.12f * melEnv;

            // 缓慢的颤音
            float vibrato = 1f + 0.02f * Mathf.Sin(t * 4f);

            data[i] = (drone + melody * vibrato) * 0.7f;
        }
        clip.SetData(data, 0);
        return clip;
    }

    // ====================
    // 公共 API
    // ====================

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    public void PlayBGM(string bgmType)
    {
        if (isMuted || bgmType == currentBGM) return;

        AudioClip clip = null;
        switch (bgmType)
        {
            case "forest": clip = synthesizedClips.GetValueOrDefault("bgm_forest"); break;
            case "combat": clip = synthesizedClips.GetValueOrDefault("bgm_combat"); break;
            case "night": clip = synthesizedClips.GetValueOrDefault("bgm_night"); break;
            default: return;
        }

        if (clip != null)
        {
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
            currentBGM = bgmType;
        }
    }

    /// <summary>
    /// 停止 BGM
    /// </summary>
    public void StopBGM()
    {
        bgmSource.Stop();
        currentBGM = "";
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    public void PlaySFX(string sfxName)
    {
        if (isMuted) return;

        AudioClip clip;
        if (synthesizedClips.TryGetValue(sfxName, out clip))
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    /// <summary>
    /// 播放环境音（循环）
    /// </summary>
    public void PlayAmbient(string ambientType)
    {
        if (isMuted) return;

        AudioClip clip;
        switch (ambientType)
        {
            case "wind": clip = synthesizedClips.GetValueOrDefault("wind"); break;
            case "rain": clip = synthesizedClips.GetValueOrDefault("rain"); break;
            case "fire": clip = synthesizedClips.GetValueOrDefault("fire"); break;
            case "birds": clip = synthesizedClips.GetValueOrDefault("birds"); break;
            default: return;
        }

        if (clip != null)
        {
            bgmSource.clip = clip;
            bgmSource.loop = true;
            bgmSource.volume = bgmVolume * 0.4f;
            bgmSource.Play();
        }
    }

    /// <summary>
    /// 静音切换
    /// </summary>
    public void ToggleMute()
    {
        isMuted = !isMuted;
        bgmSource.mute = isMuted;
        sfxSource.mute = isMuted;
    }

    // ====================
    // 集成到游戏系统
    // ====================

    /// <summary>
    /// 战斗开始时调用
    /// </summary>
    public void OnCombatStart()
    {
        PlayBGM("combat");
        sfxSource.volume = sfxVolume;
    }

    /// <summary>
    /// 攻击时调用
    /// </summary>
    public void OnAttack() => PlaySFX("attack");

    /// <summary>
    /// 防御时调用
    /// </summary>
    public void OnDefend() => PlaySFX("defend");

    /// <summary>
    /// 使用技能时调用
    /// </summary>
    public void OnSkill() => PlaySFX("skill");

    /// <summary>
    /// 受伤时调用
    /// </summary>
    public void OnHit() => PlaySFX("hit");

    /// <summary>
    /// 胜利时调用
    /// </summary>
    public void OnVictory()
    {
        PlaySFX("victory");
        PlayBGM("forest");
    }

    /// <summary>
    /// 失败时调用
    /// </summary>
    public void OnDefeat()
    {
        PlaySFX("defeat");
        StopBGM();
    }

    /// <summary>
    /// 采集时调用
    /// </summary>
    public void OnGather() => PlaySFX("gather");

    /// <summary>
    /// 制作时调用
    /// </summary>
    public void OnCraft() => PlaySFX("craft");

    /// <summary>
    /// UI 点击时调用
    /// </summary>
    public void OnClick() => PlaySFX("click");

    /// <summary>
    /// 出牌时调用
    /// </summary>
    public void OnCard() => PlaySFX("card");
}
