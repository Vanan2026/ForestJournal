using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 多周目继承系统 v1.0
/// 二周目+/结局继承/解锁追踪/周目难度递进
/// </summary>
public class NewGamePlusSystem : MonoBehaviour
{
    public static NewGamePlusSystem instance { get; private set; }

    [Header("当前周目")]
    public int currentCycle = 0;  // 0 = 第一周目, 1 = 二周目, 2 = 三周目...

    [Header("永久解锁内容")]
    public List<string> unlockedContent = new List<string>();
    public List<string> unlockedEndings = new List<string>();  // 已解锁的结局ID
    public List<string> unlockedNPCs = new List<string>();     // 已解锁的NPC
    public List<string> unlockedRecipes = new List<string>(); // 已解锁的配方
    public List<string> unlockedSkills = new List<string>();  // 已解锁的技能

    [Header("周目统计")]
    public int totalCyclesPlayed = 0;
    public int totalDaysSurvived = 0;
    public int totalEnemiesDefeated = 0;
    public int totalQuestsCompleted = 0;

    [Header("继承规则")]
    public bool enableNGPlus = true;
    public bool[] cycleUnlocked = new bool[10];  // 最多10周目

    void Awake()
    {
        instance = this;
        LoadPersistentUnlocks();
    }

    // ====================
    // 继承内容配置
    // ====================

    /// <summary>
    /// 继承给新周目的内容
    /// </summary>
    public NGPlusInheritance GetInheritanceForNewCycle()
    {
        var inheritance = new NGPlusInheritance();

        if (currentCycle == 0)
        {
            // 一周目 → 二周目继承
            inheritance.carryOverFood = Mathf.RoundToInt(GameManager.instance.food * 0.3f);
            inheritance.carryOverWood = Mathf.RoundToInt(GameManager.instance.wood * 0.3f);
            inheritance.carryOverMemories = GameManager.instance.memories;  // 记忆全部继承

            inheritance.unlockedSkills = new List<string>(unlockedSkills);
            inheritance.unlockedRecipes = new List<string>(unlockedRecipes);
            inheritance.unlockedNPCs = new List<string>(unlockedNPCs);

            // 解锁灰烬NPC（二周目后出现）
            if (!unlockedNPCs.Contains("ashes"))
                unlockedNPCs.Add("ashes");
        }
        else if (currentCycle == 1)
        {
            // 二周目 → 三周目
            inheritance.carryOverFood = Mathf.RoundToInt(GameManager.instance.food * 0.2f);
            inheritance.carryOverWood = Mathf.RoundToInt(GameManager.instance.wood * 0.2f);
            inheritance.carryOverMemories = GameManager.instance.memories;

            // 解锁更多内容
            if (!unlockedNPCs.Contains("vos"))
                unlockedNPCs.Add("vos");  // 沃斯NPC
        }

        // 随着周目增加，难度提升但获得更多初始加成
        inheritance.difficultyMultiplier = 1f + (currentCycle * 0.15f);  // 难度+15%/周目
        inheritance.startingBonusMultiplier = 1f + (currentCycle * 0.1f); // 初始资源+10%/周目

        // 特殊解锁：根据结局
        foreach (var ending in unlockedEndings)
        {
            switch (ending)
            {
                case "融入森林":
                    // 解锁特殊初始技能
                    if (!unlockedSkills.Contains("nature_blessing"))
                        unlockedSkills.Add("nature_blessing");
                    break;
                case "驱散黑雾":
                    // 解锁更强初始装备
                    if (!unlockedRecipes.Contains("ancient_relic"))
                        unlockedRecipes.Add("ancient_relic");
                    break;
                case "带走秘密":
                    // 解锁隐藏对话
                    if (!unlockedContent.Contains("hidden_dialogue"))
                        unlockedContent.Add("hidden_dialogue");
                    break;
                case "永远留下":
                    // 无特殊解锁（悲剧结局惩罚）
                    break;
            }
        }

        inheritance.unlockedContent = new List<string>(unlockedContent);
        inheritance.unlockedRecipes = new List<string>(unlockedRecipes);
        inheritance.unlockedNPCs = new List<string>(unlockedNPCs);
        inheritance.unlockedSkills = new List<string>(unlockedSkills);

        return inheritance;
    }

