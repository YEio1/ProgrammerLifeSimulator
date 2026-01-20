using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ProgrammerLifeSimulator.Models;

namespace ProgrammerLifeSimulator.Services;

public static class MockDataService
{
    private static readonly JsonSerializerOptions EventSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static Player CreateBasePlayer(string name)
    {
        return new Player
        {
            Name = string.IsNullOrWhiteSpace(name) ? "测试程序员" : name.Trim(),
            Age = 22,
            ProgrammingSkill = 50,
            AlgorithmSkill = 45,
            DebuggingSkill = 40,
            CommunicationSkill = 35,
            Stress = 20,
            Health = 80,
            Motivation = 70,
            Salary = 8000
        };
    }

    public static List<Trait> GetAvailableTraits() => new()
    {
        new Trait
        {
            Name = "算法高手",
            Description = "算法+15 / 编程+5",
            ProgrammingSkillBonus = 5,
            AlgorithmSkillBonus = 15
        },
        new Trait
        {
            Name = "调试专家",
            Description = "调试+15 / 压力-5",
            DebuggingSkillBonus = 15,
            StressDelta = -5
        },
        new Trait
        {
            Name = "沟通达人",
            Description = "沟通+15 / 激励+5",
            CommunicationSkillBonus = 15,
            MotivationDelta = 5
        },
        new Trait
        {
            Name = "全栈工程师",
            Description = "编程+10 / 调试+10",
            ProgrammingSkillBonus = 10,
            DebuggingSkillBonus = 10
        },
        new Trait
        {
            Name = "学习狂人",
            Description = "所有技能+5 / 健康-5",
            ProgrammingSkillBonus = 5,
            AlgorithmSkillBonus = 5,
            DebuggingSkillBonus = 5,
            CommunicationSkillBonus = 5,
            HealthDelta = -5
        },
        new Trait
        {
            Name = "工作生活平衡",
            Description = "健康+10 / 压力-10",
            HealthDelta = 10,
            StressDelta = -10
        }
    };
    
    public static List<GameEvent> GetMockEvents()
    {
        var loaded = LoadEventsFromDisk();
        if (loaded.Count > 0)
        {
            return loaded;
        }

        return GetLegacyEvents();
    }

