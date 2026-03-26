using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 支线任务系统 v1.0
/// 悬赏任务 + 随机事件任务 + 委托任务
/// </summary>
public class QuestSystem : MonoBehaviour
{
    public static QuestSystem instance { get; private set; }

    [Header("当前任务")]
    public List<Quest> activeQuests = new List<Quest>();
    public Quest currentQuest = null;

    [Header("已完成任务")]
    public List<Quest> completedQuests = new List<Quest>();

    [Header("任务库")]
    public List<Quest> questPool = new List<Quest>();

    [Header("每日生成上限")]
    public int maxDailyQuests = 3;
    public int questsGeneratedToday = 0;

    void Awake()
    {
        instance = this;
        InitializeQuestPool();
    }

    // ====================
    // 任务库初始化
    // ====================

    void InitializeQuestPool()
    {
        questPool = new List<Quest>
        {
            // ========== 悬赏任务 ==========
            new Quest {
                id = "bounty_wolf",
                name = "狼群威胁",
                description = "黑雾森林边缘出现了狼群，伤了三名旅人。消灭5只阴影狼。",
                type = QuestType.Bounty,
                difficulty = 1,
                objectives = new List<QuestObjective> {
                    new QuestObjective { type = ObjectiveType.KillEnemy, targetId = "shadow_wolf", count = 5, current = 0 }
                },
                rewards = new QuestReward { food = 10, soulEssence = 1, memories = 1 }
            },
            new Quest {
                id = "bounty_spider",
                name = "毒蜘蛛巢穴",
                description = "沼泽地带的毒蜘蛛开始向外扩散，在沼泽击杀4只毒蜘蛛。",
                type = QuestType.Bounty,
                difficulty = 2,
                objectives = new List<QuestObjective> {
                    new QuestObjective { type = ObjectiveType.KillEnemy, targetId = "poison_spider", count = 4, current = 0 }
                },
                rewards = new QuestReward { herb = 8, memories = 1 }
            },
            new Quest {
                id = "bounty_artifact",
                name = "遗失的宝物",
                description = "有人说在古老废墟里埋着一个古老的遗物，找回来。",
                type = QuestType.Bounty,
                difficulty = 2,
                objectives = new List<QuestObjective> {
                    new QuestObjective { type = ObjectiveType.ExploreRegion, targetId = "ancient_ruins", count = 1, current = 0 }
                },
                rewards = new QuestReward { food = 15, bone = 5, memories = 2 }
            },
            new Quest {
                id = "bounty_fog_beast",
                name = "黑雾兽清剿",
                description = "黑雾前沿出现了一头巨大的黑雾兽，据说杀死它能获得大量魂精华。",
                type = QuestType.Bounty,
                difficulty = 4,
                objectives = new List<QuestObjective> {
                    new QuestObjective { type = ObjectiveType.KillEnemy, targetId = "darkbeast", count = 1, current = 0 }
                },
                rewards = new QuestReward { soulEssence = 5, memories = 3 }
            },
            new Quest {
                id = "bounty_fog_lord",
                name = "雾主讨伐",
                description = "黑雾前沿深处出现了一个自称'雾主'的存在，带领队伍击败它。",
                type = QuestType.Bounty,
                difficulty = 5,
                objectives = new List<QuestObjective> {
                    new QuestObjective { type = ObjectiveType.KillEnemy, targetId = "fog_lord", count = 1, current = 0 }
                },
                rewards = new QuestReward { soulEssence = 10, memories = 5 }
            },

            // ========== 委托任务 ==========
            new Quest {
                id = "errand_food",
                name = "粮食危机",
                description = "队伍里的食物不多了，采集15个食物。",
                type = QuestType.Errand,
                difficulty = 1,
                objectives = new List<QuestObjective> {
                    new QuestObjective { type = ObjectiveType.GatherResource, targetId = "food", count = 15, current = 0 }
                },
                rewards = new QuestReward { memories = 1 }
            },
            new Quest {
                id = "errand_herb",
                name = "草药补给",
                description = "玛莎需要草药来制作药品，带10个草药给她。",
                type = QuestType.Errand,
                difficulty = 1,
                objectives = new List<QuestObjective> {
                    new QuestObjective { type = ObjectiveType.GatherResource, targetId = "herb", count = 10, current = 0 }
                },
                rewards = new QuestReward { herb = 5, memories = 1 }
            },
            new Quest {
                id = "errand_camp",
                name = "修建营地",
                description = "需要5个木材来修建新的营地。",
                type = QuestType.Errand,
                difficulty = 1,
                objectives = new List<QuestObjective> {
                    new QuestObjective { type = ObjectiveType.GatherResource, targetId = "wood", count = 5, current = 0 }
                },
                rewards = new QuestReward { wood = 5, memories = 1 }
            },
            new Quest {
                id = "errand_bandage",
                name = "绷带订单",
                description = "有个商人需要绷带，用纤维和草药制作8个绷带。",
                type = QuestType.Errand,
                difficulty = 2,
                objectives = new List<QuestObjective> {
                    new QuestObjective { type = ObjectiveType.CraftItem, targetId = "bandage", count = 8, current = 0 }
                },
                rewards = new QuestReward { food = 12, memories = 1 }
            },
            new Quest {
                id = "errand_explorer",
                name = "区域探索",
                description = "探索幽暗山谷，寻找新的道路。",
                type = QuestType.Errand,
                difficulty = 2,
                objectives = new List<QuestObjective> {
                    new QuestObjective { type = ObjectiveType.ExploreRegion, targetId = "dark_valley", count = 1, current = 0 }
                },
                rewards = new QuestReward { memories = 2, herb = 5 }
            },

            // ========== 随机事件任务 ==========
            new Quest {
                id = "event_lost_child",
                name = "迷路的孩子",
                description = "在路边发现了一个哭泣的孩子，帮他找到回家的路。",
                type = QuestType.Event,
                difficulty = 1,
                objectives = new List<QuestObjective> {
                    new QuestObjective { type = ObjectiveType.Choice, targetId = "help_child", count = 1, current = 0 }
                },
                rewards = new QuestReward { memories = 2 },
                choices = new List<QuestChoice> {
                    new QuestChoice { id = "help_find", text = "帮助孩子找家", reward = new QuestReward { memories = 2 } },
                    new QuestChoice { id = "ignore_child", text = "假装没看见", reward = new QuestReward { memories = -1 } }
                }
            },
            new Quest {
                id = "event_stranger",
                name = "神秘旅人",
                description = "一个神秘的旅人想用物品交换情报，你会怎么做？",
                type = QuestType.Event,
                difficulty = 1,
                objectives = new List<QuestObjective> {
                    new QuestObjective { type = ObjectiveType.Choice, targetId = "stranger_deal", count = 1, current = 0 }
                },
                rewards = new QuestReward { memories = 0 },
                choices = new List<QuestChoice> {
                    new QuestChoice { id = "trade_info", text = "用3食物换取情报", reward = new QuestReward { memories = 3 }, cost = new QuestCost { food = 3 } },
                    new QuestChoice { id = "share_free", text = "免费分享情报", reward = new QuestReward { memories = 2 }, trustNPC = "stranger", trustChange = 10 },
                    new QuestChoice { id = "refuse_stranger", text = "拒绝交易", reward = new QuestReward { memories = 0 } }
                }
            },
            new Quest {
                id = "event_injured_soldier",
                name = "受伤的士兵",
                description = "路边发现一名受伤的士兵，他说被黑雾兽袭击了。",
                type = QuestType.Event,
                difficulty = 2,
                objectives = new List<QuestObjective> {
                    new QuestObjective { type = ObjectiveType.Choice, targetId = "soldier", count = 1, current = 0 }
                },
                choices = new List<QuestChoice> {
                    new QuestChoice { id = "heal_soldier", text = "用草药治疗他", reward = new QuestReward { memories = 2, soulEssence = 1 }, cost = new QuestCost { herb = 5 } },
                    new QuestChoice { id = "abandon_soldier", text = "不关我的事", reward = new QuestReward { memories = -1 } }
                }
            },
            new Quest {
                id = "event_abandoned_village",
                name = "废弃村落",
                description = "探索时发现了一个废弃的村落，里面似乎还有物资。",
                type = QuestType.Event,
                difficulty = 2,
                objectives = new List<QuestObjective> {
                    new QuestObjective { type = ObjectiveType.Choice, targetId = "village", count = 1, current = 0 }
                },
                choices = new List<QuestChoice> {
                    new QuestChoice { id = "search_village", text = "仔细搜索村落", reward = new QuestReward { food = 8, wood = 5, stone = 3 } },
                    new QuestChoice { id = "quick_loot", text = "快速搜刮", reward = new QuestReward { food = 3, wood = 2 } },
                    new QuestChoice { id = "ignore_village", text = "不进去，太危险", reward = new QuestReward { memories = 1 } }
                }
            },
            new Quest {
                id = "event_elder_wisdom",
                name = "老者的智慧",
                description = "森林深处住着一位老者，据说知道黑雾的秘密。",
                type = QuestType.Event,
                difficulty = 3,
                objectives = new List<QuestObjective> {
                    new QuestObjective { type = ObjectiveType.Choice, targetId = "elder", count = 1, current = 0 }
                },
                choices = new List<QuestChoice> {
                    new QuestChoice { id = "visit_elder", text = "拜访老者", reward = new QuestReward { memories = 5, soulEssence = 2 }, requirement = new QuestRequirement { minDay = 10 } },
                    new QuestChoice { id = "bring_gift", text = "带礼物拜访（消耗5食物）", reward = new QuestReward { memories = 3 }, cost = new QuestCost { food = 5 }, requirement = new QuestRequirement { minDay = 5 } },
                    new QuestChoice { id = "ignore_elder", text = "不感兴趣", reward = new QuestReward { memories = 0 } }
                }
            },

            // ========== 连续任务（多步骤）==========
            new Quest {
                id = "chain_ashes_intro",
                name = "灰烬的踪迹（前篇）",
                description = "有人说在黑雾中看到了一个神秘的身影，追查下去。",
                type = QuestType.Chain,
                difficulty = 3,
                chainNextQuestId = "chain_ashes_truth",
                objectives = new List<QuestObjective> {
                    new QuestObjective { type = ObjectiveType.ExploreRegion, targetId = "fog_front", count = 1, current = 0 }
                },
                rewards = new QuestReward { memories = 2 }
            },
            new Quest {
                id = "chain_ashes_truth",
                name = "灰烬的踪迹（后篇）",
                description = "你找到了那个神秘人——灰烬。他似乎在寻找什么。",
                type = QuestType.Chain,
                difficulty = 3,
                objectives = new List<QuestObjective> {
                    new QuestObjective { type = ObjectiveType.Choice, targetId = "ashes_choice", count = 1, current = 0 }
                },
                rewards = new QuestReward { memories = 5, soulEssence = 3 }
            },
            new Quest {
                id = "chain_forest_heart_1",
                name = "森林之心（序章）",
                description = "收集5个魂精华，献给森林之心。",
                type = QuestType.Chain,
                difficulty = 4,
                chainNextQuestId = "chain_forest_heart_2",
                objectives = new List<QuestObjective> {
                    new QuestObjective { type = ObjectiveType.CollectResource, targetId = "soulEssence", count = 5, current = 0 }
                },
                rewards = new QuestReward { memories = 5 }
            },
            new Quest {
                id = "chain_forest_heart_2",
                name = "森林之心（觉醒）",
                description = "前往森林深处，与森林之心产生共鸣。",
                type = QuestType.Chain,
                difficulty = 5,
                chainNextQuestId = "chain_forest_heart_ending",
                objectives = new List<QuestObjective> {
                    new QuestObjective { type = ObjectiveType.ReachLocation, targetId = "forest_heart", count = 1, current = 0 }
                },
                rewards = new QuestReward { memories = 10 }
            }
        };
    }

