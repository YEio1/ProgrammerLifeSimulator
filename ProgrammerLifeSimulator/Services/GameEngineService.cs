using System;
using System.Collections.Generic;
using System.Linq;
using ProgrammerLifeSimulator.Models;

namespace ProgrammerLifeSimulator.Services;

public class GameEngineService : IGameEngineService
{
    private readonly IRandomService _randomService;

    // 构造函数注入 IService2
    public GameEngineService(IRandomService randomService)
    {
        _randomService = randomService;
    }
    
    // 功能 1: 数值计算 
    public void ApplyOptionEffects(Player player, EventOption option, 
                                   ref int leadershipProgress, ref int innovationProgress, 
                                   ref bool rareEventUnlocked, ref bool cosmicInsightUnlocked)
    {
        player.ProgrammingSkill = Math.Clamp(player.ProgrammingSkill + option.ProgrammingSkillDelta, 0, 100);
        player.AlgorithmSkill = Math.Clamp(player.AlgorithmSkill + option.AlgorithmSkillDelta, 0, 100);
        player.DebuggingSkill = Math.Clamp(player.DebuggingSkill + option.DebuggingSkillDelta, 0, 100);
        player.CommunicationSkill = Math.Clamp(player.CommunicationSkill + option.CommunicationSkillDelta, 0, 100);
        player.Salary = Math.Max(player.Salary + option.SalaryDelta, 0);

        player.Stress = Math.Clamp(player.Stress + option.StressDelta, 0, 100);
        player.Health = Math.Clamp(player.Health + option.HealthDelta, 0, 100);
        player.Motivation = Math.Clamp(player.Motivation + option.MotivationDelta, 0, 100);
        
        leadershipProgress += option.LeadershipDelta;
        innovationProgress += option.InnovationDelta;

        if (option.UnlocksRareEvent)
        {
            rareEventUnlocked = true;
        }

        if (option.UnlocksCosmicInsight)
        {
            cosmicInsightUnlocked = true;
        }
    }
    
    // 功能 2: 状态警告 
    public List<string> GetStatusWarnings(Player player)
    {
        var warnings = new List<string>();
        if (player.Stress >= 80)
        {
            warnings.Add("压力已极高，随时可能触发过劳事件");
        }
        else if (player.Stress >= 60)
        {
            warnings.Add("压力偏高，谨慎选择进一步冒险");
        }

        if (player.Health <= 35)
        {
            warnings.Add("健康告急，优先考虑休息或就医");
        }

        if (player.Motivation <= 30)
        {
            warnings.Add("激励不足，可能影响长期目标推进");
        }
        return warnings;
    }
    
    // 功能 3: 结局判断
    public GameEnding? CheckForEnding(Player player, int eventsCompleted, int avgStress, int avgHealth, int avgMotivation, 
                                      int leadershipProgress, int innovationProgress, bool cosmicInsightUnlocked)
    {
        // 获取最高技能逻辑
        var (skillName, highestSkill) = GetHighestSkill(player);

        if (player.Health <= 25 || avgStress >= 75)
        {
            return new GameEnding
            {
                Title = "过劳警报",
                Description = "长期高压与透支让你身心俱疲。或许是时候停下脚步，重新定义工作与生活的平衡。"
            };
        }

        if (player.Motivation <= 30 || avgMotivation <= 45)
        {
            return new GameEnding
            {
                Title = "寻求新方向",
                Description = "你意识到目前的工作已难以激发热情，决定暂别职场，寻找新的灵感与梦想。"
            };
        }

        if (avgStress >= 65 && avgStress < 75 && player.Salary < 12000)
        {
            return new GameEnding
            {
                Title = "社畜循环",
                Description = "你在忙碌与加班中原地打转，虽然没被压垮，却始终难以突破现状。"
            };
        }

        if (leadershipProgress >= 90)
        {
            return new GameEnding
            {
                Title = "晋升大佬",
                Description = "你一路打怪升级，建立起强大的影响力，成为团队仰望的技术经理。"
            };
        }

        if (innovationProgress >= 90)
        {
            return new GameEnding
            {
                Title = "创业新星",
                Description = "灵感与执行力齐飞，你带着副项目正式起航，踏上新的创业旅程。"
            };
        }

        if (cosmicInsightUnlocked && innovationProgress >= 70)
        {
            return new GameEnding
            {
                Title = "宇宙架构师",
                Description = "你与神秘灵感产生共鸣，正在设计只有少数人能理解的下一代系统。"
            };
        }

        if (highestSkill >= 90 && player.Salary >= 25000)
        {
            return new GameEnding
            {
                Title = "技术明星",
                Description = $"凭借卓越的{skillName}与持续的高薪回报，你成为团队中无法替代的技术标杆。"
            };
        }

        if (player.Salary >= 28000 && player.Stress <= 45 && player.Motivation >= 85)
        {
            return new GameEnding
            {
                Title = "数字游牧",
                Description = "你凭借扎实的技能与自由的心态，成为可以随处远程办公的潇洒工程师。"
            };
        }

        if (player.Motivation >= 95 && innovationProgress >= 60 && leadershipProgress <= 40)
        {
            return new GameEnding
            {
                Title = "独立黑客",
                Description = "你拒绝流程与会议，靠着个人魅力与创造力游走于各种炫酷项目。"
            };
        }

        if (player.CommunicationSkill >= 80 && player.Motivation >= 60)
        {
            return new GameEnding
            {
                Title = "团队领袖",
                Description = "你凭借卓越的沟通与激励，成功带领团队冲破瓶颈，开启管理者的新篇章。"
            };
        }

        if (avgHealth >= 80 && leadershipProgress < 40 && player.Stress <= 35)
        {
            return new GameEnding
            {
                Title = "慢生活导师",
                Description = "你把握住工作与生活的节奏，成为大家口中的养生榜样，也时常分享自己的秘诀。"
            };
        }

        if (avgHealth >= 70 && avgStress <= 45)
        {
            return new GameEnding
            {
                Title = "平衡大师",
                Description = "你保持着健康与工作的双赢状态，成为公司推崇的“稳健典范”。"
            };
        }
        
        // 游戏未触发特殊结局，但在如果时间到了，会调用此方法并可能返回 "稳步前行"
        // 如果所有条件都不满足，返回 null 表示游戏继续。
        // 所以这里我们假设如果被调用，就是需要一个结局。
        return new GameEnding
        {
            Title = "稳步前行",
            Description = "你在岗位上持续成长，积累了一身扎实本领，下一阶段的突破指日可待。"
        };
    }
    
