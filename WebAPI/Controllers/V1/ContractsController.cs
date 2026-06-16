using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

[ApiVersion("1.0")]
[SwaggerTag("Quản lý hợp đồng")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ContractsController(ISender sender) : ApiController
{
  // Sales Contracts Endpoints
  [HttpGet("sales")]
  [Authorize]
  [SwaggerOperation(Summary = "Lấy danh sách hợp đồng bán xe", Description = "Hỗ trợ phân trang, tìm kiếm và lọc")]
  public async Task<IActionResult> GetSalesContractsAsync(
      [FromQuery] string? keyword,
      [FromQuery] string? status,
      [FromQuery] string? vehicleModel,
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 10,
      CancellationToken cancellationToken = default)
  {
      var filters = new System.Collections.Generic.List<string>();
      if (!string.IsNullOrWhiteSpace(status))
      {
          filters.Add($"Status=={status}");
      }
      if (!string.IsNullOrWhiteSpace(vehicleModel))
      {
          filters.Add($"VehicleModel@={vehicleModel}");
      }
      if (!string.IsNullOrWhiteSpace(keyword))
      {
          filters.Add($"ContractNumber@={keyword}|CustomerFullName@={keyword}|CustomerCCCD@={keyword}|CustomerPhone@={keyword}|FrameNumber@={keyword}|EngineNumber@={keyword}");
      }

      var sieveModel = new Sieve.Models.SieveModel
      {
          Page = page,
          PageSize = pageSize,
          Filters = filters.Count > 0 ? string.Join(",", filters) : null,
          Sorts = "createdAt desc"
      };

      var query = new Application.Features.SalesContracts.Queries.GetSalesContractsList.GetSalesContractsListQuery { SieveModel = sieveModel };
      var result = await sender.Send(query, cancellationToken);
      return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
  }

  [HttpGet("sales/{id:guid}")]
  [Authorize]
  [SwaggerOperation(Summary = "Lấy chi tiết hợp đồng bán xe theo ID")]
  public async Task<IActionResult> GetSalesContractByIdAsync(Guid id, CancellationToken cancellationToken)
  {
    var query = new Application.Features.SalesContracts.Queries.GetSalesContractById.GetSalesContractByIdQuery(id);
    var result = await sender.Send(query, cancellationToken);
    return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
  }

  [HttpPost("sales")]
  [Authorize]
  [SwaggerOperation(Summary = "Tạo mới hợp đồng bán xe")]
  public async Task<IActionResult> CreateSalesContractAsync([FromBody] Application.ApiContracts.SalesContracts.Requests.CreateSalesContractRequest request, CancellationToken cancellationToken)
  {
    var command = new Application.Features.SalesContracts.Commands.CreateSalesContract.CreateSalesContractCommand(request);
    var result = await sender.Send(command, cancellationToken);
    return result.IsSuccess ? CreatedAtAction(nameof(GetSalesContractByIdAsync), new { id = result.Value!.Id }, result.Value) : BadRequest(result.Error);
  }

  [HttpPut("sales/{id:guid}")]
  [Authorize]
  [SwaggerOperation(Summary = "Cập nhật hợp đồng bán xe (Điều khoản đặc biệt, bảo hành)")]
  public async Task<IActionResult> UpdateSalesContractAsync(Guid id, [FromBody] Application.ApiContracts.SalesContracts.Requests.UpdateSalesContractRequest request, CancellationToken cancellationToken)
  {
    var command = new Application.Features.SalesContracts.Commands.UpdateSalesContract.UpdateSalesContractCommand(id, request);
    var result = await sender.Send(command, cancellationToken);
    return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
  }

  [HttpDelete("sales/{id:guid}")]
  [Authorize]
  [SwaggerOperation(Summary = "Xóa mềm hợp đồng bán xe")]
  public async Task<IActionResult> DeleteSalesContractAsync(Guid id, CancellationToken cancellationToken)
  {
    var command = new Application.Features.SalesContracts.Commands.DeleteSalesContract.DeleteSalesContractCommand(id);
    var result = await sender.Send(command, cancellationToken);
    return result.IsSuccess ? NoContent() : BadRequest(result.Error);
  }

  [HttpPatch("sales/{id:guid}/status")]
  [Authorize]
  [SwaggerOperation(Summary = "Cập nhật trạng thái hợp đồng (Draft -> Signed -> Fulfilled)")]
  public async Task<IActionResult> UpdateSalesContractStatusAsync(Guid id, [FromBody] Application.ApiContracts.SalesContracts.Requests.UpdateContractStatusRequest request, CancellationToken cancellationToken)
  {
    var command = new Application.Features.SalesContracts.Commands.UpdateSalesContractStatus.UpdateSalesContractStatusCommand(id, request.Status);
    var result = await sender.Send(command, cancellationToken);
    return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
  }

  [HttpGet("sales/statistics")]
  [Authorize]
  [SwaggerOperation(Summary = "Lấy thống kê hợp đồng bán xe (Draft, Signed, Fulfilled)")]
  public async Task<IActionResult> GetSalesContractStatisticsAsync(CancellationToken cancellationToken)
  {
    var query = new Application.Features.SalesContracts.Queries.GetSalesContractStatistics.GetSalesContractStatisticsQuery();
    var result = await sender.Send(query, cancellationToken);
    return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
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
