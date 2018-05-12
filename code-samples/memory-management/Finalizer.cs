namespace BanksySan.Workshops.AdvancedCSharp.MemoryManagement
{
    using System;
    using static System.Console;

    internal class FinalisableType
    {
        ~FinalisableType()
        {
            WriteLine("Finalized");
        }
    }

    static class FinalizableExample
    {
        private static void Main()
        {
            GC.RegisterForFullGCNotification(2, 2);
            var finalizableType = new FinalisableType();
            WriteLine($"Created finalizableType.");
            WriteLine("Calling GC.Collect() but keeping finalizableType reachable.");
            GC.Collect();
            GC.KeepAlive(finalizableType);
            WriteLine("Removing reference to finalizableType.");
            finalizableType = null;
            WriteLine("Calling GC.Collect().");
            GC.Collect();
            WriteLine("Finalizer hasn't been called yet, the finalizableType has been moved to the FReachable queue.");
            WriteLine("Calling GC.Collect() again.");
            GC.Collect();
            WriteLine("Finalization should have finished.");
        }
    }
}