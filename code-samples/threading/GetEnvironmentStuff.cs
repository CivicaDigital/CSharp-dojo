namespace BanksySan.Workshops.AdvancedCSharp.ThreadingExamples
{
    using System;
    using static System.Console;

    internal static class GetEnvironmentStuff
    {
        private static void Main()
        {
            WriteLine($"Processor count: {Environment.ProcessorCount}.");
        }
    }
}