using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// 完整存档系统 v1.0
/// 支持：游戏状态/NPC关系/章节进度/资源配置/多槽位
/// </summary>
public class SaveLoadSystem : MonoBehaviour
{
    public static SaveLoadSystem instance { get; private set; }

    [Header("存档槽位")]
    public const int MAX_SLOTS = 3;
    public SaveSlot[] saveSlots = new SaveSlot[MAX_SLOTS];
    public int lastSaveSlot = -1;

    private string SAVE_FOLDER => Application.persistentDataPath + "/Saves/";
    private const string SAVE_FILE_PREFIX = "forest_journal_save_";
    private const string SAVE_FILE_EXT = ".fjv";  // forest journal version

    void Awake()
    {
        instance = this;
        EnsureSaveFolder();
        LoadSlotInfo();
    }

    void EnsureSaveFolder()
    {
        if (!Directory.Exists(SAVE_FOLDER))
            Directory.CreateDirectory(SAVE_FOLDER);
    }

    // ====================
    // 公开 API
    // ====================

    /// <summary>
    /// 保存到指定槽位
    /// </summary>
    public bool Save(int slot)
    {
        if (slot < 0 || slot >= MAX_SLOTS) return false;

        try
        {
            var saveData = CreateSaveData();
            string json = JsonUtility.ToJson(saveData, true);
            string path = GetSlotPath(slot);

            // AES简单加密（防止直接乱改）
            string encrypted = SimpleEncrypt(json);
            File.WriteAllText(path, encrypted);

            saveSlots[slot] = saveData;
            lastSaveSlot = slot;

            Debug.Log($"[SaveLoad] 保存至槽位 {slot} 成功");
            if (GameManager.instance != null)
                GameManager.instance.AddLog($"💾 游戏已保存（槽位 {slot + 1}）");

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveLoad] 保存失败：{e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 从指定槽位加载
    /// </summary>
    public bool Load(int slot)
    {
        if (slot < 0 || slot >= MAX_SLOTS) return false;

        string path = GetSlotPath(slot);
        if (!File.Exists(path)) return false;

        try
        {
            string encrypted = File.ReadAllText(path);
            string json = SimpleDecrypt(encrypted);
            var saveData = JsonUtility.FromJson<SaveSlot>(json);

            ApplySaveData(saveData);
            lastSaveSlot = slot;

            Debug.Log($"[SaveLoad] 从槽位 {slot} 加载成功");
            if (GameManager.instance != null)
                GameManager.instance.AddLog($"📂 游戏已加载（槽位 {slot + 1}）");

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveLoad] 加载失败：{e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 删除存档
    /// </summary>
    public bool Delete(int slot)
    {
        if (slot < 0 || slot >= MAX_SLOTS) return false;

        string path = GetSlotPath(slot);
        if (File.Exists(path))
        {
            File.Delete(path);
            saveSlots[slot] = null;
            Debug.Log($"[SaveLoad] 删除槽位 {slot}");
            return true;
        }
        return false;
    }

    /// <summary>
    /// 快速保存（自动槽位）
    /// </summary>
    public bool QuickSave()
    {
        int slot = lastSaveSlot >= 0 ? lastSaveSlot : 0;
        return Save(slot);
    }

    /// <summary>
    /// 快速加载（上次存档）
    /// </summary>
    public bool QuickLoad()
    {
        if (lastSaveSlot < 0) lastSaveSlot = 0;
        return Load(lastSaveSlot);
    }

    /// <summary>
    /// 检查存档是否存在
    /// </summary>
    public bool HasSave(int slot)
    {
        if (slot < 0 || slot >= MAX_SLOTS) return false;
        return File.Exists(GetSlotPath(slot));
    }

    // ====================
    // 创建存档数据
    // ====================

    SaveSlot CreateSaveData()
    {
        var slot = new SaveSlot();
        var gm = GameManager.instance;

        if (gm == null) return slot;

        // === 游戏核心状态 ===
        slot.version = "4.2";
        slot.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        slot.gameDay = gm.currentDay;
        slot.gamePhase = gm.currentPhase;
        slot.currentRegion = gm.currentRegion;
        slot.storyPhase = gm.storyPhase;
        slot.gameState = gm.gs;

        // === 资源 ===
        slot.food = gm.food;
        slot.wood = gm.wood;
        slot.stone = gm.stone;
        slot.herb = gm.herb;
        slot.fiber = gm.fiber;
        slot.ore = gm.ore;
        slot.bone = gm.bone;
        slot.soulEssence = gm.soulEssence;
        slot.memories = gm.memories;
        slot.threatLevel = gm.threatLevel;

        // === 队伍 ===
        slot.squadMembers = new List<SquadMemberSave>();
        foreach (var m in gm.squad)
        {
            slot.squadMembers.Add(new SquadMemberSave
            {
                memberId = m.memberId,
                memberName = m.memberName,
                role = m.role,
                maxHealth = m.maxHealth,
                currentHealth = m.currentHealth,
                attack = m.attack,
                defense = m.defense,
                intelligence = m.intelligence,
                agility = m.agility,
                status = m.status
            });
        }

        // === 区域发现 ===
        slot.discoveredRegions = new List<int>();
        if (gm.regions != null)
        {
            for (int i = 0; i < gm.regions.Length; i++)
            {
                if (gm.regions[i].discovered)
                    slot.discoveredRegions.Add(i);
            }
        }

        // === 手账记录 ===
        var journal = FindObjectOfType<JournalSystem>();
        if (journal != null)
        {
            slot.journalEntryCount = journal.entries.Count;
            slot.memoryFragments = journal.memoryFragments;
        }

        // === NPC 关系 ===
        var rel = FindObjectOfType<RelationshipSystem>();
        if (rel != null)
        {
            slot.npcRelationships = new List<NPCRelationshipSave>();
            foreach (var kvp in rel.relationships)
            {
                slot.npcRelationships.Add(new NPCRelationshipSave
                {
                    npcId = kvp.Key,
                    trustLevel = kvp.Value.trustLevel,
                    timesMet = kvp.Value.timesMet,
                    attitude = kvp.Value.attitudeText,
                    unlockedSecrets = kvp.Value.unlockedSecrets
                });
            }
        }

        // === 技能状态 ===
        var skills = FindObjectOfType<SkillsSystem>();
        if (skills != null)
        {
            slot.campfireCount = skills.campfireCount;
            slot.hasForaging = skills.hasForaging;
            slot.hasHerbalism = skills.hasHerbalism;
            slot.hasHunting = skills.hasHunting;
            slot.hasTracking = skills.hasTracking;
        }

        // === 制作产物 ===
        var crafting = FindObjectOfType<CraftingSystem>();
        if (crafting != null)
        {
            slot.bandages = crafting.bandages;
            slot.herbPoultice = crafting.herbPoultice;
            slot.medicine = crafting.medicine;
            slot.torch = crafting.torch;
            slot.rope = crafting.rope;
            slot.boneBlade = crafting.boneBlade;
            slot.boneArmor = crafting.boneArmor;
            slot.huntersBow = crafting.huntersBow;
            slot.forestCloak = crafting.forestCloak;
            slot.healingFlute = crafting.healingFlute;
            slot.essenceAmulet = crafting.essenceAmulet;
            slot.ancientRelic = crafting.ancientRelic;
        }

        return slot;
    }

    // ====================
    // 应用存档数据
    // ====================

    void ApplySaveData(SaveSlot slot)
    {
        var gm = GameManager.instance;
        if (gm == null) return;

        // 版本兼容检查
        if (!string.IsNullOrEmpty(slot.version))
        {
            Debug.Log($"[SaveLoad] 加载 v{slot.version} 存档");
        }

        // === 游戏核心状态 ===
        gm.currentDay = slot.gameDay;
        gm.currentPhase = slot.gamePhase;
        gm.currentRegion = slot.currentRegion;
        gm.storyPhase = slot.storyPhase;
        gm.gs = slot.gameState ?? "normal";

        // === 资源 ===
        gm.food = slot.food;
        gm.wood = slot.wood;
        gm.stone = slot.stone;
        gm.herb = slot.herb;
        gm.fiber = slot.fiber;
        gm.ore = slot.ore;
        gm.bone = slot.bone;
        gm.soulEssence = slot.soulEssence;
        gm.memories = slot.memories;
        gm.threatLevel = slot.threatLevel;

        // === 队伍 ===
        gm.squad.Clear();
        if (slot.squadMembers != null)
        {
            foreach (var ms in slot.squadMembers)
            {
                gm.squad.Add(new SquadMember
                {
                    memberId = ms.memberId,
                    memberName = ms.memberName,
                    role = ms.role,
                    maxHealth = ms.maxHealth,
                    currentHealth = ms.currentHealth,
                    attack = ms.attack,
                    defense = ms.defense,
                    intelligence = ms.intelligence,
                    agility = ms.agility,
                    status = ms.status
                });
            }
        }
        if (gm.squad.Count > 0)
            gm.selectedMember = gm.squad[0];

        // === 区域发现 ===
        if (gm.regions != null && slot.discoveredRegions != null)
        {
            foreach (var rid in slot.discoveredRegions)
            {
                if (rid >= 0 && rid < gm.regions.Length)
                    gm.regions[rid].discovered = true;
            }
        }

        // === NPC 关系 ===
        var rel = FindObjectOfType<RelationshipSystem>();
        if (rel != null && slot.npcRelationships != null)
        {
            rel.relationships.Clear();
            foreach (var rs in slot.npcRelationships)
            {
                rel.relationships[rs.npcId] = new RelationshipSystem.NPCRelationship
                {
                    npcId = rs.npcId,
                    trustLevel = rs.trustLevel,
                    timesMet = rs.timesMet,
                    attitudeText = rs.attitude
                };
            }
        }

        // === 技能 ===
        var skills = FindObjectOfType<SkillsSystem>();
        if (skills != null)
        {
            skills.campfireCount = slot.campfireCount;
            skills.hasForaging = slot.hasForaging;
            skills.hasHerbalism = slot.hasHerbalism;
            skills.hasHunting = slot.hasHunting;
            skills.hasTracking = slot.hasTracking;
        }

        // === 制作 ===
        var crafting = FindObjectOfType<CraftingSystem>();
        if (crafting != null)
        {
            crafting.bandages = slot.bandages;
            crafting.herbPoultice = slot.herbPoultice;
            crafting.medicine = slot.medicine;
            crafting.torch = slot.torch;
            crafting.rope = slot.rope;
            crafting.boneBlade = slot.boneBlade;
            crafting.boneArmor = slot.boneArmor;
            crafting.huntersBow = slot.huntersBow;
            crafting.forestCloak = slot.forestCloak;
            crafting.healingFlute = slot.healingFlute;
            crafting.essenceAmulet = slot.essenceAmulet;
            crafting.ancientRelic = slot.ancientRelic;
        }

        // === 手账 ===
        var journal = FindObjectOfType<JournalSystem>();
        if (journal != null)
        {
            journal.memoryFragments = slot.memoryFragments;
        }

        // === UI 更新 ===
        gm.UpdateAllUI();
    }

    // ====================
    // 槽位管理
    // ====================

    void LoadSlotInfo()
    {
        for (int i = 0; i < MAX_SLOTS; i++)
        {
            string path = GetSlotPath(i);
            if (File.Exists(path))
            {
                try
                {
                    string encrypted = File.ReadAllText(path);
                    string json = SimpleDecrypt(encrypted);
                    saveSlots[i] = JsonUtility.FromJson<SaveSlot>(json);
                }
                catch
                {
                    saveSlots[i] = null;
                }
            }
        }
    }

    string GetSlotPath(int slot) => SAVE_FOLDER + SAVE_FILE_PREFIX + slot + SAVE_FILE_EXT;

    /// <summary>
    /// 获取槽位预览信息
    /// </summary>
    public (bool exists, string day, string time) GetSlotPreview(int slot)
    {
        if (slot < 0 || slot >= MAX_SLOTS) return (false, "", "");
        var s = saveSlots[slot];
        if (s == null) return (false, "", "");
        return (true, $"Day {s.gameDay}", s.saveTime);
    }

    // ====================
    // 简单加密（防止乱改）
    // ====================

    string SimpleEncrypt(string input)
    {
        byte[] data = Encoding.UTF8.GetBytes(input);
        byte[] key = System.Text.Encoding.UTF8.GetBytes("ForestJournalKey2026");
        for (int i = 0; i < data.Length; i++)
            data[i] ^= key[i % key.Length];
        return System.Convert.ToBase64String(data);
    }

    string SimpleDecrypt(string input)
    {
        byte[] data = System.Convert.FromBase64String(input);
        byte[] key = System.Text.Encoding.UTF8.GetBytes("ForestJournalKey2026");
        for (int i = 0; i < data.Length; i++)
            data[i] ^= key[i % key.Length];
        return Encoding.UTF8.GetString(data);
    }

    // ====================
    // 数据类
    // ====================

    [System.Serializable]
    public class SaveSlot
    {
        public string version = "4.2";
        public string saveTime = "";

        // 核心状态
        public int gameDay = 1;
        public int gamePhase = 0;
        public int currentRegion = 0;
        public int storyPhase = 1;
        public string gameState = "normal";

        // 资源
        public int food, wood, stone, herb, fiber, ore, bone, soulEssence;
        public int memories;
        public int threatLevel;

        // 队伍
        public List<SquadMemberSave> squadMembers;

        // 区域
        public List<int> discoveredRegions;

        // 手账
        public int journalEntryCount;
        public int memoryFragments;

        // NPC关系
        public List<NPCRelationshipSave> npcRelationships;

        // 技能
        public int campfireCount;
        public bool hasFireMastery, hasForaging, hasHerbalism, hasHunting, hasTracking;

        // 制作产物
        public int bandages, herbPoultice, medicine, torch, rope;
        public int boneBlade, boneArmor, huntersBow, forestCloak;
        public int healingFlute, essenceAmulet, ancientRelic;
    }

    [System.Serializable]
    public class SquadMemberSave
    {
        public string memberId, memberName, role;
        public int maxHealth, currentHealth;
        public int attack, defense, intelligence, agility;
        public string status;
    }

    [System.Serializable]
    public class NPCRelationshipSave
    {
        public string npcId;
        public int trustLevel;
        public int timesMet;
        public string attitude;
        public List<string> unlockedSecrets;
    }
}
