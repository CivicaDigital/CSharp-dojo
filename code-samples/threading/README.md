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
1. You can catch a `ThreadAbortedException` **however**, after handling it the `ThreadAbortedException` will continue to bubble up **unless** `ResetAbort` is called.

The last point here means that whilst you can try to abort a thread, you can't guarantee when or if is will actually happen.

``` csharp
using static System.Console;
using System.Threading;

static class AbortingAThread
{
    private static void Main()
    {
        var thread = new Thread(Counter);
        thread.Start();
        Thread.Sleep(50);
        thread.Abort("I'm aborting you.");
    }
    private static void Counter()
    {
        try
        {
            var i = 0;
            while(true)
            {
                WriteLine($"{Thread.CurrentThread.ManagedThreadId}: {i}");
                Thread.Sleep(10);
            }
        }
        catch(ThreadAbortException e)
        {
            WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}:  I caught the `ThreadAbortedException` exception: {e.Message};");
            WriteLine($"The object data is: {e.ExceptionState}");
        }
    }
}
```

Whilst we can't catch the exception to stop it from being re-thrown, we can cancel it completely by calling `ResetAbort()`.

``` csharp
using static System.Console;
using System.Threading;

static class ResettingAnAbortingThread
{
    private static void Main()
    {
        var thread = new Thread(Counter);
        thread.Start();
        Thread.Sleep(50);
        thread.Abort("I'm aborting you.");
    }
    private static void Counter()
    {
        try
        {
            var i = 0;
            while(true)
            {
                WriteLine($"{Thread.CurrentThread.ManagedThreadId}: {i}");
                Thread.Sleep(10);
            }
        }
        catch(ThreadAbortException e)
        {
            WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}:  I caught the `ThreadAbortedException` exception: {e.Message};");
            WriteLine($"The object data is: {e.ExceptionState}");
            Thread.ResetAbort();
        }

        WriteLine("Wee!  I wasn't aborted!");
    }
}
```

## Parameterized Threads

The `Thread` constructor is overloaded to accept either a `ThreadStart` delegate or a `ParameterizedThreadStart`.  The `ParameterizedThreadStart` delegate accepts a single `object` as an argument.

``` csharp
using System.Threading;
using static System.Console;

static class ParameterizedThreads
{
    private static void Main()
    {
        var thread = new Thread(CountInterval);
        thread.Start("Hello World!");
        thread.Join();
    }

    private static void CountInterval(object message)
    {
        WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId}: Message: {(string) message}.");
    }
}
```

## Thread Priority Levels

Windows uses a threads priority (relative to other threads competing for processor time) to decide what thread gets processor time.  It will share the processor time between the highest priority threads that are ready at the exclusion of all lower priority threads.  This means that you must exercise care in setting a thread's priority - in fact, there are some priorities that can only be set by a user with kernel-mode permissions.

> The absolute priority is a number from 0 to 31 (inclusive).  0 is the lowest priority and 31 is the highest.  0 however is reserved for the sole use of the _zero page thread_ that is created by the OS.

There are two values that affect the calculation of the absolute priority of a thread:

1. The thread's `ThreadPriority` value.
1. The process's priority class.

They are calculated as:

