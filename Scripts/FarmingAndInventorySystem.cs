using System;
using System.Collections.Generic;
using UnityEngine;

namespace ForestJournal
{
    // =============================================
    // 物品类型枚举
    // =============================================
    public enum ItemType
    {
        Consumable,  // 消耗品
        Equipment,   // 装备
        Material,    // 材料
        QuestItem    // 任务物品
    }

    // =============================================
    // 物品数据
    // =============================================
    [Serializable]
    public class ItemData
    {
        public string id;
        public string name;
        public ItemType type;
        public Sprite icon;
        public string description;
        public int maxStack;          // 最大堆叠数
        public bool canTrade;          // 是否可交易
        public bool canDiscard;        // 是否可丢弃

        // 消耗品特有
        public int healAmount;         // 治疗量
        public int effectValue;        // 效果值

        // 种子特有
        public CropData cropData;      // 关联作物数据

        public ItemData Clone()
        {
            return new ItemData
            {
                id = this.id,
                name = this.name,
                type = this.type,
                icon = this.icon,
                description = this.description,
                maxStack = this.maxStack,
                canTrade = this.canTrade,
                canDiscard = this.canDiscard,
                healAmount = this.healAmount,
                effectValue = this.effectValue,
                cropData = this.cropData
            };
        }
    }

    // =============================================
    // 物品实例（带数量）
    // =============================================
    [Serializable]
    public class ItemInstance
    {
        public string instanceId;      // 唯一实例ID
        public ItemData data;
        public int count;

        public ItemInstance(ItemData data, int count = 1)
        {
            this.instanceId = Guid.NewGuid().ToString();
            this.data = data;
            this.count = Mathf.Min(count, data.maxStack);
        }

        public bool CanStackWith(ItemData otherData)
        {
            return data.id == otherData.id && count < data.maxStack;
        }

        public int TryAdd(int amount)
        {
            int space = data.maxStack - count;
            int toAdd = Mathf.Min(amount, space);
            count += toAdd;
            return amount - toAdd;
        }
    }

    // =============================================
    // 背包格子
    // =============================================
    [Serializable]
    public class InventorySlot
    {
        public int index;
        public ItemInstance item;
        public bool isLocked;         // 是否锁定（防止误操作）

        public InventorySlot(int index)
        {
            this.index = index;
            this.item = null;
            this.isLocked = false;
        }

        public bool IsEmpty => item == null || item.count <= 0;
        public bool HasItem => !IsEmpty;
    }

    // =============================================
    // 背包
    // =============================================
    [Serializable]
    public class Inventory
    {
        public string ownerId;                     // 拥有者ID
        public List<InventorySlot> slots;
        public int baseCapacity = 20;
        public int maxCapacity = 40;

        public Inventory(string ownerId, int baseCapacity = 20, int maxCapacity = 40)
        {
            this.ownerId = ownerId;
            this.baseCapacity = baseCapacity;
            this.maxCapacity = maxCapacity;
            this.slots = new List<InventorySlot>();
            for (int i = 0; i < baseCapacity; i++)
            {
                slots.Add(new InventorySlot(i));
            }
        }

        public int CurrentCapacity => slots.Count;

        public bool IsFull => GetEmptySlotCount() == 0;

        public int GetEmptySlotCount()
        {
            int count = 0;
            foreach (var slot in slots)
            {
                if (slot.IsEmpty) count++;
            }
            return count;
        }

        // 尝试扩展容量
        public bool TryExpand(int targetSize)
        {
            if (targetSize > maxCapacity) return false;
            while (slots.Count < targetSize)
            {
                slots.Add(new InventorySlot(slots.Count));
            }
            return true;
        }

        // 添加物品（自动堆叠）
        public bool AddItem(ItemData data, int amount = 1)
        {
            // 先尝试堆叠到现有物品
            foreach (var slot in slots)
            {
                if (slot.HasItem && slot.item.CanStackWith(data))
                {
                    int remaining = slot.item.TryAdd(amount);
                    if (remaining == 0) return true;
                    amount = remaining;
                }
            }

            // 尝试放到空格子
            foreach (var slot in slots)
            {
                if (slot.IsEmpty)
                {
                    int toAdd = Mathf.Min(amount, data.maxStack);
                    slot.item = new ItemInstance(data, toAdd);
                    amount -= toAdd;
                    if (amount == 0) return true;
                }
            }

            return amount == 0;
        }

        // 移除物品
        public bool RemoveItem(string instanceId, int amount = 1)
        {
            foreach (var slot in slots)
            {
                if (slot.HasItem && slot.item.instanceId == instanceId)
                {
                    slot.item.count -= amount;
                    if (slot.item.count <= 0)
                    {
                        slot.item = null;
                    }
                    return true;
                }
            }
            return false;
        }

        // 移动物品到另一个格子
        public bool MoveItem(int fromIndex, int toIndex, Inventory targetInventory = null)
        {
            if (fromIndex < 0 || fromIndex >= slots.Count) return false;

            var fromSlot = slots[fromIndex];
            if (fromSlot.IsEmpty) return false;

            InventorySlot toSlot;
            if (targetInventory != null)
            {
                if (toIndex < 0 || toIndex >= targetInventory.slots.Count) return false;
                toSlot = targetInventory.slots[toIndex];
            }
            else
            {
                if (toIndex < 0 || toIndex >= slots.Count) return false;
                toSlot = slots[toIndex];
            }

            // 目标格子为空，直接移动
            if (toSlot.IsEmpty)
            {
                toSlot.item = fromSlot.item;
                fromSlot.item = null;
                return true;
            }

            // 目标格子有相同物品，尝试堆叠
            if (toSlot.item.CanStackWith(fromSlot.item.data))
            {
                int remaining = toSlot.item.TryAdd(fromSlot.item.count);
                if (remaining == 0)
                {
                    fromSlot.item = null;
                }
                else
                {
                    fromSlot.item.count = remaining;
                }
                return true;
            }

            // 目标格子有不同物品，交换
            var temp = toSlot.item;
            toSlot.item = fromSlot.item;
            fromSlot.item = temp;
            return true;
        }

