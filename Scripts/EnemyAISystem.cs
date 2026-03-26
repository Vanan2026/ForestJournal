using UnityEngine;
using System;

/// <summary>
/// 差异化敌人AI系统 v1.0
/// 每种敌人有独特的行为模式和战斗策略
/// </summary>
public class EnemyAISystem : MonoBehaviour
{
    public static EnemyAISystem instance { get; private set; }

    [Header("敌人类型配置")]
    public EnemyType[] enemyTypes;

    void Awake()
    {
        instance = this;
        InitializeEnemyTypes();
    }

    void InitializeEnemyTypes()
    {
        enemyTypes = new EnemyType[]
        {
            new EnemyType
            {
                id = "shadow_wolf",
                name = "阴影狼",
                description = "群体行动的捕食者，喜欢包围猎物",
                baseHP = 20,
                baseATK = 5,
                attackVariance = 2,
                behavior = EnemyBehavior.Aggressive,
                specialAbility = "pack_tactics",
                attackCount = 1,
                specialDesc = "群狼战术：每有一个友方狼，攻击力+2",
                lootBonus = new int[] { 2, 3 },  // food
                dropChance = 0.4f
            },
            new EnemyType
            {
                id = "poison_spider",
                name = "毒蜘蛛",
                description = "织网待猎，攻击附带中毒效果",
                baseHP = 25,
                baseATK = 8,
                attackVariance = 3,
                behavior = EnemyBehavior.Poisoner,
                specialAbility = "poison",
                attackCount = 1,
                specialDesc = "毒液攻击：30%概率使目标中毒，每回合-3HP持续3回合",
                lootBonus = new int[] { 1, 2 },  // herb
                dropChance = 0.35f
            },
            new EnemyType
            {
                id = "mist_spirit",
                name = "雾精灵",
                description = "虚无缥缈，难以命中",
                baseHP = 30,
                baseATK = 10,
                attackVariance = 4,
                behavior = EnemyBehavior.Evasive,
                specialAbility = "phase_shift",
                attackCount = 2,
                specialDesc = "相位转移：闪避率+30%，有概率发动两次攻击",
                lootBonus = new int[] { 0, 1 },  // soul essence
                dropChance = 0.25f
            },
            new EnemyType
            {
                id = "darkbeast",
                name = "黑雾兽",
                description = "黑雾的化身，攻防兼备",
                baseHP = 50,
                baseATK = 15,
                attackVariance = 5,
                behavior = EnemyBehavior.Tank,
                specialAbility = "life_drain",
                attackCount = 1,
                specialDesc = "生命汲取：造成伤害的30%转化为自己的HP",
                lootBonus = new int[] { 0, 2 },  // soul essence
                dropChance = 0.45f
            },
            new EnemyType
            {
                id = "mutant_tree",
                name = "变异树妖",
                description = "扎根不动，但攻击范围广伤害高",
                baseHP = 80,
                baseATK = 20,
                attackVariance = 6,
                behavior = EnemyBehavior.Boss,
                specialAbility = "entangling_roots",
                attackCount = 1,
                specialDesc = "缠根：首次攻击后，目标AP-2直到下个自己的回合",
                lootBonus = new int[] { 2, 5 },  // wood
                dropChance = 0.5f
            },
            new EnemyType
            {
                id = "swamp_frog",
                name = "沼泽蛙",
                description = "沼泽中的潜伏者，会召唤小怪",
                baseHP = 35,
                baseATK = 12,
                attackVariance = 4,
                behavior = EnemyBehavior.Summoner,
                specialAbility = "spawn_minions",
                attackCount = 1,
                specialDesc = "召唤：30%概率在战斗开始时召唤2只小蛙",
                lootBonus = new int[] { 1, 3 },  // food
                dropChance = 0.3f
            },
            new EnemyType
            {
                id = "stone_lizard",
                name = "岩石蜥",
                description = "厚甲高防，行动迟缓",
                baseHP = 100,
                baseATK = 8,
                attackVariance = 3,
                behavior = EnemyBehavior.Defender,
                specialAbility = "armor_plate",
                attackCount = 1,
                specialDesc = "石甲：受到的实际伤害-30%（四舍五入）",
                lootBonus = new int[] { 1, 3 },  // stone
                dropChance = 0.35f
            },
            new EnemyType
            {
                id = "fog_lord",
                name = "雾主",
                description = "黑雾的领主，区域BOSS",
                baseHP = 200,
                baseATK = 25,
                attackVariance = 8,
                behavior = EnemyBehavior.Boss,
                specialAbility = "fog_blessing",
                attackCount = 2,
                specialDesc = "黑雾祝福：每次攻击随机附加一种效果（伤害+50%/减速/致盲）",
                lootBonus = new int[] { 3, 5 },  // soul essence
                dropChance = 0.8f
            },
            new EnemyType
            {
                id = "forest_guardian",
                name = "森林守护者",
                description = "森林之心的守护者，最终BOSS",
                baseHP = 300,
                baseATK = 30,
                attackVariance = 10,
                behavior = EnemyBehavior.Boss,
                specialAbility = "nature_wrath",
                attackCount = 3,
                specialDesc = "自然之怒：3连击，每击伤害递减（100%/75%/50%），并有几率治疗自身",
                lootBonus = new int[] { 5, 10 }, // soul essence + memories
                dropChance = 1.0f
            }
        };
    }

