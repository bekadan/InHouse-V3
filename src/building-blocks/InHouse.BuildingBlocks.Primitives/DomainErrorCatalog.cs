namespace InHouse.BuildingBlocks.Primitives;

public static class DomainErrorCatalog
{
    public static class General
    {
        public static readonly Error Unexpected =
            Error.Failure("general.unexpected", "An unexpected error occurred.");
    }

    public static class Validation
    {
        public static Error Required(string field)
            => Error.Validation("validation.required", $"{field} is required.");

        public static Error Invalid(string field)
            => Error.Validation("validation.invalid", $"{field} is invalid.");
    }

    public static class Auth
    {
        public static readonly Error Unauthorized =
            Error.Unauthorized("auth.unauthorized", "User is not authorized.");

        public static readonly Error Forbidden =
            Error.Forbidden("auth.forbidden", "Access denied.");
    }
}
