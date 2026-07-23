using Application.Features.WarrantyTerms.Commands.CreateWarrantyTerm;
using Application.Features.WarrantyTerms.Commands.DeleteWarrantyTerm;
using Application.Features.WarrantyTerms.Commands.UpdateWarrantyTerm;
using Application.Features.WarrantyTerms.Queries.GetWarrantyTermById;
using Application.Features.WarrantyTerms.Queries.GetWarrantyTermsList;
using Application.Features.WarrantyTerms.Queries.GetWarrantyTermStatistics;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

[ApiVersion("1.0")]
[SwaggerTag("Qu?n lý di?u kho?n b?o hành (Warranty Terms)")]
[Route("api/v{version:apiVersion}/[controller]")]
public class WarrantyTermsController(ISender sender) : ApiController
{
    [HttpGet]
    [SwaggerOperation(Summary = "L?y danh sách di?u kho?n b?o hành")]
    public async Task<IActionResult> GetListAsync(
        [FromQuery] GetWarrantyTermsListQuery query,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    [HttpGet("statistics")]
    [SwaggerOperation(Summary = "Th?ng kê di?u kho?n b?o hành")]
    public async Task<IActionResult> GetStatisticsAsync(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWarrantyTermStatisticsQuery(), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Chi ti?t di?u kho?n b?o hành")]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWarrantyTermByIdQuery(id), cancellationToken).ConfigureAwait(true);
        return HandleResult(result);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Thêm m?i di?u kho?n b?o hành")]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateWarrantyTermCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPut("{id}")]

    [SwaggerOperation(Summary = "C?p nh?t di?u kho?n b?o hành")]
    public async Task<IActionResult> UpdateAsync(
        int id,
        [FromBody] UpdateWarrantyTermCommand request,
        CancellationToken cancellationToken)
    {
        request.Id = id;
        var command = request;
        var result = await sender.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]

    [SwaggerOperation(Summary = "Xóa di?u kho?n b?o hành")]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteWarrantyTermCommand(id), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}
