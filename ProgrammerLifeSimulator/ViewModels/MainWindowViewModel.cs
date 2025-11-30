using ProgrammerLifeSimulator.Models;
using ProgrammerLifeSimulator.Services;

namespace ProgrammerLifeSimulator.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase? _currentView;
    
    // 注入 Services
    private readonly IGameEngineService _gameEngineService;
    private readonly IRandomService _randomService;

    // 构造函数接受注入的依赖
    public MainWindowViewModel(IGameEngineService gameEngineService, IRandomService randomService)
    {
        _gameEngineService = gameEngineService;
        _randomService = randomService;
        // 角色创建页面不需要 Services，但需要导航能力
        CurrentView = new CharacterCreationViewModel(this);
    }
    
    public ViewModelBase? CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }
    
    // 关导航时，将 Services 传递给 GameViewModel
    public void NavigateToGame(Player player)
    {
        // GameViewModel 通过构造函数接收所有依赖
        CurrentView = new GameViewModel(player, _gameEngineService, _randomService);
    }
}