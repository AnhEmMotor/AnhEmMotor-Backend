namespace WebAPI.Middleware
{
    /// <summary>
    /// Cấu trúc phản hồi lỗi chi tiết cho môi trường Development.
    /// </summary>
    /// <param name="exception">Ngoại lệ gốc được sử dụng để xây dựng phản hồi chi tiết.</param>
    public class DevelopmentErrorResponse(Exception exception)
    {
        /// <summary>
        /// Chi tiết lỗi đầy đủ, bao gồm cả stack trace và inner exception.
        /// </summary>
        public DevelopmentErrorDetails Errors { get; set; } = new DevelopmentErrorDetails(exception);
    }
}