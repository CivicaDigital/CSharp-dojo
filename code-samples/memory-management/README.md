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
A simple way to display this is to use the `GC` static class get information about a value type and a reference type.  The following is crude, but it does demonstrate that the GC is affecting the string but not the integer; that being that the generation is increasing after each collection.  We'll get to what generations are soon, right now just notice what types it changes on.

``` csharp
[Fact]
public void GcStringAndInteger()
{
    const int i = 12345;
    const string s = "Hello Dave!";
    Assert.Equal(0, GC.GetGeneration(i));
    Assert.Equal(0, GC.GetGeneration(s));
    GC.Collect();
    Assert.Equal(0, GC.GetGeneration(i));
    Assert.Equal(1, GC.GetGeneration(s));
    GC.Collect();
    Assert.Equal(0, GC.GetGeneration(i));
    Assert.Equal(2, GC.GetGeneration(s));
    GC.Collect();
    Assert.Equal(0, GC.GetGeneration(i));
    Assert.Equal(2, GC.GetGeneration(s));
    GC.Collect();
}
```

## Generations

Until the most recent v

* GC
* ClrMD
* GC.Collection
