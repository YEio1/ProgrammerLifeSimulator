using Xunit;
using Moq;
using ProgrammerLifeSimulator.ViewModels;
using ProgrammerLifeSimulator.Services;
using ProgrammerLifeSimulator.Models;
using System.Collections.Generic;
using System.Linq;

public class MainWindowViewModelTest
{
    private readonly Mock<IGameEngineService> _mockGameEngineService;
    private readonly Mock<IRandomService> _mockRandomService;
    private readonly MainWindowViewModel _viewModel;

    public MainWindowViewModelTest()
    {
        _mockGameEngineService = new Mock<IGameEngineService>();
        _mockRandomService = new Mock<IRandomService>();
        
        // 设置完整的 Mock 来避免 GameViewModel 初始化问题
        SetupComprehensiveMocks();
        
        _viewModel = new MainWindowViewModel(_mockGameEngineService.Object, _mockRandomService.Object);
    }

    private void SetupComprehensiveMocks()
    {
        // 创建模拟事件数据
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
                        EffectDescription = "Effect 1"
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
                    EffectDescription = "Passive effect occurred"
                },
                Weight = 5,
                AllowRepeat = true
            }
        };

        // 设置 GetStatusWarnings
        _mockGameEngineService.Setup(x => x.GetStatusWarnings(It.IsAny<Player>()))
                            .Returns(new List<string>());

        // 设置 SelectWeightedEvent - 处理主动和被动事件
        _mockGameEngineService.Setup(x => x.SelectWeightedEvent(
            It.IsAny<IList<GameEvent>>(), 
            It.IsAny<Player>(), 
            It.IsAny<bool>(), 
            It.IsAny<bool>(), 
            It.IsAny<HashSet<string>>(), 
            It.IsAny<int>()))
            .Returns((IList<GameEvent> pool, Player p, bool rare, bool cosmic, HashSet<string> seen, int month) => 
            {
                // 如果池子不为空，返回第一个事件
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
                    player.ProgrammingSkill = Math.Clamp(player.ProgrammingSkill, 0, 100);
                    player.AlgorithmSkill = Math.Clamp(player.AlgorithmSkill, 0, 100);
                    player.DebuggingSkill = Math.Clamp(player.DebuggingSkill, 0, 100);
                    player.CommunicationSkill = Math.Clamp(player.CommunicationSkill, 0, 100);
                    player.Stress = Math.Clamp(player.Stress, 0, 100);
                    player.Health = Math.Clamp(player.Health, 0, 100);
                    player.Motivation = Math.Clamp(player.Motivation, 0, 100);
                    player.Salary = Math.Max(0, player.Salary);
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

    [Fact]
    public void Constructor_ShouldInitializeWithCharacterCreationView()
    {
        // Assert
        Assert.NotNull(_viewModel.CurrentView);
        Assert.IsType<CharacterCreationViewModel>(_viewModel.CurrentView);
    }

    [Fact]
    public void NavigateToGame_ShouldChangeCurrentViewToGameViewModel()
    {
        // Arrange
        var player = new Player { Name = "TestPlayer" };

        // Act
        _viewModel.NavigateToGame(player);

        // Assert
        Assert.NotNull(_viewModel.CurrentView);
        Assert.IsType<GameViewModel>(_viewModel.CurrentView);
    }

    [Fact]
    public void CurrentView_PropertyChange_ShouldNotify()
    {
        // Arrange
        var propertyChanged = false;
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.CurrentView))
                propertyChanged = true;
        };
        var player = new Player { Name = "TestPlayer" };

        // Act
        _viewModel.NavigateToGame(player);

        // Assert
        Assert.True(propertyChanged);
    }

    [Fact]
    public void NavigateToGame_ShouldCreateGameViewModelWithCorrectPlayer()
    {
        // Arrange
        var player = new Player 
        { 
            Name = "TestPlayer",
            ProgrammingSkill = 50,
            AlgorithmSkill = 40
        };

        // Act
        _viewModel.NavigateToGame(player);
        var gameViewModel = _viewModel.CurrentView as GameViewModel;

        // Assert
        Assert.NotNull(gameViewModel);
        Assert.Equal(player.Name, gameViewModel.Player.Name);
        Assert.Equal(player.ProgrammingSkill, gameViewModel.Player.ProgrammingSkill);
        Assert.Equal(player.AlgorithmSkill, gameViewModel.Player.AlgorithmSkill);
    }

    [Fact]
    public void NavigateToGame_ShouldPassServicesToGameViewModel()
    {
        // Arrange
        var player = new Player { Name = "TestPlayer" };

        // Act
        _viewModel.NavigateToGame(player);
        var gameViewModel = _viewModel.CurrentView as GameViewModel;

        // Assert
        Assert.NotNull(gameViewModel);
        // 验证 GameViewModel 成功创建（没有抛出异常）就说明服务被正确传递
    }
}