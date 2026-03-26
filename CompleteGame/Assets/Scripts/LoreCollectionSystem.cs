using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
                "开什么锁？造型古朴的铜钥匙，表面刻有与废墟中发现的符号相似的花纹。不知道它能打开什么，也许是某个被遗忘的宝库。",
                LoreSource.SpecialCondition, "在森林之心深处的祭坛上发现，传说只有真正的守护者才能使用它。"));

            AddLore(new LoreEntry("item_021", "黑雾结晶体", LoreCategory.ItemCodex,
                "纯净的黑雾精华。比普通黑雾碎片更加纯净，呈现出深邃的紫黑色光泽。据说蕴含着强大的精神力量。",
                LoreSource.EnemyDrop, "击败雾主后必定掉落，是最为珍贵的材料之一。"));

            AddLore(new LoreEntry("item_022", "鹿角碎片", LoreCategory.ItemCodex,
                "腐蚀之角的残留物。已经从原来的神圣物品变成了被污染的碎片，其中似乎封印着某种痛苦的情绪。",
                LoreSource.EnemyDrop, "击败腐化鹿后有几率获得，散发着令人生畏的腐蚀气息。"));

            AddLore(new LoreEntry("item_023", "机械齿轮", LoreCategory.ItemCodex,
                "远古机械的零件。精密的金属齿轮，即使历经岁月依然转动灵活。不知道这套机械原本是用作什么。",
                LoreSource.Exploration, "在废墟守卫身上有少量掉落，也可以在古老废墟的废墟中找到散落的零件。"));

            AddLore(new LoreEntry("item_024", "蝙蝠翅膀", LoreCategory.ItemCodex,
                "洞穴蝙蝠的膜翼。薄而坚韧的翼膜，在月光下呈现出独特的纹理。可以用来制作某些特殊的道具。",
                LoreSource.EnemyDrop, "击败洞穴蝙蝠后大量获得，是制作飞行的关键材料。"));

            AddLore(new LoreEntry("item_025", "幽灵精华", LoreCategory.ItemCodex,
                "幽灵狼的生命精华。在阳光下呈现半透明的状态，握持时能感受到它的冰冷。似乎可以让生物短暂进入幽灵状态。",
                LoreSource.SpecialCondition, "击败幽灵狼王后获得，是极为稀有的材料。"));
        }

        // ============================================================
        // D. 地点记录（25个）
        // ============================================================

        private void CreateLocationRecords()
        {
            AddLore(new LoreEntry("loc_001", "迷雾森林入口", LoreCategory.LocationRecord,
                "你们的起点。终年被薄雾笼罩的森林入口，阳光只能以微弱的光线形式穿透进来。这里是进入森林的第一道关卡，也是大多数旅人的噩梦开始的地方。",
                LoreSource.StoryTrigger, "这是游戏的起点区域，所有故事都从这里开始。"));

            AddLore(new LoreEntry("loc_002", "黑雾前沿", LoreCategory.LocationRecord,
                "深入禁区的第一道线。黑雾在这里与正常空气交界，形成一道肉眼可见的分界线。越过这条线，就正式踏入了被黑雾侵蚀的区域。",
                LoreSource.Exploration, "这里是第一个真正的危险区域，建议做好准备后再进入。"));

            AddLore(new LoreEntry("loc_003", "古老废墟", LoreCategory.LocationRecord,
                "高度文明的遗迹。巨大的石制建筑群，虽然已经残破不堪，但依然能看出曾经的辉煌。墙壁上的雕刻描述着某种现在已经失传的文明。",
                LoreSource.Exploration, "废墟的核心区域有废墟守卫守护着传说中的宝藏。"));

            AddLore(new LoreEntry("loc_004", "幽暗山谷", LoreCategory.LocationRecord,
                "雾气最浓之处。山谷中央的雾气浓度已经达到了伸手不见五指的程度。即使是黑雾生物也在这里放慢了脚步，似乎在警惕着什么。",
                LoreSource.SpecialCondition, "在这里需要依赖听觉和其他感官来探索。"));

            AddLore(new LoreEntry("loc_005", "森林之心", LoreCategory.LocationRecord,
                "一切的终点。森林最核心的区域，一棵巨大无比的古树矗立在中央。传说这棵树是整个森林的起源，也是所有黑雾的源头。",
                LoreSource.StoryTrigger, "这里是游戏主线剧情的终点，也是多个结局的关键地点。"));

            AddLore(new LoreEntry("loc_006", "秘密营地", LoreCategory.LocationRecord,
                "有人在这里住过。一个被巧妙隐藏的营地，有生活过的痕迹。有人曾经在这里居住了很长时间，但不知道因为什么离开了。",
                LoreSource.Exploration, "仔细搜索营地可以发现许多有用的物品和线索。"));

            AddLore(new LoreEntry("loc_007", "瀑布后洞穴", LoreCategory.LocationRecord,
                "隐藏的空间。被瀑布遮掩的天然洞穴，内部空间比外表看起来大得多。萤火苔照亮了整个洞穴，散发着幽幽的绿光。",
                LoreSource.Exploration, "洞穴内部藏有珍贵的矿物资源和古老的遗迹。"));

            AddLore(new LoreEntry("loc_008", "巨型树根网络", LoreCategory.LocationRecord,
                "地下通道。巨树的根系在地下蔓延，形成了错综复杂的通道。有些通道通往地面，有些则深入更深的地下。",
                LoreSource.Exploration, "地下通道中栖息着许多适应黑暗环境的生物。"));

            AddLore(new LoreEntry("loc_009", "骷髅祭坛", LoreCategory.LocationRecord,
                "祭祀所用。用巨大骨骼堆砌而成的祭坛，散发着诡异的气息。祭坛周围散落着远古的陶罐碎片，似乎在进行某种古老的仪式。",
                LoreSource.Exploration, "在月圆之夜来到祭坛，可能会看到异样的景象。"));

            AddLore(new LoreEntry("loc_010", "沉没村庄", LoreCategory.LocationRecord,
                "水下的痕迹。整个村庄被淹没在水下，只有屋顶和部分墙壁露出水面。村庄的布局显示这里曾经住着相当规模的居民。",
                LoreSource.Exploration, "水下搜索可以找到许多那个年代的生活用品。"));

            AddLore(new LoreEntry("loc_011", "悬空平台", LoreCategory.LocationRecord,
                "不知如何建造。巨大的石制平台悬浮在半空中，周围没有任何支撑。站在平台边缘俯瞰下方，让人感到头晕目眩。",
                LoreSource.SpecialCondition, "平台上可能藏有古代文明的遗物。"));

            AddLore(new LoreEntry("loc_012", "镜面池塘", LoreCategory.LocationRecord,
                "映照的不是你。水面平静如镜，但映照出的不是周围的景色。有时候，镜面中会出现不存在的影像，与现实交叠。",
                LoreSource.SpecialCondition, "在特定的月光角度下，可以看到过去的影像。"));

            AddLore(new LoreEntry("loc_013", "倒长树木", LoreCategory.LocationRecord,
                "重力异常区。这里的树木都倒着生长，根系朝向天空，叶子垂向地面。是森林中最为诡异的区域之一。",
                LoreSource.Exploration, "进入此区域会暂时受到重力影响，需要小心行动。"));

            AddLore(new LoreEntry("loc_014", "蘑菇圆环", LoreCategory.LocationRecord,
                "精灵的痕迹。巨大的蘑菇排列成完美的圆环，这种自然现象极为罕见。传说这里是森林精灵曾经举行仪式的地方。",
                LoreSource.Exploration, "在蘑菇圆环中央有时能发现特殊的植物。"));

            AddLore(new LoreEntry("loc_015", "流星陨坑", LoreCategory.LocationRecord,
                "那天晚上发生了什么。一个巨大的陨石坑，中心散落着奇异的矿石。那天晚上到底发生了什么，没有人知道确切答案。",
                LoreSource.SpecialCondition, "传说灵魂花会在这里的特定时刻绽放。"));

            AddLore(new LoreEntry("loc_016", "古老战壕", LoreCategory.LocationRecord,
                "谁在打仗。纵横交错的战壕系统，显示这里曾经发生过激烈的战争。锈蚀的武器和破碎的盾牌散落一地。",
                LoreSource.Exploration, "战壕深处可能有幸存者留下的遗物。"));

            AddLore(new LoreEntry("loc_017", "石碑林", LoreCategory.LocationRecord,
                "纪念碑还是墓碑。大量的石制碑文排列在空地上，每一个都刻着难以辨认的文字。没有人知道这些石碑是用于纪念还是安葬。",
                LoreSource.Exploration, "石碑林的深处据说有星光蘑菇生长。"));

            AddLore(new LoreEntry("loc_018", "黑雾漩涡", LoreCategory.LocationRecord,
                "一切开始的地方。黑雾最为浓郁的区域，形成了一个巨大的漩涡状云团。传说这里是黑雾的源头，也是一切灾祸的起点。",
                LoreSource.SpecialCondition, "这里是游戏中最危险的区域，雾主就居住在此。"));

            AddLore(new LoreEntry("loc_019", "宁静花园", LoreCategory.LocationRecord,
                "废墟中的净土。一个被遗忘的小花园，依然保持着惊人的美丽。蝴蝶兰在微风中轻轻摇曳，与周围的废墟形成鲜明对比。",
                LoreSource.Exploration, "花园中可能藏有过去的记忆和线索。"));

            AddLore(new LoreEntry("loc_020", "记忆之树", LoreCategory.LocationRecord,
                "枝条挂满了记忆。树枝上悬挂着无数发光的碎片，这些碎片似乎保存着人们的记忆。触摸它们有时能看到不属于你的画面。",
                LoreSource.SpecialCondition, "与记忆之树共鸣可能触发特殊事件。"));

            AddLore(new LoreEntry("loc_021", "回声洞穴", LoreCategory.LocationRecord,
                "声音会被放大。在这个洞穴中说话，声音会被反复放大和扭曲。有时候，甚至能听到不属于现在的声音。",
                LoreSource.Exploration, "洞穴深处的回声最为强烈，可能是某种交流方式。"));

            AddLore(new LoreEntry("loc_022", "迷雾沼泽", LoreCategory.LocationRecord,
                "没有尽头的泥沼。浓雾笼罩的沼泽地，地形随时可能变化。看似坚固的地面可能只是薄薄的一层浮萍。",
                LoreSource.Exploration, "沼泽中生活着各种毒虫和危险生物。"));

            AddLore(new LoreEntry("loc_023", "断崖平台", LoreCategory.LocationRecord,
                "通往深渊的跳板。巨大的断崖，下方是深不见底的峡谷。唯一的通路是几块看似不稳定的石柱。",
                LoreSource.Exploration, "跳过石柱需要精准的时机判断。"));

            AddLore(new LoreEntry("loc_024", "黑水湖畔", LoreCategory.LocationRecord,
                "黑雾的尽头。水面呈现诡异的黑色，倒映出的不是天空而是黑雾。湖水冰凉刺骨，据说有生物生活在湖底。",
                LoreSource.SpecialCondition, "在湖边扎营可能获得特殊的梦境。"));

            AddLore(new LoreEntry("loc_025", "时间错乱区", LoreCategory.LocationRecord,
                "过去与现在的交界。一个神秘的空间，物理法则在这里似乎不再适用。走在其中，可能会突然出现在另一个时间点。",
                LoreSource.SpecialCondition, "这里可能存在通往其他区域的捷径。"));
        }

        // ============================================================
        // E. 历史碎片（25个）
        // ============================================================

        private void CreateHistoryShards()
        {
            AddLore(new LoreEntry("hist_001", "文明兴衰记", LoreCategory.HistoryShard,
                "根据废墟中的铭文和文物推断，这片森林曾经是高度文明的发源地。那个文明拥有超越现代的科技，能够操控自然力量。然而，过度的开发触怒了某种存在，黑雾由此诞生。",
                LoreSource.Exploration, "收集更多废墟中的文物可以还原这段历史的更多细节。"));

            AddLore(new LoreEntry("hist_002", "守护者传说", LoreCategory.HistoryShard,
                "古老的传说讲述了一群被称为守护者的人。他们被选中保护森林免受黑雾侵害，代价是永远不能离开森林。森林守护者可能就是他们的化身。",
                LoreSource.NPCGift, "向隐居者询问关于守护者的传说。"));

            AddLore(new LoreEntry("hist_003", "黑雾起源", LoreCategory.HistoryShard,
                "历史研究显示，黑雾并非天然形成，而是某种力量刻意制造的。最初的目的是什么？是为了保护还是毁灭？答案仍然埋藏在迷雾之中。",
                LoreSource.QuestReward, "解开三个历史谜题后获得这本书。"));

            AddLore(new LoreEntry("hist_004", "失落部族", LoreCategory.HistoryShard,
                "一个曾经居住在此的部族留下的记录。他们是最早的森林居民，后来神秘消失。有人认为他们进化成了其他生物，有人认为他们被黑雾吞噬。",
                LoreSource.Exploration, "在石碑林中寻找部族的遗迹。"));

            AddLore(new LoreEntry("hist_005", "王朝更迭", LoreCategory.HistoryShard,
                "废墟中的王座记录了一个曾经统治这片土地的王朝。他们的旗帜上绘有一只眼睛，与废墟中发现的徽章上的图案一致。王朝覆灭的原因不明。",
                LoreSource.Exploration, "在古老废墟的核心区域找到王座。"));

            AddLore(new LoreEntry("hist_006", "炼金术士的遗产", LoreCategory.HistoryShard,
                "一位古代炼金术士的研究笔记。笔记中提到了一种可以净化黑雾的方法，但需要一种极其罕见的材料。笔记在关键处被撕掉了。",
                LoreSource.Exploration, "在秘密营地找到炼金术士的遗留物。"));

            AddLore(new LoreEntry("hist_007", "战役记录", LoreCategory.HistoryShard,
                "描述了一场发生在古老战壕中的激烈战役。参战双方不明，使用的武器介于冷兵器和某种未知科技之间。战役的结果是两败俱伤。",
                LoreSource.Exploration, "在古老战壕中收集战争遗留物。"));

            AddLore(new LoreEntry("hist_008", "异星来客", LoreCategory.HistoryShard,
                "一本破损的日记提到，某天夜晚天降流星，随后黑雾开始扩散。研究者认为这暗示了黑雾可能来自外星，或者与某种天体现象有关。",
                LoreSource.QuestReward, "在流星陨坑完成调查后获得。"));

            AddLore(new LoreEntry("hist_009", "封印之书", LoreCategory.HistoryShard,
                "一本被封印的古书，上面记录着某种强大力量的仪式。封印已经被打破，但仪式的内容已经散佚。是否应该庆幸，还是应该担忧？",
                LoreSource.SpecialCondition, "在瀑布后洞穴的密室中发现。"));

            AddLore(new LoreEntry("hist_010", "最后的前哨", LoreCategory.HistoryShard,
                "描述了一个用于监视黑雾扩散的前哨站。前哨站的最后记录显示，黑雾的扩散速度在某个时间点突然加快，但原因不明。",
                LoreSource.Exploration, "在黑雾前沿附近找到前哨站遗迹。"));

            AddLore(new LoreEntry("hist_011", "原住民的警告", LoreCategory.HistoryShard,
                "刻在岩石上的原住民警告：不要在月圆之夜进入黑雾。那天晚上，黑雾会变得更加活跃，而且会出现一些平时看不到的东西。",
                LoreSource.Exploration, "在幽暗山谷的岩壁上发现。"));

            AddLore(new LoreEntry("hist_012", "实验记录", LoreCategory.HistoryShard,
                "古代设施中的实验记录。显示曾经有人试图复制黑雾的力量进行实验。实验最终失控，导致了整个设施的毁灭。",
                LoreSource.Exploration, "在废墟核心区域找到被污染的实验室。"));

            AddLore(new LoreEntry("hist_013", "流亡者日记", LoreCategory.HistoryShard,
                "一个流亡者写下的日记。记录了他们被迫离开家园的过程，以及在森林中求生的经历。日记的最后几页充满了绝望和恐惧。",
                LoreSource.Exploration, "在沉没村庄的水下房屋中找到。"));

            AddLore(new LoreEntry("hist_014", "预言石板", LoreCategory.HistoryShard,
                "刻有模糊预言的石板。预言提到了一个人将会在某天到来，那个人能够决定森林的命运。预言的其余部分已经无法辨认。",
                LoreSource.SpecialCondition, "在石碑林深处找到预言石板。"));

            AddLore(new LoreEntry("hist_015", "灵魂研究者", LoreCategory.HistoryShard,
                "一位学者的研究笔记，记录了他对森林灵魂的研究。他声称发现灵魂可以以物质形式存在，但他的研究突然中断了。",
                LoreSource.NPCGift, "从游荡的幽灵那里获得。"));

            AddLore(new LoreEntry("hist_016", "失落语言", LoreCategory.HistoryShard,
                "对废墟中发现的神秘文字的研究报告。文字似乎是一种古老的语言，与已知的任何语系都没有关联。破译工作仍在进行中。",
                LoreSource.QuestReward, "收集所有古老硬币后获得解读能力。"));

            AddLore(new LoreEntry("hist_017", "森林之心", LoreCategory.HistoryShard,
                "关于森林之心的古老文献。记载着这棵巨树的起源：它并非天然生长，而是由第一个守护者用自己的生命精华栽种的。",
                LoreSource.StoryTrigger, "在森林之心阅读古老的铭文。"));

            AddLore(new LoreEntry("hist_018", "迁移传说", LoreCategory.HistoryShard,
                "关于一个民族被迫迁移的传说。他们原本居住在这片土地，但灾难降临时不得不离开。传说中提到他们带走了一样重要的东西。",
                LoreSource.Exploration, "在镜面池塘的幻象中看到这段历史。"));

            AddLore(new LoreEntry("hist_019", "黑暗时代", LoreCategory.HistoryShard,
                "历史学家对某个黑暗时期的记录。在那段时期，黑雾几乎吞噬了整个森林，只剩下少数几个避难所。没有人知道黑暗时代是如何结束的。",
                LoreSource.Exploration, "在宁静花园的旧文件中发现。"));

            AddLore(new LoreEntry("hist_020", "第一个觉醒者", LoreCategory.HistoryShard,
                "关于第一个能够与森林意志沟通的人的记载。她是一个年轻的女孩，据说通过灵魂花与森林建立了连接。她后来成为了第一任守护者。",
                LoreSource.SpecialCondition, "在灵魂花绽放时前往流星陨坑。"));

            AddLore(new LoreEntry("hist_021", "武器库", LoreCategory.HistoryShard,
                "关于一个隐藏武器库的记录。库中存放着专门用来对抗黑雾的武器，但位置已经失传。只有携带特定徽章的人才能找到入口。",
                LoreSource.QuestReward, "收集足够的废墟线索后解锁武器库位置。"));

            AddLore(new LoreEntry("hist_022", "结界崩溃", LoreCategory.HistoryShard,
                "记录了保护森林的结界崩溃的过程。结界的能量来源是森林之心，但不知何时起，能量开始逐渐流失。最终结界在某个夜晚彻底崩溃。",
                LoreSource.Exploration, "在记忆之树查看结界的记忆碎片。"));

            AddLore(new LoreEntry("hist_023", "瘟疫之源", LoreCategory.HistoryShard,
                "一份关于蔓延在村庄中的奇怪瘟疫的报告。瘟疫让感染者产生幻觉，最终在迷失中死去。研究指出瘟疫可能与黑雾有某种关联。",
                LoreSource.Exploration, "在沉没村庄的废墟中发现病人的遗留物。"));

            AddLore(new LoreEntry("hist_024", "最后的抵抗", LoreCategory.HistoryShard,
                "描述了幸存者们在黑雾面前进行最后抵抗的经过。他们在骷髅祭坛进行了某种仪式，试图阻挡黑雾的推进。仪式似乎起了作用，但代价惨重。",
                LoreSource.Exploration, "在骷髅祭坛阅读残留的记录。"));

            AddLore(new LoreEntry("hist_025", "历史的真相", LoreCategory.HistoryShard,
                "综合所有历史碎片得出的结论：这个森林是一个被时间遗忘的实验场。黑雾、守护者、古老文明，都是某位超越者计划的组成部分。而你，可能就是被选中的观察者。",
                LoreSource.SpecialCondition, "收集至少20个历史碎片后自动解锁。"));
        }

        // ============================================================
        // F. 隐藏真相（25个）
        // ============================================================

        private void CreateHiddenTruths()
        {
            AddLore(new LoreEntry("hidden_001", "你并非第一次来此", LoreCategory.HiddenTruth,
                "手账残页中隐藏的信息表明，你可能在很久以前就曾经来过这片森林。手账本身的来源就是一个谜团，上面记录的笔迹似乎是属于你自己的。",
                LoreSource.StoryTrigger, "仔细阅读手账中的每一个细节。"));

            AddLore(new LoreEntry("hidden_002", "黑雾的真实身份", LoreCategory.HiddenTruth,
                "所有的线索都指向一个令人不安的结论：黑雾并非外来的侵略者，而是森林本身为了自我保护而产生的抗体。它在清除所有可能伤害森林的威胁——包括人类。",
                LoreSource.QuestReward, "在森林之心与古树交流后获得这段真相。"));

            AddLore(new LoreEntry("hidden_003", "NPC的真实身份", LoreCategory.HiddenTruth,
                "那些帮助你的NPC，他们真的只是幸存者吗？某些细节暗示他们可能与你一样，被困在这个时间循环中，一次又一次地重复着相同的命运。",
                LoreSource.NPCGift, "与隐居者进行多次深入对话。"));

            AddLore(new LoreEntry("hidden_004", "手账的来源", LoreCategory.HiddenTruth,
                "这本手账并不是你购买的，也不是任何人送给你的。它似乎一直就在你的身边，但你想不起来是从什么时候开始的。也许它是你自己留下的。",
                LoreSource.StoryTrigger, "在特定条件下触发这段记忆。"));

            AddLore(new LoreEntry("hidden_005", "森林的意志", LoreCategory.HiddenTruth,
                "森林并不只是一个生态系统，它拥有自己的意识和意志。它在观察着每一个进入它领地的人，判断他们是威胁还是朋友。而这个判断的标准，似乎与黑雾有关。",
                LoreSource.SpecialCondition, "在古树桩前冥想足够长的时间。"));

            AddLore(new LoreEntry("hidden_006", "镜面池塘的真相", LoreCategory.HiddenTruth,
                "镜面池塘映照的不是过去，而是可能的未来。那些影像是在告诉你，如果你做出不同的选择，事情可能会如何发展。但选择权真的在你手上吗？",
                LoreSource.SpecialCondition, "在月圆之夜站在镜面池塘中央。"));

            AddLore(new LoreEntry("hidden_007", "时间的裂缝", LoreCategory.HiddenTruth,
                "在某些地方，时间是流动的；在另一些地方，时间是静止的。黑雾漩涡的中心就是一个时间静止的空间，那里的一切都停留在黑雾最初出现的那一刻。",
                LoreSource.Exploration, "在黑雾漩涡边缘观察并记录异常。"));

            AddLore(new LoreEntry("hidden_008", "你的记忆被篡改", LoreCategory.HiddenTruth,
                "某些NPC的反应表明，你来到这片森林的方式与他们记忆中的不一样。也许你的记忆被某个人或某种力量修改过，而你对此一无所知。",
                LoreSource.QuestReward, "完成真相追寻者任务线。"));

            AddLore(new LoreEntry("hidden_009", "另一个你", LoreCategory.HiddenTruth,
                "手账中夹着一张你从未见过的照片。照片上有一个人，长得和你一模一样，但他穿着你不记得拥有的衣服，站在一个你不认识的地方。",
                LoreSource.StoryTrigger, "在手账中找到这张被藏起来的照片。"));

            AddLore(new LoreEntry("hidden_010", "黑雾不是敌人", LoreCategory.HiddenTruth,
                "所有的敌人都被黑雾侵蚀了吗？还是说，黑雾只是给了它们力量，让它们能够在这片日益危险的森林中生存？也许黑雾从来就不是威胁，而是解决方案。",
                LoreSource.SpecialCondition, "尝试与黑雾兽进行非暴力互动。"));

            AddLore(new LoreEntry("hidden_011", "守护者的代价", LoreCategory.HistoryShard,
                "成为守护者意味着什么？代价是什么？森林守护者从不谈论这个，但它们的存在本身就说明了某种牺牲的存在。也许它们曾经也是普通的人类或生物。",
                LoreSource.NPCGift, "与森林守护者进行灵魂共鸣后得知。"));

            AddLore(new LoreEntry("hidden_012", "循环的终点", LoreCategory.HiddenTruth,
                "所有的路都通向森林之心，所有的故事都从迷雾森林入口开始。这不是巧合。也许这个森林就是一个巨大的循环，而你只是这个循环中的又一个过客。",
                LoreSource.StoryTrigger, "到达森林之心后触发这段思考。"));

            AddLore(new LoreEntry("hidden_013", "手账的秘密", LoreCategory.HiddenTruth,
                "手账不只是用来记录的。它似乎有某种特殊的能力，能够保存记忆，甚至能够创造记忆。当你写下手账的内容时，你是在记录还是在创造？",
                LoreSource.Crafting, "用手账记录足够多的内容后解锁。"));

            AddLore(new LoreEntry("hidden_014", "森林在等待", LoreCategory.HiddenTruth,
                "这片森林似乎在等待着什么。每一次黑雾的扩张，都像是在试探；每一个来到这里的旅人，都像是在接受测试。而测试的终点，就是森林之心。",
                LoreSource.Exploration, "仔细观察森林中各种生物的行为模式。"));

            AddLore(new LoreEntry("hidden_015", "消失的居民", LoreCategory.HiddenTruth,
                "沉没的村庄、古老的废墟、秘密的营地——这里曾经有很多人。他们都去了哪里？也许答案就在黑雾之中，也许他们从未真正离开。",
                LoreSource.Exploration, "在所有曾经有人的地方寻找线索。"));

            AddLore(new LoreEntry("hidden_016", "你也是被选中的", LoreCategory.HiddenTruth,
                "不是每个人都会被黑雾吸引，不是每个人都能看到镜面池塘的幻象，不是每个人都能听到森林的声音。你被选中是有原因的，只是这个原因尚未揭晓。",
                LoreSource.SpecialCondition, "在所有条件都满足的情况下与古树交流。"));

            AddLore(new LoreEntry("hidden_017", "钥匙的真正用途", LoreCategory.HiddenTruth,
                "那把钥匙不是用来打开物理的门。它是用来打开某种封印的——也许是记忆的封印，也许是力量的封印，也许是真相的封印。",
                LoreSource.QuestReward, "在找到钥匙后继续追踪它的用途。"));

            AddLore(new LoreEntry("hidden_018", "雾主的真实目的", LoreCategory.HiddenTruth,
                "雾主自称是黑雾的代言人，但它真正的目的是什么？它是在守护什么，还是在等待什么？与它对视的那一刻，你似乎看到了某种深沉的悲伤。",
                LoreSource.SpecialCondition, "在击败雾主前与它对话。"));

            AddLore(new LoreEntry("hidden_019", "游戏并不存在", LoreCategory.HiddenTruth,
                "这句话是什么意思？当你开始思考这个问题的时候，你已经触及了这个世界的边缘。也许这不是游戏，也许我们是某种更宏大计划的一部分。",
                LoreSource.StoryTrigger, "收集所有150个Lore条目后，在手账最后一页写下这句话。"));

            AddLore(new LoreEntry("hidden_020", "森林的真实形态", LoreCategory.HiddenTruth,
                "你看到的森林是它的真实形态吗？还是说，这只是它选择让你看到的样子？也许在黑雾的背后，隐藏着一个完全不同的世界。",
                LoreSource.SpecialCondition, "在黑雾漩涡中心存活足够长的时间。"));

            AddLore(new LoreEntry("hidden_021", "记忆之树的秘密", LoreCategory.HiddenTruth,
                "那些挂在记忆之树上的碎片不只是保存着过去的记忆。它们是活的。它们在等待某个人来收集它们，然后成为那个人的一部分。",
                LoreSource.NPCGift, "与记忆之树进行深度共鸣。"));

            AddLore(new LoreEntry("hidden_022", "你的另一个结局", LoreCategory.HiddenTruth,
                "在所有已知的结局之外，还有一个从未有人达成过的隐藏结局。那个结局需要你放弃一切，包括你的记忆、你的力量，甚至是你自己。",
                LoreSource.SpecialCondition, "解锁所有其他结局后，在手账上写下你的真实名字。"));

            AddLore(new LoreEntry("hidden_023", "黑雾与你的连接", LoreCategory.HiddenTruth,
                "为什么你能抵御黑雾的侵蚀，而其他旅人却不行？也许你与黑雾之间存在着某种超越表面的联系。也许你就是黑雾的一部分。",
                LoreSource.QuestReward, "完成黑雾溯源任务线。"));

            AddLore(new LoreEntry("hidden_024", "这个世界的边界", LoreCategory.HiddenTruth,
                "在这个世界的边界之外，有什么在等待着你？是虚无，还是另一个世界？有时候，最让人恐惧的不是未知本身，而是知道边界在哪里。",
                LoreSource.Exploration, "尝试走到地图的边缘。"));

            AddLore(new LoreEntry("hidden_025", "真正的自由", LoreCategory.HiddenTruth,
                "什么是自由？离开这片森林是自由吗？还是说，真正的自由是接受这片森林作为你命运的一部分？也许当你不再试图逃离的时候，你才真正获得了自由。",
                LoreSource.SpecialCondition, "在理解所有真相之后，做出你的选择。"));
        }

        // ============================================================
        // 核心方法
        // ============================================================

        private void AddLore(LoreEntry entry)
        {
            allLoreEntries.Add(entry);
        }

        private void UpdateStats()
        {
            stats.unlockedCount = unlockedLoreIds.Count;
            stats.categoryProgress.Clear();
            foreach (LoreCategory cat in System.Enum.GetValues(typeof(LoreCategory)))
            {
                if (categoryIndex.ContainsKey(cat))
                {
                    int unlocked = categoryIndex[cat].Count(e => e.isUnlocked);
                    stats.categoryProgress[cat] = unlocked;
                }
            }
        }

        // ============================================================
        // 公开API - 解锁与管理
        // ============================================================

        /// <summary>
        /// 解锁指定ID的Lore条目
        /// </summary>
        public bool UnlockLore(string loreId)
        {
            if (string.IsNullOrEmpty(loreId) || !loreDict.ContainsKey(loreId))
                return false;

            var entry = loreDict[loreId];
            if (entry.isUnlocked)
                return false;

            entry.isUnlocked = true;
            entry.isNew = true;
            unlockedLoreIds.Add(loreId);

            UpdateStats();
            SaveCollection();

            var eventArgs = new LoreUnlockedEventArgs { Entry = entry, IsNewDiscovery = true };
            OnLoreUnlocked?.Invoke(this, eventArgs);
            OnCollectionProgressChanged?.Invoke(this, stats);

            // 检查NPC对话解锁
            CheckNPCDialogueUnlocks();
            // 检查任务触发
            CheckQuestTriggers();

            return true;
        }

        /// <summary>
        /// 解锁指定分类的所有Lore
        /// </summary>
        public void UnlockAllInCategory(LoreCategory category)
        {
            if (!categoryIndex.ContainsKey(category))
                return;

            foreach (var entry in categoryIndex[category])
            {
                if (!entry.isUnlocked)
                {
                    UnlockLore(entry.id);
                }
            }
        }

        /// <summary>
        /// 根据来源类型解锁Lore
        /// </summary>
        public void UnlockBySource(LoreSource source)
        {
            foreach (var entry in allLoreEntries)
            {
                if (entry.source == source && !entry.isUnlocked)
                {
                    UnlockLore(entry.id);
                }
            }
        }

        /// <summary>
        /// 获取Lore条目
        /// </summary>
        public LoreEntry GetLore(string loreId)
        {
            return loreDict.ContainsKey(loreId) ? loreDict[loreId] : null;
        }

        /// <summary>
        /// 获取所有已解锁的Lore
        /// </summary>
        public List<LoreEntry> GetUnlockedLore()
        {
            return allLoreEntries.Where(e => e.isUnlocked).ToList();
        }

        /// <summary>
        /// 获取指定分类的Lore列表
        /// </summary>
        public List<LoreEntry> GetLoreByCategory(LoreCategory category)
        {
            return categoryIndex.ContainsKey(category) ? categoryIndex[category].ToList() : new List<LoreEntry>();
        }

        /// <summary>
        /// 获取分类中已解锁的Lore
        /// </summary>
        public List<LoreEntry> GetUnlockedLoreByCategory(LoreCategory category)
        {
            if (!categoryIndex.ContainsKey(category))
                return new List<LoreEntry>();
            return categoryIndex[category].Where(e => e.isUnlocked).ToList();
        }

        /// <summary>
        /// 标记Lore为已读（不再是新发现）
        /// </summary>
        public void MarkAsRead(string loreId)
        {
            if (loreDict.ContainsKey(loreId))
            {
                loreDict[loreId].isNew = false;
                SaveCollection();
            }
        }

        /// <summary>
        /// 获取收集统计
        /// </summary>
        public CollectionStats GetStats()
        {
            return stats;
        }

        /// <summary>
        /// 获取收集进度百分比
        /// </summary>
        public float GetCompletionPercentage()
        {
            return stats.GetProgressPercentage();
        }

        /// <summary>
        /// 获取新发现的Lore条目
        /// </summary>
        public List<LoreEntry> GetNewDiscoveries()
        {
            return allLoreEntries.Where(e => e.isUnlocked && e.isNew).ToList();
        }

        /// <summary>
        /// 检查NPC对话是否解锁
        /// </summary>
        public string CheckNPCDialogueUnlocksFor(string npcId)
        {
            foreach (var unlock in npcDialogueUnlocks)
            {
                if (unlock.npcId == npcId)
                {
                    bool allFound = unlock.requiredLoreIds.All(id =>
                        loreDict.ContainsKey(id) && loreDict[id].isUnlocked);

                    if (allFound)
                        return unlock.unlockedDialogueKey;
                }
            }
            return null;
        }

        private void CheckNPCDialogueUnlocks()
        {
            foreach (var unlock in npcDialogueUnlocks)
            {
                bool allFound = unlock.requiredLoreIds.All(id =>
                    loreDict.ContainsKey(id) && loreDict[id].isUnlocked);

                if (allFound)
                {
                    // 触发NPC对话解锁事件
                    Debug.Log($"[LoreCollection] NPC对话解锁触发: {unlock.npcId} -> {unlock.unlockedDialogueKey}");
                }
            }
        }

        /// <summary>
        /// 检查任务是否满足触发条件
        /// </summary>
        public List<string> CheckQuestTriggersForCategory(LoreCategory category)
        {
            var triggeredQuests = new List<string>();
            string categoryName = category.ToString();

            foreach (var trigger in questTriggers)
            {
                if (trigger.triggeredByCategories.Contains(categoryName))
                {
                    bool allFound = trigger.requiredLoreIds.All(id =>
                        loreDict.ContainsKey(id) && loreDict[id].isUnlocked);

                    if (allFound && !triggeredQuests.Contains(trigger.questId))
                        triggeredQuests.Add(trigger.questId);
                }
            }

            return triggeredQuests;
        }

        private void CheckQuestTriggers()
        {
            foreach (var trigger in questTriggers)
            {
                bool allFound = trigger.requiredLoreIds.All(id =>
                    loreDict.ContainsKey(id) && loreDict[id].isUnlocked);

                if (allFound)
                {
                    Debug.Log($"[LoreCollection] 任务触发: {trigger.questId}");
                }
            }
        }

        /// <summary>
        /// 获取可用的结局条件状态
        /// </summary>
        public List<EndingCondition> GetAvailableEndingConditions()
        {
            var available = new List<EndingCondition>();

            foreach (var condition in endingConditions)
            {
                bool countMet = stats.unlockedCount >= condition.requiredLoreCount;
                bool specificMet = condition.specificLoreIds.Count == 0 ||
                    condition.specificLoreIds.All(id =>
                        loreDict.ContainsKey(id) && loreDict[id].isUnlocked);

                if (countMet && specificMet)
                    available.Add(condition);
            }

            return available;
        }

        /// <summary>
        /// 是否满足某结局条件
        /// </summary>
        public bool MeetsEndingCondition(string endingId)
        {
            var condition = endingConditions.FirstOrDefault(e => e.endingId == endingId);
            if (condition == null) return false;

            bool countMet = stats.unlockedCount >= condition.requiredLoreCount;
            bool specificMet = condition.specificLoreIds.Count == 0 ||
                condition.specificLoreIds.All(id =>
                    loreDict.ContainsKey(id) && loreDict[id].isUnlocked);

            return countMet && specificMet;
        }

        // ============================================================
        // UI显示相关
        // ============================================================

        /// <summary>
        /// 设置当前显示的分类
        /// </summary>
        public void SetDisplayCategory(LoreCategory category)
        {
            currentDisplayCategory = category;
            currentDisplayList = GetLoreByCategory(category);
            currentPage = 0;
        }

        /// <summary>
        /// 获取当前页的Lore条目
        /// </summary>
        public List<LoreEntry> GetCurrentPageEntries()
        {
            int start = currentPage * ITEMS_PER_PAGE;
            if (start >= currentDisplayList.Count)
                return new List<LoreEntry>();

            int count = Mathf.Min(ITEMS_PER_PAGE, currentDisplayList.Count - start);
            return currentDisplayList.GetRange(start, count);
        }

        /// <summary>
        /// 下一页
        /// </summary>
        public bool NextPage()
        {
            int maxPage = (currentDisplayList.Count - 1) / ITEMS_PER_PAGE;
            if (currentPage < maxPage)
            {
                currentPage++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 上一页
        /// </summary>
        public bool PreviousPage()
        {
            if (currentPage > 0)
            {
                currentPage--;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取当前页码信息
        /// </summary>
        public string GetPageInfo()
        {
            int totalPages = Mathf.Max(1, (currentDisplayList.Count + ITEMS_PER_PAGE - 1) / ITEMS_PER_PAGE);
            return $"第 {currentPage + 1} / {totalPages} 页";
        }

        /// <summary>
        /// 获取分类名称（中文）
        /// </summary>
        public static string GetCategoryName(LoreCategory category)
        {
            switch (category)
            {
                case LoreCategory.EnemyCodex: return "敌人图鉴";
                case LoreCategory.PlantCodex: return "植物图鉴";
                case LoreCategory.ItemCodex: return "物品图鉴";
                case LoreCategory.LocationRecord: return "地点记录";
                case LoreCategory.HistoryShard: return "历史碎片";
                case LoreCategory.HiddenTruth: return "隐藏真相";
                default: return category.ToString();
            }
        }

        /// <summary>
        /// 获取来源名称（中文）
        /// </summary>
        public static string GetSourceName(LoreSource source)
        {
            switch (source)
            {
                case LoreSource.EnemyDrop: return "敌人掉落";
                case LoreSource.Exploration: return "探索发现";
                case LoreSource.QuestReward: return "任务获得";
                case LoreSource.SpecialCondition: return "特殊条件";
                case LoreSource.NPCGift: return "NPC赠送";
                case LoreSource.Crafting: return "合成获得";
                case LoreSource.StoryTrigger: return "剧情触发";
                default: return source.ToString();
            }
        }

        /// <summary>
        /// 获取Lore条目的解锁状态描述
        /// </summary>
        public static string GetLockStatus(LoreEntry entry)
        {
            if (entry.isUnlocked)
                return entry.isNew ? "新发现！" : "已解锁";
            return "未解锁";
        }

        // ============================================================
        // 存档相关
        // ============================================================

        /// <summary>
        /// 保存收集进度
        /// </summary>
        public void SaveCollection()
        {
            try
            {
                var saveData = new List<LoreSaveData>();
                foreach (var entry in allLoreEntries)
                {
                    if (entry.isUnlocked)
                    {
                        saveData.Add(new LoreSaveData { id = entry.id, isUnlocked = true, isNew = entry.isNew });
                    }
                }

                string json = JsonUtility.ToJson(new Serialization<LoreSaveData>(saveData));
                PlayerPrefs.SetString(SAVE_KEY, json);
                PlayerPrefs.Save();
            }
            catch (Exception e)
            {
                Debug.LogError($"[LoreCollection] 保存失败: {e.Message}");
            }
        }

        /// <summary>
        /// 加载收集进度
        /// </summary>
        public void LoadCollection()
        {
            try
            {
                if (PlayerPrefs.HasKey(SAVE_KEY))
                {
                    string json = PlayerPrefs.GetString(SAVE_KEY);
                    var saveData = JsonUtility.FromJson<Serialization<LoreSaveData>>(json);

                    if (saveData != null && saveData.items != null)
                    {
                        foreach (var data in saveData.items)
                        {
                            if (loreDict.ContainsKey(data.id))
                            {
                                loreDict[data.id].isUnlocked = data.isUnlocked;
                                loreDict[data.id].isNew = data.isNew;
                                if (data.isUnlocked)
                                    unlockedLoreIds.Add(data.id);
                            }
                        }
                    }
                }
                else
                {
                    // 默认解锁部分初始Lore
                    UnlockLore("enemy_001");
                    UnlockLore("plant_001");
                    UnlockLore("item_001");
                    UnlockLore("loc_001");
                }

                UpdateStats();
            }
            catch (Exception e)
            {
                Debug.LogError($"[LoreCollection] 加载失败: {e.Message}");
            }
        }

        /// <summary>
        /// 重置收集进度
        /// </summary>
        public void ResetCollection()
        {
            foreach (var entry in allLoreEntries)
            {
                entry.isUnlocked = false;
                entry.isNew = false;
            }
            unlockedLoreIds.Clear();
            UpdateStats();
            SaveCollection();
        }

        /// <summary>
        /// 导出收集数据（用于调试）
        /// </summary>
        public string ExportCollectionData()
        {
            var export = new CollectionExport
            {
                totalEntries = stats.totalEntries,
                unlockedCount = stats.unlockedCount,
                percentage = GetCompletionPercentage(),
                categories = new Dictionary<string, CategoryExport>()
            };

            foreach (LoreCategory cat in System.Enum.GetValues(typeof(LoreCategory)))
            {
                export.categories[cat.ToString()] = new CategoryExport
                {
                    name = GetCategoryName(cat),
                    total = stats.categoryTotal.ContainsKey(cat) ? stats.categoryTotal[cat] : 0,
                    unlocked = stats.categoryProgress.ContainsKey(cat) ? stats.categoryProgress[cat] : 0,
                    percentage = stats.GetCategoryProgress(cat)
                };
            }

            return JsonUtility.ToJson(export, true);
        }
    }

    // ============================================================
    // 辅助数据结构
    // ============================================================

    [Serializable]
    public class LoreSaveData
    {
        public string id;
        public bool isUnlocked;
        public bool isNew;
    }

    [Serializable]
    public class Serialization<T>
    {
        public List<T> items;

        public Serialization() { items = new List<T>(); }
        public Serialization(List<T> items) { this.items = items; }
    }

    [Serializable]
    public class CollectionExport
    {
        public int totalEntries;
        public int unlockedCount;
        public float percentage;
        public Dictionary<string, CategoryExport> categories;
    }

    [Serializable]
    public class CategoryExport
    {
        public string name;
        public int total;
        public int unlocked;
        public float percentage;
    }
}