        // 整理背包（合并同类物品）
        public void Organize()
        {
            List<ItemInstance> allItems = new List<ItemInstance>();
            foreach (var slot in slots)
            {
                if (slot.HasItem)
                {
                    allItems.Add(slot.item);
                    slot.item = null;
                }
            }

            // 按物品ID排序
            allItems.Sort((a, b) => a.data.id.CompareTo(b.data.id));

            // 重新放置
            int slotIndex = 0;
            foreach (var item in allItems)
            {
                while (slotIndex < slots.Count && !slots[slotIndex].IsEmpty)
                {
                    slotIndex++;
                }
                if (slotIndex < slots.Count)
                {
                    slots[slotIndex].item = item;
                }
            }
        }

        // 获取物品数量
        public int GetItemCount(string itemId)
        {
            int count = 0;
            foreach (var slot in slots)
            {
                if (slot.HasItem && slot.item.data.id == itemId)
                {
                    count += slot.item.count;
                }
            }
            return count;
        }

        // 检查是否有物品
        public bool HasItem(string itemId, int amount = 1)
        {
            return GetItemCount(itemId) >= amount;
        }

        // 消耗物品
        public bool ConsumeItem(string itemId, int amount = 1)
        {
            if (!HasItem(itemId, amount)) return false;

            int toRemove = amount;
            foreach (var slot in slots)
            {
                if (slot.HasItem && slot.item.data.id == itemId)
                {
                    int removeFromSlot = Mathf.Min(toRemove, slot.item.count);
                    slot.item.count -= removeFromSlot;
                    toRemove -= removeFromSlot;
                    if (slot.item.count <= 0)
                    {
                        slot.item = null;
                    }
                    if (toRemove == 0) break;
                }
            }
            return true;
        }
    }

    // =============================================
    // 仓库（营地共享存储）
    // =============================================
    [Serializable]
    public class Warehouse
    {
        public string campId;
        public List<InventorySlot> slots;
        public int baseCapacity = 50;
        public int maxCapacity = 100;
        public bool hasStorageBoxUpgrade = false;

        public Warehouse(string campId)
        {
            this.campId = campId;
            this.slots = new List<InventorySlot>();
            for (int i = 0; i < baseCapacity; i++)
            {
                slots.Add(new InventorySlot(i));
            }
        }

        public int CurrentCapacity => slots.Count;

        public bool IsFull => GetEmptySlotCount() == 0;

        public int GetEmptySlotCount()
        {
            int count = 0;
            foreach (var slot in slots)
            {
                if (slot.IsEmpty) count++;
            }
            return count;
        }

        // 建造储物箱升级
        public void BuildStorageBox()
        {
            if (hasStorageBoxUpgrade) return;
            hasStorageBoxUpgrade = true;
            TryExpand(maxCapacity);
        }

        public bool TryExpand(int targetSize)
        {
            if (targetSize > maxCapacity) return false;
            while (slots.Count < targetSize)
            {
                slots.Add(new InventorySlot(slots.Count));
            }
            return true;
        }

        // 添加物品
        public bool AddItem(ItemData data, int amount = 1)
        {
            foreach (var slot in slots)
            {
                if (slot.HasItem && slot.item.CanStackWith(data))
                {
                    int remaining = slot.item.TryAdd(amount);
                    if (remaining == 0) return true;
                    amount = remaining;
                }
            }

            foreach (var slot in slots)
            {
                if (slot.IsEmpty)
                {
                    int toAdd = Mathf.Min(amount, data.maxStack);
                    slot.item = new ItemInstance(data, toAdd);
                    amount -= toAdd;
                    if (amount == 0) return true;
                }
            }

            return amount == 0;
        }

        // 移除物品
        public bool RemoveItem(string instanceId, int amount = 1)
        {
            foreach (var slot in slots)
            {
                if (slot.HasItem && slot.item.instanceId == instanceId)
                {
                    slot.item.count -= amount;
                    if (slot.item.count <= 0)
                    {
                        slot.item = null;
                    }
                    return true;
                }
            }
            return false;
        }

        // 整理
        public void Organize()
        {
            List<ItemInstance> allItems = new List<ItemInstance>();
            foreach (var slot in slots)
            {
                if (slot.HasItem)
                {
                    allItems.Add(slot.item);
                    slot.item = null;
                }
            }

            allItems.Sort((a, b) => a.data.id.CompareTo(b.data.id));

            int slotIndex = 0;
            foreach (var item in allItems)
            {
                while (slotIndex < slots.Count && !slots[slotIndex].IsEmpty)
                {
                    slotIndex++;
                }
                if (slotIndex < slots.Count)
                {
                    slots[slotIndex].item = item;
                }
            }
        }

        public int GetItemCount(string itemId)
        {
            int count = 0;
            foreach (var slot in slots)
            {
                if (slot.HasItem && slot.item.data.id == itemId)
                {
                    count += slot.item.count;
                }
            }
            return count;
        }

        public bool HasItem(string itemId, int amount = 1)
        {
            return GetItemCount(itemId) >= amount;
        }

