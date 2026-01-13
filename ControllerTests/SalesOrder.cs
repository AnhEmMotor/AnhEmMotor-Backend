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
using Application.Features.Outputs.Queries.GetOutputsByUserIdByManager;
using Application.Features.Outputs.Queries.GetOutputsList;

using Domain.Primitives;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sieve.Models;
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

#pragma warning disable CRR0035
    [Fact(DisplayName = "SO_081 - GetMyPurchases - Lấy đơn hàng của chính mình")]
    public async Task GetMyPurchases_UserAuthenticated_ReturnsOrders()
    {
        var buyerId = Guid.NewGuid();
        var sieveModel = new SieveModel();
        var expectedOrder = new OutputResponse { Id = 1, BuyerId = buyerId };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetOutputsByUserIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result<PagedResult<OutputResponse>>.Success(
                    new PagedResult<OutputResponse>([ expectedOrder ], 1, 1, 10)));

        var result = await _controller.GetMyPurchasesAsync(sieveModel, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<GetOutputsByUserIdQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_082 - GetPurchasesByID - Lấy đơn hàng theo BuyerId (có quyền)")]
    public async Task GetPurchasesByID_WithPermission_ReturnsOrders()
    {
        var buyerId = Guid.NewGuid();
        var sieveModel = new SieveModel();
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetOutputsByUserIdByManagerQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<OutputResponse>>.Success(new PagedResult<OutputResponse>([], 0, 1, 10)));

        var result = await _controller.GetPurchasesByIDAsync(sieveModel, buyerId, CancellationToken.None)
            .ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<GetOutputsByUserIdByManagerQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_083 - GetOutputs - Lấy danh sách đơn hàng")]
    public async Task GetOutputs_WithSieveModel_ReturnsOrdersList()
    {
        var sieveModel = new SieveModel { Page = 1, PageSize = 10 };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetOutputsListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<OutputResponse>>.Success(new PagedResult<OutputResponse>([], 0, 1, 10)));

        var result = await _controller.GetOutputsAsync(sieveModel, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetOutputsListQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_084 - GetDeletedOutputs - Lấy danh sách đơn hàng đã xóa")]
    public async Task GetDeletedOutputs_WithSieveModel_ReturnsDeletedOrders()
    {
        var sieveModel = new SieveModel();
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDeletedOutputsListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<OutputResponse>>.Success(new PagedResult<OutputResponse>([], 0, 1, 10)));

        var result = await _controller.GetDeletedOutputsAsync(sieveModel, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<GetDeletedOutputsListQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_085 - GetOutputById - Lấy chi tiết đơn hàng")]
    public async Task GetOutputById_ValidId_ReturnsOrderDetail()
    {
        int orderId = 1;
        var expectedOrder = new OutputResponse { Id = orderId };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetOutputByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OutputResponse?>.Success(expectedOrder));

        var result = await _controller.GetOutputByIdAsync(orderId, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetOutputByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_086 - CreateOutput - Tạo đơn hàng (user thường)")]
    public async Task CreateOutput_ValidRequest_CreatesOrder()
    {
        var request = new CreateOutputCommand { BuyerId = Guid.NewGuid(), Notes = "Test order" };
        var expectedResponse = new OutputResponse { Id = 1, BuyerId = request.BuyerId };
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateOutputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OutputResponse?>.Success(expectedResponse));

        var result = await _controller.CreateOutputAsync(request, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateOutputCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_087 - CreateOutputForAdmin - Tạo đơn hàng (manager)")]
    public async Task CreateOutputForAdmin_WithManagerPermission_CreatesOrder()
    {
        var request = new CreateOutputByManagerCommand { BuyerId = Guid.NewGuid(), Notes = "Admin order" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateOutputByManagerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OutputResponse?>.Success(new OutputResponse()));

        var result = await _controller.CreateOutputForAdminAsync(request, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<CreateOutputByManagerCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_088 - UpdateOutputForManager - Sửa đơn hàng (do chính mình tạo)")]
    public async Task UpdateOutputForManager_OwnOrder_UpdatesOrder()
    {
        int orderId = 1;
        var request = new UpdateOutputCommand { Notes = "Updated notes" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateOutputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OutputResponse?>.Success(new OutputResponse { Id = orderId }));

        var result = await _controller.UpdateOutputForManagerAsync(orderId, request, CancellationToken.None)
            .ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateOutputCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_089 - UpdateOutput - Sửa đơn hàng (manager)")]
    public async Task UpdateOutput_WithManagerPermission_UpdatesOrder()
    {
        int orderId = 1;
        var request = new UpdateOutputForManagerCommand();
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateOutputForManagerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OutputResponse?>.Success(new OutputResponse()));

        var result = await _controller.UpdateOutputAsync(orderId, request, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<UpdateOutputForManagerCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_090 - UpdateOutputStatus - Cập nhật trạng thái đơn hàng")]
    public async Task UpdateOutputStatus_ValidTransition_UpdatesStatus()
    {
        int orderId = 1;
        var request = new UpdateOutputStatusCommand { StatusId = "confirmed_cod" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateOutputStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OutputResponse?>.Success(new OutputResponse { StatusId = "confirmed_cod" }));

        var result = await _controller.UpdateOutputStatusAsync(orderId, request, CancellationToken.None)
            .ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<UpdateOutputStatusCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_091 - UpdateManyOutputStatus - Cập nhật trạng thái nhiều đơn")]
    public async Task UpdateManyOutputStatus_ValidRequest_UpdatesMultipleOrders()
    {
        var request = new UpdateManyOutputStatusCommand { Ids = [ 1, 2, 3 ], StatusId = "confirmed_cod" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateManyOutputStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<OutputResponse>?>.Success([]));

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

    [Fact(DisplayName = "SO_093 - DeleteManyOutputs - Xóa nhiều đơn hàng")]
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

    [Fact(DisplayName = "SO_094 - RestoreOutput - Khôi phục đơn hàng")]
    public async Task RestoreOutput_DeletedOrder_RestoresOrder()
    {
        int orderId = 1;
        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreOutputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OutputResponse?>.Success(new OutputResponse { Id = orderId }));

        var result = await _controller.RestoreOutputAsync(orderId, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<RestoreOutputCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_095 - RestoreManyOutputs - Khôi phục nhiều đơn hàng")]
    public async Task RestoreManyOutputs_DeletedOrders_RestoresMultipleOrders()
    {
        var request = new RestoreManyOutputsCommand { Ids = [ 1, 2, 3 ] };
        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreManyOutputsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<OutputResponse>?>.Success([ new(), new(), new() ]));

        var result = await _controller.RestoreManyOutputsAsync(request, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<RestoreManyOutputsCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "SO_096 - Controller xử lý UnauthorizedAccessException")]
    public async Task CreateOutput_UnauthorizedAccess_ThrowsUnauthorizedException()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateOutputCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.CreateOutputAsync(new CreateOutputCommand(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "SO_097 - Controller xử lý NotFoundException")]
    public async Task GetOutputById_NotFound_ThrowsKeyNotFoundException()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetOutputByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _controller.GetOutputByIdAsync(999, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "SO_098 - Controller xử lý ValidationException")]
    public async Task CreateOutput_InvalidData_ThrowsInvalidOperationException()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateOutputCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Validation failed"));

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.CreateOutputAsync(new CreateOutputCommand(), CancellationToken.None))
            .ConfigureAwait(true);
    }
#pragma warning restore CRR0035
}
