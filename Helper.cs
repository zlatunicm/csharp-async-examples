namespace AsyncLab;

/// <summary>Helper methods for tidier console output (same as the whole series).</summary>
public static class Helper
{
    public static void Heading(string text)
    {
        Console.WriteLine();
        Console.WriteLine($"── {text} " + new string('─', Math.Max(3, 58 - text.Length)));
    }

    public static void Info(string text) => Console.WriteLine("   " + text);

    public static string AsText<T>(IEnumerable<T> collection) =>
        "[" + string.Join(", ", collection) + "]";

    /// <summary>Info line prefixed with a stopwatch-style timestamp and thread id.</summary>
    public static void Trace(System.Diagnostics.Stopwatch sw, string text) =>
        Console.WriteLine($"   [{sw.ElapsedMilliseconds,4} ms | t{Environment.CurrentManagedThreadId,2}] {text}");
}
