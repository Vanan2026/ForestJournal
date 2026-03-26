using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 引导教程系统 v1.0
/// 新手引导 / 上下文提示 / 首次事件触发
/// </summary>
public class TutorialSystem : MonoBehaviour
{
    public static TutorialSystem instance { get; private set; }

    [Header("当前教程状态")]
    public TutorialPhase currentPhase = TutorialPhase.None;
    public int currentStepIndex = 0;
    public bool isTutorialActive = false;
    public bool hasCompletedBasicTutorial = false;

    [Header("教程步骤")]
    public List<TutorialStep> tutorialSteps = new List<TutorialStep>();

    [Header("首次触发记录")]
    public HashSet<string> triggeredFirstEvents = new HashSet<string>();

    // 教程阶段
    public enum TutorialPhase
    {
        None,
        Basic,           // 基础操作教学
        FirstCombat,      // 首次战斗
        FirstGather,      // 首次采集
        FirstCraft,       // 首次制作
        FirstNPC,         // 首次NPC交互
        FirstCampfire,    // 首次安营
        FirstRegion,      // 首次进入新区域
        FirstMemory,      // 首次记忆碎片
        FirstForestHeart, // 首次发现森林之心
        Advanced,         // 高级技巧
    }

    // 教程步骤
    [System.Serializable]
    public class TutorialStep
    {
        public TutorialPhase phase;
        public int stepIndex;
        public string title;       // 标题
        public string content;     // 内容（支持多行）
        public string[] hints;     // 提示选项
        public string targetUI;    // 目标UI元素（高亮）
        public string action;      // 期望玩家执行的动作
        public int priority;       // 显示优先级

        public TutorialStep() { }
        public TutorialStep(TutorialPhase p, int idx, string t, string c, string a = "", string ui = "", int pri = 0)
        {
            phase = p;
            stepIndex = idx;
            title = t;
            content = c;
            action = a;
            targetUI = ui;
            priority = pri;
        }
    }

    void Awake()
    {
        instance = this;
        InitializeTutorialSteps();
        LoadTutorialProgress();
    }

    void Start()
    {
        // 根据当前游戏状态自动触发教程
        CheckAutoStartTutorial();
    }

    // ====================
    // 初始化教程步骤
    // ====================

