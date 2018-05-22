namespace BanksySan.Workshops.AdvancedCSharp.PerformanceTesting
{
    using System.Threading;
    using BenchmarkDotNet.Attributes;

    public class LimitedLockTests
    {
        private const int ITERATIONS = 1000;
        private int _balance;
        private Thread[] _threads;
        private readonly object _lock = new object();

        [Params(2, 4, 8, 16, 32)] public int ThreadCount { get; set; }

        [Benchmark]
        public int Tests()
        {
            _threads = new Thread[ThreadCount];
            for (var i = 0; i < _threads.Length; i++) _threads[i] = new Thread(ProcessTransaction);

            for (var i = 0; i < _threads.Length; i++) _threads[i].Start();

            for (var i = 0; i < _threads.Length; i++) _threads[i].Join();

            return _balance;
        }


        private void ProcessTransaction()
        {
            var balance = 0;

            for (var i = 0; i < ITERATIONS; i++)
            {
                balance += 1;
                Thread.Sleep(0);
                balance -= 1;
                Thread.Sleep(0);
            }

            lock (_lock)
                _balance += balance;
        }
    }
}