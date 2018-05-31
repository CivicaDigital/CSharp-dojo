namespace BanksySan.Workshops.AdvancedCSharp.ThreadingExamples
{
    using System;
    using System.Threading;

    internal static class TornReads
    {
        private const ulong NUMBER_1 = ulong.MaxValue;
        private const ulong NUMBER_2 = 0;
        private static ulong _NUMBER = NUMBER_1;
        private static bool @continue = true;

        private static void Main()
        {
            var writerThread = new Thread(Writer);
            var readerThread = new Thread(Reader);

            writerThread.Start();
            readerThread.Start();
            readerThread.Join();
            writerThread.Abort();
            @continue = true;
        }

        private static void Reader()
        {
            for (var i = 0; i < 100; i++)
            {
                var number = _NUMBER;
                if (number != NUMBER_1 && number != NUMBER_2)
                    Console.WriteLine($"{i, 3}: Read: {number:X16} TornRead!");
                else
                    Console.WriteLine($"{i,3}: Read: {number:X16}");
            }
        }

        private static void Writer()
        {
            while (@continue)
            {
                _NUMBER = NUMBER_2;
                _NUMBER = NUMBER_1;
            }
        }
    }
}