namespace WebAPI.Middleware
{
    /// <summary>
    /// Cấu trúc phản hồi lỗi đơn giản cho môi trường Production.
    /// </summary>
    public class ProductionErrorResponse
    {
        /// <summary>
        /// Chi tiết lỗi được lồng trong đối tượng 'errors'.
        /// </summary>
        public ErrorDetails Errors { get; set; }
        /// <summary>
        /// Khởi tạo một phản hồi lỗi Production mới.
        /// </summary>
        /// <param name="message">Thông báo lỗi chung, an toàn để hiển thị cho người dùng.</param>
        public ProductionErrorResponse(string message)
        {
            Errors = new ErrorDetails { Message = message };
        }
    }
}