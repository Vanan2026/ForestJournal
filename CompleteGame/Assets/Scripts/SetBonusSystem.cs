using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 套装数据定义
/// </summary>
[Serializable]
public class SetInfo
{
    public int id;
    public string name;
    public string description;
    public List<int> pieceItemIds;  // 套装包含的物品ID列表
    public List<SetBonus> bonuses; // 套装效果列表（按激活件数排序）
}

/// <summary>
/// 套装效果
/// </summary>
[Serializable]
public class SetBonus
{
    public int requiredCount;      // 需要穿戴的件数
    public string bonusName;       // 效果名称
    public string bonusDesc;       // 效果描述
    public List<AttributeModifier> attributeModifiers;  // 属性加成
    public string skillId;        // 解锁的技能ID（可选）
    public string specialAbilityId; // 特殊能力ID（可选）
    public SetBonusTriggerType triggerType; // 触发类型
    public string triggerCondition;     // 触发条件描述
}

/// <summary>
/// 属性加成
/// </summary>
[Serializable]
public class AttributeModifier
{
    public AttributeType attributeType;
    public float value;
    public bool isPercent;  // 是否为百分比加成
}

/// <summary>
/// 属性类型枚举
/// </summary>
public enum AttributeType
{
    HP, MaxHP,
    Attack, Defense,
    CollectRate, MemoryRate,
    SoulEssenceRate, SoulEssenceDrop,
    CraftCostReduce, PotionEffect,
    DamageToMist, CriticalRate,
    Taunt
}

/// <summary>
/// 套装效果触发类型
/// </summary>
public enum SetBonusTriggerType
{
    Passive,           // 被动（常驻）
    OnEquip,           // 穿戴时触发一次
    OnBattle,          // 战斗时触发
    OnKill,            // 击杀时触发
    OnDeath,           // 死亡时触发
    OnGather,          // 采集时触发
    OnCraft,           // 制作时触发
    OnTalk,            // 对话时触发
    OnCamp,            // 安营时触发
    OnCrit,            // 暴击时触发
    OnDeadlyAttack     // 受到致命攻击时触发
}

/// <summary>
/// 套装系统核心类
/// </summary>
public class SetBonusSystem
{
    private static SetBonusSystem _instance;
    public static SetBonusSystem Instance => _instance ??= new SetBonusSystem();

    // 所有套装定义
    private List<SetInfo> _allSets;
    
    // 当前角色穿戴的套装物品统计
    private Dictionary<int, int> _equippedSetCounts;  // setId -> 已穿戴件数
    private Dictionary<int, List<SetBonus>> _activeBonuses;  // setId -> 已激活的效果

    // 事件回调
    public Action<int, SetBonus> OnSetBonusActivated;  // 套装效果激活时
    public Action<int, SetBonus> OnSetBonusDeactivated;  // 套装效果失效时
    public Action<int, int> OnSetPieceCountChanged;  // 套装件数变化时 (setId, count)

    // 特殊能力状态
    private HashSet<string> _unlockedAbilities;  // 已解锁的特殊能力ID
    private HashSet<string> _unlockedSkills;      // 已解锁的技能ID
    private Dictionary<string, int> _dailyAbilityUses;  // 每日使用次数计数

    private SetBonusSystem()
    {
        _allSets = new List<SetInfo>();
        _equippedSetCounts = new Dictionary<int, int>();
        _activeBonuses = new Dictionary<int, List<SetBonus>>();
        _unlockedAbilities = new HashSet<string>();
        _unlockedSkills = new HashSet<string>();
        _dailyAbilityUses = new Dictionary<string, int>();

        InitializeAllSets();
    }

