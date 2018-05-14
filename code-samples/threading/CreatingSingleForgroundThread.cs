namespace BanksySan.Workshops.AdvancedCSharp.ThreadingExamples
{
    using System.Threading;
    using static System.Console;

    internal static class CreatingSingleForgroundThread
    {
        private static void Main()
        {
            WriteLine($"Main managed thread ID: {Thread.CurrentThread.ManagedThreadId}.");
            var thread = new Thread(Counter);
            WriteLine($"Created thread.  Managed thread ID: {thread.ManagedThreadId}.");
            thread.Start();
            WriteLine($"Thread started.");
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