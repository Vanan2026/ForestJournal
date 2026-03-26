using System;
using System.Collections.Generic;
using UnityEngine;

namespace ForestJournal
{
    // ==================== 称号系统 ====================
    
    /// <summary>
    /// 称号类别
    /// </summary>
    public enum TitleCategory
    {
        Combat,      // 战斗系
        Exploration, // 探索系
        Collection,  // 收集系
        Social,      // 社交系
        Special,     // 特殊系
        Hidden       // 隐藏系
    }

    /// <summary>
    /// 时间段
    /// </summary>
    public enum TimeOfDay
    {
        Morning,   // 早晨 (6:00-12:00)
        Afternoon, // 下午 (12:00-18:00)
        Evening,   // 傍晚 (18:00-22:00)
        Night      // 夜晚 (22:00-6:00)
    }

    /// <summary>
    /// 称号数据结构
    /// </summary>
    [Serializable]
    public class TitleData
    {
        public string id;                    // 称号ID
        public string name;                  // 称号名称
        public string description;            // 称号描述
        public TitleCategory category;        // 所属类别
        public StatBonus statBonus;           // 属性加成
        public List<string> skillUnlocks;    // 解锁技能列表
        public TitleRequirement requirement;  // 获得条件
        
        [HideInInspector]
        public bool isAcquired;              // 是否已获得
    }

    /// <summary>
    /// 属性加成
    /// </summary>
    [Serializable]
    public class StatBonus
    {
        public int healthBonus;      // 生命值加成
        public int staminaBonus;      // 体力加成
        public int attackBonus;       // 攻击力加成
        public int defenseBonus;      // 防御力加成
        public float speedBonus;      // 速度加成
        public int luckBonus;         // 幸运值加成
        public float critRateBonus;   // 暴击率加成
        public float critDamageBonus; // 暴击伤害加成
        
        public static StatBonus operator +(StatBonus a, StatBonus b)
        {
            return new StatBonus
            {
                healthBonus = a.healthBonus + b.healthBonus,
                staminaBonus = a.staminaBonus + b.staminaBonus,
                attackBonus = a.attackBonus + b.attackBonus,
                defenseBonus = a.defenseBonus + b.defenseBonus,
                speedBonus = a.speedBonus + b.speedBonus,
                luckBonus = a.luckBonus + b.luckBonus,
                critRateBonus = a.critRateBonus + b.critRateBonus,
                critDamageBonus = a.critDamageBonus + b.critDamageBonus
            };
        }
    }

    /// <summary>
    /// 称号获得条件
    /// </summary>
    [Serializable]
    public class TitleRequirement
    {
        public string type;     // 条件类型: kills, discoveries, collections, friendships, special
        public int count;       // 数量要求
        public string targetId; // 目标ID (敌人ID/物品ID/NPC ID等)
        public bool hidden;     // 是否隐藏条件
    }

    /// <summary>
    /// 称号系统 - 管理玩家所有称号
    /// </summary>
    public class TitleSystem
    {
        private static TitleSystem _instance;
        public static TitleSystem Instance => _instance ??= new TitleSystem();

        private List<TitleData> _allTitles;
        private HashSet<string> _acquiredTitleIds;
        private StatBonus _totalStatBonus;

        public event Action<TitleData> OnTitleAcquired;
        public IReadOnlyList<TitleData> AllTitles => _allTitles;
        public IReadOnlyCollection<string> AcquiredTitleIds => _acquiredTitleIds;
        public StatBonus TotalStatBonus => _totalStatBonus;

        private TitleSystem()
        {
            _allTitles = new List<TitleData>();
            _acquiredTitleIds = new HashSet<string>();
            InitializeTitles();
        }

