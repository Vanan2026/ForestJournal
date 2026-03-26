using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// 手账系统 - 森林手账核心
/// 自动记录游戏中的重要事件，生成手账页面
/// </summary>
public class JournalSystem : MonoBehaviour
{
    public static JournalSystem instance { get; private set; }

    [Header("手账配置")]
    public int maxMemoryFragments = 20;  // 收集20个碎片
    public int memoryFragments = 0;

    [Header("手账内容")]
    public List<JournalEntry> entries = new List<JournalEntry>();
    public List<string> unlockedChapters = new List<string>();

    [Header("UI引用")]
    public GameObject journalPanel;
    public Text journalTitleText;
    public Text journalContentText;
    public Text memoryFragmentsText;
    public Button closeJournalBtn;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        CreateJournalPanel();
        AddEntry("prologue", "序章", "我从迷雾中醒来。\n这里是被黑雾笼罩的古老森林。\n找到森林之心，是活下去的唯一希望。", "prologue");
        
        if (journalPanel != null)
            journalPanel.SetActive(false);
    }

    // ====================
    // 核心API - 其他系统调用这些方法记录事件
    // ====================

    /// <summary>
    /// 记录任意事件（其他系统调用）
    /// </summary>
    public void Record(string category, string title, string content)
    {
        AddEntry(category, title, content);
        
        // 记忆碎片
        if (category == "combat_victory" || category == "discovery" || category == "recruitment")
        {
            memoryFragments++;
            UpdateMemoryUI();
        }
    }

    /// <summary>
    /// 战斗胜利后调用
    /// </summary>
    public void OnCombatVictory(string enemyName, int foodReward, int soulReward)
    {
        AddEntry("combat_victory", $"击败 {enemyName}", $"在这片森林中，我们与{enemyName}战斗并取得了胜利。\n获得 {foodReward} 食物，{soulReward} 魂精华。");
        memoryFragments++;
        UpdateMemoryUI();
        CheckChapterUnlock();
    }

    /// <summary>
    /// 成员招募后调用
    /// </summary>
    public void OnMemberRecruited(string memberName, string fromWhere)
    {
        AddEntry("recruitment", $"新成员：{memberName}", $"在{fromWhere}，我们遇到了{memberName}。\n他/她决定加入我们的队伍。");
        memoryFragments++;
        UpdateMemoryUI();
        CheckChapterUnlock();
    }

    /// <summary>
    /// 发现新区域后调用
    /// </summary>
    public void OnRegionDiscovered(string regionName, string description)
    {
        AddEntry("discovery", $"发现：{regionName}", $"穿过迷雾，我们来到了{regionName}。\n{description}");
        memoryFragments++;
        UpdateMemoryUI();
        CheckChapterUnlock();
    }

    /// <summary>
    /// 成员重伤/死亡后调用
    /// </summary>
    public void OnMemberInjured(string memberName, int hpLost)
    {
        AddEntry("danger", $"危险：{memberName}受伤", $"{memberName}在战斗中受了重伤。\n失去 {hpLost} 点生命。");
    }

    /// <summary>
    /// 成员死亡后调用
    /// </summary>
    public void OnMemberDeath(string memberName)
    {
        AddEntry("death", $"永别：{memberName}", $"夜幕降临时，{memberName}永远地闭上了眼睛。\n他/她的记忆将与我们同在。");
    }

    /// <summary>
    /// 威胁升级后调用
    /// </summary>
    public void OnThreatIncrease(int newLevel)
    {
        string[] threatNames = { "", "安全", "较安全", "危险", "很危险", "禁区" };
        string threatName = newLevel <= 5 ? threatNames[newLevel] : "禁区";
        AddEntry("threat", $"威胁升级：{threatName}", $"黑雾越来越浓，威胁等级已升至 {newLevel}。\n这片区域正在变得不再安全。");
    }

    /// <summary>
    /// 升级后调用
    /// </summary>
    public void OnLevelUp(string memberName, int newLevel)
    {
        AddEntry("progress", $"{memberName}升级", $"{memberName}变得更强了！\n当前等级：{newLevel}");
        memoryFragments++;
        UpdateMemoryUI();
    }

    /// <summary>
    /// 森林之心结局触发
    /// </summary>
    public void OnForestHeartReached(string endingType)
    {
        AddEntry("ending", "森林之心", $"我们终于找到了森林之心。\n{endingType}");
        memoryFragments += 5;
        UpdateMemoryUI();
    }

    // ====================
    // 手账条目管理
    // ====================

    void AddEntry(string category, string title, string content, string chapterLock = null)
    {
        var entry = new JournalEntry
        {
            day = GameManager.instance != null ? GameManager.instance.currentDay : 1,
            category = category,
            title = title,
            content = content,
            chapterLock = chapterLock
        };

        entries.Add(entry);
        Debug.Log($"[Journal] 新记录：{title}");

        // 刷新UI
        if (journalPanel != null && journalPanel.activeSelf)
            UpdateJournalDisplay();
    }

    /// <summary>
    /// 检查章节解锁
    /// </summary>
    void CheckChapterUnlock()
    {
        if (GameManager.instance == null) return;
        int day = GameManager.instance.currentDay;

        // SPEC定义的章节解锁
        if (day >= 1 && !unlockedChapters.Contains("chapter1"))
        {
            unlockedChapters.Add("chapter1");
            AddEntry("chapter", "第一章：森林的危险", "黑雾笼罩的森林中，危险无处不在。\n我们需要找到一条通往森林之心的路。", "chapter1");
        }
        if (day >= 4 && !unlockedChapters.Contains("chapter2"))
        {
            unlockedChapters.Add("chapter2");
            AddEntry("chapter", "第二章：黑雾的踪迹", "黑雾并非自然现象。\n在这片森林深处，有什么东西在注视着着我们...", "chapter2");
        }
        if (day >= 8 && !unlockedChapters.Contains("chapter3"))
        {
            unlockedChapters.Add("chapter3");
            AddEntry("chapter", "第三章：森林的意志", "古老的树木似乎有自己的意志。\n森林之心在召唤我们。", "chapter3");
        }
        if (day >= 15 && !unlockedChapters.Contains("chapter4"))
        {
            unlockedChapters.Add("chapter4");
            AddEntry("chapter", "第四章：抉择之路", "每一步都是选择。\n我们必须决定自己的命运。", "chapter4");
        }
    }

    // ====================
    // UI创建与显示
    // ====================

    void CreateJournalPanel()
    {
        // 创建Canvas内手账面板
        var canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null) return;

        // 主面板（半透明黑色背景）
        journalPanel = new GameObject("JournalPanel");
        journalPanel.transform.SetParent(canvasObj.transform, false);
        journalPanel.AddComponent<RectTransform>();
        journalPanel.SetActive(false);

        var panel = journalPanel.AddComponent<Image>();
        panel.color = new Color(0.1f, 0.08f, 0.06f, 0.95f);  // 深棕色背景

        var rt = journalPanel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        // 标题
        var titleObj = new GameObject("JournalTitle");
        titleObj.transform.SetParent(journalPanel.transform, false);
        var titleTxt = titleObj.AddComponent<Text>();
        titleTxt.text = "✦ 森 林 手 账 ✦";
        titleTxt.fontSize = 28;
        titleTxt.color = new Color(0.85f, 0.75f, 0.55f);
        titleTxt.alignment = TextAnchor.MiddleCenter;
        var titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 0.85f);
        titleRt.anchorMax = new Vector2(1, 0.95f);
        titleRt.sizeDelta = new Vector2(0, 0);

        // 记忆碎片显示
        var memObj = new GameObject("MemoryFragments");
        memObj.transform.SetParent(journalPanel.transform, false);
        memoryFragmentsText = memObj.AddComponent<Text>();
        memoryFragmentsText.text = $"记忆碎片：{memoryFragments} / {maxMemoryFragments}";
        memoryFragmentsText.fontSize = 16;
        memoryFragmentsText.color = new Color(0.6f, 0.8f, 0.6f);
        memoryFragmentsText.alignment = TextAnchor.MiddleRight;
        var memRt = memObj.GetComponent<RectTransform>();
        memRt.anchorMin = new Vector2(0.6f, 0.78f);
        memRt.anchorMax = new Vector2(0.95f, 0.85f);
        memRt.sizeDelta = new Vector2(0, 0);

        // 内容区域（ScrollView 内容）
        var contentObj = new GameObject("JournalContent");
        contentObj.transform.SetParent(journalPanel.transform, false);
        var contentTxt = contentObj.AddComponent<Text>();
        contentTxt.text = "";
        contentTxt.fontSize = 14;
        contentTxt.color = new Color(0.9f, 0.85f, 0.75f);
        contentTxt.alignment = TextAnchor.UpperLeft;
        var contentRt = contentObj.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0.05f, 0.05f);
        contentRt.anchorMax = new Vector2(0.95f, 0.77f);
        contentRt.sizeDelta = new Vector2(0, 0);
        journalContentText = contentTxt;

        // 关闭按钮
        var closeBtnObj = new GameObject("CloseJournalBtn");
        closeBtnObj.transform.SetParent(journalPanel.transform, false);
        var closeBtn = closeBtnObj.AddComponent<Button>();
        var closeBtnImg = closeBtnObj.AddComponent<Image>();
        closeBtnImg.color = new Color(0.4f, 0.3f, 0.2f, 0.8f);
        closeBtn.targetGraphic = closeBtnImg;
        var closeBtnTxt = closeBtnObj.AddComponent<Text>();
        closeBtnTxt.text = "关闭";
        closeBtnTxt.fontSize = 16;
        closeBtnTxt.color = Color.white;
        closeBtnTxt.alignment = TextAnchor.MiddleCenter;
        closeBtn.onClick.AddListener(() => HideJournal());
        var closeBtnRt = closeBtnObj.GetComponent<RectTransform>();
        closeBtnRt.anchorMin = new Vector2(0.35f, 0.01f);
        closeBtnRt.anchorMax = new Vector2(0.65f, 0.04f);
        closeBtnRt.sizeDelta = new Vector2(0, 0);

        UpdateJournalDisplay();
        Debug.Log("[Journal] 手账系统初始化完成");
    }

    /// <summary>
    /// 显示手账
    /// </summary>
    public void ShowJournal()
    {
        if (journalPanel == null) CreateJournalPanel();
        journalPanel.SetActive(true);
        UpdateJournalDisplay();
        Time.timeScale = 0;  // 暂停游戏
    }

    /// <summary>
    /// 隐藏手账
    /// </summary>
    public void HideJournal()
    {
        if (journalPanel != null)
            journalPanel.SetActive(false);
        Time.timeScale = 1;  // 恢复游戏
    }

    /// <summary>
    /// 刷新手账内容显示
    /// </summary>
    void UpdateJournalDisplay()
    {
        if (journalContentText == null) return;

        // 按日期分组显示
        string content = "";
        int currentDay = -1;

        foreach (var entry in entries)
        {
            if (entry.day != currentDay)
            {
                currentDay = entry.day;
                content += $"\n━━━━━━━━━━ 第 {currentDay} 天 ━━━━━━━━━━\n";
            }

            string icon = GetCategoryIcon(entry.category);
            content += $"\n{icon} {entry.title}\n{entry.content}\n";
        }

        journalContentText.text = content;

        if (memoryFragmentsText != null)
            memoryFragmentsText.text = $"记忆碎片：{memoryFragments} / {maxMemoryFragments}";
    }

    string GetCategoryIcon(string category)
    {
        switch (category)
        {
            case "combat_victory": return "⚔️";
            case "death": return "💀";
            case "recruitment": return "🤝";
            case "discovery": return "🗺️";
            case "threat": return "⚠️";
            case "danger": return "🩸";
            case "progress": return "⬆️";
            case "chapter": return "📖";
            case "ending": return "✨";
            case "prologue": return "🌲";
            default: return "•";
        }
    }

    void UpdateMemoryUI()
    {
        if (memoryFragmentsText != null)
            memoryFragmentsText.text = $"记忆碎片：{memoryFragments} / {maxMemoryFragments}";
    }

    // ====================
    // 数据类
    // ====================

    [System.Serializable]
    public class JournalEntry
    {
        public int day;
        public string category;
        public string title;
        public string content;
        public string chapterLock;  // 如果非空，需要解锁才能显示
    }
}
