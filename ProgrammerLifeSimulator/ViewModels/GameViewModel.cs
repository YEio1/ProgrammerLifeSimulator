using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.Input;
using ProgrammerLifeSimulator.Models;
using ProgrammerLifeSimulator.Services;

namespace ProgrammerLifeSimulator.ViewModels;

public class GameViewModel : ViewModelBase
{
    private const int MonthsPerYear = 12;
    private const int TotalMonthsToPlay = 36;
    private const double NoveltyBiasChance = 0.45;
    private const double RepeatBiasChance = 0.3;
    private const double RareEventFocusChance = 0.15;
    private const double CosmicEventFocusChance = 0.3;

    // 依赖注入
    private readonly IGameEngineService _gameEngine;
    private readonly IRandomService _randomService; // 用于简单的概率判断

    private readonly IList<GameEvent> _allEvents;
    private readonly List<GameEvent> _playedEvents = new();
    private List<string> _statusWarnings = new();
    private readonly List<string> _recentRandomEvents = new();
    private readonly HashSet<string> _seenEventIds = new();

    private GameEvent? _currentEvent;
    private string _eventResultMessage = string.Empty;
    private GameEnding? _ending;
    private string _lastImpactDetails = string.Empty;

    private int _month;
    private int _eventsCompleted;
    private int _totalStress;
    private int _totalHealth;
    private int _totalMotivation;
    private int _leadershipProgress;
    private int _innovationProgress;
    
    // 这些状态字段需要传给 Service
    private bool _rareEventUnlocked;
    private bool _cosmicInsightUnlocked;

    // 构造函数：现在接受 IGameEngineService 注入
    public GameViewModel(Player player, IGameEngineService gameEngine, IRandomService randomService)
    {
        Player = player;
        _gameEngine = gameEngine;
        _randomService = randomService;

        _allEvents = MockDataService.GetMockEvents();
        Month = 1;
        SelectOptionCommand = new RelayCommand<EventOption>(SelectOption);
        
        // 调用 Service 获取初始警告
        UpdateStatusWarnings();
        LoadNextEvent();
    }

    public Player Player { get; }
    public IRelayCommand<EventOption> SelectOptionCommand { get; }

    public int Month
    {
        get => _month;
        private set
        {
            if (SetProperty(ref _month, value))
            {
                RaiseTimeProperties();
            }
        }
    }
    
    public string TimeDisplay => $"第 {((Month - 1) / MonthsPerYear) + 1} 年 · 第 {Month} 个月";
    public string TimelineDisplay => $"职业历程：第 {Math.Min(Month, TotalMonthsToPlay)} 个月 / 共 {TotalMonthsToPlay} 个月";
    public double TimelineProgress => TotalMonthsToPlay == 0 ? 0 : Math.Clamp((double)(Month - 1) / TotalMonthsToPlay, 0, 1);
    public string CareerPhase
    {
        get
        {
            if (Month <= 12) return "阶段：新手期－不断吸收与适应";
            if (Month <= 24) return "阶段：成长期－积累经验与影响力";
            if (Month <= 36) return "阶段：突破期－冲刺个人品牌";
            return "阶段：传奇篇章";
        }
    }

    public GameEvent? CurrentEvent
    {
        get => _currentEvent;
        private set => SetProperty(ref _currentEvent, value);
    }

    public string EventResultMessage
    {
        get => _eventResultMessage;
        private set => SetProperty(ref _eventResultMessage, value);
    }

    public bool IsGameCompleted => Ending is not null;

    public GameEnding? Ending
    {
        get => _ending;
        private set
        {
            if (SetProperty(ref _ending, value))
            {
                OnPropertyChanged(nameof(EndingTitle));
                OnPropertyChanged(nameof(GameSummary));
                OnPropertyChanged(nameof(IsGameCompleted));
            }
        }
    }

    public string EndingTitle => Ending?.Title ?? string.Empty;
    public string LastImpactDetails { get => _lastImpactDetails; private set => SetProperty(ref _lastImpactDetails, value); }

    public int LeadershipProgress
    {
        get => _leadershipProgress;
        private set
        {
            var clamped = Math.Clamp(value, 0, 120);
            if (SetProperty(ref _leadershipProgress, clamped)) OnPropertyChanged(nameof(GoalProgressSummary));
        }
    }

    public int InnovationProgress
    {
        get => _innovationProgress;
        private set
        {
            var clamped = Math.Clamp(value, 0, 120);
            if (SetProperty(ref _innovationProgress, clamped)) OnPropertyChanged(nameof(GoalProgressSummary));
        }
    }

    public string GoalProgressSummary => $"晋升进度 {LeadershipProgress}/100 · 创业灵感 {InnovationProgress}/100";
    public IReadOnlyList<string> StatusWarnings => _statusWarnings;
    public IReadOnlyList<string> RecentRandomEvents => _recentRandomEvents;
    public bool HasRandomHighlights => _recentRandomEvents.Count > 0;
    
    public string GameSummary
    {
        get
        {
            if (!IsGameCompleted || Ending is null) return string.Empty;
            var monthsPlayed = Math.Min(Month - 1, TotalMonthsToPlay);
            return $"{Ending.Description}\n\n职业历程：共 {monthsPlayed} 个月..."; // 省略部分显示代码，保持原样即可
        }
    }

