using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace AsyncLab.Examples;

/// <summary>
/// IAsyncEnumerable&lt;T&gt; — async streams (C# 8, 2019): 'yield return' + 'await'
/// in the SAME method, consumed with 'await foreach'.
///
/// Regular IEnumerable pulls items synchronously; Task&lt;List&lt;T&gt;&gt; waits for ALL
/// items before giving you any. An async stream gives you each item AS IT
/// ARRIVES — perfect for paged APIs, DB cursors, message queues, log tailing.
///
/// Real-world: EF Core's AsAsyncEnumerable(), ASP.NET Core can stream
/// IAsyncEnumerable responses as they're produced.
/// </summary>
public static class AsyncStreamsDemo
{
    public static async Task Run()
    {
        var sw = Stopwatch.StartNew();

        // ── await foreach ────────────────────────────────────────────────────
        Helper.Heading("await foreach — items as they arrive");
        await foreach (var order in LoadOrdersAsync())
            Helper.Trace(sw, $"stigla narudzba: {order}");
        Helper.Info("Task<List<T>> would show NOTHING for ~240 ms, then everything.");
        Helper.Info("The stream shows each one immediately — better UX, less memory.");

        // ── Streams compose with LINQ-style operators (manually here) ────────
        Helper.Heading("Filtering an async stream");
        await foreach (var order in LoadOrdersAsync())
        {
            if (!order.StartsWith("kava")) continue;      // plain code works fine
            Helper.Trace(sw, $"samo kave: {order}");
        }
        Helper.Info("(The System.Linq.Async NuGet package adds full WhereAwait/Select...)");

        // ── Cancellation flows in via EnumeratorCancellation ─────────────────
        Helper.Heading("Cancelling an async stream");
        using var cts = new CancellationTokenSource(130);
        try
        {
            await foreach (var order in LoadOrdersAsync(cts.Token))
                Helper.Trace(sw, $"prije prekida: {order}");
        }
        catch (OperationCanceledException)
        {
            Helper.Trace(sw, "stream CANCELLED mid-flight — no wasted fetches");
        }

        // ── When to reach for it ─────────────────────────────────────────────
        Helper.Heading("When to use which");
        Helper.Info("all items at once, small set     -> Task<List<T>>");
        Helper.Info("items trickle in / huge / paged  -> IAsyncEnumerable<T>");
        Helper.Info("(and remember: it's LAZY like LINQ — runs on enumeration)");
    }

    // 'async' + 'yield return' together — the compiler builds an async iterator.
    // [EnumeratorCancellation] wires the token passed by 'WithCancellation' /
    // the direct-call argument into our loop.
    private static async IAsyncEnumerable<string> LoadOrdersAsync(
        [EnumeratorCancellation] CancellationToken token = default)
    {
        string[] orders = ["kava #1", "kolac #2", "kava #3", "sendvic #4"];
        foreach (var order in orders)
        {
            await Task.Delay(60, token);      // simulate fetching the next page
            yield return order;               // hand it over IMMEDIATELY
        }
    }
}
