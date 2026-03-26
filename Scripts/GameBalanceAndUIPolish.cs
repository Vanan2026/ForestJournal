using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// 游戏数值平衡调优 + UI界面整合系统 v1.0
/// Part A: 数值平衡 - 确保前期有趣/中期成长/后期挑战
/// Part B: UI整合 - 背包/成就/商店/农场/天气/日程面板
/// </summary>
public class GameBalanceAndUIPolish : MonoBehaviour
{
    public static GameBalanceAndUIPolish instance { get; private set; }

    [Header("=== Part A: 数值平衡 ===")]
    public BalanceData balance;

    [Header("=== Part B: UI界面 ===")]
    public Button btnInventory;
    public Button btnAchievement;
    public Button btnShop;
    public Button btnFarm;
    public Button btnWeather;
    public Button btnSchedule;

    public GameObject inventoryPanel;
    public GameObject achievementPanel;
    public GameObject shopPanel;
    public GameObject farmPanel;
    public GameObject weatherPanel;
    public GameObject schedulePanel;

    private GameObject currentOpenPanel;

    void Awake()
    {
        instance = this;
        balance = new BalanceData();
    }

    void Start()
    {
        CreateMainUIButtons();
        Debug.Log("[GameBalanceAndUIPolish] 数值平衡+UI界面整合初始化完成");
    }

    // ========== PART A: 数值平衡调优 ==========

    public float GetGatherMultiplier(int currentDay)
    {
        if (currentDay <= 5) return 1.3f;
        if (currentDay <= 15) return 1.0f;
        if (currentDay <= 25) return 0.85f;
        return 0.7f;
    }

    public int GetGatherAmount(int baseAmount, int currentDay)
    {
        return Mathf.RoundToInt(baseAmount * GetGatherMultiplier(currentDay));
    }

    public float GetEnemyStatMultiplier(int threatLevel)
    {
        switch (threatLevel)
        {
            case 1: return 1.00f;
            case 2: return 1.15f;
            case 3: return 1.35f;
            case 4: return 1.60f;
            case 5: return 2.00f;
            default: return threatLevel <= 0 ? 1.0f : 2.0f;
        }
    }

    public EnemyDropTable GetEnemyDropTable(int threatLevel)
    {
        var drop = new EnemyDropTable();
        switch (threatLevel)
        {
            case 1: drop.foodMin=3; drop.foodMax=5; drop.soulEssenceMin=0; drop.soulEssenceMax=1; break;
            case 2: drop.foodMin=5; drop.foodMax=8; drop.soulEssenceMin=1; drop.soulEssenceMax=2; break;
            case 3: drop.foodMin=8; drop.foodMax=12; drop.soulEssenceMin=2; drop.soulEssenceMax=4; break;
            case 4: drop.foodMin=12; drop.foodMax=18; drop.soulEssenceMin=4; drop.soulEssenceMax=6; break;
            default: drop.foodMin=20; drop.foodMax=30; drop.soulEssenceMin=8; drop.soulEssenceMax=12; break;
        }
        return drop;
    }

    public (int food, int soulEssence) RollEnemyDrop(int threatLevel)
    {
        var table = GetEnemyDropTable(threatLevel);
        int food = UnityEngine.Random.Range(table.foodMin, table.foodMax + 1);
        int soul = UnityEngine.Random.Range(table.soulEssenceMin, table.soulEssenceMax + 1);
        return (food, soul);
    }

    public int GetQuestMemoryReward(QuestDifficulty difficulty)
    {
        switch (difficulty)
        {
            case QuestDifficulty.Easy:   return UnityEngine.Random.Range(1, 3);
            case QuestDifficulty.Medium:  return UnityEngine.Random.Range(2, 5);
            case QuestDifficulty.Hard:    return UnityEngine.Random.Range(4, 7);
            case QuestDifficulty.Boss:    return UnityEngine.Random.Range(8, 13);
            default: return 1;
        }
    }

