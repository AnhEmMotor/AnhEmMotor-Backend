using FluentValidation.Results;

namespace WebAPI.Contracts.Errors;

/// <summary>
/// Represents a standardized error response returned by an API, including error message, type, details, validation
/// errors, and optional inner exception information.
/// </summary>
/// <remarks>
/// Use this class to provide structured error information in API responses, enabling clients to handle errors
/// consistently. The class supports representing general errors, validation errors, and nested exceptions for detailed
/// diagnostics. Validation errors are included when input validation fails, and inner exceptions can be used to convey
/// exception chains for debugging purposes.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the ApiErrorResponse class with the specified error message.
/// </remarks>
/// <param name="message">The error message that describes the API error. Cannot be null.</param>
public class ApiErrorResponse(string message)
{
    /// <summary>
    /// Gets or sets the message associated with this instance.
    /// </summary>
    public string Message { get; set; } = message;

    /// <summary>
    /// Gets or sets the type identifier associated with the object.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the detailed description or additional information associated with the object.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Gets or sets the collection of validation errors associated with the current operation.
    /// </summary>
    /// <remarks>
    /// The list contains details for each validation error encountered. If no errors are present, the property may be
    /// null or an empty list.
    /// </remarks>
    public List<ValidationErrorDetail>? Errors { get; set; }

    /// <summary>
    /// Gets or sets the underlying API error response that caused the current error, if any.
    /// </summary>
    /// <remarks>
    /// Use this property to access additional details about the original error when handling nested or chained API
    /// exceptions. This can be useful for debugging or for providing more informative error messages to users.
    /// </remarks>
    public ApiErrorResponse? InnerException { get; set; }

    /// <summary>
    /// Creates a new API error response representing a production error with the specified message.
    /// </summary>
    /// <param name="message">The error message to include in the response. Cannot be null.</param>
    /// <returns>An <see cref="ApiErrorResponse"/> containing the provided error message.</returns>
    public static ApiErrorResponse CreateProductionError(string message) { return new ApiErrorResponse(message); }

    /// <summary>
    /// Creates an <see cref="ApiErrorResponse"/> containing detailed information about the specified exception for
    /// development and debugging purposes.
    /// </summary>
    /// <remarks>
    /// This method is intended for use in development environments where exposing detailed exception information is
    /// useful for debugging. It recursively includes inner exception details, which may reveal sensitive information.
    /// Avoid using this method in production scenarios.
    /// </remarks>
    /// <param name="ex">The exception to include in the error response. Cannot be null.</param>
    /// <returns>
    /// An <see cref="ApiErrorResponse"/> populated with the exception's message, type, stack trace, and any inner
    /// exceptions.
    /// </returns>
    public static ApiErrorResponse CreateDevelopmentError(Exception ex)
    {
        var response = new ApiErrorResponse(ex.Message) { Type = ex.GetType().FullName, Details = ex.ToString() };

        if(ex.InnerException != null)
        {
            response.InnerException = CreateDevelopmentError(ex.InnerException);
        }

        return response;
    }

    /// <summary>
    /// Creates an API error response representing one or more validation failures.
    /// </summary>
    /// <param name="failures">
    /// A collection of validation failures to include in the error response. Each failure should specify the property
    /// name and the associated error message.
    /// </param>
    /// <returns>An <see cref="ApiErrorResponse"/> containing a summary message and details for each validation failure provided.</returns>
    public static ApiErrorResponse CreateValidationError(IEnumerable<ValidationFailure> failures)
    {
        return new ApiErrorResponse("One or more validation errors occurred.")
        {
            Errors =
                [ .. failures.Select(f => new ValidationErrorDetail { Field = f.PropertyName, Error = f.ErrorMessage }) ]
        };
    }
}
