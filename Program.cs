using AsyncLab;
using AsyncLab.Examples;

// ─────────────────────────────────────────────────────────────────────────────
//  ASYNC / AWAIT  —  part 04 of the C# practice series
//  Note: Main itself is async here — 'await' works at the top level (C# 7.1+).
// ─────────────────────────────────────────────────────────────────────────────

var demos = new (string Title, Func<Task> Method)[]
{
    ("01 — Tasks & await basics",                 BasicsDemo.Run),
    ("02 — Sequential vs concurrent (WhenAll)",   ConcurrencyDemo.Run),
    ("03 — Exceptions in async code",             ExceptionsDemo.Run),
    ("04 — CancellationToken",                    CancellationDemo.Run),
    ("05 — Deadlocks & async all the way",        DeadlockDemo.Run),
    ("06 — CPU-bound: Task.Run & Parallel",       CpuBoundDemo.Run),
    ("07 — IAsyncEnumerable (await foreach)",     AsyncStreamsDemo.Run),
};

if (args.Length > 0 && int.TryParse(args[0], out var number) && number >= 1 && number <= demos.Length)
{
    await RunDemo(demos[number - 1]);
}
else
{
    Console.WriteLine("Running ALL examples. (Tip: `dotnet run 3` runs only example #3.)");
    foreach (var demo in demos)
        await RunDemo(demo);
}

static async Task RunDemo((string Title, Func<Task> Method) demo)
{
    Console.WriteLine();
    Console.WriteLine(new string('═', 62));
    Console.WriteLine($"  {demo.Title}");
    Console.WriteLine(new string('═', 62));
    await demo.Method();
}