        public bool ConsumeItem(string itemId, int amount = 1)
        {
            if (!HasItem(itemId, amount)) return false;

            int toRemove = amount;
            foreach (var slot in slots)
            {
                if (slot.HasItem && slot.item.data.id == itemId)
                {
                    int removeFromSlot = Mathf.Min(toRemove, slot.item.count);
                    slot.item.count -= removeFromSlot;
                    toRemove -= removeFromSlot;
                    if (slot.item.count <= 0)
                    {
                        slot.item = null;
                    }
                    if (toRemove == 0) break;
                }
            }
            return true;
        }
    }

    // =============================================
    // 作物数据
    // =============================================
    [Serializable]
    public class CropData
    {
        public string cropId;
        public string name;
        public string seedId;              // 对应种子ID
        public int seedCost;                // 种子消耗数量
        public int growthDays;              // 成熟天数
        public int harvestInterval;         // 收获间隔天数
        public int baseYield;               // 基础产量
        public float yieldBonus = 0f;       // 产量加成
        public int effectiveGrowthDays;     // 实际成熟天数（受加成影响）

        // 特殊标记
        public bool isPoisonous = false;   // 有毒
        public bool isMedicinal = false;   // 入药
        public bool isSellable = false;    // 可卖
        public bool isRare = false;         // 稀有
        public bool isLegendary = false;    // 传说

        // 产物
        public string productId;            // 产出物ID
    }

    // =============================================
    // 农田地块状态
    // =============================================
    public enum PlotState
    {
        Empty,         // 空地
        Planted,       // 已种植
        Growing,       // 生长中
        Ready,         // 可收获
        Withered       // 枯萎（旱死）
    }

    // =============================================
    // 农田地块
    // =============================================
    [Serializable]
    public class FarmPlot
    {
        public int index;                  // 地块索引 0-8
        public PlotState state = PlotState.Empty;
        public string cropId;              // 种植的作物ID
        public int plantedDay;             // 种植日（游戏天数）
        public int lastWateredDay;         // 最后浇水日
        public int harvestCount;           // 已收获次数
        public bool needsWater = false;    // 是否需要浇水
        public bool wasAutoWatered = false; // 是否被雨自动照料

        public FarmPlot(int index)
        {
            this.index = index;
            this.state = PlotState.Empty;
        }

        public bool IsUnlocked
        {
            get
            {
                if (index < 3) return true;
                if (index < 6) return HasBoneHoeUnlock;
                return HasAncientBookUnlock;
            }
        }

        // 解锁条件
        public static bool HasBoneHoeUnlock = false;
        public static bool HasAncientBookUnlock = false;
    }

    // =============================================
    // 农场系统
    // =============================================
    [Serializable]
    public class Farm
    {
        public string campId;
        public List<FarmPlot> plots;
        public int maxUnlockedPlots = 3;   // 默认3块田地

        // 解锁条件
        public static string BoneHoeUnlockItemId = "bone_hoe";
        public static string AncientBookUnlockItemId = "ancient_farming_book";

        public Farm(string campId)
        {
            this.campId = campId;
            this.plots = new List<FarmPlot>();
            for (int i = 0; i < 9; i++)
            {
                plots.Add(new FarmPlot(i));
            }
        }

        public int GetUnlockedPlotCount()
        {
            int count = 3; // 默认3块
            if (FarmPlot.HasBoneHoeUnlock) count = Mathf.Max(count, 6);
            if (FarmPlot.HasAncientBookUnlock) count = 9;
            return count;
        }

        public List<FarmPlot> GetUnlockedPlots()
        {
            int unlocked = GetUnlockedPlotCount();
            return plots.GetRange(0, unlocked);
        }

        public List<FarmPlot> GetEmptyPlots()
        {
            List<FarmPlot> empty = new List<FarmPlot>();
            foreach (var plot in GetUnlockedPlots())
            {
                if (plot.state == PlotState.Empty)
                {
                    empty.Add(plot);
                }
            }
            return empty;
        }

        public List<FarmPlot> GetReadyPlots()
        {
            List<FarmPlot> ready = new List<FarmPlot>();
            foreach (var plot in GetUnlockedPlots())
            {
                if (plot.state == PlotState.Ready)
                {
                    ready.Add(plot);
                }
            }
            return ready;
        }

        public List<FarmPlot> GetGrowingPlots()
        {
            List<FarmPlot> growing = new List<FarmPlot>();
            foreach (var plot in GetUnlockedPlots())
            {
                if (plot.state == PlotState.Growing || plot.state == PlotState.Planted)
                {
                    growing.Add(plot);
                }
            }
            return growing;
        }

        // 尝试解锁更多田地
        public bool TryUnlockWithBoneHoe()
        {
            if (FarmPlot.HasBoneHoeUnlock) return false;
            FarmPlot.HasBoneHoeUnlock = true;
            return true;
        }

        public bool TryUnlockWithAncientBook()
        {
            if (FarmPlot.HasAncientBookUnlock) return false;
            FarmPlot.HasAncientBookUnlock = true;
            return true;
        }

        // 种植作物
        public bool PlantCrop(int plotIndex, string cropId, int currentDay, CropData cropData)
        {
            if (plotIndex < 0 || plotIndex >= plots.Count) return false;
            var plot = plots[plotIndex];

            if (!plot.IsUnlocked) return false;
            if (plot.state != PlotState.Empty) return false;

            plot.state = PlotState.Planted;
            plot.cropId = cropId;
            plot.plantedDay = currentDay;
            plot.lastWateredDay = currentDay;
            plot.harvestCount = 0;
            plot.needsWater = false;

            return true;
        }

