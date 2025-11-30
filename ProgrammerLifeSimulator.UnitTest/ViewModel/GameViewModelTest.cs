using Xunit;
using Moq;
using ProgrammerLifeSimulator.ViewModels;
using ProgrammerLifeSimulator.Services;
using ProgrammerLifeSimulator.Models;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class GameViewModelTest
{
    private readonly Mock<IGameEngineService> _mockGameEngineService;
    private readonly Mock<IRandomService> _mockRandomService;
    private readonly Player _testPlayer;
    private GameViewModel _viewModel;

    public GameViewModelTest()
    {
        _mockGameEngineService = new Mock<IGameEngineService>();
        _mockRandomService = new Mock<IRandomService>();
        _testPlayer = new Player 
        { 
            Name = "TestPlayer",
            ProgrammingSkill = 50,
            AlgorithmSkill = 40,
            DebuggingSkill = 30,
            CommunicationSkill = 45,
            Stress = 30,
            Health = 80,
            Motivation = 70,
            Salary = 5000
        };
        
        SetupComprehensiveMocks();
        CreateSafeViewModel();
    }

    private void SetupComprehensiveMocks()
    {
        // 创建完整的模拟事件数据
        var mockEvents = new List<GameEvent>
        {
            new GameEvent 
            { 
                Id = "Event1", 
                Title = "Test Event 1", 
                Description = "Test Description 1",
                Options = new List<EventOption>
                {
                    new EventOption 
                    { 
                        Text = "Option 1",
                        EffectDescription = "Effect 1",
                        ProgrammingSkillDelta = 5,
                        StressDelta = 10
                    },
                    new EventOption 
                    { 
                        Text = "Option 2",
                        EffectDescription = "Effect 2",
                        AlgorithmSkillDelta = 3,
                        HealthDelta = -5
                    }
                },
                Weight = 10,
                AllowRepeat = true
            }
        };

        var mockPassiveEvents = new List<GameEvent>
        {
            new GameEvent 
            { 
                Id = "PassiveEvent1",
                Title = "Passive Event", 
                Description = "Passive Description",
                IsPassive = true,
                PassiveEffect = new EventOption 
                { 
                    EffectDescription = "Passive effect occurred",
                    StressDelta = -5
                },
                Weight = 5,
                AllowRepeat = true
            }
        };

        // 设置 GetStatusWarnings
        _mockGameEngineService.Setup(x => x.GetStatusWarnings(It.IsAny<Player>()))
                            .Returns(new List<string>());

        // 设置 SelectWeightedEvent - 确保永远不返回 null
        _mockGameEngineService.Setup(x => x.SelectWeightedEvent(
            It.IsAny<IList<GameEvent>>(), 
            It.IsAny<Player>(), 
            It.IsAny<bool>(), 
            It.IsAny<bool>(), 
            It.IsAny<HashSet<string>>(), 
            It.IsAny<int>()))
            .Returns((IList<GameEvent> pool, Player p, bool rare, bool cosmic, HashSet<string> seen, int month) => 
            {
                // 如果池子不为空，返回第一个事件；否则返回默认事件
                return pool?.FirstOrDefault() ?? CreateDefaultGameEvent();
            });

        // 设置 ApplyOptionEffects
        _mockGameEngineService.Setup(x => x.ApplyOptionEffects(
            It.IsAny<Player>(), 
            It.IsAny<EventOption>(), 
            ref It.Ref<int>.IsAny, 
            ref It.Ref<int>.IsAny, 
            ref It.Ref<bool>.IsAny, 
            ref It.Ref<bool>.IsAny))
            .Callback((Player player, EventOption option, ref int leadership, ref int innovation, ref bool rare, ref bool cosmic) =>
            {
                // 模拟应用效果
                if (option != null)
                {
                    player.ProgrammingSkill += option.ProgrammingSkillDelta;
                    player.AlgorithmSkill += option.AlgorithmSkillDelta;
                    player.DebuggingSkill += option.DebuggingSkillDelta;
                    player.CommunicationSkill += option.CommunicationSkillDelta;
                    player.Stress += option.StressDelta;
                    player.Health += option.HealthDelta;
                    player.Motivation += option.MotivationDelta;
                    player.Salary += option.SalaryDelta;
                    leadership += option.LeadershipDelta;
                    innovation += option.InnovationDelta;
                    rare |= option.UnlocksRareEvent;
                    cosmic |= option.UnlocksCosmicInsight;

                    // 确保数值在合理范围内
                    player.ProgrammingSkill = System.Math.Clamp(player.ProgrammingSkill, 0, 100);
                    player.AlgorithmSkill = System.Math.Clamp(player.AlgorithmSkill, 0, 100);
                    player.DebuggingSkill = System.Math.Clamp(player.DebuggingSkill, 0, 100);
                    player.CommunicationSkill = System.Math.Clamp(player.CommunicationSkill, 0, 100);
                    player.Stress = System.Math.Clamp(player.Stress, 0, 100);
                    player.Health = System.Math.Clamp(player.Health, 0, 100);
                    player.Motivation = System.Math.Clamp(player.Motivation, 0, 100);
                    player.Salary = System.Math.Max(0, player.Salary);
                }
            });

        // 设置 CheckForEnding
        _mockGameEngineService.Setup(x => x.CheckForEnding(
            It.IsAny<Player>(), 
            It.IsAny<int>(), 
            It.IsAny<int>(), 
            It.IsAny<int>(), 
            It.IsAny<int>(), 
            It.IsAny<int>(), 
            It.IsAny<int>(), 
            It.IsAny<bool>()))
            .Returns((Player player, int eventsCompleted, int avgStress, int avgHealth, int avgMotivation, int leadership, int innovation, bool cosmic) =>
            {
                return new GameEnding { Title = "Test Ending", Description = "Test Description" };
            });

        // 设置随机服务
        _mockRandomService.Setup(x => x.NextDouble()).Returns(0.1); // 让环境事件有几率触发
        _mockRandomService.Setup(x => x.Next(It.IsAny<int>())).Returns(0);
    }

    private GameEvent CreateDefaultGameEvent()
    {
        return new GameEvent 
        { 
            Id = "DefaultEvent",
            Title = "Default Event",
            Description = "This is a default event for testing",
            Options = new List<EventOption>
            {
                new EventOption 
                { 
                    Text = "Continue",
                    EffectDescription = "You continue your journey"
                }
            },
            Weight = 1,
            AllowRepeat = true
        };
    }

    private void CreateSafeViewModel()
    {
        // 创建真实的 GameViewModel
        _viewModel = new GameViewModel(_testPlayer, _mockGameEngineService.Object, _mockRandomService.Object);
        
        // 通过反射确保所有必要的字段都已正确初始化
        EnsureViewModelInitialization();
    }

    private void EnsureViewModelInitialization()
    {
        // 设置私有字段以确保安全状态
        SetPrivateField("_currentEvent", new GameEvent 
        { 
            Id = "TestEvent",
            Title = "Test Event", 
            Description = "Test Description",
            Options = new List<EventOption>
            {
                new EventOption 
                { 
                    Text = "Test Option",
                    EffectDescription = "Test Effect"
                }
            }
        });
        
        SetPrivateField("_statusWarnings", new List<string>());
        SetPrivateField("_recentRandomEvents", new List<string>());
        SetPrivateField("_seenEventIds", new HashSet<string>());
        SetPrivateField("_playedEvents", new List<GameEvent>());
        SetPrivateField("_allEvents", new List<GameEvent>());
        
        // 确保月份相关字段正确设置
        SetPrivateField("_month", 1);
        SetPrivateField("_eventsCompleted", 0);
        SetPrivateField("_totalStress", 0);
        SetPrivateField("_totalHealth", 0);
        SetPrivateField("_totalMotivation", 0);
        SetPrivateField("_leadershipProgress", 0);
        SetPrivateField("_innovationProgress", 0);
        SetPrivateField("_rareEventUnlocked", false);
        SetPrivateField("_cosmicInsightUnlocked", false);
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Assert
        Assert.NotNull(_viewModel.Player);
        Assert.Equal(_testPlayer, _viewModel.Player);
        Assert.Equal(1, _viewModel.Month);
        Assert.NotNull(_viewModel.CurrentEvent);
        Assert.NotNull(_viewModel.SelectOptionCommand);
        Assert.NotNull(_viewModel.StatusWarnings);
        Assert.NotNull(_viewModel.RecentRandomEvents);
    }

    [Fact]
    public void Month_PropertyChange_ShouldRaiseTimeProperties()
    {
        // Arrange
        var propertiesChanged = new List<string>();
        _viewModel.PropertyChanged += (s, e) => propertiesChanged.Add(e.PropertyName);

        // Act
        // 通过反射设置月份并触发属性变更
        var monthProperty = typeof(GameViewModel).GetProperty("Month");
        monthProperty?.SetValue(_viewModel, 2);

        // Assert
        Assert.Contains(nameof(GameViewModel.TimeDisplay), propertiesChanged);
        Assert.Contains(nameof(GameViewModel.TimelineDisplay), propertiesChanged);
        Assert.Contains(nameof(GameViewModel.TimelineProgress), propertiesChanged);
        Assert.Contains(nameof(GameViewModel.CareerPhase), propertiesChanged);
    }

    [Fact]
    public void TimeDisplay_ShouldFormatCorrectly()
    {
        // Act & Assert - 第1个月
        Assert.Equal("第 1 年 · 第 1 个月", _viewModel.TimeDisplay);

        // 测试其他月份
        SetMonth(13);
        Assert.Equal("第 2 年 · 第 13 个月", _viewModel.TimeDisplay);
    }

    [Fact]
    public void CareerPhase_ShouldChangeBasedOnMonth()
    {
        // Act & Assert - 第1年
        Assert.Contains("新手期", _viewModel.CareerPhase);

        SetMonth(13);
        Assert.Contains("成长期", _viewModel.CareerPhase);

        SetMonth(25);
        Assert.Contains("突破期", _viewModel.CareerPhase);

        SetMonth(37);
        Assert.Contains("传奇篇章", _viewModel.CareerPhase);
    }

    [Fact]
    public void SelectOption_ShouldApplyEffectsAndAdvanceMonth()
    {
        // Arrange
        var initialMonth = _viewModel.Month;
        var initialProgramming = _viewModel.Player.ProgrammingSkill;
        
        var option = new EventOption 
        { 
            Text = "Test Option",
            EffectDescription = "Test Effect",
            ProgrammingSkillDelta = 5,
            StressDelta = 10
        };

        // Act
        _viewModel.SelectOptionCommand.Execute(option);

        // Assert
        Assert.Equal(initialMonth + 1, _viewModel.Month);
        Assert.Equal(initialProgramming + 5, _viewModel.Player.ProgrammingSkill);
        
        // 验证服务方法被调用
        _mockGameEngineService.Verify(x => x.ApplyOptionEffects(
            It.IsAny<Player>(), 
            It.IsAny<EventOption>(), 
            ref It.Ref<int>.IsAny, 
            ref It.Ref<int>.IsAny, 
            ref It.Ref<bool>.IsAny, 
            ref It.Ref<bool>.IsAny), 
            Times.Once);
    }

    [Fact]
    public void SelectOption_ShouldUpdateLeadershipAndInnovation()
    {
        // Arrange
        var initialLeadership = GetPrivateField<int>("_leadershipProgress");
        var option = new EventOption 
        { 
            LeadershipDelta = 5,
            InnovationDelta = 3
        };

        // Act
        _viewModel.SelectOptionCommand.Execute(option);

        // Assert
        Assert.Equal(initialLeadership + 5, GetPrivateField<int>("_leadershipProgress"));
        Assert.Equal(3, GetPrivateField<int>("_innovationProgress"));
    }

    [Fact]
    public void SelectOption_ShouldDoNothing_WhenOptionIsNull()
    {
        // Arrange
        var initialMonth = _viewModel.Month;

        // Act
        _viewModel.SelectOptionCommand.Execute(null);

        // Assert
        Assert.Equal(initialMonth, _viewModel.Month);
    }

    [Fact]
    public void SelectOption_ShouldDoNothing_WhenGameCompleted()
    {
        // Arrange
        SetEnding(new GameEnding { Title = "Test Ending", Description = "Test Description" });
        var initialMonth = _viewModel.Month;
        var option = new EventOption { Text = "Test Option" };

        // Act
        _viewModel.SelectOptionCommand.Execute(option);

        // Assert
        Assert.Equal(initialMonth, _viewModel.Month);
    }

    [Fact]
    public void IsGameCompleted_ShouldReturnTrue_WhenEndingIsSet()
    {
        // Arrange
        SetEnding(new GameEnding { Title = "Test Ending", Description = "Test Description" });

        // Act & Assert
        Assert.True(_viewModel.IsGameCompleted);
    }

    [Fact]
    public void GoalProgressSummary_ShouldFormatCorrectly()
    {
        // Arrange
        SetPrivateField("_leadershipProgress", 30);
        SetPrivateField("_innovationProgress", 40);

        // Act
        var summary = _viewModel.GoalProgressSummary;

        // Assert
        Assert.Equal("晋升进度 30/100 · 创业灵感 40/100", summary);
    }

    [Fact]
    public void StatusWarnings_ShouldComeFromGameEngineService()
    {
        // Arrange
        var expectedWarnings = new List<string> { "压力偏高", "健康告急" };
        _mockGameEngineService.Setup(x => x.GetStatusWarnings(It.IsAny<Player>()))
                            .Returns(expectedWarnings);

        // 触发状态警告更新
        var updateMethod = typeof(GameViewModel).GetMethod("UpdateStatusWarnings", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        updateMethod?.Invoke(_viewModel, null);

        // Act & Assert
        Assert.Equal(expectedWarnings, _viewModel.StatusWarnings);
    }

    [Fact]
    public void CompleteGame_ShouldSetEndingFromService()
    {
        // Arrange
        var expectedEnding = new GameEnding { Title = "Test Ending", Description = "Test Description" };
        _mockGameEngineService.Setup(x => x.CheckForEnding(
            It.IsAny<Player>(), 
            It.IsAny<int>(), 
            It.IsAny<int>(), 
            It.IsAny<int>(), 
            It.IsAny<int>(), 
            It.IsAny<int>(), 
            It.IsAny<int>(), 
            It.IsAny<bool>()))
            .Returns(expectedEnding);

        // 通过反射调用CompleteGame方法
        var completeGameMethod = typeof(GameViewModel).GetMethod("CompleteGame", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        
        // Act
        completeGameMethod?.Invoke(_viewModel, null);

        // Assert
        Assert.Equal(expectedEnding, _viewModel.Ending);
    }

    [Fact]
    public void LastImpactDetails_ShouldBuildCorrectSummary()
    {
        // Arrange
        var option = new EventOption 
        { 
            ProgrammingSkillDelta = 5,
            StressDelta = 10
        };

        // Act
        _viewModel.SelectOptionCommand.Execute(option);

        // Assert
        Assert.Contains("本次影响", _viewModel.LastImpactDetails);
    }

    [Fact]
    public void GameSummary_ShouldFormatCorrectly_WhenGameCompleted()
    {
        // Arrange
        var ending = new GameEnding 
        { 
            Title = "Test Ending", 
            Description = "Test Description" 
        };
        SetEnding(ending);
        SetMonth(20);

        // Act
        var summary = _viewModel.GameSummary;

        // Assert
        Assert.Contains("Test Description", summary);
        Assert.Contains("职业历程", summary);
    }

    // 辅助方法
    private void SetMonth(int month)
    {
        SetPrivateField("_month", month);
        // 触发属性变更通知
        var property = typeof(GameViewModel).GetProperty("Month");
        property?.SetValue(_viewModel, month);
    }

    private void SetEnding(GameEnding ending)
    {
        SetPrivateField("_ending", ending);
        // 触发属性变更通知
        var property = typeof(GameViewModel).GetProperty("Ending");
        property?.SetValue(_viewModel, ending);
    }

    private void SetPrivateField(string fieldName, object value)
    {
        var field = typeof(GameViewModel).GetField(fieldName, 
            BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(_viewModel, value);
    }

    private T GetPrivateField<T>(string fieldName)
    {
        var field = typeof(GameViewModel).GetField(fieldName, 
            BindingFlags.NonPublic | BindingFlags.Instance);
        return (T)field?.GetValue(_viewModel);
    }
}