using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// NPC 招募系统 v1.0（SPEC v2.6 第四章）
/// 5个可招募NPC：玛莎/莉莉/埃里克/灰烬/沃斯
/// </summary>
public class NPCRecruitmentSystem : MonoBehaviour
{
    public static NPCRecruitmentSystem instance { get; private set; }

    [Header("NPC 数据")]
    public List<NPCTemplate> allNPCs = new List<NPCTemplate>();

    [Header("已招募")]
    public List<SquadMember> recruitedNPCs = new List<SquadMember>();

    void Awake()
    {
        instance = this;
        InitializeNPCs();
    }

    void InitializeNPCs()
    {
        allNPCs = new List<NPCTemplate>
        {
            new NPCTemplate
            {
                npcId = "masha",
                name = "玛莎",
                role = "炼金师",
                description = "曾经在村子里经营草药铺，黑雾来临时逃入森林",
                requiredFood = 5,
                dayRequirement = 0,
                memoryFragmentRequirement = 0,
                specialRequirement = null,
                maxHealth = 70,
                attack = 7,
                defense = 5,
                intelligence = 15,
                agility = 8,
                skillDesc = "草药采集+50%"
            },
            new NPCTemplate
            {
                npcId = "lily",
                name = "莉莉",
                role = "记录者",
                description = "无家可归的孩子，在森林中独自生存",
                requiredFood = 0,
                dayRequirement = 0,
                memoryFragmentRequirement = 0,
                specialRequirement = null,
                maxHealth = 50,
                attack = 5,
                defense = 4,
                intelligence = 12,
                agility = 14,
                skillDesc = "手账记录效果+1"
            },
            new NPCTemplate
            {
                npcId = "eric",
                name = "埃里克",
                role = "战士",
                description = "曾经的士兵，在森林中迷路多年",
                requiredFood = 0,
                dayRequirement = 5,
                memoryFragmentRequirement = 0,
                specialRequirement = "trial",
                maxHealth = 130,
                attack = 18,
                defense = 14,
                intelligence = 6,
                agility = 7,
                skillDesc = "防御姿态：受伤-50%"
            },
            new NPCTemplate
            {
                npcId = "ashes",
                name = "灰烬",
                role = "漫游者",
                description = "神秘的黑雾幸存者，很少说话",
                requiredFood = 0,
                dayRequirement = 10,
                memoryFragmentRequirement = 2,
                specialRequirement = null,
                maxHealth = 90,
                attack = 15,
                defense = 9,
                intelligence = 10,
                agility = 11,
                skillDesc = "追踪：感知敌人动向"
            },
            new NPCTemplate
            {
                npcId = "vos",
                name = "沃斯",
                role = "巫女",
                description = "据说与森林之心有神秘联系",
                requiredFood = 0,
                dayRequirement = 15,
                memoryFragmentRequirement = 0,
                specialRequirement = "refuse",
                maxHealth = 60,
                attack = 6,
                defense = 4,
                intelligence = 18,
                agility = 9,
                skillDesc = "感知威胁，初始解锁追踪"
            }
        };
    }

    // ====================
    // 查询
    // ====================

    /// <summary>
    /// 获取当前可遇到的NPC列表
    /// </summary>
    public List<NPCTemplate> GetAvailableNPCs(int currentDay, int memories)
    {
        var available = new List<NPCTemplate>();
        foreach (var npc in allNPCs)
        {
            // 排除已招募的
            bool alreadyRecruited = false;
            foreach (var r in recruitedNPCs)
            {
                if (r.memberId == npc.npcId) { alreadyRecruited = true; break; }
            }
            if (alreadyRecruited) continue;

            // 检查日期要求
            if (currentDay < npc.dayRequirement) continue;

            // 检查记忆碎片要求
            if (memories < npc.memoryFragmentRequirement) continue;

            available.Add(npc);
        }
        return available;
    }

    /// <summary>
    /// 检查某个NPC是否可招募
    /// </summary>
    public (bool canRecruit, string reason) CheckRecruitability(NPCTemplate npc, int currentDay, int food, int memories)
    {
        if (currentDay < npc.dayRequirement)
            return (false, $"需要第{npc.dayRequirement}天之后才能遇到{npc.name}");

        if (memories < npc.memoryFragmentRequirement)
            return (false, $"需要{npc.memoryFragmentRequirement}个记忆碎片才能找到{npc.name}");

        if (npc.specialRequirement == "refuse")
        {
            // 沃斯永远拒绝加入（叙事）
            return (false, null);  // canRecruit=false, reason=null 表示触发特殊剧情
        }

        if (npc.requiredFood > 0)
        {
            if (food < npc.requiredFood)
                return (false, $"需要{npc.requiredFood}食物才能招募{npc.name}");
        }

        return (true, null);
    }