        /// <summary>
        /// 初始化所有称号
        /// </summary>
        private void InitializeTitles()
        {
            // ==================== 战斗系称号 (6个) ====================
            _allTitles.Add(new TitleData
            {
                id = "fog_slayer",
                name = "屠雾者",
                description = "清除指定区域内的所有迷雾，证明你是森林的征服者",
                category = TitleCategory.Combat,
                statBonus = new StatBonus { attackBonus = 15, critRateBonus = 0.05f },
                skillUnlocks = new List<string> { "FogCleave" },
                requirement = new TitleRequirement { type = "kills", count = 50, targetId = "mist_monster" }
            });

            _allTitles.Add(new TitleData
            {
                id = "warrior",
                name = "战士",
                description = "在正面战斗中击败100名敌人",
                category = TitleCategory.Combat,
                statBonus = new StatBonus { attackBonus = 10, defenseBonus = 5 },
                skillUnlocks = new List<string>(),
                requirement = new TitleRequirement { type = "kills", count = 100 }
            });

            _allTitles.Add(new TitleData
            {
                id = "veteran",
                name = "老兵",
                description = "存活超过30天，每次战斗都让你更加坚强",
                category = TitleCategory.Combat,
                statBonus = new StatBonus { healthBonus = 50, defenseBonus = 8 },
                skillUnlocks = new List<string> { "BattleHardy" },
                requirement = new TitleRequirement { type = "special", count = 30, targetId = "survival_days" }
            });

            _allTitles.Add(new TitleData
            {
                id = "legendary_hunter",
                name = "传说猎手",
                description = "狩猎记录无人能及，你是传说中的猎人",
                category = TitleCategory.Combat,
                statBonus = new StatBonus { attackBonus = 20, critDamageBonus = 0.15f, luckBonus = 10 },
                skillUnlocks = new List<string> { "HunterInstinct" },
                requirement = new TitleRequirement { type = "kills", count = 500 }
            });

            _allTitles.Add(new TitleData
            {
                id = "boss_slayer",
                name = "Boss克星",
                description = "击败森林深处的所有Boss",
                category = TitleCategory.Combat,
                statBonus = new StatBonus { attackBonus = 25, healthBonus = 100, critRateBonus = 0.1f },
                skillUnlocks = new List<string> { "BossRage" },
                requirement = new TitleRequirement { type = "kills", count = 10, targetId = "boss" }
            });

            _allTitles.Add(new TitleData
            {
                id = "duelist",
                name = "决斗者",
                description = "在单挑中从未落败",
                category = TitleCategory.Combat,
                statBonus = new StatBonus { attackBonus = 12, critRateBonus = 0.08f },
                skillUnlocks = new List<string>(),
                requirement = new TitleRequirement { type = "special", count = 1, targetId = "perfect_duel" }
            });

            // ==================== 探索系称号 (6个) ====================
            _allTitles.Add(new TitleData
            {
                id = "explorer",
                name = "探险家",
                description = "发现森林中50%的区域",
                category = TitleCategory.Exploration,
                statBonus = new StatBonus { speedBonus = 0.1f, staminaBonus = 20 },
                skillUnlocks = new List<string> { "Cartography" },
                requirement = new TitleRequirement { type = "discoveries", count = 50, targetId = "region" }
            });

            _allTitles.Add(new TitleData
            {
                id = "adventure_king",
                name = "冒险王",
                description = "探索森林的每一个角落",
                category = TitleCategory.Exploration,
                statBonus = new StatBonus { speedBonus = 0.15f, staminaBonus = 30, luckBonus = 5 },
                skillUnlocks = new List<string> { "TreasureSense" },
                requirement = new TitleRequirement { type = "discoveries", count = 100, targetId = "region" }
            });

            _allTitles.Add(new TitleData
            {
                id = "trailblazer",
                name = "足迹",
                description = "走过森林的每一条道路",
                category = TitleCategory.Exploration,
                statBonus = new StatBonus { speedBonus = 0.08f },
                skillUnlocks = new List<string>(),
                requirement = new TitleRequirement { type = "discoveries", count = 200, targetId = "path" }
            });

            _allTitles.Add(new TitleData
            {
                id = "geographer",
                name = "地理学家",
                description = "绘制出完整的森林地图",
                category = TitleCategory.Exploration,
                statBonus = new StatBonus { speedBonus = 0.12f, luckBonus = 8 },
                skillUnlocks = new List<string> { "AutoMapping" },
                requirement = new TitleRequirement { type = "special", count = 1, targetId = "full_map" }
            });

            _allTitles.Add(new TitleData
            {
                id = "pathfinder",
                name = "引路人",
                description = "发现所有隐藏地点",
                category = TitleCategory.Exploration,
                statBonus = new StatBonus { speedBonus = 0.1f, luckBonus = 15 },
                skillUnlocks = new List<string> { "SecretPassage" },
                requirement = new TitleRequirement { type = "discoveries", count = 30, targetId = "hidden_place" }
            });

            _allTitles.Add(new TitleData
            {
                id = "deep_diver",
                name = "深渊潜者",
                description = "探索过最深的洞穴",
                category = TitleCategory.Exploration,
                statBonus = new StatBonus { defenseBonus = 10, healthBonus = 30 },
                skillUnlocks = new List<string> { "DarkVision" },
                requirement = new TitleRequirement { type = "discoveries", count = 1, targetId = "deepest_cave" }
            });

            // ==================== 收集系称号 (6个) ====================
            _allTitles.Add(new TitleData
            {
                id = "scholar",
                name = "学者",
                description = "收集100种不同的物品",
                category = TitleCategory.Collection,
                statBonus = new StatBonus { luckBonus = 5 },
                skillUnlocks = new List<string>(),
                requirement = new TitleRequirement { type = "collections", count = 100, targetId = "item" }
            });

            _allTitles.Add(new TitleData
            {
                id = "polymath",
                name = "博学者",
                description = "了解森林中所有植物和动物的知识",
                category = TitleCategory.Collection,
                statBonus = new StatBonus { luckBonus = 10, staminaBonus = 15 },
                skillUnlocks = new List<string> { "KnowledgeSharing" },
                requirement = new TitleRequirement { type = "collections", count = 200, targetId = "knowledge" }
            });

            _allTitles.Add(new TitleData
            {
                id = "collector",
                name = "收藏家",
                description = "收藏品数量惊人",
                category = TitleCategory.Collection,
                statBonus = new StatBonus { luckBonus = 12, critRateBonus = 0.05f },
                skillUnlocks = new List<string>(),
                requirement = new TitleRequirement { type = "collections", count = 500, targetId = "item" }
            });

            _allTitles.Add(new TitleData
            {
                id = "set_master",
                name = "套装大师",
                description = "集齐并制作所有套装",
                category = TitleCategory.Collection,
                statBonus = new StatBonus { attackBonus = 15, defenseBonus = 15, healthBonus = 50 },
                skillUnlocks = new List<string> { "SetBonus" },
                requirement = new TitleRequirement { type = "collections", count = 10, targetId = "set" }
            });

            _allTitles.Add(new TitleData
            {
                id = "herb_master",
                name = "草药大师",
                description = "收集所有类型的草药",
                category = TitleCategory.Collection,
                statBonus = new StatBonus { healthBonus = 40, staminaBonus = 20 },
                skillUnlocks = new List<string> { "HerbIdentification" },
                requirement = new TitleRequirement { type = "collections", count = 50, targetId = "herb" }
            });

            _allTitles.Add(new TitleData
            {
                id = "recipe_collector",
                name = "食谱收集者",
                description = "收集了所有食谱",
                category = TitleCategory.Collection,
                statBonus = new StatBonus { staminaBonus = 25, healthBonus = 30 },
                skillUnlocks = new List<string> { "MasterChef" },
                requirement = new TitleRequirement { type = "collections", count = 30, targetId = "recipe" }
            });

            // ==================== 社交系称号 (6个) ====================
            _allTitles.Add(new TitleData
            {
                id = "best_friend",
                name = "挚友",
                description = "与某位NPC建立了深厚的友谊",
                category = TitleCategory.Social,
                statBonus = new StatBonus { luckBonus = 5, speedBonus = 0.05f },
                skillUnlocks = new List<string> { "FriendDiscount" },
                requirement = new TitleRequirement { type = "friendships", count = 1, targetId = "max_affinity" }
            });

            _allTitles.Add(new TitleData
            {
                id = "recruiter",
                name = "招募者",
                description = "成功招募新的伙伴加入营地",
                category = TitleCategory.Social,
                statBonus = new StatBonus { healthBonus = 30, defenseBonus = 5 },
                skillUnlocks = new List<string> { "RecruitmentBonus" },
                requirement = new TitleRequirement { type = "friendships", count = 3, targetId = "recruit" }
            });

            _allTitles.Add(new TitleData
            {
                id = "gift_master",
                name = "礼物达人",
                description = "送出100份礼物",
                category = TitleCategory.Social,
                statBonus = new StatBonus { luckBonus = 8, speedBonus = 0.05f },
                skillUnlocks = new List<string> { "GiftIntuition" },
                requirement = new TitleRequirement { type = "friendships", count = 100, targetId = "gift" }
            });

            _allTitles.Add(new TitleData
            {
                id = "social_butterfly",
                name = "社交达人",
                description = "和所有NPC都成为了朋友",
                category = TitleCategory.Social,
                statBonus = new StatBonus { luckBonus = 15, critRateBonus = 0.05f, speedBonus = 0.08f },
                skillUnlocks = new List<string> { "SocialCharm" },
                requirement = new TitleRequirement { type = "friendships", count = 5, targetId = "max_affinity" }
            });

            _allTitles.Add(new TitleData
            {
                id = "listener",
                name = "倾听者",
                description = "听完了所有NPC的故事",
                category = TitleCategory.Social,
                statBonus = new StatBonus { luckBonus = 6 },
                skillUnlocks = new List<string>(),
                requirement = new TitleRequirement { type = "friendships", count = 50, targetId = "dialogue" }
            });

            _allTitles.Add(new TitleData
            {
                id = "camp_leader",
                name = "营地领袖",
                description = "营地的每个人都信任你",
                category = TitleCategory.Social,
                statBonus = new StatBonus { healthBonus = 50, defenseBonus = 10, speedBonus = 0.1f },
                skillUnlocks = new List<string> { "Leadership" },
                requirement = new TitleRequirement { type = "friendships", count = 5, targetId = "trust_max" }
            });

            // ==================== 特殊系称号 (6个) ====================
            _allTitles.Add(new TitleData
            {
                id = "pacifist",
                name = "和平主义者",
                description = "不杀生通关",
                category = TitleCategory.Special,
                statBonus = new StatBonus { luckBonus = 30, critRateBonus = 0.1f, speedBonus = 0.15f },
                skillUnlocks = new List<string> { "PeaceAura" },
                requirement = new TitleRequirement { type = "special", count = 1, targetId = "no_kills" }
            });

            _allTitles.Add(new TitleData
            {
                id = "speedrunner",
                name = "速通者",
                description = "在最短时间内完成主要目标",
                category = TitleCategory.Special,
                statBonus = new StatBonus { speedBonus = 0.25f, staminaBonus = 50 },
                skillUnlocks = new List<string> { "SprintMaster" },
                requirement = new TitleRequirement { type = "special", count = 1, targetId = "speed_clear" }
            });

            _allTitles.Add(new TitleData
            {
                id = "master_crafter",
                name = "全能工匠",
                description = "精通所有制作技艺",
                category = TitleCategory.Special,
                statBonus = new StatBonus { attackBonus = 10, defenseBonus = 10, healthBonus = 30 },
                skillUnlocks = new List<string> { "MasterCraft", "QualityAssurance" },
                requirement = new TitleRequirement { type = "special", count = 100, targetId = "craft_count" }
            });

            _allTitles.Add(new TitleData
            {
                id = "economist",
                name = "经济学家",
                description = "积累大量财富",
                category = TitleCategory.Special,
                statBonus = new StatBonus { luckBonus = 10 },
                skillUnlocks = new List<string> { "TradeMaster" },
                requirement = new TitleRequirement { type = "special", count = 10000, targetId = "gold" }
            });

            _allTitles.Add(new TitleData
            {
                id = "survivor",
                name = "生存专家",
                description = "在艰苦环境中存活下来",
                category = TitleCategory.Special,
                statBonus = new StatBonus { healthBonus = 80, staminaBonus = 40, defenseBonus = 10 },
                skillUnlocks = new List<string> { "SurvivalInstinct" },
                requirement = new TitleRequirement { type = "special", count = 7, targetId = "survival_days_hard" }
            });

            _allTitles.Add(new TitleData
            {
                id = "perfectionist",
                name = "完美主义者",
                description = "追求完美，不留遗憾",
                category = TitleCategory.Special,
                statBonus = new StatBonus { critRateBonus = 0.08f, critDamageBonus = 0.2f, luckBonus = 20 },
                skillUnlocks = new List<string> { "PerfectStrike" },
                requirement = new TitleRequirement { type = "special", count = 1, targetId = "perfect_run" }
            });

            // ==================== 隐藏系称号 (6个) ====================
            _allTitles.Add(new TitleData
            {
                id = "heart_of_forest",
                name = "森林之心",
                description = "与森林建立了神秘的联系",
                category = TitleCategory.Hidden,
                statBonus = new StatBonus { healthBonus = 200, staminaBonus = 100, attackBonus = 30, defenseBonus = 20, luckBonus = 30 },
                skillUnlocks = new List<string> { "ForestBlessing", "NatureWalk" },
                requirement = new TitleRequirement { type = "special", count = 1, targetId = "forest_heart", hidden = true }
            });

            _allTitles.Add(new TitleData
            {
                id = "time_traveler",
                name = "时间旅行者",
                description = "见证了时间的流逝",
                category = TitleCategory.Hidden,
                statBonus = new StatBonus { speedBonus = 0.3f, critRateBonus = 0.15f, critDamageBonus = 0.3f },
                skillUnlocks = new List<string> { "TimeStop", "Rewind" },
                requirement = new TitleRequirement { type = "special", count = 1, targetId = "time_secret", hidden = true }
            });

            _allTitles.Add(new TitleData
            {
                id = "new_order",
                name = "新秩序建立者",
                description = "改变了森林的命运",
                category = TitleCategory.Hidden,
                statBonus = new StatBonus { healthBonus = 300, attackBonus = 50, defenseBonus = 30, luckBonus = 50 },
                skillUnlocks = new List<string> { "NewDawn", "WorldBuilder" },
                requirement = new TitleRequirement { type = "special", count = 1, targetId = "new_world", hidden = true }
            });

            _allTitles.Add(new TitleData
            {
                id = "shadow_walker",
                name = "暗影行者",
                description = "在黑暗中穿行无声",
                category = TitleCategory.Hidden,
                statBonus = new StatBonus { speedBonus = 0.2f, critRateBonus = 0.2f },
                skillUnlocks = new List<string> { "ShadowForm" },
                requirement = new TitleRequirement { type = "special", count = 1, targetId = "shadow_path", hidden = true }
            });

            _allTitles.Add(new TitleData
            {
                id = "ancient_knowledge",
                name = "远古知识",
                description = "获得了失落已久的知识",
                category = TitleCategory.Hidden,
                statBonus = new StatBonus { luckBonus = 40, critDamageBonus = 0.25f },
                skillUnlocks = new List<string> { "AncientWisdom", "SkillMaster" },
                requirement = new TitleRequirement { type = "special", count = 1, targetId = "ancient_secret", hidden = true }
            });

            _allTitles.Add(new TitleData
            {
                id = "god_mode",
                name = "森林之神",
                description = "超越了凡人的存在",
                category = TitleCategory.Hidden,
                statBonus = new StatBonus { healthBonus = 500, attackBonus = 100, defenseBonus = 50, speedBonus = 0.5f, luckBonus = 100, critRateBonus = 0.3f, critDamageBonus = 0.5f },
                skillUnlocks = new List<string> { "DivinePower", "Omniscience" },
                requirement = new TitleRequirement { type = "special", count = 1, targetId = "true_ending", hidden = true }
            });

            // 更新总数
            RecalculateTotalBonus();
        }

