using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ForestJournal
{
    // ============================================================
    // Lore Collection System - 森林之书收集系统
    // 100+ 收集品，Lore条目，影响NPC/任务/结局
    // ============================================================

    /// <summary>
    /// Lore条目数据结构
    /// </summary>
    [Serializable]
    public class LoreEntry
    {
        public string id;
        public string name;
        public LoreCategory category;
        public string description;
        public LoreSource source;
        public string hint;
        public bool isUnlocked;
        public bool isNew;

        public LoreEntry(string id, string name, LoreCategory category, string description, LoreSource source, string hint)
        {
            this.id = id;
            this.name = name;
            this.category = category;
            this.description = description;
            this.source = source;
            this.hint = hint;
            this.isUnlocked = false;
            this.isNew = false;
        }
    }

    /// <summary>
    /// Lore收集品分类
    /// </summary>
    public enum LoreCategory
    {
        EnemyCodex,      // 敌人图鉴
        PlantCodex,      // 植物图鉴
        ItemCodex,       // 物品图鉴
        LocationRecord,  // 地点记录
        HistoryShard,    // 历史碎片
        HiddenTruth      // 隐藏真相
    }

    /// <summary>
    /// Lore来源类型
    /// </summary>
    public enum LoreSource
    {
        EnemyDrop,       // 敌人掉落
        Exploration,     // 探索发现
        QuestReward,     // 任务获得
        SpecialCondition,// 特殊条件
        NPCGift,         // NPC赠送
        Crafting,        // 合成获得
        StoryTrigger     // 剧情触发
    }

    /// <summary>
    /// 结局条件关联
    /// </summary>
    [Serializable]
    public class EndingCondition
    {
        public string endingId;
        public int requiredLoreCount;
        public List<string> specificLoreIds = new List<string>();
    }

    /// <summary>
    /// NPC对话解锁关联
    /// </summary>
    [Serializable]
    public class NPCDialogueUnlock
    {
        public string npcId;
        public List<string> requiredLoreIds = new List<string>();
        public string unlockedDialogueKey;
    }

    /// <summary>
    /// 任务触发关联
    /// </summary>
    [Serializable]
    public class QuestTrigger
    {
        public string questId;
        public List<string> requiredLoreIds = new List<string>();
        public List<string> triggeredByCategories = new List<string>();
    }

    /// <summary>
    /// 收集进度统计
    /// </summary>
    [Serializable]
    public class CollectionStats
    {
        public int totalEntries;
        public int unlockedCount;
        public Dictionary<LoreCategory, int> categoryProgress = new Dictionary<LoreCategory, int>();
        public Dictionary<LoreCategory, int> categoryTotal = new Dictionary<LoreCategory, int>();

        public float GetProgressPercentage()
        {
            if (totalEntries == 0) return 0f;
            return (float)unlockedCount / totalEntries * 100f;
        }

        public float GetCategoryProgress(LoreCategory category)
        {
            if (!categoryTotal.ContainsKey(category) || categoryTotal[category] == 0) return 0f;
            if (!categoryProgress.ContainsKey(category)) return 0f;
            return (float)categoryProgress[category] / categoryTotal[category] * 100f;
        }
    }

    /// <summary>
    /// 事件参数
    /// </summary>
    public class LoreUnlockedEventArgs : EventArgs
    {
        public LoreEntry Entry { get; set; }
        public bool IsNewDiscovery { get; set; }
    }

    /// <summary>
    /// Lore收集系统管理器
    /// </summary>
    public class LoreCollectionSystem : MonoBehaviour
    {
        public static LoreCollectionSystem Instance { get; private set; }

        private List<LoreEntry> allLoreEntries = new List<LoreEntry>();
        private Dictionary<string, LoreEntry> loreDict = new Dictionary<string, LoreEntry>();
        private Dictionary<LoreCategory, List<LoreEntry>> categoryIndex = new Dictionary<LoreCategory, List<LoreEntry>>();
        private CollectionStats stats = new CollectionStats();

        private List<NPCDialogueUnlock> npcDialogueUnlocks = new List<NPCDialogueUnlock>();
        private List<QuestTrigger> questTriggers = new List<QuestTrigger>();
        private List<EndingCondition> endingConditions = new List<EndingCondition>();

        public event EventHandler<LoreUnlockedEventArgs> OnLoreUnlocked;
        public event EventHandler<CollectionStats> OnCollectionProgressChanged;

        private HashSet<string> unlockedLoreIds = new HashSet<string>();

        private LoreCategory currentDisplayCategory = LoreCategory.EnemyCodex;
        private List<LoreEntry> currentDisplayList = new List<LoreEntry>();
        private int currentPage = 0;
        private const int ITEMS_PER_PAGE = 10;

        private const string SAVE_KEY = "ForestJournal_LoreCollection";

        // ============================================================
        // Unity生命周期
        // ============================================================

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeLoreDatabase();
            InitializeUnlockRelationships();
            LoadCollection();
        }

        void Start()
        {
            UpdateStats();
        }

        // ============================================================
        // 初始化
        // ============================================================

        private void InitializeLoreDatabase()
        {
            allLoreEntries = new List<LoreEntry>();
            CreateEnemyCodex();
            CreatePlantCodex();
            CreateItemCodex();
            CreateLocationRecords();
            CreateHistoryShards();
            CreateHiddenTruths();

            foreach (var entry in allLoreEntries)
            {
                loreDict[entry.id] = entry;
                if (!categoryIndex.ContainsKey(entry.category))
                    categoryIndex[entry.category] = new List<LoreEntry>();
                categoryIndex[entry.category].Add(entry);
            }

            stats.totalEntries = allLoreEntries.Count;
            foreach (LoreCategory cat in System.Enum.GetValues(typeof(LoreCategory)))
            {
                int catTotal = categoryIndex.ContainsKey(cat) ? categoryIndex[cat].Count : 0;
                stats.categoryTotal[cat] = catTotal;
            }
        }

        private void InitializeUnlockRelationships()
        {
            npcDialogueUnlocks = new List<NPCDialogueUnlock>
            {
                new NPCDialogueUnlock { npcId = "npc_ancient_sage", requiredLoreIds = new List<string> { "hist_001", "hist_002", "hist_003" }, unlockedDialogueKey = "ancient_sage_revelation" },
                new NPCDialogueUnlock { npcId = "npc_forest_guardian", requiredLoreIds = new List<string> { "loc_001", "loc_005", "loc_009" }, unlockedDialogueKey = "guardian_origin" },
                new NPCDialogueUnlock { npcId = "npc_wandering_ghost", requiredLoreIds = new List<string> { "plant_018", "item_013", "hist_015" }, unlockedDialogueKey = "ghost_memory" },
                new NPCDialogueUnlock { npcId = "npc_blacksmith", requiredLoreIds = new List<string> { "item_008", "item_009", "item_010" }, unlockedDialogueKey = "blacksmith_secret" },
                new NPCDialogueUnlock { npcId = "npc_hermit", requiredLoreIds = new List<string> { "plant_019", "plant_020", "hidden_001" }, unlockedDialogueKey = "hermit_truth" },
            };

            questTriggers = new List<QuestTrigger>
            {
                new QuestTrigger { questId = "quest_forest_heart", requiredLoreIds = new List<string> { "loc_005", "loc_010", "loc_015" }, triggeredByCategories = new List<string> { "LocationRecord" } },
                new QuestTrigger { questId = "quest_shadow_origin", requiredLoreIds = new List<string> { "enemy_008", "enemy_009", "enemy_010" }, triggeredByCategories = new List<string> { "EnemyCodex" } },
                new QuestTrigger { questId = "quest_lost_civilization", requiredLoreIds = new List<string> { "item_008", "item_009", "item_010", "hist_001", "hist_002" }, triggeredByCategories = new List<string> { "ItemCodex", "HistoryShard" } },
                new QuestTrigger { questId = "quest_hidden_truth", requiredLoreIds = new List<string> { "hidden_001", "hidden_002", "hidden_003", "hidden_004", "hidden_005" }, triggeredByCategories = new List<string> { "HiddenTruth" } },
                new QuestTrigger { questId = "quest_plant_mystery", requiredLoreIds = new List<string> { "plant_018", "plant_019", "plant_020" }, triggeredByCategories = new List<string> { "PlantCodex" } },
            };

            endingConditions = new List<EndingCondition>
            {
                new EndingCondition { endingId = "ending_nature_harmony", requiredLoreCount = 80, specificLoreIds = new List<string> { "enemy_009", "plant_019", "loc_005", "hist_020" } },
                new EndingCondition { endingId = "ending_truth_revealed", requiredLoreCount = 90, specificLoreIds = new List<string> { "hidden_001", "hidden_005", "hidden_010", "hidden_015", "hidden_020" } },
                new EndingCondition { endingId = "ending_forgotten_king", requiredLoreCount = 95, specificLoreIds = new List<string> { "hist_001", "hist_005", "hist_010", "hist_015", "hist_020", "hist_025" } },
                new EndingCondition { endingId = "ending_escape", requiredLoreCount = 50, specificLoreIds = new List<string>() },
                new EndingCondition { endingId = "ending_trapped", requiredLoreCount = 30, specificLoreIds = new List<string>() },
            };
        }

        // ============================================================
        // A. 敌人图鉴（25个）
        // ============================================================

        private void CreateEnemyCodex()
        {
            AddLore(new LoreEntry("enemy_001", "阴影狼", LoreCategory.EnemyCodex,
                "黑雾边缘的捕食者。毛皮呈现不自然的灰黑色，眼睛在黑暗中泛着幽蓝光芒。它们的行踪与黑雾扩散的轨迹高度重合，仿佛是黑雾的一部分。",
                LoreSource.EnemyDrop, "小心黑雾边缘的区域，它们常在那附近徘徊。"));

            AddLore(new LoreEntry("enemy_002", "毒蜘蛛", LoreCategory.EnemyCodex,
                "沼泽毒素的携带者。体型比普通蜘蛛大两倍，腹部有明显的绿色斑点。毒液呈酸性，能腐蚀衣物和轻甲。",
                LoreSource.EnemyDrop, "沼泽区域的阴暗角落是它们的巢穴。"));

            AddLore(new LoreEntry("enemy_003", "雾精灵", LoreCategory.EnemyCodex,
                "透明身影，来去无踪。传说它们是森林古老意志分裂出的碎片，没有实体，只能以雾气形态存在。攻击时会凝结成尖锐的冰晶。",
                LoreSource.SpecialCondition, "在浓雾弥漫的山谷深处，有更高概率遇见它们。"));

            AddLore(new LoreEntry("enemy_004", "黑雾兽", LoreCategory.EnemyCodex,
                "浓缩的黑雾实体。没有固定形态，时而像狼，时而像熊，偶尔又变成无法描述的扭曲形状。对活物有强烈的攻击欲望。",
                LoreSource.EnemyDrop, "深入黑雾后遭遇的生物，似乎被黑雾完全侵蚀了理智。"));

            AddLore(new LoreEntry("enemy_005", "变异树", LoreCategory.EnemyCodex,
                "被侵蚀的古老橡树。树皮龟裂处渗出黑色汁液，枝条能够自主活动并缠绕靠近的生物。它们的年轮中似乎记录着什么。",
                LoreSource.Exploration, "古老的橡树林中偶尔会发现正在侵蚀中的个体。"));

            AddLore(new LoreEntry("enemy_006", "沼泽蛙", LoreCategory.EnemyCodex,
                "深绿色，鸣叫如雷。体型庞大，最大的能有半人高。皮肤分泌的粘液有微毒，虽然不致命但会导致眩晕和肌肉无力。",
                LoreSource.EnemyDrop, "雨后的沼泽地带最为活跃，鸣叫声可以传遍整个山谷。"));

            AddLore(new LoreEntry("enemy_007", "岩石蜥", LoreCategory.EnemyCodex,
                "外壳坚硬如岩。背部覆盖着类似岩石的鳞片，能够完美融入碎石环境。当受到威胁时，会蜷缩成球体滚动撞击敌人。",
                LoreSource.EnemyDrop, "废墟附近的碎石堆是它们常见的栖息地。"));

            AddLore(new LoreEntry("enemy_008", "雾主", LoreCategory.EnemyCodex,
                "自称黑雾的代言人。罕见的高阶存在，体型巨大，通常以模糊的人形出现。能够操控黑雾的流动，据说能短暂预测未来。",
                LoreSource.SpecialCondition, "在黑雾漩涡中心有较高概率遭遇，需要足够的勇气才能面对。"));

            AddLore(new LoreEntry("enemy_009", "森林守护者", LoreCategory.EnemyCodex,
                "古老意志的化身。形态像是树木与人类的融合体，树根作为双脚，树枝编织成手臂。它们守护森林已经超过千年。",
                LoreSource.SpecialCondition, "不要主动攻击它们，除非你想测试自己的实力。"));

            AddLore(new LoreEntry("enemy_010", "腐化鹿", LoreCategory.EnemyCodex,
                "腐蚀之角，眼中泛红光。曾经是森林中神圣的生物，如今被黑雾侵蚀，变得凶暴而危险。鹿角分泌的腐蚀液能灼伤皮肤。",
                LoreSource.EnemyDrop, "黑雾侵蚀区域的边缘地带偶尔能看到它们徘徊的身影。"));

            AddLore(new LoreEntry("enemy_011", "黑翼蚊", LoreCategory.EnemyCodex,
                "群体出动，防不胜防。单个个体只有拳头大小，但它们以数百只的群体行动。口器尖锐，能轻易刺穿皮革护甲。",
                LoreSource.EnemyDrop, "潮湿闷热的沼泽地带是它们的聚集区，避免在黄昏时分进入。"));

            AddLore(new LoreEntry("enemy_012", "沼泽蛭", LoreCategory.EnemyCodex,
                "吸食血液，传播疾病。体型细长，通常隐藏在沼泽水面之下。当感知到猎物靠近时，会以惊人的速度游动接近。",
                LoreSource.EnemyDrop, "不要随意在沼泽中涉水，即使水看起来很浅。"));

            AddLore(new LoreEntry("enemy_013", "废墟守卫", LoreCategory.EnemyCodex,
                "远古机械，守护废墟。身体由未知的金属合金打造，即使历经岁月侵蚀依然锋利如新。没有明显的弱点，常规攻击几乎无效。",
                LoreSource.Exploration, "在古老废墟的核心区域，它们代替曾经的主人继续履行着职责。"));

            AddLore(new LoreEntry("enemy_014", "幽灵狼", LoreCategory.EnemyCodex,
                "影子分身，难以捕捉。它们能够分裂成多个影子个体，真正的本体隐藏在其中一个分身中。只有在分身全部消灭后才会现形。",
                LoreSource.SpecialCondition, "传说它们是被黑雾吞噬的狼群灵魂，永远困在寻找猎物的循环中。"));

            AddLore(new LoreEntry("enemy_015", "黑雾触手", LoreCategory.EnemyCodex,
                "缠绕拖拽。没有固定形态的主体，似乎是黑雾本身凝聚出的攻击手段。会从地面或裂缝中突然伸出，将猎物拖入黑暗。",
                LoreSource.Exploration, "黑雾漩涡附近最为常见，任何靠近的生物都可能成为目标。"));

            AddLore(new LoreEntry("enemy_016", "剧毒蘑菇", LoreCategory.EnemyCodex,
                "释放孢子毒雾。不要被它们绚丽的外表欺骗，这些蘑菇能够释放致死的孢子云。接触后会出现幻觉，最终在迷失中死去。",
                LoreSource.Exploration, "阴暗潮湿的洞穴深处，注意观察地面是否有异常的彩色真菌。"));

            AddLore(new LoreEntry("enemy_017", "暗影猎手", LoreCategory.EnemyCodex,
                "远程射击，隐匿行踪。全身笼罩在黑雾中的弓箭手，能够在完全隐身的状态下发射致命的暗影箭。极少正面现身。",
                LoreSource.SpecialCondition, "当你发现远处的黑雾突然变浓时，说明它们正在瞄准你。"));

            AddLore(new LoreEntry("enemy_018", "噩梦编织者", LoreCategory.EnemyCodex,
                "织就恐惧的梦境。无形的精神系生物，能够潜入目标的梦境，在梦中制造最可怕的噩梦。现实中被它标记的人会逐渐衰弱。",
                LoreSource.SpecialCondition, "长期睡眠不好的旅人更容易被它盯上。"));

            AddLore(new LoreEntry("enemy_019", "骷髅战士", LoreCategory.EnemyCodex,
                "骨头架子，锈刀在手。不知道已经在这里游荡了多少年，锈蚀的短刀和破碎的盔甲诉说着曾经的悲壮。奇怪的是，它们似乎保留着生前的战斗本能。",
                LoreSource.Exploration, "在骷髅祭坛附近最为常见，也可能出现在古老战壕中。"));

            AddLore(new LoreEntry("enemy_020", "洞穴蝙蝠", LoreCategory.EnemyCodex,
                "超声波定位。即使在完全黑暗的环境中也能精准定位猎物。群体行动时会发出刺耳的尖叫，使猎物短暂失聪。",
                LoreSource.EnemyDrop, "瀑布后洞穴是它们的巢穴，惊扰它们的后果不堪设想。"));

            AddLore(new LoreEntry("enemy_021", "远古幽灵", LoreCategory.EnemyCodex,
                "冤魂不散。废墟中徘徊的古老灵魂，保留着生前的记忆碎片。它们有时会莫名其妙地攻击路过的旅人，有时又显得十分悲伤。",
                LoreSource.Exploration, "在石碑林附近深夜有几率遇到，据说它们在寻找着什么。"));

            AddLore(new LoreEntry("enemy_022", "森林甲虫", LoreCategory.EnemyCodex,
                "甲壳坚硬。体型约有人手大小，背甲坚硬如铁，能够抵御大部分物理攻击。性格温顺，但受到惊吓时会分泌刺激性液体。",
                LoreSource.Exploration, "古树桩附近较为常见，它们以腐木为食。"));

            AddLore(new LoreEntry("enemy_023", "沼泽鼠", LoreCategory.EnemyCodex,
                "群居，啃食一切。体型比普通老鼠大数倍，牙齿锋利得能咬断金属。它们以群体的形式行动，能在短时间内将大型生物啃成白骨。",
                LoreSource.EnemyDrop, "沉没村庄遗址是它们的聚集地，数量惊人。"));

            AddLore(new LoreEntry("enemy_024", "沼泽蛙王", LoreCategory.EnemyCodex,
                "蛙群之王。沼泽蛙的首领，体型是普通沼泽蛙的五倍。拥有指挥蛙群的能力，单独面对它时，往往意味着已经陷入了整个蛙群的包围。",
                LoreSource.SpecialCondition, "击败它后，周围的蛙群会陷入混乱，这是逃离的最佳时机。"));

            AddLore(new LoreEntry("enemy_025", "岩石蜥王", LoreCategory.EnemyCodex,
                "甲壳如铁。岩石蜥群的统治者，背甲呈现独特的金色纹路。它领导着整个岩石蜥群，攻击时会召唤附近的岩石蜥前来支援。",
                LoreSource.SpecialCondition, "它的背甲上有古代铭文，似乎记录着什么重要的信息。"));
        }

        // ============================================================
        // B. 植物图鉴（25个）
        // ============================================================

        private void CreatePlantCodex()
        {
            AddLore(new LoreEntry("plant_001", "红浆果", LoreCategory.PlantCodex,
                "食用恢复少量饥饿。鲜艳的红色小果实，生长在灌木丛中。味道酸甜，适量食用对健康有益，但大量食用可能导致腹泻。",
                LoreSource.Exploration, "森林边缘的灌木丛中常见，采摘时请注意辨别品种。"));

            AddLore(new LoreEntry("plant_002", "蓝蘑菇", LoreCategory.PlantCodex,
                "有毒，轻视者必遭不幸。美丽的蓝色真菌，夜间会发出微弱的荧光。误食会导致剧烈腹痛和幻觉，严重者可能在数小时内死亡。",
                LoreSource.Exploration, "阴湿的树根处偶尔可见，永远不要被它的外表欺骗。"));

            AddLore(new LoreEntry("plant_003", "银叶草", LoreCategory.PlantCodex,
                "银色叶片，可入药。叶片表面覆盖着细小的银色绒毛，在阳光下闪闪发光。是制作解毒药剂的重要原料。",
                LoreSource.Exploration, "常青针树下偶尔可见，采摘后应尽快妥善保存。"));

            AddLore(new LoreEntry("plant_004", "夜光花", LoreCategory.PlantCodex,
                "夜晚发光，吸引飞虫。花瓣呈淡紫色，夜间会发出柔和的蓝绿色光芒。虽然美丽，但它的花粉对某些人来说是强效的过敏原。",
                LoreSource.Exploration, "只在夜晚开放，镜面池塘附近偶尔能发现它们的踪迹。"));

            AddLore(new LoreEntry("plant_005", "荆棘藤", LoreCategory.PlantCodex,
                "锋利但可编织。布满尖刺的藤蔓植物，但也因此具有出色的韧性。晒干后可以用来编织绳索或制作简易陷阱。",
                LoreSource.Exploration, "黑雾前沿的灌木丛中最为常见，采集时务必穿戴防护手套。"));

            AddLore(new LoreEntry("plant_006", "古树桩", LoreCategory.PlantCodex,
                "年轮记录着岁月。被砍伐或倒下的古树留下的树桩，年轮的数量记录着它生长的年头。有些古树桩的年轮甚至超过千年。",
                LoreSource.Exploration, "仔细观察年轮，有时能发现刻在上面的古老符号。"));

            AddLore(new LoreEntry("plant_007", "苔藓毯", LoreCategory.PlantCodex,
                "柔软，铺地最佳。厚实的苔藓覆盖层，触感柔软如地毯。干燥的苔藓可以当作引火材料，潮湿的则能保持物品的新鲜度。",
                LoreSource.Exploration, "宁静花园的地面几乎完全被它们覆盖，踩上去发出轻柔的声音。"));

            AddLore(new LoreEntry("plant_008", "毒藤", LoreCategory.PlantCodex,
                "接触即中毒。看起来像是普通的绿色藤蔓，但皮肤接触后会产生剧烈的灼烧感和水泡。必须用银叶草制成的药剂才能缓解。",
                LoreSource.Exploration, "隐藏在普通藤蔓之间，唯一的区分方法是它略微偏深的绿色。"));

            AddLore(new LoreEntry("plant_009", "香草束", LoreCategory.PlantCodex,
                "烹饪增香。多种香草的混合，添加到烹饪中可以大幅提升食物的口感和香气。不同的配比能带来截然不同的风味。",
                LoreSource.Exploration, "秘密营地附近的人工花园中有人曾经种植过，现在已经野化。"));

            AddLore(new LoreEntry("plant_010", "枯木", LoreCategory.PlantCodex,
                "可做柴火。被风干的老旧木材，水分含量低，是理想的燃料来源。某些枯木内部已经被菌类侵蚀，点燃时会发出独特的气味。",
                LoreSource.Exploration, "倒长树木区域附近的枯木最为干燥，是最好的柴火选择。"));

            AddLore(new LoreEntry("plant_011", "树皮衣", LoreCategory.PlantCodex,
                "简陋但能保暖。用树皮纤维编织而成的衣物，虽然外观粗糙但意外地具有良好的保温效果，而且防水性能出色。",
                LoreSource.Crafting, "制作需要大量树皮，可在古树桩附近找到材料。"));

            AddLore(new LoreEntry("plant_012", "根部块茎", LoreCategory.PlantCodex,
                "充饥来源。某些植物膨胀的根部，储存着大量淀粉。煮熟后可以作为主食，味道类似土豆，但略带回苦。",
                LoreSource.Exploration, "巨型树根网络附近的土壤中较为常见，挖掘时需要小心不要破坏根部。"));

            AddLore(new LoreEntry("plant_013", "露珠草", LoreCategory.PlantCodex,
                "叶尖常有露珠。叶片形状独特的草本植物，叶尖始终保持着一滴晶莹的水珠。传说是采集月光精华的植物。",
                LoreSource.Exploration, "清晨时分在蘑菇圆环附近可以看到它们叶尖闪烁的露珠。"));

            AddLore(new LoreEntry("plant_014", "萤火苔", LoreCategory.PlantCodex,
                "夜晚发出微光。苔藓的一种，夜晚会发出微弱的黄绿色光芒。大量生长时足以照亮小型空间，是天然的照明替代品。",
                LoreSource.Exploration, "瀑布后洞穴的岩壁上生长着大片萤火苔，是那里的主要光源。"));

            AddLore(new LoreEntry("plant_015", "腐肉花", LoreCategory.PlantCodex,
                "散发恶臭。外观类似百合但颜色更深，会散发出类似腐烂肉类的强烈气味，吸引苍蝇等腐食性昆虫传粉。",
                LoreSource.Exploration, "沉没村庄遗址的水边偶尔能看到，令人不适的气味是其标志。"));

            AddLore(new LoreEntry("plant_016", "蝴蝶兰", LoreCategory.PlantCodex,
                "美丽但无实际用途。优雅的兰科植物，花朵形似翩翩起舞的蝴蝶。只存在于宁静花园中，不知道是谁曾经种下的。",
                LoreSource.Exploration, "宁静花园是它们唯一的栖息地，生长在花园的保护性围栏内。"));

            AddLore(new LoreEntry("plant_017", "常青针", LoreCategory.PlantCodex,
                "松针，可泡茶。常绿针叶树的细长叶片，收集后可以泡制具有淡淡松木清香的茶饮。有轻微的安神效果。",
                LoreSource.Exploration, "森林中部的松林是采集地点，新鲜的针叶效果最佳。"));

            AddLore(new LoreEntry("plant_018", "黑雾苔", LoreCategory.PlantCodex,
                "只在黑雾边缘生长。只能在黑雾与正常空气交界处发现的特殊苔藓，呈深紫色。有传言说它能吸收黑雾中的某种能量。",
                LoreSource.Exploration, "黑雾前沿附近才能找到，采集时需要在黑雾边缘安全区域内进行。"));

            AddLore(new LoreEntry("plant_019", "灵魂花", LoreCategory.PlantCodex,
                "传说能连接亡魂。极为稀有的花卉，据说只在特殊时刻绽放。花瓣呈现半透明的乳白色，仿佛由月光凝结而成。触碰它时会感到一阵寒意。",
                LoreSource.SpecialCondition, "传说在流星陨坑附近，如果足够幸运和虔诚，灵魂花会在月圆之夜绽放。"));

            AddLore(new LoreEntry("plant_020", "星光蘑菇", LoreCategory.PlantCodex,
                "传说能看到未来。极为罕见的发光蘑菇，菌盖上布满了类似星光的斑点。传统说法是食用它可以看到未来的片段，但真实性无从考证。",
                LoreSource.SpecialCondition, "据说只在石碑林深处的特定位置生长，数量极其稀少。"));

            AddLore(new LoreEntry("plant_021", "缠绕草", LoreCategory.PlantCodex,
                "细长的草本植物，茎部能够缠绕在周围的物体上。看起来无害，但如果被它缠住脚踝，会越挣扎越紧。",
                LoreSource.Exploration, "蘑菇圆环周围的草地中较为常见，行走时需注意脚下。"));

            AddLore(new LoreEntry("plant_022", "幻影藤", LoreCategory.PlantCodex,
                "能够部分隐身的藤蔓植物，与雾精灵一样能够利用雾气遮掩自己的形态。只有在风吹动时才能隐约看到它的轮廓。",
                LoreSource.SpecialCondition, "幽暗山谷深处的雾气中偶尔能看到它们摇曳的身影。"));

            AddLore(new LoreEntry("plant_023", "记忆苔藓", LoreCategory.PlantCodex,
                "生长在记忆之树周围的特殊苔藓，据说能够保存记忆的碎片。将手掌贴在上面，有时能感受到不属于你自己的情感或画面。",
                LoreSource.Exploration, "记忆之树的枝条下是它们的聚集地，在那里可以尝试与之共鸣。"));

            AddLore(new LoreEntry("plant_024", "石化花", LoreCategory.PlantCodex,
                "触碰到它的生物会被短暂石化。花朵本身呈灰白色，看起来像是用石头雕刻而成。石化效果短暂，通常只持续几秒。",
                LoreSource.Exploration, "石碑林中偶有发现，它们的存在让这片区域更加神秘莫测。"));

            AddLore(new LoreEntry("plant_025", "逆生草", LoreCategory.PlantCodex,
                "生长方向与重力相反的奇异植物，叶片向下生长，根系却朝向天空。是重力异常区域特有的物种。",
                LoreSource.Exploration, "倒长树木区域是它们唯一的栖息地，也是这片异常区域最有力的证据。"));
        }

        // ============================================================
        // C. 物品图鉴（25个）
        // ============================================================

        private void CreateItemCodex()
        {
            AddLore(new LoreEntry("item_001", "骨制工具", LoreCategory.ItemCodex,
                "简单但有效。用动物骨骼打磨而成的工具，包括骨针、骨刀和骨钩。虽然材料原始，但工艺出乎意料地精良，使用寿命很长。",
                LoreSource.EnemyDrop, "击败骷髅战士后有时会掉落，骨头经过特殊处理不易折断。"));

            AddLore(new LoreEntry("item_002", "草药绷带", LoreCategory.ItemCodex,
                "止血消毒。用银叶草和苔藓混合制成的传统绷带，对外伤有显著的止血和消毒效果。野外的必备物品。",
                LoreSource.Crafting, "制作配方可以在古树桩附近找到的笔记中学到。"));

            AddLore(new LoreEntry("item_003", "黑雾碎片", LoreCategory.ItemCodex,
                "黑雾凝结而成。当黑雾浓度极高时，有时会凝结成固态的碎片。触感冰凉，握持时间过长会让人感到精神恍惚。",
                LoreSource.EnemyDrop, "击败黑雾兽后有几率获得，也是某些仪式的必需材料。"));

            AddLore(new LoreEntry("item_004", "古老硬币", LoreCategory.ItemCodex,
                "废墟中发现，不知年代。材质是不熟悉的合金，正面刻着无法辨认的文字，背面是一个模糊的图案。似乎与古代文明有关。",
                LoreSource.Exploration, "在古老废墟的各处都有发现，收集一定数量后也许能拼凑出什么。"));

            AddLore(new LoreEntry("item_005", "破碎护符", LoreCategory.ItemCodex,
                "仍有残余力量。护符的一部分，原本应该具有某种保护力量。虽然已经破碎，但残存的力量仍然在微弱地闪烁。",
                LoreSource.QuestReward, "完成逝者的祈祷任务后可获得，也许可以修复。"));

            AddLore(new LoreEntry("item_006", "干枯花束", LoreCategory.ItemCodex,
                "某人留下的纪念。已经干枯但仍然保持着完整形态的花束，被仔细地系着缎带。不知道是送给谁的，也没有人知道它在这里躺了多久。",
                LoreSource.Exploration, "秘密营地的主帐篷中放着它，也许是曾经住在这里的人留下的。"));

            AddLore(new LoreEntry("item_007", "锈铁剑", LoreCategory.ItemCodex,
                "曾经属于某位战士。锈迹斑斑的短剑，剑柄上刻着一个小小的名字。它在这里等待主人已经很久了。",
                LoreSource.Exploration, "在古老战壕的某个角落发现，也许那位战士就在不远处。"));

            AddLore(new LoreEntry("item_008", "玻璃碎片", LoreCategory.ItemCodex,
                "折射光芒。透明但略带色彩的玻璃碎片，表面光滑得不像出自天然。阳光下能够折射出彩虹般的光芒，与周围的废墟格格不入。",
                LoreSource.Exploration, "在古老废墟的底层区域发现，也许曾经是某种容器的一部分。"));

            AddLore(new LoreEntry("item_009", "布料残片", LoreCategory.ItemCodex,
                "染色布，可追溯。带有褪色图案的布料残片，染色使用的是一种特殊的植物染料。图案的一部分似乎能辨认出某个符号。",
                LoreSource.Exploration, "沉没村庄遗址的水下部分保存着一些，推测属于某个组织。"));

            AddLore(new LoreEntry("item_010", "陶罐碎片", LoreCategory.ItemCodex,
                "远古文明的痕迹。破碎的陶罐碎片，上面有精美的几何纹饰。陶土的成分与当地不同，说明它们来自远方。",
                LoreSource.Exploration, "骷髅祭坛附近散落着许多，它们的风格与废墟中的发现高度一致。"));

            AddLore(new LoreEntry("item_011", "羽毛笔", LoreCategory.ItemCodex,
                "记录者的工具。制作精良的羽毛笔，笔尖部分似乎是用某种大型鸟类羽毛制成的。蘸取墨水后可以书写出细腻的文字。",
                LoreSource.QuestReward, "完成记录者之路任务后获得，记录森林中的一切。"));

            AddLore(new LoreEntry("item_012", "封印纸条", LoreCategory.ItemCodex,
                "写着看不懂的文字。泛黄的纸条被某种蜡封住，蜡封上压着奇怪的印记。纸条上的文字无人能懂，但似乎是一种警告。",
                LoreSource.Exploration, "在悬空平台的隐蔽角落发现，触碰时能感受到微弱的震动。"));

            AddLore(new LoreEntry("item_013", "手账残页", LoreCategory.ItemCodex,
                "残缺的故事。你一直在使用的手账的其中一页，似乎是被人故意撕下来的。上面记录着令人不安的内容。",
                LoreSource.StoryTrigger, "随着游戏的进行，这页纸会自动出现在你的物品栏中。"));

            AddLore(new LoreEntry("item_014", "魂精华", LoreCategory.ItemCodex,
                "凝聚的森林意志。晶莹剔透的精华，蕴含着强大的生命力。是森林最核心的能量形态，据说能治愈一切伤痛。",
                LoreSource.SpecialCondition, "击败森林守护者后有极低几率获得，或者在森林之心虔诚祈祷。"));

            AddLore(new LoreEntry("item_015", "古物碎片", LoreCategory.ItemCodex,
                "集齐可还原真相。一系列看起来互不相关的碎片，但当它们拼合在一起时，揭示的是关于这片森林过去的惊人真相。",
                LoreSource.Exploration, "分散在五个不同地点，需要全部收集才能理解它们的含义。"));

            AddLore(new LoreEntry("item_016", "望远镜碎片", LoreCategory.ItemCodex,
                "能看到远方。两块可以拼合的镜片，安装在铜制框架上。透过它观察时，可以看到平时看不到的远处细节。",
                LoreSource.QuestReward, "在巨型树根网络深处找到散落的碎片，完成拼合后成为重要道具。"));

            AddLore(new LoreEntry("item_017", "旧照片", LoreCategory.ItemCodex,
                "已经模糊不清。褪色的老照片，画面大部分已经模糊不清。隐约能看到几个人影站在某个建筑前，背景似乎是现在的古老废墟。",
                LoreSource.Exploration, "在宁静花园的旧物箱中发现，也许是最后一批离开这里的人留下的。"));

            AddLore(new LoreEntry("item_018", "信件碎片", LoreCategory.ItemCodex,
                "寄信人不明。残破的信件，大部分文字已经无法辨认。只有最后一行还勉强能看清：不要再回来了。",
                LoreSource.Exploration, "瀑布后洞穴的岩壁夹缝中找到，信纸已经脆弱得一碰就碎。"));

            AddLore(new LoreEntry("item_019", "徽章", LoreCategory.ItemCodex,
                "某个组织的标志。金属徽章，上面刻着一个类似于眼睛的图案。佩戴它似乎能获得某些人的认可，但也可能招来敌意。",
                LoreSource.Exploration, "在石碑林深处发现，似乎是某个已经消失的组织遗留下来的身份标识。"));

            AddLore(new LoreEntry("item_020", "神秘钥匙", LoreCategory.ItemCodex,
                "开什么锁？造型古朴的铜钥匙，表面刻有与废墟中发现的符号相似的花纹。不知道它能打开什么，也许是某个被遗忘的