    // ====================
    // 敌人实例化（根据威胁等级生成）
    // ====================

    /// <summary>
    /// 根据威胁等级创建敌人
    /// </summary>
    public EnemyInstance CreateEnemy(int threatLevel)
    {
        if (threatLevel < 1) threatLevel = 1;
        if (threatLevel > 5) threatLevel = 5;

        // 按威胁等级分配敌人类型
        EnemyType[] pool = GetEnemyPoolForThreat(threatLevel);
        var type = pool[UnityEngine.Random.Range(0, pool.Length)];

        return CreateEnemyInstance(type, threatLevel);
    }

    EnemyType[] GetEnemyPoolForThreat(int threat)
    {
        switch (threat)
        {
            case 1: return new[] { FindType("shadow_wolf"), FindType("poison_spider") };
            case 2: return new[] { FindType("shadow_wolf"), FindType("poison_spider"), FindType("mist_spirit"), FindType("swamp_frog") };
            case 3: return new[] { FindType("mist_spirit"), FindType("darkbeast"), FindType("swamp_frog"), FindType("stone_lizard") };
            case 4: return new[] { FindType("darkbeast"), FindType("mutant_tree"), FindType("stone_lizard"), FindType("fog_lord") };
            case 5: return new[] { FindType("fog_lord"), FindType("mutant_tree"), FindType("darkbeast") };
            default: return new[] { FindType("shadow_wolf") };
        }
    }

    EnemyType FindType(string id)
    {
        foreach (var t in enemyTypes)
            if (t.id == id) return t;
        return enemyTypes[0];
    }

    EnemyInstance CreateEnemyInstance(EnemyType type, int threatLevel)
    {
        // 威胁等级缩放
        float scale = 1f + (threatLevel - 1) * 0.25f;

        int hp = Mathf.RoundToInt(type.baseHP * scale);
        int atk = Mathf.RoundToInt(type.baseATK * scale);

        return new EnemyInstance
        {
            typeId = type.id,
            name = type.name,
            hp = hp,
            maxHP = hp,
            atk = atk,
            attackVariance = type.attackVariance,
            behavior = type.behavior,
            specialAbility = type.specialAbility,
            attackCount = type.attackCount,
            specialDesc = type.specialDesc,
            activeEffects = new System.Collections.Generic.List<StatusEffect>()
        };
    }

    // ====================
    // 战斗AI执行
    // ====================

