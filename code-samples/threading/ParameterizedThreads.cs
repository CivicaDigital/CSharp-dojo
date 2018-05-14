using System;
using System.Threading;

namespace BanksySan.Workshops.AdvancedCSharp.ThreadingExamples
{
    using static Console;

    internal static class ParameterizedThreads
    {
        private static void Main()
        {
            var thread = new Thread(CountInterval);
            thread.Start("Hello World!");
            thread.Join();
        }

        private static void CountInterval(object message)
        {
            WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}: Message: {(string) message}.");
        }
    }
}