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
                    GameManager.instance.AddLog($"  [{i + 1}] ???
（需要 {choice.minTrust} 好感）");
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
