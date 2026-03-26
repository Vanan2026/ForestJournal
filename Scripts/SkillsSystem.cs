using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 技能系统 v1.0（SPEC v2.6 第八章）
/// 5个技能：觅食/生火/草药学/狩猎/追踪
/// </summary>
public class SkillsSystem : MonoBehaviour
{
    public static SkillsSystem instance { get; private set; }

    [Header("技能状态")]
    public bool hasFireMastery = false;       // 生火：初始解锁，食物-1/天
    public bool hasForaging = false;           // 觅食：Day10解锁，采集+30%
    public bool hasHerbalism = false;           // 草药学：Day15+炼金师，草药不消耗
    public bool hasHunting = false;            // 狩猎：Day20+漫游者，可猎大猎物
    public bool hasTracking = false;           // 追踪：Day15+巫女，显示威胁来源

    [Header("火堆")]
    public int campfireCount = 0;              // 火堆数量，减免食物消耗
    public const int FOOD_PER_CAMPFIRE = 1;    // 每堆火减少1食物消耗/天

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // 初始检查：已有生火技能
        if (GameManager.instance != null && GameManager.instance.currentDay >= 1)
        {
            hasFireMastery = true;
        }
    }

    // ====================
    // 技能激活检查（每日开始时调用）
    // ====================

    /// <summary>
    /// 每日检查技能解锁
    /// </summary>
    public void CheckDailySkillUnlocks(int day, List<SquadMember> squad)
    {
        if (day >= 10 && !hasForaging)
        {
            hasForaging = true;
            AddSkillLog("【觅食】已解锁！采集资源+30%");
        }

        if (day >= 20 && !hasHunting)
        {
            // 检查是否有漫游者
            foreach (var m in squad)
            {
                if (m.role == "漫游者")
                {
                    hasHunting = true;
                    AddSkillLog("【狩猎】已解锁！可猎大型猎物");
                    break;
                }
            }
        }

        if (day >= 15 && !hasTracking)
        {
            // 检查是否有巫女
            foreach (var m in squad)
            {
                if (m.role == "巫女")
                {
                    hasTracking = true;
                    AddSkillLog("【追踪】已解锁！可感知威胁来源");
                    break;
                }
            }
        }

        if (day >= 15 && !hasHerbalism)
        {
            // 检查是否有炼金师
            foreach (var m in squad)
            {
                if (m.role == "炼金师")
                {
                    hasHerbalism = true;
                    AddSkillLog("【草药学】已解锁！草药治疗不消耗");
                    break;
                }
            }
        }
    }

    // ====================
    // 技能效果
    // ====================

    /// <summary>
    /// 获取采集加成（觅食技能）
    /// </summary>
    public int GetGatherBonus(int baseAmount)
    {
        if (hasForaging)
            return Mathf.RoundToInt(baseAmount * 0.3f);
        return 0;
    }

    /// <summary>
    /// 获取每日食物减免（火堆+生火）
    /// </summary>
    public int GetDailyFoodReduction()
    {
        int reduction = 0;
        if (hasFireMastery) reduction++;
        reduction += campfireCount * FOOD_PER_CAMPFIRE;
        return reduction;
    }

    /// <summary>
    /// 草药治疗是否消耗草药（草药学）
    /// </summary>
    public bool DoesHerbConsume()
    {
        return !hasHerbalism;
    }

    /// <summary>
    /// 是否可以狩猎大型猎物
    /// </summary>
    public bool CanHuntLarge()
    {
        return hasHunting;
    }

    /// <summary>
    /// 追踪技能：获取威胁来源信息
    /// </summary>
    public string GetThreatSource()
    {
        if (!hasTracking) return null;

        if (GameManager.instance == null) return null;

        int threat = GameManager.instance.threatLevel;
        int region = GameManager.instance.currentRegion;

        string[] sources = {
            "黑雾浓度：正常",
            "黑雾浓度：略有上升",
            "黑雾浓度：较浓",
            "黑雾浓度：非常浓",
            "黑雾浓度：禁区级别！"
        };

        return sources[Mathf.Clamp(threat - 1, 0, 4)];
    }

    // ====================
    // 火堆建造
    // ====================

    /// <summary>
    /// 建造火堆（消耗木材）
    /// </summary>
    public bool BuildCampfire()
    {
        if (GameManager.instance == null) return false;
        if (GameManager.instance.wood < 5)
        {
            GameManager.instance.AddLog("⚠️ 木材不足！需要5木材建造火堆");
            return false;
        }

        GameManager.instance.wood -= 5;
        campfireCount++;
        GameManager.instance.AddLog($"🔥 建造火堆成功！({campfireCount}/3)");
        return true;
    }

    // ====================
    // 受伤 debuff（SPEC：攻击力永久-30%）
    // ====================

    /// <summary>
    /// 造成受伤debuff（30%概率）
    /// </summary>
    public void ApplyInjuryDebuff(SquadMember member)
    {
        if (UnityEngine.Random.value < 0.3f)
        {
            float originalATK = member.attack;
            member.attack = Mathf.RoundToInt(member.attack * 0.7f);
            member.status = "受伤";
            GameManager.instance?.AddLog(
                $"💢 {member.memberName} 受伤！攻击力 {originalATK} → {member.attack}（永久-30%）");
        }
    }

    /// <summary>
    /// 治疗受伤状态（草药/休息）
    /// </summary>
    public void HealInjury(SquadMember member)
    {
        if (member.status == "受伤")
        {
            member.status = "正常";
            GameManager.instance?.AddLog($"{member.memberName} 的伤势已痊愈");
        }
    }

    // ====================
    // UI显示
    // ====================

    void AddSkillLog(string msg)
    {
        if (GameManager.instance != null)
            GameManager.instance.AddLog(msg);
        Debug.Log($"[Skills] {msg}");
    }

    /// <summary>
    /// 获取技能状态描述
    /// </summary>
    public List<string> GetAllSkillDescriptions()
    {
        var list = new List<string>();

        list.Add($"生火：食物消耗-{(1 + campfireCount)}/天");
        if (hasForaging) list.Add("觅食：采集+30%");
        if (hasHerbalism) list.Add("草药学：治疗不消耗草药");
        if (hasHunting) list.Add("狩猎：可猎大猎物");
        if (hasTracking) list.Add("追踪：可感知威胁");

        return list;
    }
}
