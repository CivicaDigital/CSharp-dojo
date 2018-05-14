using System;

namespace BanksySan.Workshops.AdvancedCSharp.MemoryManagement
{
    using static Console;

    internal class DisposableType : IDisposable
    {
        private readonly string _tag;

        public DisposableType(string tag)
        {
            _tag = tag;
            WriteLine($"Created '{_tag}'.");
        }

        public void Dispose()
        {
            WriteLine($"Disposing '{_tag}'.");
        }
    }

    internal static class DisposableTypes
    {
        private static void Main()
        {
            using(var disposableType1 = new DisposableType("1"))
            using(var disposableType2 = new DisposableType("2"))
            {
                WriteLine($"Doing some work...");
            }
        }
    }
}