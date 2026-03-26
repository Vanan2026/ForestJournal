using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// NPC记忆系统
/// </summary>
public class NPCMemorySystem : MonoBehaviour
{
    public static NPCMemorySystem instance { get; private set; }

    [Serializable]
    public class NPCMemory
    {
        public string npcId;
        public string npcName;
        public int trustLevel;  // -10 ~ 10
        public int timesMet;
        public List<string> memories = new List<string>();
    }

    private Dictionary<string, NPCMemory> npcMemories = new Dictionary<string, NPCMemory>();

    void Awake()
    {
        instance = this;
    }

    public void RecordInteraction(string npcId, string npcName, string eventType, int trustChange)
    {
        if (!npcMemories.ContainsKey(npcId))
        {
            npcMemories[npcId] = new NPCMemory
            {
                npcId = npcId,
                npcName = npcName,
                trustLevel = 0
            };
        }

        var mem = npcMemories[npcId];
        mem.timesMet++;
        mem.trustLevel = Mathf.Clamp(mem.trustLevel + trustChange, -10, 10);
        mem.memories.Add($"[{eventType}]");

        Debug.Log($"[Memory] {npcName}: {eventType} (trust: {mem.trustLevel})");
    }

    public NPCMemory GetMemory(string npcId)
    {
        return npcMemories.ContainsKey(npcId) ? npcMemories[npcId] : null;
    }

    public string GetAttitudeText(string npcId)
    {
        var mem = GetMemory(npcId);
        if (mem == null) return "陌生人";

        if (mem.trustLevel >= 7) return "挚友";
        if (mem.trustLevel >= 4) return "信任";
        if (mem.trustLevel >= 1) return "友善";
        if (mem.trustLevel >= -1) return "中立";
        if (mem.trustLevel >= -4) return "冷淡";
        if (mem.trustLevel >= -7) return "敌意";
        return "深仇";
    }
}

/// <summary>
/// 招募系统
/// </summary>
public class RecruitmentManager : MonoBehaviour
{
    public static RecruitmentManager instance { get; private set; }

    [Serializable]
    public class NPCData
    {
        public string npcId;
        public string name;
        public string role;
        public string personality;  // 固执/善良/狡猾/勇敢/神秘
        public int minTrustRequired;
        public string[] possibleDialogues;
    }

    public NPCData[] npcs;

    void Awake()
    {
        instance = this;
        InitializeNPCs();
    }

    void InitializeNPCs()
    {
        npcs = new NPCData[]
        {
            new NPCData
            {
                npcId = "hunter_li",
                name = "李伯",
                role = "老猎人",
                personality = "固执",
                minTrustRequired = 4,
                possibleDialogues = new[] { "这片林子我熟", "小心！有动静", "哼，跟着我走" }
            },
            new NPCData
            {
                npcId = "herbalist_azhi",
                name = "阿芷",
                role = "草药师",
                personality = "善良",
                minTrustRequired = 3,
                possibleDialogues = new[] { "这些草药真香", "让我看看你的伤", "森林在歌唱" }
            },
            new NPCData
            {
                npcId = "poet_ziqi",
                name = "子期",
                role = "诗人",
                personality = "神秘",
                minTrustRequired = 6,
                possibleDialogues = new[] { "人生若只如初见", "有趣，你的灵魂有诗", "故事啊..." }
            },
            new NPCData
            {
                npcId = "soldier_asen",
                name = "阿森",
                role = "士兵",
                personality = "勇敢",
                minTrustRequired = 2,
                possibleDialogues = new[] { "让我上！", "这点伤不碍事", "以前我是士兵" }
            },
            new NPCData
            {
                npcId = "child_lili",
                name = "莉莉",
                role = "流浪儿",
                personality = "善良",
                minTrustRequired = 1,
                possibleDialogues = new[] { "呜呜...", "真的吗？", "你会保护我吗？" }
            },
            new NPCData
            {
                npcId = "spirit_wuer",
                name = "雾儿",
                role = "森林精灵",
                personality = "神秘",
                minTrustRequired = 5,
                possibleDialogues = new[] { "我是森林的孩子", "森林在哭泣", "你听到了吗？" }
            }
        };
    }

    public NPCData GetRandomNPC()
    {
        return npcs[UnityEngine.Random.Range(0, npcs.Length)];
    }

    public bool CanRecruit(string npcId, int currentTrust)
    {
        foreach (var npc in npcs)
        {
            if (npc.npcId == npcId)
                return currentTrust >= npc.minTrustRequired;
        }
        return false;
    }
}