    // ====================
    // 每日任务生成
    // ====================

    /// <summary>
    /// 每日生成新任务（安营时调用）
    /// </summary>
    public void GenerateDailyQuests()
    {
        questsGeneratedToday = 0;

        // 先检查是否有连续任务未完成
        foreach (var q in activeQuests)
        {
            if (q.type == QuestType.Chain && !string.IsNullOrEmpty(q.chainNextQuestId))
            {
                // 自动接受连续任务的下一步
                AcceptChainQuest(q.chainNextQuestId);
            }
        }

        // 生成悬赏和委托
        int bountyCount = UnityEngine.Random.Range(1, 3);
        int errandCount = UnityEngine.Random.Range(1, 2);

        GenerateQuestsOfType(QuestType.Bounty, bountyCount);
        GenerateQuestsOfType(QuestType.Errand, errandCount);

        // 30%概率触发随机事件
        if (UnityEngine.Random.value < 0.3f)
        {
            GenerateQuestsOfType(QuestType.Event, 1);
        }

        if (activeQuests.Count > 0)
        {
            string questList = "";
            foreach (var q in activeQuests)
                questList += $"[{q.name}] ";
            GameManager.instance?.AddLog($"═══ 今日任务 ═══ {questList}");
        }
    }

    void GenerateQuestsOfType(QuestType type, int count)
    {
        var pool = questPool.FindAll(q => q.type == type && !IsQuestActive(q.id) && !IsQuestCompleted(q.id));

        for (int i = 0; i < count && pool.Count > 0 && questsGeneratedToday < maxDailyQuests; i++)
        {
            int idx = UnityEngine.Random.Range(0, pool.Count);
            var quest = pool[idx];
            activeQuests.Add(quest);
            pool.RemoveAt(idx);
            questsGeneratedToday++;
        }
    }