    private static List<GameEvent> GetLegacyEvents() => new()
    {
        new GameEvent
        {
            Title = "入职第一天",
            Description = "你加入了新的科技公司，感觉既兴奋又紧张……",
            Options = new List<EventOption>
            {
                new EventOption
                {
                    Text = "主动认识同事",
                    EffectDescription = "你主动与团队成员交流，氛围变得融洽，你的心情也放松了不少。",
                    CommunicationSkillDelta = 5,
                    StressDelta = -3
                },
                new EventOption
                {
                    Text = "专心研究代码",
                    EffectDescription = "你沉浸在代码中，加深了对项目的理解，但感觉精神有点紧张。",
                    ProgrammingSkillDelta = 4,
                    StressDelta = 2
                }
            }
        },
        new GameEvent
        {
            Title = "第一个项目",
            Description = "经理分配给你第一个项目，deadline 很紧。",
            Options = new List<EventOption>
            {
                new EventOption
                {
                    Text = "加班完成",
                    EffectDescription = "你通宵达旦，终于在死线前完成了项目，技术有所提升，但身体发出了警报。",
                    ProgrammingSkillDelta = 5,
                    StressDelta = 8,
                    HealthDelta = -5
                },
                new EventOption
                {
                    Text = "申请延期",
                    EffectDescription = "你与经理进行了有效沟通，争取到了更多时间，但同事们对你的积极性略有微词。",
                    CommunicationSkillDelta = 4,
                    MotivationDelta = -3
                }
            }
        },
        new GameEvent
        {
            Title = "代码评审",
            Description = "你的代码需要进行团队评审，你希望留下好印象。",
            Options = new List<EventOption>
            {
                new EventOption
                {
                    Text = "提前自查",
                    EffectDescription = "你提前进行了细致的自查和测试，发现了几个隐藏错误，提高了调试能力，但耗费了额外精力。",
                    DebuggingSkillDelta = 5,
                    StressDelta = 2
                },
                new EventOption
                {
                    Text = "请教资深同事",
                    EffectDescription = "你虚心向资深同事请教，不仅解决了问题，还学到了一些编程技巧。",
                    CommunicationSkillDelta = 4,
                    ProgrammingSkillDelta = 2
                }
            }
        },
        new GameEvent
        {
            Title = "技术分享会",
            Description = "公司组织技术分享会，你被邀请分享你的项目经验。",
            Options = new List<EventOption>
            {
                new EventOption
                {
                    Text = "精心准备PPT",
                    EffectDescription = "你精心准备，分享非常成功，巩固了专业形象，但因此增加了不少压力。",
                    CommunicationSkillDelta = 6,
                    ProgrammingSkillDelta = 3,
                    StressDelta = 3
                },
                new EventOption
                {
                    Text = "即兴分享",
                    EffectDescription = "你临场发挥，虽然略有不足，但你对分享的热情感染了大家，获得了成就感。",
                    CommunicationSkillDelta = 3,
                    MotivationDelta = 5
                },
                new EventOption
                {
                    Text = "婉拒邀请",
                    EffectDescription = "你避免了公众演讲的压力，但错失了在团队中展现自我的机会，略感失落。",
                    StressDelta = -2,
                    MotivationDelta = -3
                }
            }
        },
        new GameEvent
        {
            Title = "生产环境Bug",
            Description = "你负责的系统在生产环境出现了严重bug，用户投诉不断。",
            Options = new List<EventOption>
            {
                new EventOption
                {
                    Text = "紧急修复",
                    EffectDescription = "你在巨大的压力下紧急修复了Bug，调试能力暴涨，但健康状况也受到了严重影响。",
                    DebuggingSkillDelta = 8,
                    StressDelta = 10,
                    HealthDelta = -5
                },
                new EventOption
                {
                    Text = "分析根因再修复",
                    EffectDescription = "你没有急于动手，而是深入分析了算法逻辑，从根本上解决了问题，但依然感到不小的压力。",
                    DebuggingSkillDelta = 5,
                    AlgorithmSkillDelta = 3,
                    StressDelta = 5
                }
            }
        },
        new GameEvent
        {
            Title = "技术选型会议",
            Description = "团队需要选择新的技术栈，你被邀请参与讨论。",
            Options = new List<EventOption>
            {
                new EventOption
                {
                    Text = "深入研究后提出建议",
                    EffectDescription = "你进行了深入研究，提出了基于数据支持的建议，展现了全面的技术和沟通能力。",
                    AlgorithmSkillDelta = 4,
                    CommunicationSkillDelta = 5,
                    ProgrammingSkillDelta = 3
                },
                new EventOption
                {
                    Text = "跟随主流选择",
                    EffectDescription = "你选择了最流行的技术，虽然没有特别突出，但保证了项目的顺利进行，压力也比较小。",
                    ProgrammingSkillDelta = 2,
                    StressDelta = -2
                }
            }
        },
        new GameEvent
        {
            Title = "代码重构",
            Description = "你发现项目中有很多遗留代码需要重构，但时间紧迫。",
            Options = new List<EventOption>
            {
                new EventOption
                {
                    Text = "加班重构",
                    EffectDescription = "你投入了数个夜晚重构了核心模块，代码质量大幅提高，但你感到精疲力尽。",
                    ProgrammingSkillDelta = 7,
                    DebuggingSkillDelta = 4,
                    StressDelta = 6,
                    HealthDelta = -3
                },
                new EventOption
                {
                    Text = "逐步重构",
                    EffectDescription = "你在日常工作中见缝插针进行重构，代码得到改善，但重构带来的额外工作量让你稍有压力。",
                    ProgrammingSkillDelta = 4,
                    StressDelta = 2
                },
                new EventOption
                {
                    Text = "暂时搁置",
                    EffectDescription = "你选择暂时放下重构，压力减轻，但你知道这是在累积技术债务，感到有些沮丧。",
                    StressDelta = -3,
                    MotivationDelta = -2
                }
            }
        },
        new GameEvent
        {
            Title = "面试新同事",
            Description = "你被安排参与技术面试，需要评估候选人的能力。",
            Options = new List<EventOption>
            {
                new EventOption
                {
                    Text = "设计算法题",
                    EffectDescription = "你设计了巧妙的算法题来考察候选人，你的算法思维得到了锻炼，也提高了面试官的沟通技巧。",
                    AlgorithmSkillDelta = 5,
                    CommunicationSkillDelta = 3
                },
                new EventOption
                {
                    Text = "考察项目经验",
                    EffectDescription = "你通过细致的交流考察了候选人的实际项目经验，提升了沟通效率，也回顾了自身的编程知识。",
                    CommunicationSkillDelta = 6,
                    ProgrammingSkillDelta = 2
                }
            }
        },
        new GameEvent
        {
            Title = "性能优化",
            Description = "系统响应变慢，需要优化性能。",
            Options = new List<EventOption>
            {
                new EventOption
                {
                    Text = "深入分析性能瓶颈",
                    EffectDescription = "你使用专业的分析工具找到了深层瓶颈，解决了复杂问题，但过程非常烧脑。",
                    AlgorithmSkillDelta = 7,
                    DebuggingSkillDelta = 5,
                    StressDelta = 4
                },
                new EventOption
                {
                    Text = "快速优化",
                    EffectDescription = "你应用了一些简单的优化技巧，性能有所提升，但未从根本解决问题。",
                    ProgrammingSkillDelta = 4,
                    DebuggingSkillDelta = 3,
                    StressDelta = 2
                }
            }
        },
        new GameEvent
        {
            Title = "学习新技术",
            Description = "公司引入了新的技术框架，需要学习。",
            Options = new List<EventOption>
            {
                new EventOption
                {
                    Text = "系统学习",
                    EffectDescription = "你投入大量时间进行系统学习和实践，技术栈得到深度扩展，但压力也有所增加。",
                    ProgrammingSkillDelta = 6,
                    AlgorithmSkillDelta = 4,
                    StressDelta = 3
                },
                new EventOption
                {
                    Text = "边用边学",
                    EffectDescription = "你在项目中摸索着使用新技术，勉强完成了任务，调试过程让你学到了一些东西。",
                    ProgrammingSkillDelta = 3,
                    DebuggingSkillDelta = 2
                }
            }
        },
        new GameEvent
        {
            Title = "团队冲突",
            Description = "团队成员对技术方案有分歧，气氛紧张。",
            Options = new List<EventOption>
            {
                new EventOption
                {
                    Text = "主动协调",
                    EffectDescription = "你主动介入，通过有效的沟通解决了团队矛盾，虽然耗费精力，但你获得了大家的尊重。",
                    CommunicationSkillDelta = 8,
                    StressDelta = 5,
                    MotivationDelta = 3
                },
                new EventOption
                {
                    Text = "保持中立",
                    EffectDescription = "你选择不参与争吵，避免了卷入压力，但看到团队分裂让你感到士气低落。",
                    StressDelta = -2,
                    MotivationDelta = -2
                }
            }
        },
        new GameEvent
        {
            Title = "年度考核",
            Description = "年度绩效考核即将开始，你需要准备述职报告。",
            Options = new List<EventOption>
            {
                new EventOption
                {
                    Text = "详细准备",
                    EffectDescription = "你精心准备了述职报告，清晰展现了你的贡献，成功获得加薪，但准备过程很辛苦。",
                    CommunicationSkillDelta = 5,
                    SalaryDelta = 2000,
                    StressDelta = 3
                },
                new EventOption
                {
                    Text = "简单准备",
                    EffectDescription = "你只是简单写了报告，虽然拿到了一点加薪，但过程轻松。",
                    SalaryDelta = 1000,
                    StressDelta = -2
                }
            }
        },
        new GameEvent
        {
            Title = "开源贡献",
            Description = "你发现一个开源项目的bug，考虑是否提交修复。",
            Options = new List<EventOption>
            {
                new EventOption
                {
                    Text = "提交PR",
                    EffectDescription = "你向开源项目提交了高质量的修复，得到了社区的认可，成就感大增。",
                    ProgrammingSkillDelta = 5,
                    DebuggingSkillDelta = 4,
                    MotivationDelta = 6
                },
                new EventOption
                {
                    Text = "只修复本地",
                    EffectDescription = "你只在本地解决了问题，避免了提交PR的麻烦，调试能力略有提升。",
                    DebuggingSkillDelta = 3
                }
            }
        },
        new GameEvent
        {
            Title = "技术债务",
            Description = "项目积累了大量技术债务，需要处理。",
            Options = new List<EventOption>
            {
                new EventOption
                {
                    Text = "制定重构计划",
                    EffectDescription = "你成功说服团队并制定了详细的重构计划，展示了你的技术远见和影响力。",
                    ProgrammingSkillDelta = 5,
                    AlgorithmSkillDelta = 3,
                    CommunicationSkillDelta = 4
                },
                new EventOption
                {
                    Text = "先处理紧急的",
                    EffectDescription = "你快速处理了最紧迫的问题，但技术债务仍未解决，让你感到些许压力。",
                    DebuggingSkillDelta = 4,
                    StressDelta = 3
                }
            }
        },
        new GameEvent
        {
            Title = "跨部门协作",
            Description = "需要与其他部门协作完成一个大型项目。",
            Options = new List<EventOption>
            {
                new EventOption
                {
                    Text = "主动沟通协调",
                    EffectDescription = "你主动承担了沟通桥梁的角色，确保了跨部门协作的顺利进行，获得了成就感。",
                    CommunicationSkillDelta = 7,
                    ProgrammingSkillDelta = 3,
                    MotivationDelta = 4
                },
                new EventOption
                {
                    Text = "被动配合",
                    EffectDescription = "你只是完成了自己的部分，被动的配合使得协作效率低下，也让你感到不快。",
                    CommunicationSkillDelta = 2,
                    StressDelta = 3
                }
            }
        },
        new GameEvent
        {
            Title = "技术博客",
            Description = "你考虑写一篇技术博客分享经验。",
            Options = new List<EventOption>
            {
                new EventOption
                {
                    Text = "认真撰写",
                    EffectDescription = "你花费大量时间撰写了高质量的技术文章，提升了知名度，并对自己的技术进行了梳理。",
                    CommunicationSkillDelta = 6,
                    ProgrammingSkillDelta = 4,
                    MotivationDelta = 5
                },
                new EventOption
                {
                    Text = "简单记录",
                    EffectDescription = "你简单记录了一些心得，虽然效果有限，但也是一种积累。",
                    CommunicationSkillDelta = 2,
                    ProgrammingSkillDelta = 2
                }
            }
        },
        new GameEvent
        {
            Title = "代码审查",
            Description = "你需要审查同事提交的代码。",
            Options = new List<EventOption>
            {
                new EventOption
                {
                    Text = "仔细审查",
                    EffectDescription = "你发现了几个关键的逻辑漏洞，并提出了建设性修改意见，得到了团队的赞赏和尊重，但审查过程有点耗神。",
                    DebuggingSkillDelta = 5,
                    CommunicationSkillDelta = 4,
                    StressDelta = 2
                },
                new EventOption
                {
                    Text = "快速通过",
                    EffectDescription = "你快速通过了审查，避免了额外工作，但心里有些不安。",
                    StressDelta = -2,
                    MotivationDelta = -2
                }
            }
        },
        new GameEvent
        {
            Title = "技术大会",
            Description = "公司派你参加技术大会，可以学习新技术。",
            Options = new List<EventOption>
            {
                new EventOption
                {
                    Text = "积极参与",
                    EffectDescription = "你在大会上与各路大牛积极交流，收获了最新的技术知识，感觉充满了能量。",
                    ProgrammingSkillDelta = 5,
                    AlgorithmSkillDelta = 5,
                    CommunicationSkillDelta = 6,
                    MotivationDelta = 5
                },
                new EventOption
                {
                    Text = "走马观花",
                    EffectDescription = "你只是简单听了几个演讲，技术上略有收获。",
                    ProgrammingSkillDelta = 2,
                    AlgorithmSkillDelta = 2
                }
            }
        },
        new GameEvent
        {
            Title = "项目上线",
            Description = "你负责的项目即将上线，需要确保一切顺利。",
            Options = new List<EventOption>
            {
                new EventOption
                {
                    Text = "熬夜保障",
                    EffectDescription = "你为了项目上线通宵达旦，虽然获得了额外的奖金，但透支了健康，压力巨大。",
                    DebuggingSkillDelta = 6,
                    StressDelta = 8,
                    HealthDelta = -5,
                    SalaryDelta = 1500
                },
                new EventOption
                {
                    Text = "提前准备",
                    EffectDescription = "你提前做好了周密的上线准备和回滚方案，减轻了临时的压力。",
                    DebuggingSkillDelta = 4,
                    ProgrammingSkillDelta = 3,
                    StressDelta = 3
                }
            }
        },
        new GameEvent
        {
            Title = "职业规划",
            Description = "你开始思考自己的职业发展方向。",
            Options = new List<EventOption>
            {
                new EventOption
                {
                    Text = "专注技术深度",
                    EffectDescription = "你决定专注于成为一名资深专家，投入大量精力打磨核心技术。",
                    AlgorithmSkillDelta = 6,
                    ProgrammingSkillDelta = 5,
                    MotivationDelta = 4
                },
                new EventOption
                {
                    Text = "转向管理",
                    EffectDescription = "你开始学习项目管理和领导力，为转岗做准备，对未来充满期待。",
                    CommunicationSkillDelta = 8,
                    MotivationDelta = 3
                },
                new EventOption
                {
                    Text = "保持现状",
                    EffectDescription = "你对现状满意，选择佛系工作，压力大幅减轻。",
                    StressDelta = -3
                }
            }
        },
        new GameEvent
        {
            Title = "工作倦怠",
            Description = "连续加班让你感到疲惫，工作效率下降。",
            Options = new List<EventOption>
            {
                new EventOption
                {
                    Text = "请假休息",
                    EffectDescription = "你果断请假，好好放松了身心，找回了工作的热情。",
                    HealthDelta = 10,
                    StressDelta = -8,
                    MotivationDelta = 5
                },
                new EventOption
                {
                    Text = "坚持工作",
                    EffectDescription = "你咬牙坚持，工作完成了，但精神状态和身体状况都进一步恶化。",
                    HealthDelta = -5,
                    StressDelta = 5,
                    MotivationDelta = -3
                }
            }
        },
        new GameEvent
        {
            Title = "技能提升",
            Description = "你决定投入时间提升自己的技能。",
            Options = new List<EventOption>
            {
                new EventOption
                {
                    Text = "系统学习算法",
                    EffectDescription = "你投入了整个周末深入研究高级算法，成功地破解了几个复杂的难题，感觉自己脱胎换骨。",
                    AlgorithmSkillDelta = 8,
                    StressDelta = 3
                },
                new EventOption
                {
                    Text = "学习新框架",
                    EffectDescription = "你快速掌握了最新的前端框架，开发效率明显提高。",
                    ProgrammingSkillDelta = 6,
                    DebuggingSkillDelta = 3
                },
                new EventOption
                {
                    Text = "提升沟通能力",
                    EffectDescription = "通过参加公开演讲训练，你的表达能力得到认可，团队氛围变得更加积极。",
                    CommunicationSkillDelta = 7,
                    MotivationDelta = 4
                }
            }
        }
    };

    private static List<GameEvent> LoadEventsFromDisk()
    {
        try
        {
            var baseDir = AppContext.BaseDirectory;
            var candidatePath = Path.Combine(baseDir, "Assets", "Data", "events.json");
            if (!File.Exists(candidatePath))
            {
                return new List<GameEvent>();
            }

            var json = File.ReadAllText(candidatePath);
            var data = JsonSerializer.Deserialize<List<GameEvent>>(json, EventSerializerOptions);
            return data?.Count > 0 ? data : new List<GameEvent>();
        }
        catch
        {
            return new List<GameEvent>();
        }
    }
}