namespace BanksySan.Workshops.AdvancedCSharp.ThreadingExamples
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class NoLockAtAll
    {
        private static void Main()
        {
            var threadCount = 10;
            var tasks = new Task<int>[threadCount];

            for (var i = 0; i < threadCount; i++) tasks[i] = new Task<int>(PerformTransactions);

            for (var i = 0; i < tasks.Length; i++) tasks[i].Start();

            var results = Task.WhenAll(tasks);

            var balance = 0;

            foreach (var result in results.Result) balance += result;

            Console.WriteLine($"Balance = {balance}.");
        }

        private static int PerformTransactions()
        {
            var balance = 0;

            for (var i = 0; i < 10000; i++)
            {
                balance += 1;
                Thread.Sleep(0);
                balance -= 1;
                Thread.Sleep(0);
            }

            return balance;
        }
    }
}