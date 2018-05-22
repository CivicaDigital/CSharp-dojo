namespace BanksySan.Workshops.AdvancedCSharp.PerformanceTesting
{
    using BenchmarkDotNet.Running;

    internal static class Program
    {
        private static void Main()
        {
            BenchmarkRunner.Run<UnsynchronisationTests>();
            BenchmarkRunner.Run<ManyLocksynchronisationTests>();
            BenchmarkRunner.Run<LimitedLockTests>();
        }
    }
}
