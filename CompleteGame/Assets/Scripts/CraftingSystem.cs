using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// 制作系统 v2.0
/// 35种配方：消耗资源制作物品/装备
/// </summary>
public class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem instance { get; private set; }

    [Header("制作配方")]
    public List<Recipe> recipes = new List<Recipe>();

    [Header("制作产物库存")]
    // === 原有12个 ===
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
    // === 新增23个 ===
    // 武器
    public int ironDagger = 0;        // 铁匕首
    public int boneBow = 0;          // 骨弓
    public int silverSword = 0;      // 银光剑
    public int shadowClaw = 0;       // 黑雾之爪
    public int thornWhip = 0;        // 荆棘之鞭
    public int forestStaff = 0;      // 森林之杖
    // 防具
    public int leatherArmor = 0;     // 皮甲
    public int heavyBoneArmor = 0;   // 重骨甲
    public int chainmail = 0;        // 锁子甲
    public int shadowAmulet = 0;    // 黑雾护符
    public int heartArmor = 0;       // 森林之心护甲
    // 消耗品
    public int healthPotion = 0;     // 生命药水
    public int strengthPotion = 0;  // 力量药剂
    public int poisonCleanser = 0;   // 抗毒药剂
    public int swiftElixir = 0;      // 迅捷药剂
    public int torchOil = 0;        // 探照火把
    public int forestEssence = 0;   // 森林精华
    // 工具
    public int boneHoe = 0;          // 骨锄
    public int orePickaxe = 0;       // 矿石镐
    public int advancedTrap = 0;     // 高级陷阱
    public int soulLantern = 0;     // 灵魂灯笼
    // 特殊
    public int ancientShardFragment = 0; // 古物碎片
    public int heartResonator = 0;   // 森林之心共鸣器

    void Awake()
    {
        instance = this;
        InitializeRecipes();
    }

    void InitializeRecipes()
    {
        recipes = new List<Recipe>
        {
            // ═══════════════════════════════════════════════
            // 消耗品类（原有3+新增6=9）
            // ═══════════════════════════════════════════════
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
            // === 新增消耗品 ===
            new Recipe {
                id = "health_potion",
                name = "生命药水",
                description = "强效治疗药水，恢复50HP",
                category = "消耗品",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "herb", 3 }, { "water", 1 }
                },
                outputCount = 1,
                effectDesc = "使用后立即恢复50HP，战斗/非战斗均可用"
            },
            new Recipe {
                id = "strength_potion",
                name = "力量药剂",
                description = "临时提升攻击力，持续3天",
                category = "消耗品",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "herb", 2 }, { "food", 1 }
                },
                outputCount = 1,
                effectDesc = "使用后攻击+5，持续3天（叠加计算）"
            },
            new Recipe {
                id = "poison_cleanser",
                name = "抗毒药剂",
                description = "清除毒素并获得抗性",
                category = "消耗品",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "herb", 4 }
                },
                outputCount = 1,
                effectDesc = "使用后清除当前毒素+24小时抗毒"
            },
            new Recipe {
                id = "swift_elixir",
                name = "迅捷药剂",
                description = "提升敏捷与行动效率",
                category = "消耗品",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "herb", 3 }, { "fiber", 2 }
                },
                outputCount = 1,
                effectDesc = "使用后敏捷+3，行动点消耗-1，持续2天"
            },
            new Recipe {
                id = "torch_oil",
                name = "探照火把",
                description = "强化火把，照明范围大幅提升",
                category = "消耗品",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "wood", 1 }, { "oil", 1 }
                },
                outputCount = 1,
                effectDesc = "装备后火把照明范围+2，黑雾可见距离扩大"
            },
            new Recipe {
                id = "forest_essence",
                name = "森林精华",
                description = "凝聚森林记忆，提升记忆获取",
                category = "消耗品",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "herb", 10 }, { "soulEssence", 5 }
                },
                outputCount = 1,
                effectDesc = "使用后记忆碎片+2（需在森林之心附近使用）"
            },

            // ═══════════════════════════════════════════════
            // 工具类（原有2+新增4=6）
            // ═══════════════════════════════════════════════
            new Recipe {
                id = "bone_hoe",
                name = "骨锄",
                description = "骨制农具，提升农业产量",
                category = "工具",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "bone", 3 }, { "wood", 2 }
                },
                outputCount = 1,
                effectDesc = "装备后采集食物时产量+20%"
            },
            new Recipe {
                id = "ore_pickaxe",
                name = "矿石镐",
                description = "矿石精炼镐，提升采矿效率",
                category = "工具",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "ore", 4 }, { "wood", 2 }
                },
                outputCount = 1,
                effectDesc = "装备后采集矿石时产量+30%"
            },
            new Recipe {
                id = "advanced_trap",
                name = "高级陷阱",
                description = "改良陷阱，大幅提升捕获率",
                category = "工具",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "wood", 5 }, { "bone", 3 }, { "ore", 2 }
                },
                outputCount = 1,
                effectDesc = "使用后狩猎陷阱捕获率+50%，持续5次使用"
            },
            new Recipe {
                id = "soul_lantern",
                name = "灵魂灯笼",
                description = "以魂精华驱动的灯笼，可照亮黑雾",
                category = "工具",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "soulEssence", 8 }, { "glass", 2 }
                },
                outputCount = 1,
                effectDesc = "装备后在黑雾区域完全照亮周围，免疫黑雾恐惧"
            },

            // ═══════════════════════════════════════════════
            // 武器类（原有2+新增6=8）
            // ═══════════════════════════════════════════════
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
            // === 新增武器 ===
            new Recipe {
                id = "iron_dagger",
                name = "铁匕首",
                description = "铁制短兵器，攻击+10",
                category = "武器",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "ore", 3 }, { "wood", 2 }
                },
                outputCount = 1,
                effectDesc = "装备后攻击+10，攻速较快"
            },
            new Recipe {
                id = "bone_bow",
                name = "骨弓",
                description = "骨骼打磨的弓，攻击+12，远程",
                category = "武器",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "bone", 5 }, { "wood", 3 }
                },
                outputCount = 1,
                effectDesc = "装备后攻击+12，远程攻击（不消耗行动点）"
            },
            new Recipe {
                id = "silver_sword",
                name = "银光剑",
                description = "银光闪耀的魔法剑，攻击+18",
                category = "武器",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "ore", 5 }, { "bone", 3 }, { "soulEssence", 2 }
                },
                outputCount = 1,
                effectDesc = "装备后攻击+18，对黑雾生物额外+5伤害"
            },
            new Recipe {
                id = "shadow_claw",
                name = "黑雾之爪",
                description = "吸收黑雾精华的爪，攻击+15",
                category = "武器",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "soulEssence", 5 }, { "bone", 3 }
                },
                outputCount = 1,
                effectDesc = "装备后攻击+15，攻击附带吸血效果（伤害10%转HP）"
            },
            new Recipe {
                id = "thorn_whip",
                name = "荆棘之鞭",
                description = "带毒的荆棘鞭，攻击+14",
                category = "武器",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "wood", 8 }, { "fiber", 3 }
                },
                outputCount = 1,
                effectDesc = "装备后攻击+14，攻击附带中毒（每回合-5HP，持续3回合）"
            },
            new Recipe {
                id = "forest_staff",
                name = "森林之杖",
                description = "森林之心赐予的权杖，攻击+20",
                category = "武器",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "wood", 10 }, { "soulEssence", 5 }
                },
                outputCount = 1,
                effectDesc = "装备后攻击+20，使用消耗品时额外恢复5HP"
            },

            // ═══════════════════════════════════════════════
            // 防具类（原有2+新增5=7）
            // ═══════════════════════════════════════════════
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
            // === 新增防具 ===
            new Recipe {
                id = "leather_armor",
                name = "皮甲",
                description = "皮革编织的护甲，防御+8",
                category = "防具",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "leather", 4 }, { "fiber", 2 }
                },
                outputCount = 1,
                effectDesc = "装备后防御+8，永久生效"
            },
            new Recipe {
                id = "heavy_bone_armor",
                name = "重骨甲",
                description = "厚重骨骼打造的护甲，防御+12",
                category = "防具",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "bone", 8 }, { "wood", 2 }
                },
                outputCount = 1,
                effectDesc = "装备后防御+12，永久生效"
            },
            new Recipe {
                id = "chainmail",
                name = "锁子甲",
                description = "金属环编织，防御+15",
                category = "防具",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "ore", 6 }, { "wood", 2 }
                },
                outputCount = 1,
                effectDesc = "装备后防御+15，永久生效"
            },
            new Recipe {
                id = "shadow_amulet",
                name = "黑雾护符",
                description = "黑雾精华凝聚，防御+10",
                category = "防具",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "soulEssence", 5 }
                },
                outputCount = 1,
                effectDesc = "装备后防御+10，全元素抗性+1"
            },
            new Recipe {
                id = "heart_armor",
                name = "森林之心护甲",
                description = "森林之心赐予的终极护甲，防御+20",
                category = "防具",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "soulEssence", 10 }, { "bone", 5 }
                },
                outputCount = 1,
                effectDesc = "装备后防御+20，每日自动清一次负面状态"
            },

            // ═══════════════════════════════════════════════
            // 特殊/终局类（原有2+新增2=4）
            // ═══════════════════════════════════════════════
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
            // === 新增特殊 ===
            new Recipe {
                id = "ancient_shard_fragment",
                name = "古物碎片",
                description = "远古遗物的碎片，用于合成古物",
                category = "特殊",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "bone", 15 }, { "soulEssence", 8 }
                },
                outputCount = 1,
                effectDesc = "收集碎片可在铁匠处合成完整古物（需3个碎片）"
            },
            new Recipe {
                id = "heart_resonator",
                name = "森林之心共鸣器",
                description = "与森林之心共鸣的终极道具",
                category = "终局",
                ingredients = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "soulEssence", 20 }, { "ancient_shard_fragment", 1 }
                },
                outputCount = 1,
                effectDesc = "携带至森林之心触发隐藏结局线"
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
            // 原有
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
            // 新增武器
            case "iron_dagger":    ironDagger += count;    break;
            case "bone_bow":      boneBow += count;       break;
            case "silver_sword":   silverSword += count;   break;
            case "shadow_claw":   shadowClaw += count;    break;
            case "thorn_whip":    thornWhip += count;     break;
            case "forest_staff":  forestStaff += count;   break;
            // 新增防具
            case "leather_armor":  leatherArmor += count;  break;
            case "heavy_bone_armor": heavyBoneArmor += count; break;
            case "chainmail":     chainmail += count;     break;
            case "shadow_amulet": shadowAmulet += count;  break;
            case "heart_armor":   heartArmor += count;    break;
            // 新增消耗品
            case "health_potion":  healthPotion += count;  break;
            case "strength_potion": strengthPotion += count; break;
            case "poison_cleanser": poisonCleanser += count; break;
            case "swift_elixir":   swiftElixir += count;  break;
            case "torch_oil":      torchOil += count;      break;
            case "forest_essence": forestEssence += count; break;
            // 新增工具
            case "bone_hoe":      boneHoe += count;       break;
            case "ore_pickaxe":   orePickaxe += count;    break;
            case "advanced_trap": advancedTrap += count;  break;
            case "soul_lantern":  soulLantern += count;   break;
            // 新增特殊
            case "ancient_shard_fragment": ancientShardFragment += count; break;
            case "heart_resonator": heartResonator += count; break;
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
            case "leather": return GameManager.instance.leather;
            case "water": return GameManager.instance.water;
            case "oil": return GameManager.instance.oil;
            case "glass": return GameManager.instance.glass;
            case "ancient_shard_fragment": return ancientShardFragment;
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
            case "herb":      GameManager.instance.herb -= amount;       break;
            case "fiber":     GameManager.instance.fiber -= amount;     break;
            case "ore":       GameManager.instance.ore -= amount;        break;
            case "bone":     GameManager.instance.bone -= amount;        break;
            case "soulEssence": GameManager.instance.soulEssence -= amount; break;
            case "leather":   GameManager.instance.leather -= amount;    break;
            case "water":     GameManager.instance.water -= amount;      break;
            case "oil":       GameManager.instance.oil -= amount;        break;
            case "glass":     GameManager.instance.glass -= amount;     break;
            case "ancient_shard_fragment": ancientShardFragment -= amount; break;
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
        if (ironDagger > 0) bonus += 10;
        if (boneBow > 0) bonus += 12;
        if (silverSword > 0) bonus += 18;
        if (shadowClaw > 0) bonus += 15;
        if (thornWhip > 0) bonus += 14;
        if (forestStaff > 0) bonus += 20;
        return bonus;
    }

    /// <summary>
    /// 获取所有装备的防御加成
    /// </summary>
    public int GetDefenseBonus()
    {
        int bonus = 0;
        if (boneArmor > 0) bonus += 3;
        if (leatherArmor > 0) bonus += 8;
        if (heavyBoneArmor > 0) bonus += 12;
        if (chainmail > 0) bonus += 15;
        if (shadowAmulet > 0) bonus += 10;
        if (heartArmor > 0) bonus += 20;
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
        int heal = 0;
        if (healingFlute > 0) heal += 5;
        if (forestStaff > 0) heal += 3;
        return heal;
    }

    /// <summary>
    /// 获取记忆碎片加成
    /// </summary>
    public int GetMemoryBonus()
    {
        int bonus = 0;
        if (essenceAmulet > 0) bonus += 1;
        if (forestEssence > 0) bonus += 2;
        return bonus;
    }

    /// <summary>
    /// 是否有古物（影响结局）
    /// </summary>
    public bool HasAncientRelic()
    {
        return ancientRelic > 0;
    }

    /// <summary>
    /// 是否有森林之心共鸣器（隐藏结局）
    /// </summary>
    public bool HasHeartResonator()
    {
        return heartResonator > 0;
    }

    /// <summary>
    /// 获取元素抗性加成
    /// </summary>
    public int GetElementalResistance()
    {
        int resist = 0;
        if (shadowAmulet > 0) resist += 1;
        if (heartArmor > 0) resist += 2;
        return resist;
    }

    /// <summary>
    /// 获取农业产量加成（百分比）
    /// </summary>
    public int GetFarmingBonus()
    {
        if (boneHoe > 0) return 20;
        return 0;
    }

    /// <summary>
    /// 获取采矿产量加成（百分比）
    /// </summary>
    public int GetMiningBonus()
    {
        if (orePickaxe > 0) return 30;
        return 0;
    }

    /// <summary>
    /// 获取陷阱捕获率加成（百分比）
    /// </summary>
    public int GetTrapBonus()
    {
        if (advancedTrap > 0) return 50;
        return 0;
    }

    /// <summary>
    /// 获取火把照明范围加成
    /// </summary>
    public int GetTorchRangeBonus()
    {
        int bonus = 0;
        if (torchOil > 0) bonus += 2;
        if (soulLantern > 0) bonus += 5;
        return bonus;
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

    // 原有按钮
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
    // 新增武器按钮
    public void BtnCraftIronDagger() { Craft("iron_dagger"); }
    public void BtnCraftBoneBow() { Craft("bone_bow"); }
    public void BtnCraftSilverSword() { Craft("silver_sword"); }
    public void BtnCraftShadowClaw() { Craft("shadow_claw"); }
    public void BtnCraftThornWhip() { Craft("thorn_whip"); }
    public void BtnCraftForestStaff() { Craft("forest_staff"); }
    // 新增防具按钮
    public void BtnCraftLeatherArmor() { Craft("leather_armor"); }
    public void BtnCraftHeavyBoneArmor() { Craft("heavy_bone_armor"); }
    public void BtnCraftChainmail() { Craft("chainmail"); }
    public void BtnCraftShadowAmulet() { Craft("shadow_amulet"); }
    public void BtnCraftHeartArmor() { Craft("heart_armor"); }
    // 新增消耗品按钮
    public void BtnCraftHealthPotion() { Craft("health_potion"); }
    public void BtnCraftStrengthPotion() { Craft("strength_potion"); }
    public void BtnCraftPoisonCleanser() { Craft("poison_cleanser"); }
    public void BtnCraftSwiftElixir() { Craft("swift_elixir"); }
    public void BtnCraftTorchOil() { Craft("torch_oil"); }
    public void BtnCraftForestEssence() { Craft("forest_essence"); }
    // 新增工具按钮
    public void BtnCraftBoneHoe() { Craft("bone_hoe"); }
    public void BtnCraftOrePickaxe() { Craft("ore_pickaxe"); }
    public void BtnCraftAdvancedTrap() { Craft("advanced_trap"); }
    public void BtnCraftSoulLantern() { Craft("soul_lantern"); }
    // 新增特殊按钮
    public void BtnCraftAncientShardFragment() { Craft("ancient_shard_fragment"); }
    public void BtnCraftHeartResonator() { Craft("heart_resonator"); }

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
