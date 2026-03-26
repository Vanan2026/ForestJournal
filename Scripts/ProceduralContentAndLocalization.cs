using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ProceduralContentAndLocalization - 程序化内容生成 + 国际化系统
/// 
/// Part A: 程序化内容生成
/// - Roguelike地图生成 (BSP树算法)
/// - 程序化敌人遭遇
/// - 程序化资源分布
/// - 程序化事件序列
/// 
/// Part B: Steam发布素材 (数据定义)
/// </summary>

public enum ThreatLevel { Safe, Low, Medium, High, Deadly }
public enum EnemyType { Melee, Ranged, Healer, Tank }
public enum ResourceType { Food, Wood, Stone, Herb, Treasure }
public enum GameRegion { Village, Forest, Swamp, Cave, Ruins, Heart }

#region Part A: 程序化内容生成

// ==================== BSP树地图生成 ====================

public class BSPNode
{
    public RectInt area;
    public BSPNode left;
    public BSPNode right;
    public RectInt room;
    public bool isLeaf => left == null && right == null;

    public BSPNode(RectInt area)
    {
        this.area = area;
    }
}

public class MapGenerator
{
    private System.Random seededRng;
    private int seed;
    private int minRoomSize = 3;
    private int maxRoomSize = 8;
    private int maxDepth = 5;

    public MapGenerator(int seed)
    {
        this.seed = seed;
        seededRng = new System.Random(seed);
    }

    /// <summary>
    /// 使用BSP树生成地图区域
    /// </summary>
    public BSPNode GenerateMap(int width, int height)
    {
        seededRng = new System.Random(seed);
        var root = new BSPNode(new RectInt(0, 0, width, height));
        SplitRecursive(root, 0);
        CreateRooms(root);
        return root;
    }

    /// <summary>
    /// 预设种子生成（可重玩）
    /// </summary>
    public static MapGenerator CreateWithSeed(int seed)
    {
        return new MapGenerator(seed);
    }

    /// <summary>
    /// 随机种子生成
    /// </summary>
    public static MapGenerator CreateRandom()
    {
        return new MapGenerator(Guid.NewGuid().GetHashCode());
    }

    private void SplitRecursive(BSPNode node, int depth)
    {
        if (depth >= maxDepth || node.area.width < minRoomSize * 2 || node.area.height < minRoomSize * 2)
            return;

        bool splitH = seededRng.NextDouble() > 0.5f;

        // 确保能正确分割
        if (node.area.width < minRoomSize * 2 && node.area.height < minRoomSize * 2)
            return;

        if (node.area.width < minRoomSize * 2) splitH = true;
        if (node.area.height < minRoomSize * 2) splitH = false;

        if (splitH)
        {
            int splitY = seededRng.Next(node.area.y + minRoomSize, node.area.yMax - minRoomSize);
            node.left = new BSPNode(new RectInt(node.area.x, node.area.y, node.area.width, splitY - node.area.y));
            node.right = new BSPNode(new RectInt(node.area.x, splitY, node.area.width, node.area.yMax - splitY));
        }
        else
        {
            int splitX = seededRng.Next(node.area.x + minRoomSize, node.area.xMax - minRoomSize);
            node.left = new BSPNode(new RectInt(node.area.x, node.area.y, splitX - node.area.x, node.area.height));
            node.right = new BSPNode(new RectInt(splitX, node.area.y, node.area.xMax - splitX, node.area.height));
        }

        SplitRecursive(node.left, depth + 1);
        SplitRecursive(node.right, depth + 1);
    }

    private void CreateRooms(BSPNode node)
    {
        if (node.isLeaf)
        {
            int roomW = seededRng.Next(minRoomSize, Mathf.Min(maxRoomSize, node.area.width - 1));
            int roomH = seededRng.Next(minRoomSize, Mathf.Min(maxRoomSize, node.area.height - 1));
            int roomX = seededRng.Next(node.area.x, node.area.xMax - roomW);
            int roomY = seededRng.Next(node.area.y, node.area.yMax - roomH);
            node.room = new RectInt(roomX, roomY, roomW, roomH);
        }
        else
        {
            if (node.left != null) CreateRooms(node.left);
            if (node.right != null) CreateRooms(node.right);
        }
    }

    /// <summary>
    /// 获取所有房间列表
    /// </summary>
    public List<RectInt> GetAllRooms(BSPNode root)
    {
        var rooms = new List<RectInt>();
        CollectRooms(root, rooms);
        return rooms;
    }

    private void CollectRooms(BSPNode node, List<RectInt> rooms)
    {
        if (node.isLeaf)
        {
            if (node.room.width > 0 && node.room.height > 0)
                rooms.Add(node.room);
        }
        else
        {
            if (node.left != null) CollectRooms(node.left, rooms);
            if (node.right != null) CollectRooms(node.right, rooms);
        }
    }

    /// <summary>
    /// 获取房间连接信息（用于通道生成）
    /// </summary>
    public List<Vector2Int> GetRoomConnections(BSPNode root)
    {
        var connections = new List<Vector2Int>();
        CollectConnections(root, connections);
        return connections;
    }

    private void CollectConnections(BSPNode node, List<Vector2Int> connections)
    {
        if (node.left != null && node.right != null)
        {
            // 计算左右子节点房间中心连接
            var leftRoom = GetCenterRoom(node.left);
            var rightRoom = GetCenterRoom(node.right);
            if (leftRoom.HasValue && rightRoom.HasValue)
            {
                connections.Add(new Vector2Int(leftRoom.Value.x, leftRoom.Value.y));
                connections.Add(new Vector2Int(rightRoom.Value.x, rightRoom.Value.y));
            }
            CollectConnections(node.left, connections);
            CollectConnections(node.right, connections);
        }
    }

    private Vector2Int? GetCenterRoom(BSPNode node)
    {
        if (node.isLeaf)
        {
            return node.room.width > 0 
                ? new Vector2Int(node.room.x + node.room.width / 2, node.room.y + node.room.height / 2)
                : null;
        }
        var left = GetCenterRoom(node.left);
        var right = GetCenterRoom(node.right);
        if (left.HasValue && right.HasValue)
        {
            // 返回中间点
            return new Vector2Int((left.Value.x + right.Value.x) / 2, (left.Value.y + right.Value.y) / 2);
        }
        return left ?? right;
    }
}

