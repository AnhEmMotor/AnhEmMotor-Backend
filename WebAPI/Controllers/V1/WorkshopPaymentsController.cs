using Application.Features.WorkshopPayments.Commands.CreateWorkshopPayment;
using Application.Features.WorkshopPayments.Queries.GetWorkshopPaymentDetail;
using Application.Features.WorkshopPayments.Queries.GetWorkshopPaymentsList;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1
{
    [ApiVersion("1.0")]
    [SwaggerTag("Quầy thanh toán - Quản lý phiếu thu dịch vụ xưởng")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class WorkshopPaymentsController(ISender sender) : ApiController
    {
        [HttpGet]
        [SwaggerOperation(Summary = "Danh sách phiếu thanh toán xưởng")]
        public async Task<IActionResult> GetListAsync([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetWorkshopPaymentsListQuery { SieveModel = sieveModel }, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Chi tiết phiếu thanh toán")]
        public async Task<IActionResult> GetDetailAsync([FromRoute] int id, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetWorkshopPaymentDetailQuery { Id = id }, cancellationToken);
            return HandleResult(result);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Tạo phiếu thanh toán mới (thu tiền tại quầy)")]
        public async Task<IActionResult> CreateAsync(CreateWorkshopPaymentCommand command, CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken);
            return HandleResult(result);
        }

        [HttpGet("stats")]
        [SwaggerOperation(Summary = "Thống kê thu ngân xưởng")]
        public async Task<IActionResult> GetStatsAsync(CancellationToken cancellationToken)
        {
            var result = await sender.Send(new Application.Features.WorkshopPayments.Queries.GetWorkshopPaymentStats.GetWorkshopPaymentStatsQuery(), cancellationToken);
            return HandleResult(result);
        }
    }
}