![Thread priorities](https://i.imgur.com/15Z4Pr9.png)

What this means is that you could create a high priority, long running thread which would block other threads from executing.  This is called _thread starvation_.

We can see this happening with the following code:

``` csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static System.Console;

static class ThreadStarvation
{
    private static void Main()
    {
        var processorCount = Environment.ProcessorCount;

        WriteLine($"There are {processorCount} processors.");
        WriteLine($"There can be {processorCount} threads running at the same time.");

        var aboveNormalPriorityThreads = new HashSet<Thread>();

        for (var i = 0; i < processorCount; i ++)
        {
            aboveNormalPriorityThreads.Add(new Thread(Counter) { Priority = ThreadPriority.AboveNormal });
        }

        var normalPriorityThread = new Thread(Counter);
        normalPriorityThread.Start();
        WriteLine($"Letting the normal priority thread run for a bit");
        Thread.Sleep(1000);
        foreach(var thread in aboveNormalPriorityThreads)
        {
            WriteLine($"Starting {thread.ManagedThreadId} with priority {thread.Priority}.");
            thread.Start();
        }
    }

    private static void Counter()
    {
        var currentThread = Thread.CurrentThread;
        var priority = currentThread.Priority;
        var id = currentThread.ManagedThreadId;

        for (var i = 0; i < 5; i++)
        {
            var then = DateTime.Now;
            var number = then.GetHashCode();
            while (DateTime.Now < then.AddSeconds(1))
            {
                number ^= number.GetHashCode();
            }

            WriteLine($"Thread ID: {id}, Priority: {priority, -15}{i, 10}{number}");
        }

        WriteLine($"Thread ID: {id}, Priority: {priority, -15}COMPLETED.");
    }
}
```

Here we create a number of high priority threads, enough to saturate the processors we have.  We also create low priority thread.  Even though the low priority thread is started before the higher priority ones it's starved of resources until one of the higher priority threads complete.

    There are 4 processors.
    There can be 4 threads running at the same time.
    Letting the normal priority thread run for a bit
    Starting 3 with priority AboveNormal.
    Thread ID: 7, Priority: Normal                  00
    Starting 4 with priority AboveNormal.
    Starting 5 with priority AboveNormal.
    Starting 6 with priority AboveNormal.
    Thread ID: 4, Priority: AboveNormal             00
    Thread ID: 3, Priority: AboveNormal             00
    Thread ID: 5, Priority: AboveNormal             00
    Thread ID: 6, Priority: AboveNormal             00
    Thread ID: 4, Priority: AboveNormal             10
    Thread ID: 5, Priority: AboveNormal             10
    Thread ID: 3, Priority: AboveNormal             10
    Thread ID: 6, Priority: AboveNormal             10
    Thread ID: 3, Priority: AboveNormal             20
    Thread ID: 4, Priority: AboveNormal             20
    Thread ID: 5, Priority: AboveNormal             20
    Thread ID: 6, Priority: AboveNormal             20
    Thread ID: 3, Priority: AboveNormal             30
    Thread ID: 5, Priority: AboveNormal             30
    Thread ID: 4, Priority: AboveNormal             30
    Thread ID: 6, Priority: AboveNormal             30
    Thread ID: 3, Priority: AboveNormal             40
    Thread ID: 3, Priority: AboveNormal    COMPLETED.
    Thread ID: 5, Priority: AboveNormal             40
    Thread ID: 5, Priority: AboveNormal    COMPLETED.
    Thread ID: 7, Priority: Normal                  10
    Thread ID: 4, Priority: AboveNormal             40
    Thread ID: 4, Priority: AboveNormal    COMPLETED.
    Thread ID: 6, Priority: AboveNormal             40
    Thread ID: 6, Priority: AboveNormal    COMPLETED.
    Thread ID: 7, Priority: Normal                  20
    Thread ID: 7, Priority: Normal                  30
    Thread ID: 7, Priority: Normal                  40
    Thread ID: 7, Priority: Normal         COMPLETED.

Notice as well that the lower priority thread is actually stopped mid-execution to make room for the higher priority threads.  When you run this you probably notice that the performance of the system degrades for the ten-ish seconds it's running for.

## The Cost of Threads

Aside from the problems demonstrated above and the need for you as the developer to optimise for an unknown number of CPUs you will also have to conciser the other substantial overheads associated with creating and managing threads.

### The Upfront Cost

Each thread created has is initialized with some data structures.  A kernel object, a thread environment block, user stack and kernel stack and DLL thread-attach and thread detach notifications.

The kernel object itself it used by the OS kernel to reference the thread.

The stacks contain the processing information for user and kernel mode.  Kernel mode and user mode can't pass by reference, so any state that needs to be passed between them needs to be copied.

The DLL thread attach and detach list the `DllMain` method for every unmanaged DLL loaded.  With some exceptions, it will call this method when it loads and unloads with different flags (i.e. `DLL_THREAD_ATTACH` and `DLL_THREAD_DETACH`);

### The Ongoing Costs

The hardware processor contains many optimisations, one of which is the cache.  When a thread is processing the cache hugely speeds up data access for recently accessed data items.  Swapping unrelated threads in and out of the processor makes the cache useless and slower then if it weren't there at all.

In order to solve this problem, each thread stores it's cache in the kernel object and has to reload the cache in the processor each time it is granted processor time.

## Possible Solutions

I use the term _solution_ rather flippantly here.  There are many best practices that help us avoid the problems described above to some extent or another, but the have drawbacks as well.  That being acknowledged, for most scenarios these are better options than the manual thread manipulation we've looked at so far.

### Thread Pools

We can avoid the overhead of thread creation by reusing threads that have already been created.  The `ThreadPool` class handles this for us.

``` csharp
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using static System.Console;

static class ThreadPoolClass
{
    private const int THREAD_COUNT = 100;
    private static readonly bool[] COMPLETION = new bool[THREAD_COUNT];
    private static readonly ConcurrentStack<int> USAGE = new ConcurrentStack<int>();

    private static void Main()
    {
        for (var i = 0; i < THREAD_COUNT; i++)
        {
            WriteLine($"Queueing {i}.");
            ThreadPool.QueueUserWorkItem(Counter, i);
        }

        while (!COMPLETION.All(x => x))
        {
            Thread.Sleep(0);
        }

        var threadUsages = new SortedDictionary<int, int>();

        foreach (var usage in USAGE)
        {
            if (threadUsages.ContainsKey(usage))
            {
                threadUsages[usage] = threadUsages[usage] + 1;
            }
            else
            {
                threadUsages.Add(usage, 1);
            }
        }

        WriteLine($"Thread reuse:");
        foreach (var usage in threadUsages)
        {
            WriteLine($"Thread {usage.Key} used {usage.Value} times.");
        }
    }

    private static void Counter(object state)
    {
        var index = (int)state;
        USAGE.Push(Thread.CurrentThread.ManagedThreadId);

        for (var i = 0; i < 10; i++)
        {
            WriteLine($"Thread ID: {Thread.CurrentThread.ManagedThreadId}: {i}");
            Thread.Sleep(1);
        }
        COMPLETION[index] = true;
    }
}
```

When you run this, you'll see something like the following at the end of the output:

    Thread reuse:
    Thread 3 used 22 times.
    Thread 4 used 22 times.
    Thread 5 used 23 times.
    Thread 6 used 22 times.
    Thread 7 used 11 times.

This might seem like the silver bullet, however we lose a lot of benefits from the manual creation.

1. No way to chose a thread's priority.
1. No way to wait for a thread to finish.
1. The thread is in an unknown state.  This means that there could be unexpected values in the TLS, maybe even secret stuff from whatever it was doing last.
1. Thread pool threads are always background threads.

## The Task Type

The `Task` and `Task<T>` types address the problem of waiting until completion.  If we change the `ThreadPool` example above to use tasks then we get exactly the same output, but we don't need the `while` loop and boolean array to wait for tasks to all finish.  We can just use the `Task.WaitAll` method.

Internally the task is using the `ThreadPool`, which is why our output is identical.

## Cancelling Threads

Cancellation has two _modes_ that can be used for cancelling.

1. Marking to the subject thread that it's creator wants it to cancel.
1. The subject thread throwing a `OperationCancelledException` if cancellation has been requested.

Cancelling threads is a cooperative pattern.  We tell the thread that we want to cancel it, it is up to the executing code to then decide what action to take.  The thread is under no obligation to cancel at all.  A _well written_ thread will cancel if it is safe to do so.  This isn't dissimilar to the disposal of objects, in that the object will perform any cleaning up actions it deems necessary before being collected.

Both these methods are shown here:

``` csharp
using System;
using System.Threading;
using static System.Threading.Thread;
using static System.Console;

internal static class ThreadCancellation
{
    private static void Main()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        ThreadPool.QueueUserWorkItem(x => Counter(cancellationToken));
        ThreadPool.QueueUserWorkItem(x => CounterWithThrow(cancellationToken));
        Sleep(10);
        cancellationTokenSource.Cancel();
        Sleep(10);
    }

    private static void Counter(CancellationToken cancellationToken)
    {
        WriteLine($"Counter running on {CurrentThread.ManagedThreadId}.");
        while (!cancellationToken.IsCancellationRequested)
        {
            WriteLine($"Thread {CurrentThread.ManagedThreadId}:  {DateTime.Now.Ticks}");
            Sleep(1);
        }

        WriteLine("Counter() was canceled");
    }

    private static void CounterWithThrow(CancellationToken cancellationToken)
    {
        WriteLine($"CounterWithThrow running on {CurrentThread.ManagedThreadId}.");

        try
        {
            while (true)
            {
                WriteLine($"Thread {CurrentThread.ManagedThreadId}:  {DateTime.Now.Ticks}");
                cancellationToken.ThrowIfCancellationRequested();
                Sleep(1);
            }

        }
        catch (OperationCanceledException e)
            {
            WriteLine($"Thread {CurrentThread.ManagedThreadId}: Caught OperationCanceledException: {e.Message}");
        }
    }
}
```

Both `Counter` and `CancellationToken`  accept a `CancellationToken` token (created by the same `CancellationTokenSource`) and both keep processing until `Cancel()` is called on the token's source; however `Counter()` just exits gracefully by querying the `IsCancellationRequest` property whereas `CounterWithThrow()` catches the exception that's thrown.  If we didn't catch this exception then the whole application would fail.

We can also catch the exception in the parent thread.  If an exception is thrown then `Cancel()`.

## Registering a Callback

We can also register callbacks to be invoked when a cancellation is called:

``` csharp
using System;
    using System.Threading;
    using static System.Console;
    using static System.Threading.Thread;

    internal class RegisterCancellationCallback
    {
        private static void Main()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = cancellationTokenSource.Token;
            ThreadPool.QueueUserWorkItem(x => Counter(cancellationToken));
            ThreadPool.QueueUserWorkItem(x => CounterWithThrow(cancellationToken));
            cancellationToken.Register(LogCanceled);
            Sleep(10);
            cancellationTokenSource.Cancel();

            Sleep(10);
        }

        private static void LogCanceled()
        {
            WriteLine($"Thread {CurrentThread.ManagedThreadId}:  Registered callback invoked.");
        }

        private static void Counter(CancellationToken cancellationToken)
        {
            WriteLine($"Counter running on {CurrentThread.ManagedThreadId}.");
            while (!cancellationToken.IsCancellationRequested)
            {
                WriteLine($"Thread {CurrentThread.ManagedThreadId}:  {DateTime.Now.Ticks}");
                Sleep(1);
            }

            WriteLine("Counter() was canceled");
        }

        private static void CounterWithThrow(CancellationToken cancellationToken)
        {
            try
            {
                WriteLine($"CounterWithThrow running on {CurrentThread.ManagedThreadId}.");
                while (true)
                {
                    WriteLine($"Thread {CurrentThread.ManagedThreadId}:  {DateTime.Now.Ticks}");
                    cancellationToken.ThrowIfCancellationRequested();
                    Sleep(1);
                }
            }
            catch (OperationCanceledException e)
            {
                WriteLine($"Thread {CurrentThread.ManagedThreadId}: Caught OperationCanceledException: {e.Message}");
            }
        }
    }
```