using System;
using System.Collections.Generic;
using System.Linq;
#if STEAMWORKS
using Steamworks;
#endif

namespace ForestJournal
{
    // ========================
    // 成就数据结构
    // ========================
    [Serializable]
    public class Achievement
    {
        public string id;              // 唯一标识
        public string name;            // 显示名称
        public string description;     // 描述
        public string iconCategory;    // 图标分类：story/collect/combat/explore/npc/special
        public string unlockCondition; // 解锁条件描述
        public bool isSecret;          // 是否为秘密成就
        public bool hiddenUntilUnlock; // 解锁前隐藏描述
        public bool isUnlocked;        // 是否已解锁
        public DateTime? unlockTime;   // 解锁时间

        public Achievement(string id, string name, string description, 
                          string iconCategory, string unlockCondition,
                          bool isSecret = false, bool hiddenUntilUnlock = true)
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.iconCategory = iconCategory;
            this.unlockCondition = unlockCondition;
            this.isSecret = isSecret;
            this.hiddenUntilUnlock = hiddenUntilUnlock;
            this.isUnlocked = false;
            this.unlockTime = null;
        }
    }

    // ========================
    // 成就统计（用于进度追踪）
    // ========================
    [Serializable]
    public class AchievementStats
    {
        public int totalKills;
        public int bossTypesDefeated;
        public int loreCollected;
        public int enemyTypesDiscovered;
        public int plantTypesDiscovered;
        public int recipesMade;
        public int regionsExplored;
        public int hiddenAreasFound;
        public int campCount;
        public int npcFriendsCount;
        public int maxNpcAffinity;
        public int npcRecruitedCount;
        public int totalAffinityGain;
        public int giftsGiven;
        public int ancientRelicPieces;
        public int endingsAchieved;
        public int currentDay;
        public bool noDamageToday;
        public int floorPassedWithoutDamage;
    }

    // ========================
    // 成就系统
    // ========================
    public class SteamAchievementSystem
    {
        private static SteamAchievementSystem _instance;
        public static SteamAchievementSystem Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SteamAchievementSystem();
                return _instance;
            }
        }

        private Dictionary<string, Achievement> achievements;
        private AchievementStats stats;
        private bool isSteamAvailable;
        private Action<Achievement> onAchievementUnlocked;

        // 分类颜色/图标映射
        private static readonly Dictionary<string, string> CategoryIcons = new Dictionary<string, string>
        {
            { "story", "📖" },
            { "collect", "📚" },
            { "combat", "⚔️" },
            { "explore", "🗺️" },
            { "npc", "💚" },
            { "special", "⭐" }
        };

        private SteamAchievementSystem()
        {
            achievements = new Dictionary<string, Achievement>();
            stats = new AchievementStats();
            isSteamAvailable = false;
            InitializeAchievements();
            CheckSteamAvailability();
        }

        // ========================
        // 初始化
        // ========================
        private void InitializeAchievements()
        {
            // ========================
            // 剧情成就（10个）
            // ========================
            AddAchievement(new Achievement("first_steps", "初入森林", "完成第一天",
                "story", "完成第一天", false, false));

            AddAchievement(new Achievement("chapter1", "第一章完成", "完成第一章故事",
                "story", "完成第一章"));

            AddAchievement(new Achievement("chapter2", "第二章完成", "完成第二章故事",
                "story", "完成第二章"));

            AddAchievement(new Achievement("chapter3", "第三章完成", "完成第三章故事",
                "story", "完成第三章"));

            AddAchievement(new Achievement("chapter4", "第四章完成", "完成第四章故事",
                "story", "完成第四章"));

            AddAchievement(new Achievement("ending_merge", "融入森林", "达成完美结局",
                "story", "达成完美结局", false, false));

            AddAchievement(new Achievement("ending_dispel", "驱散黑雾", "达成好结局",
                "story", "达成好结局", false, false));

            AddAchievement(new Achievement("ending_leave", "带走秘密", "达成普通结局",
                "story", "达成普通结局", false, false));

            AddAchievement(new Achievement("ending_forever", "永远留下", "达成悲剧结局",
                "story", "达成悲剧结局", false, false));

            AddAchievement(new Achievement("all_endings", "全知全能", "达成全部10种结局",
                "story", "达成全部10种结局", true));

            // ========================
            // 收集成就（8个）
            // ========================
            AddAchievement(new Achievement("collect_50_lore", "学者", "收集50条Lore",
                "collect", "收集50条Lore"));

            AddAchievement(new Achievement("collect_100_lore", "博学者", "收集100条Lore",
                "collect", "收集100条Lore"));

            AddAchievement(new Achievement("all_enemies", "图鉴完成", "收集全部18种敌人图鉴",
                "collect", "收集全部敌人图鉴", true));

            AddAchievement(new Achievement("all_plants", "植物学家", "收集全部25种植物图鉴",
                "collect", "收集全部植物图鉴", true));

            AddAchievement(new Achievement("all_items", "收藏家", "收集全部物品Lore",
                "collect", "收集全部物品", true));

            AddAchievement(new Achievement("all_recipes", "全知之书", "制作过全部35个配方",
                "collect", "制作全部配方", true));

            AddAchievement(new Achievement("all_set", "套装大师", "集齐任意一套套装",
                "collect", "集齐一套套装"));

            AddAchievement(new Achievement("ancient_relic", "古物收集者", "集齐古物碎片",
                "collect", "集齐古物碎片", true));

            // ========================
            // 战斗成就（7个）
            // ========================
            AddAchievement(new Achievement("kill_100", "战士之路", "累计击杀100只敌人",
                "combat", "累计击杀100只敌人"));

            AddAchievement(new Achievement("kill_500", "老兵", "累计击杀500只敌人",
                "combat", "累计击杀500只敌人"));

            AddAchievement(new Achievement("kill_1000", "传说猎手", "累计击杀1000只敌人",
                "combat", "累计击杀1000只敌人", true));

            AddAchievement(new Achievement("kill_boss_5", "Boss克星", "击败5种不同Boss",
                "combat", "击败5种不同Boss"));

            AddAchievement(new Achievement("no_damage_floor", "完美通过", "某天无伤通过",
                "combat", "无伤通过某天", true));

            AddAchievement(new Achievement("defeat_fog_lord", "屠雾者", "击败雾主",
                "combat", "击败雾主", true));

            AddAchievement(new Achievement("defeat_guardian", "守护者之敌", "击败森林守护者",
                "combat", "击败森林守护者", true));

            // ========================
            // 探索成就（5个）
            // ========================
            AddAchievement(new Achievement("all_regions", "足迹", "探索全部5个区域",
                "explore", "探索全部5个区域"));

            AddAchievement(new Achievement("hidden_10", "探索者", "发现10个隐藏区域",
                "explore", "发现10个隐藏区域"));

            AddAchievement(new Achievement("all_hidden", "冒险王", "发现全部20个隐藏区域",
                "explore", "发现全部隐藏区域", true));

            AddAchievement(new Achievement("first_camp", "安营", "首次安营",
                "explore", "首次安营", false, false));

            AddAchievement(new Achievement("camp_50", "流浪者", "安营50次",
                "explore", "安营50次"));

            // ========================
            // NPC成就（5个）
            // ========================
            AddAchievement(new Achievement("npc_friend_5", "广交善缘", "与5个NPC建立友谊",
                "npc", "与5个NPC建立友谊"));

            AddAchievement(new Achievement("npc_max", "挚友", "任意NPC好感度达到100",
                "npc", "NPC好感度达到100"));

            AddAchievement(new Achievement("recruit_all", "招募者", "招募全部5个NPC",
                "npc", "招募全部5个NPC", true));

            AddAchievement(new Achievement("npc_trust_100", "信任无间", "NPC好感度累计增加100点",
                "npc", "好感度累计增加100点"));

            AddAchievement(new Achievement("npc_gift_20", "礼物达人", "赠送NPC礼物20次",
                "npc", "赠送NPC礼物20次"));
        }

        private void AddAchievement(Achievement ach)
        {
            achievements[ach.id] = ach;
        }

        // ========================
        // Steam检测
        // ========================
        private void CheckSteamAvailability()
        {
#if STEAMWORKS
            try
            {
                isSteamAvailable = SteamClient.IsValid;
                UnityEngine.Debug.Log($"[SteamAchievement] Steam available: {isSteamAvailable}");
            }
            catch (Exception e)
            {
                isSteamAvailable = false;
                UnityEngine.Debug.Log($"[SteamAchievement] Steam not available: {e.Message}");
            }
#else
            isSteamAvailable = false;
            UnityEngine.Debug.Log("[SteamAchievement] SteamWorks not integrated, running in standalone mode");
#endif
        }

        // ========================
        // 注册回调
        // ========================
        public void RegisterUnlockCallback(Action<Achievement> callback)
        {
            onAchievementUnlocked += callback;
        }

        // ========================
        // 解锁成就（核心方法）
        // ========================
        public void UnlockAchievement(string achievementId)
        {
            if (!achievements.ContainsKey(achievementId))
            {
                UnityEngine.Debug.LogWarning($"[SteamAchievement] Unknown achievement: {achievementId}");
                return;
            }

            var ach = achievements[achievementId];
            if (ach.isUnlocked)
            {
                return; // 已解锁，不重复处理
            }

            ach.isUnlocked = true;
            ach.unlockTime = DateTime.Now;

            UnityEngine.Debug.Log($"[SteamAchievement] UNLOCKED: {ach.name} ({ach.id})");

            // Steam上传
            if (isSteamAvailable)
            {
                UploadToSteam(achievementId);
            }

            // 触发回调
            onAchievementUnlocked?.Invoke(ach);
        }

        private void UploadToSteam(string achievementId)
        {
#if STEAMWORKS
            try
            {
                SteamUserStats.SetAchievement(achievementId);
                SteamUserStats.StoreStats();
                UnityEngine.Debug.Log($"[SteamAchievement] Uploaded to Steam: {achievementId}");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[SteamAchievement] Failed to upload to Steam: {e.Message}");
            }
#endif
        }

        // ========================
        // 进度检查接口（供游戏事件调用）
        // ========================

        /// 击杀敌人时调用
        public void OnEnemyKilled(bool isBoss = false, string bossType = null)
        {
            stats.totalKills++;

            if (isBoss && !string.IsNullOrEmpty(bossType))
            {
                // 记录Boss类型（需要外部传入）
            }

            CheckKillAchievements();
        }

        private void CheckKillAchievements()
        {
            if (stats.totalKills >= 100) UnlockAchievement("kill_100");
            if (stats.totalKills >= 500) UnlockAchievement("kill_500");
            if (stats.totalKills >= 1000) UnlockAchievement("kill_1000");
        }

        /// 击败特定Boss时调用
        public void OnBossDefeated(string bossId)
        {
            stats.bossTypesDefeated++;

            if (stats.bossTypesDefeated >= 5) UnlockAchievement("kill_boss_5");

            if (bossId == "fog_lord") UnlockAchievement("defeat_fog_lord");
            if (bossId == "guardian") UnlockAchievement("defeat_guardian");
        }

        /// 无伤通过某天时调用
        public void OnFloorPassedNoDamage(int day)
        {
            stats.floorPassedWithoutDamage++;
            if (stats.floorPassedWithoutDamage >= 1) UnlockAchievement("no_damage_floor");
        }

        /// 收集Lore时调用
        public void OnLoreCollected(int totalLore)
        {
            stats.loreCollected = totalLore;

            if (stats.loreCollected >= 50) UnlockAchievement("collect_50_lore");
            if (stats.loreCollected >= 100) UnlockAchievement("collect_100_lore");
        }

        /// 发现敌人图鉴时调用
        public void OnEnemyTypeDiscovered(int totalTypes)
        {
            stats.enemyTypesDiscovered = totalTypes;

            if (stats.enemyTypesDiscovered >= 18) UnlockAchievement("all_enemies");
        }

        /// 发现植物图鉴时调用
        public void OnPlantTypeDiscovered(int totalTypes)
        {
            stats.plantTypesDiscovered = totalTypes;

            if (stats.plantTypesDiscovered >= 25) UnlockAchievement("all_plants");
        }

        /// 制作配方时调用
        public void OnRecipeMade(int totalRecipes)
        {
            stats.recipesMade = totalRecipes;

            if (stats.recipesMade >= 35) UnlockAchievement("all_recipes");
        }

        /// 收集物品时调用
        public void OnItemCollected(int totalItems)
        {
            // 需要外部判断是否收集完全部物品
        }

        /// 集齐套装时调用
        public void OnSetCollected()
        {
            UnlockAchievement("all_set");
        }

        /// 收集古物碎片时调用
        public void OnAncientRelicPieceCollected(int totalPieces)
        {
            stats.ancientRelicPieces = totalPieces;

            if (stats.ancientRelicPieces >= 8) UnlockAchievement("ancient_relic");
        }

        /// 探索区域时调用
        public void OnRegionExplored(int totalRegions)
        {
            stats.regionsExplored = totalRegions;

            if (stats.regionsExplored >= 5) UnlockAchievement("all_regions");
        }

        /// 发现隐藏区域时调用
        public void OnHiddenAreaFound(int totalHidden)
        {
            stats.hiddenAreasFound = totalHidden;

            if (stats.hiddenAreasFound >= 10) UnlockAchievement("hidden_10");
            if (stats.hiddenAreasFound >= 20) UnlockAchievement("all_hidden");
        }

        /// 安营时调用
        public void OnCampSet()
        {
            stats.campCount++;

            if (stats.campCount >= 1) UnlockAchievement("first_camp");
            if (stats.campCount >= 50) UnlockAchievement("camp_50");
        }

        /// 完成天数时调用
        public void OnDayComplete(int day)
        {
            stats.currentDay = day;

            if (day >= 1) UnlockAchievement("first_steps");
        }

        /// 章节完成时调用
        public void OnChapterComplete(int chapter)
        {
            switch (chapter)
            {
                case 1: UnlockAchievement("chapter1"); break;
                case 2: UnlockAchievement("chapter2"); break;
                case 3: UnlockAchievement("chapter3"); break;
                case 4: UnlockAchievement("chapter4"); break;
            }
        }

        /// 达成结局时调用
        public void OnEndingAchieved(string endingId, int totalEndings)
        {
            stats.endingsAchieved = totalEndings;

            switch (endingId)
            {
                case "merge": UnlockAchievement("ending_merge"); break;
                case "dispel": UnlockAchievement("ending_dispel"); break;
                case "leave": UnlockAchievement("ending_leave"); break;
                case "forever": UnlockAchievement("ending_forever"); break;
            }

            if (stats.endingsAchieved >= 10) UnlockAchievement("all_endings");
        }

        /// NPC好感度变化时调用
        public void OnNpcAffinityChanged(int change)
        {
            stats.totalAffinityGain += Mathf.Abs(change);

            if (stats.totalAffinityGain >= 100) UnlockAchievement("npc_trust_100");
        }

        /// NPC好感度达到最大值时调用
        public void OnNpcMaxAffinity(int affinity)
        {
            if (affinity >= 100)
            {
                stats.maxNpcAffinity = Mathf.Max(stats.maxNpcAffinity, affinity);
                UnlockAchievement("npc_max");
            }
        }

        /// 与NPC建立友谊时调用
        public void OnNpcFriendshipEstablished(int totalFriends)
        {
            stats.npcFriendsCount = totalFriends;

            if (stats.npcFriendsCount >= 5) UnlockAchievement("npc_friend_5");
        }

        /// 招募NPC时调用
        public void OnNpcRecruited(int totalRecruited)
        {
            stats.npcRecruitedCount = totalRecruited;

            if (stats.npcRecruitedCount >= 5) UnlockAchievement("recruit_all");
        }

        /// 赠送礼物时调用
        public void OnGiftGiven()
        {
            stats.giftsGiven++;

            if (stats.giftsGiven >= 20) UnlockAchievement("npc_gift_20");
        }

        // ========================
        // 查询接口
        // ========================

        public List<Achievement> GetAllAchievements()
        {
            return achievements.Values.ToList();
        }

        public List<Achievement> GetAchievementsByCategory(string category)
        {
            return achievements.Values.Where(a => a.iconCategory == category).ToList();
        }

        public List<Achievement> GetUnlockedAchievements()
        {
            return achievements.Values.Where(a => a.isUnlocked).ToList();
        }

        public List<Achievement> GetLockedAchievements()
        {
            return achievements.Values.Where(a => !a.isUnlocked).ToList();
        }

        public Achievement GetAchievement(string id)
        {
            return achievements.ContainsKey(id) ? achievements[id] : null;
        }

        public int GetUnlockedCount()
        {
            return achievements.Values.Count(a => a.isUnlocked);
        }

        public int GetTotalCount()
        {
            return achievements.Count;
        }

        public float GetProgress()
        {
            if (achievements.Count == 0) return 0f;
            return (float)GetUnlockedCount() / achievements.Count;
        }

        public AchievementStats GetStats()
        {
            return stats;
        }

        // ========================
        // 存档/读档
        // ========================
        [Serializable]
        public class SaveData
        {
            public List<string> unlockedIds;
            public Dictionary<string, DateTime> unlockTimes;
            public AchievementStats stats;
        }

        public SaveData GetSaveData()
        {
            var data = new SaveData();
            data.unlockedIds = achievements.Values
                .Where(a => a.isUnlocked)
                .Select(a => a.id)
                .ToList();
            data.unlockTimes = achievements.Values
                .Where(a => a.isUnlocked && a.unlockTime.HasValue)
                .ToDictionary(a => a.id, a => a.unlockTime.Value);
            data.stats = stats;
            return data;
        }

        public void LoadFromSaveData(SaveData data)
        {
            if (data == null) return;

            // 重置所有成就
            foreach (var ach in achievements.Values)
            {
                ach.isUnlocked = false;
                ach.unlockTime = null;
            }

            // 恢复已解锁成就
            foreach (var id in data.unlockedIds)
            {
                if (achievements.ContainsKey(id))
                {
                    achievements[id].isUnlocked = true;
                    if (data.unlockTimes.ContainsKey(id))
                    {
                        achievements[id].unlockTime = data.unlockTimes[id];
                    }
                }
            }

            // 恢复统计
            if (data.stats != null)
            {
                stats = data.stats;
            }
        }

        // ========================
        // UI渲染数据
        // ========================
        public string GetCategoryIcon(string category)
        {
            return CategoryIcons.ContainsKey(category) ? CategoryIcons[category] : "🏆";
        }

        public Dictionary<string, List<Achievement>> GetAchievementsGroupedByCategory()
        {
            var grouped = new Dictionary<string, List<Achievement>>();
            foreach (var ach in achievements.Values)
            {
                if (!grouped.ContainsKey(ach.iconCategory))
                {
                    grouped[ach.iconCategory] = new List<Achievement>();
                }
                grouped[ach.iconCategory].Add(ach);
            }
            return grouped;
        }
    }

    // ========================
    // Unity MonoBehaviour包装器
    // ========================
    public class SteamAchievementManager : UnityEngine.MonoBehaviour
    {
        private SteamAchievementSystem system;

        private void Awake()
        {
            system = SteamAchievementSystem.Instance;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            // 注册UI回调
            system.RegisterUnlockCallback(OnAchievementUnlocked);
        }

        private void OnDisable()
        {
            // 注销回调
        }

        private void OnAchievementUnlocked(Achievement ach)
        {
            // 可以在这里触发UI通知、音效等
            UnityEngine.Debug.Log($"[UI] Achievement Unlocked: {ach.name}");
        }

        // 供GameManager调用的便捷方法
        public void ReportEnemyKilled(bool isBoss = false, string bossType = null)
        {
            system.OnEnemyKilled(isBoss, bossType);
        }

        public void ReportBossDefeated(string bossId)
        {
            system.OnBossDefeated(bossId);
        }

        public void ReportLoreCollected(int total)
        {
            system.OnLoreCollected(total);
        }

        public void ReportDayComplete(int day)
        {
            system.OnDayComplete(day);
        }

        public void ReportChapterComplete(int chapter)
        {
            system.OnChapterComplete(chapter);
        }

        public void ReportEndingAchieved(string endingId, int totalEndings)
        {
            system.OnEndingAchieved(endingId, totalEndings);
        }

        public void ReportNpcAffinityChanged(int change)
        {
            system.OnNpcAffinityChanged(change);
        }

        public void ReportGiftGiven()
        {
            system.OnGiftGiven();
        }

        public void ReportCampSet()
        {
            system.OnCampSet();
        }

        public void ReportRegionExplored(int total)
        {
            system.OnRegionExplored(total);
        }

        public void ReportHiddenAreaFound(int total)
        {
            system.OnHiddenAreaFound(total);
        }
    }
}
