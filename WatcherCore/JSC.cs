using System.Text.Json.Serialization;

namespace WatcherCore;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(WatcherSettings))]
internal partial class SerializeOnlyContext : JsonSerializerContext;