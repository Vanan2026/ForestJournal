using System;
using System.Collections.Generic;
using UnityEngine;

namespace ForestJournal
{
    // ============================================================
    // Part A - 天气系统
    // ============================================================

    public enum WeatherType
    {
        Sunny,      // 晴
        Cloudy,     // 阴
        Foggy,      // 雾
        Rainy,      // 雨
        Stormy,     // 暴雨
        BlackMist   // 黑雾风暴
    }

    /// <summary>
    /// 天气效果数据
    /// </summary>
    [System.Serializable]
    public class WeatherEffect
    {
        public float gatherEfficiencyBonus = 0f;      // 采集效率修正
        public float enemyEncounterMod = 0f;          // 敌人遭遇修正
        public float visionMod = 0f;                  // 视野修正
        public float campfireBonus = 0f;              // 篝火效果修正
        public float soulDropBonus = 0f;              // 魂精华掉落修正
        public float playerStatsMod = 0f;             // 玩家属性修正（负值为debuff）
        public float enemyStatsMod = 0f;              // 敌人属性修正
        public bool canGatherHerbs = true;            // 能否采集草药
        public bool canGather = true;                 // 能否采集
        public bool triggersFloodEvent = false;        // 是否触发洪水事件

        public static WeatherEffect FromWeather(WeatherType type)
        {
            var e = new WeatherEffect();
            switch (type)
            {
                case WeatherType.Sunny:
                    e.gatherEfficiencyBonus = 0.2f;
                    e.enemyEncounterMod = -0.2f;
                    break;
                case WeatherType.Cloudy:
                    // 无特殊效果
                    break;
                case WeatherType.Foggy:
                    e.visionMod = -0.3f;
                    e.enemyEncounterMod = 0.3f;
                    e.soulDropBonus = 0.2f;
                    break;
                case WeatherType.Rainy:
                    e.gatherEfficiencyBonus = -0.2f;
                    e.campfireBonus = 0.5f;
                    e.canGatherHerbs = false;
                    break;
                case WeatherType.Stormy:
                    e.canGather = false;
                    e.enemyEncounterMod = 0.5f;
                    e.triggersFloodEvent = true;
                    break;
                case WeatherType.BlackMist:
                    e.playerStatsMod = -0.3f;
                    e.enemyStatsMod = 0.3f;
                    e.visionMod = -0.3f;
                    e.enemyEncounterMod = 0.5f;
                    break;
            }
            return e;
        }
    }

    /// <summary>
    /// 天气数据
    /// </summary>
    [System.Serializable]
    public class WeatherData
    {
        public WeatherType type = WeatherType.Sunny;
        public int currentDuration = 0;   // 剩余持续天数
        public int maxDuration = 3;       // 总持续天数

        public WeatherEffect effects => WeatherEffect.FromWeather(type);

        public string WeatherName => type switch
        {
            WeatherType.Sunny => "晴",
            WeatherType.Cloudy => "阴",
            WeatherType.Foggy => "雾",
            WeatherType.Rainy => "雨",
            WeatherType.Stormy => "暴雨",
            WeatherType.BlackMist => "黑雾风暴",
            _ => "未知"
        };

        public string Description => type switch
        {
            WeatherType.Sunny => "采集效率+20%，敌人遭遇-20%",
            WeatherType.Cloudy => "无特殊效果",
            WeatherType.Foggy => "视野-30%，敌人遭遇+30%，魂精华掉落+20%",
            WeatherType.Rainy => "采集-20%，篝火效果+50%，无法采集草药",
            WeatherType.Stormy => "无法采集，敌人遭遇+50%，可能触发洪水事件",
            WeatherType.BlackMist => "所有属性-30%，敌人全属性+30%，击败黑雾之主的唯一机会！",
            _ => ""
        };
    }

    /// <summary>
    /// 天气系统管理器
    /// </summary>
    public class WeatherSystem : MonoBehaviour
    {
        public static WeatherSystem Instance { get; private set; }

