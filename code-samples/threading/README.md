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

## Locking Primitives

Let's assume we have many threads performing tasks, periodically they want to adjust some value to reflect their progress.

``` csharp
using System;
using System.Threading;

internal static class AggregatingFromManyThreadsIncorrect
{
    private static float _BALANCE;

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

        for (var i = 0; i < threadCount; i++)
        {
            threads[i].Join();
        }

        Console.WriteLine($"Balance = {_BALANCE}.");
    }

    private static void PerformTransactions(object state)
    {
        for (var i = 0; i < 10000; i++)
        {
            _BALANCE += 1;
            Thread.Sleep(0);
            _BALANCE -= 1;
            Thread.Sleep(0);
        }
    }
}
```

The above code adds and removed `1` from the balance.  This should mean that the final balance should be zero.  Run this a few times though and you'll see that the final value is regularly non-zero.  

The reason for this is that the `+=` operator isn't atomic.  In fact the lines `+=` and `-=` are expanded into this:

```csharp
private static void PerformTransactions(object state)
{
    for (var i = 0; i < 10000; i++)
    {
        var b1 = _BALANCE;
        var result1 = b1 + 1;
        _BALANCE = result1;
        Thread.Sleep(0);
        var b2 = _BALANCE;
        var result2 = b2 - 1;
        _BALANCE = result2;
        Thread.Sleep(0);
    }
}
```

With many threads running it's quite probable that several threads will be in the code between assigning the current value of `_BALANCE` to the temporary variable and the code setting `_BALANCE` to the newly calculated result.

The simplest was the make this thread safe is with the `lock` keyword.  The `lock` is a keyword that tells the compiler to wrap the locked block with a `Monitor.Enter()` and `Monitor.Exit()`.

Rewrite the code above with the `lock` keyword.

``` csharp
    using System;
    using System.Threading;

    internal static class LockKeyword
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
            for (var i = 0; i < 10000; i++)
            {
                lock(LOCK)
                {
                    _BALANCE += 1;
                }

                Thread.Sleep(0);
                lock(LOCK)
                {
                    _BALANCE -= 1;
                }

                Thread.Sleep(0);
            }
        }
    }
```

Running this again will prove that the result is now what we expect.  The `lock` block keyword is actually a shortcut for calling the `Monitor.Enter()` and `Monitor.Exit()` methods in a `try-finally` pattern (similar to how the `using` statement works).

We can see this in the IL:

``` il
.method private hidebysig static void  PerformTransactions(object state) cil managed
{
  // Code size       109 (0x6d)
  .maxstack  2
  .locals init (int32 V_0,
           object V_1,
           bool V_2)
  IL_0000:  ldc.i4.0
  IL_0001:  stloc.0
  IL_0002:  br.s       IL_0064
  IL_0004:  ldsfld     object BanksySan.Workshops.AdvancedCSharp.ThreadingExamples.LockKeyword::LOCK
  IL_0009:  stloc.1
  IL_000a:  ldc.i4.0
  IL_000b:  stloc.2
  .try
  {
    IL_000c:  ldloc.1
    IL_000d:  ldloca.s   V_2
    IL_000f:  call       void [mscorlib]System.Threading.Monitor::Enter(object,
                                                                        bool&)
    IL_0014:  ldsfld     int32 BanksySan.Workshops.AdvancedCSharp.ThreadingExamples.LockKeyword::_BALANCE
    IL_0019:  ldc.i4.1
    IL_001a:  add
    IL_001b:  stsfld     int32 BanksySan.Workshops.AdvancedCSharp.ThreadingExamples.LockKeyword::_BALANCE
    IL_0020:  leave.s    IL_002c
  }  // end .try
  finally
  {
    IL_0022:  ldloc.2
    IL_0023:  brfalse.s  IL_002b
    IL_0025:  ldloc.1
    IL_0026:  call       void [mscorlib]System.Threading.Monitor::Exit(object)
    IL_002b:  endfinally
  }  // end handler
  IL_002c:  ldc.i4.0
  IL_002d:  call       void [mscorlib]System.Threading.Thread::Sleep(int32)
  IL_0032:  ldsfld     object BanksySan.Workshops.AdvancedCSharp.ThreadingExamples.LockKeyword::LOCK
  IL_0037:  stloc.1
  IL_0038:  ldc.i4.0
  IL_0039:  stloc.2
  .try
  {
    IL_003a:  ldloc.1
    IL_003b:  ldloca.s   V_2
    IL_003d:  call       void [mscorlib]System.Threading.Monitor::Enter(object,
                                                                        bool&)
    IL_0042:  ldsfld     int32 BanksySan.Workshops.AdvancedCSharp.ThreadingExamples.LockKeyword::_BALANCE
    IL_0047:  ldc.i4.1
    IL_0048:  sub
    IL_0049:  stsfld     int32 BanksySan.Workshops.AdvancedCSharp.ThreadingExamples.LockKeyword::_BALANCE
    IL_004e:  leave.s    IL_005a
  }  // end .try
  finally
  {
    IL_0050:  ldloc.2
    IL_0051:  brfalse.s  IL_0059
    IL_0053:  ldloc.1
    IL_0054:  call       void [mscorlib]System.Threading.Monitor::Exit(object)
    IL_0059:  endfinally
  }  // end handler
  IL_005a:  ldc.i4.0
  IL_005b:  call       void [mscorlib]System.Threading.Thread::Sleep(int32)
  IL_0060:  ldloc.0
  IL_0061:  ldc.i4.1
  IL_0062:  add
  IL_0063:  stloc.0
  IL_0064:  ldloc.0
  IL_0065:  ldc.i4     0x2710
  IL_006a:  blt.s      IL_0004
  IL_006c:  ret
} // end of method LockKeyword::PerformTransactions
```