// ==================== 程序化敌人遭遇 ====================

[Serializable]
public class EnemySpawn
{
    public string enemyId;
    public EnemyType type;
    public int weight;
    public ThreatLevel minThreat;
}

[Serializable]
public class EnemyEncounter
{
    public List<EnemySpawn> enemies;
    public ThreatLevel threatLevel;
    public int dayRangeMin;
    public int dayRangeMax;
}

public class EnemyEncounterGenerator
{
    private System.Random seededRng;
    private int seed;

    // 敌人组合池
    private List<EnemySpawn> meleePool = new List<EnemySpawn>
    {
        new EnemySpawn { enemyId = "wolf", type = EnemyType.Melee, weight = 30, minThreat = ThreatLevel.Low },
        new EnemySpawn { enemyId = "goblin_warrior", type = EnemyType.Melee, weight = 25, minThreat = ThreatLevel.Low },
        new EnemySpawn { enemyId = "shadow_beast", type = EnemyType.Melee, weight = 15, minThreat = ThreatLevel.Medium },
        new EnemySpawn { enemyId = "corrupted_knight", type = EnemyType.Melee, weight = 10, minThreat = ThreatLevel.High },
    };

    private List<EnemySpawn> rangedPool = new List<EnemySpawn>
    {
        new EnemySpawn { enemyId = "goblin_archer", type = EnemyType.Ranged, weight = 30, minThreat = ThreatLevel.Low },
        new EnemySpawn { enemyId = "forest_spirit", type = EnemyType.Ranged, weight = 25, minThreat = ThreatLevel.Medium },
        new EnemySpawn { enemyId = "dark_mage", type = EnemyType.Ranged, weight = 15, minThreat = ThreatLevel.High },
    };

    private List<EnemySpawn> healerPool = new List<EnemySpawn>
    {
        new EnemySpawn { enemyId = "forest_healer", type = EnemyType.Healer, weight = 20, minThreat = ThreatLevel.Medium },
        new EnemySpawn { enemyId = "light_spirit", type = EnemyType.Healer, weight = 15, minThreat = ThreatLevel.High },
    };

    private List<EnemySpawn> tankPool = new List<EnemySpawn>
    {
        new EnemySpawn { enemyId = "stone_golem", type = EnemyType.Tank, weight = 25, minThreat = ThreatLevel.Medium },
        new EnemySpawn { enemyId = "iron_turtle", type = EnemyType.Tank, weight = 20, minThreat = ThreatLevel.Low },
        new EnemySpawn { enemyId = "ancient_guardian", type = EnemyType.Tank, weight = 10, minThreat = ThreatLevel.Deadly },
    };

    public EnemyEncounterGenerator(int seed)
    {
        this.seed = seed;
        seededRng = new System.Random(seed);
    }

    public static EnemyEncounterGenerator CreateWithSeed(int seed) => new EnemyEncounterGenerator(seed);
    public static EnemyEncounterGenerator CreateRandom() => new EnemyEncounterGenerator(Guid.NewGuid().GetHashCode());

    /// <summary>
    /// 根据威胁等级生成敌人组合
    /// </summary>
    public List<EnemySpawn> GenerateEncounter(ThreatLevel threat, int day)
    {
        seededRng = new System.Random(seed + day * 1000);
        var result = new List<EnemySpawn>();

        int baseCount = GetBaseCount(threat);
        int healerChance = GetHealerChance(threat);
        int tankChance = GetTankChance(threat);

        for (int i = 0; i < baseCount; i++)
        {
            // 决定敌人类型
            int roleRoll = seededRng.Next(100);
            EnemyType role;
            if (roleRoll < 50) role = EnemyType.Melee;
            else if (roleRoll < 75) role = EnemyType.Ranged;
            else if (roleRoll < healerChance && threat >= ThreatLevel.Medium) role = EnemyType.Healer;
            else if (roleRoll < tankChance) role = EnemyType.Tank;
            else role = EnemyType.Melee;

            var pool = GetPoolForType(role);
            var enemy = SelectByWeight(pool, threat);
            if (enemy != null) result.Add(enemy);
        }

        return result;
    }

    private int GetBaseCount(ThreatLevel threat)
    {
        return threat switch
        {
            ThreatLevel.Safe => 1,
            ThreatLevel.Low => 2,
            ThreatLevel.Medium => 3,
            ThreatLevel.High => 4,
            ThreatLevel.Deadly => 5,
            _ => 2
        };
    }

    private int GetHealerChance(ThreatLevel threat)
    {
        return threat switch
        {
            ThreatLevel.Medium => 80,
            ThreatLevel.High => 75,
            ThreatLevel.Deadly => 70,
            _ => 100
        };
    }

    private int GetTankChance(ThreatLevel threat)
    {
        return threat switch
        {
            ThreatLevel.Low => 90,
            ThreatLevel.Medium => 85,
            ThreatLevel.High => 80,
            ThreatLevel.Deadly => 75,
            _ => 100
        };
    }

    private List<EnemySpawn> GetPoolForType(EnemyType type)
    {
        return type switch
        {
            EnemyType.Melee => meleePool,
            EnemyType.Ranged => rangedPool,
            EnemyType.Healer => healerPool,
            EnemyType.Tank => tankPool,
            _ => meleePool
        };
    }

    private EnemySpawn SelectByWeight(List<EnemySpawn> pool, ThreatLevel minThreat)
    {
        var valid = pool.Where(e => e.minThreat <= minThreat).ToList();
        if (valid.Count == 0) return null;

        int totalWeight = valid.Sum(e => e.weight);
        int roll = seededRng.Next(totalWeight);

        int cumulative = 0;
        foreach (var e in valid)
        {
            cumulative += e.weight;
            if (roll < cumulative) return e;
        }
        return valid.Last();
    }