    /// <summary>
    /// 初始化所有套装定义
    /// </summary>
    private void InitializeAllSets()
    {
        // ==================== 套装1：森林之子（4件）====================
        var forestChildSet = new SetInfo
        {
            id = 1,
            name = "森林之子",
            description = "蕴含森林之心的古老力量，与自然和谐共生",
            pieceItemIds = new List<int> { 101, 102, 103, 104 }, // 心甲/心头/心腿/心靴
            bonuses = new List<SetBonus>
            {
                new SetBonus
                {
                    requiredCount = 2,
                    bonusName = "森林守护",
                    bonusDesc = "HP+20，每回合恢复3HP",
                    attributeModifiers = new List<AttributeModifier>
                    {
                        new AttributeModifier { attributeType = AttributeType.HP, value = 20, isPercent = false },
                        new AttributeModifier { attributeType = AttributeType.MaxHP, value = 20, isPercent = false }
                    },
                    triggerType = SetBonusTriggerType.Passive
                },
                new SetBonus
                {
                    requiredCount = 3,
                    bonusName = "森林祝福",
                    bonusDesc = "采集量+30%，获得「森林祝福」技能",
                    attributeModifiers = new List<AttributeModifier>
                    {
                        new AttributeModifier { attributeType = AttributeType.CollectRate, value = 30, isPercent = true }
                    },
                    skillId = "skill_forest_blessing",
                    triggerType = SetBonusTriggerType.Passive
                },
                new SetBonus
                {
                    requiredCount = 4,
                    bonusName = "森林之心共鸣",
                    bonusDesc = "记忆获取+50%，安营时全员满血",
                    attributeModifiers = new List<AttributeModifier>
                    {
                        new AttributeModifier { attributeType = AttributeType.MemoryRate, value = 50, isPercent = true }
                    },
                    specialAbilityId = "ability_forest_camp_fullheal",
                    triggerType = SetBonusTriggerType.OnCamp
                }
            }
        };

        // ==================== 套装2：黑雾猎手（4件）====================
        var mistHunterSet = new SetInfo
        {
            id = 2,
            name = "黑雾猎手",
            description = "专为猎杀黑雾系敌人而生的战斗套装",
            pieceItemIds = new List<int> { 201, 202, 203, 204 }, // 黑雾猎手胸甲/头盔/护腿/靴
            bonuses = new List<SetBonus>
            {
                new SetBonus
                {
                    requiredCount = 2,
                    bonusName = "黑雾克星",
                    bonusDesc = "攻击+15，对黑雾系敌人+20伤害",
                    attributeModifiers = new List<AttributeModifier>
                    {
                        new AttributeModifier { attributeType = AttributeType.Attack, value = 15, isPercent = false },
                        new AttributeModifier { attributeType = AttributeType.DamageToMist, value = 20, isPercent = false }
                    },
                    triggerType = SetBonusTriggerType.Passive
                },
                new SetBonus
                {
                    requiredCount = 3,
                    bonusName = "猎魂者",
                    bonusDesc = "战斗获得魂精华+50%",
                    attributeModifiers = new List<AttributeModifier>
                    {
                        new AttributeModifier { attributeType = AttributeType.SoulEssenceRate, value = 50, isPercent = true }
                    },
                    triggerType = SetBonusTriggerType.OnBattle
                },
                new SetBonus
                {
                    requiredCount = 4,
                    bonusName = "黑雾猎手",
                    bonusDesc = "黑雾系敌人掉落翻倍，解锁「黑雾猎手」称号",
                    attributeModifiers = new List<AttributeModifier>
                    {
                        new AttributeModifier { attributeType = AttributeType.SoulEssenceDrop, value = 100, isPercent = true }
                    },
                    specialAbilityId = "title_mist_hunter",
                    triggerType = SetBonusTriggerType.OnKill
                }
            }
        };

        // ==================== 套装3：古老守护者（3件）====================
        var ancientGuardianSet = new SetInfo
        {
            id = 3,
            name = "古老守护者",
            description = "远古文明的防御结晶，坚不可摧",
            pieceItemIds = new List<int> { 301, 302, 303 }, // 古老巨盔/胸甲/巨盾
            bonuses = new List<SetBonus>
            {
                new SetBonus
                {
                    requiredCount = 2,
                    bonusName = "铜墙铁壁",
                    bonusDesc = "防御+25，受伤-20%",
                    attributeModifiers = new List<AttributeModifier>
                    {
                        new AttributeModifier { attributeType = AttributeType.Defense, value = 25, isPercent = false },
                        new AttributeModifier { attributeType = AttributeType.DamageToMist, value = -20, isPercent = true }
                    },
                    triggerType = SetBonusTriggerType.Passive
                },
                new SetBonus
                {
                    requiredCount = 3,
                    bonusName = "不屈意志",
                    bonusDesc = "受到致命攻击时有30%概率存活1HP（每天一次）",
                    specialAbilityId = "ability_survive_deadly",
                    triggerType = SetBonusTriggerType.OnDeadlyAttack
                },
                new SetBonus
                {
                    requiredCount = 3, // 注意：古老守护者只有3件，但效果3是4件，这里标注为3件套效果
                    bonusName = "守护嘲讽",
                    bonusDesc = "嘲讽：敌人优先攻击你",
                    attributeModifiers = new List<AttributeModifier>
                    {
                        new AttributeModifier { attributeType = AttributeType.Taunt, value = 100, isPercent = false }
                    },
                    triggerType = SetBonusTriggerType.Passive
                }
            }
        };

        // ==================== 套装4：灵魂行者（4件）====================
        var soulWalkerSet = new SetInfo
        {
            id = 4,
            name = "灵魂行者",
            description = "穿梭于生死之间的神秘旅者",
            pieceItemIds = new List<int> { 401, 402, 403, 404 }, // 长袍/面具/手套/之履
            bonuses = new List<SetBonus>
            {
                new SetBonus
                {
                    requiredCount = 2,
                    bonusName = "灵魂亲和",
                    bonusDesc = "魂精华获取+30%",
                    attributeModifiers = new List<AttributeModifier>
                    {
                        new AttributeModifier { attributeType = AttributeType.SoulEssenceRate, value = 30, isPercent = true }
                    },
                    triggerType = SetBonusTriggerType.Passive
                },
                new SetBonus
                {
                    requiredCount = 3,
                    bonusName = "灵魂复苏",
                    bonusDesc = "死亡时可以选择复活（消耗10魂精华）",
                    specialAbilityId = "ability_resurrect",
                    triggerType = SetBonusTriggerType.OnDeath
                },
                new SetBonus
                {
                    requiredCount = 4,
                    bonusName = "灵魂共鸣",
                    bonusDesc = "与NPC对话时自动获得+1好感度",
                    specialAbilityId = "ability_npc_favor",
                    triggerType = SetBonusTriggerType.OnTalk
                }
            }
        };

        // ==================== 套装5：炼金大师（4件）====================
        var alchemyMasterSet = new SetInfo
        {
            id = 5,
            name = "炼金大师",
            description = "掌握炼金术奥秘的智者套装",
            pieceItemIds = new List<int> { 501, 502, 503, 504 }, // 外袍/护目镜/手套/药剂袋
            bonuses = new List<SetBonus>
            {
                new SetBonus
                {
                    requiredCount = 2,
                    bonusName = "精打细算",
                    bonusDesc = "制作消耗-25%材料",
                    attributeModifiers = new List<AttributeModifier>
                    {
                        new AttributeModifier { attributeType = AttributeType.CraftCostReduce, value = 25, isPercent = true }
                    },
                    triggerType = SetBonusTriggerType.OnCraft
                },
                new SetBonus
                {
                    requiredCount = 3,
                    bonusName = "炼金奥秘",
                    bonusDesc = "药水效果+50%，制作时有几率双倍",
                    attributeModifiers = new List<AttributeModifier>
                    {
                        new AttributeModifier { attributeType = AttributeType.PotionEffect, value = 50, isPercent = true }
                    },
                    specialAbilityId = "ability_craft_double",
                    triggerType = SetBonusTriggerType.OnCraft
                },
                new SetBonus
                {
                    requiredCount = 4,
                    bonusName = "炼金宗师",
                    bonusDesc = "解锁隐藏配方，制作传说药剂",
                    specialAbilityId = "ability_legend_recipe",
                    triggerType = SetBonusTriggerType.OnCraft
                }
            }
        };

        _allSets.Add(forestChildSet);
        _allSets.Add(mistHunterSet);
        _allSets.Add(ancientGuardianSet);
        _allSets.Add(soulWalkerSet);
        _allSets.Add(alchemyMasterSet);
    }

