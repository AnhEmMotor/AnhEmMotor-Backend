using System.Text.Json.Serialization;
using FluentValidation.Results;

namespace Application.Common.Models
{
    public class ErrorResponse(string? message = null)
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Message { get; set; } = message;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Type { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Details { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ErrorDetail>? Errors { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ErrorResponse? InnerException { get; set; }

        public static ErrorResponse CreateProductionError(string message) { return new ErrorResponse(message); }

        public static ErrorResponse CreateDevelopmentError(Exception ex)
        {
            var response = new ErrorResponse(ex.Message) { Type = ex.GetType().FullName, Details = ex.ToString() };

            if(ex.InnerException != null)
            {
                response.InnerException = CreateDevelopmentError(ex.InnerException);
            }

            return response;
        }

        public static ErrorResponse CreateValidationError(IEnumerable<ValidationFailure> failures)
        {
            return new ErrorResponse(null)
            {
                Errors =
                    [ .. failures.Select(f => new ErrorDetail { Field = f.PropertyName, Message = f.ErrorMessage }) ]
            };
        }

        public static ErrorResponse FromError(Error error)
        {
            var hasErrors = error.Field is not null || error.Id is not null;
            return new ErrorResponse(hasErrors ? null : error.Message)
            {
                Type = error.Code,
                Errors =
                    hasErrors
                        ? [ new ErrorDetail { Field = error.Field, Message = error.Message, Id = error.Id } ]
                        : null
            };
        }

        public static ErrorResponse FromErrors(List<Error> errors)
        {
            var firstError = errors.FirstOrDefault();
            return new ErrorResponse(null)
            {
                Type = firstError?.Code,
                Errors = [ .. errors.Select(e => new ErrorDetail { Field = e.Field, Message = e.Message, Id = e.Id }) ]
            };
        }
    }
}

