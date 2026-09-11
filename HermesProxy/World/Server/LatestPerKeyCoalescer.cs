using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace HermesProxy.World.Server;

/// <summary>
/// Keeps only the newest value per key until the batch is drained. For modern-client bursts of
/// full-state packets about one thing, where the legacy server only needs the last one.
/// </summary>
internal sealed class LatestPerKeyCoalescer<TKey, TValue> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, TValue> _pending = new();
    private int _armed, _offered;

    /// <returns>True for the first value since the last <see cref="Drain"/>: the caller schedules one.</returns>
    public bool Offer(TKey key, TValue value)
    {
        _pending[key] = value;
        Interlocked.Increment(ref _offered);
        return Interlocked.Exchange(ref _armed, 1) == 0;
    }

    /// <param name="offered">How many values were offered into the batch, replaced ones included.</param>
    public List<TValue> Drain(out int offered)
    {
        Interlocked.Exchange(ref _armed, 0);                // reset first: a racing Offer re-arms
        offered = Interlocked.Exchange(ref _offered, 0);
        var values = new List<TValue>();
        foreach (var key in _pending.Keys)
            if (_pending.TryRemove(key, out var latest))    // takes whatever is newest at removal
                values.Add(latest);
        return values;                                      // no defined order between keys
    }
}
