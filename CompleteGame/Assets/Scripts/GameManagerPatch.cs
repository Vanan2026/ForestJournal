using UnityEngine;
using System;

/// <summary>
/// GameManager 扩展补丁
/// 添加事件系统、手账集成、昼夜切换
/// 配合 JournalSystem / HandCardUI / DayNightFX 使用
/// </summary>
public class GameManagerPatch : MonoBehaviour
{
    public static GameManagerPatch instance { get; private set; }

    // 事件定义
    public event Action<int> OnPhaseChanged;        // 阶段变化 (0-3: 晨昼昏夜)
    public event Action<int> OnDayAdvanced;        // 新的一天
    public event Action<string> OnLogMessage;       // 日志消息
    public event Action OnGameOver;                 // 游戏结束
    public event Action OnVictory;                 // 胜利

    private GameManager gm;
    private DayNightFX dayNightFX;
    private JournalSystem journal;
    private HandCardUI handCardUI;

    // 内部状态
    private int lastPhase = -1;
    private int lastDay = -1;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        gm = GameManager.instance;
        if (gm == null)
        {
            gm = FindObjectOfType<GameManager>();
        }

        dayNightFX = FindObjectOfType<DayNightFX>();
        journal = FindObjectOfType<JournalSystem>();
        handCardUI = FindObjectOfType<HandCardUI>();

        if (gm != null)
        {
            lastPhase = gm.currentPhase;
            lastDay = gm.currentDay;
        }

        Debug.Log($"[GMPatch] 初始化完成 | Journal: {(journal != null)} | HandCard: {(handCardUI != null)} | DayNight: {(dayNightFX != null)}");
    }

    void Update()
    {
        if (gm == null) return;

        // 检测阶段变化
        if (gm.currentPhase != lastPhase)
        {
            lastPhase = gm.currentPhase;
            OnPhaseChanged?.Invoke(lastPhase);

            if (dayNightFX != null)
                dayNightFX.SetPhase(lastPhase);

            Debug.Log($"[GMPatch] 阶段变化: {lastPhase}");
        }

        // 检测天数变化
        if (gm.currentDay != lastDay)
        {
            lastDay = gm.currentDay;
            OnDayAdvanced?.Invoke(lastDay);

            if (journal != null)
                Debug.Log($"[GMPatch] 第 {lastDay} 天开始");
        }

        // 检测游戏结束
        if (gm.gs == "gameover" || gm.gs == "victory")
        {
            if (gm.gs == "gameover") OnGameOver?.Invoke();
            if (gm.gs == "victory") OnVictory?.Invoke();
        }
    }

    // ====================
    // 战斗拦截 - 替换 GameManager.Combat()
    // ====================

    /// <summary>
    /// 开始一场手牌战斗（替换简单战斗）
    /// </summary>
    public void StartCardCombat()
    {
        if (gm == null) return;

        // 检查夜晚禁止战斗
        if (gm.currentPhase == 3)
        {
            gm.AddLog("⚠️ 夜晚无法战斗，需要安营休息！");
            return;
        }

        // 检查AP
        if (gm.actionPoints < 3)
        {
            gm.AddLog("⚠️ AP不足，需要结束回合！");
            return;
        }

        // 消耗AP
        gm.ConsumeAP(3);

        // 构建敌人
        string[] enemies = { "阴影狼", "毒蜘蛛", "雾精灵", "黑雾兽", "变异树妖" };
        int[] hpByThreat = { 20, 30, 50, 80, 120 };
        int[] atkByThreat = { 5, 8, 12, 18, 25 };

        string enemyName = enemies[UnityEngine.Random.Range(0, enemies.Length)];
        int threat = Mathf.Clamp(gm.threatLevel, 1, 5);

        var enemyInfo = new HandCardUI.EnemyInfo
        {
            enemyName = enemyName,
            hp = hpByThreat[threat - 1] + UnityEngine.Random.Range(-3, 8),
            maxHP = hpByThreat[threat - 1] + 10,
            attack = atkByThreat[threat - 1] + UnityEngine.Random.Range(-2, 3),
            foodReward = UnityEngine.Random.Range(2, 5),
            soulReward = UnityEngine.Random.Range(0, 2)
        };

        if (handCardUI != null)
        {
            handCardUI.StartCombat(enemyInfo);
        }
        else
        {
            // Fallback: 简单战斗
            gm.AddLog($"⚔️ 遭遇 {enemyName}！");
            SimpleCombat(enemyName, enemyInfo);
        }
    }

    void SimpleCombat(string enemyName, HandCardUI.EnemyInfo info)
    {
        if (gm == null) return;

        int playerATK = gm.selectedMember?.attack ?? 12;
        int playerDEF = gm.selectedMember?.defense ?? 8;

        int damageToEnemy = Mathf.Max(1, playerATK - info.hp / 5);
        int damageToPlayer = Mathf.Max(1, info.attack - playerDEF / 2);

        info.hp -= damageToEnemy;
        gm.selectedMember.currentHealth -= damageToPlayer;

        gm.AddLog($"⚔️ 攻击！造成 {damageToEnemy} 伤害");
        gm.AddLog($"{enemyName} 攻击！受到 {damageToPlayer} 伤害");

        if (info.hp <= 0)
        {
            gm.food += info.foodReward;
            gm.soulEssence += info.soulReward;
            gm.AddLog($"🎉 胜利！获得 {info.foodReward} 食物");

            if (journal != null)
                journal.OnCombatVictory(enemyName, info.foodReward, info.soulReward);
        }
        else
        {
            gm.threatLevel = Mathf.Min(gm.threatLevel + 1, 5);
            gm.AddLog("⚠️ 战斗僵持，威胁升级！");
        }

        gm.UpdateAllUI();
    }

    // ====================
    // 手账集成
    // ====================

    /// <summary>
    /// 记录招募事件
    /// </summary>
    public void RecordRecruitment(string memberName, string fromRegion)
    {
        if (journal != null)
            journal.OnMemberRecruited(memberName, fromRegion);
    }

    /// <summary>
    /// 记录区域发现
    /// </summary>
    public void RecordDiscovery(string regionName, string description)
    {
        if (journal != null)
            journal.OnRegionDiscovered(regionName, description);
    }

    /// <summary>
    /// 记录成员受伤
    /// </summary>
    public void RecordInjury(string memberName, int hpLost)
    {
        if (journal != null)
            journal.OnMemberInjured(memberName, hpLost);
    }

    /// <summary>
    /// 记录成员死亡
    /// </summary>
    public void RecordDeath(string memberName)
    {
        if (journal != null)
            journal.OnMemberDeath(memberName);
    }

    // ====================
    // UI 快捷方法
    // ====================

    /// <summary>
    /// 显示手账（供按钮调用）
    /// </summary>
    public void ShowJournal()
    {
        if (journal != null)
            journal.ShowJournal();
    }

    /// <summary>
    /// 打开手牌战斗（白天可用）
    /// </summary>
    public void OpenCardCombat()
    {
        StartCardCombat();
    }
}
