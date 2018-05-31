# String Manipulation

Strings can get you into a knot.  They're reference types, but we create them with a literals like a primitive.  They're passed by reference, but the reference could point to the heap or to the intern pool.  In functional terms, they're an array of `char`, but we can't implicitly or explicitly cast between the two.

## String Equalities

Equality in strings isn't simple.  Take this example:

``` csharp
using static System.Console;

internal static class StringEqualities
{
    private static void Main()
    {
        const string s1 = "Hello World";
        var helloWorldCharArray = s1.ToCharArray();
        var s2 = new string(helloWorldCharArray);
        var o1 = (object) s1;

        WriteLine($"s1 = '{s1}'.");
        WriteLine($"s2 = '{s2}'.");
        WriteLine($"o2 = '{o1}'.");
        WriteLine($"s1 == s2 ? {s1 == s2}");
        WriteLine($"o1 == s1 ? {o1 == s1}");
        WriteLine($"o1 == s2 ? {o1 == s2}");
    }
}
```

We create a literal string `Hello World`, then I create a new string using the same characters as `s1`.  We create a variable `o1` that's just the string cast as an `object`.  When we test equalities we get some strange results though.

    s1 = 'Hello World'.
    s2 = 'Hello World'.
    o2 = 'Hello World'.
    s1 == s2 ? True
    o1 == s1 ? True
    o1 == s2 ? False

`s1`, `s2` and `o1` all contain the same characters, but `s1` and `o1` are different objects to `s2`.  One would expect `s1` and `o1` to be equal and neither be equal to `s2` then, but that's not what we see.

When we compare the strings it seems that they're compared like a value type, but when we compare to `o1` we see the expected reference type comparison.  When we look at the source code for the `String` type we can see why this is.  `Equals()` is overloaded to with a method that performs a value comparison between the strings.  To make things more confusing try the following comparisons:

``` csharp
WriteLine($"o1.Equals(s2) ? {o1.Equals(s2)}");
WriteLine($"s2.Equals(o1) ? {s2.Equals(o1)}");
```

You'll see that both return `True`.  This contradicts `o1 == s2`!  Again, the reason for this is the strange overriding in strings.  The `==` operator is overloaded to call `Equals`, so when we're comparing the two strings with the `==` we're really calling the `Equals` method which performs a value comparison.

The operator overriding isn't actually an override though (there's no `override` keyword).  When we cast to an `object` therefore we cause the object's `==` to be used, not the string's `==`.

## String interning

The CLR has a special place to store strings it know about at compile time (i.e. literals), as well as any we explicitly put there.  The intern pool means that comparisons between those strings are faster, at the expense of the application using more memory.

The intern pool actually lives in the large object heap.  This means that it's never garbage collected and therefore never defragmented.  It means that comparing identical strings by reference may., or may not return true.

``` csharp
using static System.Console;

internal static class InternPool
{
    private static void Main()
    {
        const string S1 = "abc";
        const string S2 = "abc";
        var s1Chars = S1.ToCharArray();
        var s3 = new string(s1Chars);

        WriteLine($"S1 = '{S1}'.");
        WriteLine($"S1 = '{S2}'.");
        WriteLine($"s3 = '{s3}'.");

        WriteLine($"ReferenceEquals(s1, s2) ? {ReferenceEquals(S1, S2)}");
        WriteLine($"ReferenceEquals(s1, s3) ? {ReferenceEquals(S1, s3)}");

        WriteLine();
    }
}
```

We declare three strings here.  `S1` and `S2` are declared with literals, `s3` is generated from the `char` array that makes `s1`.  Running this gives us:

    S1 = 'abc'.
    S1 = 'abc'.
    s3 = 'abc'.
    ReferenceEquals(s1, s2) ? True
    ReferenceEquals(s1, s3) ? False

Even though `S1` and `S2` are declared independently of each other they both reference the same string instance.  This is because the compiler knew these at compile time is was able to embed it into the assembly.  If you inspect the assembly with IL DASM you'll see that the metadata has a section called _User Strings_.

    User Strings
    -------------------------------------------------------
    70000001 : ( 3) L"abc"
    70000009 : (11) L"S1 = '{0}'."
    70000021 : (11) L"s3 = '{0}'."
    70000039 : (29) L"ReferenceEquals(s1, s2) ? {0}"
    70000075 : (29) L"ReferenceEquals(s1, s3) ? {0}"

All strings in the User Strings section get interned when the application starts.  The way `s3` is formed successfully hides from the compiler that it is a constant string, so the compiler created it afresh on the heap.

> NB:  Comparing strings by reference isn't reliable even if the string is interned.  As the CLR tries to optimise the runtime it will often intern the same string more than once.

## String Formatting

Formatting strings is achieved overloading by the `ToString()` method, though there are ways to format without having to call this explicitly.  For a type to be formattable we need to implement the `IFormattable` interface.

``` csharp
using System;
using System.Globalization;

internal class MyFormattableType : IFormattable
{
    public MyFormattableType(string message)
    {
        Message = message;
    }

    public string Message { get; }

    public override string ToString()
    {
        return ToString("G");
    }

    public string ToString(string format)
    {
        return ToString(format, CultureInfo.CurrentCulture);
    }

    public string ToString(string format, IFormatProvider formatProvider)
    {
        switch (format)
        {
            case "G":
            case null:
                return Message;
            case "Upper":
                return Message.ToUpperInvariant();
            case "Lower":
                return Message.ToLowerInvariant();
            default:
                return format;
        }
    }
}
```

