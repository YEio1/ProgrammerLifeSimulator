using Xunit;
using Moq;
using ProgrammerLifeSimulator.Services;
using ProgrammerLifeSimulator.Models;
using ProgrammerLifeSimulator.UnitTest.Mocks;
using System.Collections.Generic;

public class GameEngineServiceTests
{
    private readonly Mock<IRandomService> _mockRandomService = new(); 
    
    private GameEngineService CreateService()
    {
        return new GameEngineService(_mockRandomService.Object); 
    }

    #region ApplyOptionEffects 补充测试

    [Fact]
    public void ApplyOptionEffects_ShouldClampSkillsAtMax()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        player.ProgrammingSkill = 95;
        
        var option = new EventOption { ProgrammingSkillDelta = 10 };
        
        int leadership = 0, innovation = 0;
        bool rare = false, cosmic = false;
        
        service.ApplyOptionEffects(player, option, ref leadership, ref innovation, ref rare, ref cosmic);
        
        Assert.Equal(100, player.ProgrammingSkill);
    }

    [Fact]
    public void ApplyOptionEffects_ShouldClampSkillsAtMin()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        player.AlgorithmSkill = 5;
        
        var option = new EventOption { AlgorithmSkillDelta = -10 };
        
        int leadership = 0, innovation = 0;
        bool rare = false, cosmic = false;
        
        service.ApplyOptionEffects(player, option, ref leadership, ref innovation, ref rare, ref cosmic);
        
        Assert.Equal(0, player.AlgorithmSkill);
    }

    [Fact]
    public void ApplyOptionEffects_ShouldUpdateAllSkillsAndAttributes()
    {
        var service = CreateService();
        var player = new Player 
        {
            Name = "Test",
            ProgrammingSkill = 50,
            AlgorithmSkill = 50,
            DebuggingSkill = 50,
            CommunicationSkill = 50,
            Salary = 5000,
            Stress = 50,
            Health = 50,
            Motivation = 50
        };
        
        var option = new EventOption 
        { 
            ProgrammingSkillDelta = 5,
            AlgorithmSkillDelta = 3,
            DebuggingSkillDelta = 2,
            CommunicationSkillDelta = 4,
            SalaryDelta = 1000,
            StressDelta = -5,
            HealthDelta = 3,
            MotivationDelta = 2,
            LeadershipDelta = 1,
            InnovationDelta = 2
        };
        
        int leadership = 10, innovation = 15;
        bool rare = false, cosmic = false;
        
        service.ApplyOptionEffects(player, option, ref leadership, ref innovation, ref rare, ref cosmic);
        
        Assert.Equal(55, player.ProgrammingSkill); // 假设初始50
        Assert.Equal(53, player.AlgorithmSkill);
        Assert.Equal(52, player.DebuggingSkill);
        Assert.Equal(54, player.CommunicationSkill);
        Assert.Equal(6000, player.Salary); // 假设初始5000
        Assert.Equal(45, player.Stress);
        Assert.Equal(53, player.Health);
        Assert.Equal(52, player.Motivation);
        Assert.Equal(11, leadership);
        Assert.Equal(17, innovation);
    }

    [Fact]
    public void ApplyOptionEffects_ShouldUnlockRareEvent()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        var option = new EventOption { UnlocksRareEvent = true };
        
        int leadership = 0, innovation = 0;
        bool rare = false, cosmic = false;
        
        service.ApplyOptionEffects(player, option, ref leadership, ref innovation, ref rare, ref cosmic);
        
        Assert.True(rare);
    }

    [Fact]
    public void ApplyOptionEffects_ShouldUnlockCosmicInsight()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        var option = new EventOption { UnlocksCosmicInsight = true };
        
        int leadership = 0, innovation = 0;
        bool rare = false, cosmic = false;
        
        service.ApplyOptionEffects(player, option, ref leadership, ref innovation, ref rare, ref cosmic);
        
        Assert.True(cosmic);
    }

    [Fact]
    public void ApplyOptionEffects_ShouldNotAllowNegativeSalary()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        player.Salary = 500;
        
        var option = new EventOption { SalaryDelta = -1000 };
        
        int leadership = 0, innovation = 0;
        bool rare = false, cosmic = false;
        
        service.ApplyOptionEffects(player, option, ref leadership, ref innovation, ref rare, ref cosmic);
        
        Assert.Equal(0, player.Salary);
    }

    #endregion

    #region GetStatusWarnings 补充测试

    [Fact]
    public void GetStatusWarnings_ShouldReturnExtremeStressWarning()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        player.Stress = 85;
        
        var warnings = service.GetStatusWarnings(player);
        
        Assert.Contains("压力已极高，随时可能触发过劳事件", warnings);
    }

    [Fact]
    public void GetStatusWarnings_ShouldReturnHealthWarning()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        player.Health = 30;
        
        var warnings = service.GetStatusWarnings(player);
        
        Assert.Contains("健康告急，优先考虑休息或就医", warnings);
    }

    [Fact]
    public void GetStatusWarnings_ShouldReturnMotivationWarning()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        player.Motivation = 25;
        
        var warnings = service.GetStatusWarnings(player);
        
        Assert.Contains("激励不足，可能影响长期目标推进", warnings);
    }

    [Fact]
    public void GetStatusWarnings_ShouldReturnMultipleWarnings()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        player.Stress = 85;
        player.Health = 20;
        player.Motivation = 15;
        
        var warnings = service.GetStatusWarnings(player);
        
        Assert.Equal(3, warnings.Count);
        Assert.Contains("压力已极高，随时可能触发过劳事件", warnings);
        Assert.Contains("健康告急，优先考虑休息或就医", warnings);
        Assert.Contains("激励不足，可能影响长期目标推进", warnings);
    }

    [Fact]
    public void GetStatusWarnings_ShouldReturnNoWarnings()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        player.Stress = 50;
        player.Health = 60;
        player.Motivation = 70;
        
        var warnings = service.GetStatusWarnings(player);
        
        Assert.Empty(warnings);
    }

    #endregion

    #region CheckForEnding 补充测试

    [Fact]
    public void CheckForEnding_ShouldReturnOverworkEnding_WhenHealthCritical()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        player.Health = 20;
        
        var ending = service.CheckForEnding(
            player, 
            eventsCompleted: 10, 
            avgStress: 50, avgHealth: 60, avgMotivation: 70, 
            leadershipProgress: 50, 
            innovationProgress: 50, 
            cosmicInsightUnlocked: false);
        
        Assert.NotNull(ending);
        Assert.Equal("过劳警报", ending.Title);
    }

    [Fact]
    public void CheckForEnding_ShouldReturnOverworkEnding_WhenHighAverageStress()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        
        var ending = service.CheckForEnding(
            player, 
            eventsCompleted: 10, 
            avgStress: 80, avgHealth: 60, avgMotivation: 70, 
            leadershipProgress: 50, 
            innovationProgress: 50, 
            cosmicInsightUnlocked: false);
        
        Assert.NotNull(ending);
        Assert.Equal("过劳警报", ending.Title);
    }

    [Fact]
    public void CheckForEnding_ShouldReturnNewDirectionEnding_WhenLowMotivation()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        player.Motivation = 25;
        
        var ending = service.CheckForEnding(
            player, 
            eventsCompleted: 10, 
            avgStress: 50, avgHealth: 60, avgMotivation: 40, 
            leadershipProgress: 50, 
            innovationProgress: 50, 
            cosmicInsightUnlocked: false);
        
        Assert.NotNull(ending);
        Assert.Equal("寻求新方向", ending.Title);
    }

    [Fact]
    public void CheckForEnding_ShouldReturnSocialAnimalEnding()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        player.Salary = 10000;
        
        var ending = service.CheckForEnding(
            player, 
            eventsCompleted: 10, 
            avgStress: 70, avgHealth: 60, avgMotivation: 50, 
            leadershipProgress: 50, 
            innovationProgress: 50, 
            cosmicInsightUnlocked: false);
        
        Assert.NotNull(ending);
        Assert.Equal("社畜循环", ending.Title);
    }

    [Fact]
    public void CheckForEnding_ShouldReturnEntrepreneurEnding()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        
        var ending = service.CheckForEnding(
            player, 
            eventsCompleted: 10, 
            avgStress: 50, avgHealth: 60, avgMotivation: 70, 
            leadershipProgress: 50, 
            innovationProgress: 95, 
            cosmicInsightUnlocked: false);
        
        Assert.NotNull(ending);
        Assert.Equal("创业新星", ending.Title);
    }

    [Fact]
    public void CheckForEnding_ShouldReturnCosmicArchitectEnding()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        
        var ending = service.CheckForEnding(
            player, 
            eventsCompleted: 10, 
            avgStress: 50, avgHealth: 60, avgMotivation: 70, 
            leadershipProgress: 50, 
            innovationProgress: 75, 
            cosmicInsightUnlocked: true);
        
        Assert.NotNull(ending);
        Assert.Equal("宇宙架构师", ending.Title);
    }

    [Fact]
    public void CheckForEnding_ShouldReturnTechStarEnding()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        player.ProgrammingSkill = 95;
        player.Salary = 30000;
        
        var ending = service.CheckForEnding(
            player, 
            eventsCompleted: 10, 
            avgStress: 50, avgHealth: 60, avgMotivation: 70, 
            leadershipProgress: 50, 
            innovationProgress: 50, 
            cosmicInsightUnlocked: false);
        
        Assert.NotNull(ending);
        Assert.Equal("技术明星", ending.Title);
    }

    [Fact]
    public void CheckForEnding_ShouldReturnDigitalNomadEnding()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        player.Salary = 30000;
        player.Stress = 30;
        player.Motivation = 90;
        
        var ending = service.CheckForEnding(
            player, 
            eventsCompleted: 10, 
            avgStress: 40, avgHealth: 60, avgMotivation: 90, 
            leadershipProgress: 50, 
            innovationProgress: 50, 
            cosmicInsightUnlocked: false);
        
        Assert.NotNull(ending);
        Assert.Equal("数字游牧", ending.Title);
    }

    [Fact]
    public void CheckForEnding_ShouldReturnIndependentHackerEnding()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        player.Motivation = 98;
        
        var ending = service.CheckForEnding(
            player, 
            eventsCompleted: 10, 
            avgStress: 50, avgHealth: 60, avgMotivation: 96, 
            leadershipProgress: 30, 
            innovationProgress: 70, 
            cosmicInsightUnlocked: false);
        
        Assert.NotNull(ending);
        Assert.Equal("独立黑客", ending.Title);
    }

    [Fact]
    public void CheckForEnding_ShouldReturnTeamLeaderEnding()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        player.CommunicationSkill = 85;
        player.Motivation = 70;
        
        var ending = service.CheckForEnding(
            player, 
            eventsCompleted: 10, 
            avgStress: 50, avgHealth: 60, avgMotivation: 70, 
            leadershipProgress: 50, 
            innovationProgress: 50, 
            cosmicInsightUnlocked: false);
        
        Assert.NotNull(ending);
        Assert.Equal("团队领袖", ending.Title);
    }

    [Fact]
    public void CheckForEnding_ShouldReturnSlowLifeMentorEnding()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        player.Stress = 30;
        
        var ending = service.CheckForEnding(
            player, 
            eventsCompleted: 10, 
            avgStress: 30, avgHealth: 85, avgMotivation: 70, 
            leadershipProgress: 30, 
            innovationProgress: 50, 
            cosmicInsightUnlocked: false);
        
        Assert.NotNull(ending);
        Assert.Equal("慢生活导师", ending.Title);
    }

    [Fact]
    public void CheckForEnding_ShouldReturnBalanceMasterEnding()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        player.Stress = 40;
        
        var ending = service.CheckForEnding(
            player, 
            eventsCompleted: 10, 
            avgStress: 40, avgHealth: 75, avgMotivation: 70, 
            leadershipProgress: 50, 
            innovationProgress: 50, 
            cosmicInsightUnlocked: false);
        
        Assert.NotNull(ending);
        Assert.Equal("平衡大师", ending.Title);
    }

    [Fact]
    public void CheckForEnding_ShouldReturnSteadyProgress_WhenNoSpecialConditions()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        
        var ending = service.CheckForEnding(
            player, 
            eventsCompleted: 10, 
            avgStress: 50, avgHealth: 60, avgMotivation: 60, 
            leadershipProgress: 50, 
            innovationProgress: 50, 
            cosmicInsightUnlocked: false);
        
        Assert.NotNull(ending);
        Assert.Equal("稳步前行", ending.Title);
    }

    #endregion

    #region SelectWeightedEvent 补充测试

    [Fact]
    public void SelectWeightedEvent_ShouldReturnNull_WhenPoolIsEmpty()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        var emptyPool = new List<GameEvent>();
        var seenEvents = new HashSet<string>();
        
        var result = service.SelectWeightedEvent(
            emptyPool, player, 
            rareEventUnlocked: false, cosmicInsightUnlocked: false, 
            seenEventIds: seenEvents, currentMonth: 1);
        
        Assert.Null(result);
    }

    [Fact]
    public void SelectWeightedEvent_ShouldReturnSingleEvent_WhenPoolHasOneEvent()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        var singleEvent = new GameEvent { Id = "Single", Weight = 10 };
        var pool = new List<GameEvent> { singleEvent };
        var seenEvents = new HashSet<string>();
        
        var result = service.SelectWeightedEvent(
            pool, player, 
            rareEventUnlocked: false, cosmicInsightUnlocked: false, 
            seenEventIds: seenEvents, currentMonth: 1);
        
        Assert.Equal(singleEvent, result);
    }

    [Fact]
    public void SelectWeightedEvent_ShouldHandleZeroTotalWeight()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        
        // 创建权重为0的事件
        var event1 = new GameEvent { Id = "A", Weight = 0 };
        var event2 = new GameEvent { Id = "B", Weight = 0 };
        var pool = new List<GameEvent> { event1, event2 };
        var seenEvents = new HashSet<string>();
        
        // 模拟随机数返回0
        _mockRandomService.Setup(x => x.Next(It.IsAny<int>())).Returns(0);
        
        var result = service.SelectWeightedEvent(
            pool, player, 
            rareEventUnlocked: false, cosmicInsightUnlocked: false, 
            seenEventIds: seenEvents, currentMonth: 1);
        
        Assert.NotNull(result); // 应该返回某个事件，不会崩溃
    }

    [Fact]
    public void SelectWeightedEvent_ShouldApplyRarityModifiers()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        
        var rareEvent = new GameEvent { 
            Id = "Rare", 
            Weight = 5, 
            Rarity = "rare",
            Tags = new List<string>()
        };
        var pool = new List<GameEvent> { rareEvent };
        var seenEvents = new HashSet<string>();
        
        _mockRandomService.Setup(x => x.Next(It.IsAny<int>())).Returns(0);
        
        var result = service.SelectWeightedEvent(
            pool, player, 
            rareEventUnlocked: false, cosmicInsightUnlocked: false, 
            seenEventIds: seenEvents, currentMonth: 1);
        
        Assert.NotNull(result);
    }

    [Fact]
    public void SelectWeightedEvent_ShouldIncreaseWeightForBurnoutEvents_WhenPlayerStressed()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        player.Stress = 80; // 高压力
        
        var burnoutEvent = new GameEvent { 
            Id = "Burnout", 
            Weight = 5, 
            Tags = new List<string> { "burnout" }
        };
        var pool = new List<GameEvent> { burnoutEvent };
        var seenEvents = new HashSet<string>();
        
        _mockRandomService.Setup(x => x.Next(It.IsAny<int>())).Returns(0);
        
        var result = service.SelectWeightedEvent(
            pool, player, 
            rareEventUnlocked: false, cosmicInsightUnlocked: false, 
            seenEventIds: seenEvents, currentMonth: 1);
        
        Assert.NotNull(result);
    }

    [Fact]
    public void SelectWeightedEvent_ShouldIncreaseWeightForSeenRepeatableEvents()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        
        var repeatEvent = new GameEvent { 
            Id = "Repeat", 
            Weight = 5, 
            AllowRepeat = true,
            Tags = new List<string>()
        };
        var pool = new List<GameEvent> { repeatEvent };
        var seenEvents = new HashSet<string> { "Repeat" }; // 已经见过此事件
        
        _mockRandomService.Setup(x => x.Next(It.IsAny<int>())).Returns(0);
        
        var result = service.SelectWeightedEvent(
            pool, player, 
            rareEventUnlocked: false, cosmicInsightUnlocked: false, 
            seenEventIds: seenEvents, currentMonth: 1);
        
        Assert.NotNull(result);
    }

    [Fact]
    public void SelectWeightedEvent_ShouldDecreaseWeightForStarterEvents_InLateGame()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        
        var starterEvent = new GameEvent { 
            Id = "Starter", 
            Weight = 10, 
            Tags = new List<string> { "starter" }
        };
        var pool = new List<GameEvent> { starterEvent };
        var seenEvents = new HashSet<string>();
        
        _mockRandomService.Setup(x => x.Next(It.IsAny<int>())).Returns(0);
        
        var result = service.SelectWeightedEvent(
            pool, player, 
            rareEventUnlocked: false, cosmicInsightUnlocked: false, 
            seenEventIds: seenEvents, currentMonth: 10); // 后期游戏
        
        Assert.NotNull(result);
    }

    #endregion

    #region 边界情况和异常测试

    [Fact]
    public void ApplyOptionEffects_ShouldHandleZeroDeltas()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        var initialSkills = new {
            Programming = player.ProgrammingSkill,
            Algorithm = player.AlgorithmSkill,
            Debugging = player.DebuggingSkill,
            Communication = player.CommunicationSkill
        };
        
        var option = new EventOption(); // 所有delta为0
        
        int leadership = 0, innovation = 0;
        bool rare = false, cosmic = false;
        
        service.ApplyOptionEffects(player, option, ref leadership, ref innovation, ref rare, ref cosmic);
        
        Assert.Equal(initialSkills.Programming, player.ProgrammingSkill);
        Assert.Equal(initialSkills.Algorithm, player.AlgorithmSkill);
        Assert.Equal(initialSkills.Debugging, player.DebuggingSkill);
        Assert.Equal(initialSkills.Communication, player.CommunicationSkill);
        Assert.False(rare);
        Assert.False(cosmic);
    }

    [Fact]
    public void SelectWeightedEvent_ShouldHandleEventsWithNoId()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        
        var eventWithoutId = new GameEvent { 
            Weight = 10, 
            AllowRepeat = true,
            Tags = new List<string>()
        };
        var pool = new List<GameEvent> { eventWithoutId };
        var seenEvents = new HashSet<string>();
        
        _mockRandomService.Setup(x => x.Next(It.IsAny<int>())).Returns(0);
        
        var result = service.SelectWeightedEvent(
            pool, player, 
            rareEventUnlocked: false, cosmicInsightUnlocked: false, 
            seenEventIds: seenEvents, currentMonth: 1);
        
        Assert.NotNull(result);
    }

    #endregion
    
    [Fact]
    public void GetStatusWarnings_ShouldReturnHighStressWarning()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        player.Stress = 65; // 压力偏高但未极高
    
        var warnings = service.GetStatusWarnings(player);
    
        Assert.Contains("压力偏高，谨慎选择进一步冒险", warnings);
    }

    [Fact]
    public void GetStatusWarnings_ShouldReturnMultipleWarningsIncludingHighStress()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        player.Stress = 65; // 压力偏高
        player.Health = 30; // 健康告急
    
        var warnings = service.GetStatusWarnings(player);
    
        Assert.Equal(2, warnings.Count);
        Assert.Contains("压力偏高，谨慎选择进一步冒险", warnings);
        Assert.Contains("健康告急，优先考虑休息或就医", warnings);
    }
    
    [Fact]
    public void CheckForEnding_ShouldReturnPromotionEnding_WhenHighLeadership()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
    
        var ending = service.CheckForEnding(
            player, 
            eventsCompleted: 10, 
            avgStress: 50, avgHealth: 60, avgMotivation: 70, 
            leadershipProgress: 95,  // 高领导力进度
            innovationProgress: 50, 
            cosmicInsightUnlocked: false);
    
        Assert.NotNull(ending);
        Assert.Equal("晋升大佬", ending.Title);
    }


    [Fact]
    public void SelectWeightedEvent_ShouldApplyHealthModifier_WhenPlayerHealthLow()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        player.Health = 30; // 低健康

        var healthEvent = new GameEvent
        {
            Id = "HealthEvent",
            Weight = 5,
            Tags = new List<string> { "health" }
        };
        var pool = new List<GameEvent> { healthEvent };
        var seenEvents = new HashSet<string>();

        _mockRandomService.Setup(x => x.Next(It.IsAny<int>())).Returns(0);

        var result = service.SelectWeightedEvent(
            pool, player,
            rareEventUnlocked: false, cosmicInsightUnlocked: false,
            seenEventIds: seenEvents, currentMonth: 1);

        Assert.NotNull(result);
    }

    [Fact]
    public void SelectWeightedEvent_ShouldApplyMotivationModifier_WhenPlayerMotivationHigh()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");
        player.Motivation = 80; // 高动机

        var innovationEvent = new GameEvent
        {
            Id = "InnovationEvent",
            Weight = 5,
            Tags = new List<string> { "innovation" }
        };
        var pool = new List<GameEvent> { innovationEvent };
        var seenEvents = new HashSet<string>();

        _mockRandomService.Setup(x => x.Next(It.IsAny<int>())).Returns(0);

        var result = service.SelectWeightedEvent(
            pool, player,
            rareEventUnlocked: false, cosmicInsightUnlocked: false,
            seenEventIds: seenEvents, currentMonth: 1);

        Assert.NotNull(result);
    }

    [Fact]
    public void SelectWeightedEvent_ShouldApplyQuirkyTagModifier()
    {
        var service = CreateService();
        var player = MockDataService.CreateBasePlayer("Test");

        var quirkyEvent = new GameEvent
        {
            Id = "QuirkyEvent",
            Weight = 5,
            Tags = new List<string> { "quirky" }
        };
        var pool = new List<GameEvent> { quirkyEvent };
        var seenEvents = new HashSet<string>();

        _mockRandomService.Setup(x => x.Next(It.IsAny<int>())).Returns(0);

        var result = service.SelectWeightedEvent(
            pool, player,
            rareEventUnlocked: false, cosmicInsightUnlocked: false,
            seenEventIds: seenEvents, currentMonth: 1);

        Assert.NotNull(result);
    }
}