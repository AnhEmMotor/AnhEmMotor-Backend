using System.Net;
using Microsoft.AspNetCore.Diagnostics;

namespace WebAPI.Middleware
{
    /// <summary>
    /// Xử lý các ngoại lệ chưa được xử lý một cách toàn cục cho ứng dụng.
    /// Định dạng phản hồi lỗi dựa trên môi trường (Development hoặc Production).
    /// </summary>
    /// <param name="environment">Đối tượng môi trường web, dùng để kiểm tra môi trường (ví dụ: Development).</param>
    /// <param name="logger">Đối tượng logger để ghi lại thông tin ngoại lệ.</param>
    public class GlobalExceptionHandler(IWebHostEnvironment environment, ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        /// <summary>
        /// Cố gắng xử lý ngoại lệ được cung cấp, ghi log và định dạng phản hồi JSON.
        /// </summary>
        /// <param name="httpContext">Bối cảnh HTTP hiện tại, được sử dụng để thiết lập phản hồi.</param>
        /// <param name="exception">Ngoại lệ chưa được xử lý đã xảy ra.</param>
        /// <param name="cancellationToken">Token để hủy bỏ thao tác.</param>
        /// <returns>Trả về giá trị cho biết liệu ngoại lệ đã được xử lý (luôn trả về `true` trong trường hợp này).</returns>
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            object errorResponse;
            if (environment.IsDevelopment())
            {
                errorResponse = new DevelopmentErrorResponse(exception);
            }
            else
            {
                errorResponse = new ProductionErrorResponse("Something went wrong in system. Please try again.");
            }
            await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);
            return true;
        }
    }

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