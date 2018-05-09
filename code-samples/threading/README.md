# Threading

## Defining a thread

This isn't an easy thing to do, as I discovered when I did a presentation on async JavaScript.  The term _thread_ is used very liberally for similar things.  It seems best to define it on a per-use basis.

1. A process will consist of one or more threads.
1. A thread is the basic unit through which the operating system can assign processor time.
1. A processor can execute one thread at any given moment.
1. A thread runs to completion and is then disposed.

If we had a single processor and no concept of threads then each process would block all the other operations until it had finished.

Threads are an answer to this problem.  The OS will share time on the processor between the treads requesting it.  This means that even if a thread does have an infinite loop the OS will still swap it out so the other threads keep working.

Nothing is ever free though, the management system required to perform threading used resources itself and the algorithm used to decide what thread should get precious processor time isn't perfect.  This is the reason that .NET has friendlier classes available to abstract away some of the ugliness.

## Creating a Thread

> If you create threads directly, then you're code is already obsolete.

Whilst this is true for almost all new development, it's still useful to understand them is only to understand what the problems with them are.

> The managed threads created in C# map one-to-one with Windows threads.  Originally there was an idea that a managed thread would be a thing, it didn't happen though.  This is why you have a thread ID and a managed thread ID.

Creating and running a thread is trivial.  All we need to do is create a method that's assignable to a `ThreadStart` delegate and pass it to the constructor of the `Thread` type.

``` csharp
using static System.Console;
using System.Threading;

static class Program
{
    private static void Main()
    {
        WriteLine($"Main managed thread ID: {Thread.CurrentThread.ManagedThreadId}.");
        var thread = new Thread(Counter);
        WriteLine($"Created thread.  Managed thread ID: {thread.ManagedThreadId}.");
        thread.Start();
        WriteLine($"Thread started.");
    }

    private static void Counter()
    {
        for (var i = 0; i < 10; i++)
        {
            WriteLine($"{i}:  Managed thread ID: {Thread.CurrentThread.ManagedThreadId}.");
            Thread.Sleep(100);
        }
    }
}
```

Compile and run this code.

    Main managed thread ID: 1.
    Created thread.  Managed thread ID: 3.
    Thread started.
    0:  Managed thread ID: 3.
    1:  Managed thread ID: 3.
    2:  Managed thread ID: 3.
    3:  Managed thread ID: 3.
    4:  Managed thread ID: 3.
    5:  Managed thread ID: 3.
    6:  Managed thread ID: 3.
    7:  Managed thread ID: 3.
    8:  Managed thread ID: 3.
    9:  Managed thread ID: 3.

This is no different than what we'd see if you hadn't used threads at all, in order to see threads interplaying we need another one:

``` csharp
using static System.Console;
using System.Threading;

static class Program
{
    private static void Main()
    {
        WriteLine($"Main managed thread ID: {Thread.CurrentThread.ManagedThreadId}.");

        var thread1 = new Thread(Counter);
        WriteLine($"Created thread 1.  Managed thread ID: {thread1.ManagedThreadId}.");
        thread1.Start();
        WriteLine($"Thread 1 started.");

        var thread2 = new Thread(Counter);
        WriteLine($"Created thread 2.  Managed thread ID: {thread2.ManagedThreadId}.");
        thread2.Start();
        WriteLine($"Thread 2 started.");
    }

    private static void Counter()
    {
        for (var i = 0; i < 10; i++)
        {
            WriteLine($"{i}:  Managed thread ID: {Thread.CurrentThread.ManagedThreadId}.")Thread.Sleep(100);
        }
    }
}
```

Now we can see the threads operating in parallel:

    Main managed thread ID: 1.
    Created thread 1.  Managed thread ID: 3.
    Thread 1 started.
    Created thread 2.  Managed thread ID: 4.
    0:  Managed thread ID: 3.
    Thread 2 started.
    0:  Managed thread ID: 4.
    1:  Managed thread ID: 4.
    1:  Managed thread ID: 3.
    2:  Managed thread ID: 3.
    2:  Managed thread ID: 4.
    3:  Managed thread ID: 4.
    3:  Managed thread ID: 3.
    4:  Managed thread ID: 4.
    4:  Managed thread ID: 3.
    5:  Managed thread ID: 4.
    5:  Managed thread ID: 3.
    6:  Managed thread ID: 4.
    6:  Managed thread ID: 3.
    7:  Managed thread ID: 4.
    7:  Managed thread ID: 3.
    8:  Managed thread ID: 4.
    8:  Managed thread ID: 3.
    9:  Managed thread ID: 4.
    9:  Managed thread ID: 3.

In this example the threads happened to run in perfect parallel, however the OS scheduler is free to manage them however it sees fit.  It could even run one to completion and then the next if it deemed that more optimal for the wider system.

## Foreground v Background Treads

You probably noticed in the previous examples that the application didn't exit until all the threads had finished.  This might be unexpected behaviour to you, it was to me initially.  The reason we get this behavior is because the CLR has a concept of threads being either _background_ or _foreground_.  The threads are identical with one behavioral difference.  An application will terminate when all foreground threads have returned and will terminate any remaining background threads.

