namespace BanksySan.Workshops.AdvancedCSharp.ThreadingExamples
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using static System.Console;

    internal static class ThreadStarvation
    {
        private static void Main()
        {
            var processorCount = Environment.ProcessorCount;

            WriteLine($"There are {processorCount} processors.");
            WriteLine($"There can be {processorCount} threads running at the same time.");

            var aboveNormalPriorityThreads = new HashSet<Thread>();
            
            for (var i = 0; i < processorCount; i ++) aboveNormalPriorityThreads.Add(new Thread(Counter) { Priority = ThreadPriority.AboveNormal });

            var normalPriorityThread = new Thread(Counter);
            normalPriorityThread.Start();
            WriteLine($"Letting the normal priority thread run for a bit");
            Thread.Sleep(1000);
            foreach(var thread in aboveNormalPriorityThreads)
            {
                WriteLine($"Starting {thread.ManagedThreadId} with priority {thread.Priority}.");
                thread.Start();
            }
        }

        private static void Counter()
        {
            var currentThread = Thread.CurrentThread;
            var priority = currentThread.Priority;
            var id = currentThread.ManagedThreadId;

            for (var i = 0; i < 5; i++)
            {
                
                var then = DateTime.Now;
                var number = then.GetHashCode();
                while (DateTime.Now < then.AddSeconds(1)) number ^= number.GetHashCode();

                WriteLine($"Thread ID: {id}, Priority: {priority, -15}{i, 10}{number}");
            }

            WriteLine($"Thread ID: {id}, Priority: {priority, -15}COMPLETED.");
        }
    }
}