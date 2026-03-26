using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// 动态叙事系统 v1.0
/// 关键抉择 + 夜晚事件 + 记忆觉醒 + 章节解锁 + 叙事触发点
/// </summary>
public class DynamicNarrativeSystem : MonoBehaviour
{
    public static DynamicNarrativeSystem instance { get; private set; }

    [Header("章节状态")]
    public int currentChapter = 0;  // 0=序章, 1-4=章节, 5=终章
    public bool chapter1Unlocked = false;
    public bool chapter2Unlocked = false;
    public bool chapter3Unlocked = false;
    public bool chapter4Unlocked = false;
    public bool memoryAwakeningTriggered = false;

    [Header("记忆觉醒")]
    public int memoryAwakeningThreshold = 10;  // 满10碎片触发记忆觉醒
    public bool memoryAwakeningActive = false;

    [Header("关键抉择")]
    public List<NarrativeChoice> activeChoices = new List<NarrativeChoice>();

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // 序章自动解锁
        UnlockChapter(0);
    }

    // ====================
    // 章节解锁（Day触发）
    // ====================

    /// <summary>
    /// 检查章节解锁（每日调用）
    /// </summary>
    public void CheckChapterUnlocks(int day)
    {
        if (day >= 1 && !chapter1Unlocked)
        {
            UnlockChapter(1);
            AddNarrativeEvent("chapter", "第一章：森林的危险",
                "你们在迷雾森林中醒来。黑雾笼罩着一切...\n\n" +
                "「这片森林曾经是宁静的...直到黑雾降临。」\n\n" +
                "你们决定前往森林深处，寻找黑雾的源头。");
        }

        if (day >= 4 && !chapter2Unlocked)
        {
            UnlockChapter(2);
            AddNarrativeEvent("chapter", "第二章：黑雾的踪迹",
                "黑雾并非自然现象。\n\n" +
                "在森林深处，你们发现了一些古老的符文... \n" +
                "这些符文似乎是某种封印的痕迹。\n\n" +
                "「有人曾经试图封印这片森林...但封印已经松动。」");
        }

        if (day >= 8 && !chapter3Unlocked)
        {
            UnlockChapter(3);
            AddNarrativeEvent("chapter", "第三章：森林的意志",
                "古老的树木似乎有自己的意志。\n\n" +
                "当你们穿过古树之森时，树木为你们让开了道路...\n\n" +
                "「森林在等待它的守护者。」\n\n" +
                "你们开始理解，森林之心不只是一个地点，更是一种意志。");
        }

        if (day >= 15 && !chapter4Unlocked)
        {
            UnlockChapter(4);
            AddNarrativeEvent("chapter", "第四章：抉择之路",
                "每一步都是选择。\n\n" +
                "黑雾正在逼近，时间不多了。\n\n" +
                "你们必须做出选择：\n" +
                "是融入森林，还是驱散黑雾？\n" +
                "是带走秘密，还是永远守护？\n\n" +
                "无论选择什么，都会影响最终的结局。");
        }
    }

    void UnlockChapter(int chapter)
    {
        currentChapter = chapter;
        switch (chapter)
        {
            case 0: break; // 序章
            case 1: chapter1Unlocked = true; break;
            case 2: chapter2Unlocked = true; break;
            case 3: chapter3Unlocked = true; break;
            case 4: chapter4Unlocked = true; break;
        }

        // 记录到手账
        if (JournalSystem.instance != null)
        {
            string[] chapterTitles = { "序章", "第一章：森林的危险", "第二章：黑雾的踪迹",
                "第三章：森林的意志", "第四章：抉择之路", "终章" };
            string title = chapter < chapterTitles.Length ? chapterTitles[chapter] : "终章";
            JournalSystem.instance.Record("chapter", title, $"章节已解锁：{title}");
        }

        Debug.Log($"[Narrative] 章节解锁：{chapter}");
    }

    // ====================
    // 夜晚事件（15%无事，否则触发随机事件）
    // ====================

    /// <summary>
    /// 夜晚安营时调用（安营流程的一部分）
    /// </summary>
    public void OnNightCamp()
    {
        if (GameManager.instance == null) return;

        float roll = UnityEngine.Random.value;

        if (roll < 0.15f)
        {
            // 15%无事
            AddNarrativeEvent("camp", "营地平安", "篝火噼啪作响，你们在星光下安然入睡...");
            return;
        }

        // 触发随机夜晚事件
        TriggerRandomNightEvent();
    }

    void TriggerRandomNightEvent()
    {
        if (GameManager.instance == null) return;

        var events = new List<(string title, string content, Action effect)>();

        events.Add(("🌙 森林低语", "夜深人静时，你们听到远处传来低沉的吟唱声...", () =>
        {
            GameManager.instance.memories += 1;
            GameManager.instance.AddLog("✨ 记忆碎片 +1（森林的低语）");
        }));

        events.Add(("🔥 篝火异常", "篝火突然剧烈跳动，映照出奇怪的影子...", () =>
        {
            int heal = UnityEngine.Random.Range(5, 15);
            foreach (var m in GameManager.instance.squad)
            {
                if (m.currentHealth > 0)
                    m.currentHealth = Mathf.Min(m.currentHealth + heal, m.maxHealth);
            }
            GameManager.instance.AddLog($"💚 篝火治疗：全员恢复 {heal} HP");
        }));

        events.Add(("👤 神秘访客", "深夜，有人悄悄靠近营地...", () =>
        {
            // 随机获得资源
            string[] resources = { "食物", "木材", "草药" };
            string res = resources[UnityEngine.Random.Range(0, resources.Length)];
            int amount = UnityEngine.Random.Range(1, 4);
            switch (res)
            {
                case "食物": GameManager.instance.food += amount; break;
                case "木材": GameManager.instance.wood += amount; break;
                case "草药": GameManager.instance.herb += amount; break;
            }
            GameManager.instance.AddLog($"神秘访客留下了 {amount} {res}");
        }));

        events.Add(("⚠️ 黑雾涌动", "黑雾在夜间变得更加浓密...", () =>
        {
            GameManager.instance.AddLog("黑雾涌动，威胁等级暂时上升！");
            GameManager.instance.threatLevel = Mathf.Min(
                GameManager.instance.threatLevel + 1, GameManager.instance.maxThreat);
        }));

        events.Add(("🦌 森林生物", "一只发光的生物从树丛中走出...", () =>
        {
            int food = UnityEngine.Random.Range(2, 6);
            GameManager.instance.food += food;
            GameManager.instance.AddLog($"它留下了 {food} 食物，然后消失在雾中...");
        }));

        events.Add(("💭 噩梦", "你梦见自己被黑雾吞噬...", () =>
        {
            if (GameManager.instance.selectedMember != null)
            {
                int dmg = UnityEngine.Random.Range(5, 15);
                GameManager.instance.selectedMember.currentHealth -= dmg;
                GameManager.instance.AddLog($"💀 {GameManager.instance.selectedMember.memberName} 被噩梦困扰 -{dmg} HP");
            }
        }));

        var chosen = events[UnityEngine.Random.Range(0, events.Count)];
        AddNarrativeEvent("camp", chosen.title, chosen.content);
        chosen.effect?.Invoke();
    }

    // ====================
    // 记忆觉醒（碎片满了自动触发）
    // ====================

    /// <summary>
    /// 检查记忆觉醒
    /// </summary>
    public void CheckMemoryAwakening(int memories)
    {
        if (memoryAwakeningTriggered) return;
        if (memories >= memoryAwakeningThreshold)
        {
            TriggerMemoryAwakening();
        }
    }

    void TriggerMemoryAwakening()
    {
        memoryAwakeningTriggered = true;
        memoryAwakeningActive = true;

        AddNarrativeEvent("memory", "✨ 记忆觉醒 ✨",
            "记忆碎片在你们心中汇聚成河...\n\n" +
            "曾经模糊的画面变得清晰：\n" +
            "这片森林曾经是一位守护者的化身。\n" +
            "而黑雾，是失去守护者后森林的哀伤。\n\n" +
            "你们终于明白——\n" +
            "只有找到森林之心，才能让这片土地重获新生。\n\n" +
            "（获得感知威胁的能力）");

        if (JournalSystem.instance != null)
        {
            JournalSystem.instance.Record("memory_awakening", "记忆觉醒",
                "记忆觉醒发生后，你们获得了感知森林意志的能力。\n" +
                "现在你们可以感知到威胁的来源和强度。");
        }

        // 给予玩家一个永久buff提示（这里用日志表示）
        if (GameManager.instance != null)
        {
            GameManager.instance.AddLog("✨【记忆觉醒】已触发！");
            GameManager.instance.AddLog("你们感知到了森林的意志...");
        }
    }

    // ====================
    // 关键抉择（特殊场景触发）
    // ====================

    /// <summary>
    /// 触发关键抉择点（特定区域/事件调用）
    /// </summary>
    public void TriggerChoice(string choiceId, string title, string description, ChoiceOption[] options)
    {
        if (JournalSystem.instance != null)
        {
            // 记录抉择
            JournalSystem.instance.Record("choice", $"抉择：{title}", description);
        }

        var choice = new NarrativeChoice
        {
            choiceId = choiceId,
            title = title,
            description = description,
            options = new List<ChoiceOption>(options),
            triggered = true
        };

        activeChoices.Add(choice);

        GameManager.instance?.AddLog($"═══ {title} ═══");
        GameManager.instance?.AddLog(description);

        for (int i = 0; i < options.Length; i++)
        {
            GameManager.instance?.AddLog($"  [{i + 1}] {options[i].text}");
        }

        Debug.Log($"[Narrative] 触发抉择：{title}");
    }

    /// <summary>
    /// 玩家做出选择
    /// </summary>
    public void MakeChoice(string choiceId, int optionIndex)
    {
        var choice = activeChoices.Find(c => c.choiceId == choiceId);
        if (choice == null || optionIndex >= choice.options.Count) return;

        var option = choice.options[optionIndex];
        choice.selectedIndex = optionIndex;
        choice.resolved = true;

        GameManager.instance?.AddLog($"你选择了：{option.text}");
        option.effect?.Invoke();

        // 记录到日志
        if (JournalSystem.instance != null)
        {
            JournalSystem.instance.Record("choice_made", $"抉择结果：{choice.title}",
                $"选择了「{option.text}」\n{option.description}");
        }

        activeChoices.Remove(choice);
    }

    // ====================
    // 发现事件（探索时随机触发）
    // ====================

    /// <summary>
    /// 探索时触发发现事件
    /// </summary>
    public void TriggerDiscoveryEvent()
    {
        if (GameManager.instance == null) return;
        if (UnityEngine.Random.value > 0.3f) return; // 30%概率

        var discoveries = new List<(string title, string content, Action effect)>
        {
            ("📜 古老的石碑", "你们发现了一块刻满符文的石碑...", () =>
            {
                GameManager.instance.memories += 2;
                GameManager.instance.AddLog("✨ 记忆碎片 +2（石碑的秘密）");
            }),

            ("🏚️ 废弃营地", "这里曾经有人类居住...", () =>
            {
                GameManager.instance.food += UnityEngine.Random.Range(2, 5);
                GameManager.instance.wood += UnityEngine.Random.Range(1, 4);
                GameManager.instance.AddLog("在废弃营地找到了补给！");
            }),

            ("🌿 草药丛", "一大片野生草药...", () =>
            {
                GameManager.instance.herb += UnityEngine.Random.Range(2, 5);
                GameManager.instance.AddLog("采集了草药！");
            }),

            ("💀 动物骨骸", "森林中散落着奇怪的骨骸...", () =>
            {
                GameManager.instance.bone += UnityEngine.Random.Range(1, 3);
                GameManager.instance.AddLog("收集了兽骨！");
                if (UnityEngine.Random.value < 0.3f)
                {
                    GameManager.instance.AddLog("这些骨骸似乎不是普通动物的...");
                    GameManager.instance.memories += 1;
                }
            }),

            ("✨ 魂精华", "黑雾中闪烁着微弱的光芒...", () =>
            {
                GameManager.instance.soulEssence += 1;
                GameManager.instance.AddLog("✨ 获得 1 魂精华！");
            })
        };

        var d = discoveries[UnityEngine.Random.Range(0, discoveries.Count)];
        AddNarrativeEvent("discovery", d.title, d.content);
        d.effect?.Invoke();
    }

    // ====================
    // 威胁增长（每3天+1）
    // ====================

    /// <summary>
    /// 检查威胁增长（每日调用）
    /// </summary>
    public void CheckThreatIncrease(int day)
    {
        if (GameManager.instance == null) return;
        if (day <= 1) return;

        // 每3天当前区域威胁+1
        int increases = (day - 1) / 3;
        int targetThreat = Mathf.Min(increases + 1, 5);

        if (GameManager.instance.threatLevel < targetThreat)
        {
            GameManager.instance.threatLevel = targetThreat;
            GameManager.instance.AddLog($"⚠️ 黑雾越来越浓，威胁等级升至 {targetThreat}！");

            if (JournalSystem.instance != null)
                JournalSystem.instance.Record("threat", $"威胁升级：{targetThreat}",
                    $"第 {day} 天，黑雾浓度持续上升，威胁等级已达 {targetThreat}。");
        }
    }

    // ====================
    // UI 显示
    // ====================

    void AddNarrativeEvent(string category, string title, string content)
    {
        if (JournalSystem.instance != null)
            JournalSystem.instance.Record(category, title, content);

        GameManager.instance?.AddLog($"═══ {title} ═══");
        foreach (var line in content.Split('\n'))
        {
            if (!string.IsNullOrWhiteSpace(line))
                GameManager.instance?.AddLog(line);
        }
    }

    // ====================
    // 数据类
    // ====================

    [System.Serializable]
    public class NarrativeChoice
    {
        public string choiceId;
        public string title;
        public string description;
        public List<ChoiceOption> options;
        public int selectedIndex = -1;
        public bool triggered = false;
        public bool resolved = false;
    }

    [System.Serializable]
    public class ChoiceOption
    {
        public string text;
        public string description;
        public Action effect;

        public ChoiceOption(string t, string d, Action e)
        {
            text = t;
            description = d;
            effect = e;
        }
    }
}
