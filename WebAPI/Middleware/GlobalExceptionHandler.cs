using Microsoft.AspNetCore.Diagnostics;
using System.Net;

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
            await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken).ConfigureAwait(true);
            return true;
        }
    }
}