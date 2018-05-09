namespace BanksySan.Workshops.AdvancedCSharp.ThreadingExamples
{
    using System;
    using System.Threading;
    using static System.Console;

    internal static class ExceptionInThread
    {
        private static void Main()
        {
            try
            {
                var thread = new Thread(ThrowsDummyException);
                thread.Start();
                while (true)
                {
                    WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId} is working hard...");
                    Thread.Sleep(10);
                }
            }
            catch (System.Exception)
            {
                System.Console.WriteLine($"I caught the exception.");
            }

        }

        private static void ThrowsDummyException()
        {
            var timeSpan = TimeSpan.FromMilliseconds(100);

            WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId}: Waiting {timeSpan.TotalSeconds} seconds to throw.");
            Thread.Sleep(timeSpan);
            throw new Exception("BOOM!");
        }
    }
}