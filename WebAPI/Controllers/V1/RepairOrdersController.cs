using Application.Features.RepairOrders.Commands.AssignTechnician;
using Application.Features.RepairOrders.Commands.CompleteRepairOrder;
using Application.Features.RepairOrders.Commands.CreateRepairOrder;
using Application.Features.RepairOrders.Commands.IssueParts;
using Application.Features.RepairOrders.Queries.GetRepairOrderDetail;
using Application.Features.RepairOrders.Queries.GetRepairOrdersList;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1
{
    /// <summary>
    /// Controller quản lý phiếu sửa chữa dịch vụ (Repair Orders).
    /// </summary>
    [ApiVersion("1.0")]
    [SwaggerTag("Quản lý phiếu sửa chữa dịch vụ (Repair Orders)")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class RepairOrdersController(ISender sender) : ApiController
    {
        /// <summary>
        /// Tạo phiếu sửa chữa xe (Check-in xe).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateAsync(
            CreateRepairOrderCommand command,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy danh sách phiếu sửa chữa có phân trang và bộ lọc.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetListAsync(
            [FromQuery] SieveModel sieveModel,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetRepairOrdersListQuery { SieveModel = sieveModel }, cancellationToken)
                .ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy chi tiết phiếu sửa chữa dịch vụ theo ID.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetDetailAsync(int id, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetRepairOrderDetailQuery { Id = id }, cancellationToken)
                .ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Phân công kỹ thuật viên đảm nhận.
        /// </summary>
        [HttpPost("assign-technician")]
        public async Task<IActionResult> AssignTechnicianAsync(
            AssignTechnicianCommand command,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Xuất phụ tùng và dịch vụ sửa chữa cho xe (FIFO).
        /// </summary>
        [HttpPost("issue-parts")]
        public async Task<IActionResult> IssuePartsAsync(IssuePartsCommand command, CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Hoàn thành phiếu sửa chữa và lập hóa đơn dịch vụ.
        /// </summary>
        [HttpPost("complete")]
        public async Task<IActionResult> CompleteAsync(
            CompleteRepairOrderCommand command,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(command, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }
    }
}
