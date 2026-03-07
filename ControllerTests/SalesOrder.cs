using Application.ApiContracts.Output.Responses;
using Application.Common.Models;
using Application.Features.Outputs.Commands.CreateOutput;
using Application.Features.Outputs.Commands.CreateOutputByManager;
using Application.Features.Outputs.Commands.DeleteManyOutputs;
using Application.Features.Outputs.Commands.DeleteOutput;
using Application.Features.Outputs.Commands.RestoreManyOutputs;
using Application.Features.Outputs.Commands.RestoreOutput;
using Application.Features.Outputs.Commands.UpdateManyOutputStatus;
using Application.Features.Outputs.Commands.UpdateOutput;
using Application.Features.Outputs.Commands.UpdateOutputForManager;
using Application.Features.Outputs.Commands.UpdateOutputStatus;
using Application.Features.Outputs.Queries.GetDeletedOutputsList;
using Application.Features.Outputs.Queries.GetOutputById;
using Application.Features.Outputs.Queries.GetOutputsByUserId;
using Application.Features.Outputs.Queries.GetOutputsList;
using Application.Features.Outputs.Queries.GetOutputStatusList;

using Domain.Primitives;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sieve.Models;
using System.Security.Claims;
using WebAPI.Controllers.V1;

namespace ControllerTests;

public class SalesOrder
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly SalesOrdersController _controller;

    public SalesOrder()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new SalesOrdersController(_mediatorMock.Object);

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };
    }

