using System.Reflection;
using Application.ApiContracts.Output.Requests;
using Application.ApiContracts.Output.Responses;
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
using Domain.Common.Models;
using Domain.Primitives;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sieve.Models;
using WebAPI.Controllers.V1;
using Xunit;

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
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

    [Fact(DisplayName = "SO_081 - GetMyPurchases - Lấy đơn hàng của chính mình")]
    public async Task GetMyPurchases_UserAuthenticated_ReturnsOrders()
    {
        // Arrange
        var buyerId = Guid.NewGuid();
        var sieveModel = new SieveModel();
        var expectedOrder = new OutputResponse { Id = 1, BuyerId = buyerId };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetOutputsByUserIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<OutputResponse>([expectedOrder], 1, 1, 10));

        // Act
        var result = await _controller.GetMyPurchases(sieveModel, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetOutputsByUserIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_082 - GetPurchasesByID - Lấy đơn hàng theo BuyerId (có quyền)")]
    public async Task GetPurchasesByID_WithPermission_ReturnsOrders()
    {
        // Arrange
        var buyerId = Guid.NewGuid();
        var sieveModel = new SieveModel();
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetOutputsByUserIdByManagerQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<OutputResponse>([], 0, 1, 10));

        // Act
        var result = await _controller.GetPurchasesByID(sieveModel, buyerId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetOutputsByUserIdByManagerQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_083 - GetOutputs - Lấy danh sách đơn hàng")]
    public async Task GetOutputs_WithSieveModel_ReturnsOrdersList()
    {
        // Arrange
        var sieveModel = new SieveModel { Page = 1, PageSize = 10 };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetOutputsListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<OutputResponse>([], 0, 1, 10));

        // Act
        var result = await _controller.GetOutputs(sieveModel, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetOutputsListQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_084 - GetDeletedOutputs - Lấy danh sách đơn hàng đã xóa")]
    public async Task GetDeletedOutputs_WithSieveModel_ReturnsDeletedOrders()
    {
        // Arrange
        var sieveModel = new SieveModel();
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDeletedOutputsListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<OutputResponse>([], 0, 1, 10));

        // Act
        var result = await _controller.GetDeletedOutputs(sieveModel, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetDeletedOutputsListQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_085 - GetOutputById - Lấy chi tiết đơn hàng")]
    public async Task GetOutputById_ValidId_ReturnsOrderDetail()
    {
        // Arrange
        int orderId = 1;
        var expectedOrder = new OutputResponse { Id = orderId };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetOutputByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((expectedOrder, (Application.Common.Models.ErrorResponse?)null));

        // Act
        var result = await _controller.GetOutputById(orderId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetOutputByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_086 - CreateOutput - Tạo đơn hàng (user thường)")]
    public async Task CreateOutput_ValidRequest_CreatesOrder()
    {
        // Arrange
        var request = new CreateOutputRequest
        {
            BuyerId = Guid.NewGuid(),
            Notes = "Test order"
        };
        var expectedResponse = new OutputResponse { Id = 1, BuyerId = request.BuyerId };
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateOutputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((expectedResponse, (Application.Common.Models.ErrorResponse?)null));

        // Act
        var result = await _controller.CreateOutput(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateOutputCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_087 - CreateOutputForAdmin - Tạo đơn hàng (manager)")]
    public async Task CreateOutputForAdmin_WithManagerPermission_CreatesOrder()
    {
        // Arrange
        var request = new CreateOutputByAdminRequest
        {
            BuyerId = Guid.NewGuid(),
            Notes = "Admin order"
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateOutputByManagerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new OutputResponse(), (Application.Common.Models.ErrorResponse?)null));

        // Act
        var result = await _controller.CreateOutputForAdmin(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateOutputByManagerCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_088 - UpdateOutputForManager - Sửa đơn hàng (do chính mình tạo)")]
    public async Task UpdateOutputForManager_OwnOrder_UpdatesOrder()
    {
        // Arrange
        int orderId = 1;
        var request = new UpdateOutputForManagerRequest
        {
            Notes = "Updated notes"
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateOutputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new OutputResponse { Id = orderId }, (Application.Common.Models.ErrorResponse?)null));

        // Act
        var result = await _controller.UpdateOutputForManager(orderId, request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateOutputCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_089 - UpdateOutput - Sửa đơn hàng (manager)")]
    public async Task UpdateOutput_WithManagerPermission_UpdatesOrder()
    {
        // Arrange
        int orderId = 1;
        var request = new UpdateOutputForManagerRequest();
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateOutputForManagerCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new OutputResponse(), (Application.Common.Models.ErrorResponse?)null));

        // Act
        var result = await _controller.UpdateOutput(orderId, request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateOutputForManagerCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_090 - UpdateOutputStatus - Cập nhật trạng thái đơn hàng")]
    public async Task UpdateOutputStatus_ValidTransition_UpdatesStatus()
    {
        // Arrange
        int orderId = 1;
        var request = new UpdateOutputStatusRequest { StatusId = "confirmed_cod" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateOutputStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new OutputResponse { StatusId = "confirmed_cod" }, (Application.Common.Models.ErrorResponse?)null));

        // Act
        var result = await _controller.UpdateOutputStatus(orderId, request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateOutputStatusCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_091 - UpdateManyOutputStatus - Cập nhật trạng thái nhiều đơn")]
    public async Task UpdateManyOutputStatus_ValidRequest_UpdatesMultipleOrders()
    {
        // Arrange
        var request = new UpdateManyOutputStatusRequest
        {
            Ids = [1, 2, 3],
            StatusId = "confirmed_cod"
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateManyOutputStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(([], null));

        // Act
        var result = await _controller.UpdateManyOutputStatus(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateManyOutputStatusCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_092 - DeleteOutput - Xóa đơn hàng")]
    public async Task DeleteOutput_ValidId_DeletesOrder()
    {
        // Arrange
        int orderId = 1;
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteOutputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Application.Common.Models.ErrorResponse?)null);

        // Act
        var result = await _controller.DeleteOutput(orderId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<DeleteOutputCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_093 - DeleteManyOutputs - Xóa nhiều đơn hàng")]
    public async Task DeleteManyOutputs_ValidRequest_DeletesMultipleOrders()
    {
        // Arrange
        var request = new DeleteManyOutputsRequest { Ids = [1, 2, 3] };
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteManyOutputsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Application.Common.Models.ErrorResponse?)null);

        // Act
        var result = await _controller.DeleteManyOutputs(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<DeleteManyOutputsCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_094 - RestoreOutput - Khôi phục đơn hàng")]
    public async Task RestoreOutput_DeletedOrder_RestoresOrder()
    {
        // Arrange
        int orderId = 1;
        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreOutputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((new OutputResponse { Id = orderId }, (Application.Common.Models.ErrorResponse?)null));

        // Act
        var result = await _controller.RestoreOutput(orderId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<RestoreOutputCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_095 - RestoreManyOutputs - Khôi phục nhiều đơn hàng")]
    public async Task RestoreManyOutputs_DeletedOrders_RestoresMultipleOrders()
    {
        // Arrange
        var request = new RestoreManyOutputsRequest { Ids = [1, 2, 3] };
        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreManyOutputsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(([new(), new(), new()], null));

        // Act
        var result = await _controller.RestoreManyOutputs(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mediatorMock.Verify(m => m.Send(It.IsAny<RestoreManyOutputsCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SO_096 - Controller xử lý UnauthorizedAccessException")]
    public async Task CreateOutput_UnauthorizedAccess_ThrowsUnauthorizedException()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateOutputCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => 
            _controller.CreateOutput(new CreateOutputRequest(), CancellationToken.None));
    }

    [Fact(DisplayName = "SO_097 - Controller xử lý NotFoundException")]
    public async Task GetOutputById_NotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetOutputByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => 
            _controller.GetOutputById(999, CancellationToken.None));
    }

    [Fact(DisplayName = "SO_098 - Controller xử lý ValidationException")]
    public async Task CreateOutput_InvalidData_ThrowsInvalidOperationException()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateOutputCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Validation failed"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _controller.CreateOutput(new CreateOutputRequest(), CancellationToken.None));
    }
}
