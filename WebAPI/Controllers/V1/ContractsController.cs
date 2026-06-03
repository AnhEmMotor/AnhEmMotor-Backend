using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Controller quản lý hợp đồng.
/// </summary>
/// <param name="sender"></param>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý hợp đồng")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ContractsController(ISender sender) : ApiController
{
    // Sales Contracts Endpoints
    [HttpGet("sales")]
    [Authorize]
    public async Task<IActionResult> GetSalesContractsAsync(CancellationToken cancellationToken)
    {
        return Ok(new { message = "Get Sales Contracts" });
    }

    [HttpPost("sales")]
    [Authorize]
    public async Task<IActionResult> CreateSalesContractAsync(CancellationToken cancellationToken)
    {
        return Ok(new { message = "Create Sales Contract" });
    }

    // Finance Contracts Endpoints
    [HttpGet("finance")]
    [Authorize]
    public async Task<IActionResult> GetFinanceContractsAsync(CancellationToken cancellationToken)
    {
        return Ok(new { message = "Get Finance Contracts" });
    }

    [HttpPost("finance")]
    [Authorize]
    public async Task<IActionResult> CreateFinanceContractAsync(CancellationToken cancellationToken)
    {
        return Ok(new { message = "Create Finance Contract" });
    }

    // Supplier Contracts Endpoints
    [HttpGet("supplier")]
    [Authorize]
    public async Task<IActionResult> GetSupplierContractsAsync(CancellationToken cancellationToken)
    {
        return Ok(new { message = "Get Supplier Contracts" });
    }

    [HttpPost("supplier")]
    [Authorize]
    public async Task<IActionResult> CreateSupplierContractAsync(CancellationToken cancellationToken)
    {
        return Ok(new { message = "Create Supplier Contract" });
    }

    // Contract Templates Endpoints
    [HttpGet("templates")]
    [Authorize]
    [SwaggerOperation(Summary = "Lấy danh sách mẫu hợp đồng", Description = "Hỗ trợ phân trang, tìm kiếm và lọc dữ liệu")]
    public async Task<IActionResult> GetContractTemplatesAsync([FromQuery] Sieve.Models.SieveModel request, CancellationToken cancellationToken)
    {
        var query = Application.Features.ContractTemplates.Queries.GetContractTemplates.GetContractTemplatesQuery.FromRequest(request);
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("templates/{id:guid}")]
    [Authorize]
    [SwaggerOperation(Summary = "Lấy chi tiết mẫu hợp đồng theo ID")]
    public async Task<IActionResult> GetContractTemplateByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var query = new Application.Features.ContractTemplates.Queries.GetContractTemplateById.GetContractTemplateByIdQuery(id);
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost("templates")]
    [Authorize]
    [SwaggerOperation(Summary = "Tạo mới mẫu hợp đồng")]
    public async Task<IActionResult> CreateContractTemplateAsync([FromBody] Application.ApiContracts.ContractTemplate.Requests.CreateContractTemplateRequest request, CancellationToken cancellationToken)
    {
        var command = new Application.Features.ContractTemplates.Commands.CreateContractTemplate.CreateContractTemplateCommand(request);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? CreatedAtAction(nameof(GetContractTemplateByIdAsync), new { id = result.Value }, result.Value) : BadRequest(result.Error);
    }

    [HttpPut("templates/{id:guid}")]
    [Authorize]
    [SwaggerOperation(Summary = "Cập nhật mẫu hợp đồng (Chỉ dùng khi chưa phát sinh hợp đồng)")]
    public async Task<IActionResult> UpdateContractTemplateAsync(Guid id, [FromBody] Application.ApiContracts.ContractTemplate.Requests.UpdateContractTemplateRequest request, CancellationToken cancellationToken)
    {
        var command = new Application.Features.ContractTemplates.Commands.UpdateContractTemplate.UpdateContractTemplateCommand(id, request);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpDelete("templates/{id:guid}")]
    [Authorize]
    [SwaggerOperation(Summary = "Xóa mềm mẫu hợp đồng")]
    public async Task<IActionResult> DeleteContractTemplateAsync(Guid id, CancellationToken cancellationToken)
    {
        var command = new Application.Features.ContractTemplates.Commands.DeleteContractTemplate.DeleteContractTemplateCommand(id);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? NoContent() : BadRequest(result.Error);
    }

    [HttpPost("templates/{id:guid}/clone")]
    [Authorize]
    [SwaggerOperation(Summary = "Nhân bản mẫu hợp đồng thành phiên bản mới (Clone to New Version)")]
    public async Task<IActionResult> CloneContractTemplateAsync(Guid id, CancellationToken cancellationToken)
    {
        var command = new Application.Features.ContractTemplates.Commands.CloneContractTemplate.CloneContractTemplateCommand(id);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(new { id = result.Value }) : BadRequest(result.Error);
    }

    [HttpPost("templates/validate-syntax")]
    [Authorize]
    [SwaggerOperation(Summary = "Kiểm tra cú pháp từ khóa động {{...}} trong nội dung mẫu")]
    public async Task<IActionResult> ValidateContractTemplateSyntaxAsync([FromBody] ValidateSyntaxRequest request, CancellationToken cancellationToken)
    {
        var command = new Application.Features.ContractTemplates.Commands.ValidateContractTemplateSyntax.ValidateContractTemplateSyntaxCommand(request.Content);
        var result = await sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(new { valid = true }) : BadRequest(new { valid = false, error = result.Error!.Message });
    }
}

public sealed record ValidateSyntaxRequest(string Content);