    /// <summary>
    /// 获取所有套装信息
    /// </summary>
    public List<SetInfo> GetAllSets() => _allSets;

    /// <summary>
    /// 根据套装ID获取套装信息
    /// </summary>
    public SetInfo GetSetById(int setId)
    {
        return _allSets.FirstOrDefault(s => s.id == setId);
    }

    /// <summary>
    /// 根据物品ID获取所属套装
    /// </summary>
    public SetInfo GetSetByItemId(int itemId)
    {
        return _allSets.FirstOrDefault(s => s.pieceItemIds.Contains(itemId));
    }

    /// <summary>
    /// 穿戴装备时调用，检查并激活套装效果
    /// </summary>
    public void OnEquipItem(int itemId)
    {
        var set = GetSetByItemId(itemId);
        if (set == null) return;

        int setId = set.id;
        
        // 增加套装计数
        if (!_equippedSetCounts.ContainsKey(setId))
            _equippedSetCounts[setId] = 0;
        
        int oldCount = _equippedSetCounts[setId];
        int newCount = oldCount + 1;
        _equippedSetCounts[setId] = newCount;

        // 初始化活动效果列表
        if (!_activeBonuses.ContainsKey(setId))
            _activeBonuses[setId] = new List<SetBonus>();

        // 检查是否有新的套装效果可以激活
        foreach (var bonus in set.bonuses)
        {
            if (newCount >= bonus.requiredCount && oldCount < bonus.requiredCount)
            {
                ActivateSetBonus(setId, bonus);
            }
        }

        OnSetPieceCountChanged?.Invoke(setId, newCount);
        
        // 与GameManager集成：更新属性
        ApplyAllActiveBonuses();
    }