        // 浇水
        public bool WaterPlot(int plotIndex, int currentDay)
        {
            if (plotIndex < 0 || plotIndex >= plots.Count) return false;
            var plot = plots[plotIndex];

            if (plot.state != PlotState.Planted && plot.state != PlotState.Growing) return false;

            plot.lastWateredDay = currentDay;
            plot.needsWater = false;
            plot.wasAutoWatered = false;
            return true;
        }

        // 自动雨季浇水
        public void AutoWaterAll(int currentDay)
        {
            foreach (var plot in GetUnlockedPlots())
            {
                if (plot.state == PlotState.Planted || plot.state == PlotState.Growing)
                {
                    plot.lastWateredDay = currentDay;
                    plot.needsWater = false;
                    plot.wasAutoWatered = true;
                }
            }
        }

        // 更新生长状态
        public void UpdateGrowth(int currentDay, CropData[] allCrops, bool hasForestBlessing)
        {
            foreach (var plot in GetUnlockedPlots())
            {
                if (plot.state == PlotState.Empty || plot.state == PlotState.Ready) continue;

                // 获取作物数据
                CropData cropData = null;
                foreach (var c in allCrops)
                {
                    if (c.cropId == plot.cropId)
                    {
                        cropData = c;
                        break;
                    }
                }
                if (cropData == null) continue;

                // 计算实际成熟天数
                int effectiveGrowthDays = cropData.effectiveGrowthDays;
                if (hasForestBlessing)
                {
                    effectiveGrowthDays = Mathf.Max(1, effectiveGrowthDays - 1);
                }

                // 检查是否到成熟时间
                int daysSincePlant = currentDay - plot.plantedDay;
                int harvestCycle = effectiveGrowthDays + (plot.harvestCount * cropData.harvestInterval);

                if (daysSincePlant >= harvestCycle)
                {
                    plot.state = PlotState.Ready;
                }
                else
                {
                    plot.state = PlotState.Growing;
                }

                // 检查是否需要浇水（超过2天没浇水有概率旱死）
                int daysWithoutWater = currentDay - plot.lastWateredDay;
                if (daysWithoutWater >= 2)
                {
                    plot.needsWater = true;
                    // 每天有30%概率旱死
                    if (UnityEngine.Random.value < 0.3f)
                    {
                        plot.state = PlotState.Withered;
                    }
                }
            }
        }

        // 收获
        public int Harvest(int plotIndex, CropData[] allCrops, bool hasBoneHoe, out string productId)
        {
            productId = null;

            if (plotIndex < 0 || plotIndex >= plots.Count) return 0;
            var plot = plots[plotIndex];

            if (plot.state != PlotState.Ready) return 0;

            // 获取作物数据
            CropData cropData = null;
            foreach (var c in allCrops)
            {
                if (c.cropId == plot.cropId)
                {
                    cropData = c;
                    break;
                }
            }
            if (cropData == null) return 0;

            // 计算产量（骨锄+20%）
            int yield = cropData.baseYield;
            if (hasBoneHoe)
            {
                yield = Mathf.CeilToInt(yield * 1.2f);
            }

            productId = cropData.productId;
            plot.harvestCount++;
            plot.state = PlotState.Growing;

            return yield;
        }

        // 铲除作物
        public bool RemoveCrop(int plotIndex)
        {
            if (plotIndex < 0 || plotIndex >= plots.Count) return false;
            var plot = plots[plotIndex];

            if (plot.state == PlotState.Empty) return false;

            plot.state = PlotState.Empty;
            plot.cropId = null;
            plot.plantedDay = 0;
            plot.lastWateredDay = 0;
            plot.harvestCount = 0;
            plot.needsWater = false;

            return true;
        }
    }

    // =============================================
    // 作物定义（静态数据）
    // =============================================
    public static class CropDefinitions
    {
        public static CropData[] AllCrops = new CropData[]
        {
            new CropData
            {
                cropId = "red_berry",
                name = "红浆果",
                seedId = "seed_red_berry",
                seedCost = 3,
                growthDays = 3,
                harvestInterval = 3,
                baseYield = 5,
                effectiveGrowthDays = 3,
                productId = "red_berry"
            },
            new CropData
            {
                cropId = "blue_mushroom",
                name = "蓝蘑菇",
                seedId = "seed_blue_mushroom",
                seedCost = 2,
                growthDays = 2,
                harvestInterval = 2,
                baseYield = 3,
                effectiveGrowthDays = 2,
                productId = "blue_mushroom",
                isPoisonous = true
            },
            new CropData
            {
                cropId = "silver_leaf",
                name = "银叶草",
                seedId = "seed_silver_leaf",
                seedCost = 2,
                growthDays = 4,
                harvestInterval = 4,
                baseYield = 4,
                effectiveGrowthDays = 4,
                productId = "silver_leaf",
                isMedicinal = true
            },
            new CropData
            {
                cropId = "night_bloom",
                name = "夜光花",
                seedId = "seed_night_bloom",
                seedCost = 3,
                growthDays = 5,
                harvestInterval = 5,
                baseYield = 2,
                effectiveGrowthDays = 5,
                productId = "night_bloom",
                isSellable = true
            },
            new CropData
            {
                cropId = "herb",
                name = "香草",
                seedId = "seed_herb",
                seedCost = 2,
                growthDays = 2,
                harvestInterval = 2,
                baseYield = 6,
                effectiveGrowthDays = 2,
                productId = "herb"
            },
            new CropData
            {
                cropId = "healing_grass",
                name = "治愈草",
                seedId = "seed_healing_grass",
                seedCost = 4,
                growthDays = 6,
                harvestInterval = 6,
                baseYield = 3,
                effectiveGrowthDays = 6,
                productId = "healing_grass",
                isMedicinal = true
            },
            new CropData
            {
                cropId = "dream_leaf",
                name = "梦叶",
                seedId = "seed_dream_leaf",
                seedCost = 5,
                growthDays = 8,
                harvestInterval = 8,
                baseYield = 2,
                effectiveGrowthDays = 8,
                productId = "dream_leaf",
                isRare = true
            },
            new CropData
            {
                cropId = "soul_bloom",
                name = "灵魂花",
                seedId = "seed_soul_bloom",
                seedCost = 8,
                growthDays = 10,
                harvestInterval = 10,
                baseYield = 1,
                effectiveGrowthDays = 10,
                productId = "soul_bloom",
                isLegendary = true
            }
        };

