using Microsoft.AspNetCore.Mvc;
using MediatR;
using Application.ApiContracts.Client.Catalog;
using Application.Features.Client.Catalog;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AnhEmMotor.WebAPI.Controllers.V1.Client
{
    [ApiController]
    [Route("api/v1/client/catalog")]
    public class CatalogController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CatalogController(IMediator mediator) => _mediator = mediator;

        [HttpGet("products")]
        public async Task<IActionResult> GetProducts([FromQuery] string search, [FromQuery] int? categoryId)
        {
            var result = await _mediator.Send(new GetProductsQuery(search, categoryId));
            return Ok(result);
        }

        [HttpGet("products/{id}")]
        public async Task<IActionResult> GetProductDetail(int id)
        {
            var result = await _mediator.Send(new GetProductDetailQuery(id));
            return result != null ? Ok(result) : NotFound();
        }

        [HttpPost("request-consultation")]
        public async Task<IActionResult> RequestConsultation([FromBody] ConsultationRequest request)
        {
            var result = await _mediator.Send(new RequestConsultationCommand(request));
            return result ? Ok() : BadRequest();
        }
    }
}
