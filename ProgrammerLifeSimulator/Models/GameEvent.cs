using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProgrammerLifeSimulator.Models;

public class GameEvent
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "general";
    public string Rarity { get; set; } = "Common";
    public bool IsPassive { get; set; }
    public bool AllowRepeat { get; set; }
    public int Weight { get; set; } = 1;
    public List<string> Tags { get; set; } = new();
    public EventRequirement? Requirement { get; set; }
    public EventOption? PassiveEffect { get; set; }
    public List<EventOption> Options { get; set; } = new();
}

public class EventOption
{
    public string Text { get; set; } = string.Empty;
    public string EffectDescription { get; set; } = string.Empty;
    public int ProgrammingSkillDelta { get; set; }
    public int AlgorithmSkillDelta { get; set; }
    public int DebuggingSkillDelta { get; set; }
    public int CommunicationSkillDelta { get; set; }
    public int StressDelta { get; set; }
    public int HealthDelta { get; set; }
    public int MotivationDelta { get; set; }
    public int SalaryDelta { get; set; }
    public int LeadershipDelta { get; set; }
    public int InnovationDelta { get; set; }
    public bool UnlocksRareEvent { get; set; }
    public bool UnlocksCosmicInsight { get; set; }

    [JsonIgnore]
    public string ImpactSummary => BuildImpactSummary();

    private string BuildImpactSummary()
    {
        var parts = new List<string>();
        Append(parts, "编程", ProgrammingSkillDelta);
        Append(parts, "算法", AlgorithmSkillDelta);
        Append(parts, "调试", DebuggingSkillDelta);
        Append(parts, "沟通", CommunicationSkillDelta);
        Append(parts, "压力", StressDelta);
        Append(parts, "健康", HealthDelta);
        Append(parts, "激励", MotivationDelta);
        Append(parts, "薪资", SalaryDelta);
        Append(parts, "晋升", LeadershipDelta);
        Append(parts, "创想", InnovationDelta);

        return parts.Count == 0 ? "微弱影响" : string.Join(" / ", parts);
    }

    private static void Append(ICollection<string> parts, string label, int delta)
    {
        if (delta == 0) return;
        var sign = delta > 0 ? "+" : string.Empty;
        parts.Add($"{label}{sign}{delta}");
    }
}

public class EventRequirement
{
    public int? MinMonth { get; set; }
    public int? MaxMonth { get; set; }
    public int? MinStress { get; set; }
    public int? MaxStress { get; set; }
    public int? MinHealth { get; set; }
    public int? MaxHealth { get; set; }
    public int? MinSkillTotal { get; set; }

    public bool IsSatisfied(Player player, int month)
    {
        if (MinMonth.HasValue && month < MinMonth.Value) return false;
        if (MaxMonth.HasValue && month > MaxMonth.Value) return false;
        if (MinStress.HasValue && player.Stress < MinStress.Value) return false;
        if (MaxStress.HasValue && player.Stress > MaxStress.Value) return false;
        if (MinHealth.HasValue && player.Health < MinHealth.Value) return false;
        if (MaxHealth.HasValue && player.Health > MaxHealth.Value) return false;
        if (MinSkillTotal.HasValue && TotalSkills(player) < MinSkillTotal.Value) return false;

        return true;
    }

    private static int TotalSkills(Player player)
    {
        return player.ProgrammingSkill + player.AlgorithmSkill + player.DebuggingSkill + player.CommunicationSkill;
    }
}