We can see the `try` and `finally` and the calls to `Monitor.Enter()` and `Monitor.Exit()` within them.


## Performance

Locking a code block, by necessity, causes a bottleneck because only one thread can get past that point at a time.  The performance hit, even for a very simple example like this a significant amount.  To combat this you need to limit the amount of code in a synchronised context to a minimum, or none at all.

In the example above the nature of the calculation means that we don't actually need to be updating the shared value constantly.  Each thread can calculate it's total and just lock the shared value once at the end to update it.

``` csharp
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
```

Now the lock only occurs once per thread; a big improvement which is actually faster than the unsynchronised (and erroneous) example because theres no need for enforced atomic red/writes from the shared value.

We can achieve this by using tasks.

``` csharp
using System;
using System.Threading;
using System.Threading.Tasks;

internal static class NoLockAtAll
{
    private static void Main()
    {
        var threadCount = 10;
        var tasks = new Task<int>[threadCount];

        for (var i = 0; i < threadCount; i++) tasks[i] = new Task<int>(PerformTransactions);

        for (var i = 0; i < tasks.Length; i++) tasks[i].Start();

        var results = Task.WhenAll(tasks);

        var balance = 0;

        foreach (var result in results.Result) balance += result;

        Console.WriteLine($"Balance = {balance}.");
    }

    private static int PerformTransactions()
    {
        var balance = 0;

        for (var i = 0; i < 10000; i++)
        {
            balance += 1;
            Thread.Sleep(0);
            balance -= 1;
            Thread.Sleep(0);
        }

        return balance;
    }
}
```

By re-writing the `PerformTransactions()` method so it returns a value and doesn't have any side-affects (i.e. changing any state external to it) we know have a method that is guaranteed thread safe.

> This method also always returns the same value when given the same arguments (in this case no arguments).  When a method has all three of these properties we call it a _pure_ method.  Pure methods are one of the fundamental building blocks of functional programming.

## Torn Reads

The error we got in the initial multi-threaded balance calculation was due to the read, calculate and write not being atomic, however the read and write _independently_ are atomic.  There isn't any way that a read can happen _whilst_ a write is happening.  As per the ECMA specification:

> I.12.6.5 Locks and threads
> > Built-in atomic reads and writes. All reads and writes of certain properly aligned data types are guaranteed to occur atomically

The CLI guarantees that reads and writes to the following data types are atomic:

* `bool`
* `char`
* `byte`
* `sbyte`
* `short`
* `ushort`
* `int`
* `uint`
* `float`
* Object pointer

This doesn't include `double`, `decimal`, `long` or `ulong`.  This is because these types are all larger than 32-bits.

The following code has one thread writing to a `ulong` and another reading from it.

