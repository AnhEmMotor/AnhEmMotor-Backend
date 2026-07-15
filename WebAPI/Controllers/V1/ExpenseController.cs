using Application.Common.Models;
using Application.Features.Expenses.Responses;
using Application.Features.Expenses.Commands.CreateExpense;
using Application.Features.Expenses.Commands.DeleteExpense;
using Application.Features.Expenses.Commands.UpdateExpense;
using Application.Features.Expenses.Queries.GetExpenses;
using Asp.Versioning;
using Domain.Constants.Permission;
using Domain.Primitives;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sieve.Models;
using Swashbuckle.AspNetCore.Annotations;
using WebAPI.Controllers.Base;

namespace WebAPI.Controllers.V1;

/// <summary>
/// Quản lý chi phí.
/// </summary>
[ApiVersion("1.0")]
[SwaggerTag("Quản lý chi phí")]
[Route("api/v{version:apiVersion}/expenses")]
[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
public class ExpenseController(IMediator mediator) : ApiController
{
	/// <summary>
	/// Lấy danh sách các khoản chi phí có phân trang.
	/// </summary>
	/// <param name="sieveModel">Tham số phân trang, lọc, sắp xếp.</param>
	/// <param name="cancellationToken">Token hủy bỏ.</param>
	/// <returns>Danh sách các khoản chi phí có phân trang.</returns>
	[HttpGet]
	[RequiresAnyPermissions(
		Permissions.Admin.DashboardManagement.View,
		Permissions.Accountant.DashboardManagement.View,
		Permissions.Factory.DashboardManagement.View)]
	[ProducesResponseType(typeof(PagedResult<ExpenseResponse>), StatusCodes.Status200OK)]
	public async Task<IActionResult> GetAll([FromQuery] SieveModel sieveModel, CancellationToken cancellationToken)
	{
		var result = await mediator.Send(new GetExpensesQuery(sieveModel), cancellationToken).ConfigureAwait(false);
		return HandleResult(result);
	}

	/// <summary>
	/// Tạo mới một khoản chi phí.
	/// </summary>
	/// <param name="command">Thông tin khoản chi phí mới cần tạo.</param>
	/// <param name="cancellationToken">Token hủy bỏ.</param>
	/// <returns>Thông tin khoản chi phí vừa được tạo.</returns>
	[HttpPost]
	[RequiresAnyPermissions(
		Permissions.Admin.DashboardManagement.View,
		Permissions.Accountant.DashboardManagement.View,
		Permissions.Factory.DashboardManagement.View)]
	[ProducesResponseType(typeof(ExpenseResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> Create([FromBody] CreateExpenseCommand command, CancellationToken cancellationToken)
	{
		var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
		return HandleResult(result);
	}

	/// <summary>
	/// Cập nhật một khoản chi phí theo ID.
	/// </summary>
	/// <param name="id">ID của khoản chi phí cần cập nhật.</param>
	/// <param name="command">Thông tin cập nhật khoản chi phí.</param>
	/// <param name="cancellationToken">Token hủy bỏ.</param>
	/// <returns>Thông tin khoản chi phí sau khi cập nhật.</returns>
	[HttpPut("{id:int}")]
	[RequiresAnyPermissions(
		Permissions.Admin.DashboardManagement.View,
		Permissions.Accountant.DashboardManagement.View,
		Permissions.Factory.DashboardManagement.View)]
	[ProducesResponseType(typeof(ExpenseResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> Update(int id, [FromBody] UpdateExpenseCommand command, CancellationToken cancellationToken)
	{
		if (id != command.Id)
			return BadRequest("ID không khớp.");

		var result = await mediator.Send(command, cancellationToken).ConfigureAwait(false);
		return HandleResult(result);
	}

	/// <summary>
	/// Xóa một khoản chi phí theo ID.
	/// </summary>
	/// <param name="id">ID của khoản chi phí cần xóa.</param>
	/// <param name="cancellationToken">Token hủy bỏ.</param>
	/// <returns>Kết quả xóa (true nếu thành công).</returns>
	[HttpDelete("{id:int}")]
	[RequiresAnyPermissions(
		Permissions.Admin.DashboardManagement.View,
		Permissions.Accountant.DashboardManagement.View,
		Permissions.Factory.DashboardManagement.View)]
	[ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
	public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
	{
		var result = await mediator.Send(new DeleteExpenseCommand(id), cancellationToken).ConfigureAwait(false);
		return HandleResult(result);
	}
}
