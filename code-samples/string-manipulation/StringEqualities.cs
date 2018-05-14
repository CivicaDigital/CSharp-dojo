namespace BanksySan.Workshops.AdvancedCSharp.StringManipulation
{
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
            WriteLine($"ReferenceEquals(s1, s2) ? {ReferenceEquals(s1, s2)}");
        }
    }
}