    void InitializeTutorialSteps()
    {
        tutorialSteps = new List<TutorialStep>
        {
            // ====== 基础教程 ======
            new TutorialStep(TutorialPhase.Basic, 0, "欢迎来到森林",
                "你们醒来时已经在黑雾笼罩的森林中...\n\n这是第1天。你需要带领队伍在这片森林中生存下去。\n\n点击「继续」了解基础操作。",
                "continue", "", 100),

            new TutorialStep(TutorialPhase.Basic, 1, "认识你的队伍",
                "屏幕上显示了你们的队伍信息。\n\n• 叶青 - 炼金师（擅长制作药剂）\n• 阿森 - 漫游者（擅长战斗和移动）\n\n点击选中一个角色，然后进行下一步操作。",
                "select_member", "SquadPanel", 90),

            new TutorialStep(TutorialPhase.Basic, 2, "回合与行动点",
                "每个回合你有 8 个行动点（AP）。\n\n• 攻击消耗 2AP\n• 防御消耗 1AP\n• 采集消耗 1-2AP\n• 休息消耗 1AP\n\n合理分配行动点是生存的关键。",
                "", "", 85),

            new TutorialStep(TutorialPhase.Basic, 3, "采集资源",
                "森林中有丰富的资源可以采集：\n\n• 🍎 食物 - 维持生存，每天消耗\n• 🪵 木材 - 建造设施\n• 🌿 草药 - 制作药品\n• 🪨 石材 - 制作工具\n\n点击底部按钮采集资源。",
                "gather", "BottomPanel", 80),

            new TutorialStep(TutorialPhase.Basic, 4, "手账系统",
                "📖 你的手账会自动记录重要事件。\n\n收集记忆碎片可以：\n• 解锁更多结局\n• 提升NPC好感度上限\n• 揭示森林的秘密\n\n随时可以打开手账查看历史记录。",
                "", "JournalPanel", 75),

            // ====== 首次采集教程 ======
            new TutorialStep(TutorialPhase.FirstGather, 0, "首次采集",
                "你第一次尝试采集资源！\n\n采集时注意：\n• 每次采集消耗 1-2 AP\n• 采集量随机，但技能可以提高产量\n• 采集时可能遭遇敌人",
                "", "BottomPanel", 70),

            // ====== 首次战斗教程 ======
            new TutorialStep(TutorialPhase.FirstCombat, 0, "首次遭遇战",
                "⚔️ 森林中有危险的存在！\n\n进入战斗后：\n• 每回合消耗 AP 使用卡牌\n• 攻击卡：造成伤害\n• 防御卡：减少受到的伤害\n• 治愈卡：恢复 HP\n• 合理利用 AP，不要浪费！",
                "", "CardPanel", 95),

            new TutorialStep(TutorialPhase.FirstCombat, 1, "战斗结束",
                "🎉 胜利！\n\n战斗后：\n• 获得食物和魂精华奖励\n• 记录到手账中\n• 如果打不过，可以选择撤退\n\n记住，活着比战斗更重要。",
                "", "", 65),

            // ====== 首次制作教程 ======
            new TutorialStep(TutorialPhase.FirstCraft, 0, "制作系统",
                "🔨 你学会了制作！\n\n制作可以：\n• 用材料制作武器、防具\n• 用草药和食物制作药剂\n• 制作特殊物品\n\n打开制作面板查看可用配方。",
                "", "CraftPanel", 60),

            new TutorialStep(TutorialPhase.FirstCraft, 1, "制作提示",
                "制作配方需要材料。\n\n前期建议优先制作：\n• 绷带（恢复 HP）\n• 火把（增加照明）\n• 简易武器（提高攻击）\n\n资源有限，要有规划地使用。",
                "", "", 55),

            // ====== 篝火安营教程 ======
            new TutorialStep(TutorialPhase.FirstCampfire, 0, "安营休息",
                "🔥 在篝火旁休息可以：\n• 恢复 HP 和饥饿度\n• 重置 AP\n• 触发夜晚事件\n• 更新每日任务\n\n每天结束前记得安营！",
                "", "CampfireButton", 50),

            new TutorialStep(TutorialPhase.FirstCampfire, 1, "夜晚事件",
                "🌙 夜晚会触发特殊事件：\n• NPC 可能会来访\n• 可能发现隐藏资源\n• 也可能遭遇突袭\n\n篝火的光芒可以驱赶一些敌人。",
                "", "", 45),

            // ====== NPC交互教程 ======
            new TutorialStep(TutorialPhase.FirstNPC, 0, "遇见旅人",
                "👤 森林中遇到了其他幸存者！\n\n与NPC互动可以：\n• 招募他们加入队伍\n• 交换情报\n• 接受任务\n• 建立好感度",
                "", "", 40),

            new TutorialStep(TutorialPhase.FirstNPC, 1, "好感度系统",
                "💬 与NPC建立关系：\n• 对话增加少量好感\n• 完成委托增加好感\n• 赠送礼物大幅增加好感\n• 好感度高时可以招募\n\n高好感NPC会提供更好的奖励。",
                "", "", 35),

            // ====== 区域探索教程 ======
            new TutorialStep(TutorialPhase.FirstRegion, 0, "探索新区域",
                "🗺️ 森林有多个区域：\n• 迷雾森林（起点）\n• 迷雾前沿（危险）\n• 古老废墟（资源）\n• 幽暗山谷（挑战）\n• 森林之心（终点）\n\n区域越深，危险越大，奖励越丰厚。",
                "", "RegionButton", 30),

            // ====== 记忆碎片教程 ======
            new TutorialStep(TutorialPhase.FirstMemory, 0, "记忆的力量",
                "📖 你获得了记忆碎片！\n\n记忆碎片是游戏的核心资源：\n• 影响结局走向\n• 解锁隐藏内容\n• 提升制作配方\n\n尽可能多地收集记忆！",
                "", "", 25),

            // ====== 森林之心 ======
            new TutorialStep(TutorialPhase.FirstForestHeart, 0, "森林之心",
                "🌳 你找到了传说中的森林之心！\n\n森林之心是这片森林的核心：\n• 到达这里需要足够的记忆碎片\n• 第21天后可以触发结局\n• 不同记忆数量触发不同结局\n\n探索其他区域，收集更多记忆！",
                "", "", 20),

            // ====== 高级技巧 ======
            new TutorialStep(TutorialPhase.Advanced, 0, "职业配合",
                "🎯 队伍职业搭配很重要：\n\n• 炼金师 - 制作强效药剂\n• 漫游者 - 高机动性\n• 记录者 - 记忆加成\n• 建造者 - 设施升级\n\n后期队伍至少需要2个职业配合。",
                "", "", 15),

            new TutorialStep(TutorialPhase.Advanced, 1, "威胁等级",
                "⚠️ 黑雾的威胁会随时间增长！\n\n威胁等级越高：\n• 敌人越强\n• 遭遇更频繁\n• 但奖励也更丰厚\n\n合理规划时间，不要让威胁过高。",
                "", "", 10),

            new TutorialStep(TutorialPhase.Advanced, 2, "多周目",
                "♻️ 通关后可以开始新周目！\n\n多周目继承：\n• 记忆碎片（部分）\n• 解锁的NPC和配方\n• 达成过的结局\n\n收集所有结局是最终挑战！",
                "", "", 5),
        };
    }

