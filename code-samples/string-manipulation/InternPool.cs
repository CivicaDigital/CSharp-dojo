namespace BanksySan.Workshops.AdvancedCSharp.StringManipulation
{
    using static System.Console;

    static class InternPool
    {
        private static void Main()
        {
            const string HELLO = "Hello";
            const string WORLD = "World";

            var s1 = $"{HELLO} {WORLD}";
            var s2 = "Hello World";
            WriteLine($"s1 = '{s1}'.");
            WriteLine($"s2 = '{s2}'.");
            WriteLine($"s1 == s2 ? {s1 == s2}");  
            var o1 = (object) s1;
            var o2 = (object) s2;
            WriteLine($"o1 == s1 ? {o1 == s1}");
            WriteLine($"o1 == s2 ? {o2 == s2}");
            WriteLine($"o1 == o2 ? {o1 == o2}");
            WriteLine($"(string) o1 == (string) o2 ? {(string) o1 == (string) o2}");
        }
    }
}