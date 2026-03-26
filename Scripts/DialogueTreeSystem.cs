using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 对话树系统 v1.0
/// 完整分支对话、玩家选择影响NPC态度和世界状态
/// </summary>
public class DialogueTreeSystem : MonoBehaviour
{
    public static DialogueTreeSystem instance { get; private set; }

    [Header("对话库")]
    public List<DialogueNode> allDialogues = new List<DialogueNode>();

    [Header("当前对话状态")]
    public DialogueNode currentNode;
    public string currentNpcId;
    public List<DialogueChoice> currentChoices;
    public List<string> dialogueHistory = new List<string>();
    public int dialogueDepth = 0;

    void Awake()
    {
        instance = this;
        InitializeDialogues();
    }

    // ====================
    // 初始化对话库
    // ====================

    void InitializeDialogues()
    {
        allDialogues = new List<DialogueNode>
        {
            // ========== 序章对话 ==========
            new DialogueNode {
                id = "prologue_1",
                speaker = "narrator",
                content = "你从迷雾中醒来...\n身边躺着另一个人——叶青。",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "轻轻推醒她", nextId = "prologue_wake_leaf" },
                    new DialogueChoice { text = "环顾四周", nextId = "prologue_look_around" },
                    new DialogueChoice { text = "假装还在睡", nextId = "prologue_fake_sleep" }
                }
            },
            new DialogueNode {
                id = "prologue_wake_leaf",
                speaker = "leaf",
                content = "「嗯...？你醒了。\n这片森林很奇怪，到处都是黑雾...」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你是谁？", nextId = "prologue_leaf_intro" },
                    new DialogueChoice { text = "发生了什么？", nextId = "prologue_ask_disaster" }
                }
            },
            new DialogueNode {
                id = "prologue_leaf_intro",
                speaker = "leaf",
                content = "「我叫叶青，是个草药商人。\n黑雾来之前，我住在森林边缘的村子...」\n「村子没了。我逃进森林，一直在找安全的地方。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我也是...我们一起走吧", nextId = "prologue_agree_teamup", trustChange = 5 },
                    new DialogueChoice { text = "你一个人没问题吗？", nextId = "prologue_concern_leaf" }
                }
            },
            new DialogueNode {
                id = "prologue_agree_teamup",
                speaker = "leaf",
                content = "「谢谢...有你一起，我会更安心一些。\n前面好像有个人影，要不要去看看？」",
                isEnding = true,
                choices = new List<DialogueChoice>()
            },
            new DialogueNode {
                id = "prologue_look_around",
                speaker = "narrator",
                content = "你观察四周——树木扭曲，天空被灰雾笼罩。\n不远处躺着一个人，身边散落着草药。",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "去叫醒那个人", nextId = "prologue_wake_leaf" }
                }
            },
            new DialogueNode {
                id = "prologue_fake_sleep",
                speaker = "narrator",
                content = "你闭着眼睛，听到那人的脚步声远去...\n等睁开眼时，那人已经不见了。",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "追上去", nextId = "prologue_chase_leaf" },
                    new DialogueChoice { text = "继续躺着", nextId = "prologue_stay_down" }
                }
            },
            new DialogueNode {
                id = "prologue_chase_leaf",
                speaker = "leaf",
                content = "「啊！你醒了！」\n「我以为你还在昏迷...前面就是迷雾森林深处了。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我们一起走吧", nextId = "prologue_agree_teamup" }
                }
            },
            new DialogueNode {
                id = "prologue_stay_down",
                speaker = "narrator",
                content = "你决定再休息一会儿...\n突然，一阵低沉的咆哮从不远处传来。",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "立刻跳起来！", nextId = "prologue_jump_up" }
                }
            },
            new DialogueNode {
                id = "prologue_jump_up",
                speaker = "narrator",
                content = "一只巨大的黑色野兽从雾中现身！\n叶青正拿着小刀挡在前面——但她手在抖。",
                isCombat = true,
                choices = new List<DialogueChoice>()
            },

            // ========== 玛莎对话 ==========
            new DialogueNode {
                id = "masha_greet",
                speaker = "masha",
                content = "「旅人...你们需要草药吗？」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你是谁？", nextId = "masha_intro" },
                    new DialogueChoice { text = "购买物品", nextId = "masha_trade" },
                    new DialogueChoice { text = "聊一聊", nextId = "masha_chat", minTrust = 10 }
                }
            },
            new DialogueNode {
                id = "masha_intro",
                speaker = "masha",
                content = "「我叫玛莎。以前在村里开草药铺的。\n黑雾来了以后...我只能四处漂泊。」\n「这里的草药比村子里的效果更好，但也更危险。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "为什么要帮我们？", nextId = "masha_why_help", minTrust = 20 },
                    new DialogueChoice { text = "再见", nextId = "END" }
                }
            },
            new DialogueNode {
                id = "masha_why_help",
                speaker = "masha",
                content = "「因为你们让我想起了以前的邻居...」\n「而且...」\n\n她压低声音：「我知道一些关于黑雾的事情。如果你们能帮我找到5个草药，我可以告诉你们一个秘密。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "成交", nextId = "masha_quest_accept", trustChange = 3, flag = "masha_quest" },
                    new DialogueChoice { text = "先说说是什么秘密", nextId = "masha_ask_secret", minTrust = 30 }
                }
            },
            new DialogueNode {
                id = "masha_ask_secret",
                speaker = "masha",
                content = "「森林深处有一个叫做'森林之心'的地方...」\n「传说那里是黑雾的源头，也是终结黑雾的关键。」\n「但要小心——那里有东西在守护着它。」",
                secret = "forest_heart_hint",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "谢谢你的信任", nextId = "END", trustChange = 5 }
                }
            },

            // ========== 玛莎扩展对话 ==========

            // 日常闲聊 (5)
            new DialogueNode {
                id = "masha_chat_weather",
                speaker = "masha",
                content = "「今天的雾好像淡了一点呢。」\n她轻轻嗅了嗅空气：「有股露水和薄荷的味道...」\n「这种天气，适合去采集晨露草。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你喜欢采集草药？", nextId = "masha_herb_love" },
                    new DialogueChoice { text = "晨露草有什么用？", nextId = "masha_dewgrass_info" },
                    new DialogueChoice { text = "先走了", nextId = "END" }
                }
            },
            new DialogueNode {
                id = "masha_herb_love",
                speaker = "masha",
                content = "「草药是大地的馈赠呀。」\n她的眼神变得柔和：「小时候，我奶奶教我认草药...」\n「每一株植物都有自己的故事，就像每一个人一样。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你奶奶...还在吗？", nextId = "masha_grandma", minTrust = 15 },
                    new DialogueChoice { text = "你懂得真多", nextId = "END", trustChange = 3 }
                }
            },
            new DialogueNode {
                id = "masha_grandma",
                speaker = "masha",
                content = "「...不在了。」\n她低下头：「黑雾来的第一天，就...」\n\n她很快整理好情绪：「抱歉，让你听到这些。我们聊点别的吧。」",
                trustChange = -2,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "对不起，不该问的", nextId = "masha_comfort_people", trustChange = 5 },
                    new DialogueChoice { text = "我理解你的感受", nextId = "masha_empathy", minTrust = 20 }
                }
            },
            new DialogueNode {
                id = "masha_dewgrass_info",
                speaker = "masha",
                content = "「晨露草啊，是炼制清醒药剂的材料。」\n「喝了之后，能暂时驱散黑雾对人精神的影响...」\n\n她皱起眉头：「最近黑雾越来越浓了，这种草药也越来越难找。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "黑雾会影响精神？", nextId = "masha_blackmist_mind", minTrust = 10 },
                    new DialogueChoice { text = "我帮你找吧", nextId = "masha_help_herb", trustChange = 5 },
                    new DialogueChoice { text = "先告辞了", nextId = "END" }
                }
            },
            new DialogueNode {
                id = "masha_blackmist_mind",
                speaker = "masha",
                content = "「黑雾不只是挡住视线那么简单...」\n她压低声音：「长期接触黑雾的人，会开始遗忘。起初是小事，然后...」\n\n她停顿了一下：「然后会忘记自己是谁。」",
                secret = "blackmist_memory",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "有这么严重？！", nextId = "masha_mind_shock" },
                    new DialogueChoice { text = "你怎么知道的？", nextId = "masha_mind_personal", minTrust = 20 }
                }
            },
            new DialogueNode {
                id = "masha_mind_personal",
                speaker = "masha",
                content = "「...我见过。」\n她的手指微微颤抖：「我丈夫。最后他看着我，问我是谁...」\n「那是他最后一次叫我的名字。」",
                trustChange = 8,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我很抱歉...", nextId = "END", trustChange = 3 }
                }
            },
            new DialogueNode {
                id = "masha_mind_shock",
                speaker = "masha",
                content = "「是的。所以我一直用草药保护自己。」\n她看着你：「你们也要小心。不要在黑雾里待太久。」\n「如果感到头晕或记忆模糊...立刻离开。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我会注意的", nextId = "END", trustChange = 2 }
                }
            },
            new DialogueNode {
                id = "masha_help_herb",
                speaker = "masha",
                content = "「真的吗？」\n她的眼睛亮了起来：「如果你能帮我找到5株晨露草，我可以给你特制的草药茶...」\n\n「还能告诉你更多关于这片森林的秘密。」",
                flag = "masha_herb_quest",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "成交", nextId = "END", trustChange = 3 },
                    new DialogueChoice { text = "让我想想", nextId = "END" }
                }
            },
            new DialogueNode {
                id = "masha_empathy",
                speaker = "masha",
                content = "「...谢谢。」\n她深吸一口气：「能有人愿意倾听，已经很难得了。」\n「在这片森林里，我们都失去过什么。但我们还活着...这就够了。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你很坚强", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "masha_comfort_people",
                speaker = "masha",
                content = "「没关系的，都是很久以前的事了。」\n她挤出一个微笑：「能帮助别人，对我来说也是一种治愈。」\n「如果你以后需要草药，尽管来找我。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你也保重", nextId = "END", trustChange = 3 }
                }
            },

            // 背景故事 (5)
            new DialogueNode {
                id = "masha_family_story",
                speaker = "masha",
                content = "「我以前有个幸福的家...」\n她看着远方：「丈夫、孩子、还有奶奶...我们村的草药铺是三代传承的。」\n「黑雾来之前，我觉得世界是温暖的。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "发生了什么？", nextId = "masha_family_lost", minTrust = 15 },
                    new DialogueChoice { text = "你一个人逃出来了？", nextId = "masha_escape_alone", minTrust = 10 }
                }
            },
            new DialogueNode {
                id = "masha_family_lost",
                speaker = "masha",
                content = "「那天早上，黑雾突然从森林深处涌出来...」\n她的声音颤抖：「奶奶叫我带孩子先走，说她腿脚不便...」\n「我以为她会跟上来。我以为...」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "孩子呢？", nextId = "masha_child_lost", minTrust = 25 },
                    new DialogueChoice { text = "不要太难过", nextId = "masha_comfort_people" }
                }
            },
            new DialogueNode {
                id = "masha_child_lost",
                speaker = "masha",
                content = "「...我的孩子也走了。」\n她闭上眼睛：「半路上遇到黑雾袭击，我护住了他...但他还是...」\n\n她沉默了很久：「我甚至没能好好安葬他们。」",
                trustChange = 10,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "如果你愿意，以后我就是你的家人", nextId = "masha_adopt", trustChange = 15 },
                    new DialogueChoice { text = "我会帮你找到他们", nextId = "masha_find_family", minTrust = 30 }
                }
            },
            new DialogueNode {
                id = "masha_escape_alone",
                speaker = "masha",
                content = "「是的...我最后还是一个人逃出来了。」\n她苦笑着：「我是个懦夫，对吧？抛下了奶奶...」\n「但我必须活下去，把草药的知识传承下去。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "活下去不是懦弱", nextId = "masha_not_coward", minTrust = 10 },
                    new DialogueChoice { text = "你有奶奶的故事吗？", nextId = "masha_grandma_story", minTrust = 15 }
                }
            },
            new DialogueNode {
                id = "masha_not_coward",
                speaker = "masha",
                content = "「...谢谢你这么说。」\n她擦了擦眼角：「有时候我会想，如果当初留下来...会不会不一样。」\n「但现在，我只想找到终结黑雾的方法。让更多人不用经历我的痛苦。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我们会一起终结它", nextId = "END", trustChange = 8 }
                }
            },
            new DialogueNode {
                id = "masha_grandma_story",
                speaker = "masha",
                content = "「奶奶是村里最后的草药师...」\n她的眼神变得柔和：「她说过，每种草药都有自己的灵魂。你必须尊重它们，它们才会帮助你。」\n「奶奶还说过，森林深处有一个地方...能治愈一切伤痛。」",
                secret = "forest_healing",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "是森林之心吗？", nextId = "masha_forest_heart_know", minTrust = 25 },
                    new DialogueChoice { text = "奶奶一定很爱你", nextId = "END", trustChange = 3 }
                }
            },
            new DialogueNode {
                id = "masha_forest_heart_know",
                speaker = "masha",
                content = "「你也知道森林之心？」\n她惊讶地看着你：「传说那里有纯净的生命之水，能治愈黑雾的污染...」\n\n「但也有人说，那只是一个永远不会实现的梦。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "不管真假，我想去看看", nextId = "END", trustChange = 5 },
                    new DialogueChoice { text = "我们一起去？", nextId = "masha_go_together", minTrust = 30 }
                }
            },
            new DialogueNode {
                id = "masha_go_together",
                speaker = "masha",
                content = "「...好。」\n她露出了难得的真挚笑容：「很久没有人对我说这种话了。」\n「如果你要去森林之心，我可以帮你准备一些草药防护剂。」",
                flag = "masha_companion",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "谢谢你，玛莎", nextId = "END", trustChange = 10 }
                }
            },
            new DialogueNode {
                id = "masha_adopt",
                speaker = "masha",
                content = "「...」\n她愣住了，眼泪流了下来：「你...你说什么？」\n\n她突然紧紧握住你的手：「如果你愿意...我可以把你当作自己的孩子。」\n「在这片冷漠的森林里，我们至少可以互相关爱。」",
                trustChange = 20,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "妈妈...我会照顾你的", nextId = "END", flag = "masha_family_bond", trustChange = 15 }
                }
            },
            new DialogueNode {
                id = "masha_find_family",
                speaker = "masha",
                content = "「真的吗？」\n她的眼中闪过一丝希望：「如果你真的能找到他们...请告诉他们，我很抱歉。」\n「还有...妈妈一直在等他们回家。」",
                flag = "masha_find_graves",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我答应你", nextId = "END", trustChange = 10 }
                }
            },

            // 任务委托 (3)
            new DialogueNode {
                id = "masha_quest_rare",
                speaker = "masha",
                content = "「我一直在研究如何对抗黑雾...」\n她拿出一个笔记本：「需要三种稀有草药，你能帮我收集吗？」\n「月见草、雾莲花、还有...黑根草。」",
                flag = "masha_rare_quest",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我试试看", nextId = "END", trustChange = 5 },
                    new DialogueChoice { text = "为什么需要这些？", nextId = "masha_quest_why", minTrust = 20 }
                }
            },
            new DialogueNode {
                id = "masha_quest_why",
                speaker = "masha",
                content = "「我想制作一种'纯净药剂'...」\n她解释道：「这种药剂或许能暂时抵御黑雾的侵蚀。」\n「如果成功了，就能争取更多时间...找到真正的解决办法。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "明白了，交给我", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "masha_quest_healer",
                speaker = "masha",
                content = "「最近森林里的受伤者越来越多了...」\n她忧心忡忡：「我能治疗他们，但草药不够用了。」\n「你能帮我去采集一些基础草药吗？只要5株就够了。」",
                flag = "masha_healer_quest",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "当然可以", nextId = "END", trustChange = 5 },
                    new DialogueChoice { text = "你想当医生？", nextId = "masha_be_healer", minTrust = 15 }
                }
            },
            new DialogueNode {
                id = "masha_be_healer",
                speaker = "masha",
                content = "「医生...不，我只是一个草药师。」\n她摇摇头：「但奶奶说过，能治愈伤痛的人，就是医生。」\n「看着他们好转，就是我最大的安慰了。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你很了不起", nextId = "END", trustChange = 5 }
                }
            },

            // 情感对话 (3)
            new DialogueNode {
                id = "masha_fear_night",
                speaker = "masha",
                content = "「到了晚上...我经常睡不着。」\n她看着渐暗的天空：「每当黑雾变浓，我就会想起那天晚上...」\n「到处都是尖叫声，什么都看不见。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "今晚我陪你", nextId = "masha_stay_night", trustChange = 10 },
                    new DialogueChoice { text = "黑雾会消散的", nextId = "masha_hope_future", minTrust = 10 }
                }
            },
            new DialogueNode {
                id = "masha_stay_night",
                speaker = "masha",
                content = "「...谢谢。」\n她露出了一个温暖的微笑：「有你在，我安心多了。」\n「去篝火边坐坐吧，我泡草药茶给你喝。」",
                flag = "masha_night_watch",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "好的，很期待", nextId = "END", trustChange = 8 }
                }
            },
            new DialogueNode {
                id = "masha_hope_future",
                speaker = "masha",
                content = "「...希望吧。」\n她轻轻叹息：「有时候我觉得自己像在黑暗中摸索，看不到光。」\n「但只要还有一丝希望，我就不会放弃。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "光一直都在", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "masha_trade_herbs",
                speaker = "masha",
                content = "「这是我的存货...」\n她打开一个布袋：「有治疗药草、解毒草、还有提神花。」\n「你们旅人肯定用得上。用食物或物资来换吧。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我看看有什么", nextId = "END" },
                    new DialogueChoice { text = "太贵了，算了吧", nextId = "END" }
                }
            },

            // 黑雾真相 (2)
            new DialogueNode {
                id = "masha_blackmist_truth",
                speaker = "masha",
                content = "「你问我关于黑雾的事...」\n她犹豫了一下，压低声音：「我有一些...不太确定的猜测。」\n「黑雾不是自然形成的。它是被人召唤来的。」",
                secret = "blackmist_artificial",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "什么意思？！", nextId = "masha_blackmist_who", minTrust = 25 },
                    new DialogueChoice { text = "谁会这么做？", nextId = "masha_blackmist_who", minTrust = 25 }
                }
            },
            new DialogueNode {
                id = "masha_blackmist_who",
                speaker = "masha",
                content = "「我不确定...但据说很久以前，有人试图封印森林之心的力量。」\n她皱起眉头：「封印失败后，能量失控，就变成了黑雾。」\n「所以黑雾实际上是...失控的封印之力。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "封印者是谁？", nextId = "masha_sealer_identity", minTrust = 35 },
                    new DialogueChoice { text = "有办法逆转吗？", nextId = "masha_reverse", minTrust = 30 }
                }
            },
            new DialogueNode {
                id = "masha_reverse",
                speaker = "masha",
                content = "「如果传说是真的...」\n她若有所思：「或许森林之心本身就蕴含着答案。」\n「纯净的生命之水，应该能净化失控的力量。」\n\n「但这只是我的猜测。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我会去森林之心看看", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "masha_sealer_identity",
                speaker = "masha",
                content = "「传说是一个古老的组织...被称为'守林人'。」\n她摇摇头：「他们为了保护森林，尝试封印黑暗力量。但失败了。」\n「也有说法认为，是有人故意破坏了封印...」",
                secret = "keepers_secret",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "守林人还存在吗？", nextId = "masha_keepers_exist", minTrust = 40 },
                    new DialogueChoice { text = "这个秘密很重要", nextId = "END", trustChange = 8 }
                }
            },
            new DialogueNode {
                id = "masha_keepers_exist",
                speaker = "masha",
                content = "「我不确定...但我在森林里遇到过一些人...」\n她犹豫地说：「他们穿着灰色的斗篷，似乎在监视着什么。」\n「也许，守林人并没有完全消失。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我在哪能找到他们？", nextId = "masha_keepers_location", minTrust = 45 },
                    new DialogueChoice { text = "我会小心的", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "masha_keepers_location",
                speaker = "masha",
                content = "「据说在森林的最深处...靠近黑雾核心的地方。」\n她摇摇头：「但我不建议贸然去那里。太危险了。」\n「或许你可以先问问其他旅行者...」",
                flag = "keepers_hint",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "谢谢你的情报", nextId = "END", trustChange = 10 }
                }
            },

            // 玩家选择影响 (2)
            new DialogueNode {
                id = "masha_trust_path",
                speaker = "masha",
                content = "「我观察你很久了...」\n她认真地看着你：「你是那种会为别人付出的人。」\n「在这片森林里，这样的人太少了。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "这是我应该做的", nextId = "END", trustChange = 10 },
                    new DialogueChoice { text = "你也一样", nextId = "masha_mutual_trust", trustChange = 15 }
                }
            },
            new DialogueNode {
                id = "masha_mutual_trust",
                speaker = "masha",
                content = "「...呵。」\n她笑了，是发自内心的笑：「谢谢你。」\n「以后不管发生什么，我都会站在你这边。」\n\n她轻声说：「我们是一家人了。」",
                flag = "masha_deep_bond",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "家人...", nextId = "END", trustChange = 15 }
                }
            },
            new DialogueNode {
                id = "masha_choice_herb_path",
                speaker = "masha",
                content = "「你愿意帮我找草药...」\n她的眼神柔和：「说明你愿意为了别人付出。这片森林需要更多这样的人。」\n「我会把所有的草药知识教给你。」",
                flag = "masha_herb_master",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我会好好学的", nextId = "END", trustChange = 8 }
                }
            },

            // ========== 莉莉对话 ==========
            new DialogueNode {
                id = "lily_greet",
                speaker = "lily",
                content = "「你们是好人吗？」\n一双大眼睛怯生生地看着你。",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我们是旅人", nextId = "lily_traveler" },
                    new DialogueChoice { text = "给你分点食物吧", nextId = "lily_share_food", trustChange = 10 },
                    new DialogueChoice { text = "无视她", nextId = "lily_ignore" }
                }
            },
            new DialogueNode {
                id = "lily_traveler",
                speaker = "lily",
                content = "「旅人？我也是哦！\n我在找爸爸妈妈...但他们好像走进黑雾里去了...」\n\n她低下头：「你...你能帮我找他们吗？」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我们会帮你的", nextId = "lily_promise_help", trustChange = 8 },
                    new DialogueChoice { text = "抱抱她", nextId = "lily_hug", trustChange = 15 },
                    new DialogueChoice { text = "无能为力", nextId = "lily_cant_help" }
                }
            },
            new DialogueNode {
                id = "lily_hug",
                speaker = "lily",
                content = "「...谢谢你。」\n她在你怀里哭了一会儿，然后抬起头：「我不哭了！我很坚强的！」\n「如果你帮我，我可以...我可以画画给你！我画画很厉害的！」",
                isEnding = true,
                choices = new List<DialogueChoice>()
            },
            new DialogueNode {
                id = "lily_ignore",
                speaker = "lily",
                content = "「...」\n她缩回角落，不再说话。",
                trustChange = -5,
                isEnding = true,
                choices = new List<DialogueChoice>()
            },

            // ========== 莉莉扩展对话 ==========

            // 日常闲聊 (5)
            new DialogueNode {
                id = "lily_chat_drawing",
                speaker = "lily",
                content = "「你看！我刚才画的！」\n她兴奋地拿出一张纸：「我把黑雾的样子画下来了！」\n\n纸上歪歪扭扭地画着一团黑色的东西，旁边写着密密麻麻的小字。",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你记录得真详细", nextId = "lily_record_proud" },
                    new DialogueChoice { text = "你在写什么？", nextId = "lily_notes_content" },
                    new DialogueChoice { text = "黑雾很可怕吧？", nextId = "lily_fear_admit" }
                }
            },
            new DialogueNode {
                id = "lily_record_proud",
                speaker = "lily",
                content = "「那当然！」\n她得意地仰起头：「我可是立志要当最厉害的记录者的人！」\n「所有重要的事情，我都要记下来！这样才能找到爸爸妈妈！」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "记录者？", nextId = "lily_recorder_dream" },
                    new DialogueChoice { text = "你真了不起", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "lily_recorder_dream",
                speaker = "lily",
                content = "「记录者就是记录所有重要事情的人呀！」\n她解释道：「有人必须记住历史，不然大家都会忘记！」\n「我的梦想是走遍世界，记录所有有趣的事！」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "那黑雾之后呢？", nextId = "lily_world_after" },
                    new DialogueChoice { text = "加油！", nextId = "END", trustChange = 3 }
                }
            },
            new DialogueNode {
                id = "lily_notes_content",
                speaker = "lily",
                content = "「我记下了黑雾出现的时间、地点、还有样子！」\n她翻着本子：「还有遇到的人、说过的话、闻到的味道...」\n\n「说不定里面藏着什么线索呢！」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "给我看看", nextId = "lily_show_notes", trustChange = 5 },
                    new DialogueChoice { text = "你想得真周全", nextId = "END", trustChange = 3 }
                }
            },
            new DialogueNode {
                id = "lily_show_notes",
                speaker = "lily",
                content = "「给！」\n她把本子递过来。上面密密麻麻写满了字，还画着各种符号。\n\n「这里记着黑雾来袭那天的日期...这里是我在森林里见过的所有植物...」\n\n「哦对了，这里还记着灰烬哥哥说的话！」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "灰烬说了什么？", nextId = "lily_ash_words" },
                    new DialogueChoice { text = "你见过灰烬？", nextId = "lily_meet_ash" }
                }
            },
            new DialogueNode {
                id = "lily_ash_words",
                speaker = "lily",
                content = "「他说...」\n她歪着头回忆：「'黑雾不是终点，而是开始'。」\n\n「我不太懂是什么意思，但是感觉很厉害的样子！」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "他说的也许是真的", nextId = "lily_ash_wisdom", minTrust = 20 },
                    new DialogueChoice { text = "一个小孩子说的话？", nextId = "lily_kid_thought" }
                }
            },
            new DialogueNode {
                id = "lily_ash_wisdom",
                speaker = "lily",
                content = "「对吧对吧！」\n她兴奋地拍手：「我也觉得灰烬哥哥很厉害！」\n「他总是说一些奇奇怪怪的话，但是每次回头看，都发现他说对了！」\n\n「他肯定知道很多秘密！」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "他在哪里？", nextId = "lily_ash_where", minTrust = 15 },
                    new DialogueChoice { text = "有机会介绍给我认识", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "lily_ash_where",
                speaker = "lily",
                content = "「他经常在森林里到处走...」\n她指向远处：「上次我在北边的老树那里遇到他。」\n\n「不过他很难捉摸，一转眼就不见了！」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我会去找他的", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "lily_meet_ash",
                speaker = "lily",
                content = "「嗯！灰烬哥哥是个很奇怪的人...」\n她想了想：「他总是突然出现，又突然消失。」\n\n「但是他给过我一块很漂亮的黑石头，说是黑雾里找到的！」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "黑石头？能给我看看吗？", nextId = "lily_show_stone" },
                    new DialogueChoice { text = "他为什么给你东西？", nextId = "lily_ash_gift_reason" }
                }
            },
            new DialogueNode {
                id = "lily_show_stone",
                speaker = "lily",
                content = "「在这里！」\n她小心翼翼地拿出一块漆黑的石头：「你看，它是不是很亮？」\n\n石头表面光滑，但隐隐透出一丝微光，像是有什么东西在里面流动。",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "这石头有古怪", nextId = "lily_stone_warning", minTrust = 20 },
                    new DialogueChoice { text = "要小心保存", nextId = "END" }
                }
            },
            new DialogueNode {
                id = "lily_stone_warning",
                speaker = "lily",
                content = "「真的吗？」\n她有点紧张地把石头收起来：「灰烬哥哥说...这是'雾之心'。」\n\n「还说等我准备好了，就会告诉我它是什么意思...」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "他知道的事情太多了", nextId = "END", trustChange = 3 }
                }
            },
            new DialogueNode {
                id = "lily_world_after",
                speaker = "lily",
                content = "「之后...」\n她的眼神暗了一下：「之后我要去更远的地方！」\n「把世界上所有美好的事物都记录下来...这样就算以后发生坏事，也能记住美好的时光！」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你比看起来成熟呢", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "lily_kid_thought",
                speaker = "lily",
                content = "「哼！你不要小看我！」\n她叉起腰：「小孩子也可以很厉害的！」\n\n「而且灰烬哥哥不是普通的小孩子啦！」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "好好，你说的对", nextId = "END", trustChange = 2 }
                }
            },
            new DialogueNode {
                id = "lily_ash_gift_reason",
                speaker = "lily",
                content = "「不知道...」\n她摇摇头：「他就是突然给我了。」\n\n「也许...他觉得我需要它？」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "也许吧", nextId = "END" }
                }
            },
            new DialogueNode {
                id = "lily_fear_admit",
                speaker = "lily",
                content = "「...嗯。」\n她低下头：「第一次看到黑雾的时候，我吓坏了。」\n\n「到处都是黑漆漆的，什么都看不见...我紧紧拉着爸爸妈妈的手...」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "后来呢？", nextId = "lily_parents_lost", minTrust = 15 },
                    new DialogueChoice { text = "你现在很勇敢", nextId = "lily_brave_now" }
                }
            },
            new DialogueNode {
                id = "lily_brave_now",
                speaker = "lily",
                content = "「嘿嘿...其实我也会害怕的。」\n她小声说：「但是害怕也要继续往前走呀。」\n\n「而且我现在有朋友了！有你们陪我，我就不那么怕了！」",
                trustChange = 8,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我们也会保护你的", nextId = "END", trustChange = 5 }
                }
            },

            // 背景故事 (5)
            new DialogueNode {
                id = "lily_parents_story",
                speaker = "lily",
                content = "「我爸爸是学者，妈妈是画家...」\n她的眼神充满怀念：「我们家有很多很多书和画。」\n\n「黑雾来之前，我们一家经常去森林里写生...那时候的森林好美...」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "他们现在在哪里？", nextId = "lily_parents_where" },
                    new DialogueChoice { text = "你一定很想他们", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "lily_parents_where",
                speaker = "lily",
                content = "「那天晚上...黑雾突然就把我们冲散了。」\n她攥紧拳头：「我听到爸爸在喊我名字，但是声音越来越远...」\n\n「后来我就一个人了...一直在找他们。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我们会一起找的", nextId = "lily_search_together", trustChange = 10 },
                    new DialogueChoice { text = "你有没有什么线索？", nextId = "lily_clue", minTrust = 15 }
                }
            },
            new DialogueNode {
                id = "lily_parents_lost",
                speaker = "lily",
                content = "「后来...就看不见他们了。」\n她的声音有些哽咽：「我到处喊，到处找...」\n\n「但是我只找到了这个...」\n\n她拿出一块撕碎的布角，上面有妈妈亲手绣的花纹。",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "这是你妈妈的？", nextId = "lily_cloth_memory" },
                    new DialogueChoice { text = "抱抱她", nextId = "lily_hug", trustChange = 10 }
                }
            },
            new DialogueNode {
                id = "lily_cloth_memory",
                speaker = "lily",
                content = "「嗯...这是妈妈教我绣的第一朵花。」\n她轻轻抚摸着布角：「妈妈说，只要不忘记，花就永远不会凋谢...」\n\n「所以我要记录一切。这样大家就不会被遗忘。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你妈妈很伟大", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "lily_clue",
                speaker = "lily",
                content = "「我记在本子里了！」\n她翻着本子：「爸爸妈妈最后出现的地方是东边的湖边。」\n\n「还有...他们说要去找一个叫'回声谷'的地方...」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "回声谷是什么地方？", nextId = "lily_echo_valley", minTrust = 20 },
                    new DialogueChoice { text = "东边的湖...我帮你留意", nextId = "END", trustChange = 8 }
                }
            },
            new DialogueNode {
                id = "lily_echo_valley",
                speaker = "lily",
                content = "「我也不知道...」\n她摇摇头：「爸爸说那里有很多古代的遗迹...」\n\n「也许那里藏着黑雾的秘密？爸爸妈妈可能是去调查了！」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我会帮你去那里看看", nextId = "lily_search_together", trustChange = 10 },
                    new DialogueChoice { text = "要小心那个地方", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "lily_search_together",
                speaker = "lily",
                content = "「真的吗？！」\n她开心地跳起来：「太好了！有你们帮忙，一定能找到的！」\n\n「我把这些线索都记在本子里了，你们一定要来看！」",
                flag = "lily_quest_echo_valley",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "好，我会的", nextId = "END", trustChange = 10 }
                }
            },

            // 任务委托 (3)
            new DialogueNode {
                id = "lily_quest_research",
                speaker = "lily",
                content = "「我在调查黑雾的规律...」\n她拿出本子：「我发现黑雾有时候会变淡，好像有周期一样！」\n\n「你能帮我记录一下你那边看到的情况吗？」",
                flag = "lily_blackmist_research",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "当然可以", nextId = "END", trustChange = 5 },
                    new DialogueChoice { text = "为什么觉得有周期？", nextId = "lily_cycle_explain", minTrust = 15 }
                }
            },
            new DialogueNode {
                id = "lily_cycle_explain",
                speaker = "lily",
                content = "「因为我一直在观察呀！」\n她翻着本子：「你看，这里记着每次黑雾变淡的时间...」\n\n「好像每隔几天就会有一次...而且总是和月亮的位置有关！」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "这个发现很重要！", nextId = "END", trustChange = 8 }
                }
            },
            new DialogueNode {
                id = "lily_quest_draw_map",
                speaker = "lily",
                content = "「我想画一张完整的森林地图！」\n她充满干劲地说：「但是有些地方太危险了，我一个人去不了...」\n\n「你能带我去一些新地方吗？」",
                flag = "lily_map_quest",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "好啊，我带你去", nextId = "END", trustChange = 8 },
                    new DialogueChoice { text = "太危险了，不行", nextId = "lily_disappointed", trustChange = -5 }
                }
            },
            new DialogueNode {
                id = "lily_disappointed",
                speaker = "lily",
                content = "「哦...」\n她有点失落：「好吧，我理解的...」\n\n「但是以后如果有机会，一定要带我去哦！」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "一言为定", nextId = "END" }
                }
            },

            // 情感对话 (3)
            new DialogueNode {
                id = "lily_lonely_night",
                speaker = "lily",
                content = "「晚上...我总是会梦到爸爸妈妈。」\n她轻声说：「梦里我们还在森林里散步，很开心...」\n\n「但是醒来之后，就只剩下我一个人了。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "今晚我陪你", nextId = "lily_stay_tonight", trustChange = 10 },
                    new DialogueChoice { text = "你还有我们", nextId = "lily_friends", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "lily_stay_tonight",
                speaker = "lily",
                content = "「真的吗？太好了！」\n她的眼睛亮了起来：「那你给我讲故事吧！」\n\n「我最喜欢听故事了！特别是关于森林的！」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "从前有一片魔法森林...", nextId = "END", trustChange = 8 }
                }
            },
            new DialogueNode {
                id = "lily_friends",
                speaker = "lily",
                content = "「嗯！」\n她用力点头：「有你们在我就不怕了！」\n\n「我们是同伴嘛！同伴就要一直在一起！」",
                trustChange = 8,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "说得好！", nextId = "END" }
                }
            },
            new DialogueNode {
                id = "lily_question_trust",
                speaker = "lily",
                content = "「那个...我可以问你一个问题吗？」\n她有些犹豫：「你为什么愿意帮我呀？」\n\n「我们才刚认识...你没有理由对我这么好...」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "因为你是好孩子", nextId = "END", trustChange = 5 },
                    new DialogueChoice { text = "因为我也失去过重要的人", nextId = "lily_shared_pain", trustChange = 10 }
                }
            },
            new DialogueNode {
                id = "lily_shared_pain",
                speaker = "lily",
                content = "「...真的吗？」\n她惊讶地看着你，然后露出理解的表情：「难怪我感觉...我们是同类人。」\n\n「谢谢你愿意告诉我。」",
                trustChange = 15,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我们一起加油", nextId = "END", trustChange = 8 }
                }
            },

            // 黑雾真相 (2)
            new DialogueNode {
                id = "lily_blackmist_theory",
                speaker = "lily",
                content = "「我一直在研究黑雾...」\n她压低声音：「我发现黑雾不是随机的！它好像在往某个方向移动...」\n\n「而且每次黑雾过后，植物的颜色都会变淡一点点...」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "往哪个方向？", nextId = "lily_mist_direction" },
                    new DialogueChoice { text = "你是说...黑雾在吞噬生命？", nextId = "lily_absorb_life", minTrust = 25 }
                }
            },
            new DialogueNode {
                id = "lily_mist_direction",
                speaker = "lily",
                content = "「好像是...往森林中心的方向！」\n她翻着本子指给你看：「你看，每次我记录的位置变化...」\n\n「所有的黑雾都在向一个地方聚集！」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "是森林之心吗？", nextId = "lily_forest_center", minTrust = 30 },
                    new DialogueChoice { text = "这很重要，继续观察", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "lily_absorb_life",
                speaker = "lily",
                content = "「...是的。」\n她的声音有些发抖：「我在黑雾退去的土地上发现...所有的植物都枯萎了。」\n\n「就像有什么东西...把它们的力量吸走了。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "这是很重大的发现", nextId = "END", trustChange = 8 }
                }
            },
            new DialogueNode {
                id = "lily_forest_center",
                speaker = "lily",
                content = "「森林之心！」\n她的眼睛发亮：「传说那里是所有生命的源头...」\n\n「如果黑雾在向那里聚集...会不会是森林之心出了问题？」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "有道理，继续说", nextId = "lily_theory_cont", minTrust = 20 },
                    new DialogueChoice { text = "你很聪明！", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "lily_theory_cont",
                speaker = "lily",
                content = "「我想...黑雾可能是森林之心的'伤口'。」\n她越说越激动：「就像人生病会发烧一样，森林之心可能也在试图保护自己...」\n\n「但是力量失控了，所以变成了黑雾！」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "这理论很有意思", nextId = "END", trustChange = 8 }
                }
            },
            new DialogueNode {
                id = "lily_blackmist_record",
                speaker = "lily",
                content = "「我把所有关于黑雾的记录都整理好了...」\n她递过来一叠厚厚的纸：「这是我目前知道的全部了。」\n\n「希望这些能帮到你们...」",
                flag = "lily_blackmist_notes",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "这很珍贵，谢谢你", nextId = "END", trustChange = 10 }
                }
            },

            // 玩家选择影响 (2)
            new DialogueNode {
                id = "lily_trust_path",
                speaker = "lily",
                content = "「你一直对我很好...」\n她认真地看着你：「我决定了，以后要把你的故事也记在本子里！」\n\n「因为你是我遇到的最好的人之一！」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "不用特意记我", nextId = "END", trustChange = 3 },
                    new DialogueChoice { text = "谢谢你，莉莉", nextId = "lily_special_friend", trustChange = 10 }
                }
            },
            new DialogueNode {
                id = "lily_special_friend",
                speaker = "lily",
                content = "「嘿嘿！」\n她开心地笑了：「你是我的特别朋友！」\n\n「以后不管发生什么，我都会记得你的！」\n\n她的眼睛闪闪发亮：「这是我最重要的约定！」",
                flag = "lily_best_friend",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我也是", nextId = "END", trustChange = 15 }
                }
            },
            new DialogueNode {
                id = "lily_protect_choice",
                speaker = "lily",
                content = "「有时候我在想...」\n她轻声说：「如果当时有人保护我就好了...」\n\n「所以我决定了，以后我变强了，也要保护其他人！」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你已经很强了", nextId = "END", trustChange = 5 },
                    new DialogueChoice { text = "我们一起变强", nextId = "lily_grow_strong", trustChange = 10 }
                }
            },
            new DialogueNode {
                id = "lily_grow_strong",
                speaker = "lily",
                content = "「嗯！」\n她伸出手：「拉钩！」\n\n「我们一起变强，一起找到答案，一起保护这个世界！」\n\n「这是我们的约定！」",
                flag = "lily_promise_grow",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "拉钩！", nextId = "END", trustChange = 15 }
                }
            },

            // ========== 埃里克对话 ==========
            new DialogueNode {
                id = "eric_greet",
                speaker = "eric",
                content = "「站住。」\n一个高大的男人挡在路中间，手按在剑柄上。\n「你们是什么人？为什么来这里？」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我们只是旅人", nextId = "eric_traveler" },
                    new DialogueChoice { text = "我们是来寻找森林之心的", nextId = "eric_forest_heart", minTrust = 20 },
                    new DialogueChoice { text = "让开，否则不客气", nextId = "eric_threaten", trustChange = -15 }
                }
            },
            new DialogueNode {
                id = "eric_traveler",
                speaker = "eric",
                content = "「旅人...哼。」\n他打量着你们：「这里不是你们该来的地方。但既然来了...」\n「用实力证明自己吧。」",
                isCombat = true,
                choices = new List<DialogueChoice>()
            },
            new DialogueNode {
                id = "eric_forest_heart",
                speaker = "eric",
                content = "「森林之心...」\n他的眼神变了：「你们也知道那个传说？」\n\n「我是曾经的皇家骑士，奉命调查黑雾的源头。多年了，我一直在寻找...」\n\n「如果你能通过我的考验，我可以告诉你更多。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我接受考验", nextId = "eric_trial", trustChange = 5 },
                    new DialogueChoice { text = "你为什么这么执着？", nextId = "eric_why" }
                }
            },
            new DialogueNode {
                id = "eric_why",
                speaker = "eric",
                content = "「因为...我的家人被黑雾吞噬了。」\n「我的妻子，我的孩子...都在那场灾难中消失了。」\n\n他深吸一口气：「如果你找到了森林之心，帮我看看...它们是否还在那里。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我答应你", nextId = "eric_promise", trustChange = 10 }
                }
            },
            new DialogueNode {
                id = "eric_promise",
                speaker = "eric",
                content = "「...谢谢。」\n他伸出手：「我叫埃里克。以后就是同伴了。」\n「前面有危险，让我开路。」",
                isEnding = true,
                choices = new List<DialogueChoice>()
            },

            // ========== 埃里克扩展对话 ==========

            // 日常闲聊 (5)
            new DialogueNode {
                id = "eric_chat_watch",
                speaker = "eric",
                content = "「...」\n他坐在篝火旁擦拭着剑，一言不发。\n火光映照在他严肃的脸上。",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你在想什么？", nextId = "eric_deep_thought" },
                    new DialogueChoice { text = "那把剑很重要吗？", nextId = "eric_sword_story" },
                    new DialogueChoice { text = "我不打扰了", nextId = "END" }
                }
            },
            new DialogueNode {
                id = "eric_deep_thought",
                speaker = "eric",
                content = "「...在想以前的事。」\n他停下手中的动作：「当兵的时候，我们也是这样围着篝火休息...」\n\n「那时候觉得未来很远，现在...未来已经来了。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你的战友呢？", nextId = "eric_comrades", minTrust = 20 },
                    new DialogueChoice { text = "你后悔过吗？", nextId = "eric_regret", minTrust = 25 }
                }
            },
            new DialogueNode {
                id = "eric_sword_story",
                speaker = "eric",
                content = "「...这是我妻子的遗物。」\n他的手指轻轻滑过剑身：「她也是个军人。我们是在战场上认识的。」\n\n「这把剑跟着她参加了无数战斗...后来传给了我。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "她一定很坚强", nextId = "eric_wife_strong", minTrust = 25 },
                    new DialogueChoice { text = "我很抱歉", nextId = "END", trustChange = 3 }
                }
            },
            new DialogueNode {
                id = "eric_wife_strong",
                speaker = "eric",
                content = "「...是的。」\n他的眼神变得柔和：「她是我见过最坚强的人。」\n\n「每次我想要放弃的时候，都是她在背后推我一把...」\n\n「现在轮到我推自己了。」",
                trustChange = 8,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你做到了", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "eric_comrades",
                speaker = "eric",
                content = "「...都死了。」\n他的声音平静得可怕：「黑雾来的那天，我们奉命保护村民撤退。」\n\n「最后只有我一个人活了下来。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "不是你的错", nextId = "eric_not_fault", minTrust = 15 },
                    new DialogueChoice { text = "你活下来是为了什么？", nextId = "eric_purpose", minTrust = 30 }
                }
            },
            new DialogueNode {
                id = "eric_not_fault",
                speaker = "eric",
                content = "「...我知道。」\n他沉默了一会儿：「但知道和接受是两回事。」\n\n「有时候半夜醒来，还是会想起他们的脸...」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你背负得够多了", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "eric_purpose",
                speaker = "eric",
                content = "「...为了终结这一切。」\n他站起身：「我的妻子，我的孩子，我的战友...他们的仇，我要亲手报。」\n\n「还有那些还在受苦的人...我必须保护他们。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我帮你", nextId = "eric_accept_help", trustChange = 15 },
                    new DialogueChoice { text = "你一个人扛得太多了", nextId = "eric_lonely_warrior", minTrust = 20 }
                }
            },
            new DialogueNode {
                id = "eric_accept_help",
                speaker = "eric",
                content = "「...」\n他看了你一眼：「你倒是第一个主动说要帮我的。」\n\n「...谢谢。但别拖后腿。」",
                trustChange = 12,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我不会的", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "eric_lonely_warrior",
                speaker = "eric",
                content = "「...习惯了。」\n他重新坐下：「当兵的人都知道，战争中没有朋友会活得更久。」\n\n「但是...现在好像有点不一样了。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "哪里不一样？", nextId = "eric_feel_change", minTrust = 25 }
                }
            },
            new DialogueNode {
                id = "eric_feel_change",
                speaker = "eric",
                content = "「...我不知道。」\n他看着篝火：「也许是遇到你们之后吧。」\n\n「有人愿意相信我，有人愿意跟着我...很久没有这种感觉了。」",
                trustChange = 10,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你不是一个人了", nextId = "END", trustChange = 10 }
                }
            },
            new DialogueNode {
                id = "eric_regret",
                speaker = "eric",
                content = "「后悔？」\n他冷笑了一下：「当兵是我的选择。我从不后悔。」\n\n「我唯一后悔的是...没能保护好我的人。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你已经尽力了", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "eric_chat_training",
                speaker = "eric",
                content = "「你还愣着干什么？」\n他头也不抬：「作为战士，基本功不能落下。」\n\n「去那边练练挥剑。等黑雾里的怪物来了，总不能赤手空拳。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "教我几招吧", nextId = "eric_teach", trustChange = 5 },
                    new DialogueChoice { text = "好的", nextId = "END" }
                }
            },
            new DialogueNode {
                id = "eric_teach",
                speaker = "eric",
                content = "「...好吧。」\n他站起来，把剑递给你：「先从基础开始。握剑要稳，力气从腰发出。」\n\n「记住，剑是你的延伸，不是负担。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我会认真学的", nextId = "END", trustChange = 8 }
                }
            },

            // 背景故事 (5)
            new DialogueNode {
                id = "eric_military_past",
                speaker = "eric",
                content = "「我是皇家骑士团的副团长...曾经是。」\n他的目光遥远：「十二年的军旅生涯，我以为那就是我的全部。」\n\n「但黑雾比任何敌人都可怕...它不是用剑能砍死的。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "皇家骑士团...还存在吗？", nextId = "eric_knights_left", minTrust = 20 },
                    new DialogueChoice { text = "黑雾改变了一切", nextId = "END", trustChange = 3 }
                }
            },
            new DialogueNode {
                id = "eric_knights_left",
                speaker = "eric",
                content = "「不知道...」\n他摇摇头：「通讯中断之后，我就失去了所有联系。」\n\n「也许有人活下来，在某处继续战斗...也许只剩下我了。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我们会找到答案的", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "eric_family_origin",
                speaker = "eric",
                content = "「我的妻子叫艾琳...是个比我更厉害的剑士。」\n他的嘴角微微上扬：「她总说我是她带过的最笨的学生。」\n\n「但就是这样的她，愿意嫁给我这个笨蛋。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "她一定很爱你", nextId = "eric_love_story", minTrust = 25 },
                    new DialogueChoice { text = "你一定很爱她", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "eric_love_story",
                speaker = "eric",
                content = "「...是的。」\n他沉默了一会儿：「我们是在一场战役后相遇的。我军大败，敌军追兵在后...」\n\n「是她突然出现，一剑斩断了追兵的旗帜，然后拉着我的手就跑。」\n\n「那一刻，我就决定了。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "决定什么？", nextId = "eric_decided", minTrust = 30 },
                    new DialogueChoice { text = "很浪漫", nextId = "END", trustChange = 8 }
                }
            },
            new DialogueNode {
                id = "eric_decided",
                speaker = "eric",
                content = "「决定这辈子就她了。」\n他的眼神柔和：「后来我问她为什么救我，她说...'因为你的眼神和我一样，都是想活下去的眼神。'」\n\n「她总是知道该说什么。」",
                trustChange = 10,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "很美的故事", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "eric_child_memory",
                speaker = "eric",
                content = "「我的孩子叫小明...才五岁。」\n他的声音变得轻柔：「他最喜欢我抱着他看星星...」\n\n「总是问星星上有没有小朋友。我总是回答说...也许有吧。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "他一定很可爱", nextId = "eric_son_photo", minTrust = 25 },
                    new DialogueChoice { text = "对不起让你想起这些", nextId = "eric_comfort", minTrust = 15 }
                }
            },
            new DialogueNode {
                id = "eric_son_photo",
                speaker = "eric",
                content = "「...是的。」\n他从怀中拿出一张叠旧的纸：「这是他画的画。画了我们一家三口。」\n\n「他把我画得特别大...因为他说爸爸是能打败所有怪物的英雄。」",
                trustChange = 12,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你是他的英雄", nextId = "END", trustChange = 10 }
                }
            },
            new DialogueNode {
                id = "eric_comfort",
                speaker = "eric",
                content = "「...没关系。」\n他轻轻收起那张纸：「说出来的感觉...比一直憋着好。」\n\n「很久没有对人说起他们了。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "以后可以跟我说", nextId = "END", trustChange = 8 }
                }
            },
            new DialogueNode {
                id = "eric_village_memory",
                speaker = "eric",
                content = "「黑雾来临的那天...是我的生日。」\n他的声音很轻：「艾琳说要给我一个惊喜...让小明先带我去森林里采花。」\n\n「等我回来的时候...村子已经不见了。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "这是...上天的残忍玩笑", nextId = "END", trustChange = 5 }
                }
            },

            // 任务委托 (3)
            new DialogueNode {
                id = "eric_quest_guard",
                speaker = "eric",
                content = "「我感知到前方有大量黑雾生物聚集...」\n他皱着眉：「可能是在守护什么东西。」\n\n「我一个人无法突破。你愿意和我一起去吗？」",
                flag = "eric_guard_quest",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我去", nextId = "END", trustChange = 8 },
                    new DialogueChoice { text = "为什么选我？", nextId = "eric_why_me", minTrust = 20 }
                }
            },
            new DialogueNode {
                id = "eric_why_me",
                speaker = "eric",
                content = "「因为你身上有一股...不放弃的气息。」\n他解释道：「这在黑雾笼罩的世界里很少见。」\n\n「而且...你不是会抛下同伴逃走的人吧？」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "当然不是", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "eric_quest_scout",
                speaker = "eric",
                content = "「我需要有人帮我去侦查一下北边的地形...」\n他展开一张破旧的地图：「这里以前是我们的防线，现在不知道怎么样了。」\n\n「如果有人幸存，我需要知道。」",
                flag = "eric_scout_quest",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "交给我", nextId = "END", trustChange = 5 },
                    new DialogueChoice { text = "太危险了", nextId = "eric_scared", trustChange = -5 }
                }
            },
            new DialogueNode {
                id = "eric_scared",
                speaker = "eric",
                content = "「...」\n他看了你一眼：「算了，我自己来。」\n\n「当过兵的人都知道，侦查是拿命换情报的工作。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "等等，我可以去", nextId = "END", trustChange = 3 }
                }
            },

            // 情感对话 (3)
            new DialogueNode {
                id = "eric_trust_talk",
                speaker = "eric",
                content = "「...我不知道该不该信任你。」\n他直接说：「但你身上有一种让我想起战友的东西。」\n\n「所以...暂时相信你。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我不会让你失望", nextId = "END", trustChange = 8 },
                    new DialogueChoice { text = "你也值得信任", nextId = "eric_mutual_trust", trustChange = 10 }
                }
            },
            new DialogueNode {
                id = "eric_mutual_trust",
                speaker = "eric",
                content = "「...哈。」\n他罕见地笑了一下：「第一次有人这么说。」\n\n「在这片森林里，我不信任任何人。但也许...你是例外。」",
                trustChange = 15,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我会证明你的信任是对的", nextId = "END", trustChange = 10 }
                }
            },
            new DialogueNode {
                id = "eric_night_watch",
                speaker = "eric",
                content = "「...你去睡吧。」\n他背对着篝火坐下：「我守夜。当兵多年，习惯了。」\n\n「夜间是黑雾生物最活跃的时候。有什么动静我会叫醒你。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我陪你一起守", nextId = "eric_watch_together", trustChange = 10 },
                    new DialogueChoice { text = "谢谢，小心点", nextId = "END" }
                }
            },
            new DialogueNode {
                id = "eric_watch_together",
                speaker = "eric",
                content = "「...随你。」\n他看了你一眼，没再说什么。\n\n夜色中，篝火的光芒映照着两人的身影。他沉默地坐着，但这种沉默不再让人感到压迫。",
                trustChange = 12,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "...", nextId = "END" }
                }
            },

            // 黑雾真相 (2)
            new DialogueNode {
                id = "eric_blackmist_battle",
                speaker = "eric",
                content = "「我调查过黑雾...花了很长时间。」\n他压低声音：「黑雾不是天灾，是人祸。」\n\n「有人在这片森林里进行了某种实验...打开了不该打开的东西。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "什么实验？", nextId = "eric_experiment", minTrust = 30 },
                    new DialogueChoice { text = "你知道是谁吗？", nextId = "eric_who_did", minTrust = 35 }
                }
            },
            new DialogueNode {
                id = "eric_experiment",
                speaker = "eric",
                content = "「我在废墟里找到过一些记录...字迹已经模糊了。」\n他的眼神变得阴沉：「提到了一种叫做'生命封印'的技术。」\n\n「据说可以封印一切生命...包括黑雾的源头。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "结果呢？", nextId = "eric_experiment_fail", minTrust = 35 }
                }
            },
            new DialogueNode {
                id = "eric_experiment_fail",
                speaker = "eric",
                content = "「失败了。」\n他摇摇头：「封印在崩溃，能量失控。黑雾就是从那之后开始的。」\n\n「但我还不知道...是谁在进行这个实验。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我会帮你查清楚", nextId = "END", trustChange = 8 }
                }
            },
            new DialogueNode {
                id = "eric_who_did",
                speaker = "eric",
                content = "「...我不确定。但有几个猜测。」\n他的眼神变得危险：「可能是皇家法师团的人，也可能是...守林人。」\n\n「如果是官方的人...我曾经的战友可能也牵涉其中。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你是说...你的战友制造了黑雾？", nextId = "eric_comrade_guilt", minTrust = 45 }
                }
            },
            new DialogueNode {
                id = "eric_comrade_guilt",
                speaker = "eric",
                content = "「...我不知道。」\n他的声音很低：「也许有人是无辜的被迫参与，也许有人早就知道真相。」\n\n「这就是我要找到答案的原因。不管真相是什么。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我会和你一起找到真相", nextId = "END", trustChange = 10 }
                }
            },
            new DialogueNode {
                id = "eric_forest_heart_military",
                speaker = "eric",
                content = "「森林之心...我也听说过。」\n他的眼神变得严肃：「皇家档案里有记载，说那里封印着某种强大的力量。」\n\n「如果我们能找到那里...也许能找到终结黑雾的方法。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "一起去？", nextId = "eric_go_heart", minTrust = 30 },
                    new DialogueChoice { text = "先完成眼前的任务吧", nextId = "END" }
                }
            },
            new DialogueNode {
                id = "eric_go_heart",
                speaker = "eric",
                content = "「...好。」\n他站起身：「很久没有人和我并肩作战了。」\n\n「去森林之心的路很危险。但如果你准备好了...我们明天就出发。」",
                flag = "eric_companion_forest_heart",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我准备好了", nextId = "END", trustChange = 15 }
                }
            },

            // 玩家选择影响 (2)
            new DialogueNode {
                id = "eric_combat_trust",
                speaker = "eric",
                content = "「刚才的战斗...你的表现不错。」\n他点点头：「能在那种情况下保持冷静，不是一般人能做到的。」\n\n「看来我没看错人。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "是你教得好", nextId = "END", trustChange = 8 },
                    new DialogueChoice { text = "我不想再有人牺牲", nextId = "eric_no_sacrifice", trustChange = 12 }
                }
            },
            new DialogueNode {
                id = "eric_no_sacrifice",
                speaker = "eric",
                content = "「...」\n他沉默了很久：「我听过这句话。曾经也有人对我说过。」\n\n「她是我的妻子。」\n\n他看着你：「这一次，换我来实现这句话。」",
                trustChange = 18,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我们一起实现", nextId = "END", trustChange = 15 }
                }
            },
            new DialogueNode {
                id = "eric_protect_choice",
                speaker = "eric",
                content = "「如果有一天...你必须在我和任务之间选择...」\n他的声音很平静：「选任务。」\n\n「这不是请求，是我作为老兵的命令。任务比任何人的命都重要。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我不会丢下任何人", nextId = "eric_refuse_order", trustChange = 15 },
                    new DialogueChoice { text = "...我记住了", nextId = "END", trustChange = -5 }
                }
            },
            new DialogueNode {
                id = "eric_refuse_order",
                speaker = "eric",
                content = "「...哈。」\n他笑了一声，笑容里有些无奈：「你和艾琳说的一样。」\n\n「总是说同样的话，做同样的选择。」\n\n他拍了拍你的肩膀：「...那就别让我失望。」",
                trustChange = 20,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "绝对不会", nextId = "END", trustChange = 10 }
                }
            },

            // ========== 灰烬对话 ==========

            // 日常闲聊 (5)
            new DialogueNode {
                id = "ash_greet",
                speaker = "ash",
                content = "「...你来了。」\n灰色的身影站在树影下，看不清表情。",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你是灰烬？", nextId = "ash_intro" },
                    new DialogueChoice { text = "为什么在这里？", nextId = "ash_why_here" },
                    new DialogueChoice { text = "绕过去", nextId = "END" }
                }
            },
            new DialogueNode {
                id = "ash_intro",
                speaker = "ash",
                content = "「...名字不重要。」\n他转过身，灰色的眼眸打量着你：「你身上有黑雾的气息...但不重。」\n\n「还没有被完全侵蚀。运气不错。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你知道黑雾的事？", nextId = "ash_know_mist", minTrust = 20 },
                    new DialogueChoice { text = "你能帮我吗？", nextId = "ash_help", minTrust = 15 }
                }
            },
            new DialogueNode {
                id = "ash_why_here",
                speaker = "ash",
                content = "「...等待。」\n他看向远方：「等待该来的人。」\n\n「你就是吗...？」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "什么意思？", nextId = "ash_waiting", minTrust = 25 },
                    new DialogueChoice { text = "你在等我？", nextId = "ash_waiting", minTrust = 30 }
                }
            },
            new DialogueNode {
                id = "ash_waiting",
                speaker = "ash",
                content = "「...森林在呼唤。」\n他低声说：「能听到的人，会被引导到这里。」\n\n「你听到了吗？」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我听到了...", nextId = "ash_hear_call", minTrust = 35 },
                    new DialogueChoice { text = "我不明白", nextId = "END" }
                }
            },
            new DialogueNode {
                id = "ash_hear_call",
                speaker = "ash",
                content = "「...不需要明白。」\n他转身向前走去：「跟着我。答案在路上。」\n\n他的身影渐渐融入灰色的雾气中。",
                flag = "ash_companion",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "等等我", nextId = "END", trustChange = 10 }
                }
            },
            new DialogueNode {
                id = "ash_know_mist",
                speaker = "ash",
                content = "「...知道一些。」\n他的声音平静：「黑雾不是敌人。是结果。」\n\n「就像火会产生烟一样...黑雾是某种力量失衡后的产物。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "什么力量？", nextId = "ash_force", minTrust = 25 },
                    new DialogueChoice { text = "怎么终结它？", nextId = "ash_end_mist", minTrust = 30 }
                }
            },
            new DialogueNode {
                id = "ash_force",
                speaker = "ash",
                content = "「...生命与死亡之间的裂缝。」\n他看着树叶间漏下的光线：「森林之心里封印着的东西，本不该存在于这个世界。」\n\n「有人打开了那扇门。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "谁打开了门？", nextId = "ash_who_open", minTrust = 40 },
                    new DialogueChoice { text = "可以关上吗？", nextId = "ash_close_door", minTrust = 35 }
                }
            },
            new DialogueNode {
                id = "ash_who_open",
                speaker = "ash",
                content = "「...很久以前的事了。」\n他摇摇头：「知道答案的人...都已经不在了。」\n\n「或者...已经变成了别的东西。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你也知道很多", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "ash_close_door",
                speaker = "ash",
                content = "「...也许可以。」\n他若有所思：「森林之心还在。它在等待。」\n\n「等待能够重新封印的人。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "那个人是我吗？", nextId = "ash_is_it_you", minTrust = 45 },
                    new DialogueChoice { text = "怎么找到森林之心？", nextId = "ash_where_heart", minTrust = 30 }
                }
            },
            new DialogueNode {
                id = "ash_is_it_you",
                speaker = "ash",
                content = "「...不是你。」\n他的眼神深邃：「是和你一起的人。」\n\n「一个人无法完成。需要多个人...需要羁绊。」\n\n「这就是黑雾教会我们的。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我相信我的同伴", nextId = "END", trustChange = 8 }
                }
            },
            new DialogueNode {
                id = "ash_where_heart",
                speaker = "ash",
                content = "「...一直都在。」\n他指向森林深处：「但不是用脚走的路。」\n\n「用心走。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我明白了", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "ash_help",
                speaker = "ash",
                content = "「...我帮不了你。」\n他转过身：「但我可以指路。」\n\n「剩下的，要靠你自己。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "那就指路吧", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "ash_end_mist",
                speaker = "ash",
                content = "「...终结不是抹去。」\n他的声音很轻：「黑雾无法被消灭。它只能被...转化。」\n\n「转化为别的东西。光，或者别的什么。」\n\n「这就是森林之心的力量。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "怎么转化？", nextId = "ash_transform", minTrust = 35 }
                }
            },
            new DialogueNode {
                id = "ash_transform",
                speaker = "ash",
                content = "「...需要祭品。」\n他停顿了一下：「不是血，是记忆。」\n\n「最珍贵的记忆。用来填补裂缝。」\n\n「这就是代价。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "如果我愿意付出呢？", nextId = "ash_willing", minTrust = 50 },
                    new DialogueChoice { text = "有别的方法吗？", nextId = "END" }
                }
            },
            new DialogueNode {
                id = "ash_willing",
                speaker = "ash",
                content = "「...」\n他看了你很久：「很久没有人这样说了。」\n\n「但现在还不是时候。你还不够强。」\n\n「先去找森林之心...到时候你就会知道该付出什么了。」",
                trustChange = 15,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我会去的", nextId = "END", trustChange = 10 }
                }
            },

            // 背景故事 (5)
            new DialogueNode {
                id = "ash_origin",
                speaker = "ash",
                content = "「...我的故事吗。」\n他沉默了一会儿：「我是被森林捡回来的。」\n\n「很久以前，有个孩子在黑雾中迷路。我...就是那个孩子。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "森林救了你？", nextId = "ash_forest_save", minTrust = 25 },
                    new DialogueChoice { text = "你怎么活下来的？", nextId = "ash_how_live", minTrust = 20 }
                }
            },
            new DialogueNode {
                id = "ash_forest_save",
                speaker = "ash",
                content = "「...不是救。」\n他看着自己的手：「是接纳。」\n\n「森林之心给了我一个选择。留下来，或者回去。」\n\n「我选了留下。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "为什么？", nextId = "ash_why_stay", minTrust = 35 }
                }
            },
            new DialogueNode {
                id = "ash_why_stay",
                speaker = "ash",
                content = "「...因为回去也没有意义了。」\n他的声音很轻：「我的家人...我的村子...都被黑雾带走了。」\n\n「回去只会看到废墟。不如留在这里...做森林的眼睛。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你后悔吗？", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "ash_how_live",
                speaker = "ash",
                content = "「...森林在喂我。」\n他指了指周围：「苔藓、树果、偶尔的露水。足够活下去。」\n\n「不需要很多。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你不需要同伴吗？", nextId = "ash_lonely", minTrust = 25 }
                }
            },
            new DialogueNode {
                id = "ash_lonely",
                speaker = "ash",
                content = "「...习惯了。」\n他抬头看着树冠：「一个人的好处是...不会失去任何人。」\n\n「坏处是...有时候会忘记自己还活着。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你可以来找我", nextId = "ash_find_me", trustChange = 15 },
                    new DialogueChoice { text = "孤独也是一种力量", nextId = "END", trustChange = 3 }
                }
            },
            new DialogueNode {
                id = "ash_find_me",
                speaker = "ash",
                content = "「...」\n他沉默了很久：「很久没有人这样说过了。」\n\n「也许...会去吧。」\n\n他没有看你，但语气里似乎有了一丝松动。",
                trustChange = 18,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我等你", nextId = "END", trustChange = 10 }
                }
            },
            new DialogueNode {
                id = "ash_keepers",
                speaker = "ash",
                content = "「...守林人。」\n他看向远方：「曾经有很多人。现在...不知道还有没有。」\n\n「他们负责守护森林之心。守护那道门。」\n\n「但门已经被打开了。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你是守林人吗？", nextId = "ash_is_keeper", minTrust = 35 },
                    new DialogueChoice { text = "他们去哪了？", nextId = "ash_keepers_where", minTrust = 30 }
                }
            },
            new DialogueNode {
                id = "ash_is_keeper",
                speaker = "ash",
                content = "「...不是。」\n他摇摇头：「我只是一个...留下来的人。」\n\n「守林人有他们的使命。我有我的。」\n\n「我的使命是...等该来的人。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你等到我了吗？", nextId = "ash_wait_you", minTrust = 40 }
                }
            },
            new DialogueNode {
                id = "ash_wait_you",
                speaker = "ash",
                content = "「...也许。」\n他转过身：「也许只是另一个迷路的人。」\n\n「时间会证明一切。」",
                trustChange = 5,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我会证明给你看", nextId = "END", trustChange = 8 }
                }
            },
            new DialogueNode {
                id = "ash_keepers_where",
                speaker = "ash",
                content = "「...不知道。」\n他的声音平静：「也许在森林深处的某个地方。」\n\n「也许已经和森林融为一体了。」\n\n「也许...变成了黑雾的一部分。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "变成黑雾？", nextId = "ash_become_mist", minTrust = 40 }
                }
            },
            new DialogueNode {
                id = "ash_become_mist",
                speaker = "ash",
                content = "「...是的。」\n他看着自己的手：「守林人的生命和森林之心相连。」\n\n「当封印崩溃时，他们也被吞噬了。」\n\n「所以黑雾里...有他们的意识碎片。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "这太悲伤了", nextId = "END", trustChange = 5 }
                }
            },

            // 任务委托 (3)
            new DialogueNode {
                id = "ash_quest_stone",
                speaker = "ash",
                content = "「...帮我找一个东西。」\n他递过来一块灰色的石头：「这是'回声石'。能感应黑雾核心的位置。」\n\n「如果你找到了森林之心...它会有反应。」",
                flag = "ash_seeking_stone",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我会找到的", nextId = "END", trustChange = 8 },
                    new DialogueChoice { text = "为什么给我？", nextId = "ash_why_stone", minTrust = 30 }
                }
            },
            new DialogueNode {
                id = "ash_why_stone",
                speaker = "ash",
                content = "「...因为你需要它。」\n他的理由简单：「我不需要。我已经知道路了。」\n\n「你不知道。所以给你。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "谢谢", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "ash_quest_memory",
                speaker = "ash",
                content = "「...我需要你帮我记住一些东西。」\n他看着你：「我的记忆有时候会模糊...黑雾的影响。」\n\n「如果你见到什么重要的...告诉我。我来帮你分析。」\n\n「这样我们都不会遗漏。」",
                flag = "ash_memory_quest",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "成交", nextId = "END", trustChange = 10 },
                    new DialogueChoice { text = "你的记忆受影响多深？", nextId = "ash_memory_how_bad", minTrust = 30 }
                }
            },
            new DialogueNode {
                id = "ash_memory_how_bad",
                speaker = "ash",
                content = "「...有时候会忘记昨天发生的事。」\n他的语气平淡：「有时候会忘记更早的。」\n\n「但没关系。森林会帮我记住。」\n\n「树叶不会忘记落在它们身上的东西。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我能帮你记住", nextId = "END", trustChange = 10 }
                }
            },
            new DialogueNode {
                id = "ash_quest_path",
                speaker = "ash",
                content = "「...前面有条路。」\n他指向一处看似普通的灌木丛：「看起来没路。但其实有。」\n\n「帮我看看...那边有什么。我无法离开这片区域太远。」",
                flag = "ash_path_quest",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我去看看", nextId = "END", trustChange = 8 }
                }
            },

            // 情感对话 (3)
            new DialogueNode {
                id = "ash_trust",
                speaker = "ash",
                content = "「...你为什么相信我？」\n他问得很直接：「我什么都没给你。」\n\n「没有承诺，没有交易。你为什么愿意听我说话？」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "因为你需要被相信", nextId = "ash_need_believe", trustChange = 15 },
                    new DialogueChoice { text = "直觉", nextId = "END", trustChange = 3 }
                }
            },
            new DialogueNode {
                id = "ash_need_believe",
                speaker = "ash",
                content = "「...」\n他沉默了很久。\n\n「也许吧。」\n\n他第一次主动转过身来面对你：「很久没有人...愿意相信我了。」",
                trustChange = 20,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "以后我都会信你", nextId = "END", trustChange = 15 }
                }
            },
            new DialogueNode {
                id = "ash_friend",
                speaker = "ash",
                content = "「...朋友。」\n他念这个词像是在思考什么：「我以前也有过。」\n\n「后来就没有了。」\n\n他看着你：「也许...可以再有。」",
                trustChange = 18,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你愿意吗？", nextId = "END", trustChange = 10 }
                }
            },
            new DialogueNode {
                id = "ash_feelings",
                speaker = "ash",
                content = "「...我已经很久没有感觉到什么了。」\n他的声音很轻：「黑雾会让人麻木。这是保护。」\n\n「但是今天...好像有什么在松动。」\n\n「也许是你。也许是森林在回应什么。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我很高兴", nextId = "END", trustChange = 8 }
                }
            },

            // 黑雾真相 (2)
            new DialogueNode {
                id = "ash_mist_truth",
                speaker = "ash",
                content = "「...你问得太多了。」\n他看着你：「但既然你走了这么远...就告诉你一些吧。」\n\n「黑雾的源头，是守林人的罪。」\n\n「他们试图封印永生，却打开了死亡的门。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "永生？", nextId = "ash_immortal", minTrust = 45 },
                    new DialogueChoice { text = "继续说", nextId = "ash_truth_cont", minTrust = 40 }
                }
            },
            new DialogueNode {
                id = "ash_immortal",
                speaker = "ash",
                content = "「...守林人想要的是永恒的生命。」\n他的眼神深邃：「但生命不应该被永恒。」\n\n「当他们强行延续时...平衡被打破了。」\n\n「黑雾就是失衡后的产物。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "所以终结黑雾的方法是...", nextId = "ash_solution", minTrust = 50 }
                }
            },
            new DialogueNode {
                id = "ash_solution",
                speaker = "ash",
                content = "「...接受终结。」\n他平静地说：「不是阻止死亡。是让生命流动。」\n\n「森林之心里有纯净的生命之水...它可以重新开始循环。」\n\n「但需要有人愿意放下永生的执念。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我明白了", nextId = "END", trustChange = 10 }
                }
            },
            new DialogueNode {
                id = "ash_truth_cont",
                speaker = "ash",
                content = "「...封印失败后，守林人并没有消失。」\n他看向远方：「他们和封印融为一体...成为了黑雾的一部分。」\n\n「所以黑雾里...有他们的怨恨、不甘、和遗憾。」\n\n「这就是为什么黑雾会吞噬记忆。因为他们不想被遗忘。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "这是悲剧", nextId = "END", trustChange = 8 }
                }
            },
            new DialogueNode {
                id = "ash_core",
                speaker = "ash",
                content = "「...森林之心的核心，有一样东西。」\n他的声音变得严肃：「一颗心脏。守林人最初首领的心脏。」\n\n「它还在跳动。还在试图维持封印。」\n\n「这就是为什么森林还没有完全崩溃。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我可以救它吗？", nextId = "ash_save_heart", minTrust = 55 }
                }
            },
            new DialogueNode {
                id = "ash_save_heart",
                speaker = "ash",
                content = "「...不是救。是结束。」\n他摇摇头：「让它安息。让封印终结。让一切重新开始。」\n\n「这就是森林之心需要完成的使命。」\n\n「而你...可能是唯一能做到的人。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我会去森林之心", nextId = "END", trustChange = 15 }
                }
            },

            // 玩家选择影响 (2)
            new DialogueNode {
                id = "ash_choice_future",
                speaker = "ash",
                content = "「...你选择了这条路。」\n他看着你：「黑雾里还有很多选择。每一个都会改变结局。」\n\n「记住你为什么出发。这很重要。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "为了结束这一切", nextId = "END", trustChange = 10 },
                    new DialogueChoice { text = "为了被黑雾带走的人", nextId = "ash_choice_memory", trustChange = 15 }
                }
            },
            new DialogueNode {
                id = "ash_choice_memory",
                speaker = "ash",
                content = "「...」\n他的眼睛里闪过一丝光：「很久没有人这样说了。」\n\n「为了记忆而战...这是正确的理由。」\n\n「它会引导你。也会支撑你。」",
                trustChange = 20,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我会记住所有人", nextId = "END", trustChange = 15 }
                }
            },
            new DialogueNode {
                id = "ash_choice_sacrifice",
                speaker = "ash",
                content = "「...你在想牺牲的事。」\n他似乎看穿了你：「不要想太多。」\n\n「有时候活着比牺牲更难。」\n\n「记住：终结不是结束。是新的开始。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我会记住", nextId = "END", trustChange = 10 }
                }
            },

            // ========== 沃斯对话 ==========

            // 日常闲聊 (5)
            new DialogueNode {
                id = "vos_greet",
                speaker = "vos",
                content = "「哟！稀客稀客！」\n一个圆滑的声音从货摊后传来。一个中年男人笑眯眯地看着你。\n\n「旅人，想看看货物吗？保证货真价实，童叟无欺！」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你是谁？", nextId = "vos_intro" },
                    new DialogueChoice { text = "有什么好东西？", nextId = "vos_goods" },
                    new DialogueChoice { text = "不感兴趣", nextId = "END" }
                }
            },
            new DialogueNode {
                id = "vos_intro",
                speaker = "vos",
                content = "「我？我叫沃斯！」\n他热情地伸出手：「在黑雾之前，我可是走遍大陆的大商人！」\n\n「现在嘛...只能在这森林边上做点小买卖了。」\n\n「但放心！我的东西依然是最棒的！」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你怎么活下来的？", nextId = "vos_survive", minTrust = 15 },
                    new DialogueChoice { text = "你的货物是真的吗？", nextId = "vos_trust" }
                }
            },
            new DialogueNode {
                id = "vos_survive",
                speaker = "vos",
                content = "「哈哈哈！商人有商人的生存之道！」\n他神秘地眨眨眼：「最重要的一条是——永远比灾难快一步。」\n\n「黑雾来之前，我就嗅到了不对劲的味道...所以提前转移了。」\n\n「运气好，运气好而已。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你嗅到了什么？", nextId = "vos_smell", minTrust = 25 },
                    new DialogueChoice { text = "商人就是滑头", nextId = "END" }
                }
            },
            new DialogueNode {
                id = "vos_smell",
                speaker = "vos",
                content = "「这个嘛...」\n他压低声音：「一种腐烂的味道。但不是普通的腐烂...是更深层的东西。」\n\n「我当时以为只是错觉...没想到是真的。」\n\n「但无所谓了，对吧？活下来了就是赢了。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你知道的比说出来的多", nextId = "vos_know_more", minTrust = 35 }
                }
            },
            new DialogueNode {
                id = "vos_know_more",
                speaker = "vos",
                content = "「哈哈哈！商人嘛，就是什么都要知道一点！」\n他笑着摆摆手：「但你要问我真正知道什么...那可得看交易了。」\n\n「有些情报，我可不会白送的。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "什么价格？", nextId = "vos_price_info", minTrust = 20 },
                    new DialogueChoice { text = "我买不起", nextId = "END" }
                }
            },
            new DialogueNode {
                id = "vos_price_info",
                speaker = "vos",
                content = "「价格嘛...看你要什么情报。」\n他拿出一本破旧的账本：「简单的消息可以用物资换。重要的嘛...」\n\n「需要黑雾里的东西。比如某种稀有材料...或者一条有价值的情报。」\n\n「等价交换，这是商人的规矩。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "成交", nextId = "END", trustChange = 5 },
                    new DialogueChoice { text = "你很贪婪", nextId = "vos_greedy", trustChange = -5 }
                }
            },
            new DialogueNode {
                id = "vos_greedy",
                speaker = "vos",
                content = "「哈哈哈！贪婪？」\n他不以为意地笑着：「不，贪婪是商人的本能。我只是在做我该做的事。」\n\n「不过...在黑雾之后，有些东西确实变了。」\n\n「有些交易，我已经不愿意做了。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "什么交易？", nextId = "vos_no_trade", minTrust = 30 }
                }
            },
            new DialogueNode {
                id = "vos_no_trade",
                speaker = "vos",
                content = "「...与黑雾相关的交易。」\n他的笑容淡了一些：「有些商人会收集黑雾产物来卖...据说有特殊的效果。」\n\n「但我不碰那些。」\n\n「因为我知道那些东西的真正代价。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "什么代价？", nextId = "vos_mist_cost", minTrust = 40 },
                    new DialogueChoice { text = "你有原则", nextId = "END", trustChange = 8 }
                }
            },
            new DialogueNode {
                id = "vos_mist_cost",
                speaker = "vos",
                content = "「...使用黑雾产物的人，最后都会失去最重要的东西。」\n他的眼神变得深邃：「不是生命。是比生命更重要的东西。」\n\n「记忆，羁绊，希望...或者爱。」\n\n「这就是黑雾的真面目。它吃的是人心。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你见过？", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "vos_goods",
                speaker = "vos",
                content = "「哦！你可算问对人了！」\n他兴奋地打开货摊：「治疗药剂、提神草药、还有防护道具！」\n\n「特别推荐这个——黑雾灯！能在雾里照亮一小片区域！」\n\n「现在打折！只要三个黑雾精华！」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "什么是黑雾精华？", nextId = "vos_essence", minTrust = 15 },
                    new DialogueChoice { text = "我要了", nextId = "END" }
                }
            },
            new DialogueNode {
                id = "vos_essence",
                speaker = "vos",
                content = "「黑雾精华啊...是黑雾生物体内的凝结物。」\n他压低声音：「算是稀有材料吧。很危险才能得到的东西。」\n\n「你可以在森林里猎杀黑雾生物，有几率获得。」\n\n「当然...你也可以用其他情报来换。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "明白了", nextId = "END" }
                }
            },
            new DialogueNode {
                id = "vos_trust",
                speaker = "vos",
                content = "「哈哈哈！怀疑是好事！」\n他从货摊下拿出一张纸：「这是我的信誉保证书！黑雾之前，我可是皇家商会的成员！」\n\n「虽然现在没人在乎了...但我的货绝对没问题！」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "皇家商会？", nextId = "vos_merchant_guild", minTrust = 20 },
                    new DialogueChoice { text = "好吧，我看一看", nextId = "END" }
                }
            },
            new DialogueNode {
                id = "vos_merchant_guild",
                speaker = "vos",
                content = "「哦？你知道皇家商会？」\n他的眼神里闪过一丝惊讶：「是的，我曾经是他们的一员...」\n\n「负责秘密物资的调配。包括...一些特殊物品的采购。」\n\n「不过这都是以前的事了。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "什么特殊物品？", nextId = "vos_special_items", minTrust = 35 }
                }
            },
            new DialogueNode {
                id = "vos_special_items",
                speaker = "vos",
                content = "「...」\n他沉默了一会儿：「你问得太多了。」\n\n「有些东西，知道得太早会招来麻烦。」\n\n「如果你真想知道...等你准备好再说吧。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我会回来的", nextId = "END", trustChange = 5 }
                }
            },

            // 背景故事 (5)
            new DialogueNode {
                id = "vos_past",
                speaker = "vos",
                content = "「我以前可不是这样的...」\n他叹了口气：「黑雾之前，我是商会里最风光的那个。」\n\n「有庄园，有船队，有数不清的货物...」\n\n「现在嘛，就剩这一个小摊子了。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "怀念以前吗？", nextId = "vos_miss_old", minTrust = 20 },
                    new DialogueChoice { text = "你是怎么失去一切的？", nextId = "vos_lose_all", minTrust = 30 }
                }
            },
            new DialogueNode {
                id = "vos_miss_old",
                speaker = "vos",
                content = "「怀念？」\n他摇摇头：「不。那只是过眼云烟。」\n\n「真正让我难受的是...有些人。有些事。」\n\n「当时我为了生意，牺牲了一些东西...现在想来，也许不值得。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "能说说吗？", nextId = "vos_regret_story", minTrust = 30 },
                    new DialogueChoice { text = "每个人都有后悔", nextId = "END", trustChange = 3 }
                }
            },
            new DialogueNode {
                id = "vos_lose_all",
                speaker = "vos",
                content = "「...不是黑雾拿走的。」\n他的笑容消失了：「是我自己扔掉的。」\n\n「当黑雾来的时候，我选择了货物，放弃了人。」\n\n「结果...货物也没保住。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你放弃了谁？", nextId = "vos_abandon", minTrust = 40 },
                    new DialogueChoice { text = "你后悔了", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "vos_abandon",
                speaker = "vos",
                content = "「...我的合伙人。」\n他的声音变得低沉：「他有家人。我也有。」\n\n「我们说好了要一起走。但当黑雾来的时候...我一个人跑了。」\n\n「因为我不想丢下我的货物。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "他后来怎么样了？", nextId = "vos_partner", minTrust = 45 },
                    new DialogueChoice { text = "你还为此痛苦", nextId = "vos_guilt", minTrust = 35 }
                }
            },
            new DialogueNode {
                id = "vos_partner",
                speaker = "vos",
                content = "「...死了。」\n他平静地说：「黑雾第三天的时候。」\n\n「他给我寄了最后一条消息。只有一句话：'我不怪你。'」\n\n「但我知道...是我杀了他。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你用货物赎罪？", nextId = "vos_redemption", minTrust = 40 },
                    new DialogueChoice { text = "我很抱歉", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "vos_redemption",
                speaker = "vos",
                content = "「...也许吧。」\n他苦笑：「我用剩余的货物帮助其他幸存者。不收钱。」\n\n「这是我唯一能做的补偿。」\n\n「虽然永远也还不完。」",
                trustChange = 10,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你已经在赎罪了", nextId = "END", trustChange = 10 }
                }
            },
            new DialogueNode {
                id = "vos_guilt",
                speaker = "vos",
                content = "「...痛苦是应该的。」\n他摸了摸胸前的某个地方：「我每天都会梦到他。」\n\n「梦里他还活着，还在笑...然后醒来发现只剩下我一个人。」\n\n「这就是商人的代价。为了利益抛弃一切的人，最后会被一切抛弃。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你可以原谅自己", nextId = "vos_forgive", trustChange = 15 },
                    new DialogueChoice { text = "时间会治愈一切", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "vos_forgive",
                speaker = "vos",
                content = "「...原谅？」\n他苦笑：「我不知道该怎么原谅自己。」\n\n「但也许...帮助别人，就是第一步吧。」\n\n他看向远方：「这也是我还在这里卖货的原因。不是为了钱。是为了...赎罪。」",
                trustChange = 15,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "这是好的开始", nextId = "END", trustChange = 10 }
                }
            },

            // 任务委托 (3)
            new DialogueNode {
                id = "vos_quest_trade",
                speaker = "vos",
                content = "「我有一笔生意想和你谈...」\n他压低声音：「北边的废墟里有一批货物，我一个人拿不回来。」\n\n「你帮我拿回来，我们平分。如何？」",
                flag = "vos_trade_quest",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "成交", nextId = "END", trustChange = 5 },
                    new DialogueChoice { text = "为什么找我？", nextId = "vos_why_me", minTrust = 20 }
                }
            },
            new DialogueNode {
                id = "vos_why_me",
                speaker = "vos",
                content = "「因为你看起来靠谱。」\n他打量着你：「而且你不是那种会背叛同伴的人...我能看出来。」\n\n「这个世界上，这种人越来越少了。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "谢谢你的信任", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "vos_quest_info",
                speaker = "vos",
                content = "「我需要一个情报员...」\n他神秘地说：「帮我去各处打听消息。黑雾之后，情报比货物更值钱。」\n\n「报酬好说。我这里什么都有。」",
                flag = "vos_info_quest",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我会帮你", nextId = "END", trustChange = 8 },
                    new DialogueChoice { text = "什么样的情报？", nextId = "vos_info_type", minTrust = 25 }
                }
            },
            new DialogueNode {
                id = "vos_info_type",
                speaker = "vos",
                content = "「主要是黑雾的动向...还有幸存者的下落。」\n他的眼神变得认真：「有人的地方就有生意。有生意的地方，就有我。」\n\n「如果你发现任何商队或者营地的消息...记得告诉我。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "明白了", nextId = "END", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "vos_quest_protect",
                speaker = "vos",
                content = "「最近有商人在森林里失踪了...」\n他忧心忡忡：「我本来打算去接应他们的，但现在不敢一个人走那条路了。」\n\n「你能护送我一程吗？报酬是这次旅程的安全保障...还有我的友谊。」",
                flag = "vos_protect_quest",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我去", nextId = "END", trustChange = 10 },
                    new DialogueChoice { text = "太危险了", nextId = "vos_protect_danger", trustChange = -5 }
                }
            },
            new DialogueNode {
                id = "vos_protect_danger",
                speaker = "vos",
                content = "「...没关系。」\n他勉强笑了笑：「我也习惯了一个人扛。」\n\n「但如果哪天你改变主意了...记得来找我。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "也许以后会帮", nextId = "END" }
                }
            },

            // 情感对话 (3)
            new DialogueNode {
                id = "vos_trust_talk",
                speaker = "vos",
                content = "「你知道吗...」\n他看着你：「你是第一个愿意听我说这些的人。」\n\n「以前的人要么只想和我做生意，要么听到我的过去就走了。」\n\n「但你...好像真的在乎。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "每个人都有故事", nextId = "vos_story_respect", trustChange = 15 },
                    new DialogueChoice { text = "我只是好奇", nextId = "END", trustChange = -3 }
                }
            },
            new DialogueNode {
                id = "vos_story_respect",
                speaker = "vos",
                content = "「...谢谢。」\n他罕见地露出了真诚的笑容：「在这片森林里，能被人理解是奢侈的事。」\n\n「以后你来找我买东西...我给你最低价。」",
                trustChange = 18,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "成交", nextId = "END", trustChange = 10 }
                }
            },
            new DialogueNode {
                id = "vos_hope_talk",
                speaker = "vos",
                content = "「我经常想...黑雾之后的世界会变成什么样。」\n他望着远方：「也许一切都会消失。也许会有新的开始。」\n\n「但不管怎样...生意总是要做的。人们总是需要交易。」\n\n「这就是人类的本能吧。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "你还有希望", nextId = "vos_hope_answer", trustChange = 10 },
                    new DialogueChoice { text = "很现实", nextId = "END", trustChange = 3 }
                }
            },
            new DialogueNode {
                id = "vos_hope_answer",
                speaker = "vos",
                content = "「希望？」\n他苦笑：「不是希望。只是习惯。」\n\n「商人的本能就是低买高卖。即使世界末日来了，这个本能也不会变。」\n\n「但如果你问我心里...也许是有一点希望的吧。希望能看到黑雾散去的那天。」",
                trustChange = 12,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "那一天会来的", nextId = "END", trustChange = 10 }
                }
            },
            new DialogueNode {
                id = "vos_family",
                speaker = "vos",
                content = "「...你有家人吗？」\n他突然问道：「黑雾之后，我已经不敢问别人这个问题了。」\n\n「因为答案往往都是一样的...」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我失去了他们", nextId = "vos_family_shared", trustChange = 15 },
                    new DialogueChoice { text = "我还有家人", nextId = "vos_family_happy", trustChange = 5 }
                }
            },
            new DialogueNode {
                id = "vos_family_shared",
                speaker = "vos",
                content = "「...我也是。」\n他沉默了一会儿：「但我没有你勇敢。」\n\n「我不敢承认这一点。一直用忙碌来麻痹自己。」\n\n「谢谢你愿意说出来。」",
                trustChange = 18,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我们都是幸存者", nextId = "END", trustChange = 10 }
                }
            },
            new DialogueNode {
                id = "vos_family_happy",
                speaker = "vos",
                content = "「...真好。」\n他的眼神里有一丝羡慕：「要珍惜他们。」\n\n「不管发生什么，都要和他们在一起。」\n\n「不要像我一样...等到失去了才知道后悔。」",
                trustChange = 15,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我会的", nextId = "END", trustChange = 10 }
                }
            },

            // 黑雾真相 (2)
            new DialogueNode {
                id = "vos_mist_secret",
                speaker = "vos",
                content = "「你问我关于黑雾的事...」\n他压低声音：「我有一些...不该说的情报。」\n\n「黑雾不是自然形成的。有人在它发生之前就预言到了。」\n\n「而且...那个人还活着。」",
                secret = "mist_prophet",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "谁？！", nextId = "vos_prophet", minTrust = 45 },
                    new DialogueChoice { text = "你在开玩笑？", nextId = "END" }
                }
            },
            new DialogueNode {
                id = "vos_prophet",
                speaker = "vos",
                content = "「...我不能说出名字。」\n他警觉地看了看四周：「但我见过他。在黑雾之前的三天。」\n\n「他警告过我。说三天后会有大难...让我快跑。」\n\n「我没有听。只带走了货物。结果...」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "这个人是谁？", nextId = "vos_prophet_who", minTrust = 55 },
                    new DialogueChoice { text = "他现在在哪里？", nextId = "vos_prophet_where", minTrust = 50 }
                }
            },
            new DialogueNode {
                id = "vos_prophet_who",
                speaker = "vos",
                content = "「...我不能说。」\n他的眼神变得严肃：「不是不想，是不敢。」\n\n「他告诉我，如果他预言的事没有发生，一切都会好起来。」\n\n「但如果发生了...他就会被追杀。会被灭口。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我会保护他", nextId = "END", trustChange = 10 }
                }
            },
            new DialogueNode {
                id = "vos_prophet_where",
                speaker = "vos",
                content = "「...我不知道。」\n他摇摇头：「黑雾之后，我就再也没见过他了。」\n\n「也许他死了。也许他在躲避什么。」\n\n「但如果你找到他...告诉他，我没有忘记他的警告。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我会转告", nextId = "END", trustChange = 10 }
                }
            },

            // 玩家选择影响 (2)
            new DialogueNode {
                id = "vos_choice_trade",
                speaker = "vos",
                content = "「你知道吗...」\n他看着你：「我以前从来不信任任何人。只相信契约和利益。」\n\n「但现在...也许我也该学会信任别人了。」\n\n「就像信任你一样。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我不会让你失望", nextId = "END", trustChange = 15 },
                    new DialogueChoice { text = "谢谢你的信任", nextId = "vos_mutual_trust", trustChange = 18 }
                }
            },
            new DialogueNode {
                id = "vos_mutual_trust",
                speaker = "vos",
                content = "「...哈。」\n他伸出手，这次不是商人的手势，而是一个普通人的握手：「朋友？」\n\n「在这片森林里，我已经很久没有说过这个词了。」\n\n「以后不管发生什么...记得有个商人朋友在等你回来。」",
                flag = "vos_friend",
                trustChange = 20,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "朋友", nextId = "END", trustChange = 15 }
                }
            },
            new DialogueNode {
                id = "vos_choice_future",
                speaker = "vos",
                content = "「如果黑雾散去...」\n他的眼神里有了光：「我想重新开始。建造一个新的商会。」\n\n「不是为了赚钱。是为了证明...商人也可以帮助别人。」\n\n「而不是只会趁火打劫。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "我支持你", nextId = "END", trustChange = 10 },
                    new DialogueChoice { text = "你已经在改变了", nextId = "vos_change", trustChange = 15 }
                }
            },
            new DialogueNode {
                id = "vos_change",
                speaker = "vos",
                content = "「...是吗？」\n他愣了一下，然后笑了：「也许吧。」\n\n「以前的我绝对不会做赔本买卖。但现在...看到别人因为我的东西活下来。」\n\n「这种感觉...比以前赚钱的时候还好。」",
                trustChange = 18,
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "这就是成长", nextId = "END", trustChange = 15 }
                }
            },

            // ========== 通用 END ==========
            new DialogueNode {
                id = "END",
                speaker = "narrator",
                content = "",
                isEnding = true,
                choices = new List<DialogueChoice>()
            }
        };
    }

    // ====================
    // 对话入口
    // ====================

    /// <summary>
    /// 开始与NPC对话
    /// </summary>
    public void StartDialogue(string npcId, string startNodeId = null)
    {
        currentNpcId = npcId;
        dialogueHistory.Clear();
        dialogueDepth = 0;

        // 查找起始节点
        string startId = startNodeId ?? (npcId + "_greet");
        currentNode = allDialogues.Find(d => d.id == startId);

        if (currentNode == null)
        {
            // Fallback: 通用问候
            currentNode = new DialogueNode
            {
                id = "generic_greet",
                speaker = npcId,
                content = $"「你好，旅人。」",
                choices = new List<DialogueChoice> {
                    new DialogueChoice { text = "再见", nextId = "END" }
                }
            };
        }

        AdvanceToNode(currentNode);
    }

    void AdvanceToNode(DialogueNode node)
    {
        if (node == null || node.isEnding)
        {
            EndDialogue();
            return;
        }

        currentNode = node;
        currentChoices = new List<DialogueChoice>();

        // 记录历史
        dialogueHistory.Add($"[{node.speaker}] {node.content}");

        // 记录到关系系统
        if (node.speaker != "narrator" && node.speaker != currentNpcId)
        {
            var rel = FindObjectOfType<RelationshipSystem>();
            if (rel != null)
                rel.RecordInteraction(node.speaker, RelationshipSystem.InteractionType.Conversation);
        }

        // 输出到游戏日志
        if (GameManager.instance != null)
        {
            GameManager.instance.AddLog($"═══ 对话：{node.speaker} ═══");
            foreach (var line in node.content.Split('\n'))
            {
                if (!string.IsNullOrWhiteSpace(line))
                    GameManager.instance.AddLog(line);
            }

            // 显示选项
            for (int i = 0; i < node.choices.Count; i++)
            {
                var choice = node.choices[i];
                // 检查信任要求
                bool canSelect = true;
                if (choice.minTrust > 0)
                {
                    var rel = FindObjectOfType<RelationshipSystem>();
                    if (rel != null && rel.relationships.ContainsKey(node.speaker))
                        canSelect = rel.relationships[node.speaker].trustLevel >= choice.minTrust;
                }

                if (canSelect)
                    GameManager.instance.AddLog($"  [{i + 1}] {choice.text}");
                else
                    GameManager.instance.AddLog($"  [{i + 1}] {choice.text}（需要 {choice.minTrust} 好感）");
            }
        }

        // 触发战斗
        if (node.isCombat)
        {
            GameManager.instance?.AddLog("⚔️ 触发战斗！");
            // 触发一场战斗
        }

        // 触发标志
        if (!string.IsNullOrEmpty(node.flag))
        {
            GameManager.instance?.AddLog($"📜 获得线索：{node.flag}");
        }

        dialogueDepth++;
    }

    /// <summary>
    /// 玩家选择选项
    /// </summary>
    public void SelectChoice(int choiceIndex)
    {
        if (currentNode == null || choiceIndex >= currentNode.choices.Count)
            return;

        var choice = currentNode.choices[choiceIndex];

        // 记录选择的对话
        dialogueHistory.Add($"[你] {choice.text}");

        // 应用好感变化
        if (choice.trustChange != 0 && currentNode.speaker != "narrator")
        {
            var rel = FindObjectOfType<RelationshipSystem>();
            if (rel != null)
            {
                rel.RecordInteraction(currentNode.speaker,
                    choice.trustChange > 0 ? RelationshipSystem.InteractionType.Conversation : RelationshipSystem.InteractionType.FailedToHelp,
                    $"选择：{choice.text}");
            }
        }

        // 应用世界状态标志
        if (!string.IsNullOrEmpty(choice.flag))
        {
            // 标志会在后续对话中检查
        }

        // 跳转到下一个节点
        if (choice.nextId == "END")
        {
            EndDialogue();
        }
        else
        {
            var nextNode = allDialogues.Find(d => d.id == choice.nextId);
            AdvanceToNode(nextNode);
        }
    }

    void EndDialogue()
    {
        if (GameManager.instance != null)
            GameManager.instance.AddLog("═══ 对话结束 ═══");

        currentNode = null;
        currentChoices = null;
    }

    /// <summary>
    /// 检查是否有特定标志（用于影响其他对话）
    /// </summary>
    public bool HasFlag(string flag)
    {
        foreach (var node in allDialogues)
        {
            if (node.flag == flag) return true;
        }
        return false;
    }

    // ====================
    // 数据类
    // ====================

    [System.Serializable]
    public class DialogueNode
    {
        public string id;
        public string speaker;       // npcId / "narrator"
        public string content;       // 对话内容
        public string flag;         // 触发后设置的标志
        public string secret;        // NPC透露的秘密
        public bool isEnding;       // 是否结束对话
        public bool isCombat;        // 是否触发战斗
        public List<DialogueChoice> choices;
        public int trustChange;      // 默认信任变化
    }

    [System.Serializable]
    public class DialogueChoice
    {
        public string id;
        public string text;          // 选项文本
        public string nextId;        // 跳转节点ID
        public int minTrust = -999;   // 最低信任要求（-999表示无限制）
        public float trustChange = 0; // 选择后对NPC的信任变化
        public string flag;           // 选择后设置的标志
        public string[] requiresFlag; // 需要特定标志才能显示
    }
}
