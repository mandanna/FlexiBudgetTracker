namespace ExpenseTracker.Api.Extensions
{
    public static class StringExtensions
    {
        // "IncomesToCreate[0].Source" -> "incomesToCreate[0].source"
        public static string ToCamelCasePath(this string path) =>
            string.Join('.', path.Split('.').Select(part =>
                part.Length == 0 ? part : char.ToLower(part[0]) + part[1..]));
    }
}
