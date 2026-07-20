namespace AsyncLab.Examples;

/// <summary>
/// EXCEPTIONS IN ASYNC CODE — where do they actually go?
///
/// - An exception inside an async method is CAPTURED and stored on the Task.
/// - It re-throws at the 'await' — so try/catch around await works naturally.
/// - Un-awaited ("fire and forget") tasks swallow their exceptions silently!
/// - WhenAll: awaiting it throws only the FIRST exception; the rest hide in
///   Task.Exception (an AggregateException) — a classic interview detail.
/// </summary>
public static class ExceptionsDemo
{
    public static async Task Run()
    {
        // ── try/catch around await just works ────────────────────────────────
        Helper.Heading("The exception surfaces at the await");
        try
        {
            await FailAsync("baza nedostupna", delayMs: 50);
        }
        catch (InvalidOperationException e)
        {
            Helper.Info($"Caught normally: '{e.Message}'");
        }

        // ── The exception lives ON the task until awaited ────────────────────
        Helper.Heading("Not awaited = not thrown (yet)");
        Task failing = FailAsync("disk pun", delayMs: 10);
        await Task.Delay(50);                       // it has already failed by now...
        Helper.Info($"Status without await: {failing.Status}");        // Faulted
        Helper.Info("No crash so far! The exception waits inside the Task object.");
        try { await failing; }
        catch (InvalidOperationException e) { Helper.Info($"...and throws on await: '{e.Message}'"); }

        // ── Fire-and-forget swallows exceptions ──────────────────────────────
        Helper.Heading("Fire-and-forget — the silent killer");
        _ = FailAsync("nitko me nece vidjeti", delayMs: 10);   // discarded task
        await Task.Delay(50);
        Helper.Info("The discarded task failed — and NOBODY noticed. In production");
        Helper.Info("wrap fire-and-forget work in a method that logs its own errors.");

        // ── WhenAll and multiple failures ────────────────────────────────────
        Helper.Heading("WhenAll throws the FIRST, hides the rest");
        var t1 = FailAsync("greska A", 20);
        var t2 = FailAsync("greska B", 30);
        var t3 = OkAsync("uspjeh C", 10);
        Task all = Task.WhenAll(t1, t2, t3);
        try { await all; }
        catch (InvalidOperationException e)
        {
            Helper.Info($"await threw ONE exception: '{e.Message}'");
        }
        // The full set is on the WhenAll task itself:
        var everyMessage = all.Exception!.InnerExceptions.Select(x => x.Message);
        Helper.Info($"all.Exception has BOTH: {Helper.AsText(everyMessage)}");
        Helper.Info("Need per-task outcomes? Keep the task references and inspect each.");
    }

    private static async Task FailAsync(string message, int delayMs)
    {
        await Task.Delay(delayMs);
        throw new InvalidOperationException(message);
    }

    private static async Task<string> OkAsync(string result, int delayMs)
    {
        await Task.Delay(delayMs);
        return result;
    }
}
