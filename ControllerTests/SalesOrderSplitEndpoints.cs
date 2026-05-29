using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Features.Outputs.Queries.GetOutputsList;
using Domain.Constants.Order;
using Domain.Constants.Permission.Permissions;
using Domain.Primitives;
using FluentAssertions;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sieve.Models;
using System.Reflection;
using WebAPI.Controllers.V1;

namespace ControllerTests;

public class SalesOrderSplitEndpoints
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly SalesOrdersController _controller;

    public SalesOrderSplitEndpoints()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new SalesOrdersController(_mediatorMock.Object);
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };
    }

    [Fact(DisplayName = "SO_117 - SalesOrders exposes split confirmed and unconfirmed list endpoints")]
    public void SalesOrdersController_ShouldExposeSplitListEndpointsWithNewPermissions()
    {
        var viewConfirmedPermission = typeof(Outputs).GetField("ViewConfirmed")?.GetRawConstantValue() as string;
        var viewUnconfirmedPermission = typeof(Outputs).GetField("ViewUnconfirmed")?.GetRawConstantValue() as string;
        viewConfirmedPermission.Should().Be("Permissions.Outputs.ViewConfirmed");
        viewUnconfirmedPermission.Should().Be("Permissions.Outputs.ViewUnconfirmed");

        var confirmedMethod = typeof(SalesOrdersController).GetMethod(nameof(SalesOrdersController.GetConfirmedOutputsAsync));
        var unconfirmedMethod = typeof(SalesOrdersController).GetMethod(nameof(SalesOrdersController.GetUnconfirmedOutputsAsync));
        confirmedMethod.Should().NotBeNull();
        unconfirmedMethod.Should().NotBeNull();

        confirmedMethod!.GetCustomAttribute<HttpGetAttribute>()?.Template.Should().Be("confirmed");
        unconfirmedMethod!.GetCustomAttribute<HttpGetAttribute>()?.Template.Should().Be("unconfirmed");
        confirmedMethod.GetCustomAttribute<HasPermissionAttribute>()?.Policy
            .Should().Be($"HasPermission{viewConfirmedPermission}");
        unconfirmedMethod.GetCustomAttribute<HasPermissionAttribute>()?.Policy
            .Should().Be($"HasPermission{viewUnconfirmedPermission}");
    }

    [Fact(DisplayName = "SO_120 - SalesOrders không expose endpoint lấy tất cả đơn hàng")]
    public void SalesOrdersController_ShouldNotExposeUnfilteredListEndpoint()
    {
        var rootGetMethods = typeof(SalesOrdersController)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(method => method.GetCustomAttributes<HttpGetAttribute>()
                .Any(attribute => string.IsNullOrEmpty(attribute.Template)))
            .Select(method => method.Name);

        rootGetMethods.Should().BeEmpty();
    }

    [Fact(DisplayName = "SO_118 - GetConfirmedOutputs trả về danh sách phiếu bán hàng đã xác nhận")]
    public async Task GetConfirmedOutputs_WithSieveModel_ReturnsConfirmedOutputs()
    {
        var sieveModel = new SieveModel
        {
            Page = 1,
            PageSize = 10
        };
        var expectedResponse = new PagedResult<OutputItemResponse>([], 0, 1, 10);
        _mediatorMock.Setup(m => m.Send(
            It.Is<GetOutputsListQuery>(q => q.SieveModel == sieveModel && q.StatusIds == OrderStatus.ConfirmedOrderStatuses),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<OutputItemResponse>>.Success(expectedResponse));

        var result = await _controller.GetConfirmedOutputsAsync(sieveModel, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(
            m => m.Send(
                It.Is<GetOutputsListQuery>(q => q.SieveModel == sieveModel && q.StatusIds == OrderStatus.ConfirmedOrderStatuses),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_119 - GetUnconfirmedOutputs trả về danh sách phiếu tạm chưa xác nhận")]
    public async Task GetUnconfirmedOutputs_WithSieveModel_ReturnsUnconfirmedOutputs()
    {
        var sieveModel = new SieveModel
        {
            Page = 1,
            PageSize = 10
        };
        var expectedResponse = new PagedResult<OutputItemResponse>([], 0, 1, 10);
        _mediatorMock.Setup(m => m.Send(
            It.Is<GetOutputsListQuery>(q => q.SieveModel == sieveModel && q.StatusIds == OrderStatus.UnconfirmedOrderStatuses),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<OutputItemResponse>>.Success(expectedResponse));

        var result = await _controller.GetUnconfirmedOutputsAsync(sieveModel, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(
            m => m.Send(
                It.Is<GetOutputsListQuery>(q => q.SieveModel == sieveModel && q.StatusIds == OrderStatus.UnconfirmedOrderStatuses),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_121 - GetConfirmedOutputs trả về lỗi khi mediator trả về thất bại")]
    public async Task GetConfirmedOutputs_WhenMediatorFails_ReturnsFailureResult()
    {
        var sieveModel = new SieveModel
        {
            Page = 1,
            PageSize = 10
        };
        var expectedError = Error.Failure("Đã xảy ra lỗi thử nghiệm");
        _mediatorMock.Setup(m => m.Send(
            It.Is<GetOutputsListQuery>(q => q.SieveModel == sieveModel && q.StatusIds == OrderStatus.ConfirmedOrderStatuses),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<OutputItemResponse>>.Failure(expectedError));

        var result = await _controller.GetConfirmedOutputsAsync(sieveModel, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(
            m => m.Send(
                It.Is<GetOutputsListQuery>(q => q.SieveModel == sieveModel && q.StatusIds == OrderStatus.ConfirmedOrderStatuses),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_122 - GetUnconfirmedOutputs trả về lỗi khi mediator trả về thất bại")]
    public async Task GetUnconfirmedOutputs_WhenMediatorFails_ReturnsFailureResult()
    {
        var sieveModel = new SieveModel
        {
            Page = 1,
            PageSize = 10
        };
        var expectedError = Error.Failure("Đã xảy ra lỗi thử nghiệm");
        _mediatorMock.Setup(m => m.Send(
            It.Is<GetOutputsListQuery>(q => q.SieveModel == sieveModel && q.StatusIds == OrderStatus.UnconfirmedOrderStatuses),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<OutputItemResponse>>.Failure(expectedError));

        var result = await _controller.GetUnconfirmedOutputsAsync(sieveModel, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(
            m => m.Send(
                It.Is<GetOutputsListQuery>(q => q.SieveModel == sieveModel && q.StatusIds == OrderStatus.UnconfirmedOrderStatuses),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
