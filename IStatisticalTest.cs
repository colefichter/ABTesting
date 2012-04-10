namespace ABTesting
{
    public interface IStatisticalTest
    {
        double GetPValue(Experiment test);
        bool IsStatisticallySignificant(Experiment test);
        bool IsStatisticallySignificant(Experiment test, double pValue);
        string GetResultDescription(Experiment test);
        string[] AssumptionsToCheck { get; }
    }
}