        /// <summary>
        /// 检查并授予符合条件的称号
        /// </summary>
        public void CheckAndGrantTitles(string requirementType, int count, string targetId = "")
        {
            foreach (var title in _allTitles)
            {
                if (_acquiredTitleIds.Contains(title.id))
                    continue;

                if (title.requirement.type == requirementType &&
                    (string.IsNullOrEmpty(targetId) || title.requirement.targetId == targetId) &&
                    count >= title.requirement.count)
                {
                    GrantTitle(title.id);
                }
            }
        }

        /// <summary>
        /// 授予称号
        /// </summary>
        public void GrantTitle(string titleId)
        {
            var title = _allTitles.Find(t => t.id == titleId);
            if (title == null || _acquiredTitleIds.Contains(titleId))
                return;

            title.isAcquired = true;
            _acquiredTitleIds.Add(titleId);
            _totalStatBonus = _totalStatBonus + title.statBonus;
            
            Debug.Log($"[TitleSystem] 获得称号: {title.name} - {title.description}");
            OnTitleAcquired?.Invoke(title);
        }

        /// <summary>
        /// 获取已获得称号列表
        /// </summary>
        public List<TitleData> GetAcquiredTitles()
        {
            return _allTitles.FindAll(t => _acquiredTitleIds.Contains(t.id));
        }

