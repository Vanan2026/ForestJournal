using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// NPC 关系与好感度系统 v1.0
/// 好感度/仇恨度 + NPC间关系 + 记忆影响对话 + NPC日常行为
/// </summary>
public class RelationshipSystem : MonoBehaviour
{
    public static RelationshipSystem instance { get; private set; }

    [Header("所有NPC关系")]
    public Dictionary<string, NPCRelationship> relationships = new Dictionary<string, NPCRelationship>();

    [Header("NPC间关系")]
    public Dictionary<string, Dictionary<string, float>> npcBonds = new Dictionary<string, Dictionary<string, float>>();

    [Header("NPC日常行为")]
    public List<NPCDailyAction> dailyActions = new List<NPCDailyAction>();

    void Awake()
    {
        instance = this;
    }

    // ====================
    // 关系数据
    // ====================

    /// <summary>
    /// 记录与NPC的交互（对话/赠送/战斗/帮助）
    /// </summary>
    public void RecordInteraction(string npcId, InteractionType type, string context = "")
    {
        if (!relationships.ContainsKey(npcId))
        {
            relationships[npcId] = new NPCRelationship { npcId = npcId };
            npcBonds[npcId] = new Dictionary<string, float>();
        }

        var rel = relationships[npcId];
        rel.timesMet++;

        float change = GetInteractionChange(type);
        rel.trustLevel = Mathf.Clamp(rel.trustLevel + change, -100, 100);

        rel.interactionHistory.Add(new InteractionRecord
        {
            type = type,
            day = GameManager.instance?.currentDay ?? 1,
            context = context,
            trustChange = change
        });

        // 触发记忆
        if (JournalSystem.instance != null)
        {
            string desc = $"与 {npcId} {GetInteractionDesc(type)}" +
                (string.IsNullOrEmpty(context) ? "" : $"：{context}");
            JournalSystem.instance.Record("npc_interaction", $"NPC：{npcId}", desc);
        }

        // 更新NPC态度文本
        rel.attitudeText = GetAttitudeText(rel.trustLevel);

        // NPC间关系影响
        UpdateNPCBondWithOthers(npcId, type);

        Debug.Log($"[Relation] {npcId}: {type} ({change:+0;-0}) → 信任度 {rel.trustLevel}");
    }

    float GetInteractionChange(InteractionType type)
    {
        switch (type)
        {
            case InteractionType.GaveGift: return +15;
            case InteractionType.HelpedInCombat: return +8;
            case InteractionType.SuccessfulTrade: return +5;
            case InteractionType.Conversation: return +2;
            case InteractionType.SharedFood: return +10;
            case InteractionType.HealedNPC: return +12;
            case InteractionType.ProtectedNPC: return +15;
            case InteractionType.FailedToHelp: return -3;
            case InteractionType.Insulted: return -10;
            case InteractionType.Attacked: return -20;
            case InteractionType.StoleFrom: return -25;
            case InteractionType.Betrayed: return -40;
            default: return 0;
        }
    }

    string GetInteractionDesc(InteractionType type)
    {
        switch (type)
        {
            case InteractionType.GaveGift: return "赠送了礼物";
            case InteractionType.HelpedInCombat: return "在战斗中施以援手";
            case InteractionType.SuccessfulTrade: return "成功交易";
            case InteractionType.Conversation: return "进行了交谈";
            case InteractionType.SharedFood: return "分享了食物";
            case InteractionType.HealedNPC: return "治疗了NPC";
            case InteractionType.ProtectedNPC: return "保护了NPC";
            case InteractionType.FailedToHelp: return "试图帮助但失败了";
            case InteractionType.Insulted: return "出言不逊";
            case InteractionType.Attacked: return "攻击了NPC";
            case InteractionType.StoleFrom: return "偷窃了NPC";
            case InteractionType.Betrayed: return "背叛了NPC";
            default: return "互动";
        }
    }

    // ====================
    // NPC间关系
    // ====================