    /// <summary>
    /// 开始新周目
    /// </summary>
    public void StartNewCycle(int cycleNumber)
    {
        currentCycle = cycleNumber;
        totalCyclesPlayed++;

        var inheritance = GetInheritanceForNewCycle();

        // 应用继承
        ApplyInheritance(inheritance);

        // 重置游戏状态
        ResetGameStateForNewCycle(inheritance);

        Debug.Log($"[NG+] 开始第 {currentCycle + 1} 周目");
        Debug.Log($"[NG+] 继承记忆: {inheritance.carryOverMemories}");
    }

    void ApplyInheritance(NGPlusInheritance inherit)
    {
        var gm = GameManager.instance;
        if (gm == null) return;

        // 应用初始资源加成
        gm.food = Mathf.RoundToInt(inherit.carryOverFood * inherit.startingBonusMultiplier);
        gm.wood = Mathf.RoundToInt(inherit.carryOverWood * inherit.startingBonusMultiplier);
        gm.memories = inherit.carryOverMemories;

        // 应用技能解锁
        var skills = FindObjectOfType<SkillsSystem>();
        if (skills != null)
        {
            foreach (var skill in inherit.unlockedSkills)
            {
                switch (skill)
                {
                    case "foraging": skills.hasForaging = true; break;
                    case "herbalism": skills.hasHerbalism = true; break;
                    case "hunting": skills.hasHunting = true; break;
                    case "tracking": skills.hasTracking = true; break;
                    case "nature_blessing":
                        // 自然祝福：每天开始时恢复5HP
                        break;
                }
            }
        }

        // 应用配方解锁
        var crafting = FindObjectOfType<CraftingSystem>();
        if (crafting != null)
        {
            // 配方已内置，只要解锁特殊配方即可
            if (inherit.unlockedRecipes.Contains("ancient_relic"))
            {
                // 可以制作古物
            }
        }

        // 应用NPC解锁
        var npcSystem = FindObjectOfType<NPCRecruitmentSystem>();
        if (npcSystem != null)
        {
            // 已解锁NPC在NPCRecruitmentSystem中自动可用
        }

        // 记录解锁
        unlockedContent.AddRange(inherit.unlockedContent);
    }

    void ResetGameStateForNewCycle(NGPlusInheritance inherit)
    {
        var gm = GameManager.instance;
        if (gm == null) return;

        // 重置可变状态
        gm.currentDay = 1;
        gm.currentPhase = 0;
        gm.actionPoints = gm.maxAPPerPhase;
        gm.currentRegion = 0;
        gm.storyPhase = 1;
        gm.gs = "normal";
        gm.threatLevel = 1;

        gm.stone = 3;
        gm.herb = 2;
        gm.fiber = 1;
        gm.ore = 0;
        gm.bone = 0;
        gm.soulEssence = 0;

        // 重置队伍
        gm.squad.Clear();
        InitializeStartingSquad();

        // 重置NPC关系
        var rel = FindObjectOfType<RelationshipSystem>();
        if (rel != null)
        {
            rel.relationships.Clear();
            rel.npcBonds.Clear();
        }

        // 重置区域发现
        if (gm.regions != null)
        {
            foreach (var r in gm.regions)
                r.discovered = false;
            if (gm.regions.Length > 0)
                gm.regions[0].discovered = true;
        }

        // 触发二周目开场
        TriggerNewCycleOpening();

        SavePersistentUnlocks();
    }