The `IFormattable` interface only has one method, `ToString(string format, IFormatProvider formatProvider)` however it is usual to implement the overload that accept only the format string.

The documentation states that there must be a format `G` (for _general_) which is the default format if `null` is passed.  Defaulting the `IFormatProvider` to the current culture is also recommended and consistent with the CLS types that implement `IFormattable`.

Using the `ToString` methods directly is fine if we only want that string, but often we want to embed the output into some other string.  We can achieve this with the `string.Format` method.

``` csharp
var f = new FormattableType("Hello World");
string.Format("General: {0:G}, Upper: {0:Upper}, {0:Lower}", f)
```

This will output:

    General: Hello World, Upper: HELLO WORLD, hello world

Likewise we can use the interpolated strings introduced in C# 6.0:

``` csharp
WriteLine($"Message = \"{myFormattableType}\"");
WriteLine($"null format = \"{myFormattableType}\"");
WriteLine($"\"G\" format = \"{myFormattableType:G}\"");
WriteLine($"\"Upper\" format = \"{myFormattableType:Upper}\"");
WriteLine($"\"Lower\" format = \"{myFormattableType:Lower}\"");
WriteLine($"\"Unknown\" format = \"{myFormattableType:Unknown}\"");
```

Which will output:

    Message = "Hello World"
    null format = "Hello World"
    "G" format = "Hello World"
    "Upper" format = "HELLO WORLD"
    "Lower" format = "hello world"
    "Unknown" format = "Unknown"

## Format Providers

Format providers, and custom formatters are the other side of the coin to the `IFormattable` interface.  IFormattable allows a type to define its own formatting, whereas format providers allow formatting to define formatting externally, so that it can be reused.

Formatting providers have two key actors:

1. `IFormatProvider`
1. `ICustomFormatter`

The `IFormatProvider` is responsible for deciding which formatter (`ICustomFormatter`) should be used to perform the actual string manipulation.  Is has one method which accepts a `Type` as an argument, the `Type` tells the provider what type of formatter it thinks it needs.

They are, _in my opinion_, really badly designed and very unintuitive but once you penny drops they're quite usable.

The `IFormatProvider` interface contains one method:

``` csharp
public interface IFormatProvider
{
    object GetFormat(Type formatType);
}
```

The runtime will call this object first (if you provided it), passing in a `Type` which represents tha type of formatter it expects.  If you return the wrong one then it'll just ignore it and invoke whatever default one it has.

We can create a provider that just logs out what it's getting to the console and then call it from various objects to see what's received:

``` csharp
using System;
using static System.Console;

internal class LoggingFormatProvider : IFormatProvider
{
    public object GetFormat(Type formatType)
    {
        WriteLine($"formatType: {formatType.FullName}");
        return null;
    }
}

internal static class CtsFormatters
{
    private static void Main()
    {
        var _ = default(string);
        var myLoggingProvider = new LoggingFormatProvider();

        WriteLine("Integer:");
        _ = default(int).ToString(myLoggingProvider);

        WriteLine("Decimal:");
        _ = default(decimal).ToString(myLoggingProvider);

        WriteLine("DateTime:");
        _ = default(DateTime).ToString(myLoggingProvider);

        WriteLine("Boolean:");
        _ = default(bool).ToString(myLoggingProvider);

        WriteLine("String:");
        _ = string.Empty.ToString(myLoggingProvider);

        WriteLine("string.Format:");
        _ = string.Format(myLoggingProvider,"{0}", string.Empty);
    }
}
```

The output from this is quite interesting:

    Integer:
    formatType: System.Globalization.NumberFormatInfo
    Decimal:
    formatType: System.Globalization.NumberFormatInfo
    DateTime:
    formatType: System.Globalization.DateTimeFormatInfo
    Boolean:
    String:
    string.Format:
    formatType: System.ICustomFormatter

`Boolean` and `String` don't call the provider at all, even though we pass it one.  This is actually documented behaviour, any provider is ignored, odd?  Maybe, but it does exclude them from any further part in this discussion.

Both the numeric types pass a `NumberFormatInfo` to the provider, and the date type passes a `DateTimeFormatInfo`.

Finally, when we call the `Format` method on string we get an `ICustomFormatter` type passed.  This is our opportunity to use a custom formatter we can create.

If we return either `null`, or an object of type different to the `formatType` then the runtime will just fallback to it's default, maybe making multiple requests with different type it will accept prior to this.

## The Custom Formatter

We'll create a formatter that will replace all the digits with the word for the number.  The `ICustomFormatter` interface has a single method also.

``` csharp
public interface ICustomFormatter
{
    string Format(string format, object arg, IFormatProvider formatProvider);
}
```

The `arg` object is the object to be converted to a string and formatted.  The `format` [composite string](https://docs.microsoft.com/en-gb/dotnet/standard/base-types/composite-formatting#composite-format-string) is the format supplied in the placeholder in the format string.  The `formatProvider` is a reference back up to the provider that supplied this `ICustomFormatter` implementation.

A composite string has the following structure:

```csharp
"Foo {FormatItem1} Bar {FormatItem2}"
```

Where the format items have the structure:

    {index[,alignment:formatString]}

