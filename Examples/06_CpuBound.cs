using System.Diagnostics;

namespace AsyncLab.Examples;

/// <summary>
/// CPU-BOUND WORK — when the bottleneck is the processor, not waiting.
///
/// The most misunderstood distinction in async C#:
///
///   I/O-bound  (network, disk, DB, timers) -> async/await. NO extra threads —
///              the hardware does the waiting.
///   CPU-bound  (math, parsing, image processing) -> Task.Run (offload to the
///              thread POOL) or Parallel.For* (use ALL the cores).
///
/// Interview favorite: "Does async/await create threads?"  NO. await Task.Delay
/// uses zero threads while waiting. Task.Run is the one that borrows a pool thread.
/// </summary>
public static class CpuBoundDemo
{
    public static async Task Run()
    {
        // ── Task.Run — don't hog the caller's thread ─────────────────────────
        Helper.Heading("Task.Run — offload heavy math to the pool");
        var sw = Stopwatch.StartNew();
        long primes = await Task.Run(() => CountPrimes(2, 200_000));
        Helper.Trace(sw, $"primes below 200k: {primes} (computed on a pool thread)");
        Helper.Info("In a UI app this keeps the interface alive; in ASP.NET you");
        Helper.Info("usually DON'T Task.Run — the request thread is a pool thread already.");

        // ── Parallel.For — all cores on deck ─────────────────────────────────
        Helper.Heading("Parallel.For — split a CPU job across cores");
        sw.Restart();
        long sequential = CountPrimes(2, 400_000);
        var seqMs = sw.ElapsedMilliseconds;

        sw.Restart();
        long parallel = 0;
        var ranges = Enumerable.Range(0, 8).Select(i => (from: 2 + i * 50_000, to: 2 + (i + 1) * 50_000));
        Parallel.ForEach(ranges, range =>
        {
            long local = CountPrimes(range.from, range.to);
            Interlocked.Add(ref parallel, local);      // thread-safe accumulation!
        });
        Helper.Info($"sequential: {seqMs} ms, parallel: {sw.ElapsedMilliseconds} ms (istih {parallel} prostih, {Environment.ProcessorCount} jezgri)");

        // ── Parallel.ForEachAsync (.NET 6) — throttled async fan-out ─────────
        Helper.Heading("Parallel.ForEachAsync — N concurrent DOWNLOADS, capped");
        sw.Restart();
        string[] urls = ["cjenik", "lager", "kupci", "racuni", "kartice", "stanja"];
        await Parallel.ForEachAsync(urls,
            new ParallelOptions { MaxDegreeOfParallelism = 3 },
            async (url, token) =>
            {
                await Task.Delay(60, token);           // simulated I/O
                Helper.Trace(sw, $"gotov: {url}");
            });
        Helper.Info("6 jobs, max 3 at once -> two waves. The built-in throttle is the point.");

        // ── The rule ─────────────────────────────────────────────────────────
        Helper.Heading("Cheat sheet");
        Helper.Info("waiting on something?   await it        (0 threads)");
        Helper.Info("computing something?    Task.Run        (1 pool thread)");
        Helper.Info("computing a LOT?        Parallel.For*   (all cores)");
        Helper.Info("many async calls, capped? Parallel.ForEachAsync(MaxDegree...)");
    }

    private static long CountPrimes(int from, int to)
    {
        long count = 0;
        for (int n = from; n < to; n++)
        {
            bool prime = n > 1;
            for (int d = 2; d * d <= n && prime; d++)
                if (n % d == 0) prime = false;
            if (prime) count++;
        }
        return count;
    }
}
