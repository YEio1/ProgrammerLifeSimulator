using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProgrammerLifeSimulator.Models;

public class Player : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private int _age;
    private int _programmingSkill;
    private int _algorithmSkill;
    private int _debuggingSkill;
    private int _communicationSkill;
    private int _stress;
    private int _health;
    private int _motivation;
    private int _salary;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public int Age
    {
        get => _age;
        set => SetProperty(ref _age, value);
    }

    public int ProgrammingSkill
    {
        get => _programmingSkill;
        set => SetProperty(ref _programmingSkill, value);
    }

    public int AlgorithmSkill
    {
        get => _algorithmSkill;
        set => SetProperty(ref _algorithmSkill, value);
    }

    public int DebuggingSkill
    {
        get => _debuggingSkill;
        set => SetProperty(ref _debuggingSkill, value);
    }

    public int CommunicationSkill
    {
        get => _communicationSkill;
        set => SetProperty(ref _communicationSkill, value);
    }

    public int Stress
    {
        get => _stress;
        set => SetProperty(ref _stress, value);
    }

    public int Health
    {
        get => _health;
        set => SetProperty(ref _health, value);
    }

    public int Motivation
    {
        get => _motivation;
        set => SetProperty(ref _motivation, value);
    }

    public int Salary
    {
        get => _salary;
        set => SetProperty(ref _salary, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    public void ApplyTrait(Trait trait)
    {
        ProgrammingSkill += trait.ProgrammingSkillBonus;
        AlgorithmSkill += trait.AlgorithmSkillBonus;
        DebuggingSkill += trait.DebuggingSkillBonus;
        CommunicationSkill += trait.CommunicationSkillBonus;
        Stress = Math.Max(0, Stress + trait.StressDelta);
        Health = Math.Clamp(Health + trait.HealthDelta, 0, 100);
        Motivation = Math.Clamp(Motivation + trait.MotivationDelta, 0, 100);
    }
}

