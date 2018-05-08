namespace BanksySan.Workshops.AdvancedCSharp.ThreadingExamples
{
    using static System.Console;
    using System.Threading;

    static class Program
    {
        private static void Main()
        {
            WriteLine($"Main thread ID: {Thread.CurrentThread.ManagedThreadId}, IsBackground: {Thread.CurrentThread.IsBackground}");
            var thread = new Thread(Counter);

            thread.Start();
            WriteLine($"Thread started");
        }

        private static void Counter()
        {
            for (var i = 0; i < 10; i++)
            {
                WriteLine($"{i}  Counter thread ID: {Thread.CurrentThread.ManagedThreadId}, IsBackground: {Thread.CurrentThread.IsBackground}");
                Thread.Sleep(10);
            }
        }
    }
}