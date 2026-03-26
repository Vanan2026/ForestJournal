using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ForestJournal
{
    // ============================================================
    //  Part A: 装备强化
    // ============================================================

    [Serializable]
    public class RefinementLevel
    {
        public int level;               // +1 ~ +10
        public float successRate;       // 强化成功率
        public int soulEssenceCost;     // 魂精华消耗
        public int refinementStoneCost; // 强化石消耗
        public int rareMaterialCost;    // 稀有材料消耗（+7以上）
        public int attributeBonus;      // 成功时属性提升值
    }

    public static class RefinementConfig
    {
        public static readonly RefinementLevel[] Levels = new RefinementLevel[]
        {
            new RefinementLevel { level = 1,  successRate = 0.90f, soulEssenceCost = 0,   refinementStoneCost = 1, rareMaterialCost = 0, attributeBonus = 1 },
            new RefinementLevel { level = 2,  successRate = 0.80f, soulEssenceCost = 0,   refinementStoneCost = 1, rareMaterialCost = 0, attributeBonus = 1 },
            new RefinementLevel { level = 3,  successRate = 0.70f, soulEssenceCost = 0,   refinementStoneCost = 1, rareMaterialCost = 0, attributeBonus = 1 },
            new RefinementLevel { level = 4,  successRate = 0.60f, soulEssenceCost = 50,  refinementStoneCost = 1, rareMaterialCost = 0, attributeBonus = 1 },
            new RefinementLevel { level = 5,  successRate = 0.50f, soulEssenceCost = 80,  refinementStoneCost = 1, rareMaterialCost = 0, attributeBonus = 1 },
            new RefinementLevel { level = 6,  successRate = 0.40f, soulEssenceCost = 120, refinementStoneCost = 1, rareMaterialCost = 0, attributeBonus = 1 },
            new RefinementLevel { level = 7,  successRate = 0.30f, soulEssenceCost = 200, refinementStoneCost = 0, rareMaterialCost = 1, attributeBonus = 1 },
            new RefinementLevel { level = 8,  successRate = 0.25f, soulEssenceCost = 300, refinementStoneCost = 0, rareMaterialCost = 1, attributeBonus = 1 },
            new RefinementLevel { level = 9,  successRate = 0.20f, soulEssenceCost = 450, refinementStoneCost = 0, rareMaterialCost = 2, attributeBonus = 1 },
            new RefinementLevel { level = 10, successRate = 0.15f, soulEssenceCost = 600, refinementStoneCost = 0, rareMaterialCost = 2, attributeBonus = 1 },
        };

        public const float BREAK_CHANCE_ABOVE_PLUS7 = 0.30f; // +7以上爆装概率
        public const int MAX_REFINEMENT_LEVEL = 10;

        public static RefinementLevel GetLevelConfig(int level)
        {
            int idx = Mathf.Clamp(level, 1, MAX_REFINEMENT_LEVEL) - 1;
            return Levels[idx];
        }
    }

    public enum RefinementResult
    {
        Success,
        Downgrade,
        Broken
    }

    public class RefinementSystem
    {
        public static RefinementResult TryRefine(Equipment equipment, Inventory inventory, out int newLevel)
        {
            newLevel = equipment.refinementLevel;

            if (equipment.refinementLevel >= RefinementConfig.MAX_REFINEMENT_LEVEL)
            {
                newLevel = equipment.refinementLevel;
                return RefinementResult.Success; // 已满级
            }

            int targetLevel = equipment.refinementLevel + 1;
            RefinementLevel config = RefinementConfig.GetLevelConfig(targetLevel);

            // 扣除材料
            if (!ConsumeRefinementMaterials(inventory, config, targetLevel))
            {
                return RefinementResult.Downgrade; // 材料不足也算失败
            }

            // 随机判定
            float roll = UnityEngine.Random.value;
            if (roll < config.successRate)
            {
                // 成功
                newLevel = targetLevel;
                equipment.ApplyRefinementBonus(config.attributeBonus);
                return RefinementResult.Success;
            }
            else
            {
                // 失败
                if (equipment.refinementLevel >= 7 && UnityEngine.Random.value < RefinementConfig.BREAK_CHANCE_ABOVE_PLUS7)
                {
                    return RefinementResult.Broken;
                }
                else
                {
                    // 降1级（最低到+0）
                    newLevel = Mathf.Max(0, equipment.refinementLevel - 1);
                    equipment.RemoveRefinementBonus(newLevel);
                    return RefinementResult.Downgrade;
                }
            }
        }

        private static bool ConsumeRefinementMaterials(Inventory inventory, RefinementLevel config, int targetLevel)
        {
            if (targetLevel <= 3)
            {
                // +1~+3：强化石
                if (inventory.GetItemCount("RefinementStone") < config.refinementStoneCost)
                    return false;
                inventory.RemoveItem("RefinementStone", config.refinementStoneCost);
            }
            else if (targetLevel <= 6)
            {
                // +4~+6：魂精华+强化石
                if (inventory.GetItemCount("SoulEssence") < config.soulEssenceCost)
                    return false;
                if (inventory.GetItemCount("RefinementStone") < config.refinementStoneCost)
                    return false;
                inventory.RemoveItem("SoulEssence", config.soulEssenceCost);
                inventory.RemoveItem("RefinementStone", config.refinementStoneCost);
            }
            else
            {
                // +7~+10：稀有材料+魂精华
                if (inventory.GetItemCount("SoulEssence") < config.soulEssenceCost)
                    return false;
                if (inventory.GetItemCount("RareMaterial") < config.rareMaterialCost)
                    return false;
                inventory.RemoveItem("SoulEssence", config.soulEssenceCost);
                inventory.RemoveItem("RareMaterial", config.rareMaterialCost);
            }
            return true;
        }
    }

    // ============================================================
    //  Part B: 打孔系统
    // ============================================================

    public enum GemType
    {
        Ruby,      // 红宝石 - HP+30
        Sapphire,  // 蓝宝石 - 防御+10
        Emerald,   // 绿宝石 - 回避+8%
        Topaz,     // 黄宝石 - 暴击+10%
        Amethyst,  // 紫宝石 - 魂精华+20%
        Diamond    // 钻石 - 全属性+5
    }

    [Serializable]
    public class GemBonus
    {
        public GemType type;
        public int hpBonus;
        public int defenseBonus;
        public float dodgeBonus;   // 百分比，如 0.08f
        public float critBonus;    // 百分比，如 0.10f
        public float soulEssenceBonus; // 百分比，如 0.20f
        public int allAttrBonus;   // 全属性

        public static GemBonus GetGemBonus(GemType type)
        {
            switch (type)
            {
                case GemType.Ruby:     return new GemBonus { type = type, hpBonus = 30 };
                case GemType.Sapphire: return new GemBonus { type = type, defenseBonus = 10 };
                case GemType.Emerald:  return new GemBonus { type = type, dodgeBonus = 0.08f };
                case GemType.Topaz:     return new GemBonus { type = type, critBonus = 0.10f };
                case GemType.Amethyst: return new GemBonus { type = type, soulEssenceBonus = 0.20f };
                case GemType.Diamond:   return new GemBonus { type = type, allAttrBonus = 5 };
                default:               return new GemBonus { type = type };
            }
        }

        public string GetDisplayName()
        {
            switch (type)
            {
                case GemType.Ruby:     return "红宝石";
                case GemType.Sapphire: return "蓝宝石";
                case GemType.Emerald:  return "绿宝石";
                case GemType.Topaz:    return "黄宝石";
                case GemType.Amethyst: return "紫宝石";
                case GemType.Diamond:  return "钻石";
                default: return "未知宝石";
            }
        }

        public string GetBonusText()
        {
            var parts = new List<string>();
            if (hpBonus > 0) parts.Add($"HP+{hpBonus}");
            if (defenseBonus > 0) parts.Add($"防御+{defenseBonus}");
            if (dodgeBonus > 0) parts.Add($"回避+{dodgeBonus * 100}%");
            if (critBonus > 0) parts.Add($"暴击+{critBonus * 100}%");
            if (soulEssenceBonus > 0) parts.Add($"魂精华+{soulEssenceBonus * 100}%");
            if (allAttrBonus > 0) parts.Add($"全属性+{allAttrBonus}");
            return string.Join(" ", parts);
        }
    }

    public class SocketSystem
    {
        public const int MAX_SOCKETS = 3;
        public const int SOCKET_STONE_COST = 10; // 打孔石消耗
        public const int EMBED_SOUL_ESSENCE_COST = 20;  // 镶嵌消耗魂精华
        public const int REMOVE_SOUL_ESSENCE_COST = 30; // 摘取消耗魂精华

        /// <summary>
        /// 为装备打孔（装备需支持打孔）
        /// </summary>
        public static bool AddSocket(Equipment equipment, Inventory inventory)
        {
            if (!equipment.canHaveSockets)
                return false;
            if (equipment.socketCount >= MAX_SOCKETS)
                return false;
            if (inventory.GetItemCount("SocketStone") < SOCKET_STONE_COST)
                return false;

            inventory.RemoveItem("SocketStone", SOCKET_STONE_COST);
            equipment.socketCount++;
            return true;
        }

        /// <summary>
        /// 镶嵌宝石到指定孔位
        /// </summary>
        public static bool EmbedGem(Equipment equipment, Inventory inventory, int socketIndex, GemType gemType)
        {
            if (socketIndex < 0 || socketIndex >= equipment.socketCount)
                return false;
            if (equipment.socketedGems[socketIndex] != GemType.None)
                return false; // 孔位已有宝石
            if (inventory.GetItemCount("SoulEssence") < EMBED_SOUL_ESSENCE_COST)
                return false;

            string gemItemName = GetGemItemName(gemType);
            if (inventory.GetItemCount(gemItemName) < 1)
                return false;

            inventory.RemoveItem(gemItemName, 1);
            inventory.RemoveItem("SoulEssence", EMBED_SOUL_ESSENCE_COST);
            equipment.socketedGems[socketIndex] = gemType;
            return true;
        }

        /// <summary>
        /// 摘取宝石（不返还宝石，返还部分魂精华）
        /// </summary>
        public static bool RemoveGem(Equipment equipment, Inventory inventory, int socketIndex)
        {
            if (socketIndex < 0 || socketIndex >= equipment.socketCount)
                return false;
            if (equipment.socketedGems[socketIndex] == GemType.None)
                return false;
            if (inventory.GetItemCount("SoulEssence") < REMOVE_SOUL_ESSENCE_COST)
                return false;

            inventory.RemoveItem("SoulEssence", REMOVE_SOUL_ESSENCE_COST);
            equipment.socketedGems[socketIndex] = GemType.None;
            return true;
        }

        /// <summary>
        /// 计算镶嵌宝石的总加成
        /// </summary>
        public static GemBonus CalculateTotalGemBonus(Equipment equipment)
        {
            var total = new GemBonus();
            for (int i = 0; i < equipment.socketedGems.Length; i++)
            {
                if (equipment.socketedGems[i] != GemType.None)
                {
                    var gem = GemBonus.GetGemBonus(equipment.socketedGems[i]);
                    total.hpBonus += gem.hpBonus;
                    total.defenseBonus += gem.defenseBonus;
                    total.dodgeBonus += gem.dodgeBonus;
                    total.critBonus += gem.critBonus;
                    total.soulEssenceBonus += gem.soulEssenceBonus;
                    total.allAttrBonus += gem.allAttrBonus;
                }
            }
            return total;
        }

        public static string GetGemItemName(GemType type)
        {
            switch (type)
            {
                case GemType.Ruby:     return "Ruby";
                case GemType.Sapphire: return "Sapphire";
                case GemType.Emerald:  return "Emerald";
                case GemType.Topaz:    return "Topaz";
                case GemType.Amethyst: return "Amethyst";
                case GemType.Diamond:  return "Diamond";
                default:               return "UnknownGem";
            }
        }
    }

    // ============================================================
    //  Part C: 商店系统
    // ============================================================

    public enum ShopType
    {
        Goblin,    // 地精商人（常驻）
        Wandering, // 流浪商人（随机）
        BlackMarket, // 黑市商人（高价）
        GoblinForge  // 地精工坊（完成10连续任务后）
    }

    [Serializable]
    public class ShopItem
    {
        public string itemId;
        public string displayName;
        public int soulEssencePrice;
        public int stock;       // -1 表示无限
        public bool isUnlimited => stock < 0;

        public bool IsSoldOut => !isUnlimited && stock <= 0;
    }

    [Serializable]
    public class Shop
    {
        public ShopType type;
        public string displayName;
        public List<ShopItem> items = new List<ShopItem>();
        public long lastRefreshTime; // Unix timestamp
        public bool isUnlocked;

        public Shop(ShopType type, string displayName, bool unlocked = false)
        {
            this.type = type;
            this.displayName = displayName;
            this.isUnlocked = unlocked;
            this.lastRefreshTime = 0;
        }

        public void RefreshStock()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            // 每周刷新（7天 = 604800秒）
            if (now - lastRefreshTime >= 604800)
            {
                foreach (var item in items)
                {
                    if (!item.isUnlimited)
                        item.stock = UnityEngine.Random.Range(1, 5);
                }
                lastRefreshTime = now;
            }
        }

        public ShopItem BuyItem(string itemId, Inventory inventory)
        {
            var item = items.Find(i => i.itemId == itemId);
            if (item == null || item.IsSoldOut)
                return null;
            if (inventory.GetItemCount("SoulEssence") < item.soulEssencePrice)
                return null;

            inventory.RemoveItem("SoulEssence", item.soulEssencePrice);
            if (!item.isUnlimited)
                item.stock--;
            return item;
        }
    }

    public static class ShopConfig
    {
        public static Shop CreateGoblinShop()
        {
            var shop = new Shop(ShopType.Goblin, "地精商人", true);
            shop.items = new List<ShopItem>
            {
                new ShopItem { itemId = "RefinementStone",    displayName = "强化石",       soulEssencePrice = 50,  stock = -1 },
                new ShopItem { itemId = "SocketStone",         displayName = "打孔石",       soulEssencePrice = 100, stock = -1 },
                new ShopItem { itemId = "Ruby",                displayName = "红宝石",       soulEssencePrice = 200, stock = 3 },
                new ShopItem { itemId = "Sapphire",            displayName = "蓝宝石",       soulEssencePrice = 200, stock = 3 },
                new ShopItem { itemId = "Emerald",              displayName = "绿宝石",       soulEssencePrice = 200, stock = 3 },
                new ShopItem { itemId = "Topaz",               displayName = "黄宝石",       soulEssencePrice = 200, stock = 3 },
                new ShopItem { itemId = "Amethyst",            displayName = "紫宝石",       soulEssencePrice = 300, stock = 2 },
                new ShopItem { itemId = "Diamond",              displayName = "钻石",         soulEssencePrice = 500, stock = 1 },
            };
            return shop;
        }

        public static Shop CreateWanderingShop()
        {
            var shop = new Shop(ShopType.Wandering, "流浪商人", false);
            shop.items = new List<ShopItem>
            {
                new ShopItem { itemId = "RareRecipe1", displayName = "稀有配方·壹",  soulEssencePrice = 1000, stock = 1 },
                new ShopItem { itemId = "RareRecipe2", displayName = "稀有配方·贰",  soulEssencePrice = 1200, stock = 1 },
                new ShopItem { itemId = "RareEquipment1", displayName = "流浪者之剑", soulEssencePrice = 2000, stock = 1 },
                new ShopItem { itemId = "RareMaterial", displayName = "稀有材料",     soulEssencePrice = 300,  stock = 2 },
            };
            return shop;
        }

        public static Shop CreateBlackMarketShop()
        {
            var shop = new Shop(ShopType.BlackMarket, "黑市商人", false);
            shop.items = new List<ShopItem>
            {
                new ShopItem { itemId = "QuickUpgradeToken",  displayName = "快速升级符",   soulEssencePrice = 5000, stock = 1 },
                new ShopItem { itemId = "GuaranteeStone",     displayName = "保底强化石",   soulEssencePrice = 3000, stock = 2 },
                new ShopItem { itemId = "SoulEssenceBundle",   displayName = "魂精华礼包",   soulEssencePrice = 8000, stock = 3 },
            };
            return shop;
        }

        public static Shop CreateGoblinForgeShop()
        {
            var shop = new Shop(ShopType.GoblinForge, "地精工坊", false);
            shop.items = new List<ShopItem>
            {
                new ShopItem { itemId = "SetFragment_1", displayName = "套装碎片·壹", soulEssencePrice = 1000, stock = -1 },
                new ShopItem { itemId = "SetFragment_2", displayName = "套装碎片·贰", soulEssencePrice = 1500, stock = -1 },
                new ShopItem { itemId = "SetFragment_3", displayName = "套装碎片·叁", soulEssencePrice = 2000, stock = -1 },
            };
            return shop;
        }
    }

    public class ShopManager
    {
        public List<Shop> shops = new List<Shop>();

        public ShopManager()
        {
            shops.Add(ShopConfig.CreateGoblinShop());
            shops.Add(ShopConfig.CreateWanderingShop());
            shops.Add(ShopConfig.CreateBlackMarketShop());
            shops.Add(ShopConfig.CreateGoblinForgeShop());
        }

        public Shop GetShop(ShopType type)
        {
            return shops.Find(s => s.type == type);
        }

        public void UnlockWanderingShop()
        {
            var shop = GetShop(ShopType.Wandering);
            if (shop != null) shop.isUnlocked = true;
        }

        public void UnlockBlackMarketShop()
        {
            var shop = GetShop(ShopType.BlackMarket);
            if (shop != null) shop.isUnlocked = true;
        }

        /// <summary>
        /// 完成10连续任务后解锁地精工坊
        /// </summary>
        public void UnlockGoblinForgeIfEligible(int consecutiveTasks)
        {
            if (consecutiveTasks >= 10)
            {
                var shop = GetShop(ShopType.GoblinForge);
                if (shop != null) shop.isUnlocked = true;
            }
        }

        public void RefreshAllShops()
        {
            foreach (var shop in shops)
            {
                if (shop.isUnlocked)
                    shop.RefreshStock();
            }
        }

        public List<Shop> GetAvailableShops()
        {
            return shops.FindAll(s => s.isUnlocked);
        }
    }

    // ============================================================
    //  辅助数据结构（需要配合外部 Inventory / Equipment 使用）
    // ============================================================

    /// <summary>
    /// 装备基类（需要外部实现具体装备类）
    /// 字段：refinementLevel, canHaveSockets, socketCount, socketedGems
    /// 方法：ApplyRefinementBonus, RemoveRefinementBonus
    /// </summary>
    public class Equipment
    {
        public string equipmentId;
        public string equipmentName;
        public int baseHP;
        public int baseDefense;
        public float baseDodge;
        public float baseCrit;
        public float baseSoulEssenceBonus;

        public int refinementLevel = 0;   // +0 ~ +10
        public bool canHaveSockets = true;
        public int socketCount = 0;
        public GemType[] socketedGems = new GemType[SocketSystem.MAX_SOCKETS]
        {
            GemType.None, GemType.None, GemType.None
        };

        // 当前生效的最终属性（含强化+宝石）
        public int CurrentHP => CalculateFinalStat(e => e.baseHP, g => g.hpBonus);
        public int CurrentDefense => CalculateFinalStat(e => e.baseDefense, g => g.defenseBonus);
        public float CurrentDodge => CalculateFinalStat(e => e.baseDodge, g => g.dodgeBonus);
        public float CurrentCrit => CalculateFinalStat(e => e.baseCrit, g => g.critBonus);

        private int CalculateFinalStat(Func<Equipment, int> baseGetter, Func<GemBonus, int> gemGetter)
        {
            int total = baseGetter(this);
            total += refinementLevel; // 强化每级+1
            var gemBonus = SocketSystem.CalculateTotalGemBonus(this);
            total += gemGetter(gemBonus);
            total += gemBonus.allAttrBonus;
            return total;
        }

        private float CalculateFinalStat(Func<Equipment, float> baseGetter, Func<GemBonus, float> gemGetter)
        {
            float total = baseGetter(this);
            var gemBonus = SocketSystem.CalculateTotalGemBonus(this);
            total += gemGetter(gemBonus);
            return total;
        }

        public void ApplyRefinementBonus(int attributeBonus)
        {
            refinementLevel++;
        }

        public void RemoveRefinementBonus(int targetLevel)
        {
            refinementLevel = targetLevel;
        }

        public string GetRefinementDisplay()
        {
            if (refinementLevel <= 0) return "";
            return $"+{refinementLevel}";
        }

        public string GetSocketDisplay()
        {
            var parts = new List<string>();
            for (int i = 0; i < socketCount; i++)
            {
                if (socketedGems[i] != GemType.None)
                    parts.Add(GemBonus.GetGemBonus(socketedGems[i]).GetDisplayName());
                else
                    parts.Add("空孔");
            }
            return $"[{string.Join("|", parts)}]";
        }
    }

    /// <summary>
    /// 背包/库存接口（需要外部实现具体库存类）
    /// 方法：GetItemCount, RemoveItem, AddItem
    /// </summary>
    public class Inventory
    {
        private Dictionary<string, int> items = new Dictionary<string, int>();

        public int GetItemCount(string itemId)
        {
            return items.ContainsKey(itemId) ? items[itemId] : 0;
        }

        public void AddItem(string itemId, int count = 1)
        {
            if (!items.ContainsKey(itemId))
                items[itemId] = 0;
            items[itemId] += count;
        }

        public bool RemoveItem(string itemId, int count = 1)
        {
            if (!items.ContainsKey(itemId) || items[itemId] < count)
                return false;
            items[itemId] -= count;
            return true;
        }

        public void SetItemCount(string itemId, int count)
        {
            items[itemId] = count;
        }
    }
}
