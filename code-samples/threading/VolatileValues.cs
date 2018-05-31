namespace BanksySan.Workshops.AdvancedCSharp.ThreadingExamples
{
    using System;
    using System.Threading;

    internal static class VolitileValues
    {
        private const int TERMINATING_COUNT = 10;
        private static int _COUNT;

        private static void Main()
        {
            var checker = new Thread(Checker);
            var stopper = new Thread(Counter);
            checker.Start();
            Thread.Sleep(10);
            stopper.Start();
            var timeout = TimeSpan.FromSeconds(1);
            _COUNT = 1;
            Console.WriteLine($"Waiting for {timeout} worker to stop.");
            checker.Join(timeout);
            if (checker.IsAlive)
            {
                Console.Error.WriteLine($"Thread failed to stop.  Aborting instead. ");
                checker.Abort();
            }

            Console.WriteLine("Done");
        }

        private static void Counter()
        {
            for (; _COUNT < 21; _COUNT++)
            {
                Console.WriteLine($"Count: {_COUNT}");
                if (_COUNT == TERMINATING_COUNT)
                    Console.WriteLine($"Terminator {TERMINATING_COUNT} reached.");
            }
        }

        private static void Checker()
        {
            var x = 0;
            while (Volatile.Read(ref _COUNT) < TERMINATING_COUNT) x++;
            Console.WriteLine($"{nameof(Checker)} stopped at {x}.");
        }
    }
}