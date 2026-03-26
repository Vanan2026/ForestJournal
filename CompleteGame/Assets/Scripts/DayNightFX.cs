using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 昼夜视觉系统
/// 四阶段（晨/昼/昏/夜）切换时的视觉反馈
/// </summary>
public class DayNightFX : MonoBehaviour
{
    public static DayNightFX instance { get; private set; }

    [Header("当前状态")]
    public int currentPhase = 0;  // 0=晨, 1=昼, 2=昏, 3=夜
    public string[] phaseNames = { "晨", "昼", "昏", "夜" };

    [Header("视觉配置")]
    public Color[] phaseBgColors = {
        new Color(0.95f, 0.90f, 0.80f),  // 晨 - 暖黄
        new Color(0.85f, 0.92f, 0.85f),  // 昼 - 自然绿
        new Color(0.90f, 0.65f, 0.50f),  // 昏 - 橙红
        new Color(0.10f, 0.12f, 0.20f)   // 夜 - 深蓝黑
    };

    public Color[] phaseFogColors = {
        new Color(0.80f, 0.85f, 0.90f),  // 晨雾 - 淡蓝
        new Color(0.90f, 0.92f, 0.95f),  // 昼雾 - 近白
        new Color(0.50f, 0.40f, 0.45f),  // 昏雾 - 暗红
        new Color(0.05f, 0.05f, 0.10f)    // 夜雾 - 近黑
    };

    [Header("UI引用")]
    public Image phaseIndicator;   // 阶段指示器
    public Image dayNightOverlay;  // 昼夜叠加层
    public Text phaseLabel;         // 阶段标签
    public RawImage skyImage;       // 天空背景（如有）

    [Header("过渡")]
    public float transitionDuration = 3f;
    private float transitionProgress = 1f;
    private int fromPhase;
    private int toPhase;
    private bool isTransitioning = false;

    // Camera.main.backgroundColor 会被这个值覆盖
    private Color targetBgColor;
    private Color currentBgColor;

    void Awake()
    {
        instance = this;
        targetBgColor = Camera.main != null ? Camera.main.backgroundColor : Color.black;
        currentBgColor = targetBgColor;
    }

    void Start()
    {
        // 初始设置
        ApplyPhaseVisual(currentPhase, instant: true);

        // 监听阶段变化
        if (GameManager.instance != null)
        {
            GameManager.instance.OnPhaseChanged += OnPhaseChangedHandler;
        }
    }

    void OnDestroy()
    {
        if (GameManager.instance != null)
            GameManager.instance.OnPhaseChanged -= OnPhaseChangedHandler;
    }

    void Update()
    {
        // 平滑过渡
        if (isTransitioning && transitionProgress < 1f)
        {
            transitionProgress += Time.deltaTime / transitionDuration;
            float t = EaseInOut(transitionProgress);

            Color bgFrom = phaseBgColors[fromPhase];
            Color bgTo = phaseBgColors[toPhase];
            Color newColor = Color.Lerp(bgFrom, bgTo, t);

            currentBgColor = newColor;

            if (Camera.main != null)
                Camera.main.backgroundColor = newColor;

            // 叠加层淡入
            if (dayNightOverlay != null)
            {
                float overlayAlpha = 0f;
                if (toPhase == 3) overlayAlpha = 0.3f;  // 夜晚
                else if (toPhase == 2) overlayAlpha = 0.1f;  // 黄昏
                dayNightOverlay.color = new Color(0, 0, 0.05f, overlayAlpha * t);
            }
        }
    }

    // ====================
    // 公共 API
    // ====================

    /// <summary>
    /// 切换到指定阶段（由 GameManager 调用）
    /// </summary>
    public void SetPhase(int phase)
    {
        if (phase == currentPhase) return;

        fromPhase = currentPhase;
        toPhase = phase;
        currentPhase = phase;
        transitionProgress = 0f;
        isTransitioning = true;

        // 更新阶段指示器
        UpdatePhaseIndicator(phase);

        Debug.Log($"[DayNight] 进入阶段：{phaseNames[phase]}");
    }

    /// <summary>
    /// 瞬间设置阶段（用于加载存档等）
    /// </summary>
    public void ApplyPhaseVisual(int phase, bool instant = false)
    {
        currentPhase = phase;
        isTransitioning = false;
        transitionProgress = 1f;

        var color = phaseBgColors[phase];
        currentBgColor = color;

        if (Camera.main != null)
            Camera.main.backgroundColor = color;

        UpdatePhaseIndicator(phase);
    }

    /// <summary>
    /// 获取当前阶段名称
    /// </summary>
    public string GetCurrentPhaseName()
    {
        return phaseNames[currentPhase];
    }

    /// <summary>
    /// 是否是夜晚
    /// </summary>
    public bool IsNight()
    {
        return currentPhase == 3;
    }

    /// <summary>
    /// 是否禁止战斗（夜晚禁止战斗）
    /// </summary>
    public bool IsCombatForbidden()
    {
        return currentPhase == 3;
    }

    // ====================
    // 内部逻辑
    // ====================

    void OnPhaseChangedHandler(int newPhase)
    {
        SetPhase(newPhase);
    }

    void UpdatePhaseIndicator(int phase)
    {
        if (phaseLabel != null)
        {
            string[] icons = { "🌅", "☀️", "🌇", "🌙" };
            phaseLabel.text = $"{icons[phase]} {phaseNames[phase]}";
        }

        if (phaseIndicator != null)
        {
            phaseIndicator.color = phaseBgColors[phase];
        }
    }

    float EaseInOut(float t)
    {
        return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
    }
}

// ============================================
// GameManager 扩展 - 添加事件通知
// ============================================
public static class GameManagerExtensions
{
    /// <summary>
    /// 在 GameManager.Combat() 开始时调用手牌 UI
    /// </summary>
    public static void StartCardCombat(this GameManager gm)
    {
        if (HandCardUI.instance != null && gm != null)
        {
            // 构建敌人信息
            string[] enemies = { "阴影狼", "毒蜘蛛", "雾精灵", "黑雾兽", "变异树妖" };
            int[] hpByThreat = { 20, 30, 50, 80, 120 };
            int[] atkByThreat = { 5, 8, 12, 18, 25 };

            string enemyName = enemies[Random.Range(0, enemies.Length)];
            int threat = Mathf.Clamp(gm.threatLevel, 1, 5);

            var enemyInfo = new HandCardUI.EnemyInfo
            {
                enemyName = enemyName,
                hp = hpByThreat[threat - 1] + Random.Range(-5, 10),
                maxHP = hpByThreat[threat - 1] + 10,
                attack = atkByThreat[threat - 1] + Random.Range(-2, 3),
                foodReward = Random.Range(2, 5),
                soulReward = Random.Range(0, 2)
            };

            HandCardUI.instance.StartCombat(enemyInfo);
        }
    }
}
