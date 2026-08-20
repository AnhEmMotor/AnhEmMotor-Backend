using Application.ApiContracts.Sales.Returns.Responses;
using Application.Common.Models;
using Application.Features.Sales.Returns.Commands.ProcessReturnRequest;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers.V1.Sales;

namespace ControllerTests;

public class SalesReturns
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly ReturnsController _controller;

    public SalesReturns()
    {
        _controller = new ReturnsController(_mediatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact(DisplayName = "RETURNS_001 - Route ID được gán vào lệnh xử lý trả hàng")]
    public async Task ProcessReturnRequest_RouteId_SendsCommandWithMatchingId()
    {
        var command = new ProcessReturnRequestCommand { Status = "inspecting" };
        _mediatorMock
            .Setup(mediator => mediator.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ReturnRequestResponse>.Success(new ReturnRequestResponse { Id = 1 }));

        var result = await _controller.ProcessReturnRequest(
            1,
            command,
            TestContext.Current.CancellationToken);

        result.Should().BeOfType<OkObjectResult>();
        command.ReturnRequestId.Should().Be(1);
        _mediatorMock.Verify(
            mediator => mediator.Send(command, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
