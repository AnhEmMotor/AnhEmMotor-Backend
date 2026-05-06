using Application.ApiContracts.Maintenance.Responses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

[Route("api/v{version:apiVersion}/[controller]")]
public class VehiclesController(ISender sender) : ApiController
{
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        // Placeholder for testing
        return Ok(new VehicleResponse { Id = id });
    }
}