        public static CropData GetCrop(string cropId)
        {
            foreach (var crop in AllCrops)
            {
                if (crop.cropId == cropId) return crop;
            }
            return null;
        }

        public static CropData GetCropBySeed(string seedId)
        {
            foreach (var crop in AllCrops)
            {
                if (crop.seedId == seedId) return crop;
            }
            return null;
        }
    }

    // =============================================
    // 物品定义（静态数据）
    // =============================================
    public static class ItemDefinitions
    {
        public static Dictionary<string, ItemData> AllItems = new Dictionary<string, ItemData>();

        static ItemDefinitions()
        {
            InitializeItems();
        }

        private static void InitializeItems()
        {
            // ========== 种子 ==========
            AddItem(new ItemData
            {
                id = "seed_red_berry",
                name = "红浆果种子",
                type = ItemType.Material,
                description = "红浆果的种子，种植后3天成熟。",
                maxStack = 99,
                canTrade = true,
                canDiscard = true
            });

            AddItem(new ItemData
            {
                id = "seed_blue_mushroom",
                name = "蓝蘑菇孢子",
                type = ItemType.Material,
                description = "蓝蘑菇的孢子，种植后2天成熟。有毒。",
                maxStack = 99,
                canTrade = true,
                canDiscard = true
            });

            AddItem(new ItemData
            {
                id = "seed_silver_leaf",
                name = "银叶草种子",
                type = ItemType.Material,
                description = "银叶草的种子，种植后4天成熟。可入药。",
                maxStack = 99,
                canTrade = true,
                canDiscard = true
            });

            AddItem(new ItemData
            {
                id = "seed_night_bloom",
                name = "夜光花种子",
                type = ItemType.Material,
                description = "夜光花的种子，种植后5天成熟。可出售。",
                maxStack = 99,
                canTrade = true,
                canDiscard = true
            });

            AddItem(new ItemData
            {
                id = "seed_herb",
                name = "香草种子",
                type = ItemType.Material,
                description = "香草的种子，种植后2天成熟。烹饪用。",
                maxStack = 99,
                canTrade = true,
                canDiscard = true
            });

            AddItem(new ItemData
            {
                id = "seed_healing_grass",
                name = "治愈草种子",
                type = ItemType.Material,
                description = "治愈草的种子，种植后6天成熟。可入药和直接使用。",
                maxStack = 99,
                canTrade = true,
                canDiscard = true
            });

            AddItem(new ItemData
            {
                id = "seed_dream_leaf",
                name = "梦叶种子",
                type = ItemType.Material,
                description = "梦叶的种子，种植后8天成熟。稀有材料。",
                maxStack = 99,
                canTrade = true,
                canDiscard = true
            });

            AddItem(new ItemData
            {
                id = "seed_soul_bloom",
                name = "灵魂花种子",
                type = ItemType.Material,
                description = "灵魂花的种子，种植后10天成熟。传说材料。",
                maxStack = 99,
                canTrade = true,
                canDiscard = true
            });

            // ========== 产物 ==========
            AddItem(new ItemData
            {
                id = "red_berry",
                name = "红浆果",
                type = ItemType.Consumable,
                description = "酸甜可口的浆果，可食用或制作。",
                maxStack = 50,
                canTrade = true,
                canDiscard = true,
                healAmount = 10
            });

            AddItem(new ItemData
            {
                id = "blue_mushroom",
                name = "蓝蘑菇",
                type = ItemType.Consumable,
                description = "有剧毒的蘑菇。误食会造成伤害。",
                maxStack = 50,
                canTrade = true,
                canDiscard = true,
                healAmount = -15  // 负数表示造成伤害
            });

            AddItem(new ItemData
            {
                id = "silver_leaf",
                name = "银叶草",
                type = ItemType.Material,
                description = "银白色的草叶，具有药用价值。",
                maxStack = 50,
                canTrade = true,
                canDiscard = true,
                isMedicinal = true
            });

            AddItem(new ItemData
            {
                id = "night_bloom",
                name = "夜光花",
                type = ItemType.Material,
                description = "夜间发光的美丽花朵，可出售获得金币。",
                maxStack = 50,
                canTrade = true,
                canDiscard = true,
                isSellable = true,
                effectValue = 50  // 出售价格
            });

            AddItem(new ItemData
            {
                id = "herb",
                name = "香草",
                type = ItemType.Material,
                description = "烹饪用香草，可提升食物效果。",
                maxStack = 50,
                canTrade = true,
                canDiscard = true
            });

            AddItem(new ItemData
            {
                id = "healing_grass",
                name = "治愈草",
                type = ItemType.Consumable,
                description = "具有治愈效果的草药，可直接使用或制作。",
                maxStack = 50,
                canTrade = true,
                canDiscard = true,
                healAmount = 30,
                isMedicinal = true
            });

            AddItem(new ItemData
            {
                id = "dream_leaf",
                name = "梦叶",
                type = ItemType.Material,
                description = "稀有材料，据说可以入药或制作稀有道具。",
                maxStack = 30,
                canTrade = true,
                canDiscard = true,
                isRare = true
            });

            AddItem(new ItemData
            {
                id = "soul_bloom",
                name = "灵魂花",
                type = ItemType.Material,
                description = "传说中灵魂花，蕴含神秘力量。",
                maxStack = 10,
                canTrade = true,
                canDiscard = false,
                isLegendary = true
            });

            // ========== 特殊道具 ==========
            AddItem(new ItemData
            {
                id = "bone_hoe",
                name = "骨锄",
                type = ItemType.Equipment,
                description = "用骨头制成的锄头。装备后作物产量+20%%。解锁田地4-6。",
                maxStack = 1,
                canTrade = false,
                canDiscard = false,
                effectValue = 20  // 产量加成%
            });

            AddItem(new ItemData
            {
                id = "ancient_farming_book",
                name = "古代农业典籍",
                type = ItemType.QuestItem,
                description = "记载古老农业知识的书籍。解锁田地7-9。",
                maxStack = 1,
                canTrade = false,
                canDiscard = false
            });

            AddItem(new ItemData
            {
                id = "water_bucket",
                name = "水桶",
                type = ItemType.Material,
                description = "用于浇灌农田的水桶。雨季可免费获得，或从河采集。",
                maxStack = 10,
                canTrade = true,
                canDiscard = true
            });

            // 森林祝福（buff类）
            AddItem(new ItemData
            {
                id = "forest_blessing",
                name = "森林祝福",
                type = ItemType.Consumable,
                description = "受到森林祝福的庇护，作物成熟时间-1天。",
                maxStack = 1,
                canTrade = false,
                canDiscard = false
            });
        }

