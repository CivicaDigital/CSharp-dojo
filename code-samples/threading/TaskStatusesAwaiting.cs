namespace BanksySan.Workshops.AdvancedCSharp.ThreadingExamples
{
    using System.Threading.Tasks;
    using static System.Console;
    using System;
    using static System.Threading.Thread;
    
    static class TaskStatusesAwaiting
    {
        private static void Main(string[] args)
        {
            WriteLine("Foo called");
            var counter = Task.Run(() => Counter());
            var wrappedCounter = WrappedCounter();

            while (counter.Status != TaskStatus.RanToCompletion || wrappedCounter.Status != TaskStatus.RanToCompletion)
            {
                WriteLine($"Counter Status: {counter.Status}.");
                WriteLine($"Counter Status: {wrappedCounter.Status}.");
                Sleep(10);
            }

            WriteLine("Finished.");
            ReadKey(true);
        }

        private static void Counter()
        {
            var hash = DateTime.Now.Ticks;
            for (var i = 0L; i < 1000000; i++)
            {
                hash = (i ^ hash).ToString().GetHashCode()  ;
            }

            WriteLine($"Finished.  Hash = {hash}");
        }

        private static async Task WrappedCounter()
        {
            await Task.Run(() => Counter());
        }
    }
}