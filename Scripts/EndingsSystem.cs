using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 结局系统 v1.0
/// 10种结局，分支条件详细判定
/// </summary>
public class EndingsSystem : MonoBehaviour
{
    public static EndingsSystem instance { get; private set; }

    [Header("所有结局")]
    public List<Ending> allEndings = new List<Ending>();

    void Awake()
    {
        instance = this;
        InitializeEndings();
    }

    void InitializeEndings()
    {
        allEndings = new List<Ending>
        {
            // ========== 记忆共鸣结局 ==========
            new Ending {
                id = "ending_memory_merge",
                name = "记忆共鸣",
                category = "normal",
                description = "你们的记忆碎片达到了完美的共鸣，与森林之心融为一体，成为了新的森林守护者。",
                condition = (ctx) => ctx.memories >= 15 && ctx.hasAncientRelic && ctx.npcRelationsMax >= 80,
                requirement = "记忆≥15 + 持有古物 + 最高NPC好感≥80",
                type = "完美结局"
            },
            new Ending {
                id = "ending_memory_guard",
                name = "永恒守望",
                category = "normal",
                description = "你们选择留在森林中，日复一日地守护着这片土地，直到化为森林的一部分。",
                condition = (ctx) => ctx.memories >= 12 && ctx.squadCount >= 3,
                requirement = "记忆≥12 + 队伍≥3人",
                type = "好结局"
            },

            // ========== 驱散黑雾结局 ==========
            new Ending {
                id = "ending_dispel_full",
                name = "完全驱散",
                category = "good",
                description = "借助森林之心的力量和古老仪式，你们成功驱散了黑雾。森林重见天日，阳光照耀大地。",
                condition = (ctx) => ctx.soulEssence >= 20 && ctx.hasAncientRelic && ctx.completedQuests >= 10,
                requirement = "魂精华≥20 + 持有古物 + 完成≥10任务",
                type = "好结局"
            },
            new Ending {
                id = "ending_dispel_partial",
                name = "部分驱散",
                category = "good",
                description = "黑雾暂时被压制，但并未完全消失。你们建立起了新的营地，继续守护这片土地。",
                condition = (ctx) => ctx.soulEssence >= 10 && ctx.memories >= 8,
                requirement = "魂精华≥10 + 记忆≥8",
                type = "普通结局"
            },

            // ========== 带走秘密结局 ==========
            new Ending {
                id = "ending_secret_leave",
                name = "知识传承",
                category = "neutral",
                description = "你们带着森林的秘密离开了。多年后，森林的知识传遍了整个大陆，成为了人类对抗黑雾的希望。",
                condition = (ctx) => ctx.memories >= 10 && ctx.exploredRegions >= 4,
                requirement = "记忆≥10 + 探索≥4区域",
                type = "普通结局"
            },
            new Ending {
                id = "ending_secret_die",
                name = "倒在黎明前",
                category = "bad",
                description = "你们拼尽全力寻找森林之心，但最终还是倒在了黑雾的侵袭中...森林的秘密就此失传。",
                condition = (ctx) => ctx.currentDay > 30 && ctx.gs == "gameover",
                requirement = "第30天后游戏结束",
                type = "悲剧结局"
            },

            // ========== 永远留下结局 ==========
            new Ending {
                id = "ending_forever_forest",
                name = "永眠森林",
                category = "bad",
                description = "记忆太少，无法离开。你们与这片森林永远绑定在了一起，成为了黑雾的一部分。",
                condition = (ctx) => ctx.memories < 5 && ctx.soulEssence < 5,
                requirement = "记忆<5 + 魂精华<5",
                type = "悲剧结局"
            },
            new Ending {
                id = "ending_forever_wild",
                name = "野性生存",
                category = "neutral",
                description = "你们放弃了寻找森林之心，选择在森林边缘建立新的家园。黑雾虽然存在，但生活还要继续。",
                condition = (ctx) => ctx.food >= 30 && ctx.currentRegion == 0 && ctx.memories < 10,
                requirement = "食物≥30 + 留在起始区域 + 记忆<10",
                type = "普通结局"
            },

            // ========== 特殊结局 ==========
            new Ending {
                id = "ending_forest_guardian",
                name = "森林守护者",
                category = "secret",
                description = "✨隐藏结局✨\n\n森林之心承认了你们的资格。你们成为了新的森林守护者，永远守护这片土地。\n黑雾再也无法侵蚀这里，因为守护者的意志与森林同在。",
                condition = (ctx) => ctx.currentCycle >= 1 && ctx.memories >= 20 && ctx.squadCount >= 4 && ctx.currentRegion == 4,
                requirement = "二周目+ + 记忆≥20 + 队伍≥4 + 到达森林之心",
                type = "隐藏结局"
            },
            new Ending {
                id = "ending_freedom",
                name = "真正的自由",
                category = "secret",
                description = "✨隐藏结局✨\n\n你们发现了一个惊人的真相：这片森林只是一个更大世界的投影。\n带着森林的知识，你们回到了现实世界，成为了连接两个世界的桥梁。",
                condition = (ctx) => ctx.currentCycle >= 2 && ctx.unlockedEndingsCount >= 5 && ctx.hasAncientRelic,
                requirement = "三周目+ + 已解锁≥5结局 + 持有古物",
                type = "隐藏结局"
            }
        };
    }