        private static void AddItem(ItemData item)
        {
            if (item.icon == null)
            {
                item.icon = CreateDefaultIcon(item.id, item.name);
            }
            AllItems[item.id] = item;
        }

        private static Sprite CreateDefaultIcon(string id, string name)
        {
            // 在实际项目中应该加载真实图标
            // 这里返回null，UI层可以处理
            return null;
        }

        public static ItemData GetItem(string itemId)
        {
            return AllItems.TryGetValue(itemId, out var item) ? item : null;
        }

        public static ItemData CloneItem(string itemId)
        {
            var item = GetItem(itemId);
            return item?.Clone();
        }
    }

    // =============================================
    // 天气系统（与农场交互）
    // =============================================
    public enum WeatherType
    {
        Sunny,   // 晴天
        Rainy,   // 雨天
        Cloudy,  // 多云
        Stormy   // 暴风雨
    }

    // =============================================
    // 游戏日期系统
    // =============================================
    [Serializable]
    public class GameDate
    {
        public int day = 1;
        public int season = 0;  // 0=春, 1=夏, 2=秋, 3=冬
        public int year = 1;
        public WeatherType weather = WeatherType.Sunny;

        public int TotalDays => (year - 1) * 360 + season * 90 + day;

        public void NextDay()
        {
            day++;
            if (day > 90)
            {
                day = 1;
                season++;
                if (season > 3)
                {
                    season = 0;
                    year++;
                }
            }

            // 随机天气（可以接入更复杂的气候系统）
            float rand = UnityEngine.Random.value;
            if (rand < 0.4f) weather = WeatherType.Sunny;
            else if (rand < 0.7f) weather = WeatherType.Cloudy;
            else if (rand < 0.9f) weather = WeatherType.Rainy;
            else weather = WeatherType.Stormy;
        }

        public bool IsRainySeason => weather == WeatherType.Rainy || weather == WeatherType.Stormy;
    }

    // =============================================
    // 农场和背包系统管理器（单例）
    // =============================================
    public class FarmingAndInventorySystem : MonoBehaviour
    {
        private static FarmingAndInventorySystem _instance;
        public static FarmingAndInventorySystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("FarmingAndInventorySystem");
                    _instance = go.AddComponent<FarmingAndInventorySystem>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        // 全队共享的仓库
        public Warehouse campWarehouse;

        // 全队的农场
        public Farm campFarm;

        // 角色背包字典 <characterId, Inventory>
        private Dictionary<string, Inventory> characterInventories = new Dictionary<string, Inventory>();

        // 当前游戏日期
        public GameDate gameDate = new GameDate();

        // Buff状态
        private bool hasForestBlessing = false;
        private bool hasBoneHoe = false;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            // 初始化营地仓库和农场
            if (campWarehouse == null)
            {
                campWarehouse = new Warehouse("main_camp");
            }
            if (campFarm == null)
            {
                campFarm = new Farm("main_camp");
            }
        }

        // =============================================
        // 背包管理
        // =============================================

        // 获取角色背包（如果没有则创建）
        public Inventory GetCharacterInventory(string characterId)
        {
            if (!characterInventories.TryGetValue(characterId, out var inv))
            {
                inv = new Inventory(characterId, 20, 40);
                characterInventories[characterId] = inv;
            }
            return inv;
        }

        // 扩展角色背包
        public bool ExpandCharacterInventory(string characterId, int targetSize)
        {
            var inv = GetCharacterInventory(characterId);
            return inv.TryExpand(targetSize);
        }

        // 整理角色背包
        public void OrganizeCharacterInventory(string characterId)
        {
            var inv = GetCharacterInventory(characterId);
            inv.Organize();
        }

        // 添加物品到角色背包
        public bool AddItemToCharacter(string characterId, string itemId, int amount = 1)
        {
            var itemData = ItemDefinitions.GetItem(itemId);
            if (itemData == null) return false;

            var inv = GetCharacterInventory(characterId);
            return inv.AddItem(itemData, amount);
        }

