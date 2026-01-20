namespace ProgrammerLifeSimulator.Services;

public interface IRandomService
{
    int Next(int max);
    double NextDouble();
}