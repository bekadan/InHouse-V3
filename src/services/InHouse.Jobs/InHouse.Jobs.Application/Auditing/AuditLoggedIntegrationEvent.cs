using InHouse.BuildingBlocks.Abstractions.Messaging;

public sealed record AuditLoggedIntegrationEvent(
    Guid AuditId,
    Guid? TenantId,
    string Action,
    DateTime OccurredOnUtc
) : IIntegrationEvent
{
    public string EventType => "Platform.AuditLogged";
    public int EventVersion => 1;
}