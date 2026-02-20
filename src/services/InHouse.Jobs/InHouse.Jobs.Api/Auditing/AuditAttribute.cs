namespace InHouse.Jobs.Api.Auditing;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class AuditAttribute : Attribute
{
    public string Action { get; }
    public string Resource { get; }

    public AuditAttribute(string action, string resource)
    {
        Action = action;
        Resource = resource;
    }
}