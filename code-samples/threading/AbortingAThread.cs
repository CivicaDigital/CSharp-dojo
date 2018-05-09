namespace BanksySan.Workshops.AdvancedCSharp.ThreadingExamples
{
    using static System.Console;
    using System.Threading;
    
    static class AbortingAThread
    {
        private static void Main()
        {
            var thread = new Thread(Counter);
            thread.Start();
            Thread.Sleep(50);
            thread.Abort("I'm aborting you.");
        }
        private static void Counter()
        {
            try
            {
                var i = 0;
                while(true)
                {
                    WriteLine($"{Thread.CurrentThread.ManagedThreadId}: {i}");
                    Thread.Sleep(10);
                }
            }
            catch(ThreadAbortException e)
            {
                WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}:  I caught the `ThreadAbortedException` exception: {e.Message};");
                WriteLine($"The object data is: {e.ExceptionState}");
            }
        }
    }
}