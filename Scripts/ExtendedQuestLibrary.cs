using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 扩展任务库 - 70+ 任务
/// 悬赏/委托/事件/连续/roguelike每日/隐藏任务
/// </summary>
public static class ExtendedQuestLibrary
{
    public static List<QuestData> GenerateAllQuests()
    {
        return new List<QuestData>
        {
            // ===== 悬赏 T1 =====
            new QuestData { id="bounty_wolf_01", name="狼群威胁", type=QuestType.Bounty, difficulty=1,
                description="消灭5只阴影狼",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.KillEnemy, targetId="shadow_wolf", count=5} },
                rewards=new Reward{ food=10, soulEssence=1, memories=1 }
            },
            new QuestData { id="bounty_rat_01", name="鼠患泛滥", type=QuestType.Bounty, difficulty=1,
                description="消灭8只沼泽鼠",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.KillEnemy, targetId="swamp_rat", count=8} },
                rewards=new Reward{ food=8, memories=1 }
            },
            new QuestData { id="bounty_bat_01", name="蝙蝠洞穴", type=QuestType.Bounty, difficulty=1,
                description="赶走6只洞穴蝙蝠",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.KillEnemy, targetId="cave_bat", count=6} },
                rewards=new Reward{ food=6, herb=4, memories=1 }
            },
            // ===== 悬赏 T2 =====
            new QuestData { id="bounty_spider_01", name="毒蜘蛛巢穴", type=QuestType.Bounty, difficulty=2,
                description="击杀4只毒蜘蛛",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.KillEnemy, targetId="poison_spider", count=4} },
                rewards=new Reward{ herb=8, memories=1 }
            },
            new QuestData { id="bounty_skeleton_01", name="骷髅巡逻", type=QuestType.Bounty, difficulty=2,
                description="消灭3只骷髅战士",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.KillEnemy, targetId="skeleton_warrior", count=3} },
                rewards=new Reward{ food=5, bone=8, memories=2 }
            },
            new QuestData { id="bounty_beetle_01", name="甲虫入侵", type=QuestType.Bounty, difficulty=2,
                description="消灭5只森林甲虫",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.KillEnemy, targetId="forest_beetle", count=5} },
                rewards=new Reward{ herb=6, wood=4, memories=1 }
            },
            new QuestData { id="bounty_mist_elf_01", name="雾精灵骚扰", type=QuestType.Bounty, difficulty=2,
                description="消灭3只雾精灵",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.KillEnemy, targetId="mist_elf", count=3} },
                rewards=new Reward{ soulEssence=2, memories=2 }
            },
            // ===== 悬赏 T3 =====
            new QuestData { id="bounty_fog_beast_01", name="黑雾兽清剿", type=QuestType.Bounty, difficulty=3,
                description="击杀1头黑雾兽",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.KillEnemy, targetId="darkbeast", count=1} },
                rewards=new Reward{ soulEssence=5, memories=3 }
            },
            new QuestData { id="bounty_mutant_tree_01", name="变异树攻击", type=QuestType.Bounty, difficulty=3,
                description="消灭4只变异树",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.KillEnemy, targetId="mutant_tree", count=4} },
                rewards=new Reward{ wood=15, memories=2 }
            },
            new QuestData { id="bounty_specter_01", name="幽灵徘徊", type=QuestType.Bounty, difficulty=3,
                description="消灭3只远古幽灵",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.KillEnemy, targetId="ancient_specter", count=3} },
                rewards=new Reward{ soulEssence=3, memories=3 }
            },
            new QuestData { id="bounty_golem_01", name="石头傀儡", type=QuestType.Bounty, difficulty=3,
                description="消灭2个石头傀儡",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.KillEnemy, targetId="stone_golem", count=2} },
                rewards=new Reward{ stone=20, memories=2 }
            },
            // ===== 悬赏 T4-5 =====
            new QuestData { id="bounty_frog_king", name="沼泽蛙王", type=QuestType.Bounty, difficulty=4,
                description="击败沼泽蛙王",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.KillEnemy, targetId="swamp_frog_king", count=1} },
                rewards=new Reward{ food=15, herb=10, memories=3 }
            },
            new QuestData { id="bounty_lizard_01", name="岩石蜥蜴群", type=QuestType.Bounty, difficulty=4,
                description="消灭4只岩石蜥蜴",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.KillEnemy, targetId="rock_lizard", count=4} },
                rewards=new Reward{ ore=15, memories=3 }
            },
            new QuestData { id="bounty_fog_lord_01", name="雾主讨伐", type=QuestType.Bounty, difficulty=5,
                description="击败自称雾主的强敌",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.KillEnemy, targetId="fog_lord", count=1} },
                rewards=new Reward{ soulEssence=10, memories=5 }
            },
            new QuestData { id="bounty_guardian_01", name="森林守护者", type=QuestType.Bounty, difficulty=5,
                description="击败森林守护者",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.KillEnemy, targetId="forest_guardian", count=1} },
                rewards=new Reward{ memories=8, soulEssence=8 }
            },
            // ===== 委托任务 =====
            new QuestData { id="errand_food_15", name="粮食危机", type=QuestType.Errand, difficulty=1,
                description="采集15个食物",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.GatherResource, targetId="food", count=15} },
                rewards=new Reward{ memories=1 }
            },
            new QuestData { id="errand_food_8", name="旅人路费", type=QuestType.Errand, difficulty=1,
                description="带8个食物给旅人",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.GatherResource, targetId="food", count=8} },
                rewards=new Reward{ memories=1, soulEssence=1 }
            },
            new QuestData { id="errand_herb_10", name="草药补给", type=QuestType.Errand, difficulty=1,
                description="带10个草药给玛莎",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.GatherResource, targetId="herb", count=10} },
                rewards=new Reward{ herb=5, memories=1 }
            },
            new QuestData { id="errand_wood_5", name="木材收集", type=QuestType.Errand, difficulty=1,
                description="采集5个木材",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.GatherResource, targetId="wood", count=5} },
                rewards=new Reward{ wood=5, memories=1 }
            },
            new QuestData { id="errand_stone_12", name="石材需求", type=QuestType.Errand, difficulty=1,
                description="采集12个石材",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.GatherResource, targetId="stone", count=12} },
                rewards=new Reward{ stone=12, memories=1 }
            },
            new QuestData { id="errand_fiber_20", name="纤维采集", type=QuestType.Errand, difficulty=1,
                description="采集20个纤维",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.GatherResource, targetId="fiber", count=20} },
                rewards=new Reward{ memories=1 }
            },
            new QuestData { id="errand_bone_10", name="骨骼收集", type=QuestType.Errand, difficulty=2,
                description="采集10个骨骼",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.GatherResource, targetId="bone", count=10} },
                rewards=new Reward{ memories=2 }
            },
            new QuestData { id="errand_ore_15", name="矿石勘探", type=QuestType.Errand, difficulty=3,
                description="采集15个矿石",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.GatherResource, targetId="ore", count=15} },
                rewards=new Reward{ memories=3, stone=10 }
            },
            new QuestData { id="errand_bandage_8", name="绷带订单", type=QuestType.Errand, difficulty=2,
                description="制作8个绷带",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.CraftItem, targetId="bandage", count=8} },
                rewards=new Reward{ food=12, memories=1 }
            },
            new QuestData { id="errand_blade_1", name="武器订单", type=QuestType.Errand, difficulty=2,
                description="制作1把骨刀",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.CraftItem, targetId="bone_blade", count=1} },
                rewards=new Reward{ food=15, memories=2 }
            },
            new QuestData { id="errand_torch_4", name="火把订单", type=QuestType.Errand, difficulty=1,
                description="制作4个火把",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.CraftItem, targetId="torch", count=4} },
                rewards=new Reward{ memories=1 }
            },
            new QuestData { id="errand_explore_valley", name="幽暗山谷", type=QuestType.Errand, difficulty=2,
                description="探索幽暗山谷",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.ExploreRegion, targetId="dark_valley", count=1} },
                rewards=new Reward{ memories=2, herb=5 }
            },
            new QuestData { id="errand_explore_ruins", name="废墟调查", type=QuestType.Errand, difficulty=2,
                description="调查古老废墟",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.ExploreRegion, targetId="ancient_ruins", count=1} },
                rewards=new Reward{ food=15, bone=5, memories=2 }
            },
            new QuestData { id="errand_explore_fog", name="迷雾前沿", type=QuestType.Errand, difficulty=3,
                description="深入迷雾前沿",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.ExploreRegion, targetId="fog_front", count=1} },
                rewards=new Reward{ memories=3, soulEssence=2 }
            },
            // ===== 随机事件 =====
            new QuestData { id="event_child", name="迷路的孩子", type=QuestType.Event, difficulty=1,
                description="发现迷路的孩子",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="help", text="帮助孩子", reward=new Reward{ memories=2 } },
                    new QuestChoice{ id="ignore", text="假装没看见", reward=new Reward{ memories=-1 } }
                }
            },
            new QuestData { id="event_stranger", name="神秘旅人", type=QuestType.Event, difficulty=1,
                description="神秘旅人想交换情报",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="trade", text="用3食物换情报", reward=new Reward{ memories=3 }, cost=new Cost{ food=3 } },
                    new QuestChoice{ id="free", text="免费分享", reward=new Reward{ memories=2 } },
                    new QuestChoice{ id="refuse", text="拒绝", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_soldier", name="受伤士兵", type=QuestType.Event, difficulty=2,
                description="士兵被黑雾兽袭击",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="heal", text="用草药治疗", reward=new Reward{ memories=2, soulEssence=1 }, cost=new Cost{ herb=5 } },
                    new QuestChoice{ id="abandon", text="不管", reward=new Reward{ memories=-1 } }
                }
            },
            new QuestData { id="event_village", name="废弃村落", type=QuestType.Event, difficulty=2,
                description="废弃村落可能有物资",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="search", text="仔细搜索", reward=new Reward{ food=8, wood=5, stone=3 } },
                    new QuestChoice{ id="quick", text="快速搜刮", reward=new Reward{ food=3, wood=2 } },
                    new QuestChoice{ id="leave", text="不进去", reward=new Reward{ memories=1 } }
                }
            },
            new QuestData { id="event_elder", name="老者的智慧", type=QuestType.Event, difficulty=3,
                description="老者知道黑雾秘密",
                requirement=new Requirement{ minDay=10 },
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="visit", text="拜访老者", reward=new Reward{ memories=5, soulEssence=2 } },
                    new QuestChoice{ id="gift", text="带礼物拜访", reward=new Reward{ memories=3 }, cost=new Cost{ food=5 }, requirement=new Requirement{ minDay=5 } },
                    new QuestChoice{ id="skip", text="不感兴趣", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_merchant", name="流浪商人", type=QuestType.Event, difficulty=1,
                description="商人想交换物资",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="buy", text="买药水配方", reward=new Reward{ memories=1 }, cost=new Cost{ food=10 } },
                    new QuestChoice{ id="swap", text="5草药换魂精华", reward=new Reward{ soulEssence=3 }, cost=new Cost{ herb=5 } },
                    new QuestChoice{ id="no", text="不感兴趣", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_ghost", name="幽灵村落", type=QuestType.Event, difficulty=3,
                description="诡异村落有幽灵",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="exorcize", text="驱邪仪式", reward=new Reward{ memories=4, soulEssence=3 }, requirement=new Requirement{ minDay=7 } },
                    new QuestChoice{ id="look", text="调查村落", reward=new Reward{ memories=2, food=5 } },
                    new QuestChoice{ id="run", text="赶紧离开", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_shrine", name="古老祭坛", type=QuestType.Event, difficulty=2,
                description="古老祭坛可献祭",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="sac_food", text="献祭10食物", reward=new Reward{ memories=3, soulEssence=2 }, cost=new Cost{ food=10 } },
                    new QuestChoice{ id="sac_herb", text="献祭10草药", reward=new Reward{ memories=3, herb=5 }, cost=new Cost{ herb=10 } },
                    new QuestChoice{ id="sac_no", text="不献祭", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_beast", name="受伤野兽", type=QuestType.Event, difficulty=1,
                description="路边受伤野兽",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="heal_beast", text="治疗它", reward=new Reward{ memories=2, food=5 }, cost=new Cost{ herb=3 } },
                    new QuestChoice{ id="hunt_beast", text="趁机猎杀", reward=new Reward{ food=12 }, requirement=new Requirement{ minDay=3 } },
                    new QuestChoice{ id="spare_beast", text="放它走", reward=new Reward{ memories=1 } }
                }
            },
            new QuestData { id="event_star", name="陨落星辰", type=QuestType.Event, difficulty=2,
                description="流星留下发光碎片",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="collect", text="收集碎片", reward=new Reward{ memories=3, soulEssence=2 } },
                    new QuestChoice{ id="worship", text="就地祭拜", reward=new Reward{ memories=5 }, requirement=new Requirement{ minDay=10 } },
                    new QuestChoice{ id="ignore_star", text="不理会", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_prisoner", name="被困旅人", type=QuestType.Event, difficulty=2,
                description="旅人困在陷阱中",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="rescue", text="营救", reward=new Reward{ memories=3, food=8 } },
                    new QuestChoice{ id="note", text="记下位置", reward=new Reward{ memories=1 } },
                    new QuestChoice{ id="ignore_prisoner", text="不救", reward=new Reward{ memories=-1 } }
                }
            },
            new QuestData { id="event_flower", name="神秘花朵", type=QuestType.Event, difficulty=1,
                description="发现神秘花朵",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="pick", text="采摘", reward=new Reward{ herb=8, memories=1 } },
                    new QuestChoice{ id="smell", text="闻花香", reward=new Reward{ memories=2 } },
                    new QuestChoice{ id="leave_flower", text="不碰", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_egg", name="奇怪的蛋", type=QuestType.Event, difficulty=2,
                description="发现一颗巨大蛋",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="hatch", text="孵化", reward=new Reward{ memories=3 }, requirement=new Requirement{ minDay=5 } },
                    new QuestChoice{ id="eat", text="烤来吃", reward=new Reward{ food=15 } },
                    new QuestChoice{ id="leave_egg", text="不动", reward=new Reward{ memories=1 } }
                }
            },
            new QuestData { id="event_fairy", name="精灵圆环", type=QuestType.Event, difficulty=2,
                description="奇异蘑菇环",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="dance", text="在圈内跳舞", reward=new Reward{ memories=4, herb=5 }, requirement=new Requirement{ minDay=3 } },
                    new QuestChoice{ id="pick_mush", text="采摘蘑菇", reward=new Reward{ herb=10, memories=1 } },
                    new QuestChoice{ id="avoid", text="绕道走", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_map", name="残破地图", type=QuestType.Event, difficulty=1,
                description="发现残破地图",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="follow", text="按地图探索", reward=new Reward{ memories=2, food=5 } },
                    new QuestChoice{ id="keep", text="收好备用", reward=new Reward{ memories=1 } },
                    new QuestChoice{ id="ignore_map", text="不理会", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_trap", name="猎人陷阱", type=QuestType.Event, difficulty=1,
                description="陷阱里困着兔子",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="free_rabbit", text="放走", reward=new Reward{ memories=2 } },
                    new QuestChoice{ id="take_rabbit", text="拿走", reward=new Reward{ food=8 } },
                    new QuestChoice{ id="leave_trap", text="不动", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_water", name="甘甜泉水", type=QuestType.Event, difficulty=1,
                description="发现清泉",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="drink", text="饮用", reward=new Reward{ memories=1, food=3 } },
                    new QuestChoice{ id="collect_w", text="收集", reward=new Reward{ herb=5, memories=1 } },
                    new QuestChoice{ id="leave_w", text="不碰", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_crow", name="乌鸦的启示", type=QuestType.Event, difficulty=1,
                description="乌鸦似乎在说什么",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="listen", text="仔细听", reward=new Reward{ memories=2 } },
                    new QuestChoice{ id="chase_c", text="赶走", reward=new Reward{ memories=0 } },
                    new QuestChoice{ id="feed_c", text="喂食物", reward=new Reward{ memories=3 }, cost=new Cost{ food=3 } }
                }
            },
            // ===== 新增随机事件 21-50 =====
            new QuestData { id="event_fox", name="迷路的小狐狸", type=QuestType.Event, difficulty=1,
                description="一只小狐狸跟着你走",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="adopt", text="收留它", reward=new Reward{ memories=2, food=3 } },
                    new QuestChoice{ id="feed_fox", text="喂食后放走", reward=new Reward{ memories=3, soulEssence=1 } },
                    new QuestChoice{ id="shun_fox", text="驱赶它", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_beehive", name="倒塌的树", type=QuestType.Event, difficulty=2,
                description="倒下的树中露出蜂巢",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="harvest_honey", text="采集蜂蜜", reward=new Reward{ food=12, memories=2 }, requirement=new Requirement{ minDay=3 } },
                    new QuestChoice{ id="risk_honey", text="冒险采集", reward=new Reward{ food=20, memories=3 }, requirement=new Requirement{ minDay=5 } },
                    new QuestChoice{ id="leave_honey", text="不碰", reward=new Reward{ memories=1 } }
                }
            },
            new QuestData { id="event_well", name="古老的井", type=QuestType.Event, difficulty=1,
                description="古井旁有投币孔",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="toss_coin", text="投币占卜", reward=new Reward{ memories=3 }, cost=new Cost{ food=1 } },
                    new QuestChoice{ id="look_well", text="探头看", reward=new Reward{ memories=1 } },
                    new QuestChoice{ id="ignore_well", text="不理会", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_baby", name="被遗弃的婴儿", type=QuestType.Event, difficulty=2,
                description="灌木丛中传来婴儿哭声",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="raise_baby", text="抚养", reward=new Reward{ memories=4, soulEssence=2 }, requirement=new Requirement{ minDay=7 } },
                    new QuestChoice{ id="find_npc", text="交给NPC", reward=new Reward{ memories=3 } },
                    new QuestChoice{ id="leave_baby", text="留在原地", reward=new Reward{ memories=-1 } }
                }
            },
            new QuestData { id="event_rain_song", name="暴雨中的歌声", type=QuestType.Event, difficulty=2,
                description="雨夜传来凄美的歌声",
                requirement=new Requirement{ minDay=5 },
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="listen_song", text="倾听歌声", reward=new Reward{ memories=4, soulEssence=2 } },
                    new QuestChoice{ id="follow_song", text="循声寻找", reward=new Reward{ memories=2, food=5 }, requirement=new Requirement{ minDay=10 } },
                    new QuestChoice{ id="leave_song", text="冒雨离开", reward=new Reward{ memories=1 } }
                }
            },
            new QuestData { id="event_blood", name="地上的血迹", type=QuestType.Event, difficulty=2,
                description="草叶上留有新鲜血迹",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="track_blood", text="跟踪血迹", reward=new Reward{ memories=3, soulEssence=1 } },
                    new QuestChoice{ id="cautious_blood", text="谨慎侦察", reward=new Reward{ memories=2 } },
                    new QuestChoice{ id="ignore_blood", text="绕路走", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_stone", name="奇怪的石碑", type=QuestType.Event, difficulty=3,
                description="石碑上刻着无法辨认的文字",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="touch_stone", text="触碰石碑", reward=new Reward{ memories=5 }, requirement=new Requirement{ minDay=7 } },
                    new QuestChoice{ id="study_stone", text="仔细研究", reward=new Reward{ memories=3, soulEssence=1 } },
                    new QuestChoice{ id="avoid_stone", text="不碰", reward=new Reward{ memories=1 } }
                }
            },
            new QuestData { id="event_birds", name="鸟群惊飞", type=QuestType.Event, difficulty=1,
                description="树梢的鸟群突然惊飞",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="investigate_birds", text="调查原因", reward=new Reward{ memories=2, food=5 } },
                    new QuestChoice{ id="hide_birds", text="隐蔽观察", reward=new Reward{ memories=1 } },
                    new QuestChoice{ id="pass_birds", text="继续前进", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_firefly", name="萤火虫群", type=QuestType.Event, difficulty=1,
                description="点点萤光在林间飘动",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="follow_firefly", text="跟随萤火虫", reward=new Reward{ memories=3, herb=5 } },
                    new QuestChoice{ id="catch_firefly", text="捕捉", reward=new Reward{ memories=1, soulEssence=1 } },
                    new QuestChoice{ id="watch_firefly", text="静静观赏", reward=new Reward{ memories=2 } }
                }
            },
            new QuestData { id="event_rainbow", name="彩虹", type=QuestType.Event, difficulty=1,
                description="雨后彩虹横跨森林",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="chase_rainbow", text="追寻彩虹尽头", reward=new Reward{ memories=4, food=8 }, requirement=new Requirement{ minDay=5 } },
                    new QuestChoice{ id="photo_rainbow", text="拍照留念", reward=new Reward{ memories=2 } },
                    new QuestChoice{ id="ignore_rainbow", text="不理会", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_signpost", name="倒下的路牌", type=QuestType.Event, difficulty=1,
                description="路牌倒伏在路边",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="repair_sign", text="修复路牌", reward=new Reward{ memories=2, wood=5 } },
                    new QuestChoice{ id="read_sign", text="辨认方向", reward=new Reward{ memories=2 } },
                    new QuestChoice{ id="leave_sign", text="不碰", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_picnic", name="野餐篮", type=QuestType.Event, difficulty=1,
                description="草丛中有一个被遗忘的野餐篮",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="open_picnic", text="打开查看", reward=new Reward{ food=10, memories=2 } },
                    new QuestChoice{ id="take_photo", text="拿走照片", reward=new Reward{ memories=3 } },
                    new QuestChoice{ id="ignore_picnic", text="不碰", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_cricket", name="唱歌的蟋蟀", type=QuestType.Event, difficulty=2,
                description="草丛中蟋蟀鸣叫不止",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="listen_cricket", text="倾听蟋蟀", reward=new Reward{ memories=3, soulEssence=1 } },
                    new QuestChoice{ id="catch_cricket", text="捕捉研究", reward=new Reward{ memories=2 } },
                    new QuestChoice{ id="disturb_cricket", text="打扰它", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_vine_door", name="被藤蔓缠绕的门", type=QuestType.Event, difficulty=2,
                description="藤蔓遮蔽着一扇旧木门",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="clear_vine", text="清理藤蔓进入", reward=new Reward{ memories=4, food=8 }, requirement=new Requirement{ minDay=5 } },
                    new QuestChoice{ id="peer_door", text="从门缝窥视", reward=new Reward{ memories=2 } },
                    new QuestChoice{ id="ignore_door", text="不进去", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_footprint", name="奇怪的脚印", type=QuestType.Event, difficulty=2,
                description="地上有不明生物的脚印",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="follow_foot", text="跟随脚印", reward=new Reward{ memories=3, soulEssence=2 } },
                    new QuestChoice{ id="study_foot", text="研究脚印", reward=new Reward{ memories=2 } },
                    new QuestChoice{ id="avoid_foot", text="绕开走", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_branch", name="落水的树枝", type=QuestType.Event, difficulty=1,
                description="溪流中卡着一根大树枝",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="fish_branch", text="打捞树枝", reward=new Reward{ wood=8, memories=1 } },
                    new QuestChoice{ id="search_branch", text="检查树枝", reward=new Reward{ memories=2 } },
                    new QuestChoice{ id="ignore_branch", text="不理会", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_campfire", name="远处火光", type=QuestType.Event, difficulty=2,
                description="密林深处有火光闪烁",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="approach_fire", text="靠近查看", reward=new Reward{ memories=3, food=5 } },
                    new QuestChoice{ id="observe_fire", text="远距离观察", reward=new Reward{ memories=2 } },
                    new QuestChoice{ id="avoid_fire", text="避开火光", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_map_piece", name="遗失的地图碎片", type=QuestType.Event, difficulty=1,
                description="泥水中泡着一张残破地图",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="collect_map", text="收集碎片", reward=new Reward{ memories=2 } },
                    new QuestChoice{ id="study_map", text="辨认地点", reward=new Reward{ memories=1 } },
                    new QuestChoice{ id="ignore_map_piece", text="不理会", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_fountain", name="古老的硬币", type=QuestType.Event, difficulty=1,
                description="发现一座干涸的喷泉",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="throw_coin", text="投币许愿", reward=new Reward{ memories=2, soulEssence=1 }, cost=new Cost{ food=1 } },
                    new QuestChoice{ id="examine_fountain", text="调查喷泉", reward=new Reward{ memories=2 } },
                    new QuestChoice{ id="ignore_fountain", text="离开", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_carving", name="树上的刻痕", type=QuestType.Event, difficulty=1,
                description="树干上有人留下的刻痕",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="read_carving", text="辨认信息", reward=new Reward{ memories=3 } },
                    new QuestChoice{ id="add_carving", text="添加自己的话", reward=new Reward{ memories=2 } },
                    new QuestChoice{ id="ignore_carving", text="不理会", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_storm_rainbow", name="暴风雨后的彩虹", type=QuestType.Event, difficulty=2,
                description="暴风雨过后，天空出现双彩虹",
                requirement=new Requirement{ minDay=7 },
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="chase_double", text="追寻双彩虹", reward=new Reward{ memories=5, food=10 } },
                    new QuestChoice{ id="rest_rainbow", text="休息欣赏", reward=new Reward{ memories=3 } },
                    new QuestChoice{ id="ignore_storm", text="继续旅程", reward=new Reward{ memories=1 } }
                }
            },
            new QuestData { id="event_wreath", name="枯萎的花圈", type=QuestType.Event, difficulty=2,
                description="树根旁有一个枯萎的花圈",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="investigate_wreath", text="调查花圈", reward=new Reward{ memories=4 }, requirement=new Requirement{ minDay=5 } },
                    new QuestChoice{ id="remember_wreath", text="默哀后离开", reward=new Reward{ memories=2 } },
                    new QuestChoice{ id="ignore_wreath", text="不理会", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_spiderweb", name="蜘蛛网拦路", type=QuestType.Event, difficulty=1,
                description="巨大的蜘蛛网横在路中",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="burn_web", text="烧掉蛛网", reward=new Reward{ memories=2, wood=3 } },
                    new QuestChoice{ id="go_around", text="绕路走", reward=new Reward{ memories=1 } },
                    new QuestChoice{ id="collect_web", text="收集蛛丝", reward=new Reward{ memories=2, herb=4 } }
                }
            },
            new QuestData { id="event_ufo", name="奇怪的脚印", type=QuestType.Event, difficulty=3,
                description="地上的脚印呈完美半圆形……是外星生物？",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="follow_ufo", text="跟踪脚印", reward=new Reward{ memories=5, soulEssence=3 }, requirement=new Requirement{ minDay=10 } },
                    new QuestChoice{ id="photograph_ufo", text="拍照记录", reward=new Reward{ memories=3 } },
                    new QuestChoice{ id="forget_ufo", text="假装没看见", reward=new Reward{ memories=1 } }
                }
            },
            new QuestData { id="event_deer", name="鹿群迁徙", type=QuestType.Event, difficulty=1,
                description="一群鹿正在穿越森林",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="follow_deer", text="跟随鹿群", reward=new Reward{ memories=3, food=8 } },
                    new QuestChoice{ id="hunt_deer", text="猎取一只", reward=new Reward{ food=15, memories=1 }, requirement=new Requirement{ minDay=5 } },
                    new QuestChoice{ id="watch_deer", text="静静观看", reward=new Reward{ memories=2 } }
                }
            },
            new QuestData { id="event_echo", name="回声", type=QuestType.Event, difficulty=1,
                description="山谷中传来你自己的回声",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="yell_echo", text="大声呼喊", reward=new Reward{ memories=2 } },
                    new QuestChoice{ id="whisper_echo", text="轻声呼唤", reward=new Reward{ memories=3, soulEssence=1 } },
                    new QuestChoice{ id="ignore_echo", text="安静离开", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_backpack", name="遗失的背包", type=QuestType.Event, difficulty=2,
                description="树杈上挂着一个被丢弃的背包",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="open_backpack", text="打开背包", reward=new Reward{ food=10, memories=2 } },
                    new QuestChoice{ id="search_backpack", text="仔细搜查", reward=new Reward{ memories=3, herb=5 } },
                    new QuestChoice{ id="leave_backpack", text="不动", reward=new Reward{ memories=1 } }
                }
            },
            new QuestData { id="event_lightning", name="被雷劈的树", type=QuestType.Event, difficulty=2,
                description="大树枝被雷击中正在燃烧",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="collect_charcoal", text="收集木炭", reward=new Reward{ wood=15, memories=2 } },
                    new QuestChoice{ id="extinguish_tree", text="扑灭火焰", reward=new Reward{ memories=3 }, cost=new Cost{ food=3 } },
                    new QuestChoice{ id="watch_burn", text="观察燃烧", reward=new Reward{ memories=1 } }
                }
            },
            new QuestData { id="event_butterfly", name="蝴蝶落在肩上", type=QuestType.Event, difficulty=1,
                description="一只美丽蝴蝶轻轻落在你肩头",
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="cherish_butterfly", text="珍视这一刻", reward=new Reward{ memories=3 } },
                    new QuestChoice{ id="release_butterfly", text="放飞蝴蝶", reward=new Reward{ memories=2, soulEssence=1 } },
                    new QuestChoice{ id="brush_butterfly", text="轻轻拂去", reward=new Reward{ memories=0 } }
                }
            },
            new QuestData { id="event_bell", name="远处钟声", type=QuestType.Event, difficulty=3,
                description="风中隐约传来古老的钟声",
                requirement=new Requirement{ minDay=10 },
                choices=new List<QuestChoice>{
                    new QuestChoice{ id="find_bell", text="循声寻找", reward=new Reward{ memories=6, soulEssence=3 } },
                    new QuestChoice{ id="listen_bell", text="静心聆听", reward=new Reward{ memories=4, soulEssence=2 } },
                    new QuestChoice{ id="ignore_bell", text="不理会", reward=new Reward{ memories=0 } }
                }
            },
            // ===== 连续任务 =====
            new QuestData { id="chain_ashes_1", name="灰烬的踪迹（前篇）", type=QuestType.Chain, difficulty=3,
                description="追查黑雾中的神秘身影",
                chainNextQuestId="chain_ashes_2",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.ExploreRegion, targetId="fog_front", count=1} },
                rewards=new Reward{ memories=2 }
            },
            new QuestData { id="chain_ashes_2", name="灰烬的踪迹（后篇）", type=QuestType.Chain, difficulty=3,
                description="找到神秘人——灰烬",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.Choice, targetId="ashes", count=1} },
                rewards=new Reward{ memories=5, soulEssence=3 }
            },
            new QuestData { id="chain_fh_1", name="森林之心（序章）", type=QuestType.Chain, difficulty=4,
                description="收集5个魂精华",
                chainNextQuestId="chain_fh_2",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.CollectResource, targetId="soulEssence", count=5} },
                rewards=new Reward{ memories=5 }
            },
            new QuestData { id="chain_fh_2", name="森林之心（觉醒）", type=QuestType.Chain, difficulty=5,
                description="前往森林深处共鸣",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.ReachLocation, targetId="forest_heart", count=1} },
                rewards=new Reward{ memories=10 }
            },
            new QuestData { id="chain_village_1", name="失落村落（发现）", type=QuestType.Chain, difficulty=2,
                description="调查未标记的村落",
                chainNextQuestId="chain_village_2",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.ExploreRegion, targetId="dark_valley", count=1} },
                rewards=new Reward{ memories=3 }
            },
            new QuestData { id="chain_village_2", name="失落村落（探索）", type=QuestType.Chain, difficulty=3,
                description="村落有幸存者痕迹",
                chainNextQuestId="chain_village_3",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.Choice, targetId="village", count=1} },
                rewards=new Reward{ memories=4, food=10 }
            },
            new QuestData { id="chain_village_3", name="失落村落（真相）", type=QuestType.Chain, difficulty=4,
                description="发现村落被黑雾吞噬的真相",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.Choice, targetId="truth", count=1} },
                rewards=new Reward{ memories=8 }
            },
            new QuestData { id="chain_elder_1", name="老者的托付", type=QuestType.Chain, difficulty=3,
                description="将信送到森林另一边",
                chainNextQuestId="chain_elder_2",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.ReachLocation, targetId="forest_heart", count=1} },
                rewards=new Reward{ memories=5 }
            },
            new QuestData { id="chain_elder_2", name="老者的托付（后篇）", type=QuestType.Chain, difficulty=4,
                description="收信人揭开老者身份真相",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.Choice, targetId="elder_msg", count=1} },
                rewards=new Reward{ memories=10, soulEssence=5 }
            },
            new QuestData { id="chain_fog_1", name="黑雾之源（调查）", type=QuestType.Chain, difficulty=4,
                description="收集黑雾起源线索",
                chainNextQuestId="chain_fog_2",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.KillEnemy, targetId="fog_lord", count=1} },
                rewards=new Reward{ memories=6 }
            },
            new QuestData { id="chain_fog_2", name="黑雾之源（发现）", type=QuestType.Chain, difficulty=5,
                description="发现黑雾真正来源",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.Choice, targetId="fog_origin", count=1} },
                rewards=new Reward{ memories=12 }
            },
            new QuestData { id="chain_trial_1", name="守护者试炼", type=QuestType.Chain, difficulty=4,
                description="森林守护者要进行试炼",
                chainNextQuestId="chain_trial_2",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.KillEnemy, targetId="forest_guardian", count=1} },
                rewards=new Reward{ memories=8, soulEssence=5 }
            },
            new QuestData { id="chain_trial_2", name="守护者试炼（后篇）", type=QuestType.Chain, difficulty=5,
                description="守护者提出一个选择",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.Choice, targetId="guardian", count=1} },
                rewards=new Reward{ memories=15 }
            },
            // ===== Roguelike 每日 =====
            new QuestData { id="rogue_kill3", name="今日幸存者", type=QuestType.RogueDaily, difficulty=1,
                description="今天击败3只敌人",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.KillEnemy, targetId="any", count=3} },
                rewards=new Reward{ food=5, memories=1 }
            },
            new QuestData { id="rogue_gather25", name="今日探索者", type=QuestType.RogueDaily, difficulty=1,
                description="今天采集25个资源",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.GatherAny, count=25} },
                rewards=new Reward{ memories=2 }
            },
            new QuestData { id="rogue_talk2", name="今日外交", type=QuestType.RogueDaily, difficulty=1,
                description="今天与NPC交谈2次",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.TalkNPC, count=2} },
                rewards=new Reward{ memories=1, soulEssence=1 }
            },
            new QuestData { id="rogue_craft3", name="今日工匠", type=QuestType.RogueDaily, difficulty=2,
                description="今天制作3个物品",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.CraftAny, count=3} },
                rewards=new Reward{ memories=2 }
            },
            new QuestData { id="rogue_food20", name="今日大厨", type=QuestType.RogueDaily, difficulty=1,
                description="今天收集20个食物",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.GatherResource, targetId="food", count=20} },
                rewards=new Reward{ food=10, memories=1 }
            },
            new QuestData { id="rogue_soul5", name="今日灵魂", type=QuestType.RogueDaily, difficulty=2,
                description="今天收集5个魂精华",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.CollectResource, targetId="soulEssence", count=5} },
                rewards=new Reward{ soulEssence=3, memories=2 }
            },
            new QuestData { id="rogue_elite1", name="今日精英", type=QuestType.RogueDaily, difficulty=3,
                description="今天击败1只精英",
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.KillEliteEnemy, count=1} },
                rewards=new Reward{ memories=3, soulEssence=3 }
            },
            // ===== 隐藏任务 =====
            new QuestData { id="secret_mushroom", name="蘑菇之神", type=QuestType.Secret, difficulty=2,
                description="采摘50个草药后，传说被唤醒...",
                requirement=new Requirement{ minGatherType="herb", minGatherCount=50 },
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.Choice, targetId="mushroom", count=1} },
                rewards=new Reward{ memories=8, herb=30 }
            },
            new QuestData { id="secret_bone_c", name="骨收集者", type=QuestType.Secret, difficulty=3,
                description="收集100个骨骼后，奇怪商人出现...",
                requirement=new Requirement{ minGatherType="bone", minGatherCount=100 },
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.Choice, targetId="bone_deal", count=1} },
                rewards=new Reward{ memories=10, soulEssence=10 }
            },
            new QuestData { id="secret_star_m", name="星辰记忆", type=QuestType.Secret, difficulty=3,
                description="20个魂精华后，星空在召唤...",
                requirement=new Requirement{ minSoulEssence=20 },
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.CollectResource, targetId="soulEssence", count=20} },
                rewards=new Reward{ memories=15 }
            },
            new QuestData { id="secret_fisher", name="神秘渔夫", type=QuestType.Secret, difficulty=2,
                description="钓鱼10次后，渔夫分享秘密...",
                requirement=new Requirement{ minGatherType="food_fishing", minGatherCount=10 },
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.Choice, targetId="fisherman", count=1} },
                rewards=new Reward{ memories=6, food=30 }
            },
            new QuestData { id="secret_tree_h", name="拥抱树木的人", type=QuestType.Secret, difficulty=2,
                description="采集20个木材后，神秘人现身...",
                requirement=new Requirement{ minGatherType="wood", minGatherCount=20 },
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.Choice, targetId="tree_hug", count=1} },
                rewards=new Reward{ memories=8, wood=50 }
            },
            new QuestData { id="secret_night", name="夜猫子", type=QuestType.Secret, difficulty=2,
                description="连续5个夜晚醒着，似乎发现了什么...",
                requirement=new Requirement{ minNightAwake=5 },
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.Choice, targetId="night_secret", count=1} },
                rewards=new Reward{ memories=10 }
            },
            new QuestData { id="secret_all_recipe", name="全能工匠", type=QuestType.Secret, difficulty=3,
                description="制作全部配方后，解锁隐藏配方...",
                requirement=new Requirement{ minRecipesCrafted=12 },
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.Choice, targetId="all_recipes", count=1} },
                rewards=new Reward{ memories=12, herb=20 }
            },
            new QuestData { id="secret_all_npc", name="社交达人", type=QuestType.Secret, difficulty=2,
                description="与所有NPC好感度达到最高后...",
                requirement=new Requirement{ minAllNPCFriendship=80 },
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.Choice, targetId="all_npc", count=1} },
                rewards=new Reward{ memories=10 }
            },
            new QuestData { id="secret_pacifist", name="和平主义者", type=QuestType.Secret, difficulty=3,
                description="只击杀BOSS的情况下通关...",
                requirement=new Requirement{ minNoKillDays=21 },
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.Choice, targetId="pacifist", count=1} },
                rewards=new Reward{ memories=20 }
            },
            new QuestData { id="secret_speedrun", name="速通者", type=QuestType.Secret, difficulty=4,
                description="10天内到达森林之心...",
                requirement=new Requirement{ minDay=10 },
                objectives=new List<Objective>{ new Objective{type=ObjectiveType.ReachLocation, targetId="forest_heart", count=1} },
                rewards=new Reward{ memories=15 }
            },
        };
    }
}