    // ====================
    // 教程触发检查
    // ====================

    void CheckAutoStartTutorial()
    {
        if (isTutorialActive) return;

        var gm = GameManager.instance;
        if (gm == null) return;

        // 第一天，自动开始基础教程
        if (gm.currentDay == 1 && !hasCompletedBasicTutorial)
        {
            StartTutorial(TutorialPhase.Basic);
            return;
        }

        // 检查各种首次事件
        if (ShouldTriggerFirstGather())
            TriggerFirstEvent(TutorialPhase.FirstGather);
        else if (ShouldTriggerFirstCombat())
            TriggerFirstEvent(TutorialPhase.FirstCombat);
        else if (ShouldTriggerFirstCraft())
            TriggerFirstEvent(TutorialPhase.FirstCraft);
        else if (ShouldTriggerFirstNPC())
            TriggerFirstEvent(TutorialPhase.FirstNPC);
    }

    bool ShouldTriggerFirstGather()
    {
        return !triggeredFirstEvents.Contains("first_gather") &&
               GameManager.instance.gatherCount > 0;
    }

    bool ShouldTriggerFirstCombat()
    {
        return !triggeredFirstEvents.Contains("first_combat") &&
               GameManager.instance.combatCount > 0;
    }

    bool ShouldTriggerFirstCraft()
    {
        return !triggeredFirstEvents.Contains("first_craft") &&
               GameManager.instance.craftCount > 0;
    }

    bool ShouldTriggerFirstNPC()
    {
        return !triggeredFirstEvents.Contains("first_npc") &&
               GameManager.instance.npcInteractionCount > 0;
    }

    // ====================
    // 教程控制
    // ====================

    /// <summary>
    /// 开始指定阶段的教程
    /// </summary>
    public void StartTutorial(TutorialPhase phase)
    {
        currentPhase = phase;
        currentStepIndex = 0;
        isTutorialActive = true;

        ShowCurrentStep();
    }

    /// <summary>
    /// 触发首次事件教程
    /// </summary>
    public void TriggerFirstEvent(TutorialPhase phase)
    {
        string key = phase.ToString().ToLower();
        if (triggeredFirstEvents.Contains(key)) return;

        triggeredFirstEvents.Add(key);
        SaveTutorialProgress();

        StartTutorial(phase);
    }

    /// <summary>
    /// 显示当前教程步骤
    /// </summary>
    void ShowCurrentStep()
    {
        var step = GetCurrentStep();
        if (step == null)
        {
            EndTutorial();
            return;
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.AddLog("");
            GameManager.instance.AddLog($"═══ 📖 教程：{step.title} ═══");
            GameManager.instance.AddLog(step.content);
            GameManager.instance.AddLog("");

            // 根据期望动作给出选项
            if (step.action == "continue")
            {
                GameManager.instance.AddLog("[点击任意按钮继续]");
            }
            else if (!string.IsNullOrEmpty(step.action))
            {
                GameManager.instance.AddLog($"[请执行：{step.action}]");
            }
        }
    }

    /// <summary>
    /// 玩家完成当前教程步骤
    /// </summary>
    public void CompleteCurrentStep()
    {
        currentStepIndex++;

        var step = GetCurrentStep();
        if (step == null)
        {
            EndTutorial();
            return;
        }

        ShowCurrentStep();
    }

    /// <summary>
    /// 结束当前教程
    /// </summary>
    void EndTutorial()
    {
        isTutorialActive = false;
        currentPhase = TutorialPhase.None;

        if (GameManager.instance != null)
        {
            GameManager.instance.AddLog("═══ 教程完成 ═══");
        }

        // 标记完成
        if (currentPhase == TutorialPhase.Basic)
        {
            hasCompletedBasicTutorial = true;
            SaveTutorialProgress();
        }

        currentPhase = TutorialPhase.None;
    }

