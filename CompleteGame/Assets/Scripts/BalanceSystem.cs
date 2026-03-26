using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// 数值平衡系统 v1.0
/// 基于数据分析的数值调优
/// 目标：前期紧张有趣、中期成长感强、后期挑战与回报平衡
/// </summary>
public class BalanceSystem : MonoBehaviour
{
    public static BalanceSystem instance { get; private set; }

    [Header("数值表")]
    public BalanceTable balance;

    void Awake()
    {
        instance = this;
        LoadBalance();
    }

    void LoadBalance()
    {
        balance = new BalanceTable();
    }

    // ====================
    // 获取平衡后的数值
    // ====================

    /// <summary>
    /// 获取当前难度的敌人属性倍率
    /// </summary>
    public float GetEnemyStatMultiplier(int threatLevel)
    {
        return balance.enemyStatMultiplierCurve.Evaluate(threatLevel);
    }

    /// <summary>
    /// 获取当前阶段的采集量
    /// </summary>
    public int GetGatherAmount(int baseAmount, int currentDay)
    {
        float dayFactor = balance.gatherCurve.Evaluate(currentDay);
        return Mathf.RoundToInt(baseAmount * dayFactor);
    }

    /// <summary>
    /// 获取升级所需经验
    /// </summary>
    public int GetExpForLevel(int level)
    {
        return Mathf.RoundToInt(balance.expToLevelCurve.Evaluate(level));
    }

    /// <summary>
    /// 获取每日消耗
    /// </summary>
    public int GetDailyFoodConsumption(int squadSize, bool hasCampfire)
    {
        int base_ = balance.baseFoodConsumptionPerPerson;
        int total = base_ * squadSize;
        if (hasCampfire) total = Mathf.RoundToInt(total * balance.campfireFoodReduction);
        return total;
    }

    /// <summary>
    /// 获取威胁增长速率
    /// </summary>
    public int GetDaysPerThreatIncrease(int currentThreat)
    {
        // 威胁越高，涨得越快
        return Mathf.Max(1, balance.daysPerThreatLevel - currentThreat / 2);
    }

    /// <summary>
    /// 获取记忆碎片获取量（任务/战斗/探索）
    /// </summary>
    public int GetMemoryReward(MemoryRewardType type, int day)
    {
        float dayBonus = balance.memoryDayScalingCurve.Evaluate(day);

        switch (type)
        {
            case MemoryRewardType.Combat:
                return Mathf.RoundToInt(UnityEngine.Random.Range(
                    balance.memoryRewardRanges.combatMin,
                    balance.memoryRewardRanges.combatMax) * dayBonus);
            case MemoryRewardType.Quest:
                return Mathf.RoundToInt(UnityEngine.Random.Range(
                    balance.memoryRewardRanges.questMin,
                    balance.memoryRewardRanges.questMax) * dayBonus);
            case MemoryRewardType.Exploration:
                return Mathf.RoundToInt(UnityEngine.Random.Range(
                    balance.memoryRewardRanges.explorationMin,
                    balance.memoryRewardRanges.explorationMax) * dayBonus);
            case MemoryRewardType.NPCInteraction:
                return Mathf.RoundToInt(UnityEngine.Random.Range(
                    balance.memoryRewardRanges.npcMin,
                    balance.memoryRewardRanges.npcMax) * dayBonus);
            default:
                return 1;
        }
    }

    /// <summary>
    /// 获取敌人掉落食物的范围
    /// </summary>
    public (int min, int max) GetEnemyFoodDropRange(int enemyThreat)
    {
        return (
            balance.enemyFoodDropRange.x + enemyThreat * 2,
            balance.enemyFoodDropRange.y + enemyThreat * 3
        );
    }

    /// <summary>
    /// 获取制作配方的资源消耗
    /// </summary>
    public float GetCraftCostMultiplier(int day)
    {
        return balance.craftCostScalingCurve.Evaluate(day);
    }

    /// <summary>
    /// 获取敌人遭遇概率
    /// </summary>
    public float GetEnemyEncounterChance(int threatLevel)
    {
        return Mathf.Min(balance.baseEnemyEncounterChance + threatLevel * 0.05f, 0.9f);
    }

    /// <summary>
    /// 获取区域探索危险系数
    /// </summary>
    public float GetRegionDangerMultiplier(int regionId)
    {
        if (regionId < 0 || regionId >= balance.regionDangerMultipliers.Length)
            return 1f;
        return balance.regionDangerMultipliers[regionId];
    }

