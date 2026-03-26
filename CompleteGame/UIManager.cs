using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 游戏UI初始化器 - 创建所有按钮和显示
/// </summary>
public class UIManager : MonoBehaviour
{
    void Start()
    {
        CreateCanvas();
        Debug.Log("[UIManager] UI Created!");
    }

    void CreateCanvas()
    {
        // 确保屏幕尺寸有效（编辑器下 screenW/height 可能为0）
        int screenW = Mathf.Max(Screen.width, 1280);
        int screenH = Mathf.Max(Screen.height, 720);

        // 创建 Canvas
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<UnityEngine.Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // 配置 CanvasScaler
        var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new UnityEngine.Vector2(1280, 720);
        scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.Expand;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // === 顶部状态栏 ===
        CreateText(canvasObj.transform, "DayText", "第 1 天", 10, screenH - 40, 200, 30);
        CreateText(canvasObj.transform, "APText", "AP: 2/2", 10, screenH - 70, 150, 30);
        CreateText(canvasObj.transform, "FoodText", "食物: 10", 10, screenH - 100, 150, 30);
        CreateText(canvasObj.transform, "ThreatText", "威胁: 1/5", 10, screenH - 130, 150, 30);
        CreateText(canvasObj.transform, "RegionText", "位置: 迷雾森林", 10, screenH - 160, 200, 30);

        // === 日志区域 ===
        var logPanel = CreatePanel(canvasObj.transform, "LogPanel", 10, 10, 400, 200);
        var gm = FindObjectOfType<GameManager>();
        if (gm != null) gm.uiLog = CreateText(logPanel.transform, "LogText", "日志...", 0, 0, 380, 180);

        // === 采集按钮 ===
        var gatherPanel = CreatePanel(canvasObj.transform, "GatherPanel", 10, screenH / 2 - 50, 150, 150);
        CreateText(gatherPanel.transform, "Title", "采集", 0, 60, 130, 25);
        CreateButton(gatherPanel.transform, "BtnFood", "🍎 食物", -50, 20, 100, 35, () => { gm?.BtnGatherFood(); });
        CreateButton(gatherPanel.transform, "BtnWood", "🪵 木材", -50, -20, 100, 35, () => { gm?.BtnGatherWood(); });
        CreateButton(gatherPanel.transform, "BtnHerb", "🌿 草药", -50, -60, 100, 35, () => { gm?.BtnGatherHerb(); });

        // === 行动按钮 ===
        var actionPanel = CreatePanel(canvasObj.transform, "ActionPanel", 170, screenH / 2 - 50, 150, 150);
        CreateText(actionPanel.transform, "Title", "行动", 0, 60, 130, 25);
        CreateButton(actionPanel.transform, "BtnRest", "🏕️ 安营", -50, 20, 100, 35, () => { gm?.BtnRest(); });
        CreateButton(actionPanel.transform, "BtnCombat", "⚔️ 战斗", -50, -20, 100, 35, () => { gm?.BtnCombat(); });
        CreateButton(actionPanel.transform, "BtnTalk", "💬 交谈", -50, -60, 100, 35, () => { gm?.BtnTalk(); });

        // === 区域按钮 ===
        var regionPanel = CreatePanel(canvasObj.transform, "RegionPanel", 330, screenH / 2 - 50, 200, 180);
        CreateText(regionPanel.transform, "Title", "迁移", 0, 80, 180, 25);

        int[] regionY = { 40, 0, -40, -80, -120 };
        string[] regionNames = { "迷雾森林", "黑雾前沿", "古老废墟", "幽暗山谷", "森林之心" };
        UnityEngine.Events.UnityAction[] regionActions = {
            () => gm?.BtnMigrate0(), () => gm?.BtnMigrate1(), () => gm?.BtnMigrate2(),
            () => gm?.BtnMigrate3(), () => gm?.BtnMigrate4()
        };

        for (int i = 0; i < 5; i++)
        {
            CreateButton(regionPanel.transform, $"BtnRegion{i}", regionNames[i], -75, regionY[i], 150, 35, regionActions[i]);
        }

        // === 队伍信息 ===
        var squadPanel = CreatePanel(canvasObj.transform, "SquadPanel", screenW - 220, screenH / 2 - 80, 200, 200);
        CreateText(squadPanel.transform, "Title", "队伍", 0, 90, 180, 25);

        int squadY = 60;
        foreach (var member in FindObjectsOfType<GameManager>())
        {
            foreach (var m in member.squad)
            {
                CreateText(squadPanel.transform, $"Squad_{m.memberName}", $"{m.memberName}({m.role}) HP:{m.currentHealth}/{m.maxHealth}", -75, squadY, 180, 25);
                squadY -= 30;
            }
        }

        // === 火堆按钮 ===
        var skillPanel = CreatePanel(canvasObj.transform, "SkillPanel", screenW - 220, screenH / 2 - 200, 200, 90);
        CreateText(skillPanel.transform, "Title", "设施", 0, 40, 180, 25);
        CreateButton(skillPanel.transform, "BtnCampfire", "🔥 建造火堆(5木)", -75, 0, 170, 35,
            () => {
                var sk = UnityEngine.FindObjectOfType<SkillsSystem>();
                if (sk != null) sk.BuildCampfire();
            });
        CreateButton(skillPanel.transform, "BtnJournal", "📖 手账", -75, -40, 170, 35,
            () => { gm?.BtnJournal(); });

        // === 制作按钮 ===
        var craftPanel = CreatePanel(canvasObj.transform, "CraftPanel", screenW - 440, screenH / 2 - 200, 200, 130);
        CreateText(craftPanel.transform, "Title", "制作", 0, 55, 180, 25);
        CreateButton(craftPanel.transform, "BtnCraftMenu", "📦 制作菜单", -75, 20, 170, 35,
            () => {
                var cs = UnityEngine.FindObjectOfType<CraftingSystem>();
                if (cs != null) cs.ShowCraftingMenu();
            });
        CreateButton(craftPanel.transform, "BtnCraftBandage", "绷带(1草1纤)", -75, -20, 170, 35,
            () => {
                var cs = UnityEngine.FindObjectOfType<CraftingSystem>();
                if (cs != null) cs.Craft("bandage");
            });
        CreateButton(craftPanel.transform, "BtnCraftBoneBlade", "骨刀(3骨1木)", -75, -55, 170, 35,
            () => {
                var cs = UnityEngine.FindObjectOfType<CraftingSystem>();
                if (cs != null) cs.Craft("bone_blade");
            });

        // === 存档/读档按钮 ===
        var savePanel = CreatePanel(canvasObj.transform, "SavePanel", screenW / 2 - 100, 10, 200, 70);
        CreateText(savePanel.transform, "Title", "存档", 0, 30, 180, 25);
        CreateButton(savePanel.transform, "BtnSave", "💾 保存", -75, 0, 80, 35,
            () => {
                var sl = UnityEngine.FindObjectOfType<SaveLoadSystem>();
                if (sl != null) sl.Save(0);
            });
        CreateButton(savePanel.transform, "BtnLoad", "📂 读档", 0, 0, 80, 35,
            () => {
                var sl = UnityEngine.FindObjectOfType<SaveLoadSystem>();
                if (sl != null) sl.Load(0);
            });

        // === 任务按钮 ===
        var questPanel = CreatePanel(canvasObj.transform, "QuestPanel", 10, screenH / 2 - 100, 180, 60);
        CreateText(questPanel.transform, "Title", "任务", 0, 25, 160, 25);
        CreateButton(questPanel.transform, "BtnQuest", "📜 任务列表", -75, -5, 160, 40,
            () => {
                var qs = UnityEngine.FindObjectOfType<QuestSystem>();
                if (qs != null) qs.ShowQuestLog();
            });
        CreateButton(questPanel.transform, "BtnEnding", "🏆 结局预览", -75, -45, 160, 35,
            () => {
                var es = UnityEngine.FindObjectOfType<EndingsSystem>();
                if (es != null) es.ShowEndingPreview();
            });

        // === 音效按钮 ===
        var audioPanel = CreatePanel(canvasObj.transform, "AudioPanel", 10, screenH - 90, 180, 55);
        CreateText(audioPanel.transform, "Title", "音效", 0, 20, 160, 25);
        CreateButton(audioPanel.transform, "BtnSound", "🔊 音效", -75, -5, 75, 35,
            () => {
                var audio = UnityEngine.FindObjectOfType<AudioSystem>();
                if (audio != null) audio.ToggleMute();
                GameManager.instance?.AddLog("🔊 音效切换");
            });
        CreateButton(audioPanel.transform, "BtnBGM", "🎵 音乐", -75, -40, 75, 35,
            () => {
                var audio = UnityEngine.FindObjectOfType<AudioSystem>();
                if (audio != null)
                {
                    var gm = GameManager.instance;
                    if (gm != null && gm.gs == "victory")
                        audio.PlayBGM("forest");
                    else
                        audio.PlayBGM("forest");
                }
            });

        // === 教程按钮 ===
        CreateButton(canvasObj.transform, "BtnTutorial", "📖 教程", screenW / 2 - 40, screenH - 60, 80, 30,
            () => {
                var ts = UnityEngine.FindObjectOfType<TutorialSystem>();
                if (ts != null)
                {
                    if (!ts.hasCompletedBasicTutorial)
                        ts.StartTutorial(TutorialSystem.TutorialPhase.Basic);
                    else
                        GameManager.instance?.AddLog("基础教程已完成，输入「教程」查看帮助。");
                }
            });
    }