    void InitializeStartingSquad()
    {
        var gm = GameManager.instance;
        if (gm == null) return;

        // 根据解锁的NPC决定初始队伍
        var leaf = new SquadMember
        {
            memberId = "leaf",
            memberName = "叶青",
            role = "炼金师",
            maxHealth = 80,
            currentHealth = 80,
            attack = 8,
            defense = 6,
            intelligence = 14,
            agility = 9,
            status = "正常"
        };

        var sen = new SquadMember
        {
            memberId = "sen",
            memberName = "阿森",
            role = "漫游者",
            maxHealth = 110,
            currentHealth = 110,
            attack = 14,
            defense = 10,
            intelligence = 8,
            agility = 12,
            status = "正常"
        };

        gm.squad.Add(leaf);
        gm.squad.Add(sen);

        // 如果解锁了莉莉，她可以作为初始第三成员
        if (unlockedNPCs.Contains("lily"))
        {
            var lily = new SquadMember
            {
                memberId = "lily",
                memberName = "莉莉",
                role = "记录者",
                maxHealth = 50,
                currentHealth = 50,
                attack = 5,
                defense = 4,
                intelligence = 12,
                agility = 14,
                status = "正常"
            };
            gm.squad.Add(lily);
        }

        gm.selectedMember = leaf;
    }

    void TriggerNewCycleOpening()
    {
        string[] cycleNames = {
            "第一周目", "二周目", "三周目", "四周目", "五周目",
            "六周目", "七周目", "八周目", "九周目", "十周目+"
        };

        string name = currentCycle < cycleNames.Length ? cycleNames[currentCycle] : "十周目+";

        var fx = FindObjectOfType<VisualFXSystem>();
        fx?.PlayChapterTransition(currentCycle + 1, $"{name}开始", "记忆在轮回中延续...", null);

        if (currentCycle >= 1)
        {
            var gm = GameManager.instance;
            if (gm != null)
            {
                gm.AddLog($"═══ {name} ═══");
                gm.AddLog($"继承的记忆引导着你们前行...");
                gm.AddLog($"携带记忆碎片：{gm.memories}");
                gm.AddLog($"解锁NPC：{string.Join(", ", unlockedNPCs)}");
            }
        }
    }

    // ====================
    // 结局解锁追踪
    // ====================

    /// <summary>
    /// 记录达成结局
    /// </summary>
    public void RecordEnding(string endingId, string endingName)
    {
        if (!unlockedEndings.Contains(endingId))
        {
            unlockedEndings.Add(endingId);
            Debug.Log($"[NG+] 解锁结局：{endingName}");

            if (JournalSystem.instance != null)
            {
                JournalSystem.instance.Record("ending_unlock", $"结局解锁：{endingName}",
                    $"在{nameOf(currentCycle + 1)}周目达成了「{endingName}」。");
            }
        }

        SavePersistentUnlocks();
    }

    string nameOf(int n)
    {
        string[] names = { "第一", "第二", "第三", "第四", "第五", "第六", "第七", "第八", "第九", "第十" };
        return n < names.Length ? names[n - 1] : "第" + n;
    }

    /// <summary>
    /// 获取所有解锁的结局
    /// </summary>
    public List<string> GetUnlockedEndings() => new List<string>(unlockedEndings);

    /// <summary>
    /// 获取解锁结局数量
    /// </summary>
    public int GetEndingProgress()
    {
        int totalEndings = 8;  // 总结局数
        return unlockedEndings.Count * 100 / totalEndings;
    }

    // ====================
    // 持久化存储
    // ====================

    public void SavePersistentUnlocks()
    {
        string key = "ForestJournal_NGPlus";
        var data = new NGPlusSaveData
        {
            cycle = currentCycle,
            totalCyclesPlayed = totalCyclesPlayed,
            totalDaysSurvived = totalDaysSurvived,
            totalEnemiesDefeated = totalEnemiesDefeated,
            totalQuestsCompleted = totalQuestsCompleted,
            unlockedEndings = unlockedEndings,
            unlockedNPCs = unlockedNPCs,
            unlockedRecipes = unlockedRecipes,
            unlockedSkills = unlockedSkills,
            unlockedContent = unlockedContent
        };

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(key, SimpleEncrypt(json));
        PlayerPrefs.Save();
    }