        /// <summary>
        /// 获取某类别的已获得称号
        /// </summary>
        public List<TitleData> GetAcquiredTitlesByCategory(TitleCategory category)
        {
            return _allTitles.FindAll(t => t.category == category && _acquiredTitleIds.Contains(t.id));
        }

        /// <summary>
        /// 获取当前装备的称号（可配置显示）
        /// </summary>
        public TitleData GetDisplayedTitle()
        {
            // 默认返回已获得的第一个称号，可扩展为玩家选择
            var acquired = GetAcquiredTitles();
            return acquired.Count > 0 ? acquired[0] : null;
        }

        /// <summary>
        /// 重新计算总属性加成
        /// </summary>
        private void RecalculateTotalBonus()
        {
            _totalStatBonus = new StatBonus();
            foreach (var title in _allTitles)
            {
                if (_acquiredTitleIds.Contains(title.id))
                {
                    _totalStatBonus = _totalStatBonus + title.statBonus;
                }
            }
        }

        /// <summary>
        /// 获取称号ID对应的TitleData
        /// </summary>
        public TitleData GetTitleData(string titleId)
        {
            return _allTitles.Find(t => t.id == titleId);
        }

        /// <summary>
        /// 重置所有称号（用于新游戏）
        /// </summary>
        public void Reset()
        {
            _acquiredTitleIds.Clear();
            _totalStatBonus = new StatBonus();
            foreach (var title in _allTitles)
            {
                title.isAcquired = false;
            }
        }
    }