    void UpdateNPCBondWithOthers(string npcId, InteractionType type)
    {
        // 某些互动会影响NPC与其他NPC的关系
        foreach (var other in relationships.Keys)
        {
            if (other == npcId) continue;

            float bond = GetNPCBond(npcId, other);

            switch (type)
            {
                case InteractionType.ProtectedNPC:
                    // 保护某人，其他友方NPC会更信任你
                    if (bond > 30) AdjustNPCBond(other, npcId, +5);
                    break;
                case InteractionType.Attacked:
                    // 攻击某人，其他NPC会降低对你的好感
                    if (bond > 0) AdjustNPCBond(other, npcId, -10);
                    break;
                case InteractionType.GaveGift:
                    // 赠送礼物给一人，在场的第三方NPC也会看到
                    AdjustNPCBond(other, npcId, +3);
                    break;
            }
        }
    }

    public float GetNPCBond(string npcA, string npcB)
    {
        if (!npcBonds.ContainsKey(npcA)) return 0;
        if (!npcBonds[npcA].ContainsKey(npcB)) return 0;
        return npcBonds[npcA][npcB];
    }

    void AdjustNPCBond(string npcA, string npcB, float delta)
    {
        if (!npcBonds.ContainsKey(npcA)) npcBonds[npcA] = new Dictionary<string, float>();
        float current = GetNPCBond(npcA, npcB);
        npcBonds[npcA][npcB] = Mathf.Clamp(current + delta, -100, 100);
    }

    // ====================
    // 态度文本
    // ====================

    string GetAttitudeText(int trust)
    {
        if (trust >= 80) return "挚爱";
        if (trust >= 50) return "信任";
        if (trust >= 20) return "友善";
        if (trust >= 5) return "好感";
        if (trust >= -5) return "中立";
        if (trust >= -20) return "冷淡";
        if (trust >= -50) return "厌恶";
        return "深仇";
    }

    // ====================
    // 对话分支（根据关系解锁不同对话）
    // ====================

    /// <summary>
    /// 根据关系等级获取对话分支
    /// </summary>
    public List<DialogueBranch> GetDialogueOptions(string npcId)
    {
        var options = new List<DialogueBranch>();
        if (!relationships.ContainsKey(npcId))
            return GetDefaultDialogue(npcId);

        var rel = relationships[npcId];
        int trust = rel.trustLevel;

        // 基础对话（中立可解锁）
        options.Add(new DialogueBranch { id = "greet", text = "打招呼", minTrust = -100, maxTrust = 100 });
        options.Add(new DialogueBranch { id = "trade", text = "交易", minTrust = -20, maxTrust = 100 });
        options.Add(new DialogueBranch { id = "ask_story", text = "询问森林的事", minTrust = -10, maxTrust = 100 });

        // 好感解锁
        if (trust >= 20)
            options.Add(new DialogueBranch { id = "personal", text = "聊私人话题", minTrust = 20, maxTrust = 100 });

        if (trust >= 50)
        {
            options.Add(new DialogueBranch { id = "deep_story", text = "深入了解过去", minTrust = 50, maxTrust = 100 });
            options.Add(new DialogueBranch { id = "gift", text = "赠送礼物", minTrust = 50, maxTrust = 100 });
        }

        if (trust >= 80)
        {
            options.Add(new DialogueBranch { id = "secret", text = "询问黑雾真相", minTrust = 80, maxTrust = 100 });
            options.Add(new DialogueBranch { id = "quest", text = "请求协助", minTrust = 80, maxTrust = 100 });
        }

        // 仇恨解锁
        if (trust <= -20)
            options.Add(new DialogueBranch { id = "intimidate", text = "威胁", minTrust = -100, maxTrust = -20 });

        if (trust <= -50)
            options.Add(new DialogueBranch { id = "attack_threat", text = "警告离开", minTrust = -100, maxTrust = -50 });

        return options;
    }

    List<DialogueBranch> GetDefaultDialogue(string npcId)
    {
        return new List<DialogueBranch>
        {
            new DialogueBranch { id = "greet", text = "你好", minTrust = -100, maxTrust = 100 }
        };
    }