    void AcceptChainQuest(string nextQuestId)
    {
        if (IsQuestActive(nextQuestId) || IsQuestCompleted(nextQuestId)) return;
        var next = questPool.Find(q => q.id == nextQuestId);
        if (next != null) activeQuests.Add(next);
    }

    bool IsQuestActive(string id) => activeQuests.Exists(q => q.id == id);
    bool IsQuestCompleted(string id) => completedQuests.Exists(q => q.id == id);

    // ====================
    // 任务进度更新
    // ====================

    /// <summary>
    /// 击杀敌人时调用
    /// </summary>
    public void OnEnemyKilled(string enemyType)
    {
        foreach (var quest in activeQuests)
        {
            foreach (var obj in quest.objectives)
            {
                if (obj.type == ObjectiveType.KillEnemy && obj.targetId == enemyType && obj.current < obj.count)
                {
                    obj.current++;
                    NotifyProgress(quest, obj);
                    CheckQuestComplete(quest);
                }
            }
        }
    }

    /// <summary>
    /// 采集资源时调用
    /// </summary>
    public void OnResourceGathered(string resourceType, int amount)
    {
        foreach (var quest in activeQuests)
        {
            foreach (var obj in quest.objectives)
            {
                if (obj.type == ObjectiveType.GatherResource && obj.targetId == resourceType)
                {
                    obj.current = Mathf.Min(obj.current + amount, obj.count);
                    NotifyProgress(quest, obj);
                    CheckQuestComplete(quest);
                }
            }
        }
    }