    // ====================
    // 结局判定
    // ====================

    /// <summary>
    /// 检查当前是否满足任何结局条件
    /// </summary>
    public Ending CheckEndingConditions()
    {
        var ctx = BuildEndingContext();

        // 按优先级检查
        // 先检查隐藏结局，再检查普通结局
        var secretEndings = allEndings.FindAll(e => e.category == "secret");
        foreach (var ending in secretEndings)
        {
            if (ending.condition(ctx))
            {
                RecordEndingAchieved(ending);
                return ending;
            }
        }

        // 检查普通结局
        var normalEndings = allEndings.FindAll(e => e.category != "secret");
        foreach (var ending in normalEndings)
        {
            if (ending.condition(ctx))
            {
                RecordEndingAchieved(ending);
                return ending;
            }
        }

        // 默认结局
        var defaultEnding = allEndings.Find(e => e.id == "ending_forever_wild");
        return defaultEnding;
    }

    EndingContext BuildEndingContext()
    {
        var ctx = new EndingContext();
        var gm = GameManager.instance;
        var ng = FindObjectOfType<NewGamePlusSystem>();

        ctx.gs = gm?.gs ?? "normal";
        ctx.currentDay = gm?.currentDay ?? 1;
        ctx.currentRegion = gm?.currentRegion ?? 0;
        ctx.memories = gm?.memories ?? 0;
        ctx.soulEssence = gm?.soulEssence ?? 0;
        ctx.food = gm?.food ?? 0;
        ctx.exploredRegions = CountExploredRegions();
        ctx.squadCount = gm?.squad.Count ?? 0;

        // 检查是否有古物
        var crafting = FindObjectOfType<CraftingSystem>();
        ctx.hasAncientRelic = crafting?.ancientRelic > 0;

        // 最高NPC好感
        var rel = FindObjectOfType<RelationshipSystem>();
        if (rel != null && rel.relationships.Count > 0)
        {
            int maxTrust = 0;
            foreach (var kvp in rel.relationships)
            {
                if (kvp.Value.trustLevel > maxTrust)
                    maxTrust = kvp.Value.trustLevel;
            }
            ctx.npcRelationsMax = maxTrust;
        }

        // 完成任务数
        var quest = FindObjectOfType<QuestSystem>();
        ctx.completedQuests = quest?.completedQuests.Count ?? 0;

        // NG+数据
        ctx.currentCycle = ng?.currentCycle ?? 0;
        ctx.unlockedEndingsCount = ng?.unlockedEndings.Count ?? 0;

        return ctx;
    }

    int CountExploredRegions()
    {
        if (GameManager.instance?.regions == null) return 0;
        int count = 0;
        foreach (var r in GameManager.instance.regions)
            if (r.discovered) count++;
        return count;
    }

    void RecordEndingAchieved(Ending ending)
    {
        var ng = FindObjectOfType<NewGamePlusSystem>();
        if (ng != null)
        {
            ng.RecordEnding(ending.id, ending.name);

            // 检查是否达成全结局
            int total = allEndings.Count;
            int unlocked = ng.unlockedEndings.Count;
            if (unlocked >= total)
            {
                Debug.Log("🏆 全结局达成！！");
                JournalSystem.instance?.Record("ending_unlock", "🏆 全结局达成",
                    "恭喜你达成了《森林手账》的全部结局！");
            }
        }

        // 触发视觉特效
        var fx = FindObjectOfType<VisualFXSystem>();
        fx?.PlayEndingSequence(ending);
    }