/// <summary>
/// 制作系统
/// </summary>
public class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem instance { get; private set; }

    [Serializable]
    public class Recipe
    {
        public string recipeId;
        public string name;
        public string description;
        public int[] costs;  // food, wood, stone, herb, fiber, ore, bone, soul
        public int apCost;
    }

    public Recipe[] recipes;

    void Awake()
    {
        instance = this;
        InitializeRecipes();
    }

    void InitializeRecipes()
    {
        recipes = new Recipe[]
        {
            new Recipe
            {
                recipeId = "bandage",
                name = "绷带",
                description = "恢复少量HP",
                costs = new[] { 0, 0, 0, 2, 0, 0, 0, 0 },
                apCost = 1
            },
            new Recipe
            {
                recipeId = "health_potion",
                name = "生命药水",
                description = "恢复较多HP",
                costs = new[] { 0, 0, 0, 5, 0, 0, 0, 0 },
                apCost = 2
            },
            new Recipe
            {
                recipeId = "torch",
                name = "火把",
                description = "照亮黑暗",
                costs = new[] { 0, 2, 0, 0, 0, 0, 0, 0 },
                apCost = 1
            },
            new Recipe
            {
                recipeId = "stone_axe",
                name = "石斧",
                description = "提高采集效率",
                costs = new[] { 0, 2, 3, 0, 0, 0, 0, 0 },
                apCost = 2
            },
            new Recipe
            {
                recipeId = "antidote",
                name = "解毒药",
                description = "解除中毒状态",
                costs = new[] { 0, 0, 0, 3, 0, 0, 0, 0 },
                apCost = 2
            },
            new Recipe
            {
                recipeId = "trap",
                name = "陷阱",
                description = "捕捉小动物",
                costs = new[] { 0, 3, 0, 0, 2, 0, 0, 0 },
                apCost = 2
            },
            new Recipe
            {
                recipeId = "bone_blade",
                name = "骨刀",
                description = "简易武器",
                costs = new[] { 0, 1, 0, 0, 0, 0, 4, 0 },
                apCost = 2
            },
            new Recipe
            {
                recipeId = "stone_shield",
                name = "石盾",
                description = "提高防御",
                costs = new[] { 0, 2, 5, 0, 0, 0, 0, 0 },
                apCost = 3
            }
        };
    }

    public bool CanCraft(Recipe recipe)
    {
        var gm = FindObjectOfType<GameManager>();
        if (gm == null) return false;

        return gm.food >= recipe.costs[0] &&
               gm.wood >= recipe.costs[1] &&
               gm.stone >= recipe.costs[2] &&
               gm.herb >= recipe.costs[3];
    }

    public void Craft(Recipe recipe)
    {
        var gm = FindObjectOfType<GameManager>();
        if (gm == null || !CanCraft(recipe)) return;

        gm.food -= recipe.costs[0];
        gm.wood -= recipe.costs[1];
        gm.stone -= recipe.costs[2];
        gm.herb -= recipe.costs[3];

        Debug.Log($"[Craft] 制作了 {recipe.name}");

        if (gm.uiLog != null)
            gm.uiLog.text += $"\n制作了 {recipe.name}！";
    }
}

/// <summary>
/// 战斗系统
/// </summary>
public class CombatSystem : MonoBehaviour
{
    public static CombatSystem instance { get; private set; }

    [Serializable]
    public class Enemy
    {
        public string name;
        public int hp;
        public int attack;
        public int defense;
        public int expReward;
        public int foodReward;
    }

    public Enemy[] enemies;

    void Awake()
    {
        instance = this;
        InitializeEnemies();
    }

    void InitializeEnemies()
    {
        enemies = new Enemy[]
        {
            new Enemy { name = "阴影狼", hp = 30, attack = 8, defense = 3, expReward = 5, foodReward = 3 },
            new Enemy { name = "毒蜘蛛", hp = 20, attack = 12, defense = 2, expReward = 4, foodReward = 2 },
            new Enemy { name = "雾精灵", hp = 25, attack = 10, defense = 4, expReward = 6, foodReward = 2 },
            new Enemy { name = "黑雾兽", hp = 40, attack = 15, defense = 5, expReward = 8, foodReward = 4 },
            new Enemy { name = "变异树妖", hp = 50, attack = 18, defense = 8, expReward = 10, foodReward = 5 },
            new Enemy { name = "沼泽蛙", hp = 15, attack = 6, defense = 1, expReward = 3, foodReward = 2 },
            new Enemy { name = "岩石蜥", hp = 35, attack = 12, defense = 7, expReward = 7, foodReward = 3 },
            new Enemy { name = "雾主", hp = 80, attack = 20, defense = 10, expReward = 20, foodReward = 10 },
            new Enemy { name = "森林守护者", hp = 100, attack = 25, defense = 15, expReward = 30, foodReward = 15 }
        };
    }

    public Enemy GetRandomEnemy(int regionThreat)
    {
        int index = Mathf.Min(regionThreat - 1, enemies.Length - 1);
        return enemies[index];
    }

    public bool SimulateCombat(Enemy enemy, SquadMember attacker, out int damageTaken)
    {
        int damageToEnemy = Mathf.Max(1, attacker.attack - enemy.defense);
        int actualDamage = enemy.hp - damageToEnemy;

        damageTaken = Mathf.Max(1, enemy.attack - attacker.defense);

        return actualDamage <= 0;  // true = 胜利
    }
}
