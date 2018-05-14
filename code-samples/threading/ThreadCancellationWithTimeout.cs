namespace BanksySan.Workshops.AdvancedCSharp.ThreadingExamples
{
    using System;
    using System.Threading;

    internal static class ThreadCancellationWithTimeout
    {
        private static void Main()
        {
            var cancellationTokenSource = new CancellationTokenSource(50);
            var cancellationToken = cancellationTokenSource.Token;
            ThreadPool.QueueUserWorkItem(x => Counter(cancellationToken));
            ThreadPool.QueueUserWorkItem(x => CounterWithThrow(cancellationToken));
            Thread.Sleep(100);
            Console.WriteLine("The threads should have canceled by now.");
        }

        private static void Counter(CancellationToken cancellationToken)
        {
            Console.WriteLine($"Counter running on {Thread.CurrentThread.ManagedThreadId}.");
            while (!cancellationToken.IsCancellationRequested)
            {
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}:  {DateTime.Now.Ticks}");
                Thread.Sleep(1);
            }

            Console.WriteLine("Counter() was canceled");
        }

        private static void CounterWithThrow(CancellationToken cancellationToken)
        {
            Console.WriteLine($"CounterWithThrow running on {Thread.CurrentThread.ManagedThreadId}.");

            try
            {
                while (true)
                {
                    Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}:  {DateTime.Now.Ticks}");
                    cancellationToken.ThrowIfCancellationRequested();
                    Thread.Sleep(1);
                }

            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}: Caught OperationCanceledException: {e.Message}");
            }

        }
    }
}