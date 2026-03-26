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