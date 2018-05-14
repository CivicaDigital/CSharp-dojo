namespace BanksySan.Workshops.AdvancedCSharp.MemoryManagement
{
    using System;
    using static System.Console;

    internal class FinalizableType
    {
        ~FinalizableType()
        {
            WriteLine("Finalized");
        }
    }

    internal static class ViewingFReachable
    {
        private static void Main()
        {
            GC.RegisterForFullGCNotification(2, 2);
            var finalizableType = new FinalizableType();
            WriteLine("1 - Check dump");
            ReadKey(true);
            GC.Collect();
            GC.KeepAlive(finalizableType);
            WriteLine("Removing reference to finalizableType.");
            finalizableType = null;
            WriteLine("2 - Check dump");
            ReadKey(true);
            GC.Collect();
            WriteLine("3 - Check dump");
            ReadKey(true);
            GC.Collect();
            WriteLine("4 - Check dump");
            ReadKey(true);
        }
    }
}