    /// <summary>
    /// 计算敌人回合的伤害
    /// </summary>
    public EnemyAttackResult ExecuteEnemyAttack(EnemyInstance enemy, int playerDefense)
    {
        var result = new EnemyAttackResult();
        result.attacks = new System.Collections.Generic.List<AttackResult>();

        for (int i = 0; i < enemy.attackCount; i++)
        {
            var attack = new AttackResult();

            // 基础伤害
            int baseDamage = enemy.atk + UnityEngine.Random.Range(-enemy.attackVariance, enemy.attackVariance + 1);
            int finalDamage = Mathf.Max(1, baseDamage - playerDefense);

            // 石甲减免
            if (enemy.typeId == "stone_lizard")
            {
                finalDamage = Mathf.RoundToInt(finalDamage * 0.7f);
            }

            // 多段攻击递减
            if (i == 1) finalDamage = Mathf.RoundToInt(finalDamage * 0.75f);
            if (i >= 2) finalDamage = Mathf.RoundToInt(finalDamage * 0.5f);

            attack.damage = finalDamage;
            result.attacks.Add(attack);
            result.totalDamage += finalDamage;

            // 雾主特殊：随机附加效果
            if (enemy.typeId == "fog_lord")
            {
                float roll = UnityEngine.Random.value;
                if (roll < 0.33f)
                {
                    attack.addedEffect = "damage_boost";
                    result.attacks[i] = attack;
                    result.totalDamage = Mathf.RoundToInt(result.totalDamage * 1.5f);
                }
                else if (roll < 0.66f)
                {
                    attack.addedEffect = "slow";
                }
            }

            // 黑雾兽生命汲取
            if (enemy.typeId == "darkbeast" && enemy.hp < enemy.maxHP)
            {
                int drain = Mathf.RoundToInt(finalDamage * 0.3f);
                enemy.hp = Mathf.Min(enemy.hp + drain, enemy.maxHP);
                attack.lifeDrain = drain;
            }
        }

        // 附加状态效果
        result.statusEffect = GetStatusEffectFromEnemy(enemy);

        // 战吼/特殊效果文字
        result.attackDescription = GetAttackDescription(enemy);

        return result;
    }

    string GetAttackDescription(EnemyInstance enemy)
    {
        switch (enemy.specialAbility)
        {
            case "pack_tactics": return $"{enemy.name}带领狼群发起了包围攻击！";
            case "poison": return $"{enemy.name}吐出了毒液！";
            case "phase_shift": return $"{enemy.name}闪烁不定，连续发起攻击！";
            case "life_drain": return $"{enemy.name}的黑雾触须缠绕着你，汲取生命！";
            case "entangling_roots": return $"{enemy.name}的根须从地下涌出！";
            case "spawn_minions": return $"{enemy.name}召唤出了小蛙！";
            case "armor_plate": return $"{enemy.name}用坚硬的石甲挡下了部分伤害！";
            case "fog_blessing": return $"{enemy.name}被黑雾环绕，发出震耳咆哮！";
            case "nature_wrath": return $"{enemy.name}举起巨臂，重重砸下！";
            default: return $"{enemy.name}发起攻击！";
        }
    }

    StatusEffect GetStatusEffectFromEnemy(EnemyInstance enemy)
    {
        switch (enemy.specialAbility)
        {
            case "poison":
                if (UnityEngine.Random.value < 0.3f)
                    return new StatusEffect { type = EffectType.Poison, damagePerTurn = 3, duration = 3 };
                break;
            case "entangling_roots":
                return new StatusEffect { type = EffectType.APReduction, amount = 2, duration = 1 };
        }
        return null;
    }

    // ====================
    // 掉落计算
    // ====================

