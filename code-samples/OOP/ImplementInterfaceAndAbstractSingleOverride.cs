namespace BanksySan.Workshops.AdvancedCSharp.Oop
{
    using System;
    using static System.Console;

    internal static class ImplementInterfaceAndAbstractSingleOverride
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

            Console.WriteLine("Calling Boink with FooBar");
            Boink(fooBar);
            Console.WriteLine("Calling Boink with Bar");
            Boink((Bar) fooBar);
            Console.WriteLine("Calling Boink with Foo");
            Boink((IFoo) fooBar);
        }

        private static void Boink(Bar bar) => WriteLine("Boink in Bar");
        private static void Boink(IFoo foo) => WriteLine("BBoink from foo");
        //private static void Boink(FooBar foo) => WriteLine("BBoink from FooBar");
    }

    internal interface IFoo
    {
        void Bang();
    }

    class FooBar: Bar, IFoo
    {
        void IFoo.Bang() => WriteLine("BANG! (from IFoo)");

        public override void Bang() => WriteLine("BANG! (From Bar)");
    }

    internal abstract class Bar
    {
        public abstract void Bang();
    }

    internal class Foo : IFoo
    {
        public void Bang() => WriteLine("BANG!");
    }
}