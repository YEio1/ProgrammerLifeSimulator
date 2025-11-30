using System.Collections.Generic;
using ProgrammerLifeSimulator.Models;

namespace ProgrammerLifeSimulator.Services;

public interface IGameEngineService
{
    /// 功能1：处理选项带来的数值变化 (逻辑搬运自 ApplyOptionEffects)
    void ApplyOptionEffects(Player player, EventOption option, 
        ref int leadershipProgress, ref int innovationProgress, 
        ref bool rareEventUnlocked, ref bool cosmicInsightUnlocked);
    
    /// 功能2：根据当前状态生成警告列表 (逻辑搬运自 UpdateStatusWarnings)
    List<string> GetStatusWarnings(Player player);
    
    /// 功能3：判断游戏结局 (逻辑搬运自 DetermineEnding)
    GameEnding? CheckForEnding(Player player, int eventsCompleted, int avgStress, int avgHealth, int avgMotivation, 
        int leadershipProgress, int innovationProgress, bool cosmicInsightUnlocked);
    
    /// 功能4：加权随机选择事件 (逻辑搬运自 SelectWeightedEvent)
    GameEvent SelectWeightedEvent(IList<GameEvent> pool, Player player, 
        bool rareEventUnlocked, bool cosmicInsightUnlocked, 
        HashSet<string> seenEventIds, int currentMonth);
}