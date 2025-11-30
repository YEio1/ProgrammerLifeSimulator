using ProgrammerLifeSimulator.Services;

namespace ProgrammerLifeSimulator.UnitTest.Mocks;

public class PredictableRandomService : IRandomService
{
    private readonly Queue<int> _nextValues;
    private readonly Queue<double> _nextDoubleValues;

    // 构造函数接收预设的返回值序列
    public PredictableRandomService(IEnumerable<int> nextValues, IEnumerable<double> nextDoubleValues)
    {
        _nextValues = new Queue<int>(nextValues);
        _nextDoubleValues = new Queue<double>(nextDoubleValues);
    }

    public int Next(int max) => _nextValues.TryDequeue(out var result) ? result : 0;
    public double NextDouble() => _nextDoubleValues.TryDequeue(out var result) ? result : 0.0;
}