    /// <summary>
    /// 制作物品时调用
    /// </summary>
    public void OnItemCrafted(string itemId)
    {
        foreach (var quest in activeQuests)
        {
            foreach (var obj in quest.objectives)
            {
                if (obj.type == ObjectiveType.CraftItem && obj.targetId == itemId && obj.current < obj.count)
                {
                    obj.current++;
                    NotifyProgress(quest, obj);
                    CheckQuestComplete(quest);
                }
            }
        }
    }

    /// <summary>
    /// 探索区域时调用
    /// </summary>
    public void OnRegionExplored(int regionId)
    {
        string[] regionIds = { "mist_forest", "fog_front", "ancient_ruins", "dark_valley", "forest_heart" };
        if (regionId < 0 || regionId >= regionIds.Length) return;

        foreach (var quest in activeQuests)
        {
            foreach (var obj in quest.objectives)
            {
                if (obj.type == ObjectiveType.ExploreRegion && obj.targetId == regionIds[regionId])
                {
                    obj.current = 1;
                    NotifyProgress(quest, obj);
                    CheckQuestComplete(quest);
                }
            }
        }
    }

    void NotifyProgress(Quest quest, QuestObjective obj)
    {
        GameManager.instance?.AddLog($"[任务进度] {quest.name}：{obj.current}/{obj.count}");
    }

