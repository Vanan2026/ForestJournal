using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 森林手账 - 完整版 v4.0
/// 目标：1个月+的游戏内容
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    // ====================
    // 游戏状态
    // ====================
    [Header("时间系统")]
    public int currentDay = 1;
    public string currentPhaseName = "晨";  // 晨/昼/昏/夜
    public int currentPhase = 0;  // 0-3
    public int actionPoints = 8;  // 每阶段8AP，32AP/天（SPEC v2.6）
    public int maxAPPerPhase = 8;

    [Header("资源")]
    public int food = 10;
    public int wood = 5;
    public int stone = 3;
    public int herb = 2;
    public int fiber = 1;
    public int ore = 0;
    public int bone = 0;
    public int soulEssence = 0;

    [Header("威胁系统")]
    public int threatLevel = 1;  // 1-5
    public int maxThreat = 5;
    public int fogDays = 0;  // 黑雾侵袭天数

    [Header("队伍")]
    public List<SquadMember> squad = new List<SquadMember>();
    public SquadMember selectedMember;

    [Header("地图")]
    public int currentRegion = 0;
    public Region[] regions;

    [Header("叙事")]
    public int memories = 0;  // 记忆碎片
    public List<string> storyFlags = new List<string>();
    public int storyPhase = 1;  // 1-4阶段
    public string gs = "normal";  // "normal" | "gameover" | "victory"

    // ====================
    // 系统引用
    // ====================
    public NPCMemorySystem memorySystem;
    public RecruitmentManager recruitmentSystem;
    public CraftingSystem craftingSystem;
    public CombatSystem combatSystem;

    // ====================
    // UI引用（通过场景内对象）
    // ====================
    public UnityEngine.UI.Text uiLog;
    public UnityEngine.UI.Text uiDay;
    public UnityEngine.UI.Text uiAP;
    public UnityEngine.UI.Text uiFood;
    public UnityEngine.UI.Text uiThreat;
    public UnityEngine.UI.Text uiRegion;

    private List<string> logMessages = new List<string>();

    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeRegions();
        InitializeStartingSquad();
    }

    void Start()
    {
        AddLog("=== 森林手账 ===");
        AddLog("你从迷雾中醒来...");
        AddLog("这里是被黑雾笼罩的古老森林。");
        AddLog("找到森林之心，是活下去的唯一希望。");
        AddLog("");
        AddLog("第 1 天 - 晨");
        UpdateAllUI();
    }

    // ====================
    // 初始化
    // ====================

    void InitializeRegions()
    {
        regions = new Region[]
        {
            new Region { id = 0, name = "迷雾森林", threat = 1, description = "起点，相对安全", discovered = true },
            new Region { id = 1, name = "黑雾前沿", threat = 2, description = "黑雾最浓的地方", discovered = false },
            new Region { id = 2, name = "古老废墟", threat = 3, description = "遗迹，有珍贵资源", discovered = false },
            new Region { id = 3, name = "幽暗山谷", threat = 4, description = "危险与机遇并存", discovered = false },
            new Region { id = 4, name = "森林之心", threat = 5, description = "终点的神秘区域", discovered = false }
        };
    }

    void InitializeStartingSquad()
    {
        // 初始2人队伍（SPEC v2.6）
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
        squad.Add(leaf);
        squad.Add(sen);
        selectedMember = leaf;
    }

    // ====================
    // 核心操作
    // ====================

    /// <summary>
    /// 采集资源
    /// </summary>
    public void Gather(string resourceType)
    {
        if (!CanTakeAction(2)) return;

        ConsumeAP(2);

        int amount = UnityEngine.Random.Range(1, 4);
        int bonus = GetGatherBonus();

        switch (resourceType.ToLower())
        {
            case "food":
            case "食物":
                food += amount + bonus;
                AddLog($"采集了 {amount + bonus} 个食物");
                break;
            case "wood":
            case "木材":
                wood += amount + bonus;
                AddLog($"采集了 {amount + bonus} 个木材");
                break;
            case "herb":
            case "草药":
                herb += amount + bonus;
                AddLog($"采集了 {amount + bonus} 个草药");
                break;
            case "stone":
            case "石材":
                stone += amount + bonus;
                AddLog($"采集了 {amount + bonus} 个石材");
                break;
        }

        // 触发任务采集通知
        var quest = FindObjectOfType<QuestSystem>();
        if (quest != null)
            quest.OnResourceGathered(resourceType.ToLower(), amount + bonus);

        // 危险事件
        if (UnityEngine.Random.value < 0.15f)
        {
            threatLevel = Mathf.Min(threatLevel + 1, maxThreat);
            AddLog("黑雾兽被惊动了！威胁+1");
        }

        // 探索发现事件（30%概率）
        var narrative = FindObjectOfType<DynamicNarrativeSystem>();
        if (narrative != null)
            narrative.TriggerDiscoveryEvent();

        UpdateAllUI();
    }

    /// <summary>
    /// 安营过夜
    /// </summary>
    public void Rest()
    {
        if (!CanTakeAction(4))
        {
            AddLog("行动点不足，无法安营！");
            return;
        }

        ConsumeAP(4);

        // 消耗食物
        int foodCost = squad.Count + 1;
        food -= foodCost;
        if (food < 0) food = 0;

        // 威胁波动
        int threatChange = UnityEngine.Random.Range(-1, 2);
        threatLevel = Mathf.Clamp(threatLevel + threatChange, 1, maxThreat);

        // 进入下一天
        AdvanceDay();

        AddLog($"消耗了 {foodCost} 食物");
        AddLog($"威胁等级: {threatLevel}");

        // 检查游戏结束
        if (food <= 0)
        {
            AddLog("");
            AddLog("=== 饥饿致死 ===");
            AddLog("你的队伍因为饥饿倒下了...");
            AddLog("旅程到此结束。");
            return;
        }

        // 随机事件
        TriggerRandomEvent();

        // 夜晚安营事件（叙事系统）
        var narrative = FindObjectOfType<DynamicNarrativeSystem>();
        if (narrative != null)
            narrative.OnNightCamp();

        UpdateAllUI();
    }

    /// <summary>
    /// 迁移到其他区域
    /// </summary>
    public void Migrate(int regionId)
    {
        if (regionId == currentRegion)
        {
            AddLog("你已经在这里了");
            return;
        }

        if (!CanTakeAction(3))
        {
            AddLog("行动点不足，无法迁移！");
            return;
        }

        var region = regions[regionId];
        if (!region.discovered)
        {
            AddLog($"发现新区域：{region.name}");
            region.discovered = true;
            memories += 2;
            AddLog("记忆碎片 +2");
        }

        ConsumeAP(3);
        currentRegion = regionId;
        threatLevel = Mathf.Min(threatLevel + region.threat, maxThreat);

        AddLog($"迁移到了 {region.name}");
        AddLog($"区域威胁: {region.threat}");

        // 区域事件
        TriggerRegionEvent(regionId);

        UpdateAllUI();
    }

    /// <summary>
    /// 战斗
    /// </summary>
    public void Combat()
    {
        if (!CanTakeAction(3)) return;

        // 夜晚禁止战斗
        if (currentPhase == 3)
        {
            AddLog("⚠️ 夜晚无法战斗，需要安营休息！");
            return;
        }

        ConsumeAP(3);

        // 优先使用手牌战斗系统
        var handCard = FindObjectOfType<HandCardUI>();
        var enemyAI = FindObjectOfType<EnemyAISystem>();
        if (handCard != null)
        {
            // 使用差异化AI生成敌人
            EnemyAISystem.EnemyInstance enemyInst;
            if (enemyAI != null)
            {
                enemyInst = enemyAI.CreateEnemy(threatLevel);
            }
            else
            {
                // Fallback: 简单敌人
                enemyInst = new EnemyAISystem.EnemyInstance
                {
                    name = "森林野兽",
                    hp = 30 + threatLevel * 10,
                    maxHP = 30 + threatLevel * 10,
                    atk = 8 + threatLevel * 2,
                    attackVariance = 3,
                    lootBonus = new[] { 1, 3 },
                    dropChance = 0.4f
                };
            }

            var enemyInfo = new HandCardUI.EnemyInfo
            {
                enemyName = enemyInst.name,
                hp = enemyInst.hp,
                maxHP = enemyInst.maxHP,
                attack = enemyInst.atk,
                foodReward = UnityEngine.Random.Range(2, 5),
                soulReward = enemyInst.lootBonus.Length > 1 ? UnityEngine.Random.Range(enemyInst.lootBonus[0], enemyInst.lootBonus[1] + 1) : 0
            };

            handCard.StartCombat(enemyInfo);
            AddLog($"{enemyInst.specialDesc}");

            if (enemyInst.isBoss) AddLog("⚠️ BOSS出现！");
            else if (enemyInst.isElite) AddLog("⚔️ 精英怪物！");

            return;
        }

        // Fallback: 简单战斗
        string[] simpleEnemies = { "阴影狼", "毒蜘蛛", "雾精灵", "黑雾兽", "变异树妖" };
        string enemy = simpleEnemies[UnityEngine.Random.Range(0, simpleEnemies.Length)];
        int enemyHP = UnityEngine.Random.Range(20, 50);
        int enemyATK = UnityEngine.Random.Range(8, 15);

        AddLog($"遭遇了 {enemy}！(HP:{enemyHP} ATK:{enemyATK})");

        int playerATK = selectedMember != null ? selectedMember.attack : 12;
        int playerDEF = selectedMember != null ? selectedMember.defense : 8;

        int damageToEnemy = Mathf.Max(1, playerATK - enemyHP / 5);
        int damageToPlayer = Mathf.Max(1, enemyATK - playerDEF / 2);

        enemyHP -= damageToEnemy;
        selectedMember.currentHealth -= damageToPlayer;

        AddLog($"战斗！你对敌人造成 {damageToEnemy} 伤害");
        AddLog($"敌人对你造成 {damageToPlayer} 伤害");

        if (enemyHP <= 0)
        {
            int foodReward = UnityEngine.Random.Range(2, 5);
            food += foodReward;
            soulEssence += UnityEngine.Random.Range(0, 2);
            memories += 1;
            AddLog($"胜利！获得 {foodReward} 食物");
            if (JournalSystem.instance != null)
                JournalSystem.instance.OnCombatVictory(enemy, foodReward, soulEssence);
        }
        else
        {
            AddLog("战斗陷入僵持...");
            threatLevel = Mathf.Min(threatLevel + 1, maxThreat);
        }

        // 死亡检查
        CheckDeathAndVictory();
        UpdateAllUI();
    }

    /// <summary>
    /// 检查游戏结束/胜利条件（SPEC v2.6）
    /// </summary>
    void CheckDeathAndVictory()
    {
        if (gs != "normal") return;

        // 队伍全灭 → GAME OVER
        bool allDead = true;
        foreach (var m in squad)
        {
            if (m.currentHealth > 0) { allDead = false; break; }
        }

        if (allDead)
        {
            gs = "gameover";
            AddLog("═══ 队伍全灭 ═══");
            AddLog("你们倒在了森林中...");
            AddLog("—— GAME OVER ——");
            if (JournalSystem.instance != null)
                JournalSystem.instance.Record("death", "队伍全灭", "所有人都倒在了黑雾笼罩的森林中...");
            return;
        }

        // 禁区（威胁5）持续掉血
        if (threatLevel >= 5)
        {
            foreach (var m in squad)
            {
                if (m.currentHealth > 0)
                {
                    m.currentHealth -= 5;
                    if (m.currentHealth <= 0)
                    {
                        m.currentHealth = 0;
                        m.status = "死亡";
                        AddLog($"⚠️ {m.memberName} 在禁区中倒下了！");
                    }
                }
            }
        }
    }

    /// <summary>
    /// 触发四种结局之一（到达森林之心后，Day 21+）
    /// </summary>
    void TriggerEnding()
    {
        if (currentDay < 21)
        {
            AddLog("森林之心尚未完全觉醒...");
            AddLog("需要等待第21天，黑雾最浓之时...");
            return;
        }

        // 使用 EndingsSystem 判定结局
        var endings = FindObjectOfType<EndingsSystem>();
        if (endings != null)
        {
            endings.TryTriggerEnding();
        }
        else
        {
            // Fallback: 简单结局
            gs = "victory";
            AddLog("═══ 到达森林之心 ═══");
            AddLog("你们找到了传说中的森林之心！");
            AddLog("在森林之心的光芒中，你们揭开了这片森林的秘密。");
            AddLog("—— 胜利！——");
        }
    }

    /// <summary>
    /// 使用物品
    /// </summary>
    public void UseItem(string itemType)
    {
        if (!CanTakeAction(1)) return;

        switch (itemType.ToLower())
        {
            case "food":
            case "食物":
                if (food > 0)
                {
                    food--;
                    selectedMember.currentHealth = Mathf.Min(selectedMember.currentHealth + 20, selectedMember.maxHealth);
                    AddLog("吃了食物，恢复 20 HP");
                }
                break;
            case "herb":
            case "草药":
                if (herb > 0)
                {
                    herb--;
                    selectedMember.currentHealth = Mathf.Min(selectedMember.currentHealth + 10, selectedMember.maxHealth);
                    AddLog("使用了草药，恢复 10 HP");
                }
                break;
        }

        ConsumeAP(1);
        UpdateAllUI();
    }

    /// <summary>
    /// 与NPC对话/招募
    /// </summary>
    public void TalkToNPC()
    {
        if (!CanTakeAction(2)) return;

        ConsumeAP(2);

        // 检查队伍是否已满（最多4人）
        if (squad.Count >= 4)
        {
            AddLog("队伍已满（最多4人）");
            return;
        }

        // 使用对话树系统触发对话
        var dialogueSystem = FindObjectOfType<DialogueTreeSystem>();
        var npcSystem = FindObjectOfType<NPCRecruitmentSystem>();
        if (npcSystem != null)
        {
            // 先随机遭遇一个NPC
            npcSystem.TriggerNPCEncounter();
        }

        // 如果对话树有该NPC的对话，触发对话
        if (dialogueSystem != null && npcSystem != null)
        {
            var available = npcSystem.GetAvailableNPCs(currentDay, memories);
            if (available.Count > 0)
            {
                var npc = available[UnityEngine.Random.Range(0, available.Count)];
                dialogueSystem.StartDialogue(npc.npcId);
                return;
            }
        }

        // Fallback：简单探索
        AddLog("在森林中探索...");
        UpdateAllUI();
    }

    // ====================
    // 时间系统
    // ====================

    void AdvanceDay()
    {
        currentDay++;
        currentPhase = 0;
        actionPoints = maxAPPerPhase;

        string[] phases = { "晨", "昼", "昏", "夜" };
        currentPhaseName = phases[currentPhase];

        AddLog("");
        AddLog($"=== 第 {currentDay} 天 - {currentPhaseName} ===");

        // 叙事阶段推进
        if (currentDay == 7 && storyPhase == 1)
        {
            storyPhase = 2;
            AddLog("故事进入新阶段：壮大");
            memories += 5;
            AddLog("记忆碎片 +5（剧情解锁）");
        }
        else if (currentDay == 14 && storyPhase == 2)
        {
            storyPhase = 3;
            AddLog("故事进入新阶段：探索");
            memories += 5;
        }
        else if (currentDay == 21 && storyPhase == 3)
        {
            storyPhase = 4;
            AddLog("故事进入最终阶段：归心");
            memories += 10;
            AddLog("记忆碎片 +10（发现森林之心线索）");
        }

        // 检查胜利条件
        if (currentRegion == 4 && storyPhase >= 4)
        {
            AddLog("");
            AddLog("=== 抵达森林之心 ===");
            AddLog("你终于来到了森林最深处...");
            TriggerEnding();
        }

        // 每日食物消耗（SPEC v2.6：基础1食物/成员 - 火堆减免）
        var skills = FindObjectOfType<SkillsSystem>();
        int baseFood = squad.Count;  // 每成员每天1食物
        int reduction = skills != null ? skills.GetDailyFoodReduction() : 0;
        int dailyFood = Mathf.Max(0, baseFood - reduction);

        if (food >= dailyFood)
        {
            food -= dailyFood;
            AddLog($"每日消耗 {dailyFood} 食物（基础{baseFood} - 火堆减免{reduction}）");
        }
        else
        {
            // 饥饿惩罚
            foreach (var m in squad)
            {
                if (m.currentHealth > 0)
                {
                    m.currentHealth -= 10;
                    AddLog($"⚠️ {m.memberName} 饥饿！-10 HP");
                }
            }
            threatLevel = Mathf.Min(threatLevel + 1, maxThreat);
            AddLog("⚠️ 食物不足，威胁等级上升！");
        }

        // 每日技能检查
        if (skills != null)
            skills.CheckDailySkillUnlocks(currentDay, squad);

        // NPC 每日行为
        var rel = FindObjectOfType<RelationshipSystem>();
        if (rel != null)
            rel.OnDayStart();

        // 每日任务生成
        var quest = FindObjectOfType<QuestSystem>();
        if (quest != null)
            quest.GenerateDailyQuests();

        // 每日威胁增长（每3天+1）
        var narrative = FindObjectOfType<DynamicNarrativeSystem>();
        if (narrative != null)
        {
            narrative.CheckThreatIncrease(currentDay);
            narrative.CheckChapterUnlocks(currentDay);
            narrative.CheckMemoryAwakening(memories);
        }

        // 制作系统每日效果
        var crafting = FindObjectOfType<CraftingSystem>();
        if (crafting != null)
        {
            // 治愈笛：每日自动治疗
            int dailyHeal = crafting.GetDailyHeal();
            if (dailyHeal > 0)
            {
                foreach (var m in squad)
                {
                    if (m.currentHealth > 0 && m.currentHealth < m.maxHealth)
                    {
                        m.currentHealth = Mathf.Min(m.currentHealth + dailyHeal, m.maxHealth);
                    }
                }
                if (dailyHeal > 0) AddLog($"治愈笛效果：全员恢复 {dailyHeal} HP");
            }

            // 森林斗篷：威胁减免
            int threatReduction = crafting.GetThreatReduction();
            if (threatLevel > 1 && threatReduction > 0)
            {
                threatLevel--;
                AddLog($"森林斗篷效果：威胁等级-1（当前 {threatLevel}）");
            }
        }

        // 触发手账记录
        var journal = FindObjectOfType<JournalSystem>();
        if (journal != null)
            journal.Record("day", $"第 {currentDay} 天", $"新的一天开始了。队伍现在有 {squad.Count} 名成员。");

        UpdateAllUI();
    }

    void TriggerRandomEvent()
    {
        float roll = UnityEngine.Random.value;

        if (roll < 0.1f)
        {
            // 好事
            int bonus = UnityEngine.Random.Range(3, 8);
            food += bonus;
            AddLog($"在营地周围发现了 {bonus} 个浆果！");
        }
        else if (roll < 0.2f)
        {
            // 找到物品
            herb += UnityEngine.Random.Range(1, 4);
            AddLog("采集到了一些草药！");
        }
        else if (roll < 0.3f)
        {
            // 坏事
            int loss = Mathf.Min(food, UnityEngine.Random.Range(1, 3));
            food -= loss;
            AddLog($"野兽偷吃了 {loss} 个食物...");
        }
    }

    void TriggerRegionEvent(int regionId)
    {
        if (regionId == 4 && storyPhase >= 4)
        {
            // 森林之心事件
            AddLog("");
            AddLog("你感受到了森林之心的召唤...");
        }
    }

    void TriggerEnding()
    {
        // 根据记忆碎片数量决定结局
        if (memories >= 50)
        {
            AddLog("");
            AddLog("=== 真结局：森林的守护者 ===");
            AddLog("你收集了足够的记忆，理解了森林的意志。");
            AddLog("黑雾消散，森林重现生机。");
            AddLog("");
            AddLog("你和你的同伴们成为了新的森林守护者。");
        }
        else if (memories >= 30)
        {
            AddLog("");
            AddLog("=== 结局：森林之心 ===");
            AddLog("你找到了森林之心，但黑雾还未完全消散。");
            AddLog("你选择留下，继续守护这片森林...");
        }
        else
        {
            AddLog("");
            AddLog("=== 结局：迷失 ===");
            AddLog("你没有收集足够的记忆，无法理解森林的意志。");
            AddLog("黑雾吞没了一切...");
        }

        AddLog("");
        AddLog("=== 游戏结束 ===");
        AddLog($"最终天数: {currentDay}");
        AddLog($"记忆碎片: {memories}");
    }

    // ====================
    // 辅助方法
    // ====================

    bool CanTakeAction(int cost)
    {
        if (gs != "normal") { AddLog("游戏已结束"); return false; }
        return actionPoints >= cost;
    }

    void ConsumeAP(int cost)
    {
        actionPoints -= cost;

        // AP耗尽，进入下个阶段
        while (actionPoints <= 0 && currentPhase < 3)
        {
            currentPhase++;
            actionPoints = maxAPPerPhase;
            string[] phases = { "晨", "昼", "昏", "夜" };
            currentPhaseName = phases[currentPhase];
            AddLog($"进入了 {currentPhaseName} 阶段");
        }
    }

    int GetGatherBonus()
    {
        // 草药师加成
        foreach (var m in squad)
        {
            if (m.role == "草药师")
                return UnityEngine.Random.Range(1, 3);
        }
        return 0;
    }

    public void AddLog(string msg)
    {
        logMessages.Add(msg);
        if (logMessages.Count > 12)
            logMessages.RemoveAt(0);

        if (uiLog != null)
            uiLog.text = string.Join("\n", logMessages);
    }

    void UpdateAllUI()
    {
        if (uiDay) uiDay.text = $"第 {currentDay} 天 - {currentPhaseName}";
        if (uiAP) uiAP.text = $"AP: {actionPoints}/{maxAPPerPhase}";
        if (uiFood) uiFood.text = $"食物: {food}";
        if (uiThreat) uiThreat.text = $"威胁: {threatLevel}/{maxThreat}";
        if (uiRegion) uiRegion.text = $"位置: {regions[currentRegion].name}";
    }

    // ====================
    // 公开API（供UI按钮调用）
    // ====================

    public void BtnGatherFood() { Gather("food"); }
    public void BtnGatherWood() { Gather("wood"); }
    public void BtnGatherHerb() { Gather("herb"); }
    public void BtnRest() { Rest(); }
    public void BtnCombat() { Combat(); }
    public void BtnTalk() { TalkToNPC(); }
    public void BtnMigrate0() { Migrate(0); }
    public void BtnMigrate1() { Migrate(1); }
    public void BtnMigrate2() { Migrate(2); }
    public void BtnMigrate3() { Migrate(3); }
    public void BtnMigrate4() { Migrate(4); }

    /// <summary>
    /// 打开手账（供UI按钮调用）
    /// </summary>
    public void BtnJournal()
    {
        var journal = FindObjectOfType<JournalSystem>();
        if (journal != null)
            journal.ShowJournal();
        else
            AddLog("手账系统暂不可用");
    }

    /// <summary>
    /// 重新开始游戏
    /// </summary>
    public void BtnRestart()
    {
        if (gs == "normal") return;
        // 重置游戏状态
        currentDay = 1;
        currentPhase = 0;
        actionPoints = maxAPPerPhase;
        food = 10;
        wood = 5;
        stone = 3;
        herb = 2;
        fiber = 1;
        ore = 0;
        bone = 0;
        soulEssence = 0;
        threatLevel = 1;
        memories = 0;
        gs = "normal";
        storyPhase = 1;
        currentRegion = 0;

        foreach (var m in squad)
        {
            m.currentHealth = m.maxHealth;
            m.status = "正常";
        }

        logMessages.Clear();
        AddLog("=== 森林手账 ===");
        AddLog("你从迷雾中醒来...");
        AddLog("这里是黑雾笼罩的古老森林。");
        AddLog("找到森林之心，是活下去的唯一希望。");
        AddLog("");
        AddLog("第 1 天 - 晨");
        UpdateAllUI();
    }
}

/// <summary>
/// 区域数据
/// </summary>
[System.Serializable]
public class Region
{
    public int id;
    public string name;
    public int threat;
    public string description;
    public bool discovered;
}

/// <summary>
/// 队伍成员
/// </summary>
[System.Serializable]
public class SquadMember
{
    public string memberId;
    public string memberName;
    public string role;
    public int maxHealth;
    public int currentHealth;
    public int attack;
    public int defense;
    public int intelligence;
    public int agility;
    public string status;  // 正常/受伤/中毒/睡眠
}
