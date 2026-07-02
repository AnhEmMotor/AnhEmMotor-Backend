using Application.Common.Models;
using Application.Features.SalesReports.Queries.GetSalesReport;
using Asp.Versioning;
using Domain.Constants.Permission;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Báo cáo bán hàng.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Báo cáo bán hàng")]
[Route("api/v{version:apiVersion}/sales")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class SalesReportsController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Lấy báo cáo tổng hợp bán hàng (số lượng đơn, doanh thu, tổng sản phẩm đã bán, đơn giá trung bình và chi tiết từng đơn hàng).
    /// </summary>
    [HttpGet("sales-report")]
    [RequiresAnyPermissions(Permissions.Admin.DashboardManagement.View, Permissions.Accountant.DashboardManagement.View, Permissions.Factory.DashboardManagement.View)]
    [ProducesResponseType(typeof(SalesReportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSalesReportAsync(CancellationToken cancellationToken)
    {
        var query = new GetSalesReportQuery();
        var result = await mediator.Send(query, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}
