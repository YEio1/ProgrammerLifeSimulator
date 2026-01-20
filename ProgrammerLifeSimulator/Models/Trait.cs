namespace ProgrammerLifeSimulator.Models;

public class Trait
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ProgrammingSkillBonus { get; set; }
    public int AlgorithmSkillBonus { get; set; }
    public int DebuggingSkillBonus { get; set; }
    public int CommunicationSkillBonus { get; set; }
    public int StressDelta { get; set; }
    public int HealthDelta { get; set; }
    public int MotivationDelta { get; set; }
}

