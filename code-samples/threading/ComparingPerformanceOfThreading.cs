using System;
using System.Diagnostics;
using System.Threading;

namespace BanksySan.Workshops.AdvancedCSharp.ThreadingExamples
{
    using static Console;

    internal static class ComparingPerformanceOfThreading
    {
        private static readonly Random RANDOM = new Random();

        private static void Main()
        {
            var stopwatchSynchronous = new Stopwatch();
            var stopwatchAsynchronous = new Stopwatch();
            const int ITERATIONS = 10;

            var result1 = RANDOM.Next();
            var result2 = RANDOM.Next();
            
            stopwatchSynchronous.Start();
            for (var i = 0; i < ITERATIONS; i++)
                WriteRandom();
            stopwatchSynchronous.Stop();

            stopwatchAsynchronous.Start();
            var threads = new Thread[ITERATIONS];
            
            for (var i = 0; i < ITERATIONS; i++)
            {
                var thread = new Thread(WriteRandom);
                threads[i] = thread;
                thread.Start();
            }

            Array.ForEach(threads, t => t.Join());

            WriteLine($"Synchronous: {stopwatchSynchronous.ElapsedTicks}");
            WriteLine($"Asynchronous: {stopwatchAsynchronous.ElapsedTicks}");
        }

        private static void WriteRandom()
        {            
            WriteLine($"{Thread.CurrentThread.ManagedThreadId} Random: {RANDOM.Next()}");
            WriteLine($"{Thread.CurrentThread.ManagedThreadId} Random: {RANDOM.Next()}");
            WriteLine($"{Thread.CurrentThread.ManagedThreadId} Random: {RANDOM.Next()}");
            WriteLine($"{Thread.CurrentThread.ManagedThreadId} Random: {RANDOM.Next()}");
            WriteLine($"{Thread.CurrentThread.ManagedThreadId} Random: {RANDOM.Next()}");
            WriteLine($"{Thread.CurrentThread.ManagedThreadId} Random: {RANDOM.Next()}");
        }
    }
}