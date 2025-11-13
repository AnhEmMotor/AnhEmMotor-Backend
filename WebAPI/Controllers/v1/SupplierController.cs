using Application.ApiContracts.Supplier;
using Application.Interfaces.Services.Supplier;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;

namespace WebAPI.Controllers.v1
{
    /// <summary>
    /// Quản lý danh sách nhà cung cấp.
    /// </summary>
    /// <param name="supplierInsertService"></param>
    /// <param name="supplierSelectService"></param>
    /// <param name="supplierUpdateService"></param>
    /// <param name="supplierDeleteService"></param>
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class SupplierController(
        ISupplierInsertService supplierInsertService,
        ISupplierSelectService supplierSelectService,
        ISupplierUpdateService supplierUpdateService,
        ISupplierDeleteService supplierDeleteService) : ControllerBase
    {
        /// <summary>
        /// Lấy danh sách nhà cung cấp (có phân trang, lọc, sắp xếp).
        /// </summary>
        /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetSuppliers([FromQuery] SieveModel sieveModel)
        {
            var pagedResult = await supplierSelectService.GetSuppliersAsync(sieveModel);
            return Ok(pagedResult);
        }

        /// <summary>
        /// Lấy danh sách nhà cung cấp đã bị xoá (có phân trang, lọc, sắp xếp).
        /// </summary>
        /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
        /// <returns></returns>
        [HttpGet("deleted")]
        public async Task<IActionResult> GetDeletedSuppliers([FromQuery] SieveModel sieveModel)
        {
            var pagedResult = await supplierSelectService.GetDeletedSuppliersAsync(sieveModel);
            return Ok(pagedResult);
        }

        /// <summary>
        /// Lấy thông tin của nhà cung cấp được chọn.
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần lấy thông tin.</param>
        /// <returns></returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetSupplierById(int id)
        {
            var supplierResponse = await supplierSelectService.GetSupplierByIdAsync(id);
            return supplierResponse == null ? NotFound() : Ok(supplierResponse);
        }

        /// <summary>
        /// Tạo nhà cung cấp mới.
        /// </summary>
        /// <param name="request">Thông tin nhà cung cấp cần tạo.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierRequest request)
        {
            await supplierInsertService.CreateSupplierAsync(request);
            return Ok();
        }

        /// <summary>
        /// Cập nhật thông tin nhà cung cấp (bao gồm cả trạng thái của nó).
        /// </summary>
        /// <param name="id">Id nhà cung cấp cần cập nhật.</param>
        /// <param name="request">Thông tin nhà cung cấp cần cập nhật.</param>
        /// <returns></returns>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateSupplier(int id, [FromBody] UpdateSupplierRequest request)
        {
            var success = await supplierUpdateService.UpdateSupplierAsync(id, request);
            return success ? NoContent() : NotFound();
        }

        /// <summary>
        /// Cập nhật trạng thái của nhà cung cấp.
        /// </summary>
        /// <param name="id">Id nhà cung cấp cần cập nhật trạng thái.</param>
        /// <param name="request">Trạng thái mới.</param>
        /// <returns></returns>
        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateSupplierStatus(int id, [FromBody] UpdateSupplierStatusRequest request)
        {
            var success = await supplierUpdateService.UpdateSupplierStatusAsync(id, request);
            return success ? NoContent() : NotFound();
        }

        /// <summary>
        /// Cập nhật trạng thái của nhiều nhà cung cấp cùng lúc.
        /// </summary>
        /// <param name="request">Danh sách Id nhà cung cấp và trạng thái mới.</param>
        /// <returns></returns>
        [HttpPatch("status")]
        public async Task<IActionResult> UpdateManySupplierStatus([FromBody] UpdateManySupplierStatusRequest request)
        {
            var results = await supplierUpdateService.UpdateManySupplierStatusAsync(request);
            if (results != null)
            {
                return BadRequest(results);
            }
            return NoContent();
        }

        /// <summary>
        /// Xoá nhà cung cấp.
        /// </summary>
        /// <param name="id">Id của nhà cung cấp cần xoá.</param>
        /// <returns></returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteSupplier(int id)
        {
            var success = await supplierDeleteService.DeleteSupplierAsync(id);
            return success ? NoContent() : NotFound();
        }

        /// <summary>
        /// Khôi phục lại nhà cung cấp đã xoá.
        /// </summary>
        /// <param name="id">Id của nhà cung cấp cần khôi phục</param>
        /// <returns></returns>
        [HttpPost("restore/{id:int}")]
        public async Task<IActionResult> RestoreSupplier(int id)
        {
            var success = await supplierUpdateService.RestoreSupplierAsync(id);
            return success ? NoContent() : NotFound();
        }

        /// <summary>
        /// Xoá nhiều nhà cung cấp cùng lúc.
        /// </summary>
        /// <param name="request">Danh sách Id nhà cung cấp cần xoá.</param>
        /// <returns></returns>
        [HttpPost("delete-many")]
        public async Task<IActionResult> DeleteSuppliers([FromBody] DeleteManySuppliersRequest request)
        {
            var results = await supplierDeleteService.DeleteSuppliersAsync(request);
            if (results != null)
            {
                return BadRequest(results);
            }
            return NoContent();
        }

        /// <summary>
        /// Khôi phục nhiều nhà cung cấp đã xoá cùng lúc.
        /// </summary>
        /// <param name="request">Danh sách Id nhà cung cấp cần khôi phục.</param>
        /// <returns></returns>
        [HttpPost("restore-many")]
        public async Task<IActionResult> RestoreSuppliers([FromBody] RestoreManySuppliersRequest request)
        {
            var results = await supplierUpdateService.RestoreSuppliersAsync(request);
            if (results != null)
            {
                return BadRequest(results);
            }
            return NoContent();
        }
    }
}
