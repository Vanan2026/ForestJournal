using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 制作系统 v1.0
/// 12种配方：消耗资源制作物品/装备
/// </summary>
public class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem instance { get; private set; }

    [Header("制作配方")]
    public List<Recipe> recipes = new List<Recipe>();

    [Header("制作产物库存")]
    public int bandages = 0;          // 绷带
    public int medicine = 0;         // 药剂
    public int torch = 0;             // 火把
    public int rope = 0;             // 绳索
    public int boneBlade = 0;        // 骨刀
    public int boneArmor = 0;        // 骨甲
    public int herbPoultice = 0;     // 草药膏
    public int huntersBow = 0;       // 猎人弓
    public int essenceAmulet = 0;    // 魂精华护符
    public int forestCloak = 0;      // 森林斗篷
    public int healingFlute = 0;     // 治愈笛
    public int ancientRelic = 0;     // 古物

    void Awake()
    {
        instance = this;
        InitializeRecipes();
    }

    void InitializeRecipes()
    {
        recipes = new List<Recipe>
        {
            // === 消耗品类 ===
            new Recipe {
                id = "bandage",
                name = "绷带",
                description = "止血用，恢复10HP",
                category = "消耗品",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "herb", 1 }, { "fiber", 1 }
                },
                outputCount = 2,
                effectDesc = "制作2个，战斗外使用恢复10HP"
            },
            new Recipe {
                id = "herb_poultice",
                name = "草药膏",
                description = "草药炼金，恢复25HP+解除受伤",
                category = "消耗品",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "herb", 3 }
                },
                outputCount = 1,
                effectDesc = "战斗中使用，恢复25HP并治疗受伤状态"
            },
            new Recipe {
                id = "medicine",
                name = "药剂",
                description = "强效恢复，恢复50HP",
                category = "消耗品",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "herb", 2 }, { "bone", 1 }
                },
                outputCount = 1,
                effectDesc = "战斗中使用，恢复50HP"
            },
            new Recipe {
                id = "torch",
                name = "火把",
                description = "照亮黑暗区域",
                category = "工具",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "wood", 2 }
                },
                outputCount = 1,
                effectDesc = "在黑雾前沿区域可驱散部分迷雾"
            },
            new Recipe {
                id = "rope",
                name = "绳索",
                description = "用于攀爬和陷阱",
                category = "工具",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "fiber", 3 }
                },
                outputCount = 1,
                effectDesc = "探索时有几率自动触发避险"
            },

            // === 装备类 ===
            new Recipe {
                id = "bone_blade",
                name = "骨刀",
                description = "骨制近战武器，攻击+3",
                category = "武器",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "bone", 3 }, { "wood", 1 }
                },
                outputCount = 1,
                effectDesc = "装备后攻击+3，永久生效"
            },
            new Recipe {
                id = "bone_armor",
                name = "骨甲",
                description = "骨制护甲，防御+3",
                category = "防具",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "bone", 4 }, { "fiber", 2 }
                },
                outputCount = 1,
                effectDesc = "装备后防御+3，永久生效"
            },
            new Recipe {
                id = "hunters_bow",
                name = "猎人弓",
                description = "远程武器，攻击+5",
                category = "武器",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "wood", 3 }, { "bone", 2 }, { "fiber", 2 }
                },
                outputCount = 1,
                effectDesc = "装备后攻击+5（需要狩猎技能）"
            },
            new Recipe {
                id = "forest_cloak",
                name = "森林斗篷",
                description = "减少威胁增长",
                category = "防具",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "fiber", 5 }, { "herb", 2 }
                },
                outputCount = 1,
                effectDesc = "装备后威胁等级增长-1/天"
            },
            new Recipe {
                id = "healing_flute",
                name = "治愈笛",
                description = "每日自动治疗",
                category = "特殊",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "bone", 2 }, { "herb", 3 }, { "soulEssence", 1 }
                },
                outputCount = 1,
                effectDesc = "每日自动恢复5HP（装备后生效）"
            },
            new Recipe {
                id = "essence_amulet",
                name = "魂精华护符",
                description = "增加记忆获取",
                category = "特殊",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "soulEssence", 3 }, { "bone", 2 }
                },
                outputCount = 1,
                effectDesc = "装备后记忆碎片获取+1"
            },
            new Recipe {
                id = "ancient_relic",
                name = "古物",
                description = "终局物品，影响结局",
                category = "终局",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "soulEssence", 5 }, { "bone", 5 }, { "ore", 3 }
                },
                outputCount = 1,
                effectDesc = "携带古物到达森林之心，影响最终结局"
            }
        };
    }

    // ====================
    // 制作 API
    // ====================

    /// <summary>
    /// 检查是否可以制作某配方
    /// </summary>
    public bool CanCraft(string recipeId)
    {
        var recipe = recipes.Find(r => r.id == recipeId);
        if (recipe == null) return false;
        return HasIngredients(recipe);
    }

    bool HasIngredients(Recipe recipe)
    {
        if (GameManager.instance == null) return false;

        foreach (var ing in recipe.ingredients)
        {
            int owned = GetResourceAmount(ing.Key);
            if (owned < ing.Value) return false;
        }
        return true;
    }

    /// <summary>
    /// 执行制作
    /// </summary>
    public bool Craft(string recipeId)
    {
        var recipe = recipes.Find(r => r.id == recipeId);
        if (recipe == null)
        {
            GameManager.instance?.AddLog($"配方 [{recipeId}] 不存在");
            return false;
        }

        if (!HasIngredients(recipe))
        {
            string missing = "";
            foreach (var ing in recipe.ingredients)
            {
                int owned = GetResourceAmount(ing.Key);
                if (owned < ing.Value)
                    missing += $" {ing.Key}({owned}/{ing.Value})";
            }
            GameManager.instance?.AddLog($"⚠️ 资源不足：{missing.Trim()}");
            return false;
        }

        // 消耗资源
        foreach (var ing in recipe.ingredients)
        {
            ConsumeResource(ing.Key, ing.Value);
        }

        // 获得产物
        int count = recipe.outputCount;
        ApplyCraftedItem(recipeId, count);

        string itemName = recipe.name;
        if (count > 1) itemName += $" x{count}";

        GameManager.instance?.AddLog($"🔨 制作成功：{itemName}");
        FindObjectOfType<AudioSystem>()?.OnCraft();
        if (GameManager.instance != null) GameManager.instance.craftCount++;
        UnityEngine.FindObjectOfType<TutorialSystem>()?.OnFirstCraft();
        var quest = UnityEngine.FindObjectOfType<QuestSystem>();
        if (quest != null) quest.OnItemCrafted(recipeId);
        if (JournalSystem.instance != null)
            JournalSystem.instance.Record("crafting", $"制作：{recipe.name}",
                $"消耗：{IngredientsToString(recipe)}\n获得：{itemName}");

        return true;
    }

    void ApplyCraftedItem(string recipeId, int count)
    {
        switch (recipeId)
        {
            case "bandage":       bandages += count;      break;
            case "herb_poultice": herbPoultice += count; break;
            case "medicine":      medicine += count;      break;
            case "torch":        torch += count;          break;
            case "rope":         rope += count;           break;
            case "bone_blade":   boneBlade += count;     break;
            case "bone_armor":   boneArmor += count;     break;
            case "hunters_bow":  huntersBow += count;    break;
            case "forest_cloak": forestCloak += count;   break;
            case "healing_flute": healingFlute += count; break;
            case "essence_amulet": essenceAmulet += count; break;
            case "ancient_relic": ancientRelic += count;  break;
        }
    }

    int GetResourceAmount(string resource)
    {
        if (GameManager.instance == null) return 0;
        switch (resource)
        {
            case "food": return GameManager.instance.food;
            case "wood": return GameManager.instance.wood;
            case "stone": return GameManager.instance.stone;
            case "herb": return GameManager.instance.herb;
            case "fiber": return GameManager.instance.fiber;
            case "ore": return GameManager.instance.ore;
            case "bone": return GameManager.instance.bone;
            case "soulEssence": return GameManager.instance.soulEssence;
            default: return 0;
        }
    }

    void ConsumeResource(string resource, int amount)
    {
        if (GameManager.instance == null) return;
        switch (resource)
        {
            case "food":      GameManager.instance.food -= amount;      break;
            case "wood":      GameManager.instance.wood -= amount;      break;
            case "stone":     GameManager.instance.stone -= amount;     break;
            case "herb":      GameManager.instance.herb -= amount;      break;
            case "fiber":     GameManager.instance.fiber -= amount;     break;
            case "ore":       GameManager.instance.ore -= amount;       break;
            case "bone":     GameManager.instance.bone -= amount;       break;
            case "soulEssence": GameManager.instance.soulEssence -= amount; break;
        }
    }

    string IngredientsToString(Recipe r)
    {
        var parts = new List<string>();
        foreach (var ing in r.ingredients)
            parts.Add($"{ing.Key}×{ing.Value}");
        return string.Join(", ", parts);
    }

    // ====================
    // 装备加成效果
    // ====================

    /// <summary>
    /// 获取所有装备的攻击加成
    /// </summary>
    public int GetAttackBonus()
    {
        int bonus = 0;
        if (boneBlade > 0) bonus += 3;
        if (huntersBow > 0) bonus += 5;
        return bonus;
    }

    /// <summary>
    /// 获取所有装备的防御加成
    /// </summary>
    public int GetDefenseBonus()
    {
        int bonus = 0;
        if (boneArmor > 0) bonus += 3;
        return bonus;
    }

    /// <summary>
    /// 获取每日威胁减免
    /// </summary>
    public int GetThreatReduction()
    {
        if (forestCloak > 0) return 1;
        return 0;
    }

    /// <summary>
    /// 获取每日自动治疗量
    /// </summary>
    public int GetDailyHeal()
    {
        if (healingFlute > 0) return 5;
        return 0;
    }

    /// <summary>
    /// 获取记忆碎片加成
    /// </summary>
    public int GetMemoryBonus()
    {
        if (essenceAmulet > 0) return 1;
        return 0;
    }

    /// <summary>
    /// 是否有古物（影响结局）
    /// </summary>
    public bool HasAncientRelic()
    {
        return ancientRelic > 0;
    }

    // ====================
    // UI 显示
    // ====================

    /// <summary>
    /// 获取所有可制作的配方
    /// </summary>
    public List<Recipe> GetAvailableRecipes()
    {
        return recipes;
    }

    /// <summary>
    /// 获取可制作配方（当前有材料的）
    /// </summary>
    public List<Recipe> GetCraftableRecipes()
    {
        var list = new List<Recipe>();
        foreach (var r in recipes)
        {
            if (CanCraft(r.id))
                list.Add(r);
        }
        return list;
    }

    /// <summary>
    /// 显示制作菜单到日志
    /// </summary>
    public void ShowCraftingMenu()
    {
        if (GameManager.instance == null) return;

        GameManager.instance.AddLog("═══ 制作菜单 ═══");

        var craftable = GetCraftableRecipes();
        var uncraftable = recipes.FindAll(r => !CanCraft(r.id));

        if (craftable.Count > 0)
        {
            GameManager.instance.AddLog("【可制作】");
            foreach (var r in craftable)
            {
                GameManager.instance.AddLog($"  {r.name} - {IngredientsToString(r)}");
            }
        }

        if (uncraftable.Count > 0)
        {
            GameManager.instance.AddLog("【材料不足】");
            foreach (var r in uncraftable)
            {
                string status = "";
                foreach (var ing in r.ingredients)
                {
                    int owned = GetResourceAmount(ing.Key);
                    if (owned < ing.Value)
                        status += $" {ing.Key}({owned}/{ing.Value})";
                }
                GameManager.instance.AddLog($"  {r.name}:{status}");
            }
        }
    }

    // ====================
    // 制作按钮调用（供 UIManager 按钮使用）
    // ====================

    public void BtnCraftBandage() { Craft("bandage"); }
    public void BtnCraftHerbPoultice() { Craft("herb_poultice"); }
    public void BtnCraftMedicine() { Craft("medicine"); }
    public void BtnCraftTorch() { Craft("torch"); }
    public void BtnCraftRope() { Craft("rope"); }
    public void BtnCraftBoneBlade() { Craft("bone_blade"); }
    public void BtnCraftBoneArmor() { Craft("bone_armor"); }
    public void BtnCraftHuntersBow() { Craft("hunters_bow"); }
    public void BtnCraftForestCloak() { Craft("forest_cloak"); }
    public void BtnCraftHealingFlute() { Craft("healing_flute"); }
    public void BtnCraftEssenceAmulet() { Craft("essence_amulet"); }
    public void BtnCraftAncientRelic() { Craft("ancient_relic"); }

    // ====================
    // 数据类
    // ====================

    [System.Serializable]
    public class Recipe
    {
        public string id;
        public string name;
        public string description;
        public string category;  // 消耗品/工具/武器/防具/特殊/终局
        public System.Collections.Generic.Dictionary<string, int> ingredients;
        public int outputCount;
        public string effectDesc;
    }
}
