namespace Application.Common.Models;

public record Error(string Code, string Message, string? Field = null)
{
    public static Error Validation(string message, string? field = null)
    {
        return new Error("Validation", message, field);
    }

    public static Error NotFound(string message = "The requested resource was not found.", string? field = null)
    {
        return new Error("NotFound", message, field);
    }

    public static Error Unauthorized(string message = "You are not authorized to perform this action.", string? field = null)
    {
        return new Error("Unauthorized", message, field);
    }

    public static Error Forbidden(string message = "You do not have permission to access this resource.", string? field = null)
    {
        return new Error("Forbidden", message, field);
    }

    public static Error BadRequest(string message, string? field = null)
    {
        return new Error("BadRequest", message, field);
    }

    public static Error Conflict(string message, string? field = null)
    {
        return new Error("Conflict", message, field);
    }

    public static Error Failure(string message, string? field = null)
    {
        return new Error("Failure", message, field);
    }
}
