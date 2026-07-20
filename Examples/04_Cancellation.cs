using System.Diagnostics;

namespace AsyncLab.Examples;

/// <summary>
/// CancellationToken — .NET's ONE way to say "stop, nobody needs this anymore".
///
/// The pattern (cooperative cancellation):
///   1. Someone creates a CancellationTokenSource (cts).
///   2. Its .Token travels down through every async call.
///   3. Work either passes it on (Task.Delay, HttpClient...) or checks it
///      (ThrowIfCancellationRequested) at safe points.
///   4. cts.Cancel() -> the whole chain unwinds with OperationCanceledException.
///
/// Every serious async API takes a CancellationToken — yours should too.
/// ASP.NET hands you one per request that fires when the client disconnects.
/// </summary>
public static class CancellationDemo
{
    public static async Task Run()
    {
        // ── Cancel after a timeout ───────────────────────────────────────────
        Helper.Heading("CancelAfter — a timeout in one line");
        var sw = Stopwatch.StartNew();
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(120));

        try
        {
            await ProcessOrdersAsync(count: 10, perItemMs: 40, cts.Token);
            Helper.Info("finished everything (won't happen)");
        }
        catch (OperationCanceledException)
        {
            Helper.Trace(sw, "CANCELLED — the token fired mid-work, the chain unwound");
        }

        // ── Manual cancel ────────────────────────────────────────────────────
        Helper.Heading("Manual Cancel() from 'outside'");
        sw.Restart();
        using var manual = new CancellationTokenSource();
        Task worker = ProcessOrdersAsync(count: 100, perItemMs: 30, manual.Token);

        await Task.Delay(80);                      // let it do some work...
        Helper.Trace(sw, "user pressed 'Odustani' -> Cancel()");
        manual.Cancel();

        try { await worker; }
        catch (OperationCanceledException) { Helper.Trace(sw, "worker stopped promptly"); }

        // ── Checking the token in CPU loops ──────────────────────────────────
        Helper.Heading("Cooperative = YOU choose the safe points");
        Helper.Info("I/O APIs (Task.Delay, HttpClient) honor the token for you.");
        Helper.Info("In your own loops call token.ThrowIfCancellationRequested()");
        Helper.Info("once per iteration — cancellation is COOPERATIVE, not preemptive;");
        Helper.Info("nobody kills your thread mid-instruction (thankfully).");

        // ── Linked tokens ────────────────────────────────────────────────────
        Helper.Heading("Linked tokens — timeout AND user-cancel together");
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var user = new CancellationTokenSource();
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, user.Token);
        Helper.Info("linked.Token fires when EITHER source fires — pass THAT one down.");
    }

    private static async Task ProcessOrdersAsync(int count, int perItemMs, CancellationToken token)
    {
        for (int i = 1; i <= count; i++)
        {
            // Task.Delay itself honors the token — cancels the wait instantly:
            await Task.Delay(perItemMs, token);
            Helper.Info($"obradio narudzbu #{i}");
        }
    }
}
