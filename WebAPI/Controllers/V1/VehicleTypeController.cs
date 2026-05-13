using Application.ApiContracts.Vehicle.Responses;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.VehicleType.VehicleType;
using Asp.Versioning;
using Domain.Entities;
using Domain.Primitives;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1
{
    /// <summary>
    /// Quản lý các loại xe.
    /// </summary>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class VehicleTypeController(
        IVehicleTypeReadRepository readRepository,
        IVehicleTypeInsertRepository insertRepository,
        IVehicleTypeUpdateRepository updateRepository,
        IVehicleTypeDeleteRepository deleteRepository,
        IUnitOfWork unitOfWork) : ApiController
    {
        /// <summary>
        /// Lấy danh sách các loại xe (có phân trang, lọc).
        /// </summary>
        /// <param name="sieveModel">Model lọc và phân trang.</param>
        /// <param name="cancellationToken">Token hủy bỏ.</param>
        /// <returns>Danh sách các loại xe.</returns>
        [HttpGet]
        public async Task<ActionResult<PagedResult<VehicleTypeResponse>>> GetListAsync(
            [FromQuery] SieveModel sieveModel,
            CancellationToken cancellationToken)
        {
            var result = await readRepository.GetPagedAsync<VehicleTypeResponse>(
                sieveModel,
                cancellationToken: cancellationToken)
                .ConfigureAwait(true);
            return Ok(result);
        }

        /// <summary>
        /// Lấy thông tin loại xe theo ID.
        /// </summary>
        /// <param name="id">ID của loại xe.</param>
        /// <param name="cancellationToken">Token hủy bỏ.</param>
        /// <returns>Thông tin chi tiết loại xe.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleTypeResponse>> GetByIdAsync(int id, CancellationToken cancellationToken)
        {
            var item = await readRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(true);
            if (item == null)
                return NotFound();
            return Ok(
                new VehicleTypeResponse
                {
                    Id = item.Id,
                    Name = item.Name,
                    Slug = item.Slug,
                    ImageUrl = item.ImageUrl,
                    IsActive = item.IsActive,
                    SortOrder = item.SortOrder,
                    Description = item.Description
                });
        }

        /// <summary>
        /// Tạo mới một loại xe.
        /// </summary>
        /// <param name="vehicleType">Dữ liệu loại xe mới.</param>
        /// <param name="cancellationToken">Token hủy bỏ.</param>
        /// <returns>Kết quả thực hiện.</returns>
        [HttpPost]
        public async Task<ActionResult> CreateAsync(
            [FromBody] VehicleType vehicleType,
            CancellationToken cancellationToken)
        {
            insertRepository.Add(vehicleType);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(true);
            return Ok();
        }

        /// <summary>
        /// Cập nhật thông tin loại xe.
        /// </summary>
        /// <param name="id">ID của loại xe cần cập nhật.</param>
        /// <param name="vehicleType">Dữ liệu cập nhật.</param>
        /// <param name="cancellationToken">Token hủy bỏ.</param>
        /// <returns>Kết quả thực hiện.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAsync(
            int id,
            [FromBody] VehicleType vehicleType,
            CancellationToken cancellationToken)
        {
            if (id != vehicleType.Id)
                return BadRequest();
            updateRepository.Update(vehicleType);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(true);
            return Ok();
        }

        /// <summary>
        /// Xóa một loại xe theo ID.
        /// </summary>
        /// <param name="id">ID của loại xe cần xóa.</param>
        /// <param name="cancellationToken">Token hủy bỏ.</param>
        /// <returns>Kết quả thực hiện.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var item = await readRepository.GetByIdAsync(id, cancellationToken).ConfigureAwait(true);
            if (item == null)
                return NotFound();
            deleteRepository.Remove(item);
            await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(true);
            return Ok();
        }
    }
}