    /// <summary>
    /// 计算指定天的威胁等级
    /// </summary>
    public ThreatLevel CalculateThreatLevel(int day)
    {
        if (day <= 3) return ThreatLevel.Safe;
        if (day <= 7) return ThreatLevel.Low;
        if (day <= 12) return ThreatLevel.Medium;
        if (day <= 17) return ThreatLevel.High;
        return ThreatLevel.Deadly;
    }
}

// ==================== 程序化资源分布 ====================

[Serializable]
public class ResourceNode
{
    public Vector2Int position;
    public ResourceType type;
    public int amount;
    public bool isCollected;
}

public class ResourceDistributor
{
    private System.Random seededRng;
    private int seed;
    private List<ResourceNode> resources = new List<ResourceNode>();

    // 资源类型权重（保证分布均衡）
    private Dictionary<ResourceType, int> baseWeights = new Dictionary<ResourceType, int>
    {
        { ResourceType.Food, 30 },
        { ResourceType.Wood, 25 },
        { ResourceType.Stone, 20 },
        { ResourceType.Herb, 15 },
        { ResourceType.Treasure, 10 }
    };

    public ResourceDistributor(int seed)
    {
        this.seed = seed;
        seededRng = new System.Random(seed);
    }

    public static ResourceDistributor CreateWithSeed(int seed) => new ResourceDistributor(seed);
    public static ResourceDistributor CreateRandom() => new ResourceDistributor(Guid.NewGuid().GetHashCode());

    /// <summary>
    /// 在指定区域内生成资源点
    /// </summary>
    public List<ResourceNode> DistributeResources(List<RectInt> areas, int resourceCount)
    {
        seededRng = new System.Random(seed);
        resources.Clear();

        // 计算每个资源类型的数量（保证均衡分布）
        var distribution = CalculateBalancedDistribution(resourceCount);

        foreach (var kvp in distribution)
        {
            for (int i = 0; i < kvp.Value; i++)
            {
                var area = areas[seededRng.Next(areas.Count)];
                var pos = new Vector2Int(
                    seededRng.Next(area.x, area.xMax),
                    seededRng.Next(area.y, area.yMax)
                );

                resources.Add(new ResourceNode
                {
                    position = pos,
                    type = kvp.Key,
                    amount = GetAmountForType(kvp.Key),
                    isCollected = false
                });
            }
        }

        return resources;
    }

    private Dictionary<ResourceType, int> CalculateBalancedDistribution(int total)
    {
        var result = new Dictionary<ResourceType, int>();
        int remaining = total;

        // 先保证每种类型至少有一些
        int minPerType = Mathf.Max(1, total / 6);
        foreach (var type in baseWeights.Keys)
        {
            result[type] = minPerType;
            remaining -= minPerType;
        }

        // 剩余按权重分配
        int totalWeight = baseWeights.Values.Sum();
        foreach (var type in baseWeights.Keys)
        {
            int additional = (int)((float)baseWeights[type] / totalWeight * remaining);
            result[type] += additional;
        }

        // 调整确保总数正确
        int diff = result.Values.Sum() - total;
        if (diff != 0)
        {
            var keys = result.Keys.ToList();
            result[keys[seededRng.Next(keys.Count)]] += -diff;
        }

        return result;
    }

    private int GetAmountForType(ResourceType type)
    {
        return type switch
        {
            ResourceType.Food => seededRng.Next(3, 8),
            ResourceType.Wood => seededRng.Next(2, 6),
            ResourceType.Stone => seededRng.Next(2, 5),
            ResourceType.Herb => seededRng.Next(1, 4),
            ResourceType.Treasure => seededRng.Next(1, 3),
            _ => 1
        };
    }

    public List<ResourceNode> GetResources() => resources;
}

// ==================== 程序化事件序列 ====================

[Serializable]
public class GameEvent
{
    public string eventId;
    public string titleKey;       // 国际化key
    public string descKey;        // 国际化key
    public int weight;
    public bool used;
    public string[] tags;         // 事件标签
    public EventEffect[] effects;
}

[Serializable]
public class EventEffect
{
    public string type;           // "buff", "debuff", "item", "relationship", "narrative"
    public string target;         // 影响目标
    public int value;
    public string param;
}

public class EventSequenceManager
{
    private System.Random seededRng;
    private int seed;
    private int currentDay;
    private List<GameEvent> eventLibrary;
    private List<GameEvent> dailyEvents;
    private HashSet<string> usedEventIds = new HashSet<string>();

    public EventSequenceManager(int seed)
    {
        this.seed = seed;
        seededRng = new System.Random(seed);
        currentDay = 1;
        InitializeEventLibrary();
    }

    public static EventSequenceManager CreateWithSeed(int seed) => new EventSequenceManager(seed);
    public static EventSequenceManager CreateRandom() => new EventSequenceManager(Guid.NewGuid().GetHashCode());