    /// <summary>
    /// 执行对话选项
    /// </summary>
    public void ExecuteDialogueOption(string npcId, string optionId)
    {
        switch (optionId)
        {
            case "greet":
                GameManager.instance?.AddLog($"你好。");
                RecordInteraction(npcId, InteractionType.Conversation);
                break;

            case "personal":
                GameManager.instance?.AddLog($"（深入交谈...）");
                RecordInteraction(npcId, InteractionType.Conversation, "私人话题");
                break;

            case "deep_story":
                GameManager.instance?.AddLog($"（分享了过去的故事...）");
                RecordInteraction(npcId, InteractionType.Conversation, "深入交流");
                // 解锁新知识
                GameManager.instance?.AddLog("✨ 获得了关于这片森林的信息...");
                break;

            case "gift":
                TryGiveGift(npcId);
                break;

            case "secret":
                GameManager.instance?.AddLog($"（黑雾的真相...）");
                RecordInteraction(npcId, InteractionType.Conversation, "黑雾秘密");
                GameManager.instance?.AddLog($"✨ 解锁了新记忆碎片！");
                GameManager.instance.memories += 2;
                break;

            case "quest":
                TriggerNPCQuest(npcId);
                break;

            case "ask_story":
                GameManager.instance?.AddLog($"（询问森林的过去...）");
                RecordInteraction(npcId, InteractionType.Conversation, "森林历史");
                break;

            case "trade":
                ShowTradeDialogue(npcId);
                break;

            case "intimidate":
                GameManager.instance?.AddLog($"（出言威胁...）");
                RecordInteraction(npcId, InteractionType.Insulted);
                break;

            case "attack_threat":
                GameManager.instance?.AddLog($"（发出警告...）");
                RecordInteraction(npcId, InteractionType.Attacked, "口头攻击");
                break;
        }
    }

    // ====================
    // 赠送礼物
    // ====================

    void TryGiveGift(string npcId)
    {
        if (GameManager.instance == null) return;

        // 消耗食物换取好感
        if (GameManager.instance.food >= 3)
        {
            GameManager.instance.food -= 3;
            RecordInteraction(npcId, InteractionType.GaveGift, "赠送3食物");
            GameManager.instance.AddLog($"赠送了3个食物给 {npcId}");

            // 特殊礼物（草药>5额外+5好感）
            if (GameManager.instance.herb >= 2)
            {
                GameManager.instance.herb -= 2;
                RecordInteraction(npcId, InteractionType.GaveGift, "赠送草药");
                GameManager.instance.AddLog($"额外赠送了草药，{npcId}很开心！");
            }
        }
        else
        {
            GameManager.instance.AddLog("食物不足，无法赠送礼物...");
        }
    }

    // ====================
    // NPC任务
    // ====================

    void TriggerNPCQuest(string npcId)
    {
        if (GameManager.instance == null) return;

        // 根据NPC类型触发不同任务
        var quests = new Dictionary<string, (string title, string desc, Action reward)>
        {
            ["masha"] = ("采集草药", "帮我采集5个草药，我会告诉你一个秘密",
                (Action)(() => {
                    if (GameManager.instance.herb >= 5) {
                        GameManager.instance.herb -= 5;
                        RecordInteraction("masha", InteractionType.SuccessfulTrade, "完成任务");
                        GameManager.instance.AddLog("玛莎告诉你：森林之心在...（待填充）");
                        GameManager.instance.memories += 3;
                    }
                })),
            ["lily"] = ("照顾孩子", "陪莉莉讲故事",
                (Action)(() => {
                    RecordInteraction("lily", InteractionType.Conversation, "讲故事");
                    GameManager.instance.AddLog("莉莉给你画了一幅画...");
                })),
            ["eric"] = ("战斗考验", "证明你的实力！",
                (Action)(() => {
                    RecordInteraction("eric", InteractionType.HelpedInCombat, "通过考验");
                    GameManager.instance.AddLog("埃里克认可了你的实力！");
                }))
        };

        if (quests.ContainsKey(npcId))
        {
            var q = quests[npcId];
            GameManager.instance.AddLog($"═══ 任务：{q.title} ═══");
            GameManager.instance.AddLog(q.desc);
            // 任务已添加到日志，实际完成逻辑在任务系统
        }
        else
        {
            GameManager.instance.AddLog($"{npcId}暂时没有任务给你...");
        }
    }

    // ====================
    // 交易系统
    // ====================