    /// <summary>
    /// 尝试消耗食物招募NPC
    /// </summary>
    public bool TryRecruitWithFood(string npcId, int requiredFood)
    {
        if (GameManager.instance == null) return false;

        if (GameManager.instance.food < requiredFood)
        {
            GameManager.instance.AddLog($"⚠️ 食物不足！需要{requiredFood}食物");
            return false;
        }

        GameManager.instance.food -= requiredFood;
        return true;
    }

    /// <summary===
    /// 招募NPC到队伍
    /// </summary>
    public void RecruitNPC(NPCTemplate npc)
    {
        var member = new SquadMember
        {
            memberId = npc.npcId,
            memberName = npc.name,
            role = npc.role,
            maxHealth = npc.maxHealth,
            currentHealth = npc.maxHealth,
            attack = npc.attack,
            defense = npc.defense,
            intelligence = npc.intelligence,
            agility = npc.agility,
            status = "正常"
        };

        GameManager.instance.squad.Add(member);
        recruitedNPCs.Add(member);

        GameManager.instance.AddLog($"🤝 {npc.name} 加入了队伍！({npc.role})");
        GameManager.instance.AddLog($"  特长：{npc.skillDesc}");

        // 触发手账
        if (JournalSystem.instance != null)
            JournalSystem.instance.OnMemberRecruited(npc.name, npc.description);

        Debug.Log($"[NPC] 招募：{npc.name} ({npc.role})");
    }

    // ====================
    // 对话和遭遇
    // ====================

    /// <summary>
    /// 在 TalkToNPC 时触发随机NPC遭遇
    /// </summary>
    public void TriggerNPCEncounter()
    {
        if (GameManager.instance == null) return;

        var available = GetAvailableNPCs(
            GameManager.instance.currentDay,
            GameManager.instance.memories
        );

        if (available.Count == 0)
        {
            GameManager.instance.AddLog("在森林中遇到了一些旅人的痕迹，但没有找到人...");
            return;
        }

        var npc = available[UnityEngine.Random.Range(0, available.Count)];
        var (canRecruit, reason) = CheckRecruitability(npc,
            GameManager.instance.currentDay,
            GameManager.instance.food,
            GameManager.instance.memories);

        GameManager.instance.AddLog($"遇到了旅人：{npc.name}");
        GameManager.instance.AddLog(npc.description);

        if (npc.specialRequirement == "refuse")
        {
            // 沃斯特殊剧情
            GameManager.instance.AddLog($"{npc.name}摇了摇头：「我的命运在森林里，我不会离开。」");
            GameManager.instance.AddLog("（无法招募，但她似乎知道森林之心的秘密...）");
            GameManager.instance.memories += 1;
            return;
        }

        if (npc.specialRequirement == "trial")
        {
            // 埃里克需要考验：随机战斗或问答
            GameManager.instance.AddLog($"{npc.name}说：「先证明你的实力！」");
            // 触发一场特殊战斗（暂时用简单逻辑）
            GameManager.instance.AddLog("通过第5天的考验！");
            RecruitNPC(npc);
            return;
        }

        if (!canRecruit)
        {
            GameManager.instance.AddLog($"{reason}");
            return;
        }

        // 可以招募
        if (npc.requiredFood > 0)
        {
            GameManager.instance.AddLog($"{npc.name}说：「给我{npc.requiredFood}食物，我就加入。」");
            if (TryRecruitWithFood(npc.npcId, npc.requiredFood))
            {
                RecruitNPC(npc);
            }
        }
        else
        {
            // 莉莉：无条件加入
            GameManager.instance.AddLog($"{npc.name}愿意加入队伍！");
            RecruitNPC(npc);
        }
    }

    // ====================
    // 数据类
    // ====================

    [System.Serializable]
    public class NPCTemplate
    {
        public string npcId;
        public string name;
        public string role;
        public string description;
        public int requiredFood;         // 0表示不需要
        public int dayRequirement;       // 0表示无日期要求
        public int memoryFragmentRequirement;
        public string specialRequirement; // null/"refuse"/"trial"
        public int maxHealth;
        public int attack;
        public int defense;
        public int intelligence;
        public int agility;
        public string skillDesc;
    }
}