``` csharp
using System.Threading;
using static System.Console;

static class BackgroundAndForegroundThreads
{
    private static void Main()
    {
        var backgroundThread = new Thread(Counter)
        {
            IsBackground = true
        };

        var foregroundThread = new Thread(Counter);

        WriteLine($"Starting both threads.");
        backgroundThread.Start();
        foregroundThread.Start();
        Thread.Sleep(20);
        WriteLine("We'll kill the foreground thread and the application will exit...");
        foregroundThread.Abort();
    }

    private static void Counter()
    {
        while (true)
        {
            WriteLine($"Thread ID: {Thread.CurrentThread.ManagedThreadId}, Is Background: {Thread.CurrentThread.IsBackground}.");
            Thread.Sleep(5);
        }
    }
}
```

This will produce something like the following:

    Starting both threads.
    Thread ID: 3, Is Background: True.
    Thread ID: 4, Is Background: False.
    Thread ID: 3, Is Background: True.
    Thread ID: 4, Is Background: False.
    Thread ID: 3, Is Background: True.
    Thread ID: 4, Is Background: False.
    Thread ID: 4, Is Background: False.
    Thread ID: 3, Is Background: True.
    We'll kill the foreground thread and the application will exit...
    Thread ID: 3, Is Background: True.
    Thread ID: 3, Is Background: True.

Try killing the background thread instead, you'll see that the application never finishes because `Counter()` never returns.

## Synchronising Threads

Almost always, at some point, we need to synchronise threads in some way.  Normally synchronisation is so they can manipulate some shared data or prevent a race condition error.  We have a number of requirements for synchronisation which we'll go through next.

### Waiting for a Thread to Complete

The simplest synchronisation is just waiting for completion of some other thread.  This is done my joining the other thread back.

``` csharp
using System.Threading;
using static System.Console;

static class ThreadJoining
{
    private static void Main()
    {
        var thread = new Thread(Counter) { IsBackground = true };
        thread.Start();
        for(var i = 0; i < 5; i++)
        {
            WriteLine($"{Thread.CurrentThread.ManagedThreadId}: Working on iteration {i}.");
            Thread.Sleep(20);
        }

        WriteLine($"{Thread.CurrentThread.ManagedThreadId}: Finished! waiting...");
        thread.Join();
        WriteLine($"{Thread.CurrentThread.ManagedThreadId}: All done.");
    }

    private static void Counter()
    {
        for (var i = 0; i < 10; i++)
        {
            WriteLine($"{Thread.CurrentThread.ManagedThreadId}: Working on iteration {i}.");
            Thread.Sleep(20);
        }

        WriteLine($"{Thread.CurrentThread.ManagedThreadId}: Finished!");
    }
}
```

When you run this you'll see that the execution of the `Main()` pauses at the `Join()` method.

    1: Working on iteration 0.
    3: Working on iteration 0.
    1: Working on iteration 1.
    3: Working on iteration 1.
    1: Working on iteration 2.
    3: Working on iteration 2.
    1: Working on iteration 3.
    3: Working on iteration 3.
    1: Working on iteration 4.
    3: Working on iteration 4.
    1: Finished! waiting...
    3: Working on iteration 5.
    3: Working on iteration 6.
    3: Working on iteration 7.
    3: Working on iteration 8.
    3: Working on iteration 9.
    3: Finished!
    1: All done.

## Exceptions

It's important to note that though background threads don't contribute to an application's lifetime when things are going well; they will still cause the application to terminate when an unhandled exception occurs.

``` csharp
using System;
using System.Threading;
using static System.Console;

internal static class ExceptionHandling
{
    private static void Main()
    {

        var thread = new Thread(ThrowsDummyException);
        thread.Start();
        while (true)
        {
            WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId} is working hard...");
            Thread.Sleep(10);
        }
    }

    private static void ThrowsDummyException()
    {
        var timeSpan = TimeSpan.FromMilliseconds(100);

        WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId}: Waiting {timeSpan.TotalSeconds} seconds to throw.");
        Thread.Sleep(timeSpan);
        throw new Exception("BOOM!");
    }
}
```

You might be tempted to solve this problem by adding a `try-catch` in the `Main` method like this:

``` csharp
using System;
using System.Threading;
using static System.Console;

internal static class ExceptionHandling
{
    private static void Main()
    {
        try
        {
            var thread = new Thread(ThrowsDummyException);
            thread.Start();
            while (true)
            {
                WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId} is working hard...");
                Thread.Sleep(10);
            }
        }
        catch (System.Exception)
        {
            System.Console.WriteLine($"I caught the exception.");
        }

    }

    private static void ThrowsDummyException()
    {
        var timeSpan = TimeSpan.FromMilliseconds(100);

        WriteLine($"Thread: {Thread.CurrentThread.ManagedThreadId}: Waiting {timeSpan.TotalSeconds} seconds to throw.");
        Thread.Sleep(timeSpan);
        throw new Exception("BOOM!");
    }
}
```

This doesn't actually catch the exception though, it can't because `thread` is running asynchronously to the `Main` method.  Exception handling needs to be done in the thread that's throwing the exception.  In the example above, a `try-catch` would be used in the `ThrowsDummyException` method.

## Aborting a Thread

Once a thread is started another thread can't just stop it. A thread is unloaded when it either finishes, the app domain it's in is unloaded or an exception is thrown in the thread.

This third option is common and the CLR has a special exception class for doing this.  Calling `Abort` on a thread tells the CLR to raise a `ThreadAbortedException` in that thread.

1. The `ThreadAbortedException` is sealed.
1. The `ThreadAbortedException` has no public constructor.
1. The CLR will not terminate an application if a `ThreadAbortedException` is raised.
1. You can catch a `ThreadAbortedException` however, after handling it the `ThreadAbortedException` will continue to bubble up.