    void ShowTradeDialogue(string npcId)
    {
        if (GameManager.instance == null) return;

        if (!relationships.ContainsKey(npcId) || relationships[npcId].trustLevel < -20)
        {
            GameManager.instance.AddLog($"{npcId}不愿意和你交易...");
            return;
        }

        GameManager.instance.AddLog("═══ 交易 ═══");
        GameManager.instance.AddLog($"信任度：{relationships[npcId].trustLevel}（{relationships[npcId].attitudeText}）");

        // 根据信任度决定交易价格
        int discount = 0;
        if (relationships[npcId].trustLevel >= 50) discount = 20;
        else if (relationships[npcId].trustLevel >= 20) discount = 10;

        GameManager.instance.AddLog($"（交易折扣：{discount}%）");

        // 简单交易选项
        GameManager.instance.AddLog("  [1] 用 3食物 → 换 5木材");
        GameManager.instance.AddLog("  [2] 用 2草药 → 换 3食物");
        GameManager.instance.AddLog("  [3] 退出");
    }

    // ====================
    // NPC日常行为（每日触发）
    // ====================

    /// <summary>
    /// 每日NPC行为（AdvanceDay时调用）
    /// </summary>
    public void OnDayStart()
    {
        foreach (var npcId in relationships.Keys)
        {
            var rel = relationships[npcId];
            TriggerDailyBehavior(npcId, rel);
        }
    }

    void TriggerDailyBehavior(string npcId, NPCRelationship rel)
    {
        if (GameManager.instance == null) return;

        float roll = UnityEngine.Random.value;

        // 好友：可能主动给资源
        if (rel.trustLevel >= 50 && roll < 0.2f)
        {
            int bonus = UnityEngine.Random.Range(1, 4);
            string[] gifts = { "食物", "草药", "木材" };
            string gift = gifts[UnityEngine.Random.Range(0, gifts.Length)];

            switch (gift)
            {
                case "食物": GameManager.instance.food += bonus; break;
                case "草药": GameManager.instance.herb += bonus; break;
                case "木材": GameManager.instance.wood += bonus; break;
            }

            GameManager.instance.AddLog($"{npcId}（{rel.attitudeText}）分享了 {bonus} 个{gift}给你！");
        }

        // 中立：可能触发小任务
        else if (rel.trustLevel >= 0 && roll < 0.1f)
        {
            GameManager.instance.AddLog($"{npcId}向你打了个招呼...");
            RecordInteraction(npcId, InteractionType.Conversation, "日常问候");
        }

        // 冷淡：可能说讽刺的话
        else if (rel.trustLevel <= -20 && roll < 0.15f)
        {
            string[] cold = { "保持距离", "假装没看见", "冷哼一声" };
            GameManager.instance.AddLog($"{npcId}（{rel.attitudeText}）：{cold[UnityEngine.Random.Range(0, cold.Length)]}");
        }
    }

    // ====================
    // 数据类
    // ====================

    public enum InteractionType
    {
        Conversation,
        GaveGift,
        HelpedInCombat,
        SuccessfulTrade,
        SharedFood,
        HealedNPC,
        ProtectedNPC,
        FailedToHelp,
        Insulted,
        Attacked,
        StoleFrom,
        Betrayed
    }

    [System.Serializable]
    public class NPCRelationship
    {
        public string npcId;
        public int trustLevel = 0;        // -100 ~ 100
        public string attitudeText = "陌生人";
        public int timesMet = 0;
        public List<InteractionRecord> interactionHistory = new List<InteractionRecord>();
        public List<string> unlockedSecrets = new List<string>();  // NPC告诉你的秘密
    }

    [System.Serializable]
    public class InteractionRecord
    {
        public InteractionType type;
        public int day;
        public string context;
        public float trustChange;
    }

    [System.Serializable]
    public class DialogueBranch
    {
        public string id;
        public string text;
        public int minTrust;   // 最低信任等级
        public int maxTrust;   // 最高信任等级
        public string dialogueContent;  // 对话内容
        public List<string> tags;     // 剧情标签
    }

    [System.Serializable]
    public class NPCDailyAction
    {
        public string npcId;
        public string actionId;
        public string description;
        public int dayTrigger;
        public bool triggered;
    }
}
