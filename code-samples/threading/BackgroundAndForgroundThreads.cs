namespace BanksySan.Workshops.AdvancedCSharp.ThreadingExamples
{
    using System.Threading;
    using static System.Console;

    internal static class BackgroundAndForegroundThreads
    {
        private static void Main()
        {
            var backgroundThread = new Thread(Counter)
            {
                IsBackground = true
            };

            var foregroundThread = new Thread(Counter);

            WriteLine($"Starting both threads.");
            backgroundThread.Start();
            foregroundThread.Start();
            Thread.Sleep(20);
            WriteLine("We'll kill the foreground thread and the application will exit...");
            foregroundThread.Abort();

        }

        private static void Counter()
        {
            while (true)
            {
                WriteLine($"Thread ID: {Thread.CurrentThread.ManagedThreadId}, Is Background: {Thread.CurrentThread.IsBackground}.");
                Thread.Sleep(5);
            }
        }
    }
}