    private void LoadNextEvent()
    {
        if (Month > TotalMonthsToPlay)
        {
            CompleteGame();
            return;
        }

        TryTriggerAmbientEvent();

        var availableEvents = GetEligibleEvents(includePassive: false);
        if (availableEvents.Count == 0)
        {
            _playedEvents.Clear();
            availableEvents = GetEligibleEvents(includePassive: false);
        }

        if (availableEvents.Count == 0)
        {
            CompleteGame();
            return;
        }

        var candidate = ApplyEventBiases(availableEvents);

        // 调用 Service 进行权重选择
        var selectedEvent = _gameEngine.SelectWeightedEvent(candidate, Player, _rareEventUnlocked, _cosmicInsightUnlocked, _seenEventIds, Month);

        if (!selectedEvent.AllowRepeat)
        {
            _playedEvents.Add(selectedEvent);
        }

        if (!string.IsNullOrWhiteSpace(selectedEvent.Id))
        {
            _seenEventIds.Add(selectedEvent.Id);
        }

        CurrentEvent = selectedEvent;
        OnPropertyChanged(nameof(GameSummary));
    }

    private void SelectOption(EventOption? option)
    {
        if (option is null || IsGameCompleted) return;
        
        int tempLeadership = LeadershipProgress;
        int tempInnovation = InnovationProgress;
        
        _gameEngine.ApplyOptionEffects(Player, option, ref tempLeadership, ref tempInnovation, ref _rareEventUnlocked, ref _cosmicInsightUnlocked);
        
        // 更新回 ViewModel 属性以触发 UI 通知
        LeadershipProgress = tempLeadership;
        InnovationProgress = tempInnovation;

        EventResultMessage = option.EffectDescription;
        _eventsCompleted++;
        _totalStress += Player.Stress;
        _totalHealth += Player.Health;
        _totalMotivation += Player.Motivation;

        Month++;
        Player.Age = 22 + ((Month - 1) / MonthsPerYear);

        LastImpactDetails = BuildImpactDetails(option);
        UpdateStatusWarnings();
        LoadNextEvent();
    }

    private void CompleteGame()
    {
        CurrentEvent = null;
        EventResultMessage = string.Empty;
        
        //  调用 Service 判断结局
        var avgStress = _eventsCompleted == 0 ? Player.Stress : _totalStress / _eventsCompleted;
        var avgHealth = _eventsCompleted == 0 ? Player.Health : _totalHealth / _eventsCompleted;
        var avgMotivation = _eventsCompleted == 0 ? Player.Motivation : _totalMotivation / _eventsCompleted;

        Ending = _gameEngine.CheckForEnding(Player, _eventsCompleted, avgStress, avgHealth, avgMotivation, LeadershipProgress, InnovationProgress, _cosmicInsightUnlocked);
    }

    private void RaiseTimeProperties()
    {
        OnPropertyChanged(nameof(TimeDisplay));
        OnPropertyChanged(nameof(TimelineDisplay));
        OnPropertyChanged(nameof(TimelineProgress));
        OnPropertyChanged(nameof(CareerPhase));
    }

    private IList<GameEvent> ApplyEventBiases(IList<GameEvent> pool)
    {
        if (pool.Count == 0) return pool;

        var candidate = pool;
        var unseenPool = pool.Where(e => string.IsNullOrWhiteSpace(e.Id) || !_seenEventIds.Contains(e.Id)).ToList();
        
        if (unseenPool.Count > 0 && _randomService.NextDouble() < NoveltyBiasChance)
        {
            candidate = unseenPool;
        }
        else if (_randomService.NextDouble() < RepeatBiasChance)
        {
            var repeatPool = pool.Where(e => e.AllowRepeat).ToList();
            if (repeatPool.Count > 0) candidate = repeatPool;
        }
        
        return candidate.Count == 0 ? pool : candidate;
    }

    private List<GameEvent> GetEligibleEvents(bool includePassive)
    {
        return _allEvents
            .Where(e => e.IsPassive == includePassive)
            .Where(gameEvent => gameEvent.Requirement?.IsSatisfied(Player, Month) ?? true)
            .Where(e => e.AllowRepeat || !_playedEvents.Contains(e))
            .ToList();
    }

    private void TryTriggerAmbientEvent()
    {
        var ambientEvents = GetEligibleEvents(includePassive: true);
        if (ambientEvents.Count == 0) return;

        var baseChance = 0.15; 
        
        if (_randomService.NextDouble() > baseChance) return; 
        
        var selected = _gameEngine.SelectWeightedEvent(ambientEvents, Player, _rareEventUnlocked, _cosmicInsightUnlocked, _seenEventIds, Month);
        
        if (!selected.AllowRepeat) _playedEvents.Add(selected);
        
        if (selected.PassiveEffect != null)
        {
            _recentRandomEvents.Insert(0, selected.Title + ": " + selected.PassiveEffect.EffectDescription);
            while (_recentRandomEvents.Count > 3) // 只保留最近的3条
            {
                _recentRandomEvents.RemoveAt(_recentRandomEvents.Count - 1);
            }
            OnPropertyChanged(nameof(RecentRandomEvents));
            OnPropertyChanged(nameof(HasRandomHighlights));
        }
    }

    private void UpdateStatusWarnings()
    {
        // 调用 Service 获取警告
        var warnings = _gameEngine.GetStatusWarnings(Player);
        
        _statusWarnings = warnings;
        OnPropertyChanged(nameof(StatusWarnings));
    }

    private string BuildImpactDetails(EventOption option)
    {
        return option.ImpactSummary switch
        {
            "微弱影响" => "本次事件对整体属性影响轻微。",
            _ => $"本次影响：{option.ImpactSummary}"
        };
    }
}