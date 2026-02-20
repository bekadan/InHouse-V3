using System.Text.Json;
using System.Text.Json.Nodes;
using InHouse.BuildingBlocks.Abstractions.Integration.Events;
using InHouse.BuildingBlocks.Abstractions.Integration.Versioning;

namespace InHouse.Jobs.Application.Integration.Versioning;

public sealed class JobsJobPostedV1ToV2Upcaster : IEventUpcaster
{
    public string EventType => "Jobs.JobPosted";
    public int FromVersion => 1;
    public int ToVersion => 2;

    public IntegrationEventEnvelope Upcast(IntegrationEventEnvelope envelope)
    {
        if (envelope.EventType != EventType || envelope.EventVersion != FromVersion)
            throw new InvalidOperationException("Upcaster received unsupported envelope.");

        var node = JsonNode.Parse(envelope.PayloadJson) as JsonObject
                   ?? throw new InvalidOperationException("Invalid JSON payload.");

        // v2 requires employmentType, fill default if missing
        node["employmentType"] ??= "Unknown";

        var upgradedJson = node.ToJsonString(new JsonSerializerOptions(JsonSerializerDefaults.Web));

        return envelope with
        {
            EventVersion = ToVersion,
            PayloadJson = upgradedJson
        };
    }
}