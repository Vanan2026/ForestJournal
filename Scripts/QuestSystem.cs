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
        // 使用扩展任务库
        var extended = ExtendedQuestLibrary.GenerateAllQuests();
        foreach (var q in extended)
        {
            questPool.Add(new Quest {
                id = q.id,
                name = q.name,
                description = q.description,
                type = q.type,
                difficulty = q.difficulty,
                objectives = q.objectives.ConvertAll(o => new QuestObjective {
                    type = o.type,
                    targetId = o.targetId,
                    count = o.count,
                    current = 0
                }),
                rewards = q.rewards,
                choices = q.choices?.ConvertAll(c => new QuestChoice {
                    id = c.id,
                    text = c.text,
                    reward = c.reward,
                    cost = c.cost,
                    trustNPC = c.trustNPC,
                    trustChange = c.trustChange,
                    requirement = c.requirement
                }),
                chainNextQuestId = q.chainNextQuestId
            });
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
