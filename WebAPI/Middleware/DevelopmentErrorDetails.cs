namespace WebAPI.Middleware
{
    /// <summary>
    /// Mô hình chi tiết lỗi đầy đủ cho mục đích debug trong môi trường Development.
    /// </summary>
    public class DevelopmentErrorDetails
    {
        /// <summary>
        /// Thông báo của ngoại lệ.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Loại (Type) đầy đủ của ngoại lệ.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Chuỗi `ToString()` đầy đủ của ngoại lệ (thường bao gồm stack trace).
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// Chi tiết của ngoại lệ bên trong (InnerException), nếu có.
        /// </summary>
        public DevelopmentErrorDetails? InnerException { get; set; }

        /// <summary>
        /// Khởi tạo chi tiết lỗi từ một ngoại lệ.
        /// </summary>
        /// <param name="ex">Ngoại lệ để trích xuất thông tin.</param>
        public DevelopmentErrorDetails(Exception ex)
        {
            Message = ex.Message;
            Type = ex.GetType().FullName!;
            Details = ex.ToString();
            if (ex.InnerException != null)
            {
                InnerException = new DevelopmentErrorDetails(ex.InnerException);
            }
        }
    }
}