    // ==================== NPC日程系统 ====================

    /// <summary>
    /// NPC日程数据结构
    /// </summary>
    [Serializable]
    public class NPCScheduleData
    {
        public string npcId;                      // NPC ID
        public TimeOfDay timeOfDay;              // 时间段
        public string location;                  // 位置
        public string activity;                  // 活动
        public bool availableForInteraction;     // 是否可互动
        public List<string> specialDialogues;    // 特殊对话
    }

    /// <summary>
    /// NPC信息
    /// </summary>
    [Serializable]
    public class NPCInfo
    {
        public string id;
        public string name;
        public int affinity;              // 好感度 0-100
        public int maxAffinity;           // 最高好感度
        public bool isRecruited;          // 是否已招募
        public string currentLocation;     // 当前位置
        public TimeOfDay currentTimeOfDay; // 当前时间段
        
        // 对话变化词缀
        public Dictionary<string, float> affinityDialogueMultiplier = new Dictionary<string, float>();
    }

    /// <summary>
    /// NPC日程系统 - 管理所有NPC的日程和位置
    /// </summary>
    public class NPCScheduleSystem
    {
        private static NPCScheduleSystem _instance;
        public static NPCScheduleSystem Instance => _instance ??= new NPCScheduleSystem();

        private Dictionary<string, NPCInfo> _npcInfos;
        private Dictionary<string, List<NPCScheduleData>> _npcSchedules;
        
        // 特殊事件触发器
        private HashSet<string> _triggeredEvents;
        
        public event Action<string, NPCScheduleData> OnNPCLocationChanged;
        public event Action<string> OnSpecialEventTriggered;
        public event Action<string, int> OnAffinityChanged;
        public IReadOnlyDictionary<string, NPCInfo> NPCInfos => _npcInfos;

        private NPCScheduleSystem()
        {
            _npcInfos = new Dictionary<string, NPCInfo>();
            _npcSchedules = new Dictionary<string, List<NPCScheduleData>>();
            _triggeredEvents = new HashSet<string>();
            InitializeNPCs();
            InitializeSchedules();
        }

        /// <summary>
        /// 初始化所有NPC信息
        /// </summary>
        private void InitializeNPCs()
        {
            _npcInfos["martha"] = new NPCInfo { id = "martha", name = "玛莎", affinity = 0, maxAffinity = 100 };
            _npcInfos["lily"] = new NPCInfo { id = "lily", name = "莉莉", affinity = 0, maxAffinity = 100 };
            _npcInfos["eric"] = new NPCInfo { id = "eric", name = "埃里克", affinity = 0, maxAffinity = 100 };
            _npcInfos["ash"] = new NPCInfo { id = "ash", name = "灰烬", affinity = 0, maxAffinity = 100 };
            _npcInfos["woss"] = new NPCInfo { id = "woss", name = "沃斯", affinity = 0, maxAffinity = 100 };
        }