    public bool IsSkillUnlocked(string skillId, int currentDay)
    {
        switch (skillId)
        {
            case "gather_basic":    return currentDay >= 1;
            case "herbalism":       return currentDay >= 3;
            case "hunting":         return currentDay >= 5;
            case "tracking":        return currentDay >= 8;
            case "woodcutting":     return currentDay >= 12;
            case "advanced_gather": return currentDay >= 18;
            case "advanced_combat": return currentDay >= 18;
            case "craft_master":    return currentDay >= 20;
            default: return false;
        }
    }

    public int GetSkillUnlockDay(string skillId)
    {
        switch (skillId)
        {
            case "gather_basic":    return 1;
            case "herbalism":       return 3;
            case "hunting":         return 5;
            case "tracking":        return 8;
            case "woodcutting":     return 12;
            case "advanced_gather": return 18;
            case "advanced_combat": return 18;
            case "craft_master":    return 20;
            default: return 999;
        }
    }

    public float GetEnemyEncounterChanceByRegion(int regionId)
    {
        switch (regionId)
        {
            case 0: return 0.20f;
            case 1: return 0.30f;
            case 2: return 0.35f;
            case 3: return 0.45f;
            case 4: return 0.60f;
            default: return 0.20f;
        }
    }

    public string GetRegionName(int regionId)
    {
        switch (regionId)
        {
            case 0: return "迷雾森林";
            case 1: return "迷雾前沿";
            case 2: return "古老废墟";
            case 3: return "幽暗山谷";
            case 4: return "森林之心";
            default: return "未知区域";
        }
    }

    public void OnNewDay(int currentDay)
    {
        Debug.Log("[Balance] Day " + currentDay + ": 采集x" + GetGatherMultiplier(currentDay).ToString("F2"));
    }

    public List<string> GetUnlockedSkillsList(int currentDay)
    {
        var list = new List<string>();
        string[] allSkills = { "gather_basic", "herbalism", "hunting", "tracking",
                                "woodcutting", "advanced_gather", "advanced_combat", "craft_master" };
        foreach (var s in allSkills)
            if (IsSkillUnlocked(s, currentDay)) list.Add(s);
        return list;
    }

    // ========== PART B: UI界面整合 ==========

    void CreateMainUIButtons()
    {
        var canvas = GetOrCreateCanvas();
        if (canvas == null) return;

        var buttonBar = new GameObject("MainButtonBar");
        buttonBar.transform.SetParent(canvas.transform, false);
        var barRt = buttonBar.AddComponent<RectTransform>();
        barRt.anchorMin = new Vector2(0, 0);
        barRt.anchorMax = new Vector2(1, 0);
        barRt.sizeDelta = new Vector2(0, 80);
        barRt.anchoredPosition = new Vector2(0, 40);
        var barImg = buttonBar.AddComponent<Image>();
        barImg.color = new Color(0.1f, 0.08f, 0.06f, 0.95f);

        string[] btnNames = { "背包", "成就", "商店", "农场", "天气", "日程" };
        Action<GameObject>[] btnActions = {
            delegate(GameObject btn) { OpenPanel(inventoryPanel); },
            delegate(GameObject btn) { OpenPanel(achievementPanel); },
            delegate(GameObject btn) { OpenPanel(shopPanel); },
            delegate(GameObject btn) { OpenPanel(farmPanel); },
            delegate(GameObject btn) { OpenPanel(weatherPanel); },
            delegate(GameObject btn) { OpenPanel(schedulePanel); }
        };

        for (int i = 0; i < btnNames.Length; i++)
        {
            float left = i * (1f / btnNames.Length) + 0.005f;
            float right = (i + 1) * (1f / btnNames.Length) - 0.01f;
            var btnObj = CreateButton(btnNames[i], buttonBar.transform,
                new Vector2(left, 0.1f), new Vector2(right, 0.9f),
                Vector2.zero, Vector2.zero, btnActions[i]);
            var txt = btnObj.GetComponentInChildren<Text>();
            if (txt != null) { txt.text = btnNames[i]; txt.fontSize = 16; }
        }

        CreateInventoryPanel(canvas.transform);
        CreateAchievementPanel(canvas.transform);
        CreateShopPanel(canvas.transform);
        CreateFarmPanel(canvas.transform);
        CreateWeatherPanel(canvas.transform);
        CreateSchedulePanel(canvas.transform);
    }

