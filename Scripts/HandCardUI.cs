using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// 手牌战斗 UI 系统
/// 实现基于卡牌的回合制战斗（8 AP/回合）
/// 5张基础卡：攻击/防御/重击/治愈/休息
/// </summary>
public class HandCardUI : MonoBehaviour
{
    public static HandCardUI instance { get; private set; }

    [Header("手牌配置")]
    public int maxAP = 8;
    public int currentAP = 8;
    public List<Card> handCards = new List<Card>();
    public bool isPlayerTurn = true;
    public bool isCombatActive = false;

    [Header("战斗状态")]
    public EnemyInfo currentEnemy;
    public int playerHP;
    public int playerMaxHP = 100;
    public int defenseBonus = 0;  // 防御加成

    [Header("UI引用")]
    public GameObject combatPanel;      // 战斗面板
    public GameObject cardContainer;     // 卡牌容器
    public Text apText;                 // AP显示
    public Text enemyHPText;            // 敌人HP
    public Text playerHPText;           // 玩家HP
    public Image enemyHPBar;            // 敌人HP条
    public Image playerHPBar;           // 玩家HP条
    public Text combatLogText;          // 战斗日志
    public Button btnEndTurn;           // 结束回合按钮
    public Button btnFlee;              // 逃跑按钮
    public GameObject cardPrefab;        // 卡牌预设

