using CommunityToolkit.Mvvm.Input;
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
    
    // --- 新增：返回角色创建界面的命令 ---
    [RelayCommand]
    public void NavigateToSetup()
    {
        // 重新实例化创建页面，传入 this 以便后续再次导航到游戏
        CurrentView = new CharacterCreationViewModel(this);
    }
}