        /// <summary>
        /// 初始化所有NPC日程
        /// </summary>
        private void InitializeSchedules()
        {
            // ==================== 玛莎的日程 ====================
            _npcSchedules["martha"] = new List<NPCScheduleData>
            {
                new NPCScheduleData
                {
                    npcId = "martha",
                    timeOfDay = TimeOfDay.Morning,
                    location = "营地-炼药台",
                    activity = "在营地炼药",
                    availableForInteraction = true,
                    specialDialogues = new List<string> { "morning_potion_tip", "herb_collection_reminder" }
                },
                new NPCScheduleData
                {
                    npcId = "martha",
                    timeOfDay = TimeOfDay.Afternoon,
                    location = "森林-草药采集点",
                    activity = "采集草药",
                    availableForInteraction = true,
                    specialDialogues = new List<string> { "herb_location_tip", "rare_herb_hint" }
                },
                new NPCScheduleData
                {
                    npcId = "martha",
                    timeOfDay = TimeOfDay.Evening,
                    location = "营地-帐篷",
                    activity = "整理背包",
                    availableForInteraction = true,
                    specialDialogues = new List<string> { "inventory_organization_tip", "crafting_advice" }
                },
                new NPCScheduleData
                {
                    npcId = "martha",
                    timeOfDay = TimeOfDay.Night,
                    location = "营地-空地",
                    activity = "独自看星星",
                    availableForInteraction = false,
                    specialDialogues = new List<string> { "stargazing_secret", "constellation_story" }
                }
            };

            // ==================== 莉莉的日程 ====================
            _npcSchedules["lily"] = new List<NPCScheduleData>
            {
                new NPCScheduleData
                {
                    npcId = "lily",
                    timeOfDay = TimeOfDay.Morning,
                    location = "营地-角落",
                    activity = "写日记",
                    availableForInteraction = true,
                    specialDialogues = new List<string> { "journal_entry_hint", "story_fragment" }
                },
                new NPCScheduleData
                {
                    npcId = "lily",
                    timeOfDay = TimeOfDay.Afternoon,
                    location = "森林-各处",
                    activity = "到处探索",
                    availableForInteraction = false,
                    specialDialogues = new List<string> { "exploration_discovery" }
                },
                new NPCScheduleData
                {
                    npcId = "lily",
                    timeOfDay = TimeOfDay.Evening,
                    location = "营地-篝火旁",
                    activity = "找人聊天",
                    availableForInteraction = true,
                    specialDialogues = new List<string> { "campfire_chat", "relationship_building" }
                },
                new NPCScheduleData
                {
                    npcId = "lily",
                    timeOfDay = TimeOfDay.Night,
                    location = "营地-帐篷",
                    activity = "整理记录",
                    availableForInteraction = true,
                    specialDialogues = new List<string> { "record_organization_tip", "hidden_knowledge" }
                }
            };

            // ==================== 埃里克的日程 ====================
            _npcSchedules["eric"] = new List<NPCScheduleData>
            {
                new NPCScheduleData
                {
                    npcId = "eric",
                    timeOfDay = TimeOfDay.Morning,
                    location = "营地-空地",
                    activity = "晨练",
                    availableForInteraction = true,
                    specialDialogues = new List<string> { "training_tip", "combat_advice" }
                },
                new NPCScheduleData
                {
                    npcId = "eric",
                    timeOfDay = TimeOfDay.Afternoon,
                    location = "森林-营地周边",
                    activity = "巡视营地",
                    availableForInteraction = true,
                    specialDialogues = new List<string> { "patrol_route_info", "danger_warning" }
                },
                new NPCScheduleData
                {
                    npcId = "eric",
                    timeOfDay = TimeOfDay.Evening,
                    location = "营地-装备架",
                    activity = "修理装备",
                    availableForInteraction = true,
                    specialDialogues = new List<string> { "repair_service", "equipment_upgrade_tip" }
                },
                new NPCScheduleData
                {
                    npcId = "eric",
                    timeOfDay = TimeOfDay.Night,
                    location = "营地-入口",
                    activity = "站岗",
                    availableForInteraction = false,
                    specialDialogues = new List<string> { "night_watch_warning", "security_info" }
                }
            };

            // ==================== 灰烬的日程 ====================
            _npcSchedules["ash"] = new List<NPCScheduleData>
            {
                // 灰烬不固定，总是出现在意想不到的地方
                new NPCScheduleData
                {
                    npcId = "ash",
                    timeOfDay = TimeOfDay.Morning,
                    location = "???",
                    activity = "行踪不定",
                    availableForInteraction = false,
                    specialDialogues = new List<string> { "mysterious_appearance" }
                },
                new NPCScheduleData
                {
                    npcId = "ash",
                    timeOfDay = TimeOfDay.Afternoon,
                    location = "???",
                    activity = "行踪不定",
                    availableForInteraction = false,
                    specialDialogues = new List<string> { "unpredictable_location" }
                },
                new NPCScheduleData
                {
                    npcId = "ash",
                    timeOfDay = TimeOfDay.Evening,
                    location = "???",
                    activity = "行踪不定",
                    availableForInteraction = false,
                    specialDialogues = new List<string> { "shadow_encounter" }
                },
                new NPCScheduleData
                {
                    npcId = "ash",
                    timeOfDay = TimeOfDay.Night,
                    location = "???",
                    activity = "行踪不定",
                    availableForInteraction = false,
                    specialDialogues = new List<string> { "midnight_meeting" }
                }
            };

            // ==================== 沃斯的日程 ====================
            _npcSchedules["woss"] = new List<NPCScheduleData>
            {
                new NPCScheduleData
                {
                    npcId = "woss",
                    timeOfDay = TimeOfDay.Morning,
                    location = "营地-仓库",
                    activity = "清点货物",
                    availableForInteraction = true,
                    specialDialogues = new List<string> { "inventory_check", "trade_opportunity" }
                },
                new NPCScheduleData
                {
                    npcId = "woss",
                    timeOfDay = TimeOfDay.Afternoon,
                    location = "营地-交易帐篷",
                    activity = "交易",
                    availableForInteraction = true,
                    specialDialogues = new List<string> { "daily_deal", "rare_item_hint" }
                },
                new NPCScheduleData
                {
                    npcId = "woss",
                    timeOfDay = TimeOfDay.Evening,
                    location = "森林-四处",
                    activity = "四处逛",
                    availableForInteraction = false,
                    specialDialogues = new List<string> { "merchant_rumor" }
                },
                new NPCScheduleData
                {
                    npcId = "woss",
                    timeOfDay = TimeOfDay.Night,
                    location = "营地-仓库",
                    activity = "数钱",
                    availableForInteraction = true,
                    specialDialogues = new List<string> { "wealth_management", "investment_advice" }
                }
            };
        }

