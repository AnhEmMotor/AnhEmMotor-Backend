using Asp.Versioning;
using Application.ApiContracts.PurchaseInvoice.Requests;
using Application.ApiContracts.PurchaseInvoice.Responses;
using Application.Common.Models;
using Application.Features.PurchaseInvoices.Commands.CreatePurchaseInvoice;
using Application.Features.PurchaseInvoices.Queries.GetPurchaseInvoiceById;
using Application.Features.PurchaseInvoices.Queries.GetPurchaseInvoices;
using Domain.Constants.Permission.Permissions;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1
{
    [ApiVersion("1.0")]
    [SwaggerTag("Quan ly Hoa don mua hang (Purchase Invoice)")]
    [Route("api/v{version:apiVersion}/purchaseinvoices")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public class PurchaseInvoicesController(IMediator mediator) : ApiController
    {
        [HttpPost]
        [HasPermission(PurchaseInvoices.Create)]
        [ProducesResponseType(typeof(PurchaseInvoiceDetailResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAsync(
            [FromBody] CreatePurchaseInvoiceRequest request,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new CreatePurchaseInvoiceCommand
                {
                    PurchaseRequestId = request.PurchaseRequestId,
                    InvoiceNumber = request.InvoiceNumber,
                    InvoiceDate = request.InvoiceDate,
                    DueDate = request.DueDate,
                    SupplierId = request.SupplierId,
                    SupplierName = request.SupplierName,
                    SupplierPhone = request.SupplierPhone,
                    SupplierAddress = request.SupplierAddress,
                    SupplierTaxCode = request.SupplierTaxCode,
                    CustomerName = request.CustomerName,
                    CustomerPhone = request.CustomerPhone,
                    CustomerAddress = request.CustomerAddress,
                    CustomerIdCard = request.CustomerIdCard,
                    PaymentMethod = request.PaymentMethod,
                    Notes = request.Notes,
                    Items = request.Items,
                },
                cancellationToken)
                .ConfigureAwait(false);
            return HandleCreated(result);
        }

        [HttpGet]
        [HasPermission(PurchaseInvoices.View)]
        [ProducesResponseType(typeof(PagedResult<PurchaseInvoiceListResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] SieveModel sieveModel,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new GetPurchaseInvoicesQuery { SieveModel = sieveModel },
                cancellationToken)
                .ConfigureAwait(false);
            return HandleResult(result);
        }

        [HttpGet("{id:int}")]
        [HasPermission(PurchaseInvoices.View)]
        [ProducesResponseType(typeof(PurchaseInvoiceDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAsync(
            int id,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new GetPurchaseInvoiceByIdQuery(id),
                cancellationToken)
                .ConfigureAwait(false);
            return HandleResult(result);
        }
    }
}
