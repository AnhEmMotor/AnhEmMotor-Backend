namespace WebAPI.Middleware
{
    /// <summary>
    /// Mô hình chi tiết lỗi cơ bản, chỉ chứa thông báo.
    /// </summary>
    public class ErrorDetails
    {
        /// <summary>
        /// Thông báo lỗi.
        /// </summary>
        public string? Message { get; set; }
    }
}