    /// <summary>
    /// 战斗胜利后计算掉落
    /// </summary>
    public LootResult CalculateLoot(EnemyInstance enemy)
    {
        var result = new LootResult();

        if (UnityEngine.Random.value < enemy.dropChance)
        {
            result.looted = true;
            result.food = UnityEngine.Random.Range(1, 4);
            result.wood = enemy.lootBonus.Length > 3 ? UnityEngine.Random.Range(enemy.lootBonus[2], enemy.lootBonus[3] + 1) : 0;
            result.stone = enemy.typeId == "stone_lizard" ? UnityEngine.Random.Range(1, 3) : 0;
            result.herb = enemy.typeId == "poison_spider" ? UnityEngine.Random.Range(1, 3) : 0;
            result.soulEssence = enemy.lootBonus.Length > 1 ? UnityEngine.Random.Range(enemy.lootBonus[0], enemy.lootBonus[1] + 1) : 0;

            result.dropDescription = $"{enemy.name}掉落了 {result.food} 食物";
            if (result.soulEssence > 0) result.dropDescription += $"，{result.soulEssence} 魂精华";
        }

        return result;
    }

    // ====================
    // 敌人选择（战斗开始时）
    // ====================

    /// <summary>
    /// 获取区域BOSS（森林守护者）
    /// </summary>
    public EnemyInstance CreateForestGuardian()
    {
        var guardian = CreateEnemyInstance(FindType("forest_guardian"), 5);
        guardian.name = "森林守护者";
        guardian.isBoss = true;
        return guardian;
    }

    /// <summary>
    /// 获取区域精英怪（雾主）
    /// </summary>
    public EnemyInstance CreateFogLord()
    {
        var lord = CreateEnemyInstance(FindType("fog_lord"), 4);
        lord.name = "雾主";
        lord.isElite = true;
        return lord;
    }

    // ====================
    // 数据类
    // ====================

    public enum EnemyBehavior
    {
        Normal,
        Aggressive,    // 阴影狼：群体战术
        Poisoner,      // 毒蜘蛛：持续伤害
        Evasive,       // 雾精灵：多段攻击
        Tank,          // 黑雾兽：生命偷取
        Boss,          // 树妖/BOSS：高伤害
        Summoner,      // 沼泽蛙：召唤
        Defender       // 岩石蜥：防御减伤
    }

    public enum EffectType
    {
        None,
        Poison,        // 中毒
        APReduction,   // AP减少
        Slow,          // 减速
        Stun,          // 眩晕
        Heal           // 治疗
    }

    [System.Serializable]
    public class EnemyType
    {
        public string id;
        public string name;
        public string description;
        public int baseHP;
        public int baseATK;
        public int attackVariance;
        public EnemyBehavior behavior;
        public string specialAbility;
        public int attackCount;
        public string specialDesc;
        public int[] lootBonus;  // [min, max]
        public float dropChance;
    }

    [System.Serializable]
    public class EnemyInstance
    {
        public string typeId;
        public string name;
        public int hp;
        public int maxHP;
        public int atk;
        public int attackVariance;
        public EnemyBehavior behavior;
        public string specialAbility;
        public int attackCount;
        public string specialDesc;
        public bool isBoss = false;
        public bool isElite = false;
        public float dropChance = 0.4f;
        public int[] lootBonus = { 1, 3 };
        public System.Collections.Generic.List<StatusEffect> activeEffects;
    }

    [System.Serializable]
    public class StatusEffect
    {
        public EffectType type;
        public int damagePerTurn;  // 中毒每回合伤害
        public int amount;          // 效果数值
        public int duration;        // 持续回合
    }

    public class EnemyAttackResult
    {
        public System.Collections.Generic.List<AttackResult> attacks;
        public int totalDamage;
        public StatusEffect statusEffect;
        public string attackDescription;
    }

    public class AttackResult
    {
        public int damage;
        public string addedEffect; // "damage_boost" / "slow" / etc.
        public int lifeDrain;
    }

    public class LootResult
    {
        public bool looted;
        public int food;
        public int wood;
        public int stone;
        public int herb;
        public int soulEssence;
        public string dropDescription;
    }
}