    GameObject CreatePanel(Transform parent, string name, int x, int y, int w, int h)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent);

        var rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(w, h);
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);

        var img = obj.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

        return obj;
    }

    UnityEngine.UI.Text CreateText(Transform parent, string name, string content, int x, int y, int w, int h)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent);

        var rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(w, h);
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);

        var txt = obj.AddComponent<UnityEngine.UI.Text>();
        txt.text = content;
        txt.fontSize = 16;
        txt.color = Color.white;
        txt.alignment = TextAnchor.UpperLeft;
        txt.font = UnityEngine.Resources.GetBuiltinResource<Font>("Arial.ttf");

        return txt;
    }

    void CreateButton(Transform parent, string name, string content, int x, int y, int w, int h, UnityEngine.Events.UnityAction action)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent);

        var rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(w, h);
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);

        var img = obj.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.2f, 0.4f, 0.3f, 1f);

        var btn = obj.AddComponent<UnityEngine.UI.Button>();
        btn.onClick.AddListener(action);

        var colors = btn.colors;
        colors.highlightedColor = new Color(0.3f, 0.5f, 0.4f, 1f);
        colors.pressedColor = new Color(0.1f, 0.3f, 0.2f, 1f);
        btn.colors = colors;

        // Button text
        var txtObj = new GameObject("Text");
        txtObj.transform.SetParent(obj.transform);
        var txtRect = txtObj.AddComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.sizeDelta = Vector2.zero;
        txtRect.anchoredPosition = Vector2.zero;

        var txt = txtObj.AddComponent<UnityEngine.UI.Text>();
        txt.text = content;
        txt.fontSize = 14;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = UnityEngine.Resources.GetBuiltinResource<Font>("Arial.ttf");
    }
}