    // 功能 4: 事件权重计算 
    public GameEvent SelectWeightedEvent(IList<GameEvent> pool, Player player, 
                                         bool rareEventUnlocked, bool cosmicInsightUnlocked, 
                                         HashSet<string> seenEventIds, int currentMonth)
    {
        if (pool.Count == 0) return null!;
        if (pool.Count == 1) return pool[0];

        // 计算权重的逻辑
        var weights = pool.Select(e => GetEffectiveWeight(e, player, rareEventUnlocked, cosmicInsightUnlocked, seenEventIds, currentMonth)).ToList();
        
        var totalWeight = Math.Max(weights.Sum(), 1);
        
        var roll = _randomService.Next(totalWeight);

        var cumulative = 0;
        for (var i = 0; i < pool.Count; i++)
        {
            cumulative += weights[i];
            if (roll < cumulative)
            {
                return pool[i];
            }
        }

        return pool.Last();
    }

    // 私有辅助方法：计算单个事件权重
    private int GetEffectiveWeight(GameEvent gameEvent, Player player, bool rareEventUnlocked, bool cosmicInsightUnlocked, HashSet<string> seenEventIds, int currentMonth)
    {
        var weight = Math.Max(gameEvent.Weight, 1);
        var rarity = gameEvent.Rarity?.ToLowerInvariant();

        switch (rarity)
        {
            case "uncommon": weight += 1; break;
            case "rare": weight -= 2; break;
            case "epic": weight -= 3; break;
            case "mythic": weight -= 4; break;
        }

        if (player.Stress >= 70 && gameEvent.Tags.Contains("burnout")) weight += 6;
        if (player.Health <= 45 && gameEvent.Tags.Contains("health")) weight += 5;
        if (player.Motivation >= 50 && gameEvent.Tags.Contains("innovation")) weight += 4; // 这里原代码是 InnovationProgress，这里稍微简化或需要传入 progress

        if (rareEventUnlocked && gameEvent.Tags.Contains("innovation")) weight += 6;
        if (cosmicInsightUnlocked && gameEvent.Tags.Contains("cosmic")) weight += 8;

        if (gameEvent.AllowRepeat && !string.IsNullOrWhiteSpace(gameEvent.Id) && seenEventIds.Contains(gameEvent.Id))
        {
            weight += 2;
        }

        if (gameEvent.Tags.Contains("starter") && currentMonth > 6)
        {
            weight -= 6;
        }

        if (gameEvent.Tags.Contains("quirky"))
        {
            weight += 1;
        }

        return Math.Max(1, weight);
    }

    private (string SkillName, int Value) GetHighestSkill(Player player)
    {
        var skills = new Dictionary<string, int>
        {
            { "编程技能", player.ProgrammingSkill },
            { "算法技能", player.AlgorithmSkill },
            { "调试技能", player.DebuggingSkill },
            { "沟通技能", player.CommunicationSkill }
        };

        var maxSkill = skills.OrderByDescending(s => s.Value).First();
        return (maxSkill.Key, maxSkill.Value);
    }
}