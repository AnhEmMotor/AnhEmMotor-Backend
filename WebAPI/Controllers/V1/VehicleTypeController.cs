using Application.ApiContracts.VehicleType.Responses;
using Application.Interfaces.Repositories;
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
    /// <param name="repository">The vehicle type repository.</param>
    /// <param name="paginator">The sieve paginator.</param>
    /// <param name="unitOfWork">The unit of work.</param>
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class VehicleTypeController(
        IVehicleTypeRepository repository,
        ISievePaginator paginator,
        IUnitOfWork unitOfWork) : ApiController
    {
        /// <summary>
        /// Gets a paginated list of vehicle types.
        /// </summary>
        /// <param name="sieveModel">The sieve model for filtering and pagination.</param>
        /// <returns>A paginated list of vehicle types.</returns>
        [HttpGet]
        public async Task<ActionResult<PagedResult<VehicleTypeResponse>>> GetList([FromQuery] SieveModel sieveModel)
        {
            var query = repository.GetQueryable();
            var result = await paginator.ApplyAsync<VehicleType, VehicleTypeResponse>(query, sieveModel);
            return Ok(result);
        }

        /// <summary>
        /// Gets a vehicle type by its ID.
        /// </summary>
        /// <param name="id">The ID of the vehicle type.</param>
        /// <returns>The vehicle type details.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleTypeResponse>> GetById(int id)
        {
            var item = await repository.GetByIdAsync(id, default);
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
        /// <returns>A status result.</returns>
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] VehicleType vehicleType)
        {
            repository.Add(vehicleType);
            await unitOfWork.SaveChangesAsync(default);
            return Ok();
        }

        /// <summary>
        /// Updates an existing vehicle type.
        /// </summary>
        /// <param name="id">The ID of the vehicle type to update.</param>
        /// <param name="vehicleType">The updated vehicle type data.</param>
        /// <returns>A status result.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] VehicleType vehicleType)
        {
            if (id != vehicleType.Id)
                return BadRequest();
            repository.Update(vehicleType);
            await unitOfWork.SaveChangesAsync(default);
            return Ok();
        }

        /// <summary>
        /// Deletes a vehicle type by its ID.
        /// </summary>
        /// <param name="id">The ID of the vehicle type to delete.</param>
        /// <returns>A status result.</returns>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var item = await repository.GetByIdAsync(id, default);
            if (item == null)
                return NotFound();
            repository.Remove(item);
            await unitOfWork.SaveChangesAsync(default);
            return Ok();
        }
    }
}
