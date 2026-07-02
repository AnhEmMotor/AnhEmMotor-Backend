using Application.ApiContracts.HR.Responses;
using Application.Features.HR.Commands.CreateEmployee;
using Application.Features.HR.Commands.UpdateEmployee;
using Application.Features.HR.Queries.GetEmployees;
using Asp.Versioning;
using Domain.Constants.Permission;
using Infrastructure.Authorization.Attribute;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Controller for managing employees (HR).
/// </summary>
/// <param name="mediator">The mediator instance.</param>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý nhân viên (HR)")]
[Route("api/v{version:apiVersion}/hr/employees")]
public class EmployeeController(IMediator mediator) : ApiController
{
    /// <summary>
    /// Gets all employees.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of employee responses.</returns>
    [HttpGet]
    [RequiresAnyPermissions(Permissions.Admin.EmployeeManagement.View, Permissions.Accountant.EmployeeManagement.View)]
    [ProducesResponseType(typeof(List<EmployeeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeesAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetEmployeesQuery(), cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Creates a new employee profile.
    /// </summary>
    /// <param name="command">The create command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created employee ID.</returns>
    [HttpPost]
    [RequiresAnyPermissions(Permissions.Admin.EmployeeManagement.Create, Permissions.Accountant.EmployeeManagement.Create)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateEmployeeAsync(
        [FromBody] CreateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }

    /// <summary>
    /// Updates an employee profile.
    /// </summary>
    /// <param name="id">The employee ID.</param>
    /// <param name="request">The update command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated employee ID.</returns>
    [HttpPut("{id}")]
    [RequiresAnyPermissions(Permissions.Admin.EmployeeManagement.Edit, Permissions.Accountant.EmployeeManagement.Edit)]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateEmployeeAsync(
        int id,
        [FromBody] UpdateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var command = request.Adapt<UpdateEmployeeCommand>() with { Id = id };
        var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
        return HandleResult(result);
    }
}

