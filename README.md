# C# Async/Await — Hands-on Examples

Part **04** of a hands-on C# practice series (01 collections, 02 LINQ, 03 delegates & events). Seven runnable, heavily commented examples covering **the most-asked advanced C# topic**: what `await` really does, `WhenAll` concurrency, where async exceptions go, `CancellationToken`, the famous `.Result` deadlock, CPU-bound vs I/O-bound, and `IAsyncEnumerable` streams. Timestamped + thread-id output (`[ 123 ms | t 4]`) lets you *see* threads being freed and work overlapping.

## Running

```bash
dotnet run        # all examples
dotnet run 5      # just one (e.g. deadlocks)
```

## What's inside

| # | Topic | Highlights |
|---|-------|------------|
| 01 | Tasks & await basics | thread ids across awaits, hot tasks, state machine, `async void` |
| 02 | Sequential vs concurrent | 300 ms → 120 ms with `WhenAll`, `WhenAny`, as-they-complete loop |
| 03 | Exceptions | throw-at-await, fire-and-forget swallowing, `WhenAll` + `AggregateException` |
| 04 | CancellationToken | `CancelAfter`, manual cancel, linked tokens, cooperative model |
| 05 | Deadlocks | anatomy of `.Result` deadlock, `ConfigureAwait(false)`, thread starvation |
| 06 | CPU-bound | `Task.Run`, `Parallel.ForEach` + `Interlocked`, `Parallel.ForEachAsync` throttle |
| 07 | IAsyncEnumerable | `await foreach`, streaming vs `Task<List<T>>`, `[EnumeratorCancellation]` |

## The five rules to take away

1. `await` frees the thread — it never blocks one.
2. Start tasks first, `await Task.WhenAll(...)` together — durations stop adding up.
3. Never block on async (`.Result`/`.Wait()`); go async **all the way** to `Main`.
4. Pass a `CancellationToken` through every async API you write.
5. I/O-bound → `await`. CPU-bound → `Task.Run` / `Parallel`. Don't mix them up.

## Further reading

- [Asynchronous programming (C# guide)](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/)
- [Task-based asynchronous pattern](https://learn.microsoft.com/en-us/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap)
- [ConfigureAwait FAQ (Stephen Toub)](https://devblogs.microsoft.com/dotnet/configureawait-faq/)

## Author

**Matej Zlatunić**

## License

[MIT](https://opensource.org/licenses/MIT).
