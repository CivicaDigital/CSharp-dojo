using System;
using System.Threading;

namespace BanksySan.Workshops.AdvancedCSharp.ThreadingExamples
{
    using static Console;

    internal static class ThreadJoining
    {
        private static void Main()
        {
            var thread = new Thread(Counter) { IsBackground = true };
            thread.Start();
            for(var i = 0; i < 5; i++)
            {
                WriteLine($"{Thread.CurrentThread.ManagedThreadId}: Working on iteration {i}.");
                Thread.Sleep(20);
            }

            WriteLine($"{Thread.CurrentThread.ManagedThreadId}: Finished! waiting...");
            thread.Join();
            WriteLine($"{Thread.CurrentThread.ManagedThreadId}: All done.");
        }

        private static void Counter()
        {
            for (var i = 0; i < 10; i++)
            {
                WriteLine($"{Thread.CurrentThread.ManagedThreadId}: Working on iteration {i}.");
                Thread.Sleep(20);
            }

            WriteLine($"{Thread.CurrentThread.ManagedThreadId}: Finished!");
        }
    }
}