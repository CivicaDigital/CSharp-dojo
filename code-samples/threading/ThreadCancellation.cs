﻿namespace BanksySan.Workshops.AdvancedCSharp.ThreadingExamples
{
    using System;
    using System.Threading;
    using static System.Threading.Thread;
    using static System.Console;

    internal static class ThreadCancellation
    {
        private static void Main()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            ThreadPool.QueueUserWorkItem(x => Counter(cancellationToken));
            ThreadPool.QueueUserWorkItem(x => CounterWithThrow(cancellationToken));
            Sleep(10);
            cancellationTokenSource.Cancel();
            Sleep(10);
        }

        private static void Counter(CancellationToken cancellationToken)
        {
            WriteLine($"Counter running on {CurrentThread.ManagedThreadId}.");
            while (!cancellationToken.IsCancellationRequested)
            {
                WriteLine($"Thread {CurrentThread.ManagedThreadId}:  {DateTime.Now.Ticks}");
                Sleep(1);
            }

            WriteLine("Counter() was canceled");
        }

        private static void CounterWithThrow(CancellationToken cancellationToken)
        {
            WriteLine($"CounterWithThrow running on {CurrentThread.ManagedThreadId}.");

            try
            {
                while (true)
                {
                    WriteLine($"Thread {CurrentThread.ManagedThreadId}:  {DateTime.Now.Ticks}");
                    cancellationToken.ThrowIfCancellationRequested();
                    Sleep(1);
                }

            }
            catch (OperationCanceledException e)
            {
                WriteLine($"Thread {CurrentThread.ManagedThreadId}: Caught OperationCanceledException: {e.Message}");
            }
            
        }
    }
}
