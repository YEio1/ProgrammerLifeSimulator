using System;

namespace ProgrammerLifeSimulator.Services;

public class RandomService : IRandomService
{
    private readonly Random _random = new Random();

    public int Next(int max) => _random.Next(max);

    public double NextDouble() => _random.NextDouble();
}