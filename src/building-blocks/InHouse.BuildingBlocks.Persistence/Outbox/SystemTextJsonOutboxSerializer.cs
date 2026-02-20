using System.Text.Json;

namespace InHouse.BuildingBlocks.Persistence.Outbox;

public sealed class SystemTextJsonOutboxSerializer : IOutboxSerializer
{
    private readonly JsonSerializerOptions _options;

    public SystemTextJsonOutboxSerializer(JsonSerializerOptions? options = null)
    {
        _options = options ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    public string Serialize(object value)
        => JsonSerializer.Serialize(value, _options);
}