    private void InitializeEventLibrary()
    {
        eventLibrary = new List<GameEvent>
        {
            // 生存类事件
            new GameEvent { eventId = "food_poisoning", titleKey = "event_food_poisoning_title", descKey = "event_food_poisoning_desc", weight = 10, tags = new[] { "danger", "food" }, effects = new EventEffect[] { new EventEffect { type = "debuff", target = "health", value = -20, param = "poison" } } },
            new GameEvent { eventId = "abundant_hunt", titleKey = "event_abundant_hunt_title", descKey = "event_abundant_hunt_desc", weight = 15, tags = new[] { "positive", "food" }, effects = new EventEffect[] { new EventEffect { type = "item", target = "food", value = 30, param = "" } } },
            new GameEvent { eventId = "wild_herbs", titleKey = "event_wild_herbs_title", descKey = "event_wild_herbs_desc", weight = 12, tags = new[] { "positive", "herb" }, effects = new EventEffect[] { new EventEffect { type = "item", target = "herb", value = 15, param = "" } } },
            
            // NPC互动事件
            new GameEvent { eventId = "stranger_encounter", titleKey = "event_stranger_title", descKey = "event_stranger_desc", weight = 8, tags = new[] { "npc", "choice" }, effects = new EventEffect[] { new EventEffect { type = "relationship", target = "stranger", value = 10, param = "" } } },
            new GameEvent { eventId = "merchant_arrives", titleKey = "event_merchant_title", descKey = "event_merchant_desc", weight = 10, tags = new[] { "npc", "trade" }, effects = new EventEffect[] { new EventEffect { type = "item", target = "treasure", value = 5, param = "" } } },
            
            // 探索类事件
            new GameEvent { eventId = "hidden_cache", titleKey = "event_hidden_cache_title", descKey = "event_hidden_cache_desc", weight = 7, tags = new[] { "explore", "treasure" }, effects = new EventEffect[] { new EventEffect { type = "item", target = "treasure", value = 20, param = "" } } },
            new GameEvent { eventId = "collapse_trap", titleKey = "event_collapse_title", descKey = "event_collapse_desc", weight = 8, tags = new[] { "danger", "explore" }, effects = new EventEffect[] { new EventEffect { type = "debuff", target = "team", value = -15, param = "injury" } } },
            
            // 叙事类事件
            new GameEvent { eventId = "memory_fragment", titleKey = "event_memory_title", descKey = "event_memory_desc", weight = 12, tags = new[] { "narrative", "lore" }, effects = new EventEffect[] { new EventEffect { type = "narrative", target = "memory", value = 1, param = "" } } },
            new GameEvent { eventId = "black_mist_warning", titleKey = "event_mist_warning_title", descKey = "event_mist_warning_desc", weight = 6, tags = new[] { "narrative", "danger" }, effects = new EventEffect[] { new EventEffect { type = "buff", target = "awareness", value = 10, param = "" } } },
            
            // 战斗类事件
            new GameEvent { eventId = "ambush", titleKey = "event_ambush_title", descKey = "event_ambush_desc", weight = 10, tags = new[] { "combat", "danger" }, effects = new EventEffect[] { new EventEffect { type = "debuff", target = "team", value = -10, param = "surprise" } } },
            new GameEvent { eventId = "easy_victory", titleKey = "event_easy_victory_title", descKey = "event_easy_victory_desc", weight = 8, tags = new[] { "combat", "positive" }, effects = new EventEffect[] { new EventEffect { type = "item", target = "treasure", value = 15, param = "" } } },
            
            // 特殊事件
            new GameEvent { eventId = "blessing_of_forest", titleKey = "event_blessing_title", descKey = "event_blessing_desc", weight = 5, tags = new[] { "special", "positive" }, effects = new EventEffect[] { new EventEffect { type = "buff", target = "all", value = 15, param = "forest_blessing" } } },
            new GameEvent { eventId = "dark_omen", titleKey = "event_dark_omen_title", descKey = "event_dark_omen_desc", weight = 5, tags = new[] { "special", "danger" }, effects = new EventEffect[] { new EventEffect { type = "debuff", target = "morale", value = -20, param = "omen" } } },
        };
    }

    /// <summary>
    /// 获取当天的事件（每日事件不重复）
    /// </summary>
    public GameEvent GetDailyEvent(int day)
    {
        seededRng = new System.Random(seed + day * 777);
        currentDay = day;

        // 排除已使用的事件
        var available = eventLibrary.Where(e => !usedEventIds.Contains(e.eventId)).ToList();
        if (available.Count == 0)
        {
            // 重置所有事件（进入新循环）
            usedEventIds.Clear();
            available = eventLibrary.ToList();
        }

        // 按权重随机选取
        int totalWeight = available.Sum(e => e.weight);
        int roll = seededRng.Next(totalWeight);

        int cumulative = 0;
        foreach (var e in available)
        {
            cumulative += e.weight;
            if (roll < cumulative)
            {
                usedEventIds.Add(e.eventId);
                return e;
            }
        }

        return available.Last();
    }

    /// <summary>
    /// 按标签筛选事件
    /// </summary>
    public GameEvent GetEventByTag(string tag, int day)
    {
        var available = eventLibrary.Where(e => !usedEventIds.Contains(e.eventId) && e.tags.Contains(tag)).ToList();
        if (available.Count == 0) return null;

        int totalWeight = available.Sum(e => e.weight);
        int roll = seededRng.Next(totalWeight);

        int cumulative = 0;
        foreach (var e in available)
        {
            cumulative += e.weight;
            if (roll < cumulative)
            {
                usedEventIds.Add(e.eventId);
                return e;
            }
        }
        return null;
    }

    /// <summary>
    /// 获取所有未使用事件
    /// </summary>
    public List<GameEvent> GetRemainingEvents()
    {
        return eventLibrary.Where(e => !usedEventIds.Contains(e.eventId)).ToList();
    }

    /// <summary>
    /// 重置事件序列（新月）
    /// </summary>
    public void ResetEventSequence()
    {
        usedEventIds.Clear();
    }
}

#endregion

#region Part B: Steam发布素材（数据定义）

// ==================== Steam商店素材清单 ====================