#pragma warning disable IDE0079 
#pragma warning disable CRR0035
    [Fact(DisplayName = "SO_081 - GetMyPurchases - L?y don hng c?a chnh mnh")]
    public async Task GetMyPurchases_UserAuthenticated_ReturnsOrders()
    {
        var buyerId = Guid.NewGuid();

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, buyerId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
        var sieveModel = new SieveModel();
        var expectedOrder = new OutputItemResponse { Id = 1, BuyerId = buyerId.ToString() };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetOutputsByUserIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result<PagedResult<OutputItemResponse>>.Success(
                    new PagedResult<OutputItemResponse>([ expectedOrder ], 1, 1, 10)));

        var result = await _controller.GetMyPurchasesAsync(sieveModel, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<GetOutputsByUserIdQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_082 - GetPurchasesByID - L?y don h�ng theo BuyerId (c� quy?n)")]
    public async Task GetPurchasesByID_WithPermission_ReturnsOrders()
    {
        var buyerId = Guid.NewGuid();
        var sieveModel = new SieveModel();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetOutputsByUserIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result<PagedResult<OutputItemResponse>>.Success(new PagedResult<OutputItemResponse>([], 0, 1, 10)));

        var result = await _controller.GetPurchasesByIDAsync(sieveModel, buyerId, CancellationToken.None)
            .ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<GetOutputsByUserIdQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_083 - GetOutputs - L?y danh s�ch don h�ng")]
    public async Task GetOutputs_WithSieveModel_ReturnsOrdersList()
    {
        var sieveModel = new SieveModel { Page = 1, PageSize = 10 };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetOutputsListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result<PagedResult<OutputItemResponse>>.Success(new PagedResult<OutputItemResponse>([], 0, 1, 10)));

        var result = await _controller.GetOutputsAsync(sieveModel, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetOutputsListQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_084 - GetDeletedOutputs - L?y danh s�ch don h�ng d� x�a")]
    public async Task GetDeletedOutputs_WithSieveModel_ReturnsDeletedOrders()
    {
        var sieveModel = new SieveModel();
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDeletedOutputsListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result<PagedResult<OutputItemResponse>>.Success(new PagedResult<OutputItemResponse>([], 0, 1, 10)));

        var result = await _controller.GetDeletedOutputsAsync(sieveModel, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<GetDeletedOutputsListQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_085 - GetOutputById - L?y chi ti?t don h�ng")]
    public async Task GetOutputById_ValidId_ReturnsOrderDetail()
    {
        int orderId = 1;
        var expectedOrder = new OrderDetailResponse { Id = orderId };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetOutputByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderDetailResponse>.Success(expectedOrder));

        var result = await _controller.GetOutputByIdAsync(orderId, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetOutputByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_086 - CreateOutput - T?o don hng (user thu?ng)")]
    public async Task CreateOutput_ValidRequest_CreatesOrder()
    {
        var request = new CreateOutputCommand { BuyerId = Guid.NewGuid(), Notes = "Test order" };
        var expectedResponse = new OrderDetailResponse { Id = 1, BuyerId = request.BuyerId };
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateOutputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderDetailResponse>.Success(expectedResponse));

        var result = await _controller.CreateOutputAsync(request, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateOutputCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_087 - CreateOutputForAdmin - T?o don hng (manager)")]
    public async Task CreateOutputForAdmin_WithManagerPermission_CreatesOrder()
    {
        var managerId = Guid.NewGuid();
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, managerId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _controller.ControllerContext.HttpContext.User = claimsPrincipal;

        var request = new CreateOutputByManagerCommand { BuyerId = Guid.NewGuid(), Notes = "Admin order" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateOutputByManagerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderDetailResponse>.Success(new OrderDetailResponse()));

        var result = await _controller.CreateOutputForAdminAsync(request, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(
            m => m.Send(
                It.Is<CreateOutputByManagerCommand>(c => c.CurrentUserId == managerId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_088 - UpdateOutputForManager - S?a don hng (do chnh mnh t?o)")]
    public async Task UpdateOutputForManager_OwnOrder_UpdatesOrder()
    {
        int orderId = 1;
        var request = new UpdateOutputCommand { Notes = "Updated notes" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateOutputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderDetailResponse>.Success(new OrderDetailResponse { Id = orderId }));

        var result = await _controller.UpdateOutputForManagerAsync(orderId, request, CancellationToken.None)
            .ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateOutputCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_089 - UpdateOutput - S?a don hng (manager)")]
    public async Task UpdateOutput_WithManagerPermission_UpdatesOrder()
    {
        var managerId = Guid.NewGuid();
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, managerId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _controller.ControllerContext.HttpContext.User = claimsPrincipal;

        int orderId = 1;
        var request = new UpdateOutputForManagerCommand();
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateOutputForManagerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderDetailResponse>.Success(new OrderDetailResponse()));

        var result = await _controller.UpdateOutputAsync(orderId, request, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(
            m => m.Send(
                It.Is<UpdateOutputForManagerCommand>(c => c.CurrentUserId == managerId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_090 - UpdateOutputStatus - C?p nh?t tr?ng thi don hng")]
    public async Task UpdateOutputStatus_ValidTransition_UpdatesStatus()
    {
        int orderId = 1;
        var request = new UpdateOutputStatusCommand { StatusId = "confirmed_cod" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateOutputStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderDetailResponse>.Success(new OrderDetailResponse { StatusId = "confirmed_cod" }));

        var result = await _controller.UpdateOutputStatusAsync(orderId, request, CancellationToken.None)
            .ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<UpdateOutputStatusCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_091 - UpdateManyOutputStatus - C?p nh?t tr?ng thi nhi?u don")]
    public async Task UpdateManyOutputStatus_ValidRequest_UpdatesMultipleOrders()
    {
        var request = new UpdateManyOutputStatusCommand { Ids = [ 1, 2, 3 ], StatusId = "confirmed_cod" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateManyOutputStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<OutputItemResponse>?>.Success([]));

        var result = await _controller.UpdateManyOutputStatusAsync(request, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<UpdateManyOutputStatusCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_092 - DeleteOutput - Xóa đơn hàng")]
    public async Task DeleteOutput_ValidId_DeletesOrder()
    {
        int orderId = 1;
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteOutputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _controller.DeleteOutputAsync(orderId, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<DeleteOutputCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_110 - GetLockedStatuses - Lấy danh sách trạng thái bị khóa")]
    public void GetLockedStatuses_ReturnsValidList()
    {
        var result = _controller.GetLockedStatuses() as OkObjectResult;

        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        var value = result.Value as HashSet<string>;
        value.Should().NotBeNull();
        value.Should().Contain(Domain.Constants.Order.OrderStatus.Completed);
    }

    [Fact(DisplayName = "SO_093 - DeleteManyOutputs - Xa nhi?u don hng")]
    public async Task DeleteManyOutputs_ValidRequest_DeletesMultipleOrders()
    {
        var request = new DeleteManyOutputsCommand { Ids = [ 1, 2, 3 ] };
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteManyOutputsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _controller.DeleteManyOutputsAsync(request, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<DeleteManyOutputsCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_094 - RestoreOutput - Khi ph?c don hng")]
    public async Task RestoreOutput_DeletedOrder_RestoresOrder()
    {
        int orderId = 1;
        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreOutputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderDetailResponse>.Success(new OrderDetailResponse { Id = orderId }));

        var result = await _controller.RestoreOutputAsync(orderId, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<RestoreOutputCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_095 - RestoreManyOutputs - Kh�i ph?c nhi?u don h�ng")]
    public async Task RestoreManyOutputs_DeletedOrders_RestoresMultipleOrders()
    {
        var request = new RestoreManyOutputsCommand { Ids = [ 1, 2, 3 ] };
        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreManyOutputsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<OutputItemResponse>?>.Success([ new(), new(), new() ]));

        var result = await _controller.RestoreManyOutputsAsync(request, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<RestoreManyOutputsCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_096 - Controller x? l� UnauthorizedAccessException")]
    public async Task CreateOutput_UnauthorizedAccess_ThrowsUnauthorizedException()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateOutputCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.CreateOutputAsync(new CreateOutputCommand(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "SO_097 - Controller x? l� NotFoundException")]
    public async Task GetOutputById_NotFound_ThrowsKeyNotFoundException()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetOutputByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _controller.GetOutputByIdAsync(999, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "SO_098 - Controller x? l� ValidationException")]
    public async Task CreateOutput_InvalidData_ThrowsInvalidOperationException()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateOutputCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Validation failed"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.CreateOutputAsync(new CreateOutputCommand(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "SO_102 - L?y danh s�ch tr?ng th�i don h�ng khi thi?u quy?n tr? 403")]
    public async Task GetOutputStatuses_MissingPermission_ThrowsUnauthorized()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetOutputStatusListQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not have permission Permissions.Outputs.View"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetOutputStatusesAsync(CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "SO_103 - Controller g?i MediatR d�ng 1 l?n khi l?y danh s�ch tr?ng th�i don h�ng")]
    public async Task GetOutputStatuses_ValidRequest_CallsMediatorOnce()
    {
        var expectedStatuses = new Dictionary<string, string>
        { { Domain.Constants.Order.OrderStatus.Pending, "Ch? x�c nh?n" } };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetOutputStatusListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Dictionary<string, string>>.Success(expectedStatuses));

        var result = await _controller.GetOutputStatusesAsync(CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<OkObjectResult>();
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<GetOutputStatusListQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_104 - Controller tr? d�ng d? li?u t? Handler khi l?y tr?ng th�i don h�ng")]
    public async Task GetOutputStatuses_ValidRequest_ReturnsExpectedData()
    {
        var expectedStatuses = new Dictionary<string, string>
        { { Domain.Constants.Order.OrderStatus.Pending, "Ch? x�c nh?n" } };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetOutputStatusListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Dictionary<string, string>>.Success(expectedStatuses));

        var result = await _controller.GetOutputStatusesAsync(CancellationToken.None).ConfigureAwait(true);

        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(expectedStatuses);
    }
#pragma warning restore CRR0035
#pragma warning restore IDE0079
}
