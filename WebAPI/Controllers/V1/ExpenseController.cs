using Application.Features.Expenses.Commands.CreateExpense;
using Application.Features.Expenses.Commands.DeleteExpense;
using Application.Features.Expenses.Queries.GetExpenses;
using Application.Features.Expenses.Responses;
using Asp.Versioning;
using Domain.Constants.Permission;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;
using Application.Common.Models;

namespace WebAPI.Controllers.V1;

[ApiVersion("1.0")]
[SwaggerTag("Quản lý chi phí")]
[Route("api/v{version:apiVersion}/expenses")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class ExpenseController(IMediator mediator) : ApiController
{
    [HttpGet]
    [RequiresAnyPermissions(Permissions.Admin.DashboardManagement.View, Permissions.Accountant.DashboardManagement.View, Permissions.Factory.DashboardManagement.View)]
    [ProducesResponseType(typeof(List<ExpenseResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetExpensesQuery(), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpPost]
    [RequiresAnyPermissions(Permissions.Admin.DashboardManagement.View, Permissions.Accountant.DashboardManagement.View, Permissions.Factory.DashboardManagement.View)]
    [ProducesResponseType(typeof(ExpenseResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateExpenseCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    [HttpDelete("{id:int}")]
    [RequiresAnyPermissions(Permissions.Admin.DashboardManagement.View, Permissions.Accountant.DashboardManagement.View, Permissions.Factory.DashboardManagement.View)]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new DeleteExpenseCommand(id), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}