        [Header("天气配置")]
        public WeatherData currentWeather = new WeatherData();
        public int weatherCycleDays = 3;  // 每3天轮换天气
        public float praySuccessRate = 0.4f; // 祈晴成功率

        [Header("黑雾风暴")]
        public int blackMistAvailableDay = 15; // 第15天后才可能出黑雾风暴
        public float blackMistProbability = 0.15f; // 出现概率

        [Header("洪水事件")]
        public float floodProbability = 0.1f; // 暴雨时洪水事件概率

        private System.Random rng = new System.Random();

        public event Action<WeatherType, WeatherType> OnWeatherChanged;
        public event Action<string> OnWeatherEvent; // 洪水、黑雾之主等事件

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            // 初始化为晴天，持续3天
            currentWeather = new WeatherData
            {
                type = WeatherType.Sunny,
                currentDuration = 3,
                maxDuration = 3
            };
        }

        /// <summary>
        /// 推进一天（由游戏主循环调用）
        /// </summary>
        public void AdvanceDay()
        {
            currentWeather.currentDuration--;

            if (currentWeather.currentDuration <= 0)
            {
                ChangeWeather();
            }

            // 暴雨时检查洪水事件
            if (currentWeather.type == WeatherType.Stormy && rng.NextDouble() < floodProbability)
            {
                OnWeatherEvent?.Invoke("洪水事件：河水暴涨，部分区域被淹没！");
            }
        }

        /// <summary>
        /// 切换天气
        /// </summary>
        void ChangeWeather()
        {
            var oldType = currentWeather.type;
            var newType = RollNewWeather();

            currentWeather.type = newType;
            currentWeather.maxDuration = GetDurationForWeather(newType);
            currentWeather.currentDuration = currentWeather.maxDuration;

            OnWeatherChanged?.Invoke(oldType, newType);
            Debug.Log($"[天气] {oldType} → {newType}，持续{currentWeather.maxDuration}天");
        }

        WeatherType RollNewWeather()
        {
            int day = GameClock?.Instance?.day ?? 0;

            if (day >= blackMistAvailableDay && rng.NextDouble() < blackMistProbability)
            {
                return WeatherType.BlackMist;
            }

            // 普通天气随机
            var types = new[] { WeatherType.Sunny, WeatherType.Cloudy, WeatherType.Foggy, WeatherType.Rainy, WeatherType.Stormy };
            // 暴雨概率降低
            var weights = new[] { 0.30f, 0.25f, 0.20f, 0.20f, 0.05f };

            float r = (float)rng.NextDouble();
            float cumulative = 0f;
            for (int i = 0; i < types.Length; i++)
            {
                cumulative += weights[i];
                if (r <= cumulative) return types[i];
            }
            return WeatherType.Sunny;
        }

        int GetDurationForWeather(WeatherType type)
        {
            return type switch
            {
                WeatherType.BlackMist => 2,  // 黑雾风暴持续短但强
                WeatherType.Stormy => 1,
                _ => rng.Next(2, 4)  // 2-3天
            };
        }

        /// <summary>
        /// 祈晴：安营时可消耗1魂精华尝试改变天气
        /// </summary>
        public bool PrayForSunny()
        {
            // 检查是否有魂精华（由物品系统判断）
            if (!HasSoulEssence(1)) return false;

            ConsumeSoulEssence(1);

            if (rng.NextDouble() < praySuccessRate)
            {
                var oldType = currentWeather.type;
                currentWeather.type = WeatherType.Sunny;
                currentWeather.currentDuration = 3;
                currentWeather.maxDuration = 3;
                OnWeatherChanged?.Invoke(oldType, WeatherType.Sunny);
                OnWeatherEvent?.Invoke("祈晴成功！天空放晴！");
                return true;
            }
            else
            {
                OnWeatherEvent?.Invoke("祈晴失败，天气依旧...");
                return false;
            }
        }

        /// <summary>
        /// 获取当前天气效果
        /// </summary>
        public WeatherEffect GetCurrentEffects() => currentWeather.effects;

        /// <summary>
        /// 是否为黑雾风暴（用于触发黑雾之主战斗）
        /// </summary>
        public bool IsBlackMistActive() => currentWeather.type == WeatherType.BlackMist;

