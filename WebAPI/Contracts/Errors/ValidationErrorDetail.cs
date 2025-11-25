namespace WebAPI.Contracts.Errors
{
    /// <summary>
    /// Represents details about a validation error for a specific field.
    /// </summary>
    /// <remarks>
    /// Use this class to convey information about which field failed validation and the associated error message. This
    /// is typically used in validation result collections to provide structured error feedback.
    /// </remarks>
    public class ValidationErrorDetail
    {
        /// <summary>
        /// Gets or sets the value of the field.
        /// </summary>
        public string Field { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error message associated with the current operation.
        /// </summary>
        public string Error { get; set; } = string.Empty;
    }
}