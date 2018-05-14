using System;

namespace BanksySan.Workshops.AdvancedCSharp.ThreadingExamples
{
    using static Console;

    internal static class GetEnvironmentStuff
    {
        private static void Main()
        {
            WriteLine($"Processor count: {Environment.ProcessorCount}.");
        }
    }
}