        // === 桩函数（对接物品系统） ===
        bool HasSoulEssence(int count) => true; // TODO: 对接物品系统
        void ConsumeSoulEssence(int count) { }  // TODO: 对接物品系统

        // === 桩函数（对接游戏时钟） ===
        GameClock GameClock => FindObjectOfType<GameClock>();
    }

    // ============================================================
    // Part B - 宠物同伴系统
    // ============================================================

    public enum CompanionType
    {
        Firefly,    // 萤火虫
        Ghost,      // 小幽灵
        MushroomCat,// 蘑菇猫
        ForestTurtle, // 森林龟
        DreamButterfly // 梦蝶
    }

    /// <summary>
    /// 宠物技能
    /// </summary>
    [System.Serializable]
    public class CompanionSkill
    {
        public string name;
        public string description;
        public int unlockLevel; // 解锁等级

        public static List<CompanionSkill> GetSkillsFor(CompanionType type)
        {
            return type switch
            {
                CompanionType.Firefly => new List<CompanionSkill>
                {
                    new() { name = "微光照明", description = "照亮周围，夜晚敌人遭遇-10%", unlockLevel = 1 },
                    new() { name = "萤火指引", description = "夜晚移动速度+15%", unlockLevel = 5 },
                    new() { name = "星光庇护", description = "夜晚受到伤害-10%", unlockLevel = 10 },
                },
                CompanionType.Ghost => new List<CompanionSkill>
                {
                    new() { name = "幽灵跟随", description = "显示宠物", unlockLevel = 1 },
                    new() { name = "侦察模式", description = "提前发现隐藏敌人", unlockLevel = 5 },
                    new() { name = "灵体穿透", description = "可穿越障碍物", unlockLevel = 10 },
                },
                CompanionType.MushroomCat => new List<CompanionSkill>
                {
                    new() { name = "毒疗", description = "战斗中治疗中毒的同伴", unlockLevel = 1 },
                    new() { name = "蘑菇护盾", description = "为主人吸收10%伤害", unlockLevel = 5 },
                    new() { name = "孢子云", description = "使周围敌人中毒", unlockLevel = 10 },
                },
                CompanionType.ForestTurtle => new List<CompanionSkill>
                {
                    new() { name = "嘲讽", description = "吸引敌人攻击", unlockLevel = 1 },
                    new() { name = "甲壳防御", description = "受到伤害-20%", unlockLevel = 5 },
                    new() { name = "反甲", description = "反弹15%近战伤害", unlockLevel = 10 },
                },
                CompanionType.DreamButterfly => new List<CompanionSkill>
                {
                    new() { name = "梦之舞", description = "敌人命中率-20%，我方回避+15%", unlockLevel = 1 },
                    new() { name = "幻影分身", description = "有概率闪避一次攻击", unlockLevel = 5 },
                    new() { name = "梦境迷惑", description = "敌人有30%概率攻击同伴", unlockLevel = 10 },
                },
                _ => new List<CompanionSkill>()
            };
        }
    }

    /// <summary>
    /// 宠物同伴数据
    /// </summary>
    [System.Serializable]
    public class CompanionData
    {
        public string id;
        public string name;
        public CompanionType type;
        public int level = 1;
        public int exp = 0;
        public int loyalty = 100;  // 忠诚度 0-100
        public int maxHP;
        public int attack;
        public int defense;
        public string specialAbility;
        public List<CompanionSkill> allSkills;
        public List<CompanionSkill> unlockedSkills;

        // 战斗统计
        public int currentHP;
        public bool isDead => currentHP <= 0;

        public static CompanionData Create(CompanionType type)
        {
            var baseStats = GetBaseStats(type);
            var skills = CompanionSkill.GetSkillsFor(type);

            return new CompanionData
            {
                id = Guid.NewGuid().ToString(),
                name = GetDefaultName(type),
                type = type,
                level = 1,
                exp = 0,
                loyalty = 70,
                maxHP = baseStats.hp,
                attack = baseStats.atk,
                defense = baseStats.def,
                specialAbility = GetSpecialAbility(type),
                allSkills = skills,
                unlockedSkills = new List<CompanionSkill>(),
                currentHP = baseStats.hp
            };
        }

        static (int hp, int atk, int def) GetBaseStats(CompanionType type) => type switch
        {
            CompanionType.Firefly => (10, 2, 1),
            CompanionType.Ghost => (20, 5, 3),
            CompanionType.MushroomCat => (30, 8, 5),
            CompanionType.ForestTurtle => (80, 4, 15),
            CompanionType.DreamButterfly => (25, 10, 8),
            _ => (20, 5, 5)
        };

        static string GetDefaultName(CompanionType type) => type switch
        {
            CompanionType.Firefly => "小萤",
            CompanionType.Ghost => "飘飘",
            CompanionType.MushroomCat => "菇菇",
            CompanionType.ForestTurtle => "壳壳",
            CompanionType.DreamButterfly => "梦梦",
            _ => "宠物"
        };

        static string GetSpecialAbility(CompanionType type) => type switch
        {
            CompanionType.Firefly => "微光照明：夜晚敌人遭遇-10%",
            CompanionType.Ghost => "侦察：提前发现隐藏敌人",
            CompanionType.MushroomCat => "毒疗：治疗中毒同伴",
            CompanionType.ForestTurtle => "嘲讽：吸收伤害保护主人",
            CompanionType.DreamButterfly => "梦之舞：敌人命中-20%，我方回避+15%",
            _ => ""
        };

        /// <summary>
        /// 获取当前等级对应的属性（每级+10%基础属性）
        /// </summary>
        public float LevelMultiplier => 1f + (level - 1) * 0.1f;

        public int DisplayMaxHP => Mathf.RoundToInt(maxHP * LevelMultiplier);
        public int DisplayAttack => Mathf.RoundToInt(attack * LevelMultiplier);
        public int DisplayDefense => Mathf.RoundToInt(defense * LevelMultiplier);

        /// <summary>
        /// 升级所需经验
        /// </summary>
        public int ExpToNextLevel => level * 100;

        /// <summary>
        /// 添加经验，可能升级
        /// </summary>
        public bool AddExp(int amount)
        {
            exp += amount;
            bool leveledUp = false;
            while (exp >= ExpToNextLevel)
            {
                exp -= ExpToNextLevel;
                level++;
                OnLevelUp();
                leveledUp = true;
            }
            return leveledUp;
        }

        void OnLevelUp()
        {
            currentHP = DisplayMaxHP; // 升级回满血
            UnlockSkillsForLevel();
        }

        /// <summary>
        /// 解锁当前等级可用的技能
        /// </summary>
        void UnlockSkillsForLevel()
        {
            foreach (var skill in allSkills)
            {
                if (skill.unlockLevel == level && !unlockedSkills.Contains(skill))
                {
                    unlockedSkills.Add(skill);
                }
            }
        }

        /// <summary>
        /// 降低忠诚度
        /// </summary>
        public void LowerLoyalty(int amount)
        {
            loyalty = Mathf.Max(0, loyalty - amount);
        }

        /// <summary>
        /// 提高忠诚度（使用食物）
        /// </summary>
        public void RaiseLoyalty(int amount)
        {
            loyalty = Mathf.Min(100, loyalty + amount);
        }

        /// <summary>
        /// 检查是否会逃跑
        /// </summary>
        public bool MayRunAway() => loyalty < 50;

        public string Rarity => type switch
        {
            CompanionType.Firefly => "★",
            CompanionType.Ghost => "★★",
            CompanionType.MushroomCat => "★★★",
            CompanionType.ForestTurtle => "★★★★",
            CompanionType.DreamButterfly => "★★★★★",
            _ => "★"
        };

        public string TypeName => type switch
        {
            CompanionType.Firefly => "萤火虫",
            CompanionType.Ghost => "小幽灵",
            CompanionType.MushroomCat => "蘑菇猫",
            CompanionType.ForestTurtle => "森林龟",
            CompanionType.DreamButterfly => "梦蝶",
            _ => "未知"
        };
    }

    /// <summary>
    /// 宠物同伴系统管理器
    /// </summary>
    public class CompanionSystem : MonoBehaviour
    {
        public static CompanionSystem Instance { get; private set; }

        [Header("宠物配置")]
        public List<CompanionData> companions = new();
        public int maxCompanions = 3;  // 最多同时携带3只宠物

        [Header("捕获配置")]
        public float baseCaptureRate = 0.2f; // 基础捕获概率
        public float loyaltyCaptureBonus = 0.1f; // 每10点忠诚度+10%捕获率

        // UI回调
        public event Action<CompanionData> OnCompanionCaptured;
        public event Action<CompanionData> OnCompanionLevelUp;
        public event Action<CompanionData, bool> OnCompanionRunAway; // bool=是否逃跑成功

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        /// <summary>
        /// 尝试捕获一只宠物（战斗后调用）
        /// </summary>
        /// <param name="enemyType">可捕获的宠物类型（由战斗系统提供掉落表）</param>
        /// <param name="hasAdvancedTrap">是否拥有高级陷阱</param>
        /// <returns>是否捕获成功</returns>
        public bool TryCapture(CompanionType enemyType, bool hasAdvancedTrap)
        {
            if (companions.Count >= maxCompanions)
            {
                Debug.Log("[宠物] 已达携带上限，无法捕获更多宠物");
                return false;
            }

            if (!hasAdvancedTrap)
            {
                Debug.Log("[宠物] 需要高级陷阱才能捕获宠物");
                return false;
            }

            // 计算捕获概率
            float rate = baseCaptureRate;
            if (hasAdvancedTrap) rate += 0.3f; // 高级陷阱+30%
            rate = Mathf.Clamp01(rate);

            System.Random rng = new System.Random();
            if (rng.NextDouble() < rate)
            {
                var companion = CompanionData.Create(enemyType);
                companions.Add(companion);
                OnCompanionCaptured?.Invoke(companion);
                Debug.Log($"[宠物] 捕获成功！获得 {companion.name}");
                return true;
            }

            Debug.Log($"[宠物] 捕获失败...");
            return false;
        }

        /// <summary>
        /// 战斗结束后给予经验（所有宠物获得）
        /// </summary>
        public void AwardExpToAll(int expAmount)
        {
            foreach (var companion in companions)
            {
                if (companion.isDead) continue;

                if (companion.AddExp(expAmount))
                {
                    OnCompanionLevelUp?.Invoke(companion);
                }

                // 战斗增加忠诚度
                companion.RaiseLoyalty(2);
            }
        }

        /// <summary>
        /// 检查逃跑（每次天亮时调用）
        /// </summary>
        public void CheckRunAway()
        {
            for (int i = companions.Count - 1; i >= 0; i--)
            {
                var c = companions[i];
                if (c.loyalty < 50)
                {
                    System.Random rng = new System.Random();
                    if (rng.NextDouble() < (50 - c.loyalty) / 100.0)
                    {
                        companions.RemoveAt(i);
                        OnCompanionRunAway?.Invoke(c, true);
                        Debug.Log($"[宠物] {c.name} 因忠诚度不足逃跑了...");
                    }
                }
            }
        }

        /// <summary>
        /// 使用食物安抚宠物
        /// </summary>
        public bool FeedCompanion(string companionId, int foodCount)
        {
            var companion = companions.Find(c => c.id == companionId);
            if (companion == null) return false;

            companion.RaiseLoyalty(foodCount * 10);
            Debug.Log($"[宠物] 给 {companion.name} 使用了 {foodCount} 个食物，忠诚度提升至 {companion.loyalty}");
            return true;
        }

        /// <summary>
        /// 获取跟随的宠物列表（用于UI显示）
        /// </summary>
        public List<CompanionData> GetActiveCompanions() => companions.FindAll(c => !c.isDead);

        /// <summary>
        /// 获取宠物详情（用于UI面板）
        /// </summary>
        public CompanionData GetCompanion(string id) => companions.Find(c => c.id == id);

        // ============================================================
        // 战斗中的宠物效果（由战斗系统调用）
        // ============================================================

        /// <summary>
        /// 获取所有存活动物的战斗属性加成
        /// </summary>
        public (float enemyHitMod, float playerEvadeMod, float damageReduction) GetBattleBonuses()
        {
            float enemyHitMod = 0f;
            float playerEvadeMod = 0f;
            float damageReduction = 0f;

            foreach (var c in companions)
            {
                if (c.isDead) continue;

                switch (c.type)
                {
                    case CompanionType.DreamButterfly:
                        enemyHitMod -= 0.2f;
                        playerEvadeMod += 0.15f;
                        break;
                    case CompanionType.MushroomCat:
                        damageReduction += 0.05f;
                        break;
                    case CompanionType.ForestTurtle:
                        damageReduction += 0.1f;
                        break;
                }
            }

            return (enemyHitMod, playerEvadeMod, damageReduction);
        }

        /// <summary>
        /// 萤火虫效果：夜晚遭遇降低（由遭遇系统调用）
        /// </summary>
        public float GetNightEncounterMod(bool isNight)
        {
            if (!isNight) return 0f;
            foreach (var c in companions)
            {
                if (c.isDead) continue;
                if (c.type == CompanionType.Firefly)
                    return -0.1f;
            }
            return 0f;
        }

        /// <summary>
        /// 小幽灵侦察：是否发现隐藏敌人
        /// </summary>
        public bool HasScoutDetection()
        {
            foreach (var c in companions)
            {
                if (c.isDead) continue;
                if (c.type == CompanionType.Ghost && c.unlockedSkills.Exists(s => s.name == "侦察模式"))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 蘑菇猫治疗中毒同伴（返回治疗量）
        /// </summary>
        public int HealPoisonedAlly()
        {
            foreach (var c in companions)
            {
                if (c.isDead) continue;
                if (c.type == CompanionType.MushroomCat)
                    return c.DisplayAttack; // 治疗量=攻击力
            }
            return 0;
        }

        /// <summary>
        /// 森林龟嘲讽：吸引下一次攻击
        /// </summary>
        public bool IsTurtleTaunting()
        {
            foreach (var c in companions)
            {
                if (c.isDead) continue;
                if (c.type == CompanionType.ForestTurtle)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 宠物受伤（通知UI）
        /// </summary>
        public void OnCompanionDamaged(CompanionData companion, int damage)
        {
            companion.currentHP = Mathf.Max(0, companion.currentHP - damage);
            // 受伤降低忠诚度
            companion.LowerLoyalty(damage / 5);
        }

        /// <summary>
        /// 宠物死亡
        /// </summary>
        public void OnCompanionDeath(CompanionData companion)
        {
            companion.currentHP = 0;
            companion.LowerLoyalty(30);
            Debug.Log($"[宠物] {companion.name} 倒下了...");
        }

        /// <summary>
        /// 复活宠物（消耗道具）
        /// </summary>
        public bool ReviveCompanion(string companionId)
        {
            var c = companions.Find(x => x.id == companionId);
            if (c == null) return false;

            c.currentHP = c.DisplayMaxHP / 2; // 复活至50%血量
            c.RaiseLoyalty(20);
            Debug.Log($"[宠物] {c.name} 复活了！");
            return true;
        }

        /// <summary>
        /// 放生宠物
        /// </summary>
        public bool ReleaseCompanion(string companionId)
        {
            var c = companions.Find(x => x.id == companionId);
            if (c == null) return false;

            companions.Remove(c);
            Debug.Log($"[宠物] 放生了 {c.name}，愿它自由。");
            return true;
        }
    }

    // ============================================================
    // 集成接口（对接其他系统）
    // ============================================================

    /// <summary>
    /// 游戏时钟（桩，待对接）
    /// </summary>
    public class GameClock : MonoBehaviour
    {
        public static GameClock Instance { get; private set; }
        public int day = 1;

        void Awake() { Instance = this; }
    }
}
