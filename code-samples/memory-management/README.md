# Memory Management

## The Bold Dream of .NET

.NET was, and largely is, the answer to the memory leaks problem the many languages suffered from.  The idea was that the framework would keep track of what resources were still needed a garbage collector would clean up any unneeded resources in an optimal and timely fashion.  You, the developer, never have to think about it.
As you'll see, this isn't 100% true, nothing is free:

1. Garbage collection can take a substantial amount of time to complete.  
2. The garbage collector cannot clean up unmanaged resources.

## Heap v Stack

Firstly, the heap and the stack are implementation details.  There's no guarantee that future CLRs will use this technique, all that is guaranteed is that value types and reference types with behave functionally as they do now.  Having said that, we are currently working with heaps and stacks so let's call them that.
Assignments to the stack aren't collected.  When they're not needed, the pointer to the head of the stack just moves down the stack.  When something else is assigned to it, the value of the new object just overwrites the old value.  This has two key effects:

1. Value types don't cause bloat.
2. Value types cannot be accessed by a reference (expect in unsafe code).

Reference types also have an entry on the stack.  This entry, rather than the value of the object, is a managed pointer to where the object can be found in the heap.  (Pointers are also value types).
A simple way to display this is to use the `GC` static class get information about a value type and a reference type.  The following is crude, but it does demonstrate that the GC is affecting the string but not the integer.

``` csharp
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
        WriteLine($"Generation of s: {GC.GetGeneration(s)}");
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
```

Notice that being that the generation is increasing after each collection.  We'll get to what generations are soon, right now just notice what types it changes on.

    Generation of i: 0
    Generation of s: 0
    Force garbage collection
    Generation of i: 0
    Generation of s: 1
    Force garbage collection
    Generation of i: 0
    Generation of s: 2
    Force garbage collection
    Generation of i: 0
    Generation of s: 2

## Generations

The generation of an object determines how aggressively it will be garbage collected.  There are three generation in the current the .NET Framework as of present, though this has never changed it is not guarenteed.  

> The `GC.MaxGeneration` property contains the max generation number (i.e. one fewer than the number of generations).