    // ====================
    // 任务完成
    // ====================

    void CheckQuestComplete(Quest quest)
    {
        bool allDone = true;
        foreach (var obj in quest.objectives)
        {
            if (obj.current < obj.count) { allDone = false; break; }
        }

        if (allDone)
        {
            CompleteQuest(quest);
        }
    }

    void CompleteQuest(Quest quest)
    {
        GameManager.instance?.AddLog($"═══ 任务完成：{quest.name} ═══");
        GameManager.instance?.AddLog("奖励：");

        if (quest.rewards.food > 0) GameManager.instance.AddLog($"  🍎 食物 +{quest.rewards.food}");
        if (quest.rewards.wood > 0) GameManager.instance.AddLog($"  🪵 木材 +{quest.rewards.wood}");
        if (quest.rewards.herb > 0) GameManager.instance.AddLog($"  🌿 草药 +{quest.rewards.herb}");
        if (quest.rewards.soulEssence > 0) GameManager.instance.AddLog($"  ✨ 魂精华 +{quest.rewards.soulEssence}");
        if (quest.rewards.memories > 0) GameManager.instance.AddLog($"  📖 记忆碎片 +{quest.rewards.memories}");

        // 应用奖励
        var gm = GameManager.instance;
        if (gm != null)
        {
            gm.food += quest.rewards.food;
            gm.wood += quest.rewards.wood;
            gm.herb += quest.rewards.herb;
            gm.soulEssence += quest.rewards.soulEssence;
            gm.memories += quest.rewards.memories;
        }

        // 移动到已完成
        activeQuests.Remove(quest);
        completedQuests.Add(quest);

        // 触发连续任务
        if (!string.IsNullOrEmpty(quest.chainNextQuestId))
        {
            var next = questPool.Find(q => q.id == quest.chainNextQuestId);
            if (next != null)
            {
                activeQuests.Add(next);
                GameManager.instance?.AddLog($"═══ 新任务：{next.name} ═══");
                GameManager.instance?.AddLog(next.description);
            }
        }

        // 触发视觉特效
        var fx = FindObjectOfType<VisualFXSystem>();
        if (fx != null)
            fx.ShowMemoryAwakeningEffect();
    }

