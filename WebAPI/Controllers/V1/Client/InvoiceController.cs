using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Application.ApiContracts.Client.Invoices;
using Application.Features.Client.Invoices;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AnhEmMotor.WebAPI.Controllers.V1.Client
{
    [ApiController]
    [Route("api/v1/client/invoices")]
    [Authorize]
    public class InvoiceController : ControllerBase
    {
        private readonly IMediator _mediator;
        public InvoiceController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetMyInvoices()
        {
            var result = await _mediator.Send(new GetMyInvoicesQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoiceDetail(int id)
        {
            var result = await _mediator.Send(new GetInvoiceDetailQuery(id));
            return result != null ? Ok(result) : NotFound();
        }
    }
}
