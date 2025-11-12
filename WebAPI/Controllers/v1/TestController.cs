using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.v1
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TestController(ILogger<TestController> logger) : ControllerBase
    {
        private readonly ILogger<TestController> _logger = logger;
        [HttpGet]
        public ActionResult<string> Get()
        {
            _logger.LogInformation("Endpoint Test/Get đã được gọi.");
            return Ok("API Test Controller đang hoạt động!");
        }

        [HttpPost]
        public ActionResult<string> Post([FromBody] string inputData)
        {
            if (string.IsNullOrEmpty(inputData))
            {
                return BadRequest("Vui lòng gửi dữ liệu đầu vào.");
            }
            _logger.LogInformation("Endpoint Test/Post đã được gọi với dữ liệu: {Data}", inputData);
            return Ok($"Đã nhận dữ liệu: {inputData}. Phản hồi thành công.");
        }
    }
}