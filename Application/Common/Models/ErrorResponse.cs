using FluentValidation.Results;

namespace Application.Common.Models
{
    public class ErrorResponse(string? message = null)
    {
        public string? Message { get; set; } = message;
        public string? Type { get; set; }
        public string? Details { get; set; }
        public List<ErrorDetail>? Errors { get; set; }
        public ErrorResponse? InnerException { get; set; }

        public static ErrorResponse CreateProductionError(string message)
        {
            return new ErrorResponse(message);
        }

        public static ErrorResponse CreateDevelopmentError(Exception ex)
        {
            var response = new ErrorResponse(ex.Message)
            {
                Type = ex.GetType().FullName,
                Details = ex.ToString()
            };

            if (ex.InnerException != null)
            {
                response.InnerException = CreateDevelopmentError(ex.InnerException);
            }

            return response;
        }

        public static ErrorResponse CreateValidationError(IEnumerable<ValidationFailure> failures)
        {
            return new ErrorResponse("One or more validation errors occurred.")
            {
                Errors =
                    [.. failures.Select(f => new ErrorDetail { Field = f.PropertyName, Message = f.ErrorMessage })]
            };
        }

        public static ErrorResponse FromError(Error error)
        {
            return new ErrorResponse(error.Message)
            {
                Type = error.Code,
                Errors = error.Field is not null
                    ? [new ErrorDetail { Field = error.Field, Message = error.Message }]
                    : null
            };
        }

        public static ErrorResponse FromErrors(List<Error> errors)
        {
            var firstError = errors.FirstOrDefault();
            return new ErrorResponse(firstError?.Message ?? "One or more errors occurred.")
            {
                Type = firstError?.Code,
                Errors = [.. errors.Select(e => new ErrorDetail { Field = e.Field, Message = e.Message })]
            };
        }
    }
}
