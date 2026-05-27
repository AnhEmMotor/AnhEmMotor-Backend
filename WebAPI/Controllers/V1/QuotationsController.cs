using Application.ApiContracts.Quotation.Responses;
using Application.Common.Models;
using Application.Features.Quotations.Commands.CreateQuotation;
using Application.Features.Quotations.Commands.DeleteQuotation;
using Application.Features.Quotations.Commands.UpdateQuotation;
using Microsoft.AspNetCore.Authorization;
using Application.Features.Quotations.Commands.SendQuotation;
using Application.Features.Quotations.Commands.ApproveQuotation;
using Application.Features.Quotations.Commands.RejectQuotation;
using Application.Features.Quotations.Queries.GetQuotationById;
using Application.Features.Quotations.Queries.GetQuotationsList;
using Asp.Versioning;
using Domain.Constants.Permission.Permissions;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1
{
    /// <summary>
    /// Quản lý danh sách các yêu cầu báo giá (Quotations).
    /// </summary>
    [ApiVersion("1.0")]
    [SwaggerTag("Quản lý danh sách các yêu cầu báo giá (Quotations)")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public class QuotationsController(IMediator mediator) : ApiController
    {
        /// <summary>
        /// Lấy danh sách yêu cầu báo giá (có phân trang, lọc, sắp xếp).
        /// </summary>
        /// <param name="sieveModel">Thông tin phân trang, lọc, sắp xếp của Sieve.</param>
        /// <param name="cancellationToken">Token hủy bỏ.</param>
        /// <returns>Danh sách yêu cầu báo giá.</returns>
        [HttpGet]
        [HasPermission(Quotations.View)]
        [ProducesResponseType(typeof(PagedResult<QuotationSummaryResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetQuotationsAsync(
            [FromQuery] SieveModel sieveModel,
            CancellationToken cancellationToken)
        {
            var query = new GetQuotationsListQuery { SieveModel = sieveModel };
            var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Lấy thông tin chi tiết yêu cầu báo giá theo ID.
        /// </summary>
        /// <param name="id">ID yêu cầu báo giá.</param>
        /// <param name="cancellationToken">Token hủy bỏ.</param>
        /// <returns>Chi tiết yêu cầu báo giá.</returns>
        [HttpGet("{id:int}", Name = Domain.Constants.RouteNames.Quotations.GetById)]
        [HasPermission(Quotations.View)]
        [ProducesResponseType(typeof(QuotationDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetQuotationByIdAsync(int id, CancellationToken cancellationToken)
        {
            var query = new GetQuotationByIdQuery { Id = id };
            var result = await mediator.Send(query, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Tạo mới yêu cầu báo giá.
        /// </summary>
        /// <param name="command">Thông tin yêu cầu báo giá cần tạo.</param>
        /// <param name="cancellationToken">Token hủy bỏ.</param>
        /// <returns>Yêu cầu báo giá vừa tạo.</returns>
        [HttpPost]
        [HasPermission(Quotations.Create)]
        [ProducesResponseType(typeof(QuotationDetailResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateQuotationAsync(
            [FromBody] CreateQuotationCommand command,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
            return HandleCreated(
                result,
                Domain.Constants.RouteNames.Quotations.GetById,
                new { id = result.IsSuccess && result.Value != null ? result.Value.Id : 0 });
        }

        /// <summary>
        /// Cập nhật thông tin yêu cầu báo giá.
        /// </summary>
        /// <param name="id">ID yêu cầu báo giá.</param>
        /// <param name="command">Thông tin cập nhật.</param>
        /// <param name="cancellationToken">Token hủy bỏ.</param>
        /// <returns>Thông tin yêu cầu báo giá sau cập nhật.</returns>
        [HttpPut("{id:int}")]
        [HasPermission(Quotations.Edit)]
        [ProducesResponseType(typeof(QuotationDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateQuotationAsync(
            int id,
            [FromBody] UpdateQuotationCommand command,
            CancellationToken cancellationToken)
        {
            var request = command with { Id = id };
            var result = await mediator.Send(request, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Gửi yêu cầu báo giá (draft -> sent).
        /// </summary>
        [HttpPatch("{id:int}/send")]
        [HasPermission(Quotations.Send)]
        [ProducesResponseType(typeof(QuotationDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SendQuotationAsync(int id, CancellationToken cancellationToken)
        {
            var command = new SendQuotationCommand(id);
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Xác nhận yêu cầu báo giá (sent -> approved).
        /// </summary>
        [HttpPatch("{id:int}/approve")]
        [HasPermission(Quotations.Approve)]
        [ProducesResponseType(typeof(QuotationDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ApproveQuotationAsync(int id, CancellationToken cancellationToken)
        {
            var command = new ApproveQuotationCommand(id);
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Hủy yêu cầu báo giá (sent -> rejected).
        /// </summary>
        [HttpPatch("{id:int}/reject")]
        [HasPermission(Quotations.Approve)]
        [ProducesResponseType(typeof(QuotationDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RejectQuotationAsync(int id, CancellationToken cancellationToken)
        {
            var command = new RejectQuotationCommand(id);
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }

        /// <summary>
        /// Xóa yêu cầu báo giá (Soft delete).
        /// </summary>
        /// <param name="id">ID yêu cầu báo giá.</param>
        /// <param name="cancellationToken">Token hủy bỏ.</param>
        /// <returns>Trạng thái thực hiện.</returns>
        [HttpDelete("{id:int}")]
        [HasPermission(Quotations.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteQuotationAsync(
            int id, 
            [FromServices] IAuthorizationService authorizationService,
            CancellationToken cancellationToken)
        {
            var authResult = await authorizationService.AuthorizeAsync(
                User,
                null,
                new Infrastructure.Authorization.Requirement.PermissionRequirement(Quotations.Approve));

            var command = new DeleteQuotationCommand { Id = id, HasApprovePermission = authResult.Succeeded };
            var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
            return HandleResult(result);
        }
    }
}