        /// <summary>
        /// 根据时间段更新所有NPC位置
        /// </summary>
        public void UpdateNPCLocations(TimeOfDay currentTime)
        {
            foreach (var kvp in _npcSchedules)
            {
                var schedule = kvp.Value.Find(s => s.timeOfDay == currentTime);
                if (schedule != null && _npcInfos.ContainsKey(kvp.Key))
                {
                    var npc = _npcInfos[kvp.Key];
                    if (npc.currentLocation != schedule.location)
                    {
                        npc.currentLocation = schedule.location;
                        npc.currentTimeOfDay = schedule.timeOfDay;
                        OnNPCLocationChanged?.Invoke(kvp.Key, schedule);
                    }
                }
            }
        }

        /// <summary>
        /// 获取NPC当前日程
        /// </summary>
        public NPCScheduleData GetNPCSchedule(string npcId, TimeOfDay time)
        {
            if (!_npcSchedules.ContainsKey(npcId))
                return null;
            return _npcSchedules[npcId].Find(s => s.timeOfDay == time);
        }

        /// <summary>
        /// 获取NPC当前位置
        /// </summary>
        public string GetNPCLocation(string npcId)
        {
            return _npcInfos.ContainsKey(npcId) ? _npcInfos[npcId].currentLocation : "";
        }

        /// <summary>
        /// 检查NPC是否可互动
        /// </summary>
        public bool IsNPCInteractable(string npcId, TimeOfDay currentTime)
        {
            var schedule = GetNPCSchedule(npcId, currentTime);
            if (schedule == null)
                return false;

            // 基础检查：日程是否允许互动
            if (!schedule.availableForInteraction)
                return false;

            // 好感度检查：低好感NPC可能拒绝互动
            if (_npcInfos.ContainsKey(npcId))
            {
                var npc = _npcInfos[npcId];
                if (npc.affinity < 20)
                {
                    // 20%概率拒绝互动
                    return UnityEngine.Random.value > 0.2f;
                }
            }

            return true;
        }

        /// <summary>
        /// 获取NPC特殊对话
        /// </summary>
        public string GetSpecialDialogue(string npcId, TimeOfDay currentTime)
        {
            var schedule = GetNPCSchedule(npcId, currentTime);
            if (schedule == null || schedule.specialDialogues.Count == 0)
                return "";

            // 根据好感度选择不同的对话
            var npc = _npcInfos[npcId];
            int dialogueIndex = Mathf.Clamp(npc.affinity / 25, 0, schedule.specialDialogues.Count - 1);
            return schedule.specialDialogues[dialogueIndex];
        }

        /// <summary>
        /// 修改NPC好感度
        /// </summary>
        public void ModifyAffinity(string npcId, int amount)
        {
            if (!_npcInfos.ContainsKey(npcId))
                return;

            var npc = _npcInfos[npcId];
            npc.affinity = Mathf.Clamp(npc.affinity + amount, 0, npc.maxAffinity);
            
            Debug.Log($"[NPCSchedule] {npc.name} 好感度: {npc.affinity} (变化: {amount:+#;-#;0})");
            OnAffinityChanged?.Invoke(npcId, npc.affinity);

            // 触发特定好感度事件
            CheckAffinityMilestones(npcId);
        }

        /// <summary>
        /// 检查好感度里程碑
        /// </summary>
        private void CheckAffinityMilestones(string npcId)
        {
            var npc = _npcInfos[npcId];
            
            // 招募门槛
            if (npc.affinity >= 80 && !npc.isRecruited)
            {
                npc.isRecruited = true;
                Debug.Log($"[NPCSchedule] {npc.name} 已同意加入营地！");
                // 触发招募事件
                OnSpecialEventTriggered?.Invoke($"recruit_{npcId}");
            }

            // 最高好感度触发特殊称号
            if (npc.affinity >= npc.maxAffinity)
            {
                TitleSystem.Instance.CheckAndGrantTitles("max_affinity", 1, npcId);
            }
        }

        /// <summary>
        /// 设置灰烬的随机位置（特殊NPC）
        /// </summary>
        public void SetAshRandomLocation()
        {
            if (!_npcInfos.ContainsKey("ash"))
                return;

            string[] hiddenLocations = {
                "森林-古树顶端",
                "洞穴-最深处的暗室",
                "湖泊-水底遗迹",
                "废墟-地下密室",
                "悬崖-鹰巢",
                "湿地-芦苇深处"
            };

            var npc = _npcInfos["ash"];
            npc.currentLocation = hiddenLocations[UnityEngine.Random.Range(0, hiddenLocations.Length)];
            
            Debug.Log($"[NPCSchedule] 灰烬出现在: {npc.currentLocation}");
        }

