# OOP

> “The problem with object-oriented languages is they’ve got all this implicit environment that they carry around with them. You wanted a banana but what you got was a gorilla holding the banana and the entire jungle.” ~ Joe Armstrong

The following features make a language an OOP language:

* Encapsulation
* Inheritance
* Polymorphism

Whilst we all, I assume, understand what is meant by these terms there are still some areas worthy of mention in .NET's implementation.

## Overriding

A key aspect of inheritance is the ability to override a base class's methods.  In .NET a method must be marked as `virtual` in order for overriding to be possible.  All interface methods are virtual by necessity as are any abstract methods.

> In fact, both interface methods and abstract methods are marked `virtual` and `abstract`.  `virtual` so they can be implemented and `abstract` so they can't be instantiated.  Abstract only makes sense in a classical OOP language.

Consider these:

``` csharp
internal interface IFoo
{
    void Bang();
}

internal abstract class Bar
{
    public abstract void Bang();
}
```

If we look at the IL for the two `Bang()` methods you can see that they're identical:

``` il
.method public hidebysig newslot abstract virtual 
        instance void  Bang() cil managed
{
}
```

If we implement these of these in the same class them we run into a question.

``` csharp
class FooBar : Bar, IFoo
{
    public override void Bang() => throw new NotImplementedException();
}
```

When I call `Bang()` from an instance of `FooBarImplementation` do I run the `Bang()` in `IFoo` or the `Bang()` in `Bar`?

``` csharp
using static System.Console;

internal static class VirtualMethodImplementation
{
    private static void Main()
    {
        var fooBar = new FooBar();
        WriteLine($"Call Bang.");
        fooBar.Bang();
        WriteLine("Call IFoo.Bang");
        ((IFoo) fooBar).Bang();
        WriteLine("Call Bar.Bang");
        ((Bar)fooBar).Bang();
    }
}
```

Running this just prints `Bang!` from each call.  The method is _technically_ implementing the `Bar` version, but the requirements from `IFoo` have been met so `IFoo`.  We can see from the metadata in the assembly that only one method is registered:

``` assembly
TypeDef #3 (02000004)
-------------------------------------------------------
    TypDefName: BanksySan.Workshops.AdvancedCSharp.Oop.FooBar  (02000004)
    Flags     : [NotPublic] [AutoLayout] [Class] [AnsiClass] [BeforeFieldInit]  (00100000)
    Extends   : 02000005 [TypeDef] BanksySan.Workshops.AdvancedCSharp.Oop.Bar
    Method #1 (06000003)
    -------------------------------------------------------
        MethodName: Bang (06000003)
        Flags     : [Public] [Virtual] [HideBySig] [ReuseSlot]  (000000c6)
        RVA       : 0x00002086
        ImplFlags : [IL] [Managed]  (00000000)
        CallCnvntn: [DEFAULT]
        hasThis 
        ReturnType: Void
        No arguments.

    Method #2 (06000004)
    -------------------------------------------------------
        MethodName: .ctor (06000004)
        Flags     : [Public] [HideBySig] [ReuseSlot] [SpecialName] [RTSpecialName] [.ctor]  (00001886)
        RVA       : 0x00002092
        ImplFlags : [IL] [Managed]  (00000000)
        CallCnvntn: [DEFAULT]
        hasThis 
        ReturnType: Void
        No arguments.

    InterfaceImpl #1 (09000001)
    -------------------------------------------------------
        Class     : BanksySan.Workshops.AdvancedCSharp.Oop.FooBar
        Token     : 02000003 [TypeDef] BanksySan.Workshops.AdvancedCSharp.Oop.IFoo
```

Testing this:

``` none
Call Bang.
BANG!
Call IFoo.Bang
BANG!
Call Bar.Bang
BANG!s
```

It's quite possible that we'd have different implementations for `IFoo.Bang` and `Bar.Bang`.

> In fact, if we don't have different implementations then it might be a code smell suggesting that `Bar` might implement `IFoo` instead.

To do this we need to use an explicit interface implementation.

``` csharp
class FooBar: Bar, IFoo
{
    void IFoo.Bang() => WriteLine("BANG! (from IFoo)");

    public override void Bang() => WriteLine("BANG! (From Bar)");
}
```

> Only interfaces can have an explicit implementation.  This isn't a problem because we can only inherit from a single concrete class.  If we supported multiple inheritance then we'd need a syntax for explicit implementation for those.

Running the code again, with the new implementations of `IFoo` and `Bar` gives us the expected output:

``` none
Call Bang.
BANG! (From Bar)
Call IFoo.Bang
BANG! (From Bar)
Call Bar.Bang
BANG! (From Bar)
```

Notice though that the `FooBar` is a `Bar` by default, it needs to be explicitly cast to `IFoo`.

This raises the question of what overload will be picked if I an expecting an `IFoo`, a `Bar` or a `FooBar`.

``` csharp
private static void Main()
{
    var fooBar = new FooBar();

    Console.WriteLine("Calling Boink with FooBar");
    Boink(fooBar);
    Console.WriteLine("Calling Boink with Bar");
    Boink((Bar) fooBar);
    Console.WriteLine("Calling Boink with Foo");
    Boink((IFoo) fooBar);
}

private static void Boink(Bar bar) => WriteLine("Boink in Bar");
private static void Boink(IFoo foo) => WriteLine("Boink from foo");
private static void Boink(FooBar foo) => WriteLine("Boink from FooBar");
}
```

In this case the expected happens, however, if we remove `Boink(FooBar)` we have a compile-time exception:

``` none
error CS0121: The call is ambiguous between the following methods or properties: 'ImplementInterfaceAndAbstractSingleOverride.Boink(Bar)' and 'ImplementInterfaceAndAbstractSingleOverride.Boink(IFoo)'
```

Because it might be unclear to a developer which overload might be invoked, the compiler requires you to be explicit by casting the type to the correct supertype.

## Overriding Implementation v Hiding Members

Just as it's quite reasonable for there to be duplicate member names across interfaces and classes, it is also possible for you to need to use a member signature that a base type have used.  Consider the following two classes:

``` csharp
using static System.Console;

class Foo
{
    public void Bang() => WriteLine("Bang from Foo");
}

class Bar
{
    public void Bang() => WriteLine("Bang from Bar");
}
```

We want `Bar` to extend `Foo`, but we don't want to override `Bar.Bang()` - we want to, unconnected methods.

``` csharp
class Bar : Foo
{
    public void Bang() => WriteLine("Bang from Bar");
}
```

This will cause an compile time error because it is ambiguous as to whether you meant to hide the base member or whether you meant to override it.  To hide a base member we need to use the `new` keyword.

``` csharp
class Bar : Foo
{
    public new void Bang() => WriteLine("Bang from Bar");
}
```

Now `Bang()` has no relation to the `base.Bang()`.  Both exist, unlike overriding where the base implementation is effectively made `protected`, only accessible via the `base` keyword in the sub-class.  We can still access both, but by