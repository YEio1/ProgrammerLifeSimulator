using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using ProgrammerLifeSimulator.Models;
using ProgrammerLifeSimulator.Services;

namespace ProgrammerLifeSimulator.ViewModels;

public class CharacterCreationViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _navigation;
    private string _playerName = string.Empty;
    private Trait? _selectedTrait;

    public CharacterCreationViewModel(MainWindowViewModel navigation)
    {
        _navigation = navigation;
        AvailableTraits = MockDataService.GetAvailableTraits();
        _selectedTrait = AvailableTraits.FirstOrDefault();
        StartGameCommand = new RelayCommand(StartGame, CanStartGame);
    }

    public IReadOnlyList<Trait> AvailableTraits { get; }

    public string PlayerName
    {
        get => _playerName;
        set
        {
            if (SetProperty(ref _playerName, value))
            {
                StartGameCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public Trait? SelectedTrait
    {
        get => _selectedTrait;
        set
        {
            if (SetProperty(ref _selectedTrait, value))
            {
                StartGameCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public IRelayCommand StartGameCommand { get; }

    private bool CanStartGame() => !string.IsNullOrWhiteSpace(PlayerName) && SelectedTrait != null;

    private void StartGame()
    {
        if (SelectedTrait is null)
        {
            return;
        }

        var player = MockDataService.CreateBasePlayer(PlayerName);
        player.ApplyTrait(SelectedTrait);
        _navigation.NavigateToGame(player);
    }
}