    /// <summary>
    /// 跳过教程
    /// </summary>
    public void SkipTutorial()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.AddLog("教程已跳过。如需查看，输入「教程」重新开始。");
        }

        hasCompletedBasicTutorial = true;
        isTutorialActive = false;
        currentPhase = TutorialPhase.None;
        SaveTutorialProgress();
    }

    TutorialStep GetCurrentStep()
    {
        foreach (var step in tutorialSteps)
        {
            if (step.phase == currentPhase && step.stepIndex == currentStepIndex)
                return step;
        }
        return null;
    }

    // ====================
    // 玩家行为通知
    // ====================

    /// <summary>
    /// 玩家执行动作时调用（由 GameManager 等调用）
    /// </summary>
    public void OnPlayerAction(string actionType)
    {
        if (!isTutorialActive) return;

        var step = GetCurrentStep();
        if (step == null) return;

        // 检查是否执行了期望的动作
        if (step.action == actionType)
        {
            CompleteCurrentStep();
        }
    }

    /// <summary>
    /// 标记首次采集
    /// </summary>
    public void OnFirstGather() { TriggerFirstEvent(TutorialPhase.FirstGather); }

    /// <summary>
    /// 标记首次战斗
    /// </summary>
    public void OnFirstCombat() { TriggerFirstEvent(TutorialPhase.FirstCombat); }

    /// <summary>
    /// 标记首次制作
    /// </summary>
    public void OnFirstCraft() { TriggerFirstEvent(TutorialPhase.FirstCraft); }

    /// <summary>
    /// 标记首次NPC交互
    /// </summary>
    public void OnFirstNPC() { TriggerFirstEvent(TutorialPhase.FirstNPC); }

    /// <summary>
    /// 标记首次安营
    /// </summary>
    public void OnFirstCampfire()
    {
        if (!triggeredFirstEvents.Contains("first_campfire"))
        {
            triggeredFirstEvents.Add("first_campfire");
            TriggerFirstEvent(TutorialPhase.FirstCampfire);
        }
    }

    /// <summary>
    /// 标记首次进入新区域
    /// </summary>
    public void OnFirstRegion()
    {
        if (!triggeredFirstEvents.Contains("first_region"))
        {
            triggeredFirstEvents.Add("first_region");
            TriggerFirstEvent(TutorialPhase.FirstRegion);
        }
    }

    /// <summary>
    /// 标记首次获得记忆
    /// </summary>
    public void OnFirstMemory()
    {
        if (!triggeredFirstEvents.Contains("first_memory"))
        {
            triggeredFirstEvents.Add("first_memory");
            TriggerFirstEvent(TutorialPhase.FirstMemory);
        }
    }

    /// <summary>
    /// 标记发现森林之心
    /// </summary>
    public void OnFirstForestHeart()
    {
        if (!triggeredFirstEvents.Contains("first_forest_heart"))
        {
            triggeredFirstEvents.Add("first_forest_heart");
            TriggerFirstEvent(TutorialPhase.FirstForestHeart);
        }
    }

    // ====================
    // 持久化
    // ====================

    void SaveTutorialProgress()
    {
        string key = "ForestJournal_Tutorial";
        var data = new TutorialSaveData
        {
            hasCompletedBasic = hasCompletedBasicTutorial,
            triggeredEvents = new List<string>(triggeredFirstEvents)
        };

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();
    }

    void LoadTutorialProgress()
    {
        string key = "ForestJournal_Tutorial";
        if (!PlayerPrefs.HasKey(key)) return;

        try
        {
            string json = PlayerPrefs.GetString(key);
            var data = JsonUtility.FromJson<TutorialSaveData>(json);

            hasCompletedBasicTutorial = data.hasCompletedBasic;
            triggeredFirstEvents = new HashSet<string>(data.triggeredEvents);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Tutorial] 加载失败：{e.Message}");
        }
    }

    /// <summary>
    /// 重置教程进度
    /// </summary>
    public void ResetTutorial()
    {
        hasCompletedBasicTutorial = false;
        triggeredFirstEvents.Clear();
        currentPhase = TutorialPhase.None;
        currentStepIndex = 0;
        isTutorialActive = false;
        PlayerPrefs.DeleteKey("ForestJournal_Tutorial");
        Debug.Log("[Tutorial] 已重置");
    }

    [System.Serializable]
    class TutorialSaveData
    {
        public bool hasCompletedBasic;
        public List<string> triggeredEvents;
    }
}
