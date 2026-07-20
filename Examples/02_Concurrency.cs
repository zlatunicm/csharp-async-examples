using System.Diagnostics;

namespace AsyncLab.Examples;

/// <summary>
/// SEQUENTIAL vs CONCURRENT — the single biggest practical async win.
///
///   await A(); await B(); await C();      -> durations ADD UP (sequential)
///   await Task.WhenAll(A(), B(), C());    -> duration = the SLOWEST one
///
/// Start the tasks first (hot!), await them together. Also here:
/// WhenAny (first winner), and the .NET 9-style Task.WhenEach idea via WhenAny loop.
/// </summary>
public static class ConcurrencyDemo
{
    public static async Task Run()
    {
        // ── Sequential: 100 + 120 + 80 ≈ 300 ms ─────────────────────────────
        Helper.Heading("Sequential awaits — times add up");
        var sw = Stopwatch.StartNew();
        var cjenik = await FetchAsync("cjenik", 100);
        var lager  = await FetchAsync("lager", 120);
        var kupci  = await FetchAsync("kupci", 80);
        Helper.Info($"Total: {sw.ElapsedMilliseconds} ms for {Helper.AsText(new[] { cjenik, lager, kupci })}");

        // ── Concurrent: max(100, 120, 80) ≈ 120 ms ──────────────────────────
        Helper.Heading("Task.WhenAll — start together, await together");
        sw.Restart();
        Task<string> t1 = FetchAsync("cjenik", 100);     // all three RUNNING now
        Task<string> t2 = FetchAsync("lager", 120);
        Task<string> t3 = FetchAsync("kupci", 80);

        string[] results = await Task.WhenAll(t1, t2, t3);
        Helper.Info($"Total: {sw.ElapsedMilliseconds} ms for {Helper.AsText(results)}");
        Helper.Info("3x the work, ~1x the time — THIS is why async matters in services.");

        // ── WhenAny — take the fastest ───────────────────────────────────────
        Helper.Heading("Task.WhenAny — first one wins");
        sw.Restart();
        var mirror1 = FetchAsync("mirror-eu", 90);
        var mirror2 = FetchAsync("mirror-us", 160);
        Task<string> winner = await Task.WhenAny(mirror1, mirror2);
        Helper.Info($"Winner after {sw.ElapsedMilliseconds} ms: {await winner}");
        Helper.Info("(the slower task still finishes in the background — remember that!)");

        // ── Process-as-they-complete loop ────────────────────────────────────
        Helper.Heading("Handle results AS they complete (WhenAny loop)");
        sw.Restart();
        var pending = new List<Task<string>>
        {
            FetchAsync("spori", 150), FetchAsync("srednji", 90), FetchAsync("brzi", 40),
        };
        while (pending.Count > 0)
        {
            var done = await Task.WhenAny(pending);
            pending.Remove(done);
            Helper.Trace(sw, $"finished: {await done}");
        }
        Helper.Info("(.NET 9 adds Task.WhenEach that does exactly this loop for you.)");

        // ── The classic mistake ──────────────────────────────────────────────
        Helper.Heading("Anti-pattern check");
        Helper.Info("var a = await A(); var b = await B();   <- B WAITS for A for no reason");
        Helper.Info("var ta = A(); var tb = B(); await Task.WhenAll(ta, tb);   <- concurrent");
        Helper.Info("Only sequence awaits when B genuinely NEEDS A's result.");
    }

    private static async Task<string> FetchAsync(string name, int ms)
    {
        await Task.Delay(ms);
        return name;
    }
}