    [Header("卡牌数据")]
    public Sprite cardAttack;
    public Sprite cardDefend;
    public Sprite cardHeavy;
    public Sprite cardHeal;
    public Sprite cardRest;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        CreateCombatUI();
        DrawInitialHand();
    }

    // ====================
    // 战斗入口（由 GameManager.Combat 调用）
    // ====================

    /// <summary>
    /// 开始一场战斗
    /// </summary>
    public void StartCombat(EnemyInfo enemy)
    {
        currentEnemy = enemy;
        currentAP = maxAP;
        playerHP = GameManager.instance.selectedMember != null 
            ? GameManager.instance.selectedMember.currentHealth 
            : 100;
        playerMaxHP = GameManager.instance.selectedMember != null 
            ? GameManager.instance.selectedMember.maxHealth 
            : 100;
        defenseBonus = 0;
        isPlayerTurn = true;
        isCombatActive = true;

        if (combatPanel != null)
            combatPanel.SetActive(true);

        DrawInitialHand();
        UpdateAllUI();

        AddCombatLog($"⚔️ 与 {enemy.enemyName} 展开战斗！");
    }

    /// <summary>
    /// 结束战斗（胜利）
    /// </summary>
    public void EndCombat_Victory()
    {
        isCombatActive = false;
        AddCombatLog($"🎉 胜利！击败了 {currentEnemy.enemyName}！");
        FindObjectOfType<AudioSystem>()?.OnVictory();

        if (GameManager.instance != null)
        {
            GameManager.instance.combatCount++;
            UnityEngine.FindObjectOfType<TutorialSystem>()?.OnFirstCombat();
        }

        // 奖励
        if (GameManager.instance != null)
        {
            GameManager.instance.food += currentEnemy.foodReward;
            GameManager.instance.soulEssence += currentEnemy.soulReward;

            // 触发手账
            if (JournalSystem.instance != null)
                JournalSystem.instance.OnCombatVictory(currentEnemy.enemyName, 
                    currentEnemy.foodReward, currentEnemy.soulReward);

            // 触发任务击杀通知
            var quest = UnityEngine.FindObjectOfType<QuestSystem>();
            if (quest != null)
                quest.OnEnemyKilled(currentEnemy.typeId);
        }

        // 2秒后关闭战斗面板
        Invoke("CloseCombatPanel", 2f);
    }

    /// <summary>
    /// 结束战斗（失败/逃跑）
    /// </summary>
    public void EndCombat_Defeat()
    {
        isCombatActive = false;
        AddCombatLog($"💀 战斗失败...");
        FindObjectOfType<AudioSystem>()?.OnDefeat();

        if (GameManager.instance != null)
            GameManager.instance.threatLevel = Mathf.Min(GameManager.instance.threatLevel + 1, 5);

        Invoke("CloseCombatPanel", 1.5f);
    }

    void CloseCombatPanel()
    {
        if (combatPanel != null)
            combatPanel.SetActive(false);
    }

    // ====================
    // 出牌逻辑
    // ====================

    /// <summary>
    /// 使用一张卡牌
    /// </summary>
    public void UseCard(Card card)
    {
        if (!isCombatActive || !isPlayerTurn) return;
        if (currentAP < card.apCost) 
        {
            AddCombatLog($"⚠️ AP不足！需要 {card.apCost}，当前 {currentAP}");
            return;
        }
        if (card.isOnCooldown) return;

        currentAP -= card.apCost;

        switch (card.cardType)
        {
            case CardType.Attack:
                ExecuteAttack(card);
                break;
            case CardType.Defend:
                ExecuteDefend(card);
                break;
            case CardType.HeavyAttack:
                ExecuteHeavyAttack(card);
                break;
            case CardType.Heal:
                ExecuteHeal(card);
                break;
            case CardType.Rest:
                ExecuteRest(card);
                break;
        }

        // 抽新卡
        DrawCard();

        UpdateAllUI();

        // 检查是否AP耗尽
        if (currentAP <= 0)
        {
            StartCoroutine(EnemyTurnRoutine());
        }
    }

    void ExecuteAttack(Card card)
    {
        // 85% 命中率
        bool hit = UnityEngine.Random.value < 0.85f;
        if (hit)
        {
            int damage = card.baseDamage + (GameManager.instance?.selectedMember?.attack ?? 12);
            currentEnemy.hp -= damage;
            AddCombatLog($"⚔️ 攻击！命中 {damage} 伤害");
            FindObjectOfType<AudioSystem>()?.OnAttack();

            if (currentEnemy.hp <= 0)
            {
                EndCombat_Victory();
                return;
            }
        }
        else
        {
            AddCombatLog($"⚔️ 攻击落空！");
        }

        card.cooldownCurrent = card.cooldown;
        card.isOnCooldown = true;
    }

    void ExecuteDefend(Card card)
    {
        int defense = (GameManager.instance?.selectedMember?.defense ?? 8) + card.baseDamage;
        defenseBonus = Mathf.RoundToInt(defense * 0.5f);
        AddCombatLog($"🛡️ 防御姿态！受伤-50%");
        FindObjectOfType<AudioSystem>()?.OnDefend();
    }

    void ExecuteHeavyAttack(Card card)
    {
        // 60% 命中率
        bool hit = UnityEngine.Random.value < 0.60f;
        if (hit)
        {
            int damage = card.baseDamage * 2 + (GameManager.instance?.selectedMember?.attack ?? 12);
            currentEnemy.hp -= damage;
            AddCombatLog($"💥 重击！命中 {damage} 伤害");
            FindObjectOfType<AudioSystem>()?.OnSkill();

            if (currentEnemy.hp <= 0)
            {
                EndCombat_Victory();
                return;
            }
        }
        else
        {
            AddCombatLog($"💥 重击落空！");
        }

        card.cooldownCurrent = card.cooldown;
        card.isOnCooldown = true;
    }

    void ExecuteHeal(Card card)
    {
        int heal = card.baseHeal + (GameManager.instance?.selectedMember?.intelligence / 2);
        playerHP = Mathf.Min(playerHP + heal, playerMaxHP);
        AddCombatLog($"💚 治愈！恢复 {heal} HP");
        FindObjectOfType<AudioSystem>()?.OnSkill();

        // 草药消耗（草药学技能：草药治疗不消耗）
        var skills = UnityEngine.FindObjectOfType<SkillsSystem>();
        bool consumesHerb = skills == null || skills.DoesHerbConsume();

        if (consumesHerb && GameManager.instance != null && GameManager.instance.herb > 0)
        {
            GameManager.instance.herb--;
        }
        else if (!consumesHerb)
        {
            AddCombatLog("🌿 草药学：草药未消耗");
        }
        else
        {
            // 没有草药，消耗一半效果
            playerHP = Mathf.Min(playerHP + heal / 2, playerMaxHP);
            AddCombatLog("⚠️ 没有草药，效果减半");
        }

        card.cooldownCurrent = card.cooldown;
        card.isOnCooldown = true;
    }

    void ExecuteRest(Card card)
    {
        int energy = card.baseHeal;
        currentAP = Mathf.Min(currentAP + energy, maxAP);
        AddCombatLog($"🌙 休息！恢复 {energy} AP");
    }

    // ====================
    // 敌方回合
    // ====================

    System.Collections.IEnumerator EnemyTurnRoutine()
    {
        isPlayerTurn = false;
        AddCombatLog($"━━━ {currentEnemy.enemyName} 回合 ━━━");

        yield return new WaitForSeconds(1f);

        // 敌人攻击
        int damage = currentEnemy.attack - defenseBonus;
        damage = Mathf.Max(1, damage);
        playerHP -= damage;

        AddCombatLog($"{currentEnemy.enemyName} 攻击！造成 {damage} 伤害");

        // 30% 概率玩家受伤（攻击力永久-30%，SPEC v2.6）
        var skills = UnityEngine.FindObjectOfType<SkillsSystem>();
        if (UnityEngine.Random.value < 0.3f)
        {
            if (GameManager.instance?.selectedMember != null && skills != null)
            {
                skills.ApplyInjuryDebuff(GameManager.instance.selectedMember);
            }
            else
            {
                AddCombatLog($"💀 你受伤了！攻击力永久下降");
            }
        }

        yield return new WaitForSeconds(1f);

        // 检查玩家死亡
        if (playerHP <= 0)
        {
            EndCombat_Defeat();
            yield break;
        }

        // 重置防御
        defenseBonus = 0;

        // 重置卡牌冷却 & 新回合
        ResetCardCooldowns();
        currentAP = maxAP;
        isPlayerTurn = true;
        DrawInitialHand();

        AddCombatLog($"━━━ 你的回合 ({currentAP}AP) ━━━");
        UpdateAllUI();
    }

    public void OnEndTurnClicked()
    {
        if (!isCombatActive || !isPlayerTurn) return;
        StartCoroutine(EnemyTurnRoutine());
    }

    public void OnFleeClicked()
    {
        if (!isCombatActive || !isPlayerTurn) return;

        // 威胁越高逃跑成功率越低
        int threat = GameManager.instance?.threatLevel ?? 1;
        float fleeChance = 0.7f - (threat - 1) * 0.1f;
        fleeChance = Mathf.Max(0.2f, fleeChance);

        if (UnityEngine.Random.value < fleeChance)
        {
            AddCombatLog($"🏃 逃跑成功！");
            EndCombat_Defeat();
        }
        else
        {
            AddCombatLog($"🏃 逃跑失败！");
            StartCoroutine(EnemyTurnRoutine());
        }
    }

    // ====================
    // 卡牌管理
    // ====================

    void DrawInitialHand()
    {
        handCards.Clear();

        // 初始手牌：攻击x2, 防御x1, 重击x1, 治愈x1
        handCards.Add(CreateCard(CardType.Attack, 2, "攻击", "10伤害\n85%命中", 0));
        handCards.Add(CreateCard(CardType.Attack, 2, "攻击", "10伤害\n85%命中", 0));
        handCards.Add(CreateCard(CardType.Defend, 1, "防御", "受伤-50%", 0));
        handCards.Add(CreateCard(CardType.HeavyAttack, 4, "重击", "20伤害\n60%命中", 2));
        handCards.Add(CreateCard(CardType.Heal, 2, "治愈", "草药恢复30HP\n无草药减半", 2));

        RefreshCardDisplay();
    }

    void DrawCard()
    {
        // 每回合抽1张卡补到手牌5张
        if (handCards.Count < 5)
        {
            Card[] drawOptions = {
                CreateCard(CardType.Attack, 2, "攻击", "10伤害", 0),
                CreateCard(CardType.Defend, 1, "防御", "受伤-50%", 0),
                CreateCard(CardType.HeavyAttack, 4, "重击", "20伤害", 2),
                CreateCard(CardType.Heal, 2, "治愈", "草药治疗", 2),
                CreateCard(CardType.Rest, 1, "休息", "恢复4AP", 0)
            };

            var drawn = drawOptions[UnityEngine.Random.Range(0, drawOptions.Length)];
            handCards.Add(drawn);
        }

        RefreshCardDisplay();
    }

    void ResetCardCooldowns()
    {
        foreach (var card in handCards)
        {
            if (card.cooldownCurrent > 0)
                card.cooldownCurrent--;
            if (card.cooldownCurrent <= 0)
                card.isOnCooldown = false;
        }
    }

    Card CreateCard(CardType type, int ap, string name, string desc, int cooldown)
    {
        return new Card
        {
            cardType = type,
            cardName = name,
            apCost = ap,
            description = desc,
            cooldown = cooldown,
            cooldownCurrent = 0,
            isOnCooldown = false,
            baseDamage = 10,
            baseHeal = 15
        };
    }

    // ====================
    // UI 创建
    // ====================

    void CreateCombatUI()
    {
        var canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null) return;

        // 战斗面板（初始隐藏）
        combatPanel = new GameObject("CombatPanel");
        combatPanel.transform.SetParent(canvasObj.transform, false);
        var panelRt = combatPanel.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.sizeDelta = Vector2.zero;

        var panelBg = combatPanel.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.85f);
        combatPanel.SetActive(false);

        // 顶部：敌人信息
        CreateEnemyInfo(combatPanel.transform);

        // 中间：战斗日志
        CreateCombatLog(combatPanel.transform);

        // 底部：手牌区域
        CreateHandArea(combatPanel.transform);

        Debug.Log("[HandCardUI] 战斗UI创建完成");
    }

    void CreateEnemyInfo(Transform parent)
    {
        // 敌人HP条背景
        var hpBg = new GameObject("EnemyHPBar_BG");
        hpBg.transform.SetParent(parent, false);
        var hpBgImg = hpBg.AddComponent<Image>();
        hpBgImg.color = new Color(0.2f, 0.2f, 0.2f);
        var hpBgRt = hpBg.AddComponent<RectTransform>();
        hpBgRt.anchorMin = new Vector2(0.1f, 0.75f);
        hpBgRt.anchorMax = new Vector2(0.9f, 0.82f);
        hpBgRt.sizeDelta = new Vector2(0, 0);

        // 敌人HP条前景
        var hpBar = new GameObject("EnemyHPBar");
        hpBar.transform.SetParent(parent, false);
        enemyHPBar = hpBar.AddComponent<Image>();
        enemyHPBar.color = new Color(0.9f, 0.2f, 0.2f);
        var hpBarRt = hpBar.AddComponent<RectTransform>();
        hpBarRt.anchorMin = new Vector2(0.1f, 0.75f);
        hpBarRt.anchorMax = new Vector2(0.9f, 0.82f);
        hpBarRt.sizeDelta = new Vector2(0, 0);
        hpBarRt.pivot = new Vector2(0, 0.5f);

        // 敌人名 + HP文字
        var enemyNameObj = new GameObject("EnemyName");
        enemyNameObj.transform.SetParent(parent, false);
        enemyHPText = enemyNameObj.AddComponent<Text>();
        enemyHPText.text = "敌人: ??";
        enemyHPText.fontSize = 18;
        enemyHPText.color = Color.white;
        enemyHPText.alignment = TextAnchor.MiddleCenter;
        var enemyNameRt = enemyNameObj.GetComponent<RectTransform>();
        enemyNameRt.anchorMin = new Vector2(0.1f, 0.83f);
        enemyNameRt.anchorMax = new Vector2(0.9f, 0.90f);
        enemyNameRt.sizeDelta = new Vector2(0, 0);

        // 玩家HP条
        var playerHpBg = new GameObject("PlayerHPBar_BG");
        playerHpBg.transform.SetParent(parent, false);
        var playerHpBgImg = playerHpBg.AddComponent<Image>();
        playerHpBgImg.color = new Color(0.2f, 0.2f, 0.2f);
        var playerHpBgRt = playerHpBg.AddComponent<RectTransform>();
        playerHpBgRt.anchorMin = new Vector2(0.1f, 0.65f);
        playerHpBgRt.anchorMax = new Vector2(0.9f, 0.72f);
        playerHpBgRt.sizeDelta = new Vector2(0, 0);

        var playerHpBar = new GameObject("PlayerHPBar");
        playerHpBar.transform.SetParent(parent, false);
        playerHPBar = playerHpBar.AddComponent<Image>();
        playerHPBar.color = new Color(0.2f, 0.8f, 0.2f);
        var playerHpBarRt = playerHpBar.AddComponent<RectTransform>();
        playerHpBarRt.anchorMin = new Vector2(0.1f, 0.65f);
        playerHpBarRt.anchorMax = new Vector2(0.9f, 0.72f);
        playerHpBarRt.sizeDelta = new Vector2(0, 0);
        playerHpBarRt.pivot = new Vector2(0, 0.5f);

        var playerHpTxt = new GameObject("PlayerHPText");
        playerHpTxt.transform.SetParent(parent, false);
        playerHPText = playerHpTxt.AddComponent<Text>();
        playerHPText.text = "HP: 100/100";
        playerHPText.fontSize = 16;
        playerHPText.color = Color.white;
        playerHPText.alignment = TextAnchor.MiddleCenter;
        var playerHpTxtRt = playerHpTxt.GetComponent<RectTransform>();
        playerHpTxtRt.anchorMin = new Vector2(0.1f, 0.72f);
        playerHpTxtRt.anchorMax = new Vector2(0.9f, 0.75f);
        playerHpTxtRt.sizeDelta = new Vector2(0, 0);
    }

    void CreateCombatLog(Transform parent)
    {
        var logBg = new GameObject("CombatLog_BG");
        logBg.transform.SetParent(parent, false);
        var logBgImg = logBg.AddComponent<Image>();
        logBgImg.color = new Color(0.1f, 0.08f, 0.06f, 0.9f);
        var logBgRt = logBg.AddComponent<RectTransform>();
        logBgRt.anchorMin = new Vector2(0.05f, 0.35f);
        logBgRt.anchorMax = new Vector2(0.95f, 0.63f);
        logBgRt.sizeDelta = new Vector2(0, 0);

        var logObj = new GameObject("CombatLogText");
        logObj.transform.SetParent(parent, false);
        combatLogText = logObj.AddComponent<Text>();
        combatLogText.text = "准备战斗...";
        combatLogText.fontSize = 14;
        combatLogText.color = new Color(0.9f, 0.85f, 0.75f);
        combatLogText.alignment = TextAnchor.UpperLeft;
        var logRt = logObj.GetComponent<RectTransform>();
        logRt.anchorMin = new Vector2(0.06f, 0.36f);
        logRt.anchorMax = new Vector2(0.94f, 0.62f);
        logRt.sizeDelta = new Vector2(0, 0);

        // 结束回合按钮
        var endTurnObj = new GameObject("BtnEndTurn");
        endTurnObj.transform.SetParent(parent, false);
        btnEndTurn = endTurnObj.AddComponent<Button>();
        var endTurnImg = endTurnObj.AddComponent<Image>();
        endTurnImg.color = new Color(0.3f, 0.5f, 0.3f);
        btnEndTurn.targetGraphic = endTurnImg;
        var endTurnTxt = endTurnObj.AddComponent<Text>();
        endTurnTxt.text = "结束回合";
        endTurnTxt.fontSize = 16;
        endTurnTxt.color = Color.white;
        endTurnTxt.alignment = TextAnchor.MiddleCenter;
        btnEndTurn.onClick.AddListener(OnEndTurnClicked);
        var endTurnRt = endTurnObj.GetComponent<RectTransform>();
        endTurnRt.anchorMin = new Vector2(0.55f, 0.25f);
        endTurnRt.anchorMax = new Vector2(0.75f, 0.33f);
        endTurnRt.sizeDelta = new Vector2(0, 0);

        // 逃跑按钮
        var fleeObj = new GameObject("BtnFlee");
        fleeObj.transform.SetParent(parent, false);
        btnFlee = fleeObj.AddComponent<Button>();
        var fleeImg = fleeObj.AddComponent<Image>();
        fleeImg.color = new Color(0.5f, 0.3f, 0.3f);
        btnFlee.targetGraphic = fleeImg;
        var fleeTxt = fleeObj.AddComponent<Text>();
        fleeTxt.text = "逃跑";
        fleeTxt.fontSize = 16;
        fleeTxt.color = Color.white;
        fleeTxt.alignment = TextAnchor.MiddleCenter;
        btnFlee.onClick.AddListener(OnFleeClicked);
        var fleeRt = fleeObj.GetComponent<RectTransform>();
        fleeRt.anchorMin = new Vector2(0.77f, 0.25f);
        fleeRt.anchorMax = new Vector2(0.95f, 0.33f);
        fleeRt.sizeDelta = new Vector2(0, 0);
    }

    void CreateHandArea(Transform parent)
    {
        cardContainer = new GameObject("CardContainer");
        cardContainer.transform.SetParent(parent, false);
        var containerRt = cardContainer.AddComponent<RectTransform>();
        containerRt.anchorMin = new Vector2(0f, 0f);
        containerRt.anchorMax = new Vector2(1f, 0.24f);
        containerRt.sizeDelta = new Vector2(0, 0);
        containerRt.anchoredPosition = Vector2.zero;

        // AP显示
        var apObj = new GameObject("APText");
        apObj.transform.SetParent(parent, false);
        apText = apObj.AddComponent<Text>();
        apText.text = "AP: 8/8";
        apText.fontSize = 22;
        apText.color = new Color(0.6f, 0.9f, 0.6f);
        apText.alignment = TextAnchor.MiddleCenter;
        apText.fontStyle = FontStyle.Bold;
        var apRt = apObj.GetComponent<RectTransform>();
        apRt.anchorMin = new Vector2(0f, 0.25f);
        apRt.anchorMax = new Vector2(0.15f, 0.33f);
        apRt.sizeDelta = new Vector2(0, 0);
    }

    void RefreshCardDisplay()
    {
        if (cardContainer == null) return;

        // 清除旧卡牌
        foreach (Transform child in cardContainer.transform)
            Destroy(child);

        // 创建新卡牌
        float totalWidth = cardContainer.GetComponent<RectTransform>().rect.width;
        float cardWidth = 120f;
        float spacing = (totalWidth - cardWidth * handCards.Count) / (handCards.Count + 1);

        for (int i = 0; i < handCards.Count; i++)
        {
            var card = handCards[i];
            var cardObj = CreateCardObject(card, i);
            var cardRt = cardObj.GetComponent<RectTransform>();

            float x = spacing + i * (cardWidth + spacing);
            cardRt.anchorMin = new Vector2(0, 0);
            cardRt.anchorMax = new Vector2(0, 0);
            cardRt.sizeDelta = new Vector2(cardWidth, 160);
            cardRt.anchoredPosition = new Vector2(x + cardWidth / 2, 80);
        }
    }

    GameObject CreateCardObject(Card card, int index)
    {
        var cardObj = new GameObject($"Card_{card.cardName}_{index}");
        cardObj.transform.SetParent(cardContainer.transform, false);

        // 卡牌背景
        var cardBg = cardObj.AddComponent<Image>();
        Color cardColor = GetCardColor(card.cardType);
        if (card.isOnCooldown) cardColor = new Color(0.3f, 0.3f, 0.3f);
        cardBg.color = cardColor;

        // 卡牌名称
        var nameObj = new GameObject("CardName");
        nameObj.transform.SetParent(cardObj.transform, false);
        var nameTxt = nameObj.AddComponent<Text>();
        nameTxt.text = card.cardName;
        nameTxt.fontSize = 16;
        nameTxt.color = Color.white;
        nameTxt.alignment = TextAnchor.MiddleCenter;
        nameTxt.fontStyle = FontStyle.Bold;
        var nameRt = nameObj.GetComponent<RectTransform>();
        nameRt.anchorMin = new Vector2(0.05f, 0.7f);
        nameRt.anchorMax = new Vector2(0.95f, 0.85f);
        nameRt.sizeDelta = new Vector2(0, 0);

        // AP消耗
        var apObj = new GameObject("CardAP");
        apObj.transform.SetParent(cardObj.transform, false);
        var apTxt = apObj.AddComponent<Text>();
        apTxt.text = card.isOnCooldown ? $"CD:{card.cooldownCurrent}" : $"{card.apCost}AP";
        apTxt.fontSize = 14;
        apTxt.color = card.isOnCooldown ? Color.red : new Color(1f, 0.8f, 0.2f);
        apTxt.alignment = TextAnchor.MiddleCenter;
        apTxt.fontStyle = FontStyle.Bold;
        var apRt = apObj.GetComponent<RectTransform>();
        apRt.anchorMin = new Vector2(0.7f, 0.85f);
        apRt.anchorMax = new Vector2(0.95f, 0.95f);
        apRt.sizeDelta = new Vector2(0, 0);

        // 描述
        var descObj = new GameObject("CardDesc");
        descObj.transform.SetParent(cardObj.transform, false);
        var descTxt = descObj.AddComponent<Text>();
        descTxt.text = card.description;
        descTxt.fontSize = 11;
        descTxt.color = new Color(0.9f, 0.9f, 0.9f);
        descTxt.alignment = TextAnchor.MiddleCenter;
        var descRt = descObj.GetComponent<RectTransform>();
        descRt.anchorMin = new Vector2(0.05f, 0.15f);
        descRt.anchorMax = new Vector2(0.95f, 0.68f);
        descRt.sizeDelta = new Vector2(0, 0);

        // 点击按钮
        var btn = cardObj.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(1f, 1f, 0.8f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.6f);
        btn.colors = colors;

        int cardIndex = index;
        btn.onClick.AddListener(() => 
        {
            if (cardIndex < handCards.Count)
                UseCard(handCards[cardIndex]);
        });

        return cardObj;
    }

    Color GetCardColor(CardType type)
    {
        switch (type)
        {
            case CardType.Attack: return new Color(0.7f, 0.2f, 0.2f);    // 红
            case CardType.Defend: return new Color(0.2f, 0.4f, 0.7f);   // 蓝
            case CardType.HeavyAttack: return new Color(0.6f, 0.1f, 0.6f);// 紫
            case CardType.Heal: return new Color(0.2f, 0.6f, 0.2f);     // 绿
            case CardType.Rest: return new Color(0.5f, 0.4f, 0.2f);      // 棕
            default: return new Color(0.4f, 0.4f, 0.4f);
        }
    }

    // ====================
    // UI 更新
    // ====================

    void UpdateAllUI()
    {
        if (apText != null)
            apText.text = $"AP\n{currentAP}/{maxAP}";

        if (enemyHPText != null && currentEnemy != null)
        {
            enemyHPText.text = $"{currentEnemy.enemyName}: {Mathf.Max(0, currentEnemy.hp)}/{currentEnemy.maxHP}";
            if (enemyHPBar != null)
                enemyHPBar.fillAmount = Mathf.Clamp01((float)currentEnemy.hp / currentEnemy.maxHP);
        }

        if (playerHPText != null)
        {
            playerHPText.text = $"HP: {Mathf.Max(0, playerHP)}/{playerMaxHP}";
            if (playerHPBar != null)
                playerHPBar.fillAmount = Mathf.Clamp01((float)playerHP / playerMaxHP);
        }

        RefreshCardDisplay();
    }

    void AddCombatLog(string message)
    {
        if (combatLogText == null) return;
        combatLogText.text = combatLogText.text + "\n" + message;
    }

    // ====================
    // 数据结构
    // ====================

    public enum CardType { Attack, Defend, HeavyAttack, Heal, Rest }

    [System.Serializable]
    public class Card
    {
        public CardType cardType;
        public string cardName;
        public int apCost;
        public string description;
        public int cooldown;        // 冷却回合
        public int cooldownCurrent;
        public bool isOnCooldown;
        public int baseDamage;
        public int baseHeal;
    }

    [System.Serializable]
    public class EnemyInfo
    {
        public string enemyName;
        public int hp;
        public int maxHP;
        public int attack;
        public int foodReward;
        public int soulReward;
    }
}
