using Application.ApiContracts.Brand;
using Application.Interfaces.Services.Brand;
using Asp.Versioning;
using Domain.Helpers;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace WebAPI.Controllers.v1
{
    /// <summary>
    /// Quản lý danh sách các thương hiệu sản phẩm (ví dụ: Honda, Yamaha, Suzuki).
    /// </summary>
    /// <param name="brandInsertService"></param>
    /// <param name="brandSelectService"></param>
    /// <param name="brandUpdateService"></param>
    /// <param name="brandDeleteService"></param>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public class BrandController(
        IBrandInsertService brandInsertService,
        IBrandSelectService brandSelectService,
        IBrandUpdateService brandUpdateService,
        IBrandDeleteService brandDeleteService) : ControllerBase
    {
        /// <summary>
        /// Lấy danh sách thương hiệu (có phân trang, lọc, sắp xếp).
        /// </summary>
        /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<BrandResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBrands([FromQuery] SieveModel sieveModel)
        {
            var pagedResult = await brandSelectService.GetBrandsAsync(sieveModel);
            return Ok(pagedResult);
        }

        /// <summary>
        /// Lấy danh sách thương hiệu đã bị xoá (có phân trang, lọc, sắp xếp).
        /// </summary>
        /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
        /// <returns></returns>
        [HttpGet("deleted")]
        [ProducesResponseType(typeof(List<BrandResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDeletedBrands([FromQuery] SieveModel sieveModel)
        {
            var pagedResult = await brandSelectService.GetDeletedBrandsAsync(sieveModel);
            return Ok(pagedResult);
        }

        /// <summary>
        /// Lấy thông tin của thương hiệu được chọn.
        /// </summary>
        /// <param name="id">Mã thương hiệu cần lấy thông tin.</param>
        /// <returns></returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBrandById(int id)
        {
            var (data, error) = await brandSelectService.GetBrandByIdAsync(id);
            if (error != null)
            {
                return NotFound(error);
            }
            return Ok(data);
        }

        /// <summary>
        /// Tạo thương hiệu mới.
        /// </summary>
        /// <param name="request">Truyền tên và mô tả cho thương hiệu đó. Cả 2 đều là 1 chuỗi.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateBrand([FromBody] CreateBrandRequest request)
        {
            await brandInsertService.CreateBrandAsync(request);
            return Ok();
        }

        /// <summary>
        /// Cập nhật thông tin thương hiệu.
        /// </summary>
        /// <param name="id">Id thương hiệu cần cập nhật.</param>
        /// <param name="request">Tên thương hiệu và mô tả cho thương hiệu đó, tất cả đều là 1 chuỗi.</param>
        /// <returns></returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateBrand(int id, [FromBody] UpdateBrandRequest request)
        {
            var error = await brandUpdateService.UpdateBrandAsync(id, request);
            if (error != null)
            {
                return BadRequest(error);
            }
            return Ok();
        }

        /// <summary>
        /// Xoá thương hiệu.
        /// </summary>
        /// <param name="id">Id của thương hiệu cần xoá.</param>
        /// <returns></returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var error = await brandDeleteService.DeleteBrandAsync(id);
            if (error != null)
            {
                return NotFound(error);
            }
            return Ok();
        }

        /// <summary>
        /// Khôi phục lại thương hiệu đã xoá.
        /// </summary>
        /// <param name="id">Id của thương hiệu cần khôi phục</param>
        /// <returns></returns>
        [HttpPost("restore/{id:int}")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RestoreBrand(int id)
        {
            var error = await brandUpdateService.RestoreBrandAsync(id);
            if (error != null)
            {
                return NotFound(error);
            }
            return Ok();
        }

        /// <summary>
        /// Xoá nhiều thương hiệu cùng lúc.
        /// </summary>
        /// <param name="request">Danh sách Id thương hiệu cần xoá.</param>
        /// <returns></returns>
        [HttpPost("delete-many")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteBrands([FromBody] DeleteManyBrandsRequest request)
        {
            var results = await brandDeleteService.DeleteBrandsAsync(request);
            if (results != null)
            {
                return BadRequest(results);
            }
            return NoContent();
        }

        /// <summary>
        /// Khôi phục nhiều thương hiệu đã xoá cùng lúc.
        /// </summary>
        /// <param name="request">Danh sách Id thương hiệu cần khôi phục.</param>
        /// <returns></returns>
        [HttpPost("restore-many")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RestoreBrands([FromBody] RestoreManyBrandsRequest request)
        {
            var results = await brandUpdateService.RestoreBrandsAsync(request);
            if (results != null)
            {
                return BadRequest(results);
            }
            return NoContent();
        }
    }
}
