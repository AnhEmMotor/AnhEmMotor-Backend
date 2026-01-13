namespace Application.Common.Models;

public record Error(string Code, string Message, string? Field = null, string? Id = null)
{
    public static Error Validation(string message, string? field = null, string? id = null)
    { return new Error("Validation", message, field, id); }

    public static Error NotFound(
        string message = "The requested resource was not found.",
        string? field = null,
        string? id = null)
    { return new Error("NotFound", message, field, id); }

    public static Error Unauthorized(
        string message = "You are not authorized to perform this action.",
        string? field = null,
        string? id = null)
    { return new Error("Unauthorized", message, field, id); }

    public static Error Forbidden(
        string message = "You do not have permission to access this resource.",
        string? field = null,
        string? id = null)
    { return new Error("Forbidden", message, field, id); }

    public static Error BadRequest(string message, string? field = null, string? id = null)
    { return new Error("BadRequest", message, field, id); }

    public static Error Conflict(string message, string? field = null, string? id = null)
    { return new Error("Conflict", message, field, id); }

    public static Error Failure(string message, string? field = null, string? id = null)
    { return new Error("Failure", message, field, id); }
}