using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 故事线类型
/// </summary>
public enum StorylineType
{
    NPCBackground,      // NPC背景故事线
    RegionExploration,  // 区域探索故事线
    DarkFogTruth,       // 黑雾真相故事线
    HistoricalMystery,  // 历史谜团故事线（与区域探索合并）
    HiddenEnding       // 隐藏结局故事线
}

/// <summary>
/// 故事线阶段状态
/// </summary>
public enum StorylineStepState
{
    Locked,
    Active,
    Completed,
    Failed
}

/// <summary>
/// 单个故事线数据
/// </summary>
[Serializable]
public class Storyline
{
    public string id;
    public string name;
    public string shortName;
    public StorylineType type;
    public int difficulty;
    public List<StorylineStep> steps;
    public int currentStepIndex;
    public bool isUnlocked;
    public bool isCompleted;
    public string prerequisite;
    public List<string> triggerNPCs;
    public List<string> triggerAreas;
    public string titleReward;
    public int memoryReward;
    public int soulReward;

    public StorylineStep CurrentStep =>
        (currentStepIndex >= 0 && currentStepIndex < steps.Count)
            ? steps[currentStepIndex]
            : null;

    public float Progress =>
        steps.Count > 0 ? (float)currentStepIndex / steps.Count : 0f;

    public Storyline()
    {
        steps = new List<StorylineStep>();
        triggerNPCs = new List<string>();
        triggerAreas = new List<string>();
        currentStepIndex = 0;
        isUnlocked = false;
        isCompleted = false;
    }
}

/// <summary>
/// 单个步骤数据
/// </summary>
[Serializable]
public class StorylineStep
{
    public int index;
    public string title;
    public string narrative;
    public string npcDialogue;
    public string playerChoicePrompt;
    public List<StoryChoice> choices;
    public string locationId;
    public string requiredItem;
    public int requiredDay;
    public bool requiresNGPlus;
    public bool requiresAllEndings;
    public StorylineStepState state;
    public string completionHint;

    public StorylineStep()
    {
        choices = new List<StoryChoice>();
        state = StorylineStepState.Locked;
    }
}

/// <summary>
/// 故事选择分支
/// </summary>
[Serializable]
public class StoryChoice
{
    public string id;
    public string text;
    public string resultNarrative;
    public int memoryReward;
    public int soulReward;
    public string statBonus;
    public string titleGranted;
    public string nextStepForce;
}

/// <summary>
/// 故事线进度存档数据
/// </summary>
[Serializable]
public class StorylineSaveData
{
    public List<string> unlockedStorylineIds;
    public List<string> completedStorylineIds;
    public Dictionary<string, int> stepProgress;
    public Dictionary<string, List<string>> choiceHistory;
    public List<string> grantedTitles;
}

// =====================================================================
// 故事线扩展系统 v1.0
// 15条支线故事线：NPC背景(5) + 区域探索(3) + 黑雾真相(4) + 隐藏结局(3)
// =====================================================================
public class StorylineExpansionSystem : MonoBehaviour
{
    public static StorylineExpansionSystem instance { get; private set; }

    [Header("所有故事线")]
    public List<Storyline> allStorylines = new List<Storyline>();

    [Header("已解锁")]
    public List<string> unlockedIds = new List<string>();
    [Header("已完成")]
    public List<string> completedIds = new List<string>();
    [Header("已获称号")]
    public List<string> acquiredTitles = new List<string>();

    private Dictionary<string, Storyline> storylineDict;

