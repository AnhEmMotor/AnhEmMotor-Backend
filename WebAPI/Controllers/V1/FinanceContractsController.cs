using Application.ApiContracts.FinanceContract.Requests;
using Application.Features.FinanceContracts.Commands.CreateFinanceContract;
using Application.Features.FinanceContracts.Commands.DeleteFinanceContract;
using Application.Features.FinanceContracts.Commands.UpdateCavetState;
using Application.Features.FinanceContracts.Commands.UpdateDisbursementPayment;
using Application.Features.FinanceContracts.Commands.UpdateFinanceContract;
using Application.Features.FinanceContracts.Commands.UploadDisbursementEvidence;
using Application.Features.FinanceContracts.Queries.GetFinanceContractDetail;
using Application.Features.FinanceContracts.Queries.GetFinanceContractsList;
using Application.Common.Models;
using Asp.Versioning;
using Domain.Constants.Permission;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;
using WebAPI.Models;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý hợp đồng tài chính — phân trang, tạo, cập nhật, xoá, cập nhật giải ngân và tải lên bằng chứng giải ngân.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý hợp đồng tài chính")]
[Route("api/v{version:apiVersion}/[controller]")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class FinanceContractsController(ISender sender) : ApiController
{
    /// <summary>
    /// Lấy danh sách hợp đồng tài chính (có phân trang, lọc, sắp xếp theo quy tắc Sieve).
    /// </summary>
    /// <param name="sieveModel">Tham số phân trang, lọc, sắp xếp của Sieve.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách hợp đồng tài chính đã phân trang.</returns>
    /// <response code="200">Trả về danh sách hợp đồng tài chính thành công.</response>
    /// <response code="400">Tham số truy vấn không hợp lệ.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    [HttpGet]
    [HasPermission(Permissions.Admin.FinanceContractManagement.View)]
    [ProducesResponseType(typeof(PagedResult<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFinanceContractsList(
        [FromQuery] SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        var query = new GetFinanceContractsListQuery { SieveModel = sieveModel };
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Lấy thông tin chi tiết của một hợp đồng tài chính theo ID (GUID).
    /// </summary>
    /// <param name="financeContractId">ID (GUID) của hợp đồng tài chính cần xem chi tiết.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Thông tin chi tiết hợp đồng tài chính bao gồm thông tin người mua, xe, lịch sử giải ngân, cọc giữ chỗ.</returns>
    /// <response code="200">Trả về chi tiết hợp đồng tài chính thành công.</response>
    /// <response code="404">Không tìm thấy hợp đồng tài chính.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    [HttpGet("{financeContractId:guid}", Name = "GetFinanceContractDetail")]
    [HasPermission(Permissions.Admin.FinanceContractManagement.View)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetFinanceContractDetail(
        [FromRoute] Guid financeContractId,
        CancellationToken cancellationToken)
    {
        var query = new GetFinanceContractDetailQuery(
            new GetFinanceContractDetailRequest(financeContractId),
            Guid.Empty);
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Tạo mới một hợp đồng tài chính.
    /// </summary>
    /// <param name="request">Thông tin hợp đồng tài chính cần tạo (người mua, xe, lịch sử giải ngân, cọc giữ chỗ, v.v.).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Hợp đồng tài chính vừa được tạo thành công.</returns>
    /// <response code="201">Tạo hợp đồng tài chính thành công — trả về ID hợp đồng mới tạo.</response>
    /// <response code="400">Dữ liệu không hợp lệ.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    [HttpPost]
    [HasPermission(Permissions.Admin.FinanceContractManagement.Create)]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateFinanceContract(
        [FromBody] CreateFinanceContractRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateFinanceContractCommand(request, Guid.Empty);
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess
            ? CreatedAtAction(nameof(GetFinanceContractDetail), new { financeContractId = result.Value }, result.Value)
            : BadRequest(result.Error);
    }

    /// <summary>
    /// Cập nhật thông tin một hợp đồng tài chính đã có.
    /// </summary>
    /// <param name="financeContractId">ID (GUID) của hợp đồng tài chính cần cập nhật.</param>
    /// <param name="request">Thông tin cập nhật hợp đồng tài chính.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Hợp đồng tài chính sau khi cập nhật.</returns>
    /// <response code="200">Cập nhật hợp đồng tài chính thành công.</response>
    /// <response code="400">Dữ liệu không hợp lệ hoặc hợp đồng đã duyệt không thể sửa.</response>
    /// <response code="404">Không tìm thấy hợp đồng tài chính.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    [HttpPut("{financeContractId:guid}")]
    [HasPermission(Permissions.Admin.FinanceContractManagement.Edit)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateFinanceContract(
        [FromRoute] Guid financeContractId,
        [FromBody] UpdateFinanceContractRequest request,
        CancellationToken cancellationToken)
    {
        request.Id = financeContractId;
        var command = new UpdateFinanceContractCommand(financeContractId, request, Guid.Empty);
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Xoá (soft-delete) một hợp đồng tài chính.
    /// </summary>
    /// <param name="financeContractId">ID (GUID) của hợp đồng tài chính cần xoá.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>204 No Content nếu xoá thành công.</returns>
    /// <response code="204">Xoá hợp đồng tài chính thành công.</response>
    /// <response code="400">Không thể xoá (hợp đồng đã có giao dịch liên quan).</response>
    /// <response code="404">Không tìm thấy hợp đồng tài chính.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    [HttpDelete("{financeContractId:guid}")]
    [HasPermission(Permissions.Admin.FinanceContractManagement.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteFinanceContract(
        [FromRoute] Guid financeContractId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteFinanceContractCommand(financeContractId);
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    /// <summary>
    /// Cập nhật thông tin thanh toán giải ngân (disbursement) của một hợp đồng tài chính.
    /// </summary>
    /// <param name="financeContractId">ID (GUID) của hợp đồng tài chính.</param>
    /// <param name="request">Thông tin cập nhật giải ngân (số tiền, ngày giải ngân, phương thức, v.v.).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả cập nhật thanh toán giải ngân.</returns>
    /// <response code="200">Cập nhật giải ngân thành công.</response>
    /// <response code="400">Dữ liệu không hợp lệ.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    [HttpPost("{financeContractId:guid}/disbursement/payment")]
    [HasPermission(Permissions.Admin.FinanceContractManagement.Edit)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateDisbursementPayment(
        [FromRoute] Guid financeContractId,
        [FromBody] UpdateDisbursementPaymentRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateDisbursementPaymentCommand(financeContractId, request, Guid.Empty), cancellationToken)
            .ConfigureAwait(false);
        return Ok(new { success = true });
    }

    /// <summary>
    /// Cập nhật trạng thái của giấy chứng nhận đăng ký xe (CAVET/Đăng ký xe) liên quan đến hợp đồng tài chính.
    /// </summary>
    /// <param name="financeContractId">ID (GUID) của hợp đồng tài chính.</param>
    /// <param name="request">Trạng thái mới của CAVET (đã nhận, đang chuyển, đã giao, v.v.).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả cập nhật trạng thái CAVET.</returns>
    /// <response code="200">Cập nhật trạng thái CAVET thành công.</response>
    /// <response code="400">Dữ liệu không hợp lệ.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    [HttpPost("{financeContractId:guid}/cavet/state")]
    [HasPermission(Permissions.Admin.FinanceContractManagement.Edit)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateCavetState(
        [FromRoute] Guid financeContractId,
        [FromBody] UpdateCavetStateRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateCavetStateCommand(financeContractId, request, Guid.Empty), cancellationToken)
            .ConfigureAwait(false);
        return Ok(new { success = true });
    }

    /// <summary>
    /// Tải lên bằng chứng giải ngân (evidence file: PDF, hình ảnh) cho một hợp đồng tài chính.
    /// </summary>
    /// <param name="financeContractId">ID (GUID) của hợp đồng tài chính cần tải bằng chứng.</param>
    /// <param name="form">Form data chứa tệp bằng chứng (multipart/form-data).</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả tải lên bằng chứng giải ngân.</returns>
    /// <response code="200">Tải lên bằng chứng giải ngân thành công.</response>
    /// <response code="400">File bằng chứng bắt buộc hoặc định dạng không hợp lệ.</response>
    /// <response code="401">Chưa đăng nhập hoặc token không hợp lệ.</response>
    [HttpPost("{financeContractId:guid}/disbursement/evidence/upload")]
    [Consumes("multipart/form-data")]
    [HasPermission(Permissions.Admin.FinanceContractManagement.Edit)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadDisbursementEvidence(
        [FromRoute] Guid financeContractId,
        [FromForm] UploadDisbursementEvidenceForm form,
        CancellationToken cancellationToken)
    {
        if (form?.File == null)
            return BadRequest(new { success = false, message = "File is required" });
        using var stream = form.File.OpenReadStream();
        await sender.Send(
            new UploadDisbursementEvidenceCommand(
                financeContractId,
                new UploadDisbursementEvidenceRequest { FileContent = stream, FileName = form.File.FileName },
                Guid.Empty),
            cancellationToken).ConfigureAwait(false);
        return Ok(new { success = true });
    }
}