    /// <summary>
    /// 任务选择（事件型任务）
    /// </summary>
    public void MakeQuestChoice(Quest quest, string choiceId)
    {
        if (quest.choices == null) return;
        var choice = quest.choices.Find(c => c.id == choiceId);
        if (choice == null) return;

        // 检查前置条件
        if (choice.requirement != null)
        {
            if (choice.requirement.minDay > 0 && (GameManager.instance?.currentDay ?? 1) < choice.requirement.minDay)
            {
                GameManager.instance?.AddLog($"需要第{choice.requirement.minDay}天后才能执行此选项...");
                return;
            }
        }

        // 消耗资源
        if (choice.cost != null && GameManager.instance != null)
        {
            if (choice.cost.food > 0 && GameManager.instance.food < choice.cost.food)
            {
                GameManager.instance.AddLog("资源不足！");
                return;
            }
            if (choice.cost.food > 0) GameManager.instance.food -= choice.cost.food;
        }

        // 应用奖励
        if (choice.reward != null)
        {
            var gm = GameManager.instance;
            if (gm != null)
            {
                gm.food += choice.reward.food;
                gm.wood += choice.reward.wood;
                gm.herb += choice.reward.herb;
                gm.soulEssence += choice.reward.soulEssence;
                gm.memories += choice.reward.memories;
            }
        }

        // NPC信任变化
        if (!string.IsNullOrEmpty(choice.trustNPC) && choice.trustChange != 0)
        {
            var rel = FindObjectOfType<RelationshipSystem>();
            rel?.RecordInteraction(choice.trustNPC, RelationshipSystem.InteractionType.Conversation,
                $"任务选择：{choice.text}");
        }

        // 标记选择完成
        foreach (var obj in quest.objectives)
        {
            if (obj.type == ObjectiveType.Choice) obj.current = 1;
        }

        GameManager.instance?.AddLog($"选择了：{choice.text}");

        // 完成任务
        CompleteQuest(quest);
    }

    // ====================
    // UI 显示
    // ====================

    /// <summary>
    /// 显示任务列表
    /// </summary>
    public void ShowQuestLog()
    {
        if (GameManager.instance == null) return;

        GameManager.instance.AddLog("═══ 任务列表 ═══");

        if (activeQuests.Count == 0)
        {
            GameManager.instance.AddLog("（无进行中的任务）");
            return;
        }

        foreach (var q in activeQuests)
        {
            string progress = "";
            foreach (var obj in q.objectives)
            {
                progress += $"{obj.current}/{obj.count} ";
            }
            GameManager.instance.AddLog($"[{q.type}]{q.name} {progress}");
            GameManager.instance.AddLog($"  {q.description}");

            if (q.choices != null && q.choices.Count > 0)
            {
                for (int i = 0; i < q.choices.Count; i++)
                {
                    var c = q.choices[i];
                    string costStr = c.cost != null && c.cost.food > 0 ? $"(消耗{c.cost.food}食物)" : "";
                    GameManager.instance.AddLog($"  [{i + 1}] {c.text} {costStr}");
                }
            }
        }

        GameManager.instance.AddLog($"═══ 已完成任务：{completedQuests.Count} ═══");
    }

    // ====================
    // 数据类
    // ====================

    public enum QuestType { Bounty, Errand, Event, Chain }
    public enum ObjectiveType { KillEnemy, GatherResource, CraftItem, ExploreRegion, ReachLocation, Choice }

    [System.Serializable]
    public class Quest
    {
        public string id;
        public string name;
        public string description;
        public QuestType type;
        public int difficulty;  // 1-5
        public List<QuestObjective> objectives;
        public QuestReward rewards;
        public List<QuestChoice> choices;  // 事件任务的选择
        public string chainNextQuestId;  // 连续任务下一步ID
    }

    [System.Serializable]
    public class QuestObjective
    {
        public ObjectiveType type;
        public string targetId;
        public int count;
        public int current;
    }

    [System.Serializable]
    public class QuestReward
    {
        public int food, wood, herb, stone, soulEssence;
        public int memories;
    }

    [System.Serializable]
    public class QuestChoice
    {
        public string id;
        public string text;
        public QuestReward reward;
        public QuestCost cost;
        public string trustNPC;
        public int trustChange;
        public QuestRequirement requirement;
    }

    [System.Serializable]
    public class QuestCost
    {
        public int food, herb, wood;
    }

    [System.Serializable]
    public class QuestRequirement
    {
        public int minDay;
        public int minMemories;
    }
}