    // 事件
    public event Action<Storyline> OnStorylineUnlocked;
    public event Action<Storyline, StorylineStep> OnStepActivated;
    public event Action<Storyline, StorylineStep> OnStepCompleted;
    public event Action<Storyline> OnStorylineCompleted;
    public event Action<string, string> OnTitleGranted;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAllStorylines();
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
            GameManager gm = GameManager.instance;
            gm.onNPCDialogue += HandleNPCDialogue;
            gm.onExploreLocation += HandleLocationExplored;
            gm.onItemUsed += HandleItemUsed;
            gm.onDayChanged += HandleDayChanged;
        }
    }

    void OnDestroy()
    {
        if (GameManager.instance != null)
        {
            GameManager gm = GameManager.instance;
            gm.onNPCDialogue -= HandleNPCDialogue;
            gm.onExploreLocation -= HandleLocationExplored;
            gm.onItemUsed -= HandleItemUsed;
            gm.onDayChanged -= HandleDayChanged;
        }
    }

    // ==================== 15条故事线初始化 ====================

    private void InitializeAllStorylines()
    {
        storylineDict = new Dictionary<string, Storyline>();

        // ==================== NPC背景故事线（5条）====================

        // 1. 玛莎的秘密（4步）
        AddStoryline(new Storyline
        {
            id = "story_martha_secret",
            name = "玛莎的秘密",
            shortName = "母亲的心",
            type = StorylineType.NPCBackground,
            difficulty = 2,
            prerequisite = "与玛莎对话3次",
            triggerNPCs = new List<string> { "martha" },
            triggerAreas = new List<string> { "abandoned_village" },
            titleReward = "母亲的心",
            memoryReward = 8,
            soulReward = 3,
            steps = new List<StorylineStep>
            {
                new StorylineStep
                {
                    index = 0, title = "尘封的往事",
                    narrative = "在篝火旁，玛莎的眼神变得悠远。她轻声说起了很久以前的事——\n\n「我曾经有个女儿，叫小萤。她有着和你一样明亮的眼睛...那时候森林还没有被黑雾侵蚀，我们住在东边的村子里。」\n\n她的声音微微颤抖，似乎这段记忆对她来说既珍贵又痛苦。\n\n「小萤...她最喜欢在森林里采蘑菇了。每次回来篮子里都是满满的...」",
                    npcDialogue = "「她才八岁...八岁那年，她为了救一只受伤的小鹿冲进了黑雾。我永远忘不了那天...」",
                    locationId = "camp", requiredDay = 3,
                    state = StorylineStepState.Locked,
                    completionHint = "与玛莎深入交谈，询问她女儿的更多事情"
                },
                new StorylineStep
                {
                    index = 1, title = "废弃村落的回响",
                    narrative = "你根据玛莎的描述，找到了那座早已被黑雾侵蚀的废弃村落。断壁残垣间，杂草丛生。\n\n在一间半倒塌的小屋里，你发现了一个布满灰尘的篮子，里面还有几朵早已干枯的彩色蘑菇，以及一张泛黄的画像——画中是一个有着明亮眼睛的小女孩。\n\n篮子下面压着一封信，字迹稚嫩：「妈妈，我在森林里发现了一个秘密的地方，等我长大了带你去看！」",
                    npcDialogue = "",
                    locationId = "abandoned_village", requiredDay = 5,
                    state = StorylineStepState.Locked,
                    completionHint = "在废弃村落找到小萤的遗物并带回给玛莎"
                },
                new StorylineStep
                {
                    index = 2, title = "黑雾的代价",
                    narrative = "玛莎颤抖着接过那些遗物，泪水无声地滑落。\n\n「她...她是为了救一只受伤的小鹿才冲进黑雾的...」\n\n玛莎的声音哽咽：「那是我最后一次见到她。黑雾吞噬了她的身影，我只来得及听见她最后一声呼唤...」\n\n「她才八岁...她才八岁啊...」\n\n玛莎紧紧抱着那个篮子，仿佛要把它揉进自己的生命里。",
                    npcDialogue = "「黑雾...是黑雾夺走了我的小萤！如果早知道会变成这样，我绝不会让她离开我的视线...」",
                    locationId = "camp", requiredDay = 7,
                    state = StorylineStepState.Locked,
                    completionHint = "安慰玛莎，听她讲述完整的过去"
                },
                new StorylineStep
                {
                    index = 3, title = "母亲的释怀",
                    narrative = "那天晚上，玛莎在篝火旁为小萤点了一盏小小的灯笼。\n\n「谢谢你...帮我找回了这些记忆。」她望着火焰，眼中不再只有悲伤，「小萤她...一直希望我能快乐。我不能辜负她的期望。」\n\n她转过头，目光温柔而坚定：「你愿意听听我其他的故事吗？我发现，和你聊这些让我好受多了。」\n\n—— 玛莎的心结终于解开了一段。她不再是那个被过去困住的人，而是一个愿意分享、愿意继续前行的灵魂。",
                    npcDialogue = "「母亲的心」称号获得者：玛莎\n「谢谢你，孩子。你让我明白了，回忆不是枷锁，而是力量。」",
                    locationId = "camp",
                    state = StorylineStepState.Locked,
                    completionHint = "玛莎已释怀"
                }
            }
        });

        // 2. 莉莉的父母（5步）
        AddStoryline(new Storyline
        {
            id = "story_lily_parents",
            name = "莉莉的父母",
            shortName = "传承者",
            type = StorylineType.NPCBackground,
            difficulty = 4,
            prerequisite = "莉莉好感度≥50",
            triggerNPCs = new List<string> { "lily" },
            triggerAreas = new List<string> { "forbidden_library" },
            titleReward = "传承者",
            memoryReward = 15,
            soulReward = 8,
            steps = new List<StorylineStep>
            {
                new StorylineStep
                {
                    index = 0, title = "森林研究者",
                    narrative = "莉莉很少提起自己的家人。但今天，她似乎愿意多说一些。\n\n「我的父母...他们曾经是森林研究者。」她的目光望向远方，「他们走遍了这片森林的每一个角落，记录着这里的植物、动物，还有...黑雾的规律。」\n\n「他们说，总有一天会找到对抗黑雾的方法。为了所有人。」",
                    npcDialogue = "「他们留下了一大堆笔记和地图，但有些地方...他们特别警告我不要去。说是太危险了。」",
                    locationId = "camp", requiredDay = 5,
                    state = StorylineStepState.Locked,
                    completionHint = "询问莉莉关于她父母研究的更多细节"
                },
                new StorylineStep
                {
                    index = 1, title = "禁忌图书馆",
                    narrative = "你冒险进入了禁忌图书馆——那个莉莉父母曾留下足迹的地方。\n\n灰尘厚重的书架间，你找到了一本被特殊封印的笔记本。封面写着：「森林之心研究——林远、苏雪梅」。\n\n笔记中记载着他们对森林之心的发现，以及一个惊人的理论：「黑雾并非天灾，而是人为封印失败的结果。」\n\n最后一页写着：「我们找到了修复封印的方法。但需要前往森林之心。我们必须去。」",
                    npcDialogue = "",
                    locationId = "forbidden_library", requiredDay = 10,
                    state = StorylineStepState.Locked,
                    completionHint = "在禁忌图书馆找到莉莉父母的研究笔记"
                },
                new StorylineStep
                {
                    index = 2, title = "森林之心",
                    narrative = "笔记的最后几页被水渍模糊，但还能辨认出零星的文字：「森林之心...黑雾的源头...我们到了...」\n\n「封印的核心在森林最深处的石碑下。但它已经破损了数十年...需要用特殊的仪式来修复。」\n\n「如果成功，黑雾将永远被压制。如果失败...」\n\n笔记到此戛然而止。",
                    npcDialogue = "",
                    locationId = "forest_heart", requiredDay = 15,
                    state = StorylineStepState.Locked,
                    completionHint = "解读笔记中关于森林之心的信息"
                },
                new StorylineStep
                {
                    index = 3, title = "父母的牺牲",
                    narrative = "在笔记的夹层中，你发现了一封单独的信——是莉莉父母留下的。\n\n「亲爱的莉莉：\n\n如果你读到这封信，说明我们没能回来。我们找到了修复封印的方法，但仪式需要献祭者的生命作为引导。\n\n我们选择了留下，让森林的其他人有机会逃离。这不是牺牲，这是我们的选择。\n\n爸爸妈妈永远爱你。希望你能原谅我们的离开。」\n\n泪水模糊了你的视线——莉莉的父母，是真正的英雄。",
                    npcDialogue = "",
                    locationId = "forest_heart", requiredDay = 18,
                    state = StorylineStepState.Locked,
                    completionHint = "找到莉莉父母最后的遗书"
                },
                new StorylineStep
                {
                    index = 4, title = "继承遗志",
                    narrative = "当你把这一切告诉莉莉时，她沉默了很久。\n\n「所以...他们是为了保护大家...」她的声音带着哭腔，但眼神却渐渐坚定。\n\n「我要完成他们没做完的事。」她握紧了拳头，「我要亲眼看看森林之心，然后告诉所有人——黑雾是可以被战胜的。」\n\n她抬起头，眼中闪烁着泪光与决心的光芒：「你愿意和我一起去吗？」\n\n—— 莉莉成为了「传承者」。她继承了父母的意志，踏上了寻找真相的道路。",
                    npcDialogue = "「传承者」称号获得者：莉莉\n「爸爸妈妈，我明白了。我会带着你们的梦想，继续走下去。」",
                    locationId = "camp",
                    state = StorylineStepState.Locked,
                    completionHint = "莉莉继承了父母的遗志"
                }
            }
        });

        // 3. 埃里克战友（4步）
        AddStoryline(new Storyline
        {
            id = "story_erik_comrade",
            name = "埃里克战友",
            shortName = "故友之念",
            type = StorylineType.NPCBackground,
            difficulty = 3,
            prerequisite = "埃里克好感度≥40",
            triggerNPCs = new List<string> { "erik" },
            triggerAreas = new List<string> { "ancient_trench" },
            titleReward = "故友之念",
            memoryReward = 10,
            soulReward = 5,
            steps = new List<StorylineStep>
            {
                new StorylineStep
                {
                    index = 0, title = "无法说出的名字",
                    narrative = "埃里克总是独自坐在营地边缘，擦拭着他那把刻有符文的佩剑。\n\n今天，他罕见地开口了：「你知道吗，我曾经有个并肩作战的兄弟。」\n\n他的目光变得遥远：「他叫阿城。我们一起巡逻、一起战斗、一起喝酒...那是我最快乐的日子。」\n\n「但是...」他的声音突然停住，「算了，都是过去的事了。」",
                    npcDialogue = "「每次看到这把剑上的符文，我就想起他。我们曾约定，要一起活着看到黑雾消散的那一天...」",
                    locationId = "camp", requiredDay = 5,
                    state = StorylineStepState.Locked,
                    completionHint = "继续询问埃里克关于他战友的事情"
                },
                new StorylineStep
                {
                    index = 1, title = "古老战壕",
                    narrative = "你前往埃里克提到过的「古老战壕」——那是早期巡逻队对抗黑雾兽的防御工事。\n\n在一处被野草覆盖的角落，你发现了一座简陋的石碑，上面刻着几行字：\n\n「于此长眠：阿城\n    森林巡逻队第三小队\n    牺牲于黑雾漩涡平息之战\n    永远的兄弟」\n\n石碑旁还放着一枚已经生锈的巡逻队徽章，和一封被密封保存得很好的信。",
                    npcDialogue = "",
                    locationId = "ancient_trench", requiredDay = 8,
                    state = StorylineStepState.Locked,
                    completionHint = "在古老战壕找到埃里克战友的墓地"
                },
                new StorylineStep
                {
                    index = 2, title = "战友的遗书",
                    narrative = "你小心翼翼地打开那封信，是阿城的笔迹：\n\n「埃里克：\n\n如果你看到这封信，说明我没能活着回来。别太难过，兄弟。\n\n那天黑雾漩涡突然爆发，我选择断后让你带着伤员撤退。这是我的选择，不怪你。\n\n记得替我看看黑雾消散的那一天。如果天空重新变蓝，替我高兴一下。\n\n我这辈子最幸运的事，就是有你这个兄弟。\n\n阿城」\n\n信纸上有泪水的痕迹，但字迹依然清晰。",
                    npcDialogue = "",
                    locationId = "ancient_trench", requiredDay = 10,
                    state = StorylineStepState.Locked,
                    completionHint = "找到阿城的遗书并带给埃里克"
                },
                new StorylineStep
                {
                    index = 3, title = "放下与前行",
                    narrative = "埃里克读完信后，久久没有说话。\n\n然后，一向坚强的战士低下了头，泪水无声地滑落。\n\n「原来...原来他没有怪我...」他的声音哽咽，「我一直以为是我的错...如果我当时不走...」\n\n他深吸一口气，抬起头时眼中已有了新的光芒：「谢谢你，兄弟。谢谢你让我知道真相。」\n\n他站起身，拔出那把刻有符文的佩剑：「阿城，我会替你继续战斗。直到黑雾完全消散的那一天。」\n\n—— 埃里克的战斗属性永久提升 +5\n—— 「故友之念」称号已激活：埃里克在战斗中更加勇猛，不再被过去的阴影所束缚。",
                    npcDialogue = "「故友之念」效果激活：埃里克攻击+5\n「阿城，我不会再逃避了。你的那份，我会一起扛起来。」",
                    locationId = "camp",
                    state = StorylineStepState.Locked,
                    completionHint = "埃里克终于释怀"
                }
            }
        });

        // 4. 灰烬的真相（5步）
        AddStoryline(new Storyline
        {
            id = "story_ashes_truth",
            name = "灰烬的真相",
            shortName = "守护者的遗产",
            type = StorylineType.NPCBackground,
            difficulty = 5,
            prerequisite = "完成「黑雾漩涡」隐藏区域发现",
            triggerNPCs = new List<string> { "ashes" },
            triggerAreas = new List<string> { "dark_vortex" },
            titleReward = "守护者的遗产",
            memoryReward = 20,
            soulReward = 15,
            steps = new List<StorylineStep>
            {
                new StorylineStep
                {
                    index = 0, title = "死过一次的人",
                    narrative = "灰烬站在黑雾边缘，身影在浓雾中若隐若现。\n\n「你知道吗，」他头也不回地说，「我已经死过一次了。」\n\n你走近几步，看到他肩上的伤口正在以不自然的速度愈合。\n\n「很多年前，我也曾在这片森林中冒险。那时候，我也有一群并肩的伙伴...」\n\n他转过身，那双眼睛中仿佛燃烧着余烬：「直到我们触碰了不该触碰的东西。」",
                    npcDialogue = "「黑雾...它不是敌人。它是惩罚。是对我们贪婪的代价。」",
                    locationId = "fog_front", requiredDay = 10,
                    state = StorylineStepState.Locked,
                    completionHint = "在黑雾边缘与灰烬深入交谈"
                },
                new StorylineStep
                {
                    index = 1, title = "黑雾漩涡事件",
                    narrative = "当你们一同前往黑雾漩涡时，周围的空气突然变得冰冷。\n\n灰烬的脸色骤变：「这里...我认得这里...」\n\n他跪倒在地，双手插入黑雾中，黑雾竟然像水一样被他搅动：「封印...封印就在这里...」\n\n突然，黑雾中闪过一道光芒，一个虚幻的身影浮现——那是年轻时的灰烬，正与一群伙伴站在一个巨大的符文阵前。\n\n「我们成功了！」那个年轻的声音喊道，「封印修复了！」\n\n然后是爆炸、黑光、尖叫...一切都消失了。",
                    npcDialogue = "「那时候我们太天真了...以为修复封印就能解决一切。结果...代价是我们的生命。」",
                    locationId = "dark_vortex", requiredDay = 15,
                    state = StorylineStepState.Locked,
                    completionHint = "见证黑雾漩涡事件的历史幻象"
                },
                new StorylineStep
                {
                    index = 2, title = "上一代守护者",
                    narrative = "幻象消散后，灰烬缓缓站起身。\n\n「那一战中，我的伙伴们全部牺牲。而我...被封印本身的力量拉了回来。」\n\n他看着自己的双手：「我的身体早已死亡。但这团黑雾给了我新的存在方式...或者说，是诅咒。」\n\n「我是上一代的森林守护者。」他的声音平静而沉重，「那一战后，我独自守护这片森林数十年，等待着...下一个能够继承这一切的人。」",
                    npcDialogue = "「守护者不是荣耀，是责任。是将自己的一生奉献给这片土地的决心。」",
                    locationId = "dark_vortex", requiredDay = 18,
                    state = StorylineStepState.Locked,
                    completionHint = "了解灰烬作为上一代守护者的真相"
                },
                new StorylineStep
                {
                    index = 3, title = "赎罪的理由",
                    narrative = "「你一定在想，为什么我选择留在黑雾中而不是离开。」\n\n灰烬苦笑：「因为我没有资格离开。我的同伴们为了封印献出了生命，而我...活了下来。」\n\n「这是我欠他们的。所以我留在黑雾中，守护着这片用他们的生命换来的森林。直到有一天...有人能够接过这份责任。」\n\n他看向你：「这也是为什么我一直在暗中帮助你。我想看看你是否就是那个被选中的人。」",
                    npcDialogue = "「如果你能成功...我的伙伴们也会安息的吧。」",
                    locationId = "dark_vortex", requiredDay = 20,
                    state = StorylineStepState.Locked,
                    completionHint = "理解灰烬留在黑雾中的真正原因"
                },
                new StorylineStep
                {
                    index = 4, title = "守护者之证",
                    narrative = "灰烬从胸口取出一块散发着微光的晶石——那是守护者代代相传的信物。\n\n「拿着它。」他把晶石递到你面前，「这是守护者之证，是上一代守护者临终前交给我的。」\n\n「我没有办法完成修复封印的使命，因为我太老了，黑雾的力量也已经侵蚀了我的身体。」\n\n「但你可以。」他的目光灼热，「带着这份力量，去完成我们这一代人没能完成的事业吧。」\n\n—— 你获得了「守护者之证」\n—— 解锁成就「传承之火」\n—— 灰烬的故事线「守护者的遗产」完成",
                    npcDialogue = "「守护者的遗产」称号已激活\n「谢谢你...让我终于能够卸下这份重担。我的伙伴们...我来了。」",
                    locationId = "dark_vortex",
                    state = StorylineStepState.Locked,
                    completionHint = "接受灰烬的守护者之证"
                }
            }
        });

        // 5. 沃斯的背叛（4步）
        AddStoryline(new Storyline
        {
            id = "story_woss_betrayal",
            name = "沃斯的背叛",
            shortName = "诚实商人",
            type = StorylineType.NPCBackground,
            difficulty = 2,
            prerequisite = "完成10个连续任务，发现地精工坊",
            triggerNPCs = new List<string> { "woss" },
            triggerAreas = new List<string> { "goblin_workshop" },
            titleReward = "诚实商人",
            memoryReward = 8,
            soulReward = 4,
            steps = new List<StorylineStep>
            {
                new StorylineStep
                {
                    index = 0, title = "不可告人的秘密",
                    narrative = "沃斯是森林中最富有的商人，他的小店总能拿出各种珍稀物品。\n\n但今天，他的表情异常凝重。\n\n「你来了。」他放下手中的账本，「我观察你很久了。你是个可以信任的人...或者说，我希望你是。」\n\n他叹了口气：「很多年前，我做过一件无法原谅的事。我背叛了我最好的合伙人，独吞了我们一起发现的财宝。」\n\n「从那以后，我成了森林中最成功的商人...但每一个夜晚，我都被噩梦惊醒。」",
                    npcDialogue = "「我背叛了托林。我把他骗进了黑雾深处，然后独自带着所有财宝离开。他再也没有回来。」",
                    locationId = "trading_post", requiredDay = 8,
                    state = StorylineStepState.Locked,
                    completionHint = "倾听沃斯坦白他的罪行"
                },
                new StorylineStep
                {
                    index = 1, title = "地精工坊的线索",
                    narrative = "「托林...他可能还活着。」沃斯递给你一张泛黄的地图，「这是我们当年发现的地方——地精工坊。那里有我们藏起来的财宝，也有...托林的踪迹。」\n\n「我派人去找过，但工坊被地精们占据了。它们不会让外人轻易进入。」\n\n「如果你能找到托林...或者找到他的下落，请告诉我。我愿意用我所有的资产换取...一个赎罪的机会。」",
                    npcDialogue = "",
                    locationId = "trading_post", requiredDay = 10,
                    state = StorylineStepState.Locked,
                    completionHint = "获得地图并探索地精工坊"
                },
                new StorylineStep
                {
                    index = 2, title = "工坊深处的真相",
                    narrative = "地精工坊的深处，你发现了一间被遗弃的小屋。\n\n屋内有生活过的痕迹，角落里堆满了研究笔记。在一张桌子上，你发现了一封信：\n\n「沃斯：\n\n我知道是你害了我。但我不需要复仇。\n\n我在这里找到了比财宝更重要的东西——地精族群的信任。它们接纳了我，教我它们的智慧。\n\n我的腿已经无法行走，但我生活得很好。如果你还有良心，就用你的财富做些好事吧。\n\n不要再寻找我了。\n\n托林」\n\n信纸上还画着一个微笑的小人，旁边写着：「我原谅你了。」",
                    npcDialogue = "",
                    locationId = "goblin_workshop", requiredDay = 15,
                    state = StorylineStepState.Locked,
                    completionHint = "在地精工坊找到托林的下落"
                },
                new StorylineStep
                {
                    index = 3, title = "原谅与新生",
                    narrative = "沃斯读完信后，泪流满面。\n\n「他原谅我了...他早就原谅我了...」\n\n他颤抖着把地图和账本递给你：「这些...是我所有的资产。店铺、商品、还有那张我们当年发现的金矿地图。」\n\n「帮我把它们分给需要的人。用我的财富建造孤儿院、帮助穷人、修复村庄...做一切我应该做却没做的事。」\n\n他露出如释重负的笑容：「谢谢你，让我能够重新抬头做人。」\n\n—— 沃斯获得「诚实商人」称号\n—— 沃斯的商店解锁特价商品（每日随机7折）\n—— 获得成就「救赎之路」",
                    npcDialogue = "「诚实商人」称号获得者：沃斯\n「从今以后，我的商店不再赚取黑心钱。每一枚铜币，都要来得光明正大。」",
                    locationId = "trading_post",
                    state = StorylineStepState.Locked,
                    completionHint = "沃斯获得新生"
                }
            }
        });

        // ==================== 区域探索故事线（3条）====================

        // 6. 瀑布的秘密（4步）
        AddStoryline(new Storyline
        {
            id = "story_waterfall_secret",
            name = "瀑布的秘密",
            shortName = "历史学家",
            type = StorylineType.RegionExploration,
            difficulty = 3,
            prerequisite = "发现瀑布后洞穴隐藏区域",
            triggerNPCs = new List<string> { "martha" },
            triggerAreas = new List<string> { "waterfall_cave" },
            titleReward = "历史学家",
            memoryReward = 12,
            soulReward = 6,
            steps = new List<StorylineStep>
            {
                new StorylineStep
                {
                    index = 0, title = "瀑布后的洞穴",
                    narrative = "穿过轰鸣的水幕，你发现了一个隐藏在瀑布背后的巨大洞穴。\n\n洞口长满了发光的地衣，散发着幽幽的蓝绿色光芒。洞壁上刻满了古老的壁画——描绘着人类与黑雾战斗的场景。\n\n壁画的风格古老而神秘，似乎是某种失落的文明所留下。\n\n在洞穴入口处，你发现了一块石碑，上面刻着：「此地记录真相，过往之人请勿遗忘。」",
                    npcDialogue = "",
                    locationId = "waterfall_cave", requiredDay = 5,
                    state = StorylineStepState.Locked,
                    completionHint = "探索瀑布后洞穴的入口区域"
                },
                new StorylineStep
                {
                    index = 1, title = "古代文明遗迹",
                    narrative = "深入洞穴，你发现了一座埋藏在尘土下的古代遗迹。\n\n巨大的石柱上雕刻着精美的花纹，柱子上镶嵌的宝石依然闪烁着光芒。一座半倒塌的神殿静静地矗立在洞穴深处。\n\n神殿中央有一座石像——是一个身披斗篷、手持法杖的人形。神像的底座上刻着：\n\n「第一代封印者：艾琳娜·星河\n她用生命封印了黑雾，却无法将其彻底消灭。」\n\n旁边还有一行小字：「愿后来者能找到真正的解决方法。」",
                    npcDialogue = "",
                    locationId = "waterfall_cave", requiredDay = 10,
                    state = StorylineStepState.Locked,
                    completionHint = "在洞穴深处发现古代遗迹"
                },
                new StorylineStep
                {
                    index = 2, title = "黑雾的起源",
                    narrative = "在神殿深处，你找到了一本保存完好的石书——用某种特殊技术刻在石板上的记录。\n\n「艾琳娜历987年：黑雾突然出现。它从地底裂缝中涌出，吞噬一切生命。我们称之为'深渊之息'。」\n\n「艾琳娜历1002年：封印成功。黑雾被暂时压制，但深渊裂缝依然存在。艾琳娜牺牲了自己的生命化为封印的核心。」\n\n「警告：封印并非永恒。每一百年，封印需要加固一次。否则后果不堪设想...」\n\n原来，黑雾的源头是一个被称为「深渊裂缝」的地方！",
                    npcDialogue = "",
                    locationId = "waterfall_cave", requiredDay = 15,
                    state = StorylineStepState.Locked,
                    completionHint = "解读石书了解黑雾起源"
                },
                new StorylineStep
                {
                    index = 3, title = "历史的回响",
                    narrative = "你把发现带回了营地，所有人都震惊了。\n\n「所以黑雾不是天灾...是地底的怪物？」玛莎难以置信地说。\n\n「而且每一百年都要加固封印...上一次加固是什么时候？」莉莉翻阅着笔记。\n\n灰烬的声音从黑暗中传来：「很久以前了。久到几乎没人记得这件事...也许这就是为什么黑雾越来越严重。」\n\n—— 解锁成就「历史学家」\n—— 获得记忆碎片+12，魂精华+6\n—— 了解黑雾真正起源：深渊裂缝",
                    npcDialogue = "",
                    locationId = "camp",
                    state = StorylineStepState.Locked,
                    completionHint = "分享你的发现给所有人"
                }
            }
        });

        // 7. 石碑林之谜（3步）
        AddStoryline(new Storyline
        {
            id = "story_stone_forest_mystery",
            name = "石碑林之谜",
            shortName = "碑文解读师",
            type = StorylineType.RegionExploration,
            difficulty = 3,
            prerequisite = "收集20个以上Lore",
            triggerNPCs = new List<string> { "ashes" },
            triggerAreas = new List<string> { "stone_monument" },
            titleReward = "碑文解读师",
            memoryReward = 10,
            soulReward = 5,
            steps = new List<StorylineStep>
            {
                new StorylineStep
                {
                    index = 0, title = "二十块石碑",
                    narrative = "在森林的深处，有一片被称为「石碑林」的地方。\n\n二十块巨大的石碑整齐地排列成一个圆环，每块石碑都有三米多高，刻满了密密麻麻的文字。\n\n这些石碑使用的是一种古老的文字，与你在其他地方看到的任何文字都不同。但奇怪的是...有些符号似乎在发光，似乎在等待着什么。\n\n第一块石碑上写着：「沉睡于此者，为森林之灵。破坏者将承受永恒的诅咒。」",
                    npcDialogue = "",
                    locationId = "stone_monument", requiredDay = 10,
                    state = StorylineStepState.Locked,
                    completionHint = "前往石碑林，尝试理解石碑的排列"
                },
                new StorylineStep
                {
                    index = 1, title = "解读古代文字",
                    narrative = "你拿出在禁忌图书馆找到的符号对照表，开始尝试解读石碑上的文字。\n\n随着你的解读，一段惊人的历史逐渐浮现：\n\n「第一碑：森林本是两界交汇之地。白昼属于生灵，黑夜属于虚空。」\n「第七碑：森林之心是平衡的关键。它连接着两个世界。」\n「第十三碑：当黑雾开始蔓延，守护者们设置了二十道封印。」\n「第二十碑：集齐所有记忆碎片者，将获得揭开最后真相的资格。」\n\n原来，石碑林是一个警告系统，诉说着森林的过去和未来！",
                    npcDialogue = "",
                    locationId = "stone_monument", requiredDay = 15,
                    state = StorylineStepState.Locked,
                    completionHint = "使用符号对照表解读石碑上的文字"
                },
                new StorylineStep
                {
                    index = 2, title = "森林是封印",
                    narrative = "最后，当你触碰最后一块石碑时，石碑上的文字突然全部亮起！\n\n一个声音在你脑海中响起：\n\n「森林，即封印。」\n「整片森林本身，就是镇压黑雾的巨大型封印！」\n「树木是封印的符文，土地是封印的媒介，而你们...这些在森林中生活的生命，都是封印的一部分。」\n\n「当森林被砍伐、生灵被驱逐，封印就会减弱。这就是为什么黑雾越来越强大...」\n\n—— 你发现了森林的真正秘密：整片森林是一个活着的封印！\n—— 解锁成就「碑文解读师」\n—— 石碑林成为特殊研究区域，可在此提升「考古学」技能",
                    npcDialogue = "",
                    locationId = "stone_monument",
                    state = StorylineStepState.Locked,
                    completionHint = "见证石碑林的终极启示"
                }
            }
        });

        // 8. 时间裂隙（3步）
        AddStoryline(new Storyline
        {
            id = "story_time_rift",
            name = "时间裂隙",
            shortName = "时空旅人",
            type = StorylineType.RegionExploration,
            difficulty = 5,
            prerequisite = "二周目玩家",
            triggerNPCs = new List<string> { "ashes" },
            triggerAreas = new List<string> { "time_rift" },
            titleReward = "时空旅人",
            memoryReward = 25,
            soulReward = 20,
            steps = new List<StorylineStep>
            {
                new StorylineStep
                {
                    index = 0, title = "回到过去",
                    narrative = "二周目的你发现了一处奇异的空间——时间裂隙。\n\n这是一道悬浮在虚空中的裂痕，边缘闪烁着金色的光芒。它就像是一道被撕开的时空伤口。\n\n「这是时间裂隙。」灰烬的声音在身后响起，「只有在特殊的条件下...比如完成了一段完整的循环，才会出现。」\n\n「你可以选择进去。」他警告道，「但记住，改变过去不一定会带来更好的未来。每一个选择都有代价。」",
                    npcDialogue = "「时间是一条河流，你可以跃入支流，但主流永远不会改变。」",
                    locationId = "time_rift", requiredDay = 1,
                    requiresNGPlus = true,
                    state = StorylineStepState.Locked,
                    completionHint = "进入时间裂隙"
                },
                new StorylineStep
                {
                    index = 1, title = "改变历史",
                    narrative = "你纵身跃入裂隙，天旋地转间，你发现自己站在了黑雾尚未笼罩的森林中。\n\n阳光从树叶间洒落，鸟儿在歌唱——这是一片没有被黑雾侵蚀的森林。\n\n你看到了熟悉的身影：年轻的玛莎正带着一个小女孩采蘑菇，那是小萤；年轻的莉莉父母正在记录着什么；年轻的埃里克和他的战友阿城在巡逻...;\n\n你有一次机会改变历史。你会怎么做？",
                    npcDialogue = "",
                    playerChoicePrompt = "你的选择将影响结局：",
                    choices = new List<StoryChoice>
                    {
                        new StoryChoice { id = "save_xiaoying", text = "警告玛莎不要让小萤进入黑雾",
                            resultNarrative = "你冲上前警告了玛莎。她惊恐地抱紧了小萤，发誓再也不让她靠近黑雾一步。\n\n多年后，小萤长大成人，成为了森林中著名的草药医师。而玛莎，终于能看着女儿幸福地生活。\n\n但当你回到现在，你发现世界已经发生了改变——有些东西消失了，有些东西...变得更加美好了。",
                            memoryReward = 15, soulReward = 10, titleGranted = "命运改写者" },
                        new StoryChoice { id = "stop_experiment", text = "警告莉莉的父母停止研究",
                            resultNarrative = "你找到了莉莉的父母，告诉他们即将发生的悲剧。\n\n他们沉默了很久，然后烧毁了所有研究笔记：「为了莉莉...我们不再追寻这些了。」\n\n黑雾依然存在，但没有被加速扩散。世界保持了原有的轨迹——只是莉莉，再也无法找到父母的秘密了。",
                            memoryReward = 10, soulReward = 5 },
                        new StoryChoice { id = "let_history", text = "不改变任何事，让历史自然发生",
                            resultNarrative = "你静静地看完了那段历史，没有做出任何改变。\n\n回到现在后，灰烬对你点了点头：「明智的选择。有些事情，即使痛苦，也是必须的。」\n\n世界没有任何改变——但你获得了内心的平静。",
                            memoryReward = 8, soulReward = 8 }
                    },
                    locationId = "time_rift", requiredDay = 1,
                    requiresNGPlus = true,
                    state = StorylineStepState.Locked,
                    completionHint = "做出你的选择"
                },
                new StorylineStep
                {
                    index = 2, title = "回到现在",
                    narrative = "金色的光芒再次包裹了你，当你睁开眼睛时，已经回到了二周目的世界。\n\n时间裂隙在你身后缓缓闭合，但它的能量似乎在你身上留下了某种印记。\n\n「你改变了过去...或者说，你创造了一个新的分支。」灰烬的声音在耳边响起，「这不是结束，而是新的开始。」\n\n—— 解锁成就「时空旅人」\n—— 你的选择永久改变了这个世界的一部分\n—— 时间裂隙故事线完成",
                    npcDialogue = "",
                    locationId = "time_rift",
                    state = StorylineStepState.Locked,
                    completionHint = "时间裂隙故事线完成"
                }
            }
        });

        // ==================== 黑雾真相故事线（4条）====================

        // 9. 黑雾之源（5步）
        AddStoryline(new Storyline
        {
            id = "story_darkfog_origin",
            name = "黑雾之源",
            shortName = "深渊之眼",
            type = StorylineType.DarkFogTruth,
            difficulty = 5,
            prerequisite = "完成「瀑布的秘密」和「石碑林之谜」",
            triggerNPCs = new List<string> { "ashes", "lily" },
            triggerAreas = new List<string> { "dark_vortex" },
            titleReward = "深渊之眼",
            memoryReward = 20,
            soulReward = 15,
            steps = new List<StorylineStep>
            {
                new StorylineStep
                {
                    index = 0, title = "调查黑雾",
                    narrative = "结合你在瀑布洞穴和石碑林发现的线索，你开始系统性地调查黑雾的起源。\n\n莉莉的父母笔记中提到「深渊裂缝」；石碑林的碑文揭示森林是封印；而瀑布遗迹中的石书更是直接指向了一个被称为「深渊」的地方。\n\n「所有的线索都指向同一个地方。」莉莉指着地图，「森林的最深处，黑雾漩涡的核心。」",
                    npcDialogue = "「如果深渊裂缝是黑雾的源头，那我们需要亲眼去看看。」——莉莉",
                    locationId = "camp", requiredDay = 20,
                    state = StorylineStepState.Locked,
                    completionHint = "整理所有已知线索"
                },
                new StorylineStep
                {
                    index = 1, title = "发现是人祸",
                    narrative = "在黑雾漩涡的最深处，你发现了一块巨大的记事碑——上面记载的不是自然现象，而是一段被遗忘的历史。\n\n「深渊本是沉睡的存在。百年前，人类的贪婪唤醒了她。」\n\n「当时的国王试图汲取深渊的力量来获得永生。他们进行了禁忌的实验，打开了连接深渊的通道。」\n\n「但他们失败了。深渊的力量不是人类能够控制的。黑雾只是深渊愤怒的副产品。」\n\n原来，黑雾不是天灾，而是人祸！",
                    npcDialogue = "",
                    locationId = "dark_vortex", requiredDay = 22,
                    state = StorylineStepState.Locked,
                    completionHint = "在深渊入口发现记录碑"
                },
                new StorylineStep
                {
                    index = 2, title = "找到罪魁祸首",
                    narrative = "碑文的最后一段记载了实验主持者的名字：「首席法师卡尔索斯」。\n\n「他用自己的身体堵住了通道，但他没有死。他在深渊的力量中获得了永生...以怪物的形态。」\n\n「每年的黑雾爆发，都是他在试图挣脱封印。」\n\n而在碑文旁边，有一幅褪色的画像——那是一张熟悉的脸。「这...这是我们的国王！」",
                    npcDialogue = "",
                    locationId = "dark_vortex", requiredDay = 25,
                    state = StorylineStepState.Locked,
                    completionHint = "找到真正的罪魁祸首"
                },
                new StorylineStep
                {
                    index = 3, title = "最终抉择",
                    narrative = "你面前出现了三条道路：\n\n第一条：彻底消灭卡尔索斯，让深渊永眠。但这需要付出巨大的代价。\n\n第二条：利用深渊的力量。你可以利用卡尔索斯的力量反过来对抗黑雾，但这是一把双刃剑。\n\n第三条：与深渊共存。找到一种方法，让人类和深渊和谐相处，而不是相互毁灭。",
                    playerChoicePrompt = "这是改变一切的选择：",
                    choices = new List<StoryChoice>
                    {
                        new StoryChoice { id = "destroy", text = "彻底消灭黑雾和深渊",
                            resultNarrative = "你选择了一条最危险但也许最彻底的道路。你开始准备终极封印仪式。\n\n（此选择将通向「完全驱散」结局路线）",
                            memoryReward = 10, soulReward = 10 },
                        new StoryChoice { id = "use_power", text = "利用深渊的力量",
                            resultNarrative = "你决定利用卡尔索斯的力量。但这需要签订某种契约...你愿意承受未知的代价吗？\n\n（此选择将通向「黑暗支配者」结局路线）",
                            memoryReward = 15, soulReward = 20, titleGranted = "深渊契约者" },
                        new StoryChoice { id = "coexist", text = "寻找共存之道",
                            resultNarrative = "你相信人类和深渊可以和平共处。你开始研究如何安抚这个古老的存在。\n\n（此选择将通向「平衡之道」结局路线）",
                            memoryReward = 12, soulReward = 8 }
                    },
                    locationId = "dark_vortex", requiredDay = 28,
                    state = StorylineStepState.Locked,
                    completionHint = "做出你的最终抉择"
                },
                new StorylineStep
                {
                    index = 4, title = "深渊之眼",
                    narrative = "无论你做出了怎样的选择，你都已经不再是那个懵懂的新手冒险者了。\n\n你亲眼见证了深渊的恐怖，了解了黑雾的真相，也做出了属于自己的选择。\n\n这段经历将成为你最深刻的记忆，永远铭刻在灵魂之中。\n\n—— 故事线「黑雾之源」完成\n—— 获得成就「深渊之眼」\n—— 根据你的选择，世界将走向不同的未来",
                    npcDialogue = "",
                    locationId = "dark_vortex",
                    state = StorylineStepState.Locked,
                    completionHint = "黑雾之源故事线完成"
                }
            }
        });

        // 10. 封印仪式（4步）
        AddStoryline(new Storyline
        {
            id = "story_sealing_ritual",
            name = "封印仪式",
            shortName = "仪式执行者",
            type = StorylineType.DarkFogTruth,
            difficulty = 4,
            prerequisite = "收集15个魂精华，找到森林之心",
            triggerNPCs = new List<string> { "ashes", "lily" },
            triggerAreas = new List<string> { "forest_heart" },
            titleReward = "仪式执行者",
            memoryReward = 15,
            soulReward = 10,
            steps = new List<StorylineStep>
            {
                new StorylineStep
                {
                    index = 0, title = "收集材料",
                    narrative = "莉莉翻开了她父母笔记中记载的封印仪式方法。\n\n「仪式需要三种关键材料：」她解释道，\n\n「第一，森林之心——森林最核心处的神秘能量源。」\n「第二，守护者之证——证明你有资格执行仪式。」\n「第三，献祭者的生命——这是最残酷的部分...」\n\n她抬起头看向你：「材料清单中提到，守护者可以选择替代方案。」",
                    npcDialogue = "「父母他们...他们就是用自己的生命完成了第一次封印。」——莉莉",
                    locationId = "camp", requiredDay = 15,
                    state = StorylineStepState.Locked,
                    completionHint = "了解封印仪式所需的材料"
                },
                new StorylineStep
                {
                    index = 1, title = "重启封印",
                    narrative = "带着收集齐全的材料，你来到了森林之心——一颗巨大的、发光的古树。\n\n古树的树干上刻满了符文，与你在石碑林看到的完全一致。\n\n「就是这里了。」灰烬的声音响起，他站在你身后，「准备好了吗？」\n\n你深吸一口气，开始按照笔记中的步骤布置仪式阵法。符文一个接一个亮起，整个空间开始震动。",
                    npcDialogue = "",
                    locationId = "forest_heart", requiredDay = 20,
                    state = StorylineStepState.Locked,
                    completionHint = "在森林之心布置封印仪式阵法"
                },
                new StorylineStep
                {
                    index = 2, title = "选择仪式主角",
                    narrative = "就在仪式即将启动的关键时刻，阵法中央出现了两个光影。\n\n一个是你自己，另一个是灰烬。\n\n「封印仪式需要一个引导者。」灰烬的声音平静，「这个人的生命将成为封印的一部分。」\n\n「我有守护者之证...」你说。\n\n「但你还有未来。」灰烬微笑，「我已经活得太久了。是时候让我完成使命了。」\n\n或者，你可以选择自己成为引导者。",
                    playerChoicePrompt = "谁将成为封印的核心？",
                    choices = new List<StoryChoice>
                    {
                        new StoryChoice { id = "ashes_lead", text = "让灰烬成为引导者",
                            resultNarrative = "灰烬点了点头，他走向阵法中央。\n\n「谢谢你...让我有机会像我的伙伴们一样光荣地离开。」\n\n他最后看了你一眼，然后闭上了眼睛。金色的光芒吞没了他，也吞没了整个阵法。\n\n—— 灰烬将生命献给了封印，黑雾将暂时平息",
                            memoryReward = 20, soulReward = 15 },
                        new StoryChoice { id = "player_lead", text = "自己成为引导者",
                            resultNarrative = "「不。」你走向阵法中央，「我不能让别人为我牺牲。」\n\n灰烬想要阻止你，但你已经踏入了阵法。\n\n金色的光芒包围了你——然后，一切陷入黑暗。\n\n—— 你将成为新的封印核心...",
                            memoryReward = 25, soulReward = 20, titleGranted = "封印之魂" }
                    },
                    locationId = "forest_heart", requiredDay = 22,
                    state = StorylineStepState.Locked,
                    completionHint = "选择仪式的引导者"
                },
                new StorylineStep
                {
                    index = 3, title = "仪式的终结",
                    narrative = "仪式完成后，森林陷入了短暂的寂静。\n\n黑雾在消退，阳光穿透了云层洒落在森林中。\n\n但所有人都知道，这不是结束——只是新的开始。封印仍然需要维护，而你们...将成为下一代的守护者。\n\n—— 故事线「封印仪式」完成\n—— 解锁成就「仪式执行者」\n—— 黑雾进入休眠期，森林暂时恢复和平",
                    npcDialogue = "",
                    locationId = "forest_heart",
                    state = StorylineStepState.Locked,
                    completionHint = "见证封印仪式的完成"
                }
            }
        });

        // 11. 守护者传承（3步）
        AddStoryline(new Storyline
        {
            id = "story_guardian_succession",
            name = "守护者传承",
            shortName = "新一代守护者",
            type = StorylineType.DarkFogTruth,
            difficulty = 5,
            prerequisite = "获得守护者之证，完成封印仪式",
            triggerNPCs = new List<string> { "ashes" },
            triggerAreas = new List<string> { "forest_heart" },
            titleReward = "新一代守护者",
            memoryReward = 20,
            soulReward = 15,
            steps = new List<StorylineStep>
            {
                new StorylineStep
                {
                    index = 0, title = "接受试炼",
                    narrative = "守护者之证在你手中发出温暖的光芒，灰烬点了点头。\n\n「要成为真正的守护者，你必须通过试炼。」他指向森林之心，「进去吧。森林会考验你的。」\n\n你走进森林之心，眼前的景象突然变化——你发现自己站在一片虚无之中，周围是无尽的黑暗。\n\n一个声音响起：「你是谁？你为什么想要守护这片森林？」",
                    npcDialogue = "",
                    locationId = "forest_heart", requiredDay = 20,
                    state = StorylineStepState.Locked,
                    completionHint = "进入森林之心的试炼空间"
                },
                new StorylineStep
                {
                    index = 1, title = "选择牺牲或替代",
                    narrative = "你回答了森林之心的问题——你为什么想要守护。\n\n黑暗中出现了一幅画面：你看到自己年迈的样子，正把守护者之证交给下一个人。\n\n「成为守护者，意味着你将永远与这片森林绑定。」声音再次响起，「你将失去普通人的生活，失去与朋友共度的时光，失去...很多东西。」\n\n「你愿意吗？」",
                    playerChoicePrompt = "你的答案将决定你的命运：",
                    choices = new List<StoryChoice>
                    {
                        new StoryChoice { id = "accept_sacrifice", text = "我愿意接受一切牺牲",
                            resultNarrative = "你毫不犹豫地回答：「我愿意。」\n\n光芒包围了你，你感受到了森林的力量涌入体内。\n\n—— 你成为了新一代守护者，你将用自己的生命守护这片土地，直到下一位继承者出现。",
                            memoryReward = 25, soulReward = 20, titleGranted = "森林之心" },
                        new StoryChoice { id = "find_alternative", text = "寻找不牺牲的守护方法",
                            resultNarrative = "「一定还有别的办法。」你说。\n\n森林沉默了很久，然后叹息：「很久没有人问过这个问题了...也许真的有。」\n\n—— 你拒绝了传统的守护者道路，开始寻找新的可能。\n—— 但这意味着你需要承担更大的风险...。",
                            memoryReward = 15, soulReward = 10 }
                    },
                    locationId = "forest_heart", requiredDay = 25,
                    state = StorylineStepState.Locked,
                    completionHint = "回答森林之心的问题"
                },
                new StorylineStep
                {
                    index = 2, title = "成为新守护者",
                    narrative = "无论你做出了怎样的选择，你都已经完成了守护者的试炼。\n\n森林在你身上留下了印记——这是守护者的证明。\n\n你感受到了森林的心跳，感受到了每一棵树、每一株草的生命。你理解了灰烬为什么愿意用一生守护这片土地。\n\n「从今以后，」灰烬的声音在耳边响起，「你就是新的守护者了。」\n\n—— 故事线「守护者传承」完成\n—— 解锁成就「新新一代守护者」\n—— 获得特殊能力：森林之声（可与森林沟通）",
                    npcDialogue = "",
                    locationId = "forest_heart",
                    state = StorylineStepState.Locked,
                    completionHint = "完成守护者试炼"
                }
            }
        });

        // 12. 黑雾与森林（3步）
        AddStoryline(new Storyline
        {
            id = "story_fog_and_forest",
            name = "黑雾与森林",
            shortName = "平衡者",
            type = StorylineType.DarkFogTruth,
            difficulty = 4,
            prerequisite = "完成石碑林之谜，了解森林封印真相",
            triggerNPCs = new List<string> { "ashes" },
            triggerAreas = new List<string> { "stone_monument", "dark_vortex" },
            titleReward = "平衡者",
            memoryReward = 15,
            soulReward = 10,
            steps = new List<StorylineStep>
            {
                new StorylineStep
                {
                    index = 0, title = "发现本是一体",
                    narrative = "在解读石碑林的过程中，你发现了一个惊人的真相。\n\n石碑上记载：「白昼之子与黑夜之子，本是双生。」\n\n「森林与黑雾，并非对立。它们是一体两面，如同阴阳，如同生死。」\n\n「当一方过于强大，另一方就会衰弱。而当两者完全失衡，世界就会毁灭。」\n\n原来，黑雾和森林从来就不是敌人——它们是这片土地的两张面孔！",
                    npcDialogue = "「这就是为什么单纯的消灭黑雾并不可行...」——灰烬若有所思",
                    locationId = "stone_monument", requiredDay = 15,
                    state = StorylineStepState.Locked,
                    completionHint = "理解黑雾与森林的真正关系"
                },
                new StorylineStep
                {
                    index = 1, title = "平衡 or 消灭",
                    narrative = "石碑的最后一页记载了一个预言：\n\n「当黑雾之子与森林之子再次相遇，新的时代将开启。」\n「他们可以选择让一方永远消失，带来短暂但不平衡的和平。」\n「或者，他们可以携手，重新找回失落的平衡，让两个世界共存。」\n\n莉莉和灰烬都看向了你——这个选择的重担，落在了你的肩上。",
                    playerChoicePrompt = "你选择哪条道路？",
                    choices = new List<StoryChoice>
                    {
                        new StoryChoice { id = "destroy_fog", text = "消灭黑雾，让森林独存",
                            resultNarrative = "你选择了一条看似美好的道路。但当你尝试消灭黑雾时，却发现...森林开始枯萎。\n\n没有黑雾，森林失去了它存在的另一半意义。\n\n—— 警告：这条路可能导致森林毁灭结局",
                            memoryReward = 10, soulReward = 5 },
                        new StoryChoice { id = "balance", text = "寻求两者平衡",
                            resultNarrative = "你选择了更艰难的道路——让黑雾和森林共存。\n\n这需要建立一个新的封印系统，一个能够同时容纳两者的秩序。\n\n—— 这条路漫长而艰难，但可能是唯一正确的选择",
                            memoryReward = 20, soulReward = 15, titleGranted = "平衡之道" }
                    },
                    locationId = "dark_vortex", requiredDay = 20,
                    state = StorylineStepState.Locked,
                    completionHint = "做出关于黑雾与森林的决定"
                },
                new StorylineStep
                {
                    index = 2, title = "新的秩序",
                    narrative = "无论你做出了怎样的选择，这个世界都已经被你永远地改变了。\n\n黑雾和森林的关系，不再是秘密。你亲手揭开了这个被遗忘的真相。\n\n这段旅程让你明白了一个道理：世界上很少有绝对的黑与白、对与错。真正的智慧，在于找到平衡。\n\n—— 故事线「黑雾与森林」完成\n—— 解锁成就「平衡者」\n—— 获得新能力：感知之眼（可看穿事物本质）",
                    npcDialogue = "",
                    locationId = "forest_heart",
                    state = StorylineStepState.Locked,
                    completionHint = "见证平衡的重建"
                }
            }
        });

        // ==================== 隐藏结局故事线（3条）====================

        // 13. 真正的自由（4步）
        AddStoryline(new Storyline
        {
            id = "story_true_freedom",
            name = "真正的自由",
            shortName = "觉醒者",
            type = StorylineType.HiddenEnding,
            difficulty = 5,
            prerequisite = "二周目 + 收集全部Lore和记忆碎片",
            triggerNPCs = new List<string> { "ashes" },
            triggerAreas = new List<string> { "mirror_pond", "time_rift" },
            titleReward = "觉醒者",
            memoryReward = 30,
            soulReward = 25,
            requiresAllEndings = false,
            steps = new List<StorylineStep>
            {
                new StorylineStep
                {
                    index = 0, title = "森林只是投影",
                    narrative = "二周目的你，在镜面池塘边发现了一个惊人的现象。\n\n当你把守护者之证放入池塘时，水面荡漾出一幅画面——那是你从未见过的景象。\n\n一片没有森林、没有黑雾的世界。城市、街道、人群...那是你曾经生活的世界吗？\n\n「森林只是投影。」一个声音在脑海中响起，「你真正的身体，还在另一个地方沉睡着。」\n\n记忆碎片开始涌现——你想起了现实世界，想起了你为什么会来到这里...",
                    npcDialogue = "「这是一场梦...或者说，是一个被创造出来的世界。」——守护者之证的低语",
                    locationId = "mirror_pond", requiredDay = 1,
                    requiresNGPlus = true,
                    state = StorylineStepState.Locked,
                    completionHint = "在镜面池塘见证真相"
                },
                new StorylineStep
                {
                    index = 1, title = "选择离开",
                    narrative = "守护者之证在你手中震动：「你有两个选择。」\n\n「留在这里，成为森林的守护者，永远生活在梦中。」\n\n「或者，回到现实，忘记这里的一切——但你的记忆将以另一种形式陪伴你。」\n\n「你想离开吗？」",
                    playerChoicePrompt = "这是关于你命运的选择：",
                    choices = new List<StoryChoice>
                    {
                        new StoryChoice { id = "leave", text = "选择离开，回到现实",
                            resultNarrative = "你选择了离开。\n\n当你醒来时，你发现自己躺在一张床上。周围是熟悉的房间，窗外是熟悉的城市。\n\n一切都像是一场梦。但你的口袋里，有一块发光的小石头——那是守护者之证的碎片。\n\n你永远不会忘记那片森林，那些朋友，那段冒险。因为它们已经成为了你的一部分。",
                            memoryReward = 30, soulReward = 20, titleGranted = "觉醒者" },
                        new StoryChoice { id = "stay", text = "留在森林，继续守护",
                            resultNarrative = "你选择了留下。\n\n这个世界虽然虚幻，但这里的友情、这里的战斗、这里的一切...对你来说都是真实的。\n\n你愿意用一生去守护它。\n\n—— 你成为了永恒的森林守护者",
                            memoryReward = 20, soulReward = 25, titleGranted = "永恒守护者" }
                    },
                    locationId = "mirror_pond", requiredDay = 5,
                    requiresNGPlus = true,
                    state = StorylineStepState.Locked,
                    completionHint = "做出关于离开还是留下的选择"
                },
                new StorylineStep
                {
                    index = 2, title = "分别",
                    narrative = "无论你做出了怎样的选择，离别总是伤感的。\n\n如果选择离开：\n玛莎给了你一个拥抱：「孩子，无论你去哪里，请记住，回忆不是枷锁，而是力量。」\n莉莉递给你一封信：「这是我父母研究的精华部分。希望对你有用。」\n埃里克默默递上他的佩剑：「拿着它，就当是我陪着你。」\n\n如果选择留下：\n所有人围在篝火旁，为你举行了一个简单但温馨的仪式。「从今以后，你就是我们的一份子了。」",
                    npcDialogue = "「无论你做出怎样的选择，我们都支持你。」——玛莎",
                    locationId = "camp", requiredDay = 10,
                    state = StorylineStepState.Locked,
                    completionHint = "与你所爱的人告别"
                },
                new StorylineStep
                {
                    index = 3, title = "真正的结局",
                    narrative = "无论你选择了哪条道路，你的冒险都画上了一个圆满的句号。\n\n也许这不是完美的结局，但这是属于你的结局。\n\n你经历了战斗，经历了离别，经历了成长。你帮助了很多人，也被很多人帮助。\n\n这片森林会记得你。无论你身在何处，这份记忆都会永远伴随着你。\n\n—— 故事线「真正的自由」完成\n—— 解锁隐藏结局「觉醒之路」\n—— 感谢你的游玩！",
                    npcDialogue = "",
                    locationId = "camp",
                    state = StorylineStepState.Locked,
                    completionHint = "见证属于你的结局"
                }
            }
        });

        // 14. 新秩序（4步）
        AddStoryline(new Storyline
        {
            id = "story_new_order",
            name = "新秩序",
            shortName = "森林之主",
            type = StorylineType.HiddenEnding,
            difficulty = 5,
            prerequisite = "击败森林守护者（二周目限定）",
            triggerNPCs = new List<string> { "ashes" },
            triggerAreas = new List<string> { "final_trial" },
            titleReward = "森林之主",
            memoryReward = 30,
            soulReward = 30,
            steps = new List<StorylineStep>
            {
                new StorylineStep
                {
                    index = 0, title = "挑战守护者",
                    narrative = "二周目的你，来到了森林的最深处——最终试炼场。\n\n灰烬站在你对面，他的眼神中没有了往日的温和，取而代之的是冷酷。\n\n「要建立新秩序，就必须打破旧秩序。」他的声音冰冷，「你准备好了吗？」\n\n巨大的黑雾在他身后翻涌，他的身影开始变得高大起来——那是你从未见过的形态。\n\n战斗，开始了。",
                    npcDialogue = "",
                    locationId = "final_trial", requiredDay = 1,
                    requiresNGPlus = true,
                    state = StorylineStepState.Locked,
                    completionHint = "与森林守护者最终对决"
                },
                new StorylineStep
                {
                    index = 1, title = "接管森林",
                    narrative = "经过激烈的战斗，你终于击败了灰烬。\n\n他跪倒在地，身上的黑雾逐渐消散。「你赢了...」他露出苦笑，「也许这就是我一直等待的结局。」\n\n他抬起手，将一块散发着黑暗光芒的晶石递给你：「拿着它。从现在起，你就是森林的新主人。」\n\n你接过晶石，一股强大的力量涌入体内。你感受到了整个森林——每一棵树，每一滴水，每一个生命，都在向你臣服。",
                    npcDialogue = "「不要像我一样被责任束缚...创造你自己的秩序吧。」——灰烬",
                    locationId = "final_trial", requiredDay = 3,
                    state = StorylineStepState.Locked,
                    completionHint = "击败森林守护者"
                },
                new StorylineStep
                {
                    index = 2, title = "建立新规则",
                    narrative = "你成为了森林的新主人。现在，你需要决定如何管理这片土地。\n\n你可以建立新的秩序——一个完全不同于过去的规则。",
                    playerChoicePrompt = "你将如何统治森林？",
                    choices = new List<StoryChoice>
                    {
                        new StoryChoice { id = "benevolent", text = "仁慈统治，与所有人共享森林",
                            resultNarrative = "你选择了与所有生灵共享森林的智慧。\n\n人类、精灵、兽人...所有种族都可以在这片土地上和平共处。\n\n—— 通向「理想国」结局",
                            memoryReward = 20, soulReward = 20, titleGranted = "仁慈之主" },
                        new StoryChoice { id = "strict", text = "严格统治，森林规则由你制定",
                            resultNarrative = "你选择了铁腕统治。任何破坏森林平衡的存在都将被清除。\n\n秩序得到了维护，但自由...少了一些。\n\n—— 通向「铁秩序」结局",
                            memoryReward = 15, soulReward = 25, titleGranted = "铁腕之主" },
                        new StoryChoice { id = "release", text = "释放森林，让它自由发展",
                            resultNarrative = "你选择放弃控制权。森林将拥有自己的意志，不再受任何人的统治。\n\n—— 通向「自然之道」结局",
                            memoryReward = 25, soulReward = 15, titleGranted = "自由之手" }
                    },
                    locationId = "final_trial", requiredDay = 5,
                    state = StorylineStepState.Locked,
                    completionHint = "做出关于统治方式的选择"
                },
                new StorylineStep
                {
                    index = 3, title = "新世界的开始",
                    narrative = "无论你选择了怎样的道路，新的时代已经开始了。\n\n森林记住了你的名字，你的选择，你的理想。\n\n也许在未来，会有人质疑你的决定，会有人试图推翻你的秩序。但这是你的选择，你从不后悔。\n\n「森林之主」的称号将永远伴随着你。\n\n—— 故事线「新秩序」完成\n—— 解锁隐藏结局「新世界」\n—— 你创造了一个全新的森林秩序",
                    npcDialogue = "",
                    locationId = "forest_heart",
                    state = StorylineStepState.Locked,
                    completionHint = "见证新秩序的建立"
                }
            }
        });

        // 15. 永恒轮回（3步）
        AddStoryline(new Storyline
        {
            id = "story_eternal_cycle",
            name = "永恒轮回",
            shortName = "轮回终结者",
            type = StorylineType.HiddenEnding,
            difficulty = 5,
            prerequisite = "完成全部14条故事线 + 全部结局",
            triggerNPCs = new List<string> { "ashes", "lily", "martha", "erik" },
            triggerAreas = new List<string> { "stone_monument", "time_rift" },
            titleReward = "轮回终结者",
            memoryReward = 50,
            soulReward = 50,
            steps = new List<StorylineStep>
            {
                new StorylineStep
                {
                    index = 0, title = "完成全结局后",
                    narrative = "你完成了所有的故事线，见证了所有的结局。\n\n森林之心在你面前绽放耀眼光芒，一个声音响起：\n\n「轮回者啊，你已经走过了无数次循环。你见证了每一种可能，经历了每一种命运。」\n\n「现在，是时候做出最终的选择了——继续轮回，还是...终结一切？」\n\n石碑林的所有石碑同时亮起，二十道光芒汇聚在你身上。",
                    npcDialogue = "「你终于来了。」——所有石碑同时发出声音",
                    locationId = "stone_monument", requiredDay = 1,
                    requiresAllEndings = true,
                    state = StorylineStepState.Locked,
                    completionHint = "触发永恒轮回的条件"
                },
                new StorylineStep
                {
                    index = 1, title = "打破轮回",
                    narrative = "时间裂隙再次开启，但这一次，它不是通往过去，而是通往...开始。\n\n「在所有时间线的交汇处，存在着一个原点。」声音继续说道，「回到那里，你就可以重新开始——真正的开始。」\n\n「或者，你可以选择让轮回永远继续。你将成为新的轮回守护者，编织新的故事。」",
                    playerChoicePrompt = "最终选择：",
                    choices = new List<StoryChoice>
                    {
                        new StoryChoice { id = "break_cycle", text = "打破轮回，让一切终结",
                            resultNarrative = "你选择了打破轮回。\n\n时间裂隙开始崩塌，所有的时间线开始收束。你看到了无数次循环中的自己——那些成功、那些失败、那些欢笑、那些泪水。\n\n然后，一切归于寂静。\n\n当你再次醒来，你发现自己躺在一片草地上。阳光温暖，鸟儿歌唱。没有黑雾，没有森林，只有...一个新的世界。\n\n—— 通向「真正结局」",
                            memoryReward = 50, soulReward = 50, titleGranted = "轮回终结者" },
                        new StoryChoice { id = "continue_cycle", text = "成为轮回守护者",
                            resultNarrative = "你选择了继续轮回。\n\n作为轮回守护者，你将见证无限的可能性，无数的故事。\n\n但你知道，这个选择意味着责任——永远编织故事，永远看着它们开始和结束。\n\n—— 通向「永恒守护者」结局",
                            memoryReward = 40, soulReward = 40, titleGranted = "永恒编织者" }
                    },
                    locationId = "time_rift", requiredDay = 1,
                    requiresAllEndings = true,
                    state = StorylineStepState.Locked,
                    completionHint = "做出最终选择"
                },
                new StorylineStep
                {
                    index = 2, title = "真正的结局",
                    narrative = "无论你做出了怎样的选择，你的旅程都已经圆满结束了。\n\n你不再是被命运摆布的棋子，而是书写自己故事的作者。\n\n这片森林、这些角色、这段旅程——它们都将成为你最珍贵的宝藏。\n\n感谢你陪伴《森林手账》走过这段旅程。\n\n无论你身在何处，愿记忆的碎片永远照耀你的道路。\n\n—— 故事线「永恒轮回」完成\n—— 解锁终极成就「真正的结局」\n—— 《森林手账》主线故事全部完成！\n\n★★★★★★★★★★★★★★★\n制作：森林手账开发团队\n感谢游玩！\n★★★★★★★★★★★★★★★",
                    npcDialogue = "",
                    locationId = "stone_monument",
                    state = StorylineStepState.Locked,
                    completionHint = "见证真正的结局"
                }
            }
        });
    }

    // ==================== 故事线注册 ====================

    private void AddStoryline(Storyline storyline)
    {
        allStorylines.Add(storyline);
        storylineDict[storyline.id] = storyline;
    }

    // ==================== 事件处理 ====================

    private void HandleNPCDialogue(string npcId, string dialogueId)
    {
        foreach (var storyline in allStorylines)
        {
            if (!storyline.isUnlocked) continue;
            if (!storyline.triggerNPCs.Contains(npcId)) continue;

            var step = storyline.CurrentStep;
            if (step != null && step.state == StorylineStepState.Active
                && string.IsNullOrEmpty(step.locationId) || step.locationId == "camp")
            {
                TryAdvanceStep(storyline.id);
            }
        }
    }

    private void HandleLocationExplored(string locationId, string action)
    {
        foreach (var storyline in allStorylines)
        {
            if (!storyline.isUnlocked) continue;

            var step = storyline.CurrentStep;
            if (step == null || step.state != StorylineStepState.Active) continue;

            bool locationMatch = string.IsNullOrEmpty(step.locationId)
                || step.locationId == locationId
                || (step.completionHint != null && step.completionHint.Contains(locationId));

            if (locationMatch)
            {
                TryAdvanceStep(storyline.id);
            }
        }
    }

    private void HandleItemUsed(string itemId, string locationId)
    {
        foreach (var storyline in allStorylines)
        {
            if (!storyline.isUnlocked) continue;

            var step = storyline.CurrentStep;
            if (step == null || step.state != StorylineStepState.Active) continue;

            if (!
                string.IsNullOrEmpty(step.requiredItem) && step.requiredItem == itemId)
            {
                TryAdvanceStep(storyline.id);
            }
        }
    }

    private void HandleDayChanged(int newDay)
    {
        foreach (var storyline in allStorylines)
        {
            if (!storyline.isUnlocked) continue;

            var step = storyline.CurrentStep;
            if (step == null || step.state != StorylineStepState.Active) continue;

            if (step.requiredDay > 0 && newDay >= step.requiredDay)
            {
                TryAdvanceStep(storyline.id);
            }
        }
    }

    // ==================== 公开API ====================

    /// <summary>
    /// 尝试推进故事线到下一步
    /// </summary>
    public void TryAdvanceStep(string storylineId)
    {
        if (!storylineDict.ContainsKey(storylineId)) return;

        var storyline = storylineDict[storylineId];
        var step = storyline.CurrentStep;
        if (step == null) return;

        // 检查前置条件
        if (step.requiresNGPlus && (GameManager.instance == null || !GameManager.instance.isNewGamePlus)) return;
        if (step.requiredDay > 0 && (GameManager.instance == null || GameManager.instance.currentDay < step.requiredDay)) return;

        // 完成当前步骤
        step.state = StorylineStepState.Completed;
        storyline.currentStepIndex++;

        // 给予奖励
        if (GameManager.instance != null)
        {
            GameManager.instance.AddMemory(step.index >= 0 ? storyline.memoryReward / storyline.steps.Count : 0);
        }

        // 触发事件
        OnStepCompleted?.Invoke(storyline, step);

        // 检查故事线是否完成
        if (storyline.currentStepIndex >= storyline.steps.Count)
        {
            CompleteStoryline(storylineId);
            return;
        }

        // 激活下一步
        var nextStep = storyline.CurrentStep;
        if (nextStep != null)
        {
            nextStep.state = StorylineStepState.Active;
            OnStepActivated?.Invoke(storyline, nextStep);
        }
    }

    /// <summary>
    /// 完成故事线
    /// </summary>
    public void CompleteStoryline(string storylineId)
    {
        if (!storylineDict.ContainsKey(storylineId)) return;

        var storyline = storylineDict[storylineId];
        storyline.isCompleted = true;

        if (!completedIds.Contains(storylineId))
            completedIds.Add(storylineId);

        // 给予完成奖励
        if (GameManager.instance != null)
        {
            GameManager.instance.AddMemory(storyline.memoryReward);
            for (int i = 0; i < storyline.soulReward; i++)
                GameManager.instance.AddSoulEssence(1);
        }

        // 授予称号
        if (!string.IsNullOrEmpty(storyline.titleReward) && !acquiredTitles.Contains(storyline.titleReward))
        {
            acquiredTitles.Add(storyline.titleReward);
            OnTitleGranted?.Invoke(storylineId, storyline.titleReward);

            // 触发Steam成就（如果集成）
            TriggerAchievement($"storyline_complete_{storyline.id}");
        }

        Debug.Log($"[故事线完成] {storyline.name} | 称号: {storyline.titleReward}");
        OnStorylineCompleted?.Invoke(storyline);
    }

    /// <summary>
    /// 解锁故事线
    /// </summary>
    public void UnlockStoryline(string storylineId)
    {
        if (!storylineDict.ContainsKey(storylineId)) return;

        var storyline = storylineDict[storylineId];
        if (storyline.isUnlocked) return;

        storyline.isUnlocked = true;
        storyline.currentStepIndex = 0;

        if (!unlockedIds.Contains(storylineId))
            unlockedIds.Add(storylineId);

        if (storyline.steps.Count > 0)
            storyline.steps[0].state = StorylineStepState.Active;

        Debug.Log($"[故事线解锁] {storyline.name}");
        OnStorylineUnlocked?.Invoke(storyline);
    }

    /// <summary>
    /// 获取当前激活的故事线步骤文本（用于UI显示）
    /// </summary>
    public string GetCurrentStepNarrative(string storylineId)
    {
        if (!storylineDict.ContainsKey(storylineId)) return null;

        var storyline = storylineDict[storylineId];
        var step = storyline.CurrentStep;
        return step?.narrative;
    }

    /// <summary>
    /// 获取故事线当前提示
    /// </summary>
    public string GetCurrentStepHint(string storylineId)
    {
        if (!storylineDict.ContainsKey(storylineId)) return null;

        var storyline = storylineDict[storylineId];
        var step = storyline.CurrentStep;
        return step?.completionHint;
    }

    /// <summary>
    /// 检查是否可以开始某个故事线
    /// </summary>
    public bool CanStartStoryline(string storylineId)
    {
        if (!storylineDict.ContainsKey(storylineId)) return false;

        var storyline = storylineDict[storylineId];
        if (storyline.isUnlocked || storyline.isCompleted) return false;

        // 检查前置条件
        if (storyline.type == StorylineType.HiddenEnding && storyline.requiresAllEndings)
        {
            int completed = completedIds.Count;
            return completed >= 14;
        }

        if (storyline.type == StorylineType.HiddenEnding && storyline.requiresNGPlus)
        {
            return GameManager.instance != null && GameManager.instance.isNewGamePlus;
        }

        return true;
    }

    /// <summary>
    /// 获取所有可开始的故事线
    /// </summary>
    public List<Storyline> GetAvailableStorylines()
    {
        var result = new List<Storyline>();
        foreach (var s in allStorylines)
        {
            if (CanStartStoryline(s.id))
                result.Add(s);
        }
        return result;
    }

    /// <summary>
    /// 获取指定故事线
    /// </summary>
    public Storyline GetStoryline(string id)
    {
        return storylineDict.ContainsKey(id) ? storylineDict[id] : null;
    }

    /// <summary>
    /// 获取故事线进度描述
    /// </summary>
    public string GetProgressText(string storylineId)
    {
        if (!storylineDict.ContainsKey(storylineId)) return "";
        var s = storylineDict[storylineId];
        return $"[{s.currentStepIndex}/{s.steps.Count}] {s.name}";
    }

    /// <summary>
    /// 触发成就（通过Steam或自定义）
    /// </summary>
    private void TriggerAchievement(string achievementId)
    {
        var steam = FindObjectOfType<SteamAchievementSystem>();
        steam?.UnlockAchievement(achievementId);
    }

    // ==================== 存档集成 ====================

    public StorylineSaveData GetSaveData()
    {
        var data = new StorylineSaveData
        {
            unlockedStorylineIds = new List<string>(unlockedIds),
            completedStorylineIds = new List<string>(completedIds),
            stepProgress = new Dictionary<string, int>(stepProgress),
            choiceHistory = new Dictionary<string, List<string>>(choiceHistory),
            grantedTitles = new List<string>(acquiredTitles)
        };
        return data;
    }

    public void LoadSaveData(StorylineSaveData data)
    {
        if (data == null) return;

        unlockedIds = data.unlockedStorylineIds ?? new List<string>();
        completedIds = data.completedStorylineIds ?? new List<string>();
        stepProgress = data.stepProgress ?? new Dictionary<string, int>();
        choiceHistory = data.choiceHistory ?? new Dictionary<string, List<string>>();
        acquiredTitles = data.grantedTitles ?? new List<string>();

        // 恢复故事线状态
        foreach (var storyline in allStorylines)
        {
            storyline.isUnlocked = unlockedIds.Contains(storyline.id);
            storyline.isCompleted = completedIds.Contains(storyline.id);

            if (stepProgress.ContainsKey(storyline.id))
                storyline.currentStepIndex = stepProgress[storyline.id];
            else
                storyline.currentStepIndex = 0;

            // 恢复步骤状态
            for (int i = 0; i < storyline.steps.Count; i++)
            {
                storyline.steps[i].state = i < storyline.currentStepIndex
                    ? StorylineStepState.Completed
                    : (i == storyline.currentStepIndex && storyline.isUnlocked
                        ? StorylineStepState.Active
                        : StorylineStepState.Locked);
            }
        }
    }

    // ==================== UI辅助 ====================

    /// <summary>
    /// 获取故事线类型中文名
    /// </summary>
    public string GetStorylineTypeName(StorylineType type)
    {
        switch (type)
        {
            case StorylineType.NPCBackground: return "NPC背景";
            case StorylineType.RegionExploration: return "区域探索";
            case StorylineType.DarkFogTruth: return "黑雾真相";
            case StorylineType.HistoricalMystery: return "历史谜团";
            case StorylineType.HiddenEnding: return "隐藏结局";
            default: return "未知";
        }
    }

    /// <summary>
    /// 获取故事线状态描述
    /// </summary>
    public string GetStorylineStatus(string storylineId)
    {
        if (!storylineDict.ContainsKey(storylineId)) return "未知";
        var s = storylineDict[storylineId];
        if (s.isCompleted) return "已完成";
        if (!s.isUnlocked) return "未解锁";
        return $"进行中 {s.currentStepIndex + 1}/{s.steps.Count}";
    }
}