        // 从角色背包移除物品
        public bool RemoveItemFromCharacter(string characterId, string instanceId, int amount = 1)
        {
            var inv = GetCharacterInventory(characterId);
            return inv.RemoveItem(instanceId, amount);
        }

        // 消耗角色背包物品
        public bool ConsumeItemFromCharacter(string characterId, string itemId, int amount = 1)
        {
            var inv = GetCharacterInventory(characterId);
            return inv.ConsumeItem(itemId, amount);
        }

        // 角色背包之间移动物品
        public bool MoveItemBetweenCharacters(string fromCharacterId, string toCharacterId, string instanceId, int amount = 1)
        {
            var fromInv = GetCharacterInventory(fromCharacterId);
            var toInv = GetCharacterInventory(toCharacterId);

            var slot = fromInv.slots.Find(s => s.HasItem && s.item.instanceId == instanceId);
            if (slot == null) return false;

            // 找到目标背包空位或可堆叠位置
            foreach (var toSlot in toInv.slots)
            {
                if (toSlot.IsEmpty || (toSlot.HasItem && toSlot.item.CanStackWith(slot.item.data)))
                {
                    return fromInv.MoveItem(slot.index, toSlot.index, toInv);
                }
            }
            return false;
        }

        // 从角色背包移动到仓库
        public bool MoveItemToWarehouse(string characterId, string instanceId)
        {
            var charInv = GetCharacterInventory(characterId);
            var slot = charInv.slots.Find(s => s.HasItem && s.item.instanceId == instanceId);
            if (slot == null) return false;

            // 找仓库空位
            foreach (var wSlot in campWarehouse.slots)
            {
                if (wSlot.IsEmpty || (wSlot.HasItem && wSlot.item.CanStackWith(slot.item.data)))
                {
                    return charInv.MoveItem(slot.index, wSlot.index, campWarehouse);
                }
            }
            return false;
        }

        // 从仓库移动到角色背包
        public bool MoveItemFromWarehouse(string characterId, string instanceId)
        {
            var charInv = GetCharacterInventory(characterId);
            var wSlot = campWarehouse.slots.Find(s => s.HasItem && s.item.instanceId == instanceId);
            if (wSlot == null) return false;

            // 找角色背包空位
            foreach (var slot in charInv.slots)
            {
                if (slot.IsEmpty || (slot.HasItem && slot.item.CanStackWith(wSlot.item.data)))
                {
                    return campWarehouse.MoveItem(wSlot.index, slot.index, charInv);
                }
            }
            return false;
        }

        // =============================================
        // 仓库管理
        // =============================================

        // 添加物品到仓库
        public bool AddItemToWarehouse(string itemId, int amount = 1)
        {
            var itemData = ItemDefinitions.GetItem(itemId);
            if (itemData == null) return false;
            return campWarehouse.AddItem(itemData, amount);
        }

        // 从仓库移除物品
        public bool RemoveItemFromWarehouse(string instanceId, int amount = 1)
        {
            return campWarehouse.RemoveItem(instanceId, amount);
        }

        // 消耗仓库物品
        public bool ConsumeItemFromWarehouse(string itemId, int amount = 1)
        {
            return campWarehouse.ConsumeItem(itemId, amount);
        }

        // 建造储物箱升级仓库
        public void BuildStorageBox()
        {
            campWarehouse.BuildStorageBox();
        }

        // 整理仓库
        public void OrganizeWarehouse()
        {
            campWarehouse.Organize();
        }

        // =============================================
        // 农场管理
        // =============================================

        // 种植作物
        public bool PlantCrop(int plotIndex, string cropId)
        {
            var cropData = CropDefinitions.GetCrop(cropId);
            if (cropData == null) return false;

            // 消耗种子
            bool hasSeeds = campWarehouse.HasItem(cropData.seedId, cropData.seedCost);
            if (!hasSeeds)
            {
                // 尝试从角色背包消耗
                foreach (var kvp in characterInventories)
                {
                    if (kvp.Value.HasItem(cropData.seedId, cropData.seedCost))
                    {
                        hasSeeds = true;
                        kvp.Value.ConsumeItem(cropData.seedId, cropData.seedCost);
                        break;
                    }
                }
            }
            else
            {
                campWarehouse.ConsumeItem(cropData.seedId, cropData.seedCost);
            }

            if (!hasSeeds) return false;

            return campFarm.PlantCrop(plotIndex, cropId, gameDate.TotalDays, cropData);
        }

        // 浇水
        public bool WaterCrop(int plotIndex)
        {
            // 消耗水桶
            bool hasBucket = campWarehouse.HasItem("water_bucket", 1);
            if (!hasBucket)
            {
                foreach (var kvp in characterInventories)
                {
                    if (kvp.Value.HasItem("water_bucket", 1))
                    {
                        hasBucket = true;
                        kvp.Value.ConsumeItem("water_bucket", 1);
                        break;
                    }
                }
            }

            if (!hasBucket) return false;

            return campFarm.WaterPlot(plotIndex, gameDate.TotalDays);
        }

        // 收获
        public int HarvestCrop(int plotIndex, out string productId)
        {
            return campFarm.Harvest(plotIndex, CropDefinitions.AllCrops, hasBoneHoe, out productId);
        }

        // 收获后自动添加到仓库
        public bool HarvestAndCollect(int plotIndex)
        {
            int yield = HarvestCrop(plotIndex, out string productId);
            if (yield <= 0 || productId == null) return false;

            return AddItemToWarehouse(productId, yield);
        }

