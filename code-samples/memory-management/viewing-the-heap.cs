namespace BanksySan.Workshops.AdvancedCSharp.MemoryManagement
{
    using System;
    using static System.Console;

    internal static class Program
    {
        private static void Main()
        {
            const int i = 12345;
            const string s = "Hello Dave!";
            WriteLine($"Generation of i: {GC.GetGeneration(i)}");
            WriteLine($"Generation of s: {GC.GetGeneration(s)}");
            WriteLine($"Force garbage collection");
            GC.Collect();
            WriteLine($"Generation of i: {GC.GetGeneration(i)}");
            WriteLine($"Generation of s {GC.GetGeneration(s)}");
            WriteLine($"Force garbage collection"); 
            GC.Collect();
            WriteLine($"Generation of i: {GC.GetGeneration(i)}");
            WriteLine($"Generation of s: {GC.GetGeneration(s)}");
            WriteLine($"Force garbage collection"); 
            GC.Collect();
            WriteLine($"Generation of i: {GC.GetGeneration(i)}");
            WriteLine($"Generation of s: {GC.GetGeneration(s)}");
        }
    }
}