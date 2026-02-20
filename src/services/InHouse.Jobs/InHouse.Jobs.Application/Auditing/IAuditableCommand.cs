namespace InHouse.Jobs.Application.Auditing;

public interface IAuditableCommand
{
    string Action { get; }
    string Resource { get; }
    string? ResourceId { get; }
}