    /// <summary>
    /// 卸下装备时调用，检查并停用套装效果
    /// </summary>
    public void OnUnequipItem(int itemId)
    {
        var set = GetSetByItemId(itemId);
        if (set == null) return;

        int setId = set.id;
        
        // 减少套装计数
        if (!_equippedSetCounts.ContainsKey(setId))
            _equippedSetCounts[setId] = 0;
        
        int oldCount = _equippedSetCounts[setId];
        int newCount = Math.Max(0, oldCount - 1);
        _equippedSetCounts[setId] = newCount;

        // 检查是否有效果需要停用
        if (_activeBonuses.ContainsKey(setId))
        {
            var toDeactivate = _activeBonuses[setId]
                .Where(b => newCount < b.requiredCount)
                .ToList();
            
            foreach (var bonus in toDeactivate)
            {
                DeactivateSetBonus(setId, bonus);
            }
        }

        OnSetPieceCountChanged?.Invoke(setId, newCount);
        
        // 更新属性
        ApplyAllActiveBonuses();
    }

    /// <summary>
    /// 激活套装效果
    /// </summary>
    private void ActivateSetBonus(int setId, SetBonus bonus)
    {
        if (!_activeBonuses.ContainsKey(setId))
            _activeBonuses[setId] = new List<SetBonus>();
        
        if (!_activeBonuses[setId].Contains(bonus))
            _activeBonuses[setId].Add(bonus);

        // 解锁技能
        if (!string.IsNullOrEmpty(bonus.skillId))
        {
            _unlockedSkills.Add(bonus.skillId);
        }

        // 解锁特殊能力
        if (!string.IsNullOrEmpty(bonus.specialAbilityId))
        {
            _unlockedAbilities.Add(bonus.specialAbilityId);
        }

        OnSetBonusActivated?.Invoke(setId, bonus);
    }

    /// <summary>
    /// 停用套装效果
    /// </summary>
    private void DeactivateSetBonus(int setId, SetBonus bonus)
    {
        if (_activeBonuses.ContainsKey(setId))
        {
            _activeBonuses[setId].Remove(bonus);
        }

        // 注意：技能和能力一旦解锁不会因为装备减少而移除
        // 这是常见的设计决策，可以根据需求修改

        OnSetBonusDeactivated?.Invoke(setId, bonus);
    }

    /// <summary>
    /// 应用所有已激活的套装效果
    /// </summary>
    public void ApplyAllActiveBonuses()
    {
        // 清除所有套装属性加成
        ClearAllBonusAttributes();

        // 重新应用所有激活效果
        foreach (var kvp in _activeBonuses)
        {
            foreach (var bonus in kvp.Value)
            {
                ApplyBonusAttributes(bonus);
            }
        }
    }

    /// <summary>
    /// 应用单个效果的属性加成
    /// </summary>
    private void ApplyBonusAttributes(SetBonus bonus)
    {
        if (bonus.attributeModifiers == null) return;

        foreach (var mod in bonus.attributeModifiers)
        {
            ApplyAttributeModifier(mod);
        }
    }

    /// <summary>
    /// 应用属性修改器
    /// </summary>
    private void ApplyAttributeModifier(AttributeModifier mod)
    {
        // 与GameManager集成，实际修改角色属性
        // 这里通过GameManager.Instance来访问属性系统
        // GameManager.Instance.ModifyAttribute(mod.attributeType, mod.value, mod.isPercent);
    }

    /// <summary>
    /// 清除所有套装属性加成
    /// </summary>
    private void ClearAllBonusAttributes()
    {
        // 通知GameManager清除临时属性
        // GameManager.Instance.ClearTemporaryAttributes();
    }

    /// <summary>
    /// 获取当前穿戴某套装的件数
    /// </summary>
    public int GetEquippedCount(int setId)
    {
        return _equippedSetCounts.TryGetValue(setId, out int count) ? count : 0;
    }

    /// <summary>
    /// 获取某套装的信息（名称、已穿件数、总件数、已激活效果）
    /// </summary>
    public SetProgress GetSetProgress(int setId)
    {
        var set = GetSetById(setId);
        if (set == null) return null;

        int equipped = GetEquippedCount(setId);
        var activeBonuses = _activeBonuses.TryGetValue(setId, out var bonuses) 
            ? bonuses.ToList() 
            : new List<SetBonus>();

        return new SetProgress
        {
            setInfo = set,
            equippedCount = equipped,
            totalCount = set.pieceItemIds.Count,
            activeBonuses = activeBonuses
        };
    }

    /// <summary>
    /// 获取所有套装的进度信息
    /// </summary>
    public List<SetProgress> GetAllSetProgress()
    {
        return _allSets.Select(s => GetSetProgress(s.id)).ToList();
    }

    /// <summary>
    /// 检查是否解锁了某个技能
    /// </summary>
    public bool HasSkill(string skillId)
    {
        return _unlockedSkills.Contains(skillId);
    }

    /// <summary>
    /// 检查是否解锁了某个特殊能力
    /// </summary>
    public bool HasAbility(string abilityId)
    {
        return _unlockedAbilities.Contains(abilityId);
    }

    /// <summary>
    /// 使用每日一次的特殊能力
    /// </summary>
    public bool UseDailyAbility(string abilityId)
    {
        if (!_unlockedAbilities.Contains(abilityId)) return false;
        
        string dailyKey = $"{abilityId}_{GetCurrentDayKey()}";
        
        if (_dailyAbilityUses.TryGetValue(dailyKey, out int uses) && uses >= 1)
            return false; // 已用完

        _dailyAbilityUses[dailyKey] = uses + 1;
        return true;
    }

    /// <summary>
    /// 获取当前日期key（用于每日重置）
    /// </summary>
    private string GetCurrentDayKey()
    {
        return DateTime.Now.ToString("yyyy-MM-dd");
    }

    /// <summary>
    /// 重置每日能力计数（每天调用一次）
    /// </summary>
    public void ResetDailyAbilities()
    {
        string todayKey = GetCurrentDayKey();
        var keysToRemove = _dailyAbilityUses.Keys
            .Where(k => !k.EndsWith(todayKey))
            .ToList();
        
        foreach (var key in keysToRemove)
        {
            _dailyAbilityUses.Remove(key);
        }
    }

    /// <summary>
    /// 触发套装特效（由战斗系统、采集系统等调用）
    /// </summary>
    public void TriggerSetEffect(SetBonusTriggerType triggerType, object context = null)
    {
        foreach (var kvp in _activeBonuses)
        {
            foreach (var bonus in kvp.Value)
            {
                if (bonus.triggerType == triggerType)
                {
                    TriggerBonusEffect(bonus, context);
                }
            }
        }
    }

