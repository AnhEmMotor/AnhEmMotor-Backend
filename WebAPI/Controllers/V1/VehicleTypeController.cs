using Application.ApiContracts.VehicleType.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Asp.Versioning;
using WebAPI.Controllers.Base;
using Domain.Primitives;

namespace WebAPI.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class VehicleTypeController(
        IVehicleTypeRepository repository,
        ISievePaginator paginator,
        IUnitOfWork unitOfWork) : ApiController
    {
        [HttpGet]
        public async Task<ActionResult<PagedResult<VehicleTypeResponse>>> GetList([FromQuery] SieveModel sieveModel)
        {
            var query = repository.GetQueryable();
            
            var result = await paginator.ApplyAsync<VehicleType, VehicleTypeResponse>(
                query,
                sieveModel);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleTypeResponse>> GetById(int id)
        {
            var item = await repository.GetByIdAsync(id, default);
            if (item == null) return NotFound();

            return Ok(new VehicleTypeResponse
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

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] VehicleType vehicleType)
        {
            repository.Add(vehicleType);
            await unitOfWork.SaveChangesAsync(default);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] VehicleType vehicleType)
        {
            if (id != vehicleType.Id) return BadRequest();
            repository.Update(vehicleType);
            await unitOfWork.SaveChangesAsync(default);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var item = await repository.GetByIdAsync(id, default);
            if (item == null) return NotFound();
            repository.Remove(item);
            await unitOfWork.SaveChangesAsync(default);
            return Ok();
        }
    }
}
