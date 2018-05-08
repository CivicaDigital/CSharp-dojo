namespace BanksySan.Workshops.AdvancedCSharp.ThreadingExamples
{
    using static System.Console;
    using System.Threading;

    static class CreatingMultipleForgroundThreads
    {
        private static void Main()
        {
            WriteLine($"Main managed thread ID: {Thread.CurrentThread.ManagedThreadId}.");
            
            var thread1 = new Thread(Counter);
            WriteLine($"Created thread 1.  Managed thread ID: {thread1.ManagedThreadId}.");
            thread1.Start();
            WriteLine($"Thread 1 started.");

            var thread2 = new Thread(Counter);
            WriteLine($"Created thread 2.  Managed thread ID: {thread2.ManagedThreadId}.");
            thread2.Start();
            WriteLine($"Thread 2 started.");
        }

        private static void Counter()
        {
            for (var i = 0; i < 10; i++)
            {
                WriteLine($"{i}:  Managed thread ID: {Thread.CurrentThread.ManagedThreadId}.");
                Thread.Sleep(100);
            }
        }
    }
}