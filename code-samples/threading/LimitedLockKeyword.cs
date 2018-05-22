﻿namespace BanksySan.Workshops.AdvancedCSharp.ThreadingExamples
{
    using System;
    using System.Threading;

    internal static class LimitedLockKeyword
    {
        private static int _BALANCE;
        private static readonly object LOCK = new object();

        private static void Main()
        {
            var threadCount = 10;
            var threads = new Thread[threadCount];

            for (var i = 0; i < threadCount; i++)
            {
                var thread = new Thread(PerformTransactions);
                thread.Start();
                threads[i] = thread;
            }

            for (var i = 0; i < threadCount; i++) threads[i].Join();

            Console.WriteLine($"Balance = {_BALANCE}.");
        }

        private static void PerformTransactions(object state)
        {
            var balance = 0;

            for (var i = 0; i < 10000; i++)
            {
                balance += 1;
                Thread.Sleep(0);
                balance-= 1;
                Thread.Sleep(0);
            }

            lock (LOCK)
            {
                _BALANCE += balance;
            }
        }
    }
}