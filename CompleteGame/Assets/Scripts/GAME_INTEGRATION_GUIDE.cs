using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// GameManager 核心类扩展
/// 添加 gs 游戏状态 / 阶段事件 / 手账集成
/// 
/// 在原有 GameManager.cs 基础上追加以下内容：
/// 1. gs 字段（gameover/victory/normal）
/// 2. OnPhaseChanged 事件
/// 3. GameOver/Victory 判断
/// 4. 手账记录调用
/// 5. BtnCardCombat 手牌战斗入口
/// </summary>
public static class GameManagerExtensions2
{
    // ====================
    // gs 状态字段（追加到 GameManager.cs）
    // ====================

    // 在 GameManager.cs 的 [Header("叙事")] 区域添加：
    // public string gs = "normal";  // "normal" | "gameover" | "victory"

    // ====================
    // 游戏状态检查（追加 CanTakeAction 末尾）
    // ====================

    // 在 GameManager.cs 的 CanTakeAction 方法末尾添加：
    // if (gs != "normal") { AddLog("游戏已结束"); return false; }

    // ====================
    // Combat 末尾添加（追加检查死亡/胜利）
    // ====================

    // 在 Combat() 方法末尾，UpdateAllUI() 之前添加：
    /*
    // 死亡检查
    if (selectedMember != null && selectedMember.currentHealth <= 0)
    {
        selectedMember.status = "重伤";
        AddLog($"{selectedMember.memberName} 失去战斗能力！");

        // 队伍是否全灭
        bool allDead = true;
        foreach (var m in squad)
        {
            if (m.currentHealth > 0) { allDead = false; break; }
        }
        if (allDead)
        {
            gs = "gameover";
            AddLog("═══ 队伍全灭 ═══");
            AddLog("你们倒在了森林中...");
            AddLog("—— GAME OVER ——");
        }
    }

    // 森林之心胜利检查
    if (currentRegion == 4 && storyPhase >= 4)
    {
        gs = "victory";
        AddLog("═══ 到达森林之心 ═══");
        AddLog("你们找到了传说中的森林之心！");
        AddLog("—— 胜利！——");
        if (JournalSystem.instance != null)
            JournalSystem.instance.OnForestHeartReached("在森林之心的光芒中，你们揭开了这片森林的秘密。");
    }
    */

    // ====================
    // AdvanceDay 末尾添加（追加每日消耗检查）
    // ====================

    // 在 AdvanceDay() 末尾添加：
    /*
    // 每日食物消耗
    int dailyFoodConsumption = squad.Count;
    if (food >= dailyFoodConsumption)
    {
        food -= dailyFoodConsumption;
        AddLog($"每日消耗 {dailyFoodConsumption} 食物");
    }
    else
    {
        // 饥饿惩罚
        foreach (var m in squad)
        {
            m.currentHealth -= 10;
            AddLog($"{m.memberName} 饥饿！-10 HP");
        }
        threatLevel = Mathf.Min(threatLevel + 1, 5);
    }

    // 触发手账记录
    if (JournalSystem.instance != null)
    {
        JournalSystem.instance.Record("day", $"第 {currentDay} 天", $"新的一天开始了。队伍现在有 {squad.Count} 名成员。");
    }
    */
}

// ============================================
// 快速集成指南
// ============================================
// 将以下内容追加到 Unity_Full/GameManager.cs 末尾（在最后一个 } 之前）：
//
// 1. 在 [Header("叙事")] 区域后添加：
//    public string gs = "normal";  // "normal" | "gameover" | "victory"
//
// 2. 在 CanTakeAction() return true 之前添加：
//    if (gs != "normal") { AddLog("游戏已结束"); return false; }
//
// 3. 在 Combat() 的 UpdateAllUI() 之前添加死亡/胜利检查（见上方代码块）
//
// 4. 在 AdvanceDay() 末尾添加每日消耗+手账记录（见上方代码块）
//
// 5. 将 BtnCombat 替换为手牌战斗：
//    public void BtnCombat() { GameManagerPatch.instance?.StartCardCombat(); }
//
// 6. 添加手账按钮：
//    public void BtnJournal() { GameManagerPatch.instance?.ShowJournal(); }