    /// <summary>
    /// 触发单个效果
    /// </summary>
    private void TriggerBonusEffect(SetBonus bonus, object context)
    {
        switch (bonus.triggerType)
        {
            case SetBonusTriggerType.OnKill:
                // 处理击杀奖励（黑雾猎手4件套）
                HandleKillEffect(bonus, context);
                break;
                
            case SetBonusTriggerType.OnDeath:
                // 处理死亡效果（灵魂行者3件套）
                HandleDeathEffect(bonus, context);
                break;
                
            case SetBonusTriggerType.OnCamp:
                // 处理安营效果（森林之子4件套）
                HandleCampEffect(bonus, context);
                break;
                
            case SetBonusTriggerType.OnTalk:
                // 处理对话效果（灵魂行者4件套）
                HandleTalkEffect(bonus, context);
                break;
                
            case SetBonusTriggerType.OnDeadlyAttack:
                // 处理致命攻击（古老守护者3件套）
                HandleDeadlyAttackEffect(bonus, context);
                break;
        }
    }

    /// <summary>
    /// 处理击杀效果
    /// </summary>
    private void HandleKillEffect(SetBonus bonus, object context)
    {
        // context应该是被击杀的敌人信息
        // 检查是否为黑雾系敌人，若是则触发掉落翻倍
    }

    /// <summary>
    /// 处理死亡效果
    /// </summary>
    private void HandleDeathEffect(SetBonus bonus, object context)
    {
        // 显示复活选项UI
        // 消耗10魂精华复活
    }

    /// <summary>
    /// 处理安营效果
    /// </summary>
    private void HandleCampEffect(SetBonus bonus, object context)
    {
        // 实现全员满血
    }

    /// <summary>
    /// 处理对话效果
    /// </summary>
    private void HandleTalkEffect(SetBonus bonus, object context)
    {
        // context应该是NPC信息，自动增加好感度
    }

    /// <summary>
    /// 处理致命攻击效果
    /// </summary>
    private void HandleDeadlyAttackEffect(SetBonus bonus, object context)
    {
        // 检查是否还有每日次数
        if (UseDailyAbility("ability_survive_deadly"))
        {
            // 30%概率存活1HP
            // 注意：这个概率应该在效果触发时决定，而不是使用时决定
        }
    }

    /// <summary>
    /// 获取套装激活的UI显示信息
    /// </summary>
    public List<SetUIInfo> GetSetUIInfos()
    {
        var result = new List<SetUIInfo>();
        
        foreach (var set in _allSets)
        {
            var progress = GetSetProgress(set.id);
            result.Add(new SetUIInfo
            {
                setId = set.id,
                setName = set.name,
                description = set.description,
                equippedCount = progress.equippedCount,
                totalCount = progress.totalCount,
                nextBonusDesc = GetNextBonusDescription(set.id),
                activeBonusDescs = progress.activeBonuses.Select(b => b.bonusDesc).ToList()
            });
        }
        
        return result;
    }

    /// <summary>
    /// 获取下一级套装效果描述
    /// </summary>
    private string GetNextBonusDescription(int setId)
    {
        var set = GetSetById(setId);
        int equipped = GetEquippedCount(setId);
        
        var nextBonus = set.bonuses
            .Where(b => b.requiredCount > equipped)
            .OrderBy(b => b.requiredCount)
            .FirstOrDefault();
        
        return nextBonus?.bonusDesc ?? "已全部激活";
    }
}

/// <summary>
/// 套装进度信息
/// </summary>
public class SetProgress
{
    public SetInfo setInfo;
    public int equippedCount;
    public int totalCount;
    public List<SetBonus> activeBonuses;
}

/// <summary>
/// 套装UI显示信息
/// </summary>
public class SetUIInfo
{
    public int setId;
    public string setName;
    public string description;
    public int equippedCount;
    public int totalCount;
    public string nextBonusDesc;
    public List<string> activeBonusDescs;
}
