using Application.ApiContracts.Supplier;
using Application.Interfaces.Services.Supplier;
using Asp.Versioning;
using Domain.Helpers;
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<SupplierResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSuppliers([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
        {
            var pagedResult = await supplierSelectService.GetSuppliersAsync(sieveModel, cancellationToken).ConfigureAwait(true);
            return Ok(pagedResult);
        }

        /// <summary>
        /// Lấy danh sách nhà cung cấp đã bị xoá (có phân trang, lọc, sắp xếp).
        /// </summary>
        /// <param name="sieveModel">Các thông tin phân trang, lọc, sắp xếp theo quy tắc của Sieve.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("deleted")]
        [ProducesResponseType(typeof(List<SupplierResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDeletedSuppliers([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
        {
            var pagedResult = await supplierSelectService.GetDeletedSuppliersAsync(sieveModel, cancellationToken).ConfigureAwait(true);
            return Ok(pagedResult);
        }

        /// <summary>
        /// Lấy thông tin của nhà cung cấp được chọn.
        /// </summary>
        /// <param name="id">Mã nhà cung cấp cần lấy thông tin.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSupplierById(int id, CancellationToken cancellationToken)
        {
            var (data, error) = await supplierSelectService.GetSupplierByIdAsync(id, cancellationToken).ConfigureAwait(true);
            if (error != null)
            {
                return NotFound(error);
            }
            return Ok(data);
        }

        /// <summary>
        /// Tạo nhà cung cấp mới.
        /// </summary>
        /// <param name="request">Thông tin nhà cung cấp cần tạo.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(SupplierResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierRequest request, CancellationToken cancellationToken)
        {
            await supplierInsertService.CreateSupplierAsync(request, cancellationToken).ConfigureAwait(true);
            return Ok();
        }

        /// <summary>
        /// Cập nhật thông tin nhà cung cấp (bao gồm cả trạng thái của nó).
        /// </summary>
        /// <param name="id">Id nhà cung cấp cần cập nhật.</param>
        /// <param name="request">Thông tin nhà cung cấp cần cập nhật.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSupplier(int id, [FromBody] UpdateSupplierRequest request, CancellationToken cancellationToken)
        {
            var error = await supplierUpdateService.UpdateSupplierAsync(id, request, cancellationToken).ConfigureAwait(true);
            if (error != null)
            {
                return NotFound(error);
            }
            return Ok();
        }

        /// <summary>
        /// Cập nhật trạng thái của nhà cung cấp.
        /// </summary>
        /// <param name="id">Id nhà cung cấp cần cập nhật trạng thái.</param>
        /// <param name="request">Trạng thái mới.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPatch("{id:int}/status")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateSupplierStatus(int id, [FromBody] UpdateSupplierStatusRequest request, CancellationToken cancellationToken)
        {
            var error = await supplierUpdateService.UpdateSupplierStatusAsync(id, request, cancellationToken).ConfigureAwait(true);
            if (error != null)
            {
                return NotFound(error);
            }
            return Ok();
        }

        /// <summary>
        /// Cập nhật trạng thái của nhiều nhà cung cấp cùng lúc.
        /// </summary>
        /// <param name="request">Danh sách Id nhà cung cấp và trạng thái mới.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPatch("status")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateManySupplierStatus([FromBody] UpdateManySupplierStatusRequest request, CancellationToken cancellationToken)
        {
            var results = await supplierUpdateService.UpdateManySupplierStatusAsync(request, cancellationToken).ConfigureAwait(true);
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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteSupplier(int id, CancellationToken cancellationToken)
        {
            var error = await supplierDeleteService.DeleteSupplierAsync(id, cancellationToken).ConfigureAwait(true);
            if (error != null)
            {
                return NotFound(error);
            }
            return Ok();
        }

        /// <summary>
        /// Khôi phục lại nhà cung cấp đã xoá.
        /// </summary>
        /// <param name="id">Id của nhà cung cấp cần khôi phục</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("restore/{id:int}")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RestoreSupplier(int id, CancellationToken cancellationToken)
        {
            var error = await supplierUpdateService.RestoreSupplierAsync(id, cancellationToken).ConfigureAwait(true);
            if (error != null)
            {
                return NotFound(error);
            }
            return Ok();
        }

        /// <summary>
        /// Xoá nhiều nhà cung cấp cùng lúc.
        /// </summary>
        /// <param name="request">Danh sách Id nhà cung cấp cần xoá.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("delete-many")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteSuppliers([FromBody] DeleteManySuppliersRequest request, CancellationToken cancellationToken)
        {
            var results = await supplierDeleteService.DeleteSuppliersAsync(request, cancellationToken).ConfigureAwait(true);
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
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("restore-many")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RestoreSuppliers([FromBody] RestoreManySuppliersRequest request, CancellationToken cancellationToken)
        {
            var results = await supplierUpdateService.RestoreSuppliersAsync(request, cancellationToken).ConfigureAwait(true);
            if (results != null)
            {
                return BadRequest(results);
            }
            return NoContent();
        }
    }
}