``` csharp
using System;
using System.Threading;

internal static class TornReads
{
    private const ulong NUMBER_1 = 0xFFFFFFFFFFFFFFFF;
    private const ulong NUMBER_2 = 0x0000000000000000;
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
                Console.WriteLine($"{i,3}: Read: {number:X16} TornRead!");
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
```

A _good_ read would be either `0x0000000000000000` or `0xFFFFFFFFFFFFFFFF` (i.e. setting all bits to `0` or setting all bits to `1`).  A _bad_ read would be when some of the bits have been changed, but not all.

Compile this code, **targeting x32**  and you will see that some values are `0x00000000FFFFFFFF` and some are `0xFFFFFFFF00000000`.  This is called a _torn read_.  We only see these two torn values because 32 bits are atomic, this it takes two atomic 32 bit writes to the full 62 bit value.

Try targeting x64 now, you'll see that there aren't any torn reads at all.  This is because a x64 CPU can read and write 64 bits atomically.

> NB The atomic reading of the values larger than 32 bits is because of the CPU architecture.  It's not guaranteed by the CLI.

## Interlocked operations

Interlocking is the term used for performing read _and_ write operations atomically.  C# has a static class `Interlocked` which has several methods to achieve this behaviour.  For example, the `Increment` and `Decrement` methods perform the `+=` and `-=` operations we used in the `lock` examples.

Torn reads could be fixed with the `lock` keyword again, but `Interlocked` provides us with a better option using the `Read` method.

## Volatile

When compiling C# you have the option of optimising the code.  Effectively, optimising rewrites the code you've written so that it runs faster.  Production code should always be optimised.  

> Because optimised code is different version it should have a different version number.

Look at this code:

``` csharp
using System;
using System.Threading;

internal static class OptimisationBugs
{
    private const int TERMINATING_COUNT = 10;
    private static int _COUNT;

    private static void Main()
    {
        var checker = new Thread(Checker);
        var stopper = new Thread(Counter);
        checker.Start();
        Thread.Sleep(10);
        stopper.Start();
        var timeout = TimeSpan.FromSeconds(1);
        _COUNT = 1;
        Console.WriteLine($"Waiting for {timeout} worker to stop.");
        checker.Join(timeout);
        if (checker.IsAlive)
        {
            Console.Error.WriteLine($"Thread failed to stop.  Aborting instead. ");
            checker.Abort();
        }

        Console.WriteLine("Done");
    }

    private static void Counter()
    {
        for (; _COUNT < 21; _COUNT++)
        {
            Console.WriteLine($"Count: {_COUNT}");
            if (_COUNT == TERMINATING_COUNT)
                Console.WriteLine($"Terminator {TERMINATING_COUNT} reached.");
        }
    }

    private static void Checker()
    {
        var x = 0;
        while (_COUNT < TERMINATING_COUNT) x++;
        Console.WriteLine($"{nameof(Checker)} stopped at {x}.");
    }
}
```

If we compile this code without any optimisations then things happen as we intend; that being that the `Checker` counts as high as it can before the `_COUNT` variable exceeds `9`.

    Waiting for 00:00:01 worker to stop.
    Count: 1
    Count: 2
    Count: 3
    Count: 4
    Count: 5
    Count: 6
    Count: 7
    Count: 8
    Count: 9
    Count: 10
    Terminator 10 reached.
    Count: 11
    Count: 12
    Count: 13
    Count: 14
    Count: 15
    Count: 16
    Count: 17
    Count: 18
    Count: 19
    Count: 20
    Checker stopped at 9101073.
    Done

Now compile this again, this time optimised.  Now the output is different, the timeout on the `Join` is breached.  If fact, if we didn't have the timeout there the application would never exit.

To fix this problem we need to signal to the compiler that it need to fetch the value in `_COUNT` each time.  We need to mark the read as volatile.  We have two options here, we could put the `volatile` keyword in from of the declaration or we can use the static methods off the `Volatile` class.  The latter of these options is preferred for two reasons:

1. Using the `volatile` keyword causes every read and write to be volatile.
1. Volatility is an operation of individual reads and writes, not of declaration.

In this case it's the read that should be volatile, so we can correct the `Checker` method.

``` csharp
private static void Checker()
{
    var x = 0;
    while (Volatile.Read(ref _COUNT) < TERMINATING_COUNT) x++;
    Console.WriteLine($"{nameof(Checker)} stopped at {x}.");
}
```

Notice that the target of the read is passed by reference.