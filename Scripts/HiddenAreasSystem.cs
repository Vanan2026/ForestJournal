using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ForestGame
{
    /// <summary>
    /// 隐藏区域数据
    /// </summary>
    [Serializable]
    public class HiddenArea
    {
        public string id;
        public string name;
        public string description;
        public DiscoveryCondition discoveryCondition;
        public UnlockRequirement unlockRequirement;
        public HiddenContent hiddenContent;
        public bool isDiscovered;
        public bool isUnlocked;
        public float discoveryProgress; // 0-1 用于进度显示

        public HiddenArea() { }

        public HiddenArea(string id, string name, string description)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.isDiscovered = false;
            this.isUnlocked = false;
            this.discoveryProgress = 0f;
        }
    }

    /// <summary>
    /// 发现条件类型
    /// </summary>
    public enum ConditionType
    {
        None,
        ActionAtLocation,      // 在特定位置执行动作
        ItemInInventory,       // 背包持有特定道具
        SkillLevel,            // 技能等级达到
        DaysSurvived,          // 存活天数
        EnemyDefeated,          // 击败特定敌人
        NPCAffinity,           // NPC好感度
        LoreCollected,         // 收集lore数量
        QuestCompleted,        // 完成特定任务
        ItemUsedAtLocation,    // 在位置使用道具
        TimeAndLocation,        // 特定时间+位置
        NGPlus,                // 二周目
        TotalAffection,        // 总体好感度
        MemoryFragments,       // 记忆碎片数量
        SequentialQuests,       // 连续任务完成
        BossDefeated,           // 打败Boss
        EnemyTypeDefeated,      // 击败不同敌人种类
        TrueEndingPrereq       // 真结局前置条件
    }

    /// <summary>
    /// 发现条件
    /// </summary>
    [Serializable]
    public class DiscoveryCondition
    {
        public ConditionType type;
        public string targetId;       // 目标ID (道具/敌人/NPC/位置ID)
        public string locationId;     // 位置ID
        public string action;         // 动作名称
        public int requiredLevel;
        public int requiredCount;
        public int requiredDays;
        public int requiredAffinity;
        public float requiredValue;

        public DiscoveryCondition()
        {
            type = ConditionType.None;
        }

        /// <summary>
        /// 工厂方法：创建位置+动作条件
        /// </summary>
        public static DiscoveryCondition CreateActionAtLocation(string locationId, string action)
        {
            return new DiscoveryCondition
            {
                type = ConditionType.ActionAtLocation,
                locationId = locationId,
                action = action
            };
        }

        /// <summary>
        /// 工厂方法：创建使用道具条件
        /// </summary>
        public static DiscoveryCondition CreateItemUsed(string itemId, string locationId)
        {
            return new DiscoveryCondition
            {
                type = ConditionType.ItemUsedAtLocation,
                targetId = itemId,
                locationId = locationId
            };
        }

        /// <summary>
        /// 工厂方法：创建技能等级条件
        /// </summary>
        public static DiscoveryCondition CreateSkillLevel(string skillId, int level)
        {
            return new DiscoveryCondition
            {
                type = ConditionType.SkillLevel,
                targetId = skillId,
                requiredLevel = level
            };
        }

        /// <summary>
        /// 工厂方法：创建存活天数条件
        /// </summary>
        public static DiscoveryCondition CreateDaysSurvived(int days)
        {
            return new DiscoveryCondition
            {
                type = ConditionType.DaysSurvived,
                requiredDays = days
            };
        }

        /// <summary>
        /// 工厂方法：创建击败敌人条件
        /// </summary>
        public static DiscoveryCondition CreateEnemyDefeated(string enemyId)
        {
            return new DiscoveryCondition
            {
                type = ConditionType.EnemyDefeated,
                targetId = enemyId
            };
        }

        /// <summary>
        /// 工厂方法：创建NPC好感度条件
        /// </summary>
        public static DiscoveryCondition CreateNPCAffinity(string npcId, int affinity)
        {
            return new DiscoveryCondition
            {
                type = ConditionType.NPCAffinity,
                targetId = npcId,
                requiredAffinity = affinity
            };
        }

        /// <summary>
        /// 工厂方法：创建Lore收集数量条件
        /// </summary>
        public static DiscoveryCondition CreateLoreCollected(int count)
        {
            return new DiscoveryCondition
            {
                type = ConditionType.LoreCollected,
                requiredCount = count
            };
        }

        /// <summary>
        /// 工厂方法：创建总体好感度条件
        /// </summary>
        public static DiscoveryCondition CreateTotalAffection(float total)
        {
            return new DiscoveryCondition
            {
                type = ConditionType.TotalAffection,
                requiredValue = total
            };
        }

        /// <summary>
        /// 工厂方法：创建记忆碎片条件
        /// </summary>
        public static DiscoveryCondition CreateMemoryFragments(int count)
        {
            return new DiscoveryCondition
            {
                type = ConditionType.MemoryFragments,
                requiredCount = count
            };
        }

        /// <summary>
        /// 工厂方法：创建连续任务条件
        /// </summary>
        public static DiscoveryCondition CreateSequentialQuests(int count)
        {
            return new DiscoveryCondition
            {
                type = ConditionType.SequentialQuests,
                requiredCount = count
            };
        }

        /// <summary>
        /// 工厂方法：创建击败不同敌人种类条件
        /// </summary>
        public static DiscoveryCondition CreateEnemyTypeDefeated(int typeCount)
        {
            return new DiscoveryCondition
            {
                type = ConditionType.EnemyTypeDefeated,
                requiredCount = typeCount
            };
        }

        /// <summary>
        /// 工厂方法：创建时间+位置条件
        /// </summary>
        public static DiscoveryCondition CreateTimeAndLocation(int day, string locationId)
        {
            return new DiscoveryCondition
            {
                type = ConditionType.TimeAndLocation,
                requiredDays = day,
                locationId = locationId
            };
        }

        /// <summary>
        /// 工厂方法：创建二周目条件
        /// </summary>
        public static DiscoveryCondition CreateNGPlus()
        {
            return new DiscoveryCondition
            {
                type = ConditionType.NGPlus
            };
        }

        /// <summary>
        /// 工厂方法：创建持有道具条件
        /// </summary>
        public static DiscoveryCondition CreateItemInInventory(string itemId)
        {
            return new DiscoveryCondition
            {
                type = ConditionType.ItemInInventory,
                targetId = itemId
            };
        }

        /// <summary>
        /// 工厂方法：创建真结局前置条件
        /// </summary>
        public static DiscoveryCondition CreateTrueEndingPrereq()
        {
            return new DiscoveryCondition
            {
                type = ConditionType.TrueEndingPrereq
            };
        }
    }

    /// <summary>
    /// 解锁需求（可选，发现后还需额外条件解锁内容）
    /// </summary>
    [Serializable]
    public class UnlockRequirement
    {
        public bool hasRequirement;
        public ConditionType type;
        public string targetId;
        public int requiredCount;
        public float requiredValue;

        public static UnlockRequirement CreateNone()
        {
            return new UnlockRequirement { hasRequirement = false };
        }

        public static UnlockRequirement CreateItemCount(string itemId, int count)
        {
            return new UnlockRequirement
            {
                hasRequirement = true,
                type = ConditionType.ItemInInventory,
                targetId = itemId,
                requiredCount = count
            };
        }
    }

    /// <summary>
    /// 隐藏内容类型
    /// </summary>
    public enum HiddenContentType
    {
        RareResource,    // 稀有资源
        SpecialEnemy,    // 特殊敌人
        NPC,             // NPC
        Quest,           // 任务
        Lore,            // 背景故事
        EndingClue,      // 结局线索
        Recipe,          // 配方
        Item,            // 道具
        LocationLink,    // 区域链接
        EquipmentUpgrade // 装备升级
    }

    /// <summary>
    /// 隐藏内容
    /// </summary>
    [Serializable]
    public class HiddenContent
    {
        public HiddenContentType type;
        public string id;
        public string name;
        public string description;
        public List<HiddenContentType> contentTypes; // 支持多种类型
        public List<string> rewards; // 奖励列表
        public bool isReusable; // 是否可重复触发

        public HiddenContent()
        {
            contentTypes = new List<HiddenContentType>();
            rewards = new List<string>();
        }

        public static HiddenContent CreateRareResource(string id, string name, string description)
        {
            return new HiddenContent
            {
                type = HiddenContentType.RareResource,
                id = id,
                name = name,
                description = description,
                contentTypes = new List<HiddenContentType> { HiddenContentType.RareResource },
                isReusable = true
            };
        }

        public static HiddenContent CreateNPC(string id, string name, string description)
        {
            return new HiddenContent
            {
                type = HiddenContentType.NPC,
                id = id,
                name = name,
                description = description,
                contentTypes = new List<HiddenContentType> { HiddenContentType.NPC },
                isReusable = true
            };
        }

        public static HiddenContent CreateQuest(string id, string name, string description, List<string> rewards)
        {
            return new HiddenContent
            {
                type = HiddenContentType.Quest,
                id = id,
                name = name,
                description = description,
                contentTypes = new List<HiddenContentType> { HiddenContentType.Quest },
                rewards = rewards,
                isReusable = false
            };
        }

        public static HiddenContent CreateLore(string id, string name, string description)
        {
            return new HiddenContent
            {
                type = HiddenContentType.Lore,
                id = id,
                name = name,
                description = description,
                contentTypes = new List<HiddenContentType> { HiddenContentType.Lore },
                isReusable = false
            };
        }

        public static HiddenContent CreateEndingClue(string id, string name, string description)
        {
            return new HiddenContent
            {
                type = HiddenContentType.EndingClue,
                id = id,
                name = name,
                description = description,
                contentTypes = new List<HiddenContentType> { HiddenContentType.EndingClue },
                isReusable = false
            };
        }
    }

    /// <summary>
    /// 隐藏区域系统 - 管理所有隐藏区域和彩蛋
    /// </summary>
    public class HiddenAreasSystem : MonoBehaviour
    {
        public static HiddenAreasSystem instance { get; private set; }

        [Header("所有隐藏区域")]
        public List<HiddenArea> hiddenAreas = new List<HiddenArea>();

        [Header("已发现区域")]
        public List<string> discoveredAreaIds = new List<string>();

        [Header("已解锁内容")]
        public List<string> unlockedContentIds = new List<string>();

        [Header("区域检查间隔")]
        public float checkInterval = 0.5f;

        private Dictionary<string, HiddenArea> areaDict;
        private Coroutine checkCoroutine;

        // 事件
        public event Action<HiddenArea> OnAreaDiscovered;
        public event Action<HiddenArea> OnAreaUnlocked;
        public event Action<HiddenArea, HiddenContent> OnContentRevealed;
        public event Action<string, string> OnHintGenerated; // areaId, hintMessage

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAreas();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.onExploreLocation += CheckDiscoveryAtLocation;
                GameManager.instance.onDayChanged += OnDayChanged;
                GameManager.instance.onItemUsed += OnItemUsed;
            }

            checkCoroutine = StartCoroutine(AutoCheckConditions());
        }

        void OnDestroy()
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.onExploreLocation -= CheckDiscoveryAtLocation;
                GameManager.instance.onDayChanged -= OnDayChanged;
                GameManager.instance.onItemUsed -= OnItemUsed;
            }

            if (checkCoroutine != null)
                StopCoroutine(checkCoroutine);
        }

        /// <summary>
        /// 初始化所有20个隐藏区域
        /// </summary>
        private void InitializeAreas()
        {
            hiddenAreas = new List<HiddenArea>
            {
                // 1. 瀑布后洞穴
                CreateArea("waterfall_cave", "瀑布后洞穴",
                    "瀑布后方隐藏着一个古老的洞穴入口",
                    DiscoveryCondition.CreateActionAtLocation("waterfall", "调查"),
                    UnlockRequirement.CreateNone(),
                    HiddenContent.CreateRareResource("ancient_ore", "古老矿石", "蕴含神秘能量的稀有矿石"),
                    HiddenContent.CreateLore("artifact_clue", "古物线索", "记载着森林起源的碎片")
                ),

                // 2. 巨型树根网络
                CreateArea("giant_roots", "巨型树根网络",
                    "巨大的树根在地底蔓延，形成复杂的通道网络",
                    DiscoveryCondition.CreateItemInInventory("bone_hoe"),
                    UnlockRequirement.CreateNone(),
                    HiddenContent.CreateRareResource("root_network_access", "根网通道", "连接各区域的地下通道"),
                    HiddenContent.CreateItem("shadow_glade_key", "暗影溪谷钥匙", "开启暗影溪谷的钥匙")
                ),

                // 3. 骷髅祭坛
                CreateArea("skeleton_altar", "骷髅祭坛",
                    "废墟深处的神秘祭坛，周围散落着骷髅的残骸",
                    DiscoveryCondition.CreateActionAtLocation("ruins", "调查"),
                    UnlockRequirement.CreateNone(),
                    HiddenContent.CreateRecipe("bone_equipment", "骨制装备配方", "制作骨制武器和护甲的古老配方"),
                    HiddenContent.CreateLore("war_history", "战争历史", "记载着森林古老战争的真相")
                ),

                // 4. 沉没村庄
                CreateArea("sunken_village", "沉没村庄",
                    "暴雨后浮出水面的古老村庄遗迹",
                    DiscoveryCondition.CreateActionAtLocation("riverbank", "调查"),
                    UnlockRequirement.CreateNone(),
                    HiddenContent.CreateRareResource("relic", "古旧物", "从水底打捞上来的珍贵遗物"),
                    HiddenContent.CreateLore("village_lore", "村庄记忆", "记录着村庄沉没前的最后时光")
                ),

                // 5. 悬空平台
                CreateArea("sky_platform", "悬空平台",
                    "高耸入云的天然平台，可俯瞰整片森林",
                    DiscoveryCondition.CreateItemUsed("telescope", "high_point"),
                    UnlockRequirement.CreateNone(),
                    HiddenContent.CreateRareResource("forest_heart_location", "森林之心位置", "标记了森林之心的精确位置"),
                    HiddenContent.CreateItem("sky_material", "天空素材", "只在高空才能获得的稀有材料")
                ),

                // 6. 镜面池塘
                CreateArea("mirror_pond", "镜面池塘",
                    "平静如镜的水面，倒映出另一个世界的景象",
                    DiscoveryCondition.CreateItemUsed("forest_essence", "pond"),
                    UnlockRequirement.CreateNone(),
                    HiddenContent.CreateLore("parallel_world", "平行世界线索", "显示了一个平行森林的画面"),
                    HiddenContent.CreateEndingClue("mirror_revelation", "镜像启示", "暗示了世界的真相")
                ),

                // 7. 倒长树木
                CreateArea("inverted_tree", "倒长树木",
                    "违背重力的树木，枝干向下生长进入地底",
                    DiscoveryCondition.CreateItemUsed("anti_gravity_seed", "dark_valley"),
                    UnlockRequirement.CreateNone(),
                    HiddenContent.CreateSpecialEnemy("gravity_enemy", "重力异兽", "适应了异常重力的可怕生物"),
                    HiddenContent.CreateRareResource("rare_plant", "逆生草", "在倒转空间中生长的珍稀草药")
                ),

                // 8. 流星陨坑
                CreateArea("meteor_crater", "流星陨坑",
                    "第七天夜晚坠落的流星留下的巨大坑洞",
                    DiscoveryCondition.CreateTimeAndLocation(7, "dark_fog_front"),
                    UnlockRequirement.CreateNone(),
                    HiddenContent.CreateItem("meteor_shard", "陨星碎片", "蕴含天外之力的神秘碎片"),
                    HiddenContent.CreateLore("meteor_lore", "流星传说", "记载着流星降临的古老预言")
                ),

                // 9. 古老战壕
                CreateArea("ancient_trench", "古老战壕",
                    "古代战争中留下的防御工事遗迹",
                    DiscoveryCondition.CreateSkillLevel("archaeology", 2),
                    UnlockRequirement.CreateNone(),
                    HiddenContent.CreateLore("erik_backstory", "埃里克背景", "揭示埃里克真实身份的线索"),
                    HiddenContent.CreateItem("ancient_weapon", "古战武器", "一把锈迹斑斑但依然锋利的武器")
                ),

                // 10. 石碑林
                CreateArea("stone_monument", "石碑林",
                    "二十块石碑排列成神秘的阵型，记录着森林的历史",
                    DiscoveryCondition.CreateLoreCollected(20),
                    UnlockRequirement.CreateNone(),
                    HiddenContent.CreateLore("forest_history", "森林全史", "所有Lore的完整汇总"),
                    HiddenContent.CreateEndingClue("history_truth", "历史真相", "揭示森林深层秘密的碑文")
                ),

                // 11. 黑雾漩涡
                CreateArea("dark_vortex", "黑雾漩涡",
                    "森林之心中央的黑雾核心，蕴含着未知的恐怖",
                    DiscoveryCondition.CreateItemUsed("soul_essence_10", "forest_heart"),
                    UnlockRequirement.CreateNone(),
                    HiddenContent.CreateLore("dark_origin", "黑雾起源", "揭示黑雾真正来源的记录"),
                    HiddenContent.CreateEndingClue("dark_revelation", "黑暗启示", "关于黑雾本质的终极真相")
                ),

                // 12. 宁静花园
                CreateArea("peaceful_garden", "宁静花园",
                    "隐藏在各区域交汇处的秘密花园，NPC们在这里相聚",
                    DiscoveryCondition.CreateTotalAffection(200f),
                    UnlockRequirement.CreateNone(),
                    HiddenContent.CreateNPC("all_npcs", "所有NPC", "所有可交朋友的NPC聚集地"),
                    HiddenContent.CreateRareResource("friendship_flower", "友谊之花", "只有真正朋友才能摘下的花朵")
                ),

                // 13. 记忆之树
                CreateArea("memory_tree", "记忆之树",
                    "挂满了发光记忆碎片的神圣古树",
                    DiscoveryCondition.CreateMemoryFragments(30),
                    UnlockRequirement.CreateNone(),
                    HiddenContent.CreateLore("player_memory", "玩家记忆", "关于玩家过去的重要记忆片段"),
                    HiddenContent.CreateEndingClue("ending_key", "结局关键", "解锁真结局的必要线索")
                ),

                // 14. 地精工坊
                CreateArea("goblin_workshop", "地精工坊",
                    "隐藏在地下的小型工坊，地精工匠的秘密基地",
                    DiscoveryCondition.CreateSequentialQuests(10),
                    UnlockRequirement.CreateNone(),
                    HiddenContent.CreateNPC("goblin_smith", "地精工匠", "出售稀有配方和特殊装备的商人"),
                    HiddenContent.CreateRecipe("secret_recipe", "秘密配方", "包含游戏中最稀有配方的配方书")
                ),

                // 15. 镜像迷宫
                CreateArea("mirror_labyrinth", "镜像迷宫",
                    "由幽灵构成的迷宫，只有最敏捷的猎手才能通过",
                    DiscoveryCondition.CreateEnemyDefeated("shadow_hunter"),
                    UnlockRequirement.CreateNone(),
                    HiddenContent.CreateSpecialEnemy("ghost_wolf", "幽灵狼", "守护迷宫的幽灵生物群"),
                    HiddenContent.CreateItem("mirror_shard", "镜之碎片", "逃离迷宫的关键道具")
                ),

                // 16. 灵魂熔炉
                CreateArea("soul_furnace", "灵魂熔炉",
                    "用灵魂作为燃料的古老熔炉，可强化装备",
                    DiscoveryCondition.CreateEnemyTypeDefeated(10),
                    UnlockRequirement.CreateNone(),
                    HiddenContent.CreateEquipmentUpgrade("upgrade_3", "+3强化", "将装备强化至+3的熔炉"),
                    HiddenContent.CreateLore("furnace_lore", "熔炉传说", "关于灵魂熔炉来历的记载")
                ),

                // 17. 预言者小屋
                CreateArea("prophet_cottage", "预言者小屋",
                    "森林深处的神秘小屋，里面住着知晓未来的老太太",
                    DiscoveryCondition.CreateDaysSurvived(15),
                    UnlockRequirement.CreateNone(),
                    HiddenContent.CreateNPC("prophet_grandma", "预言者奶奶", "可以预知未来事件的智者"),
                    HiddenContent.CreateQuest("future_hint", "未来提示", "奶奶给予的关于未来的暗示任务")
                ),

                // 18. 禁忌图书馆
                CreateArea("forbidden_library", "禁忌图书馆",
                    "被封印的图书馆，收藏着关于黑雾的禁忌知识",
                    DiscoveryCondition.CreateNPCAffinity("lily", 60),
                    UnlockRequirement.CreateNone(),
                    HiddenContent.CreateLore("dark_history", "黑雾历史", "记载黑雾起源和本质的禁书"),
                    HiddenContent.CreateEndingClue("forbidden_truth", "禁忌真相", "关于世界末日的预言")
                ),

                // 19. 时间裂隙
                CreateArea("time_rift", "时间裂隙",
                    "时空扭曲产生的裂隙，可回到过去",
                    DiscoveryCondition.CreateNGPlus(),
                    UnlockRequirement.CreateNone(),
                    HiddenContent.CreateQuest("new_game_plus", "二周目任务", "回到原点但保持一切属性"),
                    HiddenContent.CreateLore("time_lore", "时间悖论", "关于时间循环的解释")
                ),

                // 20. 最终试炼场
                CreateArea("final_trial", "最终试炼场",
                    "只有真正的勇者才能进入的最终战场",
                    DiscoveryCondition.CreateTrueEndingPrereq(),
                    UnlockRequirement.CreateNone(),
                    HiddenContent.CreateSpecialEnemy("final_boss", "最终Boss", "毁灭森林的元凶"),
                    HiddenContent.CreateEndingClue("true_ending", "真结局", "触发真结局的必要条件")
                )
            };

            // 构建字典便于快速查找
            areaDict = new Dictionary<string, HiddenArea>();
            foreach (var area in hiddenAreas)
            {
                areaDict[area.id] = area;
            }
        }

        /// <summary>
        /// 创建隐藏区域的辅助方法
        /// </summary>
        private HiddenArea CreateArea(string id, string name, string description,
            DiscoveryCondition discovery, UnlockRequirement unlock,
            HiddenContent primary, HiddenContent secondary = null)
        {
            HiddenArea area = new HiddenArea(id, name, description);
            area.discoveryCondition = discovery;
            area.unlockRequirement = unlock;

            if (secondary != null)
            {
                area.hiddenContent = new HiddenContent
                {
                    type = primary.type,
                    id = primary.id,
                    name = primary.name,
                    description = primary.description,
                    contentTypes = new List<HiddenContentType> { primary.type, secondary.type },
                    rewards = new List<string>(),
                    isReusable = primary.isReusable || secondary.isReusable
                };
                if (!string.IsNullOrEmpty(primary.id)) area.hiddenContent.rewards.Add(primary.id);
                if (!string.IsNullOrEmpty(secondary.id)) area.hiddenContent.rewards.Add(secondary.id);
            }
            else
            {
                area.hiddenContent = primary;
            }

            return area;
        }

        /// <summary>
        /// 在特定位置检查发现条件
        /// </summary>
        private void CheckDiscoveryAtLocation(string locationId, string action)
        {
            foreach (var area in hiddenAreas)
            {
                if (area.isDiscovered) continue;

                if (area.discoveryCondition.type == ConditionType.ActionAtLocation &&
                    area.discoveryCondition.locationId == locationId &&
                    area.discoveryCondition.action == action)
                {
                    DiscoverArea(area);
                }
                else if (area.discoveryCondition.type == ConditionType.ItemUsedAtLocation &&
                         area.discoveryCondition.locationId == locationId)
                {
                    // 物品使用触发
                    DiscoverArea(area);
                }
                else if (area.discoveryCondition.type == ConditionType.TimeAndLocation &&
                         area.discoveryCondition.locationId == locationId)
                {
                    if (GameManager.instance != null &&
                        GameManager.instance.currentDay >= area.discoveryCondition.requiredDays)
                    {
                        DiscoverArea(area);
                    }
                }
            }
        }

        /// <summary>
        /// 物品使用时检查
        /// </summary>
        private void OnItemUsed(string itemId, string locationId)
        {
            foreach (var area in hiddenAreas)
            {
                if (area.isDiscovered) continue;

                if (area.discoveryCondition.type == ConditionType.ItemUsedAtLocation &&
                    area.discoveryCondition.targetId == itemId &&
                    area.discoveryCondition.locationId == locationId)
                {
                    DiscoverArea(area);
                }
                else if (area.discoveryCondition.type == ConditionType.ItemInInventory &&
                         area.discoveryCondition.targetId == itemId)
                {
                    DiscoverArea(area);
                }
            }
        }

        /// <summary>
        /// 天数变化时检查
        /// </summary>
        private void OnDayChanged(int newDay)
        {
            foreach (var area in hiddenAreas)
            {
                if (area.isDiscovered) continue;

                if (area.discoveryCondition.type == ConditionType.DaysSurvived &&
                    newDay >= area.discoveryCondition.requiredDays)
                {
                    DiscoverArea(area);
                }
                else if (area.discoveryCondition.type == ConditionType.TimeAndLocation &&
                         newDay >= area.discoveryCondition.requiredDays)
                {
                    // 时间和位置都满足时发现
                    if (GameManager.instance != null &&
                        GameManager.instance.currentLocationId == area.discoveryCondition.locationId)
                    {
                        DiscoverArea(area);
                    }
                }
            }
        }

        /// <summary>
        /// 自动检查各种条件
        /// </summary>
        private IEnumerator AutoCheckConditions()
        {
            while (true)
            {
                yield return new WaitForSeconds(checkInterval);

                if (GameManager.instance == null) continue;

                foreach (var area in hiddenAreas)
                {
                    if (area.isDiscovered) continue;

                    bool shouldDiscover = false;

                    switch (area.discoveryCondition.type)
                    {
                        case ConditionType.SkillLevel:
                            shouldDiscover = CheckSkillLevel(area.discoveryCondition);
                            break;

                        case ConditionType.NPCAffinity:
                            shouldDiscover = CheckNPCAffinity(area.discoveryCondition);
                            break;

                        case ConditionType.TotalAffection:
                            shouldDiscover = CheckTotalAffection(area.discoveryCondition);
                            break;

                        case ConditionType.LoreCollected:
                            shouldDiscover = CheckLoreCollected(area.discoveryCondition);
                            break;

                        case ConditionType.MemoryFragments:
                            shouldDiscover = CheckMemoryFragments(area.discoveryCondition);
                            break;

                        case ConditionType.SequentialQuests:
                            shouldDiscover = CheckSequentialQuests(area.discoveryCondition);
                            break;

                        case ConditionType.EnemyTypeDefeated:
                            shouldDiscover = CheckEnemyTypeDefeated(area.discoveryCondition);
                            break;

                        case ConditionType.NGPlus:
                            shouldDiscover = CheckNGPlus();
                            break;

                        case ConditionType.TrueEndingPrereq:
                            shouldDiscover = CheckTrueEndingPrereq();
                            break;

                        case ConditionType.EnemyDefeated:
                            shouldDiscover = CheckEnemyDefeated(area.discoveryCondition);
                            break;
                    }

                    if (shouldDiscover)
                    {
                        DiscoverArea(area);
                    }
                }
            }
        }

        /// <summary>
        /// 检查技能等级
        /// </summary>
        private bool CheckSkillLevel(DiscoveryCondition cond)
        {
            if (GameManager.instance == null) return false;

            var skillSystem = GameManager.instance.GetComponent<SkillSystem>();
            if (skillSystem != null)
            {
                int currentLevel = skillSystem.GetSkillLevel(cond.targetId);
                return currentLevel >= cond.requiredLevel;
            }

            return false;
        }

        /// <summary>
        /// 检查NPC好感度
        /// </summary>
        private bool CheckNPCAffinity(DiscoveryCondition cond)
        {
            if (GameManager.instance == null) return false;

            var npcSystem = GameManager.instance.GetComponent<NPCSystem>();
            if (npcSystem != null)
            {
                int affinity = npcSystem.GetNPCAffinity(cond.targetId);
                return affinity >= cond.requiredAffinity;
            }

            return false;
        }

        /// <summary>
        /// 检查总体好感度
        /// </summary>
        private bool CheckTotalAffection(DiscoveryCondition cond)
        {
            if (GameManager.instance == null) return false;

            var npcSystem = GameManager.instance.GetComponent<NPCSystem>();
            if (npcSystem != null)
            {
                float total = npcSystem.GetTotalAffection();
                return total >= cond.requiredValue;
            }

            return false;
        }

        /// <summary>
        /// 检查Lore收集数量
        /// </summary>
        private bool CheckLoreCollected(DiscoveryCondition cond)
        {
            if (GameManager.instance == null) return false;

            var loreSystem = GameManager.instance.GetComponent<LoreSystem>();
            if (loreSystem != null)
            {
                int count = loreSystem.GetCollectedLoreCount();
                return count >= cond.requiredCount;
            }

            return false;
        }

        /// <summary>
        /// 检查记忆碎片数量
        /// </summary>
        private bool CheckMemoryFragments(DiscoveryCondition cond)
        {
            if (GameManager.instance == null) return false;

            var memorySystem = GameManager.instance.GetComponent<MemorySystem>();
            if (memorySystem != null)
            {
                int count = memorySystem.GetFragmentCount();
                return count >= cond.requiredCount;
            }

            return false;
        }

        /// <summary>
        /// 检查连续任务完成数
        /// </summary>
        private bool CheckSequentialQuests(DiscoveryCondition cond)
        {
            if (GameManager.instance == null) return false;

            var questSystem = GameManager.instance.GetComponent<QuestSystem>();
            if (questSystem != null)
            {
                int count = questSystem.GetCompletedSequentialQuestCount();
                return count >= cond.requiredCount;
            }

            return false;
        }

        /// <summary>
        /// 检查击败不同敌人种类数
        /// </summary>
        private bool CheckEnemyTypeDefeated(DiscoveryCondition cond)
        {
            if (GameManager.instance == null) return false;

            var battleSystem = GameManager.instance.GetComponent<BattleSystem>();
            if (battleSystem != null)
            {
                int count = battleSystem.GetDefeatedEnemyTypeCount();
                return count >= cond.requiredCount;
            }

            return false;
        }

        /// <summary>
        /// 检查二周目
        /// </summary>
        private bool CheckNGPlus()
        {
            if (GameManager.instance == null) return false;
            return GameManager.instance.isNewGamePlus;
        }

        /// <summary>
        /// 检查真结局前置条件
        /// </summary>
        private bool CheckTrueEndingPrereq()
        {
            if (GameManager.instance == null) return false;

            // 检查所有结局前置条件是否满足
            int prereqCount = 0;
            int requiredPrereqs = 5; // 假设需要满足5个前置条件

            // 检查1：已发现至少10个隐藏区域
            if (discoveredAreaIds.Count >= 10) prereqCount++;

            // 检查2：持有陨星碎片
            if (GameManager.instance.HasItem("meteor_shard")) prereqCount++;

            // 检查3：莉莉好感度>80
            var npcSystem = GameManager.instance.GetComponent<NPCSystem>();
            if (npcSystem != null && npcSystem.GetNPCAffinity("lily") > 80) prereqCount++;

            // 检查4：记忆碎片>50
            var memorySystem = GameManager.instance.GetComponent<MemorySystem>();
            if (memorySystem != null && memorySystem.GetFragmentCount() > 50) prereqCount++;

            // 检查5：打败暗影猎手
            var battleSystem = GameManager.instance.GetComponent<BattleSystem>();
            if (battleSystem != null && battleSystem.HasDefeatedEnemy("shadow_hunter")) prereqCount++;

            return prereqCount >= requiredPrereqs;
        }

        /// <summary>
        /// 检查击败特定敌人
        /// </summary>
        private bool CheckEnemyDefeated(DiscoveryCondition cond)
        {
            if (GameManager.instance == null) return false;

            var battleSystem = GameManager.instance.GetComponent<BattleSystem>();
            if (battleSystem != null)
            {
                return battleSystem.HasDefeatedEnemy(cond.targetId);
            }

            return false;
        }

        /// <summary>
        /// 发现隐藏区域
        /// </summary>
        public void DiscoverArea(HiddenArea area)
        {
            if (area.isDiscovered) return;

            area.isDiscovered = true;

            if (!discoveredAreaIds.Contains(area.id))
            {
                discoveredAreaIds.Add(area.id);
            }

            Debug.Log($"[隐藏区域发现] {area.name}: {area.description}");

            // 触发UI显示
            ShowDiscoveryUI(area);

            // 触发事件
            OnAreaDiscovered?.Invoke(area);

            // 检查是否可以立即解锁
            TryUnlockArea(area);
        }

        /// <summary>
        /// 尝试解锁区域内容
        /// </summary>
        public void TryUnlockArea(HiddenArea area)
        {
            if (area.isUnlocked || !area.isDiscovered) return;

            if (!area.unlockRequirement.hasRequirement)
            {
                UnlockArea(area);
                return;
            }

            // 检查解锁条件
            bool canUnlock = false;

            if (area.unlockRequirement.type == ConditionType.ItemInInventory)
            {
                if (GameManager.instance != null)
                {
                    canUnlock = GameManager.instance.HasItem(area.unlockRequirement.targetId);
                }
            }

            if (canUnlock)
            {
                UnlockArea(area);
            }
        }

        /// <summary>
        /// 解锁隐藏区域
        /// </summary>
        public void UnlockArea(HiddenArea area)
        {
            if (area.isUnlocked) return;

            area.isUnlocked = true;

            Debug.Log($"[隐藏区域解锁] {area.name} 的内容已解锁！");

            // 显示解锁UI
            ShowUnlockUI(area);

            // 触发事件
            OnAreaUnlocked?.Invoke(area);

            // 揭示内容
            RevealContent(area, area.hiddenContent);
        }

        /// <summary>
        /// 揭示隐藏内容
        /// </summary>
        public void RevealContent(HiddenArea area, HiddenContent content)
        {
            if (content == null) return;

            if (!unlockedContentIds.Contains(content.id))
            {
                unlockedContentIds.Add(content.id);
            }

            Debug.Log($"[隐藏内容揭示] {area.name}: {content.name} - {content.description}");

            // 显示内容UI
            ShowContentUI(area, content);

            // 触发事件
            OnContentRevealed?.Invoke(area, content);

            // 根据内容类型给予奖励
            GrantContentRewards(content);
        }

        /// <summary>
        /// 给予内容奖励
        /// </summary>
        private void GrantContentRewards(HiddenContent content)
        {
            if (GameManager.instance == null) return;

            foreach (var rewardId in content.rewards)
            {
                if (!string.IsNullOrEmpty(rewardId))
                {
                    GameManager.instance.AddItem(rewardId);
                }
            }
        }

        /// <summary>
        /// 显示发现UI
        /// </summary>
        private void ShowDiscoveryUI(HiddenArea area)
        {
            if (UIManager.instance != null)
            {
                UIManager.instance.ShowHiddenAreaDiscovery(area);
            }
            else
            {
                // 备用：显示通用提示
                Debug.LogWarning("[UI] UIManager未找到，使用控制台提示");
                NotificationSystem.instance?.ShowNotification(
                    $"✨ 发现隐藏区域: {area.name}",
                    area.description,
                    NotificationType.HiddenArea
                );
            }
        }

        /// <summary>
        /// 显示解锁UI
        /// </summary>
        private void ShowUnlockUI(HiddenArea area)
        {
            NotificationSystem.instance?.ShowNotification(
                $"🔓 隐藏区域解锁: {area.name}",
                "新的内容可以探索了！",
                NotificationType.Unlock
            );
        }

        /// <summary>
        /// 显示内容UI
        /// </summary>
        private void ShowContentUI(HiddenArea area, HiddenContent content)
        {
            string typeName = GetContentTypeName(content.type);
            NotificationSystem.instance?.ShowNotification(
                $"📜 {typeName}揭示: {content.name}",
                content.description,
                NotificationType.Discovery
            );
        }

        /// <summary>
        /// 获取内容类型名称
        /// </summary>
        private string GetContentTypeName(HiddenContentType type)
        {
            switch (type)
            {
                case HiddenContentType.RareResource: return "稀有资源";
                case HiddenContentType.SpecialEnemy: return "特殊敌人";
                case HiddenContentType.NPC: return "NPC";
                case HiddenContentType.Quest: return "任务";
                case HiddenContentType.Lore: return "背景故事";
                case HiddenContentType.EndingClue: return "结局线索";
                case HiddenContentType.Recipe: return "配方";
                case HiddenContentType.Item: return "道具";
                case HiddenContentType.LocationLink: return "区域链接";
                case HiddenContentType.EquipmentUpgrade: return "装备升级";
                default: return "未知";
            }
        }

        /// <summary>
        /// 获取区域提示
        /// </summary>
        public string GetAreaHint(string areaId)
        {
            if (!areaDict.ContainsKey(areaId)) return null;

            var area = areaDict[areaId];
            if (area.isDiscovered) return null;

            // 根据未满足的条件生成提示
            switch (area.discoveryCondition.type)
            {
                case ConditionType.ActionAtLocation:
                    return $"在 {area.discoveryCondition.locationId} 使用「{area.discoveryCondition.action}」动作可能会发现什么...";

                case ConditionType.ItemUsedAtLocation:
                    return $"如果能在 {area.discoveryCondition.locationId} 使用正确的物品...";

                case ConditionType.SkillLevel:
                    return $"提升 {area.discoveryCondition.targetId} 技能到 {area.discoveryCondition.requiredLevel} 级可能会有新发现";

                case ConditionType.DaysSurvived:
                    return $"再坚持 {area.discoveryCondition.requiredDays - (GameManager.instance?.currentDay ?? 0)} 天也许会有奇迹";

                case ConditionType.NPCAffinity:
                    return $"加深与 {area.discoveryCondition.targetId} 的友谊（好感度 {area.discoveryCondition.requiredAffinity}+）";

                case ConditionType.LoreCollected:
                    int currentLore = 0;
                    if (GameManager.instance != null)
                    {
                        var loreSystem = GameManager.instance.GetComponent<LoreSystem>();
                        if (loreSystem != null) currentLore = loreSystem.GetCollectedLoreCount();
                    }
                    return $"收集更多 Lore（当前 {currentLore}/{area.discoveryCondition.requiredCount}）";

                case ConditionType.TotalAffection:
                    return $"与所有NPC建立更深的友谊（好感度总计 >{area.discoveryCondition.requiredValue}）";

                case ConditionType.MemoryFragments:
                    int currentMem = 0;
                    if (GameManager.instance != null)
                    {
                        var memSystem = GameManager.instance.GetComponent<MemorySystem>();
                        if (memSystem != null) currentMem = memSystem.GetFragmentCount();
                    }
                    return $"收集更多记忆碎片（当前 {currentMem}/{area.discoveryCondition.requiredCount}）";

                case ConditionType.TimeAndLocation:
                    return $"第七天夜晚前往 {area.discoveryCondition.locationId}...";

                case ConditionType.NGPlus:
                    return "在新的旅程中寻找...";

                case ConditionType.EnemyDefeated:
                    return $"击败 {area.discoveryCondition.targetId} 后可能会有新发现";

                case ConditionType.EnemyTypeDefeated:
                    return $"击败更多种类的敌人（{area.discoveryCondition.requiredCount} 种）";

                case ConditionType.TrueEndingPrereq:
                    return "完成所有准备工作，迎接最终挑战...";

                default:
                    return "某个隐藏的秘密正在等待被发现...";
            }
        }

        /// <summary>
        /// 获取已发现区域列表
        /// </summary>
        public List<HiddenArea> GetDiscoveredAreas()
        {
            return hiddenAreas.FindAll(a => a.isDiscovered);
        }

        /// <summary>
        /// 获取已解锁区域列表
        /// </summary>
        public List<HiddenArea> GetUnlockedAreas()
        {
            return hiddenAreas.FindAll(a => a.isUnlocked);
        }

        /// <summary>
        /// 获取区域统计
        /// </summary>
        public HiddenAreaStats GetStats()
        {
            return new HiddenAreaStats
            {
                totalAreas = hiddenAreas.Count,
                discoveredCount = discoveredAreaIds.Count,
                unlockedCount = unlockedContentIds.Count,
                discoveryRate = hiddenAreas.Count > 0 ? (float)discoveredAreaIds.Count / hiddenAreas.Count : 0f
            };
        }

        /// <summary>
        /// 根据ID获取区域
        /// </summary>
        public HiddenArea GetArea(string id)
        {
            return areaDict.ContainsKey(id) ? areaDict[id] : null;
        }

        /// <summary>
        /// 检查区域是否被发现
        /// </summary>
        public bool IsDiscovered(string id)
        {
            return areaDict.ContainsKey(id) && areaDict[id].isDiscovered;
        }

        /// <summary>
        /// 检查区域内容是否解锁
        /// </summary>
        public bool IsUnlocked(string id)
        {
            return areaDict.ContainsKey(id) && areaDict[id].isUnlocked;
        }

        /// <summary>
        /// 手动触发区域发现（调试用）
        /// </summary>
        public void DebugDiscover(string id)
        {
            if (areaDict.ContainsKey(id))
            {
                DiscoverArea(areaDict[id]);
            }
        }

        /// <summary>
        /// 重置所有隐藏区域（调试用）
        /// </summary>
        public void ResetAll()
        {
            discoveredAreaIds.Clear();
            unlockedContentIds.Clear();

            foreach (var area in hiddenAreas)
            {
                area.isDiscovered = false;
                area.isUnlocked = false;
                area.discoveryProgress = 0f;
            }

            Debug.Log("[HiddenAreasSystem] 所有隐藏区域已重置");
        }

        /// <summary>
        /// 保存隐藏区域数据
        /// </summary>
        public HiddenAreaSaveData GetSaveData()
        {
            return new HiddenAreaSaveData
            {
                discoveredAreaIds = new List<string>(discoveredAreaIds),
                unlockedContentIds = new List<string>(unlockedContentIds)
            };
        }

        /// <summary>
        /// 加载隐藏区域数据
        /// </summary>
        public void LoadSaveData(HiddenAreaSaveData data)
        {
            if (data == null) return;

            discoveredAreaIds = new List<string>(data.discoveredAreaIds);
            unlockedContentIds = new List<string>(data.unlockedContentIds);

            // 恢复区域状态
            foreach (var area in hiddenAreas)
            {
                area.isDiscovered = discoveredAreaIds.Contains(area.id);
                area.isUnlocked = !string.IsNullOrEmpty(area.hiddenContent?.id) &&
                                   unlockedContentIds.Contains(area.hiddenContent.id);
            }

            Debug.Log($"[HiddenAreasSystem] 已加载 {discoveredAreaIds.Count} 个已发现区域");
        }
    }

    /// <summary>
    /// 隐藏区域统计
    /// </summary>
    [Serializable]
    public class HiddenAreaStats
    {
        public int totalAreas;
        public int discoveredCount;
        public int unlockedCount;
        public float discoveryRate;
    }

    /// <summary>
    /// 隐藏区域存档数据
    /// </summary>
    [Serializable]
    public class HiddenAreaSaveData
    {
        public List<string> discoveredAreaIds;
        public List<string> unlockedContentIds;
    }
}
