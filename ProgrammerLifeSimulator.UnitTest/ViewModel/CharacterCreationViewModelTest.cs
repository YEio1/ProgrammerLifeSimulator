using Xunit;
using Moq;
using ProgrammerLifeSimulator.ViewModels;
using ProgrammerLifeSimulator.Models;
using ProgrammerLifeSimulator.Services;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class CharacterCreationViewModelTest
{
    private readonly TestMainWindowViewModel _testNavigation;
    private readonly CharacterCreationViewModel _viewModel;
    private readonly List<Trait> _mockTraits;

    public CharacterCreationViewModelTest()
    {
        _testNavigation = new TestMainWindowViewModel();
        
        // 创建模拟特征数据
        _mockTraits = new List<Trait>
        {
            new Trait 
            { 
                Name = "快速学习者", 
                Description = "编程技能提升更快",
                ProgrammingSkillBonus = 10,
                AlgorithmSkillBonus = 5
            },
            new Trait 
            { 
                Name = "抗压达人", 
                Description = "压力增长更慢",
                StressDelta = -10
            }
        };

        _viewModel = new CharacterCreationViewModel(_testNavigation);
        
        // 使用反射设置 AvailableTraits 和 SelectedTrait
        SetAvailableTraits(_viewModel, _mockTraits);
        SetSelectedTrait(_viewModel, _mockTraits.First());
    }

    private void SetAvailableTraits(CharacterCreationViewModel viewModel, List<Trait> traits)
    {
        var availableTraitsField = typeof(CharacterCreationViewModel)
            .GetField("AvailableTraits", BindingFlags.Instance | BindingFlags.Public);
        
        if (availableTraitsField != null)
        {
            availableTraitsField.SetValue(viewModel, traits.AsReadOnly());
        }
    }

    private void SetSelectedTrait(CharacterCreationViewModel viewModel, Trait trait)
    {
        var selectedTraitProperty = typeof(CharacterCreationViewModel)
            .GetProperty("SelectedTrait");
        selectedTraitProperty?.SetValue(viewModel, trait);
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Assert
        Assert.NotNull(_viewModel.AvailableTraits);
        Assert.NotEmpty(_viewModel.AvailableTraits);
        Assert.NotNull(_viewModel.SelectedTrait);
        Assert.NotNull(_viewModel.StartGameCommand);
        Assert.Equal(string.Empty, _viewModel.PlayerName);
    }

    [Fact]
    public void PlayerName_PropertyChange_ShouldNotifyAndUpdateCommand()
    {
        // Arrange
        var propertyChanged = false;
        var commandCanExecuteChanged = false;
        
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(CharacterCreationViewModel.PlayerName))
                propertyChanged = true;
        };
        
        _viewModel.StartGameCommand.CanExecuteChanged += (s, e) =>
        {
            commandCanExecuteChanged = true;
        };

        // Act
        _viewModel.PlayerName = "NewName";

        // Assert
        Assert.True(propertyChanged);
        Assert.True(commandCanExecuteChanged);
        Assert.Equal("NewName", _viewModel.PlayerName);
    }

    [Fact]
    public void SelectedTrait_PropertyChange_ShouldNotifyAndUpdateCommand()
    {
        // Arrange
        var propertyChanged = false;
        var commandCanExecuteChanged = false;
        
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(CharacterCreationViewModel.SelectedTrait))
                propertyChanged = true;
        };
        
        _viewModel.StartGameCommand.CanExecuteChanged += (s, e) =>
        {
            commandCanExecuteChanged = true;
        };

        var newTrait = _viewModel.AvailableTraits.Last();

        // Act
        _viewModel.SelectedTrait = newTrait;

        // Assert
        Assert.True(propertyChanged);
        Assert.True(commandCanExecuteChanged);
        Assert.Equal(newTrait, _viewModel.SelectedTrait);
    }

    [Fact]
    public void CanStartGame_ShouldReturnFalse_WhenPlayerNameIsEmpty()
    {
        // Arrange
        _viewModel.PlayerName = "";
        _viewModel.SelectedTrait = _viewModel.AvailableTraits.First();

        // Act & Assert
        Assert.False(_viewModel.StartGameCommand.CanExecute(null));
    }

    [Fact]
    public void CanStartGame_ShouldReturnFalse_WhenPlayerNameIsWhitespace()
    {
        // Arrange
        _viewModel.PlayerName = "   ";
        _viewModel.SelectedTrait = _viewModel.AvailableTraits.First();

        // Act & Assert
        Assert.False(_viewModel.StartGameCommand.CanExecute(null));
    }

    [Fact]
    public void CanStartGame_ShouldReturnFalse_WhenSelectedTraitIsNull()
    {
        // Arrange
        _viewModel.PlayerName = "TestPlayer";
        _viewModel.SelectedTrait = null;

        // Act & Assert
        Assert.False(_viewModel.StartGameCommand.CanExecute(null));
    }

    [Fact]
    public void CanStartGame_ShouldReturnTrue_WhenValidInput()
    {
        // Arrange
        _viewModel.PlayerName = "TestPlayer";
        _viewModel.SelectedTrait = _viewModel.AvailableTraits.First();

        // Act & Assert
        Assert.True(_viewModel.StartGameCommand.CanExecute(null));
    }

    [Fact]
    public void StartGame_ShouldDoNothing_WhenSelectedTraitIsNull()
    {
        // Arrange
        _viewModel.PlayerName = "TestPlayer";
        _viewModel.SelectedTrait = null;

        bool navigationCalled = false;
        _testNavigation.NavigateToGameCallback = (player) => navigationCalled = true;

        // Act
        _viewModel.StartGameCommand.Execute(null);

        // Assert
        Assert.False(navigationCalled);
    }

    // 测试用的 MainWindowViewModel 子类 - 重写 NavigateToGame 以避免 GameViewModel 初始化问题
    private class TestMainWindowViewModel : MainWindowViewModel
    {
        public Action<Player> NavigateToGameCallback { get; set; }
        
        public TestMainWindowViewModel() 
            : base(CreateMockGameEngineService(), CreateMockRandomService())
        {
        }
        
        public void NavigateToGame(Player player)
        {
            NavigateToGameCallback?.Invoke(player);
            // 不调用基类方法，避免创建真实的 GameViewModel
        }

        private static IGameEngineService CreateMockGameEngineService()
        {
            var mock = new Mock<IGameEngineService>();
            // 设置基本的mock行为，避免空引用
            mock.Setup(x => x.GetStatusWarnings(It.IsAny<Player>())).Returns(new List<string>());
            mock.Setup(x => x.SelectWeightedEvent(It.IsAny<IList<GameEvent>>(), It.IsAny<Player>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<HashSet<string>>(), It.IsAny<int>()))
                .Returns(new GameEvent { Title = "Test Event", Options = new List<EventOption>() });
            return mock.Object;
        }

        private static IRandomService CreateMockRandomService()
        {
            var mock = new Mock<IRandomService>();
            mock.Setup(x => x.NextDouble()).Returns(0.5);
            return mock.Object;
        }
    }
}