namespace AsyncLab.Examples;

/// <summary>
/// DEADLOCKS &amp; "ASYNC ALL THE WAY" — the most famous async interview question:
/// "Why does .Result deadlock?"
///
/// The classic scenario (UI apps / old ASP.NET):
///   1. UI thread calls  var x = GetAsync().Result;   and BLOCKS waiting.
///   2. Inside GetAsync, the await finishes and wants to resume ON THE UI
///      THREAD (its captured SynchronizationContext).
///   3. But the UI thread is blocked in .Result... waiting for GetAsync.
///   -> Each waits for the other. Forever. 💀
///
/// Console apps and modern ASP.NET Core have NO SynchronizationContext, so the
/// same code "happens to work" — which is exactly why the bug ships to prod.
///
/// Rules:
///   - Never block on async: no .Result, no .Wait(), no .GetAwaiter().GetResult()
///     on unfinished tasks. Go async ALL the way up to Main.
///   - In LIBRARY code add .ConfigureAwait(false) to awaits — "I don't need to
///     resume on the original context" — faster and deadlock-immune.
/// </summary>
public static class DeadlockDemo
{
    public static async Task Run()
    {
        // ── Blocking wastes; awaiting frees ──────────────────────────────────
        Helper.Heading(".Result vs await (in a console app)");
        Helper.Info("Here .Result would 'work' (no sync context) — but it still");
        Helper.Info("BLOCKS a whole thread for the duration. await blocks none.");

        // We demonstrate the blocked-thread cost safely:
        var sw = System.Diagnostics.Stopwatch.StartNew();
        int result = await ComputeAsync();          // correct: thread stays free
        Helper.Trace(sw, $"await result = {result}");

        // ── The deadlock, narrated (we won't hang ourselves on purpose) ──────
        Helper.Heading("Anatomy of the classic deadlock");
        Helper.Info("UI thread:    var data = LoadAsync().Result;      // blocks UI thread");
        Helper.Info("LoadAsync:    await http.GetAsync(url);           // captures UI context");
        Helper.Info("              // completion wants the UI thread to resume...");
        Helper.Info("              // ...but the UI thread is stuck in .Result");
        Helper.Info("=> mutual wait, app frozen. Fix: 'await LoadAsync()' instead.");

        // ── ConfigureAwait(false) ────────────────────────────────────────────
        Helper.Heading("ConfigureAwait(false) — the library rule");
        await LibraryWorkAsync();
        Helper.Info("Library code doesn't touch UI, so it resumes on ANY pool thread:");
        Helper.Info("faster (no context hop) and immune to the deadlock above.");
        Helper.Info("App-level code (controllers, UI handlers) usually omits it.");

        // ── The 'sync over async' tax ────────────────────────────────────────
        Helper.Heading("Why blocking hurts servers even without deadlock");
        Helper.Info("Every .Result parks one THREAD per waiting request. Threads are");
        Helper.Info("~1 MB of stack each and the pool grows slowly — under load, the");
        Helper.Info("service starves. await scales to thousands of requests on a");
        Helper.Info("handful of threads. That's the whole point of async servers.");
    }

    private static async Task<int> ComputeAsync()
    {
        await Task.Delay(60);
        return 42;
    }

    private static async Task LibraryWorkAsync()
    {
        // The habit to build in reusable/library code:
        await Task.Delay(30).ConfigureAwait(false);
        await Task.Delay(30).ConfigureAwait(false);
    }
}