Garbage collection is expensive.  In order to perform collection the GC needs to get all the threads running into a _safe state_.  To do this it suspends execution of all threads.  It then needs to walk the heap looking for roots that no longer have a reference to them, when it finds one it marks it and carries on looking.  When it's finished it will go back through performing the collection and either freeing  the memory previously occupied by these objects or marking them for finalization.  The GC will move all the objects that aren't pinned in order to defragment the now fragmented heap.  Finally, the GC will move all the objects up one generation (unless they're already at the maximum generation) and create a new, empty gen-0.  

More objects and more threads mean more time with all the threads suspended and so more time that the application can't do any work for you.  Because of this the GC only performs collections when it deems it necessary or when you force a collection with `GC.Collect()`.

Generation 0 and generation 1 (a.k.a. gen-0 and gen-1) have a amount of memory reserved for them, called the _budget_.  The GC will allocate memory in gen-0 when you create a new object.  If there isn't enough memory left within the budget then the GC will perform a collection.  If there still isn't enough memory after the collection then the CLR will throw an `OutOfMemoryException`.

There are three reasons that a collection will be triggered:

1. A call to `GC.Collect()`.
1. Generation exceeds its budget.
1. Memory pressure

It's important to realise that gen-2 objects are collected very rarely.

## Disposing v Finalizing

The GC does a good job of cleaning up managed resources, but we often have to interface with unmanaged resources such as the file system, COM, unmanaged DLLs, Win API etc.  In these cases the GC cannot keep track of what's safe to collect or know what _safe_ might mean.  To deal with these problems we have the `IDisposable` interface and the finalizer method.

> Finalizers are usually called destructors.  Why the name was changed for the CLR is a mystery to me.

Programatically we can run anything we want before an object goes out of scope but doing it correctly requires a but of overhead that's not pleasent to use.  Instead classes should know how to release resources they own if it's needed.

The `IDisposable` interface contains a single method called `Dispose` that accests no arguments.  Types that implement the `IDisposable` interface are called `disposable classes`.  When wrapped in a `using` statement the compiler wraps the block with a `try-finally`, calling `Dispose()` in the `finally`.

``` csharp
using System;
using static System.Console;

class DisposableType : IDisposable
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

static class DisposableTypes
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
```

This means that you need to be careful not to throw any exceptions, or the cleanup will fail and (unless you've wrapped the `using` in a `try-catch` block) so will your application.  Probably leaving resources locked until the OS notices and does something about it.

The finalizer has the same goal as the `IDispose` interface, in that it used to clean up unmanaged resources.  It's usage is very different though in that it is used by the CLR, rather than being invoked in your code.  As you can see here, it's signature isn't a valid method name.  The following shows this behavior:

``` csharp
using System;
using static System.Console;

internal class FinalizableType
{
    ~FinalizableType()
    {
        WriteLine("Finalized");
    }
}

static class FinalizableExample
{
    private static void Main()
    {
        GC.RegisterForFullGCNotification(2, 2);
        var finalizableType = new FinalizableType();
        WriteLine($"Created finalizableType.");
        WriteLine("Calling GC.Collect() but keeping finalizableType reachable.");
        GC.Collect();
        GC.KeepAlive(finalizableType);
        WriteLine("Removing reference to finalizableType.");
        finalizableType = null;
        WriteLine("Calling GC.Collect().");
        GC.Collect();
        WriteLine("Finalizer hasn't been called yet, the finalizableType has been moved to the FReachable queue.");
        GC.Collect();
        WriteLine("Finalization should have finished.");
    }
}
```

There are two important points to note about this output.

    Created finalizableType.
    Calling GC.Collect() but keeping finalizableType reachable.
    Removing reference to finalizableType.
    Calling GC.Collect().
    Finalizer hasn't been called yet, the finalizableType has been moved to the FReachable queue.
    Calling GC.Collect() again.
    Finalized
    Finalization should have finished.


Firstly that there is no explicit call to invoke the finalizer.  You have no control over when the finalizer will run (unless you're invoking explicit collections liek this demonstration, but that's almost certainally a bad idea).  

Secondly, the object isn't collected until the second collection after the last reference (`GC.KeepAlive()`).  This is important, when you have a finalizer method the GC doesn't collect it on it's initial pass, instead it marks it for finalization by putting it on the finalizer queue (called FReachable).  This means tha the resources will not be collected until at least a gen-1 collection.  It also means that none of the resources it references will be collected because the GC will see that they're still accessable.

You may notice that the `Object` type has a finalizer declared, if this is the only finalizer then the GC won't add it to the FReachable queue and will collect it as normal.

> It doesn't matter if the finalizer has anything in it, if it's there then the GC will move the object to FReachable.

## When to use a Finalizer

The finalizer is a strange beast in ways I haven't gone into here.

See Eric Lippert's articles (prepare a hot brew first):

* [When everything you know is wrong, part one](https://ericlippert.com/2015/05/18/when-everything-you-know-is-wrong-part-one/)
* [When everything you know is wrong, part two](https://ericlippert.com/2015/05/21/when-everything-you-know-is-wrong-part-two/)

It does has a use though, primarily guarding against developers misimplementing disposing via the `IDisposable` interface.  If there is some resource that must be released then it's prudent to implement **both** `IDisposable` and the finalizer method. It needs to be implemented with caution.

### The Disposal Pattern

See: [https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/dispose-pattern](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/dispose-pattern)

This pattern allows us to implement both a `IDisposable` and the finalizer without the danger of having both run, or having the object added to FReachable is it has been disposed of correctly.

``` csharp
using System;
using static System.Console;

public class CorrectDisposeFinalizeType : IDisposable 
{  
    protected virtual void Dispose(bool disposing)
    {
        WriteLine($"Dispose called with disposing = {disposing}.");
        WriteLine("Release unmanaged, non-disposable objects");
        if (disposing)
            WriteLine("Disposing of other IDisposable objects");
    }  

    ~ CorrectDisposeFinalizeType(){
        WriteLine("Finalizer call.");
        Dispose(false);  
    }  

    public void Dispose()
    {
        WriteLine("Called IDisposable.Dispose().");
        Dispose(true);  
        WriteLine("Suppressing finalize.");
        GC.SuppressFinalize(this);  
    }  
}

static class FinalizerDisposePattern
{
    private static void Main()
    {
        WriteLine("Wrapped with using:");
        using (var correctDisposeFinalizeType = new CorrectDisposeFinalizeType())
        {
            GC.KeepAlive(correctDisposeFinalizeType);
        }
        WriteLine("Not wrapped with using:");
        var o2 = new CorrectDisposeFinalizeType();
    }
}
```

In the code above we have both `Dispose()` and the finaliser.  

There are two important points to note.  The first is the virtual override of Dispose that accepts a boolen.  This boolean indicates whether the proceedure is being invoked by the `IDisposable.Dispose()` method or the finaliser.  If this is a 'normal' dispose then the we are free to release any unmanaged resources and call `Dispose()` on any objects we have.  If this is not then it's not necessarily safe to call any methods on any other objects, so we just dispose of the unmanaged resources we own.

The second important point is the call to `GC.SuppressFinalize()`.  This prevents the GC from putting the object on the FReachable queue.

You can see this working in the output from the above code:

    Wrapped with using:
    Called IDisposable.Dispose().
    Dispose called with disposing = True.
    Release unmanaged, non-disposable objects
    Disposing of other IDisposable objects
    Suppressing finalize.
    Not wrapped with using:
    Finalizer call.
    Dispose called with disposing = False.
    Release unmanaged, non-disposable objects