    GameObject CreateButton(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPosition, Vector2 sizeDelta,
        Action<GameObject> onClick = null)
    {
        var btnObj = new GameObject(name + "_Btn");
        btnObj.transform.SetParent(parent, false);
        var btnRt = btnObj.AddComponent<RectTransform>();
        btnRt.anchorMin = anchorMin;
        btnRt.anchorMax = anchorMax;
        btnRt.anchoredPosition = anchoredPosition;
        btnRt.sizeDelta = sizeDelta;
        btnRt.pivot = new Vector2(0.5f, 0.5f);

        var btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.25f, 0.2f, 0.15f, 0.9f);

        var btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.colors = new ColorBlock()
        {
            normalColor = new Color(0.25f, 0.2f, 0.15f, 0.9f),
            highlightedColor = new Color(0.4f, 0.32f, 0.24f, 1f),
            pressedColor = new Color(0.15f, 0.12f, 0.09f, 1f),
            disabledColor = new Color(0.1f, 0.08f, 0.06f, 0.5f),
            colorMultiplier = 1f
        };

        var txtObj = new GameObject("Text");
        txtObj.transform.SetParent(btnObj.transform, false);
        var txtRt = txtObj.AddComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = Vector2.zero;
        var txt = txtObj.AddComponent<Text>();
        txt.text = name;
        txt.fontSize = 14;
        txt.color = new Color(0.9f, 0.85f, 0.75f);
        txt.alignment = TextAnchor.MiddleCenter;

