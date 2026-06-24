using Application.Features.HR.Commands.CreateCommissionPolicy;
using Application.Features.HR.Commands.DeleteCommissionPolicy;
using Application.Features.HR.Commands.UpdateCommissionPolicy;
using Application.Features.HR.Queries.GetCommissionPolicies;
using Application.Features.HR.Queries.GetCommissionPolicyAuditLogs;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý các chính sách hoa hồng.
/// </summary>
/// <param name="mediator">Instance của mediator.</param>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý các chính sách hoa hồng")]
[Route("api/v{version:apiVersion}/hr/commission-policies")]
public class CommissionPolicyController(ISender mediator) : ApiController
{
    /// <summary>
    /// Lấy danh sách các chính sách hoa hồng.
    /// </summary>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách chính sách hoa hồng.</returns>
    [HttpGet]
    public async Task<IActionResult> GetPoliciesAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCommissionPoliciesQuery(), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Tạo mới một chính sách hoa hồng.
    /// </summary>
    /// <param name="command">Lệnh tạo chính sách.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Dữ liệu chính sách mới được tạo.</returns>
    [HttpPost]
    public async Task<IActionResult> CreatePolicyAsync(
        [FromBody] CreateCommissionPolicyCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Cập nhật chính sách hoa hồng hiện có.
    /// </summary>
    /// <param name="id">ID của chính sách.</param>
    /// <param name="command">Lệnh cập nhật chính sách.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả thực hiện.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePolicyAsync(
        int id,
        [FromBody] UpdateCommissionPolicyCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest();
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Lấy lịch sử thay đổi (audit logs) của chính sách.
    /// </summary>
    /// <param name="id">ID của chính sách.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Danh sách lịch sử thay đổi.</returns>
    [HttpGet("{id}/audit-logs")]
    public async Task<IActionResult> GetAuditLogsAsync(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetCommissionPolicyAuditLogsQuery(id), cancellationToken)
            .ConfigureAwait(true);
        return HandleResult(result);
    }

    /// <summary>
    /// Xóa một chính sách hoa hồng.
    /// </summary>
    /// <param name="id">ID của chính sách cần xóa.</param>
    /// <param name="cancellationToken">Token hủy bỏ.</param>
    /// <returns>Kết quả thực hiện.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePolicyAsync(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteCommissionPolicyCommand(id), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }
}
