using Application.Common.Models;
using Application.Features.HR.Queries.GetEmployees;
using Application.Features.HR.Commands.UpdateEmployee;
using Application.Features.HR.Commands.CreateEmployee;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
    /// <returns>A list of employee DTOs.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<EmployeeDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeesAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetEmployeesQuery(), cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Creates a new employee profile.
    /// </summary>
    /// <param name="command">The create command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created employee ID.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateEmployeeAsync([FromBody] CreateEmployeeCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Updates an employee profile.
    /// </summary>
    /// <param name="id">The employee ID.</param>
    /// <param name="command">The update command.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated employee ID.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateEmployeeAsync(int id, [FromBody] UpdateEmployeeCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest("ID mismatch.");
        }

        var result = await mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }
}
