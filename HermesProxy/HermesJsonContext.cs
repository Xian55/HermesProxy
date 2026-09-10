using System.Collections.Generic;
using System.Text.Json.Serialization;
using HermesProxy.World.Server;

namespace HermesProxy;

// Source-generated System.Text.Json metadata for everything HermesProxy serializes, so the
// trimmed publish never falls back on reflection-based serialization. WriteIndented only
// shapes output; reads accept exactly what the default options accept.
[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(PlayerSettings.InternalStorage))]
[JsonSerializable(typeof(CollectionFavorites))]
[JsonSerializable(typeof(Dictionary<string, object>))]
internal sealed partial class HermesJsonContext : JsonSerializerContext
{
}