        // 铲除作物
        public bool RemoveCrop(int plotIndex)
        {
            return campFarm.RemoveCrop(plotIndex);
        }

        // 获取田地信息
        public FarmPlot GetPlot(int index)
        {
            if (index >= 0 && index < campFarm.plots.Count)
            {
                return campFarm.plots[index];
            }
            return null;
        }

        // 获取所有解锁的田地
        public List<FarmPlot> GetUnlockedPlots()
        {
            return campFarm.GetUnlockedPlots();
        }

        // 获取可收获的田地
        public List<FarmPlot> GetReadyPlots()
        {
            return campFarm.GetReadyPlots();
        }

        // 获取生长中的田地
        public List<FarmPlot> GetGrowingPlots()
        {
            return campFarm.GetGrowingPlots();
        }

        // 获取空田地
        public List<FarmPlot> GetEmptyPlots()
        {
            return campFarm.GetEmptyPlots();
        }

        // =============================================
        // 解锁管理
        // =============================================

        // 检查并解锁骨锄
        public bool TryUnlockBoneHoe()
        {
            return campFarm.TryUnlockWithBoneHoe();
        }

        // 检查并解锁古代农业典籍
        public bool TryUnlockAncientBook()
        {
            return campFarm.TryUnlockWithAncientBook();
        }

        // =============================================
        // Buff管理
        // =============================================

        // 激活森林祝福
        public void ActivateForestBlessing()
        {
            hasForestBlessing = true;
        }

        // 激活骨锄加成
        public void ActivateBoneHoe()
        {
            hasBoneHoe = true;
        }

        // =============================================
        // 时间推进
        // =============================================

        // 新的一天
        public void NextDay()
        {
            gameDate.NextDay();

            // 雨季自动浇水
            if (gameDate.IsRainySeason)
            {
                campFarm.AutoWaterAll(gameDate.TotalDays);
            }

            // 更新作物生长状态
            campFarm.UpdateGrowth(gameDate.TotalDays, CropDefinitions.AllCrops, hasForestBlessing);
        }

        // =============================================
        // 便捷查询
        // =============================================

        // 获取所有角色背包中某物品总数
        public int GetTotalItemCount(string itemId)
        {
            int count = campWarehouse.GetItemCount(itemId);
            foreach (var kvp in characterInventories)
            {
                count += kvp.Value.GetItemCount(itemId);
            }
            return count;
        }

        // 检查是否有足够物品
        public bool HasEnoughItems(string itemId, int amount)
        {
            return GetTotalItemCount(itemId) >= amount;
        }

        // 消耗物品（优先从仓库，再从背包）
        public bool ConsumeItems(string itemId, int amount)
        {
            if (!HasEnoughItems(itemId, amount)) return false;

            int remaining = amount;

            // 先从仓库消耗
            if (campWarehouse.HasItem(itemId, remaining))
            {
                campWarehouse.ConsumeItem(itemId, remaining);
                return true;
            }
            else
            {
                remaining -= campWarehouse.GetItemCount(itemId);
                campWarehouse.ConsumeItem(itemId, campWarehouse.GetItemCount(itemId));
            }

            // 再从角色背包消耗
            foreach (var kvp in characterInventories)
            {
                if (kvp.Value.HasItem(itemId, remaining))
                {
                    kvp.Value.ConsumeItem(itemId, remaining);
                    return true;
                }
                else
                {
                    remaining -= kvp.Value.GetItemCount(itemId);
                    kvp.Value.ConsumeItem(itemId, kvp.Value.GetItemCount(itemId));
                }
            }

            return remaining <= 0;
        }

        // 添加物品（优先到仓库，再分散到背包）
        public bool AddItems(string itemId, int amount)
        {
            var itemData = ItemDefinitions.GetItem(itemId);
            if (itemData == null) return false;

            int remaining = amount;

            // 先尝试加入仓库
            while (remaining > 0)
            {
                int before = campWarehouse.GetItemCount(itemId);
                bool success = campWarehouse.AddItem(itemData, remaining);
                int after = campWarehouse.GetItemCount(itemId);
                remaining -= (after - before);
                if (!success || remaining == before) break;
            }

            // 仓库满了就分散到背包
            if (remaining > 0)
            {
                foreach (var kvp in characterInventories)
                {
                    while (remaining > 0 && !kvp.Value.IsFull)
                    {
                        int before = kvp.Value.GetItemCount(itemId);
                        kvp.Value.AddItem(itemData, remaining);
                        int after = kvp.Value.GetItemCount(itemId);
                        remaining -= (after - before);
                        if (remaining == 0) return true;
                    }
                }
            }

            return remaining == 0;
        }

        // =============================================
        // 存档/读档
        // =============================================

        [Serializable]
        private class SaveData
        {
            public Warehouse warehouse;
            public Farm farm;
            public Dictionary<string, Inventory> inventories;
            public GameDate date;
            public bool forestBlessing;
            public bool boneHoe;
        }

        public string GetSaveJson()
        {
            var saveData = new SaveData
            {
                warehouse = campWarehouse,
                farm = campFarm,
                inventories = characterInventories,
                date = gameDate,
                forestBlessing = hasForestBlessing,
                boneHoe = hasBoneHoe
            };
            return JsonUtility.ToJson(saveData);
        }

        public void LoadFromJson(string json)
        {
            var saveData = JsonUtility.FromJson<SaveData>(json);
            if (saveData == null) return;

            campWarehouse = saveData.warehouse;
            campFarm = saveData.farm;
            characterInventories = saveData.inventories;
            gameDate = saveData.date;
            hasForestBlessing = saveData.forestBlessing;
            hasBoneHoe = saveData.boneHoe;
        }
    }
}