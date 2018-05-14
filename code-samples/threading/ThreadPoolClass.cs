using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BanksySan.Workshops.AdvancedCSharp.ThreadingExamples
{
    using static Console;

    internal static class ThreadPoolClass
    {
        private const int THREAD_COUNT = 100;
        private static readonly bool[] COMPLETION = new bool[THREAD_COUNT];
        private static readonly ConcurrentStack<int> USAGE = new ConcurrentStack<int>();

        private static void Main()
        {
            for (var i = 0; i < THREAD_COUNT; i++)
            {
                WriteLine($"Queueing {i}.");
                ThreadPool.QueueUserWorkItem(Counter, i);
            }

            while (!COMPLETION.All(x => x)) Thread.Sleep(0);

            var threadUsages = new SortedDictionary<int, int>();

            foreach (var usage in USAGE)
                if (threadUsages.ContainsKey(usage))
                    threadUsages[usage] = threadUsages[usage] + 1;
                else
                    threadUsages.Add(usage, 1);

            WriteLine($"Thread reuse:");
            foreach(var usage in threadUsages) WriteLine($"Thread {usage.Key} used {usage.Value} times.");
        }

        private static void Counter(object state)
        {
            var index = (int) state;
            USAGE.Push(Thread.CurrentThread.ManagedThreadId);

            for (var i = 0; i < 10; i++)
            {
                WriteLine($"Thread ID: {Thread.CurrentThread.ManagedThreadId}: {i}");
                Thread.Sleep(1);
            }
            COMPLETION[index] = true;
        }
    }
}