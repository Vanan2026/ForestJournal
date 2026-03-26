using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// 差异化敌人AI系统 v1.1
/// 每种敌人有独特的行为模式和战斗策略
/// 18种敌人类型：森林/黑雾主题全覆盖
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
            // ============ 原有9种敌人 ============
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
            },
            // ============ 新增9种敌人 ============
            new EnemyType
            {
                id = "rotting_deer",
                name = "腐化鹿",
                description = "被黑雾侵蚀的森林之鹿，腐蚀之角可腐蚀护甲",
                baseHP = 40,
                baseATK = 14,
                attackVariance = 4,
                behavior = EnemyBehavior.Corrupter,
                specialAbility = "corrosive_horn",
                attackCount = 1,
                specialDesc = "腐蚀之角：攻击降低目标防御2点，持续2回合",
                lootBonus = new int[] { 1, 2, 2, 4 },  // food, bone
                dropChance = 0.35f
            },
            new EnemyType
            {
                id = "blackwing_mosquito",
                name = "黑翼蚊",
                description = "黑雾滋生的吸血蚊群，成群结队发动攻击",
                baseHP = 15,
                baseATK = 6,
                attackVariance = 2,
                behavior = EnemyBehavior.Swarm,
                specialAbility = "swarm_attack",
                attackCount = 3,
                specialDesc = "群体叮咬：连续3次低伤害攻击，总伤害可突破防御",
                lootBonus = new int[] { 0, 1 },  // blood sac
                dropChance = 0.2f
            },
            new EnemyType
            {
                id = "swamp_leech",
                name = "沼泽蛭",
                description = "潜伏在沼泽中的吸血怪物，攻击附带减速",
                baseHP = 30,
                baseATK = 10,
                attackVariance = 3,
                behavior = EnemyBehavior.Leech,
                specialAbility = "blood_suck",
                attackCount = 1,
                specialDesc = "吸血减速：造成伤害的40%转化为HP，并使目标减速1回合",
                lootBonus = new int[] { 1, 2 },  // herb
                dropChance = 0.3f
            },
            new EnemyType
            {
                id = "ruins_guardian",
                name = "废墟守卫",
                description = "古老废墟中的石头傀儡进阶版，能自我修复",
                baseHP = 120,
                baseATK = 12,
                attackVariance = 4,
                behavior = EnemyBehavior.Regen,
                specialAbility = "self_repair",
                attackCount = 1,
                specialDesc = "自我修复：每回合回复5HP，被击败时自爆造成范围伤害",
                lootBonus = new int[] { 2, 4 },  // stone, ancient shard
                dropChance = 0.4f
            },
            new EnemyType
            {
                id = "ghost_wolf",
                name = "幽灵狼",
                description = "怨念化成的狼灵，能够分裂出影子分身",
                baseHP = 25,
                baseATK = 12,
                attackVariance = 3,
                behavior = EnemyBehavior.Phantom,
                specialAbility = "shadow_clone",
                attackCount = 2,
                specialDesc = "影子分身：30%概率制造一个分身，分身可抵挡一次攻击",
                lootBonus = new int[] { 1, 3 },  // soul essence
                dropChance = 0.35f
            },
            new EnemyType
            {
                id = "blackmist_tentacle",
                name = "黑雾触手",
                description = "黑雾凝聚而成的触手，从地下发起攻击",
                baseHP = 45,
                baseATK = 18,
                attackVariance = 5,
                behavior = EnemyBehavior.AoE,
                specialAbility = "ground_strike",
                attackCount = 1,
                specialDesc = "地面打击：对目标及其相邻单位造成50%溅射伤害",
                lootBonus = new int[] { 1, 2 },  // tentacle
                dropChance = 0.3f
            },
            new EnemyType
            {
                id = "toxic_mushroom",
                name = "剧毒蘑菇",
                description = "黑雾滋生的变异蘑菇，释放有毒孢子",
                baseHP = 35,
                baseATK = 8,
                attackVariance = 3,
                behavior = EnemyBehavior.AoEPoison,
                specialAbility = "spore_cloud",
                attackCount = 1,
                specialDesc = "孢子云：对区域内所有敌人造成中毒，每回合-4HP持续2回合",
                lootBonus = new int[] { 2, 3 },  // mushroom
                dropChance = 0.25f
            },
            new EnemyType
            {
                id = "shadow_hunter",
                name = "暗影猎手",
                description = "隐匿于黑雾中的远程杀手，攻击后短暂消失",
                baseHP = 28,
                baseATK = 16,
                attackVariance = 4,
                behavior = EnemyBehavior.Stealth,
                specialAbility = "vanish",
                attackCount = 1,
                specialDesc = "消失：攻击后进入隐身，下回合攻击必定暴击+无视防御",
                lootBonus = new int[] { 1, 2 },  // shadow dust
                dropChance = 0.35f
            },
            new EnemyType
            {
                id = "nightmare_weaver",
                name = "噩梦编织者",
                description = "能侵入猎物梦境的恐惧化身，令目标陷入恐慌",
                baseHP = 55,
                baseATK = 13,
                attackVariance = 5,
                behavior = EnemyBehavior.Fearmonger,
                specialAbility = "nightmare",
                attackCount = 1,
                specialDesc = "噩梦之触：35%概率使目标恐惧，接下来2回合攻击力-50%",
                lootBonus = new int[] { 2, 4 },  // soul essence, nightmare shard
                dropChance = 0.4f
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
            case 1: return new[] { FindType("shadow_wolf"), FindType("poison_spider"), FindType("blackwing_mosquito") };
            case 2: return new[] { FindType("shadow_wolf"), FindType("poison_spider"), FindType("mist_spirit"), FindType("swamp_frog"), FindType("swamp_leech"), FindType("ghost_wolf") };
            case 3: return new[] { FindType("mist_spirit"), FindType("darkbeast"), FindType("swamp_frog"), FindType("stone_lizard"), FindType("rotting_deer"), FindType("toxic_mushroom") };
            case 4: return new[] { FindType("darkbeast"), FindType("mutant_tree"), FindType("stone_lizard"), FindType("fog_lord"), FindType("ruins_guardian"), FindType("blackmist_tentacle"), FindType("shadow_hunter"), FindType("nightmare_weaver") };
            case 5: return new[] { FindType("fog_lord"), FindType("mutant_tree"), FindType("darkbeast"), FindType("nightmare_weaver"), FindType("ruins_guardian") };
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

            // 沼泽蛭吸血减速
            if (enemy.typeId == "swamp_leech" && enemy.hp < enemy.maxHP)
            {
                int drain = Mathf.RoundToInt(finalDamage * 0.4f);
                enemy.hp = Mathf.Min(enemy.hp + drain, enemy.maxHP);
                attack.lifeDrain = drain;
                attack.addedEffect = "slow";
            }

            // 幽灵狼影子分身
            if (enemy.typeId == "ghost_wolf")
            {
                if (UnityEngine.Random.value < 0.3f)
                {
                    enemy.hasShield = true;
                    attack.addedEffect = "shield_clone";
                }
            }

            // 黑雾触手溅射伤害（AoE）
            if (enemy.typeId == "blackmist_tentacle")
            {
                int splashDamage = Mathf.RoundToInt(finalDamage * 0.5f);
                attack.splashDamage = splashDamage;
            }

            // 暗影猎手隐身暴击
            if (enemy.typeId == "shadow_hunter")
            {
                if (enemy.isInvisible)
                {
                    attack.damage = Mathf.RoundToInt(attack.damage * 2f);
                    attack.addedEffect = "crit";
                    enemy.isInvisible = false;
                }
            }

            // 噩梦编织者恐惧效果
            if (enemy.typeId == "nightmare_weaver")
            {
                if (UnityEngine.Random.value < 0.35f)
                {
                    attack.addedEffect = "fear";
                }
            }

            // 废墟守卫自爆（当HP低于30%时）
            if (enemy.typeId == "ruins_guardian" && enemy.hp < enemy.maxHP * 0.3f)
            {
                attack.addedEffect = "self_destruct";
                int selfDmg = Mathf.RoundToInt(enemy.hp * 0.5f);
                enemy.hp = 0;
                attack.selfDamage = selfDmg;
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
            case "corrosive_horn": return $"{enemy.name}低下腐蚀之角，直冲而来！";
            case "swarm_attack": return $"{enemy.name}群起而攻，针刺如雨！";
            case "blood_suck": return $"{enemy.name}吸血的颚紧紧咬住你！";
            case "self_repair": return $"{enemy.name}的石缝中渗出黑雾，伤口在愈合！";
            case "shadow_clone": return $"{enemy.name}的身影分裂，幽灵分身出现！";
            case "ground_strike": return $"{enemy.name}的触手从地下猛然抽出！";
            case "spore_cloud": return $"{enemy.name}释放出剧毒孢子云！";
            case "vanish": return $"{enemy.name}化作黑影，消失于迷雾中！";
            case "nightmare": return $"{enemy.name}的触须伸向你的额头，注入噩梦！";
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
            case "corrosive_horn":
                return new StatusEffect { type = EffectType.DefenseReduction, amount = 2, duration = 2 };
            case "blood_suck":
                return new StatusEffect { type = EffectType.Slow, amount = 1, duration = 1 };
            case "spore_cloud":
                if (UnityEngine.Random.value < 0.4f)
                    return new StatusEffect { type = EffectType.Poison, damagePerTurn = 4, duration = 2 };
                break;
            case "nightmare":
                if (UnityEngine.Random.value < 0.35f)
                    return new StatusEffect { type = EffectType.Fear, amount = 50, duration = 2 };
                break;
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
            result.stone = enemy.typeId == "stone_lizard" || enemy.typeId == "ruins_guardian" ? UnityEngine.Random.Range(1, 3) : 0;
            result.herb = enemy.typeId == "poison_spider" || enemy.typeId == "swamp_leech" ? UnityEngine.Random.Range(1, 3) : 0;
            result.soulEssence = enemy.lootBonus.Length > 1 ? UnityEngine.Random.Range(enemy.lootBonus[0], enemy.lootBonus[1] + 1) : 0;

            result.dropDescription = $"{enemy.name}掉落了 {result.food} 食物";
            if (result.soulEssence > 0) result.dropDescription += $"，{result.soulEssence} 魂精华";
            if (result.stone > 0 && enemy.typeId == "ruins_guardian") result.dropDescription += $"，{result.stone} 古代碎片";
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
        Defender,      // 岩石蜥：防御减伤
        Corrupter,     // 腐化鹿：腐蚀防御
        Swarm,         // 黑翼蚊：多段低伤
        Leech,         // 沼泽蛭：吸血减速
        Regen,         // 废墟守卫：自我修复
        Phantom,       // 幽灵狼：影子分身
        AoE,           // 黑雾触手：范围溅射
        AoEPoison,     // 剧毒蘑菇：范围毒
        Stealth,       // 暗影猎手：隐身暴击
        Fearmonger     // 噩梦编织者：恐惧debuff
    }

    public enum EffectType
    {
        None,
        Poison,        // 中毒
        APReduction,   // AP减少
        Slow,          // 减速
        Stun,          // 眩晕
        Heal,          // 治疗
        DefenseReduction, // 护甲腐蚀
        Fear           // 恐惧
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
        public int[] lootBonus;  // [min, max] 或 [min, max, min2, max2]
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
        public bool isInvisible = false;
        public bool hasShield = false;
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
        public string addedEffect; // "damage_boost" / "slow" / "fear" / "crit" / etc.
        public int lifeDrain;
        public int splashDamage;
        public int selfDamage;
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