    public void LoadPersistentUnlocks()
    {
        string key = "ForestJournal_NGPlus";
        if (!PlayerPrefs.HasKey(key)) return;

        try
        {
            string encrypted = PlayerPrefs.GetString(key);
            string json = SimpleDecrypt(encrypted);
            var data = JsonUtility.FromJson<NGPlusSaveData>(json);

            currentCycle = data.cycle;
            totalCyclesPlayed = data.totalCyclesPlayed;
            totalDaysSurvived = data.totalDaysSurvived;
            totalEnemiesDefeated = data.totalEnemiesDefeated;
            totalQuestsCompleted = data.totalQuestsCompleted;
            unlockedEndings = new List<string>(data.unlockedEndings);
            unlockedNPCs = new List<string>(data.unlockedNPCs);
            unlockedRecipes = new List<string>(data.unlockedRecipes);
            unlockedSkills = new List<string>(data.unlockedSkills);
            unlockedContent = new List<string>(data.unlockedContent);
        }
        catch (Exception e)
        {
            Debug.LogError($"[NG+] 加载失败：{e.Message}");
        }
    }

    /// <summary>
    /// 清除所有永久数据（用于重新开始）
    /// </summary>
    public void ResetAllProgress()
    {
        currentCycle = 0;
        totalCyclesPlayed = 0;
        totalDaysSurvived = 0;
        totalEnemiesDefeated = 0;
        totalQuestsCompleted = 0;
        unlockedEndings.Clear();
        unlockedNPCs.Clear();
        unlockedRecipes.Clear();
        unlockedSkills.Clear();
        unlockedContent.Clear();

        for (int i = 0; i < cycleUnlocked.Length; i++)
            cycleUnlocked[i] = false;

        PlayerPrefs.DeleteKey("ForestJournal_NGPlus");
        Debug.Log("[NG+] 已重置所有永久进度");
    }

    // ====================
    // 工具
    // ====================

    string SimpleEncrypt(string input)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(input);
        byte[] key = System.Text.Encoding.UTF8.GetBytes("ForestJournalKey2026NG");
        for (int i = 0; i < data.Length; i++)
            data[i] ^= key[i % key.Length];
        return System.Convert.ToBase64String(data);
    }

    string SimpleDecrypt(string input)
    {
        byte[] data = System.Convert.FromBase64String(input);
        byte[] key = System.Text.Encoding.UTF8.GetBytes("ForestJournalKey2026NG");
        for (int i = 0; i < data.Length; i++)
            data[i] ^= key[i % key.Length];
        return System.Text.Encoding.UTF8.GetString(data);
    }

    // ====================
    // 数据类
    // ====================

    [System.Serializable]
    public class NGPlusInheritance
    {
        public int carryOverFood;
        public int carryOverWood;
        public int carryOverMemories;
        public float difficultyMultiplier = 1f;
        public float startingBonusMultiplier = 1f;
        public List<string> unlockedContent = new List<string>();
        public List<string> unlockedRecipes = new List<string>();
        public List<string> unlockedNPCs = new List<string>();
        public List<string> unlockedSkills = new List<string>();
    }

    [System.Serializable]
    public class NGPlusSaveData
    {
        public int cycle;
        public int totalCyclesPlayed;
        public int totalDaysSurvived;
        public int totalEnemiesDefeated;
        public int totalQuestsCompleted;
        public List<string> unlockedEndings;
        public List<string> unlockedNPCs;
        public List<string> unlockedRecipes;
        public List<string> unlockedSkills;
        public List<string> unlockedContent;
    }
}