        if (onClick != null)
            btn.onClick.AddListener(() => onClick(btnObj));
        return btnObj;
    }

    GameObject CreatePanel(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 sizeDelta, Vector2 anchoredPosition,
        float bgAlpha = 0.92f)
    {
        var panel = new GameObject(name + "_Panel");
        panel.transform.SetParent(parent, false);
        var panelRt = panel.AddComponent<RectTransform>();
        panelRt.anchorMin = anchorMin;
        panelRt.anchorMax = anchorMax;
        panelRt.sizeDelta = sizeDelta;
        panelRt.anchoredPosition = anchoredPosition;
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        var panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0.05f, 0.04f, 0.03f, bgAlpha);
        return panel;
    }

    Text CreateText(string name, Transform parent,
        string content, int fontSize, Color color, TextAnchor alignment,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        var txtObj = new GameObject(name + "_Text");
        txtObj.transform.SetParent(parent, false);
        var txtRt = txtObj.AddComponent<RectTransform>();
        txtRt.anchorMin = anchorMin;
        txtRt.anchorMax = anchorMax;
        txtRt.anchoredPosition = anchoredPosition;
        txtRt.sizeDelta = sizeDelta;
        txtRt.pivot = new Vector2(0.5f, 0.5f);
        var txt = txtObj.AddComponent<Text>();
        txt.text = content;
        txt.fontSize = fontSize;
        txt.color = color;
        txt.alignment = alignment;
        return txt;
    }

    Button CreateCloseButton(Transform parent, GameObject panelToClose,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition)
    {
        var btnObj = CreateButton("X", parent,
            anchorMin, anchorMax, anchoredPosition, new Vector2(40, 40),
            delegate (GameObject b) { ClosePanel(panelToClose); });
        return btnObj.GetComponent<Button>();
    }

    Canvas GetOrCreateCanvas()
    {
        var canvas = GameObject.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        return canvas;
    }

    void OpenPanel(GameObject panel)
    {
        if (panel == null) return;
        if (currentOpenPanel != null && currentOpenPanel != panel)
            currentOpenPanel.SetActive(false);
        panel.SetActive(true);
        currentOpenPanel = panel;
    }

    void ClosePanel(GameObject panel)
    {
        if (panel == null) return;
        panel.SetActive(false);
        if (currentOpenPanel == panel)
            currentOpenPanel = null;
    }

    // ===== 1. 背包面板 (InventoryPanel) =====
    void CreateInventoryPanel(Transform parent)
    {
        inventoryPanel = CreatePanel("Inventory", parent,
            new Vector2(0.15f, 0.1f), new Vector2(0.85f, 0.9f),
            Vector2.zero, Vector2.zero);

        CreateText("Title", inventoryPanel.transform,
            "背包", 24, new Color(0.9f, 0.85f, 0.7f), TextAnchor.UpperCenter,
            new Vector2(0.35f, 0.9f), new Vector2(0.65f, 0.98f), Vector2.zero, Vector2.zero);

        CreateCloseButton(inventoryPanel.transform, inventoryPanel,
            new Vector2(0.9f, 0.9f), new Vector2(1f, 0.98f), Vector2.zero);

        var gridBg = new GameObject("Grid_BG");
        gridBg.transform.SetParent(inventoryPanel.transform, false);
        var gridBgRt = gridBg.AddComponent<RectTransform>();
        gridBgRt.anchorMin = new Vector2(0.05f, 0.05f);
        gridBgRt.anchorMax = new Vector2(0.95f, 0.85f);
        gridBgRt.sizeDelta = Vector2.zero;
        var gridBgImg = gridBg.AddComponent<Image>();
        gridBgImg.color = new Color(0.08f, 0.06f, 0.05f, 0.8f);

        int cols = 5, rows = 8;
        float cellW = 80f, cellH = 60f;
        float startX = -180f, startY = 180f;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int idx = r * cols + c;
                var cell = new GameObject("Slot_" + idx);
                cell.transform.SetParent(inventoryPanel.transform, false);
                var cellRt = cell.AddComponent<RectTransform>();
                cellRt.anchorMin = new Vector2(0.5f, 0.5f);
                cellRt.anchorMax = new Vector2(0.5f, 0.5f);
                cellRt.sizeDelta = new Vector2(cellW - 4, cellH - 4);
                cellRt.anchoredPosition = new Vector2(startX + c * cellW, startY - r * cellH);
                var cellImg = cell.AddComponent<Image>();
                cellImg.color = new Color(0.15f, 0.12f, 0.1f, 0.9f);
                cell.AddComponent<Button>();
            }
        }

        CreateButton("整理", inventoryPanel.transform,
            new Vector2(0.05f, 0.02f), new Vector2(0.2f, 0.08f),
            Vector2.zero, Vector2.zero,
            delegate (GameObject b) { Debug.Log("[Inventory] 整理背包"); });

        CreateButton("扩容", inventoryPanel.transform,
            new Vector2(0.22f, 0.02f), new Vector2(0.37f, 0.08f),
            Vector2.zero, Vector2.zero,
            delegate (GameObject b) { Debug.Log("[Inventory] 扩容背包"); });

        inventoryPanel.SetActive(false);
    }

    // ===== 2. 成就面板 (AchievementPanel) =====
    void CreateAchievementPanel(Transform parent)
    {
        achievementPanel = CreatePanel("Achievement", parent,
            new Vector2(0.2f, 0.15f), new Vector2(0.8f, 0.85f),
            Vector2.zero, Vector2.zero);

        CreateText("Title", achievementPanel.transform,
            "成就", 24, new Color(0.9f, 0.85f, 0.7f), TextAnchor.UpperCenter,
            new Vector2(0.35f, 0.9f), new Vector2(0.65f, 0.98f), Vector2.zero, Vector2.zero);

        CreateCloseButton(achievementPanel.transform, achievementPanel,
            new Vector2(0.9f, 0.9f), new Vector2(1f, 0.98f), Vector2.zero);

        string[] categories = { "战斗", "探索", "收集", "故事" };
        for (int i = 0; i < categories.Length; i++)
        {
            float left = 0.08f + i * 0.21f;
            CreateButton(categories[i], achievementPanel.transform,
                new Vector2(left, 0.82f), new Vector2(left + 0.19f, 0.9f),
                Vector2.zero, Vector2.zero,
                delegate (GameObject b) { Debug.Log("[Achievement] 切换分类: " + categories[i]); });
        }

        var listBg = new GameObject("AchievementList_BG");
        listBg.transform.SetParent(achievementPanel.transform, false);
        var listBgRt = listBg.AddComponent<RectTransform>();
        listBgRt.anchorMin = new Vector2(0.05f, 0.05f);
        listBgRt.anchorMax = new Vector2(0.95f, 0.78f);
        listBgRt.sizeDelta = Vector2.zero;
        var listBgImg = listBg.AddComponent<Image>();
        listBgImg.color = new Color(0.08f, 0.06f, 0.05f, 0.8f);

        string[] sampleAchievements = {
            "初次狩猎 - 击败第一只敌人",
            "探索者 - 发现5个新区域",
            "收藏家 - 收集10种材料"
        };
        for (int i = 0; i < sampleAchievements.Length; i++)
        {
            var entry = new GameObject("Achievement_" + i);
            entry.transform.SetParent(achievementPanel.transform, false);
            var entryRt = entry.AddComponent<RectTransform>();
            entryRt.anchorMin = new Vector2(0.05f, 0.7f - i * 0.18f);
            entryRt.anchorMax = new Vector2(0.95f, 0.82f - i * 0.18f);
            entryRt.sizeDelta = Vector2.zero;
            var entryImg = entry.AddComponent<Image>();
            entryImg.color = new Color(0.12f, 0.1f, 0.08f, 0.8f);

            CreateText("Name_" + i, entry.transform,
                sampleAchievements[i], 14, new Color(0.85f, 0.8f, 0.7f), TextAnchor.MiddleLeft,
                new Vector2(0.02f, 0f), new Vector2(0.98f, 1f), Vector2.zero, Vector2.zero);
        }

        CreateText("Progress", achievementPanel.transform,
            "已解锁: 0/20", 14, new Color(0.7f, 0.65f, 0.55f), TextAnchor.LowerRight,
            new Vector2(0.5f, 0.02f), new Vector2(0.98f, 0.06f), Vector2.zero, Vector2.zero);

        achievementPanel.SetActive(false);
    }

    // ===== 3. 商店面板 (ShopPanel) =====
    void CreateShopPanel(Transform parent)
    {
        shopPanel = CreatePanel("Shop", parent,
            new Vector2(0.2f, 0.15f), new Vector2(0.8f, 0.85f),
            Vector2.zero, Vector2.zero);

        CreateText("Title", shopPanel.transform,
            "商店", 24, new Color(0.9f, 0.85f, 0.7f), TextAnchor.UpperCenter,
            new Vector2(0.35f, 0.9f), new Vector2(0.65f, 0.98f), Vector2.zero, Vector2.zero);

        CreateCloseButton(shopPanel.transform, shopPanel,
            new Vector2(0.9f, 0.9f), new Vector2(1f, 0.98f), Vector2.zero);

        string[] merchantTypes = { "杂货商", "武器商", "炼金师" };
        for (int i = 0; i < merchantTypes.Length; i++)
        {
            float left = 0.08f + i * 0.28f;
            CreateButton(merchantTypes[i], shopPanel.transform,
                new Vector2(left, 0.82f), new Vector2(left + 0.25f, 0.9f),
                Vector2.zero, Vector2.zero,
                delegate (GameObject b) { Debug.Log("[Shop] 切换商人: " + merchantTypes[i]); });
        }

        var shopListBg = new GameObject("ShopList_BG");
        shopListBg.transform.SetParent(shopPanel.transform, false);
        var shopListBgRt = shopListBg.AddComponent<RectTransform>();
        shopListBgRt.anchorMin = new Vector2(0.05f, 0.05f);
        shopListBgRt.anchorMax = new Vector2(0.95f, 0.78f);
        shopListBgRt.sizeDelta = Vector2.zero;
        var shopListBgImg = shopListBg.AddComponent<Image>();
        shopListBgImg.color = new Color(0.08f, 0.06f, 0.05f, 0.8f);

        string[] sampleItems = {
            "治疗药水 x1 - 20食物",
            "铁剑 - 50食物",
            "种子袋 - 15食物",
            "魂精华 - 30食物"
        };
        for (int i = 0; i < sampleItems.Length; i++)
        {
            var itemEntry = new GameObject("ShopItem_" + i);
            itemEntry.transform.SetParent(shopPanel.transform, false);
            var itemEntryRt = itemEntry.AddComponent<RectTransform>();
            itemEntryRt.anchorMin = new Vector2(0.05f, 0.65f - i * 0.18f);
            itemEntryRt.anchorMax = new Vector2(0.8f, 0.8f - i * 0.18f);
            itemEntryRt.sizeDelta = Vector2.zero;
            var itemEntryImg = itemEntry.AddComponent<Image>();
            itemEntryImg.color = new Color(0.12f, 0.1f, 0.08f, 0.8f);

            CreateText("ItemName_" + i, itemEntry.transform,
                sampleItems[i], 14, new Color(0.85f, 0.8f, 0.7f), TextAnchor.MiddleLeft,
                new Vector2(0.02f, 0f), new Vector2(0.7f, 1f), Vector2.zero, Vector2.zero);

            CreateButton("购买", shopPanel.transform,
                new Vector2(0.8f, 0.65f - i * 0.18f),
                new Vector2(0.95f, 0.8f - i * 0.18f),
                Vector2.zero, Vector2.zero,
                delegate (GameObject b) { Debug.Log("[Shop] 购买: " + sampleItems[i]); });
        }

        CreateText("Currency", shopPanel.transform,
            "食物: --  魂精华: --", 14, new Color(0.8f, 0.75f, 0.6f), TextAnchor.LowerRight,
            new Vector2(0.3f, 0.02f), new Vector2(0.98f, 0.06f), Vector2.zero, Vector2.zero);

        shopPanel.SetActive(false);
    }

    // ===== 4. 农场面板 (FarmPanel) =====
    void CreateFarmPanel(Transform parent)
    {
        farmPanel = CreatePanel("Farm", parent,
            new Vector2(0.2f, 0.15f), new Vector2(0.8f, 0.85f),
            Vector2.zero, Vector2.zero);

        CreateText("Title", farmPanel.transform,
            "农场", 24, new Color(0.9f, 0.85f, 0.7f), TextAnchor.UpperCenter,
            new Vector2(0.35f, 0.9f), new Vector2(0.65f, 0.98f), Vector2.zero, Vector2.zero);

        CreateCloseButton(farmPanel.transform, farmPanel,
            new Vector2(0.9f, 0.9f), new Vector2(1f, 0.98f), Vector2.zero);

        CreateText("Hint", farmPanel.transform,
            "提示：在营地时可以使用农场功能", 12, new Color(0.6f, 0.55f, 0.45f), TextAnchor.UpperLeft,
            new Vector2(0.05f, 0.85f), new Vector2(0.45f, 0.9f), Vector2.zero, Vector2.zero);

        int farmRows = 3, farmCols = 3;
        float farmCellW = 100f, farmCellH = 70f;
        float farmStartX = -150f, farmStartY = 150f;
        string[] farmStates = { "空地", "种子期", "生长期", "成熟期" };

        for (int r = 0; r < farmRows; r++)
        {
            for (int c = 0; c < farmCols; c++)
            {
                int idx = r * farmCols + c;
                var plot = new GameObject("Plot_" + idx);
                plot.transform.SetParent(farmPanel.transform, false);
                var plotRt = plot.AddComponent<RectTransform>();
                plotRt.anchorMin = new Vector2(0.5f, 0.5f);
                plotRt.anchorMax = new Vector2(0.5f, 0.5f);
                plotRt.sizeDelta = new Vector2(farmCellW - 4, farmCellH - 4);
                plotRt.anchoredPosition = new Vector2(farmStartX + c * farmCellW, farmStartY - r * farmCellH);
                var plotImg = plot.AddComponent<Image>();
                plotImg.color = new Color(0.2f, 0.15f, 0.1f, 0.9f);

                CreateText("PlotText_" + idx, plot.transform,
                    farmStates[0], 12, new Color(0.75f, 0.7f, 0.6f), TextAnchor.MiddleCenter,
                    Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

                CreateButton("地块" + idx, plot.transform,
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f), Vector2.zero,
                    new Vector2(farmCellW - 4, farmCellH - 4),
                    delegate (GameObject b) { Debug.Log("[Farm] 点击地块 " + idx); });
            }
        }

        string[] farmActions = { "种植", "收获", "浇水" };
        for (int i = 0; i < farmActions.Length; i++)
        {
            CreateButton(farmActions[i], farmPanel.transform,
                new Vector2(0.08f + i * 0.25f, 0.02f),
                new Vector2(0.28f + i * 0.25f, 0.1f),
                Vector2.zero, Vector2.zero,
                delegate (GameObject b) { Debug.Log("[Farm] 执行: " + farmActions[i]); });
        }

        farmPanel.SetActive(false);
    }

    // ===== 5. 天气面板 (WeatherPanel) =====
    void CreateWeatherPanel(Transform parent)
    {
        weatherPanel = CreatePanel("Weather", parent,
            new Vector2(0.3f, 0.3f), new Vector2(0.7f, 0.7f),
            Vector2.zero, Vector2.zero, 0.95f);

        CreateText("Title", weatherPanel.transform,
            "天气", 24, new Color(0.9f, 0.85f, 0.7f), TextAnchor.UpperCenter,
            new Vector2(0.35f, 0.88f), new Vector2(0.65f, 0.98f), Vector2.zero, Vector2.zero);

        CreateCloseButton(weatherPanel.transform, weatherPanel,
            new Vector2(0.88f, 0.88f), new Vector2(0.98f, 0.98f), Vector2.zero);

        CreateText("CurrentWeather", weatherPanel.transform,
            "当前天气：晴朗", 18, new Color(0.9f, 0.85f, 0.7f), TextAnchor.UpperLeft,
            new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.82f), Vector2.zero, Vector2.zero);

        CreateText("WeatherEffect", weatherPanel.transform,
            "效果：采集效率+10%", 14, new Color(0.7f, 0.65f, 0.55f), TextAnchor.UpperLeft,
            new Vector2(0.08f, 0.60f), new Vector2(0.92f, 0.70f), Vector2.zero, Vector2.zero);

        CreateText("ForecastLabel", weatherPanel.transform,
            "天气预报：", 14, new Color(0.8f, 0.75f, 0.65f), TextAnchor.UpperLeft,
            new Vector2(0.08f, 0.48f), new Vector2(0.5f, 0.55f), Vector2.zero, Vector2.zero);

        string[] forecast = { "明天：多云", "后天：小雨", "大后天：晴" };
        for (int i = 0; i < forecast.Length; i++)
        {
            CreateText("Forecast_" + i, weatherPanel.transform,
                forecast[i], 13, new Color(0.7f, 0.65f, 0.55f), TextAnchor.UpperLeft,
                new Vector2(0.08f, 0.38f - i * 0.1f), new Vector2(0.5f, 0.45f - i * 0.1f),
                Vector2.zero, Vector2.zero);
        }

        CreateButton("祈晴", weatherPanel.transform,
            new Vector2(0.25f, 0.05f), new Vector2(0.55f, 0.15f),
            Vector2.zero, Vector2.zero,
            delegate (GameObject b) { Debug.Log("[Weather] 祈晴请求"); });

        CreateText("Tip", weatherPanel.transform,
            "提示：不同天气影响采集效率和敌人遭遇", 11, new Color(0.5f, 0.45f, 0.35f), TextAnchor.LowerLeft,
            new Vector2(0.08f, 0.02f), new Vector2(0.92f, 0.06f), Vector2.zero, Vector2.zero);

        weatherPanel.SetActive(false);
    }

    // ===== 6. 日程面板 (SchedulePanel) =====
    void CreateSchedulePanel(Transform parent)
    {
        schedulePanel = CreatePanel("Schedule", parent,
            new Vector2(0.2f, 0.15f), new Vector2(0.8f, 0.85f),
            Vector2.zero, Vector2.zero);

        CreateText("Title", schedulePanel.transform,
            "日程", 24, new Color(0.9f, 0.85f, 0.7f), TextAnchor.UpperCenter,
            new Vector2(0.35f, 0.9f), new Vector2(0.65f, 0.98f), Vector2.zero, Vector2.zero);

        CreateCloseButton(schedulePanel.transform, schedulePanel,
            new Vector2(0.9f, 0.9f), new Vector2(1f, 0.98f), Vector2.zero);

        CreateText("Hint", schedulePanel.transform,
            "NPC日程安排（仅供参考）", 12, new Color(0.6f, 0.55f, 0.45f), TextAnchor.UpperLeft,
            new Vector2(0.05f, 0.82f), new Vector2(0.5f, 0.88f), Vector2.zero, Vector2.zero);

        // 列表背景
        var schedBg = new GameObject("ScheduleList_BG");
        schedBg.transform.SetParent(schedulePanel.transform, false);
        var schedBgRt = schedBg.AddComponent<RectTransform>();
        schedBgRt.anchorMin = new Vector2(0.05f, 0.05f);
        schedBgRt.anchorMax = new Vector2(0.95f, 0.78f);
        schedBgRt.sizeDelta = Vector2.zero;
        var schedBgImg = schedBg.AddComponent<Image>();
        schedBgImg.color = new Color(0.08f, 0.06f, 0.05f, 0.8f);

        // 5个NPC日程条目
        string[] npcNames = { "NPC-A", "NPC-B", "NPC-C", "NPC-D", "NPC-E" };
        string[] npcSchedules = {
            "08:00 营地 | 12:00 森林 | 18:00 营地",
            "09:00 废墟 | 14:00 山谷 | 20:00 营地",
            "全天在营地休息",
            "10:00 森林 | 16:00 迷雾前沿 | 22:00 营地",
            "不定时出现于森林之心"
        };
        string[] npcLocations = { "营地", "森林", "营地", "迷雾前沿", "森林之心" };

        for (int i = 0; i < npcNames.Length; i++)
        {
            var npcEntry = new GameObject("NPCSchedule_" + i);
            npcEntry.transform.SetParent(schedulePanel.transform, false);
            var npcEntryRt = npcEntry.AddComponent<RectTransform>();
            npcEntryRt.anchorMin = new Vector2(0.05f, 0.65f - i * 0.14f);
            npcEntryRt.anchorMax = new Vector2(0.95f, 0.78f - i * 0.14f);
            npcEntryRt.sizeDelta = Vector2.zero;
            var npcEntryImg = npcEntry.AddComponent<Image>();
            npcEntryImg.color = new Color(0.12f, 0.1f, 0.08f, 0.8f);

            // NPC名称
            CreateText("NPCName_" + i, npcEntry.transform,
                npcNames[i], 14, new Color(0.85f, 0.8f, 0.7f), TextAnchor.MiddleLeft,
                new Vector2(0.02f, 0.5f), new Vector2(0.3f, 1f),
                Vector2.zero, Vector2.zero);

            // 日程时间表
            CreateText("NPCScheduleText_" + i, npcEntry.transform,
                npcSchedules[i], 12, new Color(0.7f, 0.65f, 0.55f), TextAnchor.MiddleLeft,
                new Vector2(0.02f, 0f), new Vector2(0.75f, 0.5f),
                Vector2.zero, Vector2.zero);

            // 当前位置
            CreateText("NPCLocation_" + i, npcEntry.transform,
                "当前位置: " + npcLocations[i], 12, new Color(0.6f, 0.55f, 0.45f), TextAnchor.MiddleRight,
                new Vector2(0.75f, 0f), new Vector2(0.98f, 0.5f),
                Vector2.zero, Vector2.zero);
        }

        schedulePanel.SetActive(false);
    }

    // ========== 数据结构 ==========

    [System.Serializable]
    public class BalanceData
    {
        // 预留扩展字段
        public int version = 1;
    }

    [System.Serializable]
    public class EnemyDropTable
    {
        public int foodMin;
        public int foodMax;
        public int soulEssenceMin;
        public int soulEssenceMax;
    }

    public enum QuestDifficulty
    {
        Easy = 0,
        Medium = 1,
        Hard = 2,
        Boss = 3
    }
}