using System.Diagnostics;

namespace AsyncLab.Examples;

/// <summary>
/// TASKS &amp; AWAIT — the foundation (async/await arrived in C# 5, 2012).
///
/// Task            = a promise of work that will finish (maybe with a result).
/// Task&lt;T&gt;         = ...and will hand you a T when it does.
/// await           = "pause THIS method here, FREE the thread, resume when done."
///
/// THE key sentence for interviews: await does NOT block a thread. While the
/// task runs (a timer, network, disk...), the thread goes off to do other work;
/// the rest of the method is registered as a CONTINUATION.
///
/// async is contagious by design: methods that await become async Task methods,
/// their callers await them, and so on up to Main ("async all the way").
/// </summary>
public static class BasicsDemo
{
    public static async Task Run()
    {
        var sw = Stopwatch.StartNew();

        // ── The thread is released during await ─────────────────────────────
        Helper.Heading("await frees the thread (watch the thread ids)");
        Helper.Trace(sw, "before await");
        await Task.Delay(100);              // async pause — NO thread sleeps for this
        Helper.Trace(sw, "after 1st await  (often a DIFFERENT thread resumes!)");
        await Task.Delay(100);
        Helper.Trace(sw, "after 2nd await");
        Helper.Info("Thread.Sleep would HOLD a thread hostage; Task.Delay holds none.");

        // ── Task<T> carries a result ─────────────────────────────────────────
        Helper.Heading("Task<T> — awaiting produces the value");
        Task<string> download = FakeDownloadAsync("cjenik.json", 150);
        Helper.Trace(sw, "download STARTED (method runs until its first await)");
        // ...we could do other things here...
        string content = await download;                 // now we need the result
        Helper.Trace(sw, $"got: '{content}'");

        // ── Async methods return "hot" tasks ─────────────────────────────────
        Helper.Heading("Calling an async method STARTS it (hot task)");
        var t = FakeDownloadAsync("slika.png", 80);
        Helper.Info($"IsCompleted immediately after call: {t.IsCompleted}");
        await t;
        Helper.Info($"IsCompleted after await:            {t.IsCompleted}");

        // ── What async/await compiles into ───────────────────────────────────
        Helper.Heading("Under the hood (the interview answer)");
        Helper.Info("The compiler rewrites an async method into a STATE MACHINE:");
        Helper.Info("each await is a state; the continuation resumes at the right");
        Helper.Info("state when the awaited task completes. Your 'sequential' code");
        Helper.Info("is really a chain of callbacks — written like a story.");

        // ── async void — the forbidden fruit ─────────────────────────────────
        Helper.Heading("async void — only for event handlers");
        Helper.Info("async void cannot be awaited and its exceptions CRASH the app");
        Helper.Info("(nobody can catch them). Always 'async Task' — except UI event");
        Helper.Info("handlers, where the signature forces void.");
    }

    private static async Task<string> FakeDownloadAsync(string file, int ms)
    {
        await Task.Delay(ms);                            // simulate network latency
        return $"{file} ({ms} kB)";
    }
}