public static class SteamStoreAssets
{
    // ==================== 1. 商店图标描述 ====================
    public static string StoreIconDescription => @"
【商店图标 512x512 描述】

像素风格森林场景与手账封面融合设计：

主体设计：
- 中央：一本打开的像素风手账，封面有精致的树叶花纹边框
- 手账左页：显示森林地图路线，标注21天生存进度
- 手账右页：战斗卡牌图案（剑、盾、法术图标）
- 手账上方：像素风格的黑色雾气笼罩效果
- 手账下方：一行发光的翠绿色文字『森林手账 - Forest Journal』

色彩方案：
- 主色调：深森林绿 (#1a472a) + 暗夜黑 (#0d1117)
- 强调色：神秘紫 (#6b5b95) + 魔法蓝 (#4a90d9)
- 高光：金色 (#ffd700) 用于重要元素

背景元素：
- 手账周围散落：树叶、指南针、羽毛笔等生存道具
- 左下角：小型像素太阳，被黑雾遮挡一半
- 右上角：闪烁的记忆碎片光点

整体风格：复古像素（16-bit）+ 手账温馨感
";

    // ==================== 2. 游戏截图描述 ====================
    public static string[] ScreenshotDescriptions => new string[]
    {
        @"【截图1：标题画面】
描述：
- 全屏像素森林背景，黑雾从画面边缘涌入
- 中央：游戏标题『森林手账』用优雅的像素字体
- 副标题：英文『Forest Journal - Survive 21 Days』
- 下方：三个按钮 - 『新的旅程』『继续冒险』『游戏设置』
- 背景细节：黑雾中有若隐若现的怪物剪影

氛围：神秘、史诗、略带紧张感",

        @"【截图2：战斗界面】
描述：
- 8AP能量条显示在屏幕顶部（当前AP：6/8）
- 手牌区（5张卡牌）：攻击牌、防御牌、技能牌
- 敌人区：3个像素敌人（近战狼×2 + 远程法师×1）
- 我方角色区：3个角色头像显示HP
- 场景：黑暗森林空地，树木环绕
- UI风格：底部手账式信息栏，显示剩余天数

氛围：紧张刺激的回合制战斗",

        @"【截图3：探索界面】
描述：
- 像素地图视图，显示已探索区域
- 当前区域高亮：绿色（安全）、黄色（危险）、红色（高危）
- 隐藏区域用问号图标标记
- 右侧：队伍状态面板
- 下方：资源收集快捷栏（食物、木材、药草数量）
- 地图上可见：NPC标记点、商店图标、事件点

氛围：充满发现的探索乐趣",

        @"【截图4：NPC对话】
描述：
- 像素风格角色立绘（占画面40%）
- 对话框：复古羊皮纸样式
- 角色名：NPC名字 + 当前心情图标
- 对话内容3-4行，带有角色特色语气
- 选项区：2-3个对话选择（显示好感度变化提示）
- 背景：角色所在场景（小屋/森林/废墟）

氛围：温馨的社交互动",

        @"【截图5：制作系统】
描述：
- 中央：制作菜单面板（木质边框风格）
- 左侧：材料背包（图标+数量）
- 右侧：可制作物品列表（武器/防具/消耗品）
- 当前选中物品详情：高亮显示、属性加成说明
- 制作按钮：显示所需材料和成功率
- 下方：套装收集进度条

氛围：满足感强的资源管理",

        @"【截图6：结局画面】
描述：
- 宽屏像素艺术画：森林之心被光芒照耀
- 黑雾消散，露出美丽的真实森林
- 结局类型标志：10种结局之一（根据玩家选择）
- 统计面板：存活天数、完成支线、收集率
- 评价等级：S/A/B/C/D + 综合评分
- 按钮：『回顾旅程』『再次挑战』『分享成就』

氛围：成就感与情感共鸣并存"
    };

    // ==================== 3. 宣传视频脚本（60秒） ====================
    public static string TrailerScript => @"
【森林手账 - 宣传视频脚本 60秒】

[00:00-00:05] 开场 - 黑雾降临
画面：全黑，缓缓飘入黑色雾气
旁白（低沉神秘）：『这片森林，曾经宁静祥和...』
音效：风声、远处狼嚎

[00:05-00:10] 黑雾笼罩
画面：黑雾吞噬像素风格的森林村庄
旁白：『直到有一天，黑雾笼罩了一切...』
音乐：转为紧张、悬疑

[00:10-00:18] 醒来
画面：主角从昏迷中醒来，身边有队友
旁白：『你和队友在这片被诅咒的土地上醒来』
游戏画面：标题画面淡入
音乐：史诗感渐强

[00:18-00:28] 游戏特色展示（快切）
画面切换：
- 战斗界面：使用卡牌攻击敌人（2秒）
- 探索地图：穿越森林寻找线索（2秒）
- NPC互动：与神秘角色对话（2秒）
- 资源收集：采集药草、狩猎（2秒）
- 农场种植：建设自己的基地（2秒）
旁白：简述各项特色

[00:28-00:38] 挑战与危机
画面：
- 遭遇强大敌人（2秒）
- 艰难抉择时刻（2秒）
- 队友倒下危机（2秒）
- 收集记忆碎片（2秒）
旁白：『21天的生存挑战，黑雾的真相等待揭晓』
音乐：紧张感达到高潮

[00:38-00:50] 森林之心
画面：
- 逐渐接近森林中心（2秒）
- 黑雾开始消散（2秒）
- 光芒照耀森林（2秒）
- 主角站在森林之心前（3秒）
旁白：『传说中的森林之心，是唯一的希望』
音乐：转向希望、光明

[00:50-00:58] 你的选择决定命运
画面：
- 显示『10种不同结局』
- 不同选择导致不同画面片段
- 主角团队站在阳光下（2秒）
旁白：『你的选择，将决定这片森林的命运』

[00:58-01:00] 结束
画面：游戏标题 + Steam发布日期
旁白：『森林手账 - 现已在Steam愿望单』
音乐：余韵悠长的钢琴曲

---

【配乐建议】
风格：史诗 + 神秘 + 希望
参考：Ori and the Blind Forest + Hades的战斗史诗感
主旋律：神秘森林 → 紧张战斗 → 希望曙光
";

    // ==================== 4. 游戏描述文案 ====================
    public static string ShortDescription => "在黑雾笼罩的森林中，带领队伍生存21天。卡牌战斗、资源管理、NPC互动、多结局。";

    public static string FullDescription => @"
标题：森林手账 - Forest Journal

一款融合了卡牌战斗、资源管理、角色扮演和 roguelike 元素的像素风生存游戏。

【游戏特色】

• 8AP回合制战斗 - 每回合使用卡牌与敌人战斗
• 18种敌人 - 各有独特AI和行为模式
• 5位可招募NPC - 与他们建立关系，解锁专属剧情
• 15条支线故事线 - 揭示黑雾的真相
• 10种结局 - 你的选择决定森林的命运
• 48个随机事件 - 每次游玩体验不同
• 20个隐藏区域 - 探索未知的秘密
• 5套装备套装 - 收集最强装备
• 农场种植系统 - 自给自足
• 多周目继承 - 保留进度，挑战更高难度

【故事背景】

黑雾笼罩了这片曾经宁静的森林。
你和队友醒来时已经迷失在这片被诅咒的土地上。
传说中的""森林之心""是唯一的希望——
但要到达那里，你们必须生存21天，
面对黑雾中的怪物，收集记忆碎片，
揭开这片森林被诅咒的真相。

【配置要求】

操作系统：Windows 10+
处理器：Intel Core i3+
内存：4 GB RAM
显卡：DirectX 9.0c compatible
存储空间：500 MB可用空间";

    // ==================== 5. Steam标签建议 ====================
    public static string[] SteamTags => new string[]
    {
        "Indie",
        "RPG",
        "Card Game",
        "Roguelike",
        "Pixel Graphics",
        "Survival",
        "Story Rich",
        "Multiple Endings",
        "Replayability",
        "Turn-Based Combat",
        "2D",
        "Adventure"
    };

    // ==================== 6. 发行计划 ====================
    public static string ReleasePlan => @"
【发行计划】

2026年Q2（4月-6月）：Alpha测试
- 核心战斗系统测试
- 基础地图生成测试
- 限量玩家内测
- 收集早期反馈

2026年Q3（7月-9月）：Beta测试
- 完整内容开放
- 多人联机功能（如有）
- 公开测试阶段
- 社区运营启动

2026年Q4（10月-12月）：正式发布
- Steam正式上线
- 首发优惠活动
- 配套DLC规划
- 主机版评估";
}

#endregion

#region Part C: 国际化系统

// ==================== 国际化系统 ====================

public static class LocalizationKeys
{
    // 事件标题和描述的key（供EventSequenceManager使用）
    public static class Events
    {
        public const string FoodPoisoningTitle = "event_food_poisoning_title";
        public const string FoodPoisoningDesc = "event_food_poisoning_desc";
        public const string AbundantHuntTitle = "event_abundant_hunt_title";
        public const string AbundantHuntDesc = "event_abundant_hunt_desc";
        public const string WildHerbsTitle = "event_wild_herbs_title";
        public const string WildHerbsDesc = "event_wild_herbs_desc";
        public const string StrangerEncounterTitle = "event_stranger_title";
        public const string StrangerEncounterDesc = "event_stranger_desc";
        public const string MerchantArrivesTitle = "event_merchant_title";
        public const string MerchantArrivesDesc = "event_merchant_desc";
        public const string HiddenCacheTitle = "event_hidden_cache_title";
        public const string HiddenCacheDesc = "event_hidden_cache_desc";
        public const string CollapseTrapTitle = "event_collapse_title";
        public const string CollapseTrapDesc = "event_collapse_desc";
        public const string MemoryFragmentTitle = "event_memory_title";
        public const string MemoryFragmentDesc = "event_memory_desc";
        public const string BlackMistWarningTitle = "event_mist_warning_title";
        public const string BlackMistWarningDesc = "event_mist_warning_desc";
        public const string AmbushTitle = "event_ambush_title";
        public const string AmbushDesc = "event_ambush_desc";
        public const string EasyVictoryTitle = "event_easy_victory_title";
        public const string EasyVictoryDesc = "event_easy_victory_desc";
        public const string BlessingOfForestTitle = "event_blessing_title";
        public const string BlessingOfForestDesc = "event_blessing_desc";
        public const string DarkOmenTitle = "event_dark_omen_title";
        public const string DarkOmenDesc = "event_dark_omen_desc";
    }

    // 敌人名称
    public static class Enemies
    {
        public const string Wolf = "enemy_wolf";
        public const string GoblinWarrior = "enemy_goblin_warrior";
        public const string GoblinArcher = "enemy_goblin_archer";
        public const string ShadowBeast = "enemy_shadow_beast";
        public const string CorruptedKnight = "enemy_corrupted_knight";
        public const string ForestSpirit = "enemy_forest_spirit";
        public const string DarkMage = "enemy_dark_mage";
        public const string ForestHealer = "enemy_forest_healer";
        public const string LightSpirit = "enemy_light_spirit";
        public const string StoneGolem = "enemy_stone_golem";
        public const string IronTurtle = "enemy_iron_turtle";
        public const string AncientGuardian = "enemy_ancient_guardian";
    }

    // UI文本
    public static class UI
    {
        public const string NewJourney = "ui_new_journey";
        public const string ContinueAdventure = "ui_continue";
        public const string Settings = "ui_settings";
        public const string DayLabel = "ui_day";
        public const string ApLabel = "ui_ap";
        public const string HealthLabel = "ui_health";
        public const string FoodLabel = "ui_food";
        public const string WoodLabel = "ui_wood";
        public const string StoneLabel = "ui_stone";
        public const string HerbLabel = "ui_herb";
    }

    // 结局
    public static class Endings
    {
        public const string TrueEnding = "ending_true_ending";
        public const string DarkEnding = "ending_dark_ending";
        public const string SacrificeEnding = "ending_sacrifice";
        public const string EscapeEnding = "ending_escape";
        public const string MysteryEnding = "ending_mystery";
    }
}

public class LocalizationManager
{
    private static LocalizationManager instance;
    public static LocalizationManager Instance => instance ??= new LocalizationManager();

    private string currentLanguage = "zh-CN";
    private Dictionary<string, Dictionary<string, string>> translations;

    LocalizationManager()
    {
        InitializeTranslations();
    }

    private void InitializeTranslations()
    {
        translations = new Dictionary<string, Dictionary<string, string>>
        {
            ["zh-CN"] = new Dictionary<string, string>
            {
                // 事件标题
                ["event_food_poisoning_title"] = "食物中毒",
                ["event_food_poisoning_desc"] = "你们吃下了有毒的浆果，部分队员感到不适。",
                ["event_abundant_hunt_title"] = "丰收狩猎",
                ["event_abundant_hunt_desc"] = "今天的狩猎格外顺利，收获了大量食物。",
                ["event_wild_herbs_title"] = "野生药草",
                ["event_wild_herbs_desc"] = "在路边发现了珍贵的药草，可以用来制作回复药。",
                ["event_stranger_title"] = "陌生人",
                ["event_stranger_desc"] = "一个神秘的陌生人出现在你们面前。",
                ["event_merchant_title"] = "商人到来",
                ["event_merchant_desc"] = "流浪商人带来了稀有的货物。",
                ["event_hidden_cache_title"] = "隐藏的宝藏",
                ["event_hidden_cache_desc"] = "你们发现了一个隐藏的储藏点。",
                ["event_collapse_title"] = "坍塌陷阱",
                ["event_collapse_desc"] = "地面突然坍塌，队伍受到了伤害。",
                ["event_memory_title"] = "记忆碎片",
                ["event_memory_desc"] = "一块发光的碎片，蕴含着过去的记忆。",
                ["event_mist_warning_title"] = "黑雾预警",
                ["event_mist_warning_desc"] = "黑雾正在逼近，你们需要提高警惕。",
                ["event_ambush_title"] = "伏击",
                ["event_ambush_desc"] = "敌人早已埋伏，你们遭到了突然袭击！",
                ["event_easy_victory_title"] = "轻松胜利",
                ["event_easy_victory_desc"] = "这场战斗比预想的要轻松。",
                ["event_blessing_title"] = "森林祝福",
                ["event_blessing_desc"] = "森林似乎在眷顾着你们。",
                ["event_dark_omen_title"] = "黑暗预兆",
                ["event_dark_omen_desc"] = "不祥的预感笼罩着队伍。",

                // 敌人名称
                ["enemy_wolf"] = "森林狼",
                ["enemy_goblin_warrior"] = "哥布林战士",
                ["enemy_goblin_archer"] = "哥布林弓箭手",
                ["enemy_shadow_beast"] = "暗影兽",
                ["enemy_corrupted_knight"] = "堕落骑士",
                ["enemy_forest_spirit"] = "森林精灵",
                ["enemy_dark_mage"] = "黑暗法师",
                ["enemy_forest_healer"] = "森林治疗师",
                ["enemy_light_spirit"] = "光明精灵",
                ["enemy_stone_golem"] = "石魔像",
                ["enemy_iron_turtle"] = "铁甲龟",
                ["enemy_ancient_guardian"] = "古老守护者",

                // UI
                ["ui_new_journey"] = "新的旅程",
                ["ui_continue"] = "继续冒险",
                ["ui_settings"] = "游戏设置",
                ["ui_day"] = "第{0}天",
                ["ui_ap"] = "行动点",
                ["ui_health"] = "生命值",
                ["ui_food"] = "食物",
                ["ui_wood"] = "木材",
                ["ui_stone"] = "石材",
                ["ui_herb"] = "药草",

                // 结局
                ["ending_true_ending"] = "真正的结局",
                ["ending_dark_ending"] = "黑暗结局",
                ["ending_sacrifice"] = "牺牲结局",
                ["ending_escape"] = "逃离结局",
                ["ending_mystery"] = "神秘结局",
            },
            ["en-US"] = new Dictionary<string, string>
            {
                // Event Titles
                ["event_food_poisoning_title"] = "Food Poisoning",
                ["event_food_poisoning_desc"] = "You ate poisonous berries. Some team members feel unwell.",
                ["event_abundant_hunt_title"] = "Bountiful Hunt",
                ["event_abundant_hunt_desc"] = "Today's hunt was especially fruitful, gaining plenty of food.",
                ["event_wild_herbs_title"] = "Wild Herbs",
                ["event_wild_herbs_desc"] = "You discovered precious herbs on the roadside.",
                ["event_stranger_title"] = "A Stranger",
                ["event_stranger_desc"] = "A mysterious stranger appears before you.",
                ["event_merchant_title"] = "Merchant Arrives",
                ["event_merchant_desc"] = "A traveling merchant brings rare goods.",
                ["event_hidden_cache_title"] = "Hidden Cache",
                ["event_hidden_cache_desc"] = "You discovered a hidden storage spot.",
                ["event_collapse_title"] = "Collapse Trap",
                ["event_collapse_desc"] = "The ground suddenly collapsed, damaging the team.",
                ["event_memory_title"] = "Memory Fragment",
                ["event_memory_desc"] = "A glowing fragment containing memories of the past.",
                ["event_mist_warning_title"] = "Black Mist Warning",
                ["event_mist_warning_desc"] = "The black mist is approaching. Stay vigilant.",
                ["event_ambush_title"] = "Ambush",
                ["event_ambush_desc"] = "Enemies were lying in wait. You've been ambushed!",
                ["event_easy_victory_title"] = "Easy Victory",
                ["event_easy_victory_desc"] = "This battle was easier than expected.",
                ["event_blessing_title"] = "Forest's Blessing",
                ["event_blessing_desc"] = "The forest seems to favor you.",
                ["event_dark_omen_title"] = "Dark Omen",
                ["event_dark_omen_desc"] = "An ominous feeling笼罩着队伍。",

                // Enemy Names
                ["enemy_wolf"] = "Forest Wolf",
                ["enemy_goblin_warrior"] = "Goblin Warrior",
                ["enemy_goblin_archer"] = "Goblin Archer",
                ["enemy_shadow_beast"] = "Shadow Beast",
                ["enemy_corrupted_knight"] = "Corrupted Knight",
                ["enemy_forest_spirit"] = "Forest Spirit",
                ["enemy_dark_mage"] = "Dark Mage",
                ["enemy_forest_healer"] = "Forest Healer",
                ["enemy_light_spirit"] = "Light Spirit",
                ["enemy_stone_golem"] = "Stone Golem",
                ["enemy_iron_turtle"] = "Iron Turtle",
                ["enemy_ancient_guardian"] = "Ancient Guardian",

                // UI
                ["ui_new_journey"] = "New Journey",
                ["ui_continue"] = "Continue",
                ["ui_settings"] = "Settings",
                ["ui_day"] = "Day {0}",
                ["ui_ap"] = "Action Points",
                ["ui_health"] = "Health",
                ["ui_food"] = "Food",
                ["ui_wood"] = "Wood",
                ["ui_stone"] = "Stone",
                ["ui_herb"] = "Herbs",

                // Endings
                ["ending_true_ending"] = "True Ending",
                ["ending_dark_ending"] = "Dark Ending",
                ["ending_sacrifice"] = "Sacrifice Ending",
                ["ending_escape"] = "Escape Ending",
                ["ending_mystery"] = "Mystery Ending",
            }
        };
    }

    /// <summary>
    /// 获取本地化文本
    /// </summary>
    public string Get(string key, params object[] args)
    {
        if (translations.TryGetValue(currentLanguage, out var langDict))
        {
            if (langDict.TryGetValue(key, out var value))
            {
                return args.Length > 0 ? string.Format(value, args) : value;
            }
        }
        return key;
    }

    /// <summary>
    /// 设置语言
    /// </summary>
    public void SetLanguage(string languageCode)
    {
        if (translations.ContainsKey(languageCode))
        {
            currentLanguage = languageCode;
        }
    }

    /// <summary>
    /// 获取当前语言
    /// </summary>
    public string GetCurrentLanguage() => currentLanguage;

    /// <summary>
    /// 获取所有支持的语言
    /// </summary>
    public IEnumerable<string> GetSupportedLanguages() => translations.Keys;
}

#endregion

#region Part D: 主控制器

/// <summary>
/// ProceduralContentManager - 程序化内容系统主控制器
/// 整合所有程序化生成系统，统一管理
/// </summary>
public class ProceduralContentManager : MonoBehaviour
{
    public static ProceduralContentManager instance { get; private set; }

    [Header("生成设置")]
    [SerializeField] private int mapWidth = 50;
    [SerializeField] private int mapHeight = 50;
    [SerializeField] private int resourceCount = 30;

    [Header("当前状态")]
    [SerializeField] private int currentSeed;
    [SerializeField] private int currentDay;

    private MapGenerator mapGenerator;
    private EnemyEncounterGenerator enemyGenerator;
    private ResourceDistributor resourceDistributor;
    private EventSequenceManager eventManager;

    private BSPNode currentMapRoot;
    private List<RectInt> currentRooms;
    private List<ResourceNode> currentResources;

    // 事件
    public event Action<int, BSPNode> OnMapGenerated;
    public event Action<List<EnemySpawn>> OnEncounterGenerated;
    public event Action<GameEvent> OnDailyEventTriggered;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        InitializeWithSeed(currentSeed);
    }

    /// <summary>
    /// 使用指定种子初始化（预设种子可重玩）
    /// </summary>
    public void InitializeWithSeed(int seed)
    {
        currentSeed = seed;
        currentDay = 1;

        mapGenerator = MapGenerator.CreateWithSeed(seed);
        enemyGenerator = EnemyEncounterGenerator.CreateWithSeed(seed);
        resourceDistributor = ResourceDistributor.CreateWithSeed(seed);
        eventManager = EventSequenceManager.CreateWithSeed(seed);

        GenerateNewGame();
    }

    /// <summary>
    /// 使用随机种子开始新游戏
    /// </summary>
    public void InitializeNewGame()
    {
        int randomSeed = Guid.NewGuid().GetHashCode();
        InitializeWithSeed(randomSeed);
    }

    /// <summary>
    /// 生成新游戏地图和资源
    /// </summary>
    public void GenerateNewGame()
    {
        // 生成BSP地图
        currentMapRoot = mapGenerator.GenerateMap(mapWidth, mapHeight);
        currentRooms = mapGenerator.GetAllRooms(currentMapRoot);

        // 分配资源
        currentResources = resourceDistributor.DistributeResources(currentRooms, resourceCount);

        OnMapGenerated?.Invoke(currentSeed, currentMapRoot);
    }

    /// <summary>
    /// 获取当前房间列表
    /// </summary>
    public List<RectInt> GetCurrentRooms() => currentRooms;

    /// <summary>
    /// 获取当前资源点列表
    /// </summary>
    public List<ResourceNode> GetCurrentResources() => currentResources;

    /// <summary>
    /// 推进到下一天
    /// </summary>
    public void AdvanceDay()
    {
        currentDay++;
        OnDailyEventTriggered?.Invoke(eventManager.GetDailyEvent(currentDay));
    }

    /// <summary>
    /// 获取当天威胁等级
    /// </summary>
    public ThreatLevel GetCurrentThreatLevel()
    {
        return enemyGenerator.CalculateThreatLevel(currentDay);
    }

    /// <summary>
    /// 生成当天敌人遭遇
    /// </summary>
    public List<EnemySpawn> GenerateTodayEncounter()
    {
        var threat = enemyGenerator.CalculateThreatLevel(currentDay);
        var encounter = enemyGenerator.GenerateEncounter(threat, currentDay);
        OnEncounterGenerated?.Invoke(encounter);
        return encounter;
    }

    /// <summary>
    /// 获取当天事件
    /// </summary>
    public GameEvent GetTodayEvent()
    {
        return eventManager.GetDailyEvent(currentDay);
    }

    /// <summary>
    /// 获取指定标签的事件
    /// </summary>
    public GameEvent GetEventByTag(string tag)
    {
        return eventManager.GetEventByTag(tag, currentDay);
    }

    /// <summary>
    /// 获取剩余未触发事件数
    /// </summary>
    public int GetRemainingEventCount()
    {
        return eventManager.GetRemainingEvents().Count;
    }

    /// <summary>
    /// 重置事件序列
    /// </summary>
    public void ResetEvents()
    {
        eventManager.ResetEventSequence();
    }

    /// <summary>
    /// 获取当前种子（用于存档/重玩）
    /// </summary>
    public int GetCurrentSeed() => currentSeed;

    /// <summary>
    /// 获取当前天数
    /// </summary>
    public int GetCurrentDay() => currentDay;
}

#endregion

/// <summary>
/// 存档数据类
/// </summary>
[Serializable]
public class ProceduralSaveData
{
    public int seed;
    public int currentDay;
    public List<string> usedEventIds;
    public List<string> collectedResourceIds;
}