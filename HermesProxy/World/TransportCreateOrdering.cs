using System;
using System.Collections.Generic;

namespace HermesProxy.World;

/// <summary>
/// Ordering rule for CreateObject batches split into one packet each.
/// </summary>
public static class TransportCreateOrdering
{
    /// <summary>
    /// Moves transport creates to the front, preserving relative order otherwise.
    /// Anything riding a transport names it by GUID, so the client has to be given the
    /// transport first or it drops the attachment. Transports reference nothing
    /// themselves, so hoisting them cannot break another dependency.
    /// </summary>
    public static List<T> TransportsFirst<T>(List<T> creates, Func<T, WowGuid128> guidOf)
    {
        var ordered = new List<T>(creates.Count);
        foreach (var item in creates)
            if (guidOf(item).IsTransport())
                ordered.Add(item);

        int transportCount = ordered.Count;
        if (transportCount == 0 || transportCount == creates.Count)
            return creates;

        foreach (var item in creates)
            if (!guidOf(item).IsTransport())
                ordered.Add(item);

        return ordered;
    }

    public static int CountTransports<T>(List<T> creates, Func<T, WowGuid128> guidOf)
    {
        int count = 0;
        foreach (var item in creates)
            if (guidOf(item).IsTransport())
                count++;
        return count;
    }
}
