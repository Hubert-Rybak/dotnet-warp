namespace DotnetWarp.Extensions
{
    public static class StringExtensions
    {
        public static string WithQuotes(this string text)
        {
            return $"\"{text}\"";
        }
    }
}