    /// <summary>
    /// 显示结局预览（玩家可查看已解锁结局）
    /// </summary>
    public void ShowEndingPreview()
    {
        if (GameManager.instance == null) return;

        var ng = FindObjectOfType<NewGamePlusSystem>();

        GameManager.instance.AddLog("═══ 结局预览 ═══");

        foreach (var ending in allEndings)
        {
            bool unlocked = ng?.unlockedEndings.Contains(ending.id) ?? false;
            string status = unlocked ? "✅" : "🔒";

            if (ending.category == "secret" && !unlocked)
            {
                GameManager.instance.AddLog($"{status} {ending.name}（隐藏结局）");
            }
            else
            {
                string req = unlocked ? ending.requirement : "???";
                GameManager.instance.AddLog($"{status} {ending.name} [{ending.type}]");
                if (unlocked)
                    GameManager.instance.AddLog($"  条件：{req}");
            }
        }

        int unlockedCount = ng?.unlockedEndings.Count ?? 0;
        int total = allEndings.Count;
        GameManager.instance.AddLog($"═══ 结局进度：{unlockedCount}/{total} ═══");
    }

    /// <summary>
    /// 尝试触发结局（在森林之心区域触发）
    /// </summary>
    public void TryTriggerEnding()
    {
        if (GameManager.instance == null) return;

        if (GameManager.instance.currentRegion != 4)
        {
            GameManager.instance.AddLog("必须到达森林之心才能触发结局...");
            return;
        }

        if (GameManager.instance.currentDay < 21)
        {
            GameManager.instance.AddLog("森林之心尚未完全觉醒...");
            GameManager.instance.AddLog($"还需要等待至第21天（当前第{GameManager.instance.currentDay}天）");
            return;
        }

        // 触发结局检查
        var ending = CheckEndingConditions();
        DisplayEnding(ending);
    }

    void DisplayEnding(Ending ending)
    {
        var gm = GameManager.instance;
        if (gm == null) return;

        gm.gs = "victory";

        // 触发视觉特效
        var fx = FindObjectOfType<VisualFXSystem>();
        fx?.PlayChapterTransition(99, ending.name, ending.type, () =>
        {
            gm.AddLog("");
            gm.AddLog("═══════════════════════════════════");
            gm.AddLog($"  {ending.name}");
            gm.AddLog("═══════════════════════════════════");
            gm.AddLog(ending.description);
            gm.AddLog("");
            gm.AddLog("═════ THE END ═════");
            gm.AddLog("");
            gm.AddLog("感谢游玩《森林手账》！");
            gm.AddLog("");
            gm.AddLog("[1] 开始新周目（继承记忆）");
            gm.AddLog("[2] 返回标题画面");
            gm.AddLog("[3] 自由探索（不触发结局）");
        });
    }

    /// <summary>
    /// 处理结局后的选择
    /// </summary>
    public void HandleEndingChoice(int choice)
    {
        var ng = FindObjectOfType<NewGamePlusSystem>();
        var gm = GameManager.instance;

        switch (choice)
        {
            case 1: // 新周目
                if (ng != null)
                {
                    ng.currentCycle++;
                    ng.StartNewCycle(ng.currentCycle);
                }
                break;

            case 2: // 标题画面
                gm?.BtnRestart();
                break;

            case 3: // 继续探索
                gm.gs = "normal";
                gm.AddLog("继续在森林中探索...");
                break;
        }
    }

    // ====================
    // 数据类
    // ====================

    public class EndingContext
    {
        public string gs = "normal";
        public int currentDay = 1;
        public int currentRegion = 0;
        public int memories = 0;
        public int soulEssence = 0;
        public int food = 0;
        public int exploredRegions = 0;
        public int squadCount = 0;
        public bool hasAncientRelic = false;
        public int npcRelationsMax = 0;
        public int completedQuests = 0;
        public int currentCycle = 0;
        public int unlockedEndingsCount = 0;
    }

    [System.Serializable]
    public class Ending
    {
        public string id;
        public string name;
        public string category;  // normal/good/bad/secret
        public string type;      // 完美结局/好结局/普通结局/悲剧结局/隐藏结局
        public string description;
        public string requirement;  // 显示给玩家的条件
        public Func<EndingContext, bool> condition;
    }
}
