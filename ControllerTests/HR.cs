using Application.ApiContracts.HR.Responses;
using Application.Common.Models;
using Application.Features.HR.Commands.UpdateEmployee;
using Application.Features.HR.Queries.GetPayrollSummary;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers.V1;

namespace ControllerTests;

public class HR
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly EmployeeController _employeeController;
    private readonly CommissionController _commissionController;

    public HR()
    {
        _mediatorMock = new Mock<IMediator>();
        _employeeController = new EmployeeController(_mediatorMock.Object);
        _commissionController = new CommissionController(_mediatorMock.Object);
        var httpContext = new DefaultHttpContext();
        _employeeController.ControllerContext = new ControllerContext() { HttpContext = httpContext };
        _commissionController.ControllerContext = new ControllerContext() { HttpContext = httpContext };
    }

    [Fact(DisplayName = "HR02 - Cập nhật lương cơ bản nhân viên thành công")]
    public async Task HR02_Update_BaseSalary_Success()
    {
        var command = new UpdateEmployeeCommand
        {
            Id = 1,
            BaseSalary = 15000000,
            IdentityNumber = "123456789",
            JobTitle = "Sales Manager"
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateEmployeeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Success(1));
        var result = await _employeeController.UpdateEmployeeAsync(1, command, CancellationToken.None)
            .ConfigureAwait(true);
        result.Should().BeOfType<OkObjectResult>();
        _mediatorMock.Verify(
            m => m.Send(It.Is<UpdateEmployeeCommand>(c => c.BaseSalary == 15000000), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "HR06 - Tổng hợp bảng lương tháng cho nhân viên")]
    public async Task HR06_GetPayrollSummary_Success()
    {
        var month = 12;
        var year = 2025;
        var summaryData = new List<PayrollResponse>
        {
            new() { FullName = "Test Employee", BaseSalary = 10000000, ConfirmedCommission = 5000000 }
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPayrollSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<PayrollResponse>>.Success(summaryData));
        var result = await _commissionController.GetPayrollSummaryAsync(month, year, CancellationToken.None)
            .ConfigureAwait(true);
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(summaryData);
    }

    [Fact(DisplayName = "HR06 - Tổng hợp bảng lương tháng khi không có dữ liệu")]
    public async Task HR06_GetPayrollSummary_Empty_Success()
    {
        var month = 1;
        var year = 2026;
        var emptyData = new List<PayrollResponse>();
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPayrollSummaryQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<PayrollResponse>>.Success(emptyData));
        var result = await _commissionController.GetPayrollSummaryAsync(month, year, CancellationToken.None)
            .ConfigureAwait(true);
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(emptyData);
    }
}

