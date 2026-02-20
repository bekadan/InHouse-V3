using System.Text.Json;
using InHouse.BuildingBlocks.Abstractions.Integration.Bus;
using InHouse.BuildingBlocks.Abstractions.Integration.Events;

namespace InHouse.BuildingBlocks.Api.Integration.Bus;

public sealed class JsonIntegrationEventEnvelopeSerializer : IIntegrationEventEnvelopeSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public byte[] Serialize(IntegrationEventEnvelope envelope)
        => JsonSerializer.SerializeToUtf8Bytes(envelope, Options);

    public IntegrationEventEnvelope Deserialize(ReadOnlySpan<byte> bytes)
        => JsonSerializer.Deserialize<IntegrationEventEnvelope>(bytes, Options)
           ?? throw new InvalidOperationException("Invalid IntegrationEventEnvelope JSON payload.");
}