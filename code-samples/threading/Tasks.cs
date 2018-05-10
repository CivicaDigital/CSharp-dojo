namespace BanksySan.Workshops.AdvancedCSharp.ThreadingExamples
{
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using static System.Console;
    using System.Threading.Tasks;

    static class TaskClasses
    {
        private const int THREAD_COUNT = 100;
        private static readonly bool[] COMPLETION = new bool[THREAD_COUNT];
        private static readonly ConcurrentStack<int> USAGE = new ConcurrentStack<int>();
        
        private static void Main()
        {
            var tasks = new Task[THREAD_COUNT];
            
            for (var i = 0; i < THREAD_COUNT; i++)
            {
                var task = new Task(Counter);
                tasks[i] = task;
                task.Start();
            }
            
            Task.WaitAll(tasks);

            var threadUsages = new SortedDictionary<int, int>();

            foreach (var usage in USAGE)
            {
                if (threadUsages.ContainsKey(usage))
                {
                    threadUsages[usage] = threadUsages[usage] + 1;
                }
                else
                {
                    threadUsages.Add(usage, 1);
                }
            }

            WriteLine($"Thread reuse:");
            foreach(var usage in threadUsages)
            {
                WriteLine($"Thread {usage.Key} used {usage.Value} times.");
            }    
        }

        private static void Counter()
        {
            USAGE.Push(Thread.CurrentThread.ManagedThreadId);

            for (var i = 0; i < 10; i++)
            {
                WriteLine($"Thread ID: {Thread.CurrentThread.ManagedThreadId}: {i}");
                Thread.Sleep(1);
            }
        }
    }
}