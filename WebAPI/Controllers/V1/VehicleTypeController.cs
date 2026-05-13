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
    /// Controller for managing vehicle types.
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
        /// Gets a paginated list of vehicle types.
        /// </summary>
        /// <param name="sieveModel">The sieve model for filtering and pagination.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A paginated list of vehicle types.</returns>
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
        /// Gets a vehicle type by its ID.
        /// </summary>
        /// <param name="id">The ID of the vehicle type.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The vehicle type details.</returns>
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
        /// Creates a new vehicle type.
        /// </summary>
        /// <param name="vehicleType">The vehicle type to create.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A status result.</returns>
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
        /// Updates an existing vehicle type.
        /// </summary>
        /// <param name="id">The ID of the vehicle type to update.</param>
        /// <param name="vehicleType">The updated vehicle type data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A status result.</returns>
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
        /// Deletes a vehicle type by its ID.
        /// </summary>
        /// <param name="id">The ID of the vehicle type to delete.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A status result.</returns>
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