        /// <summary>
        /// 检查夜晚特殊事件
        /// </summary>
        public void CheckNightSpecialEvents(TimeOfDay currentTime)
        {
            if (currentTime != TimeOfDay.Night)
                return;

            foreach (var kvp in _npcInfos)
            {
                var npc = kvp.Value;
                if (npc.affinity >= 60)
                {
                    // 高好感NPC夜晚可能触发特殊事件
                    float triggerChance = (npc.affinity - 60) / 100f;
                    if (UnityEngine.Random.value < triggerChance)
                    {
                        string eventId = $"night_event_{npc.id}";
                        if (!_triggeredEvents.Contains(eventId))
                        {
                            _triggeredEvents.Add(eventId);
                            Debug.Log($"[NPCSchedule] 触发夜晚特殊事件: {eventId}");
                            OnSpecialEventTriggered?.Invoke(eventId);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取可互动的NPC列表
        /// </summary>
        public List<NPCInfo> GetInteractableNPCs(TimeOfDay currentTime)
        {
            var result = new List<NPCInfo>();
            foreach (var kvp in _npcInfos)
            {
                if (IsNPCInteractable(kvp.Key, currentTime))
                {
                    result.Add(kvp.Value);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取当前时间段的NPC位置摘要
        /// </summary>
        public Dictionary<string, string> GetNPCLocationSummary(TimeOfDay currentTime)
        {
            var summary = new Dictionary<string, string>();
            foreach (var kvp in _npcSchedules)
            {
                var schedule = kvp.Value.Find(s => s.timeOfDay == currentTime);
                if (schedule != null)
                {
                    summary[kvp.Key] = $"{schedule.location}: {schedule.activity}";
                }
            }
            return summary;
        }

        /// <summary>
        /// 获取NPC信息
        /// </summary>
        public NPCInfo GetNPCInfo(string npcId)
        {
            return _npcInfos.ContainsKey(npcId) ? _npcInfos[npcId] : null;
        }

        /// <summary>
        /// 重置系统
        /// </summary>
        public void Reset()
        {
            foreach (var npc in _npcInfos.Values)
            {
                npc.affinity = 0;
                npc.isRecruited = false;
                npc.currentLocation = "";
            }
            _triggeredEvents.Clear();
        }
    }

    // ==================== 整合系统 ====================
    
    /// <summary>
    /// 称号与日程系统整合管理器
    /// 提供统一的接口访问两个子系统
    /// </summary>
    public class TitleAndDNCSystemManager
    {
        private static TitleAndDNCSystemManager _instance;
        public static TitleAndDNCSystemManager Instance => _instance ??= new TitleAndDNCSystemManager();

        public TitleSystem TitleSystem => TitleSystem.Instance;
        public NPCScheduleSystem ScheduleSystem => NPCScheduleSystem.Instance;

        private TitleAndDNCSystemManager() { }

        /// <summary>
        /// 根据游戏时间更新系统
        /// </summary>
        public void UpdateForTimeOfDay(TimeOfDay currentTime)
        {
            ScheduleSystem.UpdateNPCLocations(currentTime);
            ScheduleSystem.CheckNightSpecialEvents(currentTime);
        }

        /// <summary>
        /// 获取玩家当前享受的所有属性加成
        /// </summary>
        public StatBonus GetTotalPlayerStatBonus()
        {
            return TitleSystem.TotalStatBonus;
        }

        /// <summary>
        /// 检查是否可以与NPC互动
        /// </summary>
        public bool CanInteractWithNPC(string npcId, TimeOfDay currentTime)
        {
            return ScheduleSystem.IsNPCInteractable(npcId, currentTime);
        }

        /// <summary>
        /// 给予NPC礼物并增加好感度
        /// </summary>
        public void GiftNPC(string npcId, int affinityGain)
        {
            ScheduleSystem.ModifyAffinity(npcId, affinityGain);
            TitleSystem.CheckAndGrantTitles("friendships", 1, "gift");
        }

        /// <summary>
        /// 完成与NPC的重要互动
        /// </summary>
        public void CompleteNPCInteraction(string npcId)
        {
            ScheduleSystem.ModifyAffinity(npcId, 5);
            TitleSystem.CheckAndGrantTitles("friendships", 1, "dialogue");
        }

        /// <summary>
        /// 记录敌人击杀
        /// </summary>
        public void RecordEnemyKill(string enemyId)
        {
            TitleSystem.CheckAndGrantTitles("kills", 1, enemyId);
            TitleSystem.CheckAndGrantTitles("kills", 1); // 总击杀数
        }

        /// <summary>
        /// 记录区域发现
        /// </summary>
        public void RecordRegionDiscovery(string regionId)
        {
            TitleSystem.CheckAndGrantTitles("discoveries", 1, regionId);
            TitleSystem.CheckAndGrantTitles("discoveries", 1); // 总发现数
        }

        /// <summary>
        /// 记录物品收集
        /// </summary>
        public void RecordItemCollection(string itemId)
        {
            TitleSystem.CheckAndGrantTitles("collections", 1, itemId);
            TitleSystem.CheckAndGrantTitles("collections", 1); // 总收集数
        }

        /// <summary>
        /// 重置所有系统（新游戏）
        /// </summary>
        public void ResetAll()
        {
            TitleSystem.Reset();
            ScheduleSystem.Reset();
        }
    }
}