    // ====================
    // 游戏内应用（自动调整）
    // ====================

    /// <summary>
    /// 每天自动调整数值（前期放宽，后期收紧）
    /// </summary>
    public void ApplyDailyBalanceAdjustments()
    {
        var gm = GameManager.instance;
        if (gm == null) return;

        int day = gm.currentDay;

        // 第1-5天：新手保护，资源丰富
        if (day <= 5)
        {
            // 采集量+20%
        }
        // 第6-15天：正常节奏
        else if (day <= 15)
        {
            // 标准数值
        }
        // 第16-25天：压力上升
        else if (day <= 25)
        {
            // 敌人属性+10%
            // 遭遇概率+5%
        }
        // 25天后：最终挑战期
        else
        {
            // 敌人属性+20%
            // 每日消耗+10%
        }

        Debug.Log($"[Balance] Day {day} - 应用数值调整");
    }

    // ====================
    // 数值表
    // ====================

    [System.Serializable]
    public class BalanceTable
    {
        // 敌人属性随威胁增长曲线（x=威胁等级, y=倍率）
        public AnimationCurve enemyStatMultiplierCurve = new AnimationCurve(
            new Keyframe(1, 1.0f),
            new Keyframe(2, 1.15f),
            new Keyframe(3, 1.35f),
            new Keyframe(4, 1.6f),
            new Keyframe(5, 2.0f)
        );

        // 采集量随天数曲线（x=天数, y=倍率）
        public AnimationCurve gatherCurve = new AnimationCurve(
            new Keyframe(1, 1.3f),   // 第1天采集多
            new Keyframe(7, 1.0f),   // 第7天正常
            new Keyframe(14, 0.85f), // 第14天减少
            new Keyframe(21, 0.75f), // 第21天最少
            new Keyframe(30, 0.65f)  // 30天后资源枯竭
        );

        // 经验曲线（x=等级, y=经验值）
        public AnimationCurve expToLevelCurve = new AnimationCurve(
            new Keyframe(1, 0),
            new Keyframe(2, 15),
            new Keyframe(3, 40),
            new Keyframe(4, 80),
            new Keyframe(5, 140),
            new Keyframe(6, 220),
            new Keyframe(7, 320),
            new Keyframe(8, 450),
            new Keyframe(9, 620),
            new Keyframe(10, 850)
        );

        // 记忆获取随天数缩放（后期记忆更难获取）
        public AnimationCurve memoryDayScalingCurve = new AnimationCurve(
            new Keyframe(1, 1.4f),
            new Keyframe(7, 1.1f),
            new Keyframe(14, 1.0f),
            new Keyframe(21, 0.9f),
            new Keyframe(30, 0.75f)
        );

        // 制作消耗随天数缩放（后期配方更贵）
        public AnimationCurve craftCostScalingCurve = new AnimationCurve(
            new Keyframe(1, 0.8f),
            new Keyframe(7, 1.0f),
            new Keyframe(14, 1.1f),
            new Keyframe(21, 1.2f),
            new Keyframe(30, 1.4f)
        );

        // 基础数值
        public int baseFoodConsumptionPerPerson = 4;
        public float campfireFoodReduction = 0.25f;
        public int daysPerThreatLevel = 3;
        public float baseEnemyEncounterChance = 0.25f;

        // 记忆获取范围
        public MemoryRewardRanges memoryRewardRanges = new MemoryRewardRanges
        {
            combatMin = 1, combatMax = 3,
            questMin = 2, questMax = 4,
            explorationMin = 1, explorationMax = 2,
            npcMin = 1, npcMax = 3
        };

        // 敌人食物掉落范围
        public Vector2Int enemyFoodDropRange = new Vector2Int(3, 8);

        // 各区域危险系数（0=安全区, 4=森林之心）
        public float[] regionDangerMultipliers = new float[] {
            0.5f,   // 迷雾森林（起始）
            0.8f,   // 迷雾前沿
            1.0f,   // 古老废墟
            1.2f,   // 幽暗山谷
            1.5f    // 森林之心
        };
    }

    public enum MemoryRewardType { Combat, Quest, Exploration, NPCInteraction }

    [System.Serializable]
    public class MemoryRewardRanges
    {
        public int combatMin, combatMax;
        public int questMin, questMax;
        public int explorationMin, explorationMax;
        public int npcMin, npcMax;
    }
}
