using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 视觉演出系统 v1.0
/// 屏幕特效/粒子/UI动画/过场演出
/// </summary>
public class VisualFXSystem : MonoBehaviour
{
    public static VisualFXSystem instance { get; private set; }

    [Header("全局引用")]
    public Camera mainCamera;

    [Header("特效配置")]
    public int maxParticles = 200;

    // 粒子池
    private Queue<GameObject> particlePool = new Queue<GameObject>();
    private List<GameObject> activeParticles = new List<GameObject>();

    void Awake()
    {
        instance = this;
        mainCamera = Camera.main;
        InitializeParticlePool();
    }

    // ====================
    // 粒子池初始化
    // ====================

    void InitializeParticlePool()
    {
        for (int i = 0; i < maxParticles; i++)
        {
            var obj = CreateParticleObject();
            obj.SetActive(false);
            particlePool.Enqueue(obj);
        }
    }

    GameObject CreateParticleObject()
    {
        var obj = new GameObject("Particle");
        var sr = obj.AddComponent<SpriteRenderer>();
        var life = obj.AddComponent<ParticleLife>();
        obj.SetActive(false);
        return obj;
    }

    // ====================
    // 特效 API（供战斗/叙事/采集等调用）
    // ====================

    /// <summary>
    /// 伤害数字飘字
    /// </summary>
    public void ShowDamageNumber(Vector3 worldPos, int damage, Color color, bool isPlayer = false)
    {
        var obj = SpawnParticle("damage");

        var sr = obj.GetComponent<SpriteRenderer>();
        // 创建文字纹理（简单版）
        var tex = CreateNumberTexture(damage.ToString(), color);
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        sr.sortingOrder = 100;

        var life = obj.GetComponent<ParticleLife>();
        life.lifetime = 1.2f;
        life.velocity = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 2f, 0);
        life.gravity = -3f;
        life.fadeSpeed = 1.5f;

        Vector3 screenPos = mainCamera != null
            ? mainCamera.WorldToScreenPoint(worldPos)
            : worldPos;

        // 放到屏幕上
        obj.transform.position = new Vector3(screenPos.x / Screen.width * 10 - 5,
            screenPos.y / Screen.height * 6 - 3, 0);
    }

    /// <summary>
    /// 资源获取特效
    /// </summary>
    public void ShowResourceGain(Vector3 worldPos, string resourceName, int amount)
    {
        var obj = SpawnParticle("gain");
        var sr = obj.GetComponent<SpriteRenderer>();
        var tex = CreateTextTexture($"+{amount} {resourceName}", new Color(0.3f, 0.9f, 0.3f));
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        sr.sortingOrder = 100;

        var life = obj.GetComponent<ParticleLife>();
        life.lifetime = 2f;
        life.velocity = new Vector3(0, 1.5f, 0);
        life.gravity = 0;
        life.fadeSpeed = 1f;

        Vector3 screenPos = mainCamera != null
            ? mainCamera.WorldToScreenPoint(worldPos)
            : worldPos;
        obj.transform.position = new Vector3(
            screenPos.x / Screen.width * 10 - 5,
            screenPos.y / Screen.height * 6 - 3, 0);
    }

    /// <summary>
    /// 治愈特效
    /// </summary>
    public void ShowHealEffect(Vector3 worldPos, int amount)
    {
        // 创建多个漂浮的光球
        for (int i = 0; i < 5; i++)
        {
            var obj = SpawnParticle("heal");
            var sr = obj.GetComponent<SpriteRenderer>();
            var tex = CreateCircleTexture(8, new Color(0.2f, 1f, 0.2f, 0.8f));
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            sr.sortingOrder = 90;

            var life = obj.GetComponent<ParticleLife>();
            life.lifetime = 1f;
            life.velocity = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(1f, 2f), 0);
            life.gravity = -1f;
            life.fadeSpeed = 1f;

            Vector3 pos = mainCamera != null
                ? mainCamera.WorldToScreenPoint(worldPos)
                : worldPos;
            obj.transform.position = new Vector3(
                pos.x / Screen.width * 10 - 5 + UnityEngine.Random.Range(-0.5f, 0.5f),
                pos.y / Screen.height * 6 - 3,
                0);
        }
    }

    /// <summary>
    /// 采集特效（粒子爆发）
    /// </summary>
    public void ShowGatherEffect(Vector3 worldPos, string resourceType)
    {
        Color color;
        switch (resourceType)
        {
            case "food": color = new Color(0.9f, 0.4f, 0.1f); break;
            case "wood": color = new Color(0.6f, 0.4f, 0.2f); break;
            case "herb": color = new Color(0.2f, 0.9f, 0.2f); break;
            case "stone": color = new Color(0.5f, 0.5f, 0.5f); break;
            case "soulEssence": color = new Color(0.6f, 0.2f, 1f); break;
            default: color = new Color(0.8f, 0.8f, 0.2f); break;
        }

        for (int i = 0; i < 8; i++)
        {
            var obj = SpawnParticle("burst");
            var sr = obj.GetComponent<SpriteRenderer>();
            var tex = CreateCircleTexture(UnityEngine.Random.Range(3, 8), color);
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            sr.sortingOrder = 80;

            var life = obj.GetComponent<ParticleLife>();
            life.lifetime = 0.8f;

            float angle = (i / 8f) * Mathf.PI * 2;
            float speed = UnityEngine.Random.Range(1f, 3f);
            life.velocity = new Vector3(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed, 0);
            life.gravity = -2f;
            life.fadeSpeed = 1.5f;

            Vector3 pos = mainCamera != null
                ? mainCamera.WorldToScreenPoint(worldPos)
                : worldPos;
            obj.transform.position = new Vector3(
                pos.x / Screen.width * 10 - 5,
                pos.y / Screen.height * 6 - 3,
                0);
        }
    }

    /// <summary>
    /// 夜晚星空背景特效
    /// </summary>
    public void ShowNightOverlay()
    {
        StartCoroutine(NightOverlayRoutine());
    }

    IEnumerator NightOverlayRoutine()
    {
        // 创建夜间叠加层
        var overlay = new GameObject("NightOverlay");
        var sr = overlay.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(
            new Texture2D(Screen.width, Screen.height),
            new Rect(0, 0, Screen.width, Screen.height),
            new Vector2(0.5f, 0.5f));
        sr.sortingOrder = 200;

        var tex = new Texture2D(Screen.width / 4, Screen.height / 4);
        for (int x = 0; x < tex.width; x++)
        {
            for (int y = 0; y < tex.height; y++)
            {
                float noise = UnityEngine.Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
                tex.SetPixel(x, y, new Color(0, 0, 0.05f, 0.3f + noise * 0.15f));
            }
        }
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

        yield return new WaitForSeconds(3f);

        // 淡出
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;
            sr.color = new Color(1, 1, 1, 1 - t);
            yield return null;
        }
        UnityEngine.Object.Destroy(overlay);
    }

    /// <summary===
    /// 篝火火焰效果
    /// </summary>
    public void ShowCampfireEffect(Vector3 worldPos)
    {
        StartCoroutine(CampfireEffectRoutine(worldPos));
    }

    IEnumerator CampfireEffectRoutine(Vector3 worldPos)
    {
        var fire = new GameObject("CampfireFX");
        fire.transform.position = worldPos;

        for (int i = 0; i < 15; i++)
        {
            var obj = SpawnParticle("fire");
            var sr = obj.GetComponent<SpriteRenderer>();
            var tex = CreateCircleTexture(UnityEngine.Random.Range(3, 10),
                new Color(1f, 0.5f + UnityEngine.Random.Range(0f, 0.5f), 0f, 0.9f));
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            sr.sortingOrder = 70;

            var life = obj.GetComponent<ParticleLife>();
            life.lifetime = UnityEngine.Random.Range(0.5f, 1.5f);
            life.velocity = new Vector3(UnityEngine.Random.Range(-0.3f, 0.3f), UnityEngine.Random.Range(1f, 2f), 0);
            life.gravity = -0.5f;
            life.fadeSpeed = 1f;

            obj.transform.position = worldPos;

            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// 记忆觉醒特效（粒子螺旋上升）
    /// </summary>
    public void ShowMemoryAwakeningEffect()
    {
        StartCoroutine(MemoryAwakeningRoutine());
    }

    IEnumerator MemoryAwakeningRoutine()
    {
        var center = new Vector3(Screen.width / 2, Screen.height / 2, 0);

        for (int wave = 0; wave < 3; wave++)
        {
            for (int i = 0; i < 20; i++)
            {
                var obj = SpawnParticle("memory");
                var sr = obj.GetComponent<SpriteRenderer>();
                var tex = CreateCircleTexture(4, new Color(0.8f, 0.6f, 1f, 1f));
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                sr.sortingOrder = 150;

                float angle = (i / 20f) * Mathf.PI * 2;
                float radius = 50 + wave * 100;
                Vector3 startPos = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);

                var life = obj.GetComponent<ParticleLife>();
                life.lifetime = 2f;
                life.velocity = new Vector3(-Mathf.Cos(angle) * 30, -Mathf.Sin(angle) * 30, 0);
                life.gravity = 0;
                life.fadeSpeed = 0.5f;

                obj.transform.position = new Vector3(
                    startPos.x / Screen.width * 10 - 5,
                    startPos.y / Screen.height * 6 - 3, 0);

                yield return new WaitForSeconds(0.05f);
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    // ====================
    // 过场演出
    // ====================

    /// <summary>
    /// 播放章节过渡演出
    /// </summary>
    public void PlayChapterTransition(int chapter, string title, string subtitle, Action onComplete)
    {
        StartCoroutine(ChapterTransitionRoutine(chapter, title, subtitle, onComplete));
    }

    IEnumerator ChapterTransitionRoutine(int chapter, string title, string subtitle, Action onComplete)
    {
        Time.timeScale = 0;

        // 创建黑屏
        var overlay = new GameObject("ChapterOverlay");
        var sr = overlay.AddComponent<SpriteRenderer>();
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.black);
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        sr.sortingOrder = 300;
        sr.color = new Color(0, 0, 0, 1);

        // 淡入
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / 2f;
            sr.color = new Color(0, 0, 0, t);
            yield return null;
        }

        // 显示章节文字（通过 UGUI 或 Debug.Log 模拟）
        Debug.Log($"═══════════════════════════");
        Debug.Log($"  第 {chapter} 章");
        Debug.Log($"  {title}");
        if (!string.IsNullOrEmpty(subtitle))
            Debug.Log($"  {subtitle}");
        Debug.Log($"═══════════════════════════");

        yield return new WaitForSeconds(2f);

        // 淡出
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / 1.5f;
            sr.color = new Color(0, 0, 0, 1 - t);
            yield return null;
        }

        UnityEngine.Object.Destroy(overlay);
        Time.timeScale = 1;
        onComplete?.Invoke();
    }

    // ====================
    // 屏幕震动
    // ====================

    public void ScreenShake(float intensity, float duration)
    {
        StartCoroutine(ScreenShakeRoutine(intensity, duration));
    }

    IEnumerator ScreenShakeRoutine(float intensity, float duration)
    {
        if (mainCamera == null) yield break;

        Vector3 original = mainCamera.transform.position;
        float elapsed = 0;

        while (elapsed < duration)
        {
            float x = UnityEngine.Random.Range(-1f, 1f) * intensity;
            float y = UnityEngine.Random.Range(-1f, 1f) * intensity;
            mainCamera.transform.position = original + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            intensity *= 0.95f;
            yield return null;
        }

        mainCamera.transform.position = original;
    }

    // ====================
    // 工具方法
    // ====================

    GameObject SpawnParticle(string type)
    {
        GameObject obj;
        if (particlePool.Count > 0)
            obj = particlePool.Dequeue();
        else
            obj = CreateParticleObject();

        obj.SetActive(true);
        activeParticles.Add(obj);
        return obj;
    }

    public void Update()
    {
        // 更新所有活跃粒子
        for (int i = activeParticles.Count - 1; i >= 0; i--)
        {
            var p = activeParticles[i];
            if (p == null || !p.activeSelf) { activeParticles.RemoveAt(i); continue; }

            var life = p.GetComponent<ParticleLife>();
            if (life == null) continue;

            life.lifetime -= Time.deltaTime;
            if (life.lifetime <= 0)
            {
                p.SetActive(false);
                particlePool.Enqueue(p);
                activeParticles.RemoveAt(i);
                continue;
            }

            // 运动
            p.transform.position += life.velocity * Time.deltaTime;
            life.velocity.y += life.gravity * Time.deltaTime;

            // 淡出
            var sr = p.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                float alpha = Mathf.Clamp01(life.lifetime * life.fadeSpeed);
                sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);
            }

            // 缩放
            float scale = Mathf.Clamp01(life.lifetime * 0.5f + 0.5f);
            p.transform.localScale = Vector3.one * scale;
        }
    }

    // ====================
    // 纹理生成
    // ====================

    Texture2D CreateNumberTexture(string text, Color color)
    {
        var tex = new Texture2D(64, 32);
        var pixels = new Color[64 * 32];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;
        tex.SetPixels(pixels);

        // 简单绘制（用Debug生成，不依赖字体）
        for (int x = 0; x < 64; x++)
            for (int y = 0; y < 32; y++)
                if (y > 8 && y < 24 && x > 8 && x < 56)
                    tex.SetPixel(x, y, color);

        tex.Apply();
        return tex;
    }

    Texture2D CreateTextTexture(string text, Color color)
    {
        var tex = new Texture2D(128, 32);
        var pixels = new Color[128 * 32];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;
        tex.SetPixels(pixels);

        for (int x = 0; x < 128; x++)
            for (int y = 0; y < 32; y++)
                if (y > 8 && y < 24 && x > 4 && x < 124)
                    tex.SetPixel(x, y, color);

        tex.Apply();
        return tex;
    }

    Texture2D CreateCircleTexture(int radius, Color color)
    {
        var tex = new Texture2D(radius * 2 + 1, radius * 2 + 1);
        var pixels = new Color[tex.width * tex.height];

        for (int x = 0; x < tex.width; x++)
        {
            for (int y = 0; y < tex.height; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius));
                if (dist <= radius)
                    pixels[y * tex.width + x] = color;
                else
                    pixels[y * tex.width + x] = Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    // ====================
    // 结局演出
    // ====================

    /// <summary>
    /// 播放结局演出
    /// </summary>
    public void PlayEndingSequence(EndingsSystem.Ending ending, Action onComplete = null)
    {
        StartCoroutine(EndingSequenceRoutine(ending, onComplete));
    }

    System.Collections.IEnumerator EndingSequenceRoutine(EndingsSystem.Ending ending, Action onComplete)
    {
        Time.timeScale = 0;

        // 1. 白色闪光
        var flash = new GameObject("EndingFlash");
        var sr = flash.AddComponent<SpriteRenderer>();
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        sr.sortingOrder = 500;
        sr.color = new Color(1, 1, 1, 0);

        float t = 0;
        while (t < 0.5f)
        {
            t += Time.deltaTime / 0.5f;
            sr.color = new Color(1, 1, 1, t);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.3f);

        // 2. 淡出到黑色
        while (t > 0)
        {
            t -= Time.deltaTime / 0.5f;
            sr.color = new Color(1, 1, 1, t);
            yield return null;
        }

        // 3. 粒子特效（记忆碎片飞舞）
        for (int i = 0; i < 50; i++)
        {
            var obj = SpawnParticle("memory");
            var psr = obj.GetComponent<SpriteRenderer>();
            var ptex = CreateCircleTexture(UnityEngine.Random.Range(4, 12), new Color(0.8f, 0.6f, 1f, 1f));
            psr.sprite = Sprite.Create(ptex, new Rect(0, 0, ptex.width, ptex.height), new Vector2(0.5f, 0.5f));
            psr.sortingOrder = 400;

            var life = obj.GetComponent<ParticleLife>();
            life.lifetime = 3f;

            float angle = UnityEngine.Random.Range(0, Mathf.PI * 2);
            float radius = UnityEngine.Random.Range(100f, 300f);
            Vector3 center = new Vector3(Screen.width / 2, Screen.height / 2, 0);
            Vector3 startPos = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);

            obj.transform.position = new Vector3(
                startPos.x / Screen.width * 10 - 5,
                startPos.y / Screen.height * 6 - 3, 0);

            life.velocity = -new Vector3(Mathf.Cos(angle) * 2, Mathf.Sin(angle) * 2, 0);
            life.gravity = 0;
            life.fadeSpeed = 0.3f;
        }

        yield return new WaitForSecondsRealtime(2f);

        // 4. 淡入结局文字
        Time.timeScale = 1;
        onComplete?.Invoke();

        yield return new WaitForSecondsRealtime(1f);

        UnityEngine.Object.Destroy(flash);
    }

    // ====================
    // 数据类
    // ====================

    public class ParticleLife : MonoBehaviour
    {
        public float lifetime = 1f;
        public Vector3 velocity;
        public float gravity = 0;
        public float fadeSpeed = 1f;
    }
}
