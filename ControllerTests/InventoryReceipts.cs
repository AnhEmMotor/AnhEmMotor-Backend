using Application.ApiContracts.Input.Requests;
using Application.ApiContracts.Input.Responses;
using Application.Common.Models;
using Application.Features.Inputs.Commands.CloneInput;
using Application.Features.Inputs.Commands.CreateInput;
using Application.Features.Inputs.Commands.DeleteInput;
using Application.Features.Inputs.Commands.DeleteManyInputs;
using Application.Features.Inputs.Commands.RestoreInput;
using Application.Features.Inputs.Commands.RestoreManyInputs;
using Application.Features.Inputs.Commands.UpdateInput;
using Application.Features.Inputs.Commands.UpdateInputStatus;
using Application.Features.Inputs.Commands.UpdateManyInputStatus;
using Application.Features.Inputs.Queries.GetDeletedInputsList;
using Application.Features.Inputs.Queries.GetInputById;
using Application.Features.Inputs.Queries.GetInputsBySupplierId;
using Application.Features.Inputs.Queries.GetInputsList;
using Domain.Primitives;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sieve.Models;
using WebAPI.Controllers.V1;
using Xunit;

namespace ControllerTests;

public class InventoryReceipts
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly InventoryReceiptsController _controller;

    public InventoryReceipts()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new InventoryReceiptsController(_mediatorMock.Object);

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

    [Fact(DisplayName = "INPUT_003 - Tạo phiếu nhập thiếu quyền Create")]
    public async Task CreateInput_MissingPermission_ReturnsForbidden()
    {
        // Arrange
        var request = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products =
            [
                new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 }
            ]
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateInputCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not have permission Permissions.Inputs.Create"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _controller.CreateInput(request, CancellationToken.None));
    }

    [Fact(DisplayName = "INPUT_019 - Lấy danh sách phiếu nhập thiếu quyền View")]
    public async Task GetInputs_MissingPermission_ReturnsForbidden()
    {
        // Arrange
        var sieveModel = new SieveModel { Page = 1, PageSize = 10 };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetInputsListQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not have permission Permissions.Inputs.View"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _controller.GetInputs(sieveModel, CancellationToken.None));
    }

    [Fact(DisplayName = "INPUT_022 - Lấy chi tiết phiếu nhập thiếu quyền View")]
    public async Task GetInputById_MissingPermission_ReturnsForbidden()
    {
        // Arrange
        int inputId = 1;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetInputByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not have permission Permissions.Inputs.View"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _controller.GetInputById(inputId, CancellationToken.None));
    }

    [Fact(DisplayName = "INPUT_026 - Cập nhật phiếu nhập thiếu quyền Edit")]
    public async Task UpdateInput_MissingPermission_ReturnsForbidden()
    {
        // Arrange
        int inputId = 1;
        var request = new UpdateInputCommand
        {
            Notes = "Updated",
            SupplierId = 2,
            Products = []
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateInputCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not have permission Permissions.Inputs.Edit"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _controller.UpdateInput(inputId, request, CancellationToken.None));
    }

    [Fact(DisplayName = "INPUT_027 - Cập nhật phiếu nhập không tồn tại")]
    public async Task UpdateInput_NotFound_ReturnsNotFound()
    {
        // Arrange
        int inputId = 9999;
        var request = new UpdateInputCommand
        {
            Notes = "Updated",
            SupplierId = 2,
            Products = []
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateInputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InputResponse?>.Failure(Error.NotFound("Input not found")));

        // Act
        var result = await _controller.UpdateInput(inputId, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "INPUT_031 - Cập nhật trạng thái phiếu nhập thiếu quyền ChangeStatus")]
    public async Task UpdateInputStatus_MissingPermission_ReturnsForbidden()
    {
        // Arrange
        int inputId = 1;
        var request = new UpdateInputStatusCommand { StatusId = "finished" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateInputStatusCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not have permission Permissions.Inputs.ChangeStatus"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _controller.UpdateInputStatus(inputId, request, CancellationToken.None));
    }

    [Fact(DisplayName = "INPUT_035 - Xóa phiếu nhập ở trạng thái finished (không cho phép)")]
    public async Task DeleteInput_FinishedStatus_ReturnsBadRequest()
    {
        // Arrange
        int inputId = 1;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteInputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(Error.BadRequest("Cannot delete finished input")));

        // Act
        var result = await _controller.DeleteInput(inputId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "INPUT_036 - Xóa phiếu nhập thiếu quyền Delete")]
    public async Task DeleteInput_MissingPermission_ReturnsForbidden()
    {
        // Arrange
        int inputId = 1;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteInputCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not have permission Permissions.Inputs.Delete"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _controller.DeleteInput(inputId, CancellationToken.None));
    }

    [Fact(DisplayName = "INPUT_037 - Xóa phiếu nhập không tồn tại")]
    public async Task DeleteInput_NotFound_ReturnsNotFound()
    {
        // Arrange
        int inputId = 9999;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteInputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(Error.NotFound("Input not found")));

        // Act
        var result = await _controller.DeleteInput(inputId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "INPUT_040 - Khôi phục phiếu nhập chưa bị xóa")]
    public async Task RestoreInput_NotDeleted_ReturnsBadRequest()
    {
        // Arrange
        int inputId = 1;

        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreInputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InputResponse>.Failure(Error.BadRequest("Input is not deleted")));

        // Act
        var result = await _controller.RestoreInput(inputId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "INPUT_044 - Clone phiếu nhập thiếu quyền Create")]
    public async Task CloneInput_MissingPermission_ReturnsForbidden()
    {
        // Arrange
        int inputId = 1;

        _mediatorMock.Setup(m => m.Send(It.IsAny<CloneInputCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not have permission Permissions.Inputs.Create"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _controller.CloneInput(inputId, CancellationToken.None));
    }

    [Fact(DisplayName = "INPUT_045 - Clone phiếu nhập không tồn tại")]
    public async Task CloneInput_NotFound_ReturnsNotFound()
    {
        // Arrange
        int inputId = 9999;

        _mediatorMock.Setup(m => m.Send(It.IsAny<CloneInputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InputResponse?>.Failure(Error.NotFound("Input not found")));

        // Act
        var result = await _controller.CloneInput(inputId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "INPUT_C_001 - Xóa nhiều phiếu nhập với danh sách rỗng")]
    public async Task DeleteManyInputs_EmptyList_ReturnsBadRequest()
    {
        // Arrange
        var request = new DeleteManyInputsCommand { Ids = [] };

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteManyInputsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("InputIds cannot be empty"));

        // Act & Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            _controller.DeleteManyInputs(request, CancellationToken.None));
    }

    [Fact(DisplayName = "INPUT_C_002 - Cập nhật trạng thái nhiều phiếu nhập với danh sách rỗng")]
    public async Task UpdateManyInputStatus_EmptyList_ReturnsBadRequest()
    {
        // Arrange
        var request = new UpdateManyInputStatusCommand { Ids = [], StatusId = "finished" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateManyInputStatusCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("InputIds cannot be empty"));

        // Act & Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            _controller.UpdateManyInputStatus(request, CancellationToken.None));
    }

    [Fact(DisplayName = "INPUT_C_003 - Khôi phục nhiều phiếu nhập với danh sách rỗng")]
    public async Task RestoreManyInputs_EmptyList_ReturnsBadRequest()
    {
        // Arrange
        var request = new RestoreManyInputsCommand { Ids = [] };

        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreManyInputsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("InputIds cannot be empty"));

        // Act & Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            _controller.RestoreManyInputs(request, CancellationToken.None));
    }

    [Fact(DisplayName = "INPUT_C_004 - Lấy danh sách phiếu nhập đã xóa với phân trang")]
    public async Task GetDeletedInputs_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var sieveModel = new SieveModel { Page = 1, PageSize = 10 };
        var expectedResponse = new PagedResult<InputResponse>([], 0, 1, 10);

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDeletedInputsListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<InputResponse>>.Success(expectedResponse));

        // Act
        var result = await _controller.GetDeletedInputs(sieveModel, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact(DisplayName = "INPUT_C_005 - Lấy danh sách phiếu nhập theo SupplierId hợp lệ")]
    public async Task GetInputsBySupplierId_ValidSupplierId_ReturnsSuccess()
    {
        // Arrange
        int supplierId = 1;
        var sieveModel = new SieveModel { Page = 1, PageSize = 10 };
        var expectedResponse = new PagedResult<InputResponse>([], 0, 1, 10);

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetInputsBySupplierIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<InputResponse>>.Success(expectedResponse));

        // Act
        var result = await _controller.GetInputsBySupplierId(supplierId, sieveModel, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact(DisplayName = "INPUT_C_006 - Tạo phiếu nhập với request hợp lệ")]
    public async Task CreateInput_ValidRequest_CallsMediator()
    {
        // Arrange
        var request = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products =
            [
                new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 }
            ]
        };

        var expectedResponse = new InputResponse { Id = 1, StatusId = "working" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateInputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InputResponse?>.Success(expectedResponse));

        // Act
        var result = await _controller.CreateInput(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateInputCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "INPUT_C_007 - Cập nhật phiếu nhập với request hợp lệ")]
    public async Task UpdateInput_ValidRequest_CallsMediator()
    {
        // Arrange
        int inputId = 1;
        var request = new UpdateInputCommand
        {
            Notes = "Updated",
            SupplierId = 2,
            Products = []
        };

        var expectedResponse = new InputResponse { Id = inputId, Notes = "Updated" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateInputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InputResponse?>.Success(expectedResponse));

        // Act
        var result = await _controller.UpdateInput(inputId, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateInputCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "INPUT_C_008 - Cập nhật trạng thái phiếu nhập hợp lệ")]
    public async Task UpdateInputStatus_ValidRequest_CallsMediator()
    {
        // Arrange
        int inputId = 1;
        var request = new UpdateInputStatusCommand { StatusId = "finished" };

        var expectedResponse = new InputResponse { Id = inputId, StatusId = "finished" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateInputStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InputResponse>.Success(expectedResponse));

        // Act
        var result = await _controller.UpdateInputStatus(inputId, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateInputStatusCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
