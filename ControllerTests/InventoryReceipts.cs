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
using Application.Features.Inputs.Queries.GetInputStatusList;
using Domain.Constants.Input;
using Domain.Primitives;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sieve.Models;
using WebAPI.Controllers.V1;

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
        _controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };
    }

    #pragma warning disable IDE0079 
    #pragma warning disable CRR0035
    [Fact(DisplayName = "INPUT_003 - Tạo phiếu nhập thiếu quyền Create")]
    public async Task CreateInput_MissingPermission_ReturnsForbidden()
    {
        var request = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products = [new CreateInputInfoRequest { ProductVariantId = 1, Count = 10, InputPrice = 100000 }]
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateInputCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new UnauthorizedAccessException(
                    "User does not have permission Domain.Constants.Permission.Permissions.Inputs.Create"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.CreateInputAsync(request, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "INPUT_019 - Lấy danh sách phiếu nhập thiếu quyền View")]
    public async Task GetInputs_MissingPermission_ReturnsForbidden()
    {
        var sieveModel = new SieveModel { Page = 1, PageSize = 10 };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetInputsListQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new UnauthorizedAccessException(
                    "User does not have permission Domain.Constants.Permission.Permissions.Inputs.View"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetInputsAsync(sieveModel, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "INPUT_022 - Lấy chi tiết phiếu nhập thiếu quyền View")]
    public async Task GetInputById_MissingPermission_ReturnsForbidden()
    {
        int inputId = 1;
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetInputByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new UnauthorizedAccessException(
                    "User does not have permission Domain.Constants.Permission.Permissions.Inputs.View"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetInputByIdAsync(inputId, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "INPUT_026 - Cập nhật phiếu nhập thiếu quyền Edit")]
    public async Task UpdateInput_MissingPermission_ReturnsForbidden()
    {
        int inputId = 1;
        var request = new UpdateInputCommand { Notes = "Updated", SupplierId = 2, Products = [] };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateInputCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new UnauthorizedAccessException(
                    "User does not have permission Domain.Constants.Permission.Permissions.Inputs.Edit"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.UpdateInputAsync(inputId, request, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "INPUT_027 - Cập nhật phiếu nhập không tồn tại")]
    public async Task UpdateInput_NotFound_ReturnsNotFound()
    {
        int inputId = 9999;
        var request = new UpdateInputCommand { Notes = "Updated", SupplierId = 2, Products = [] };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateInputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InputDetailResponse?>.Failure(Error.NotFound("Input not found")));
        var result = await _controller.UpdateInputAsync(inputId, request, CancellationToken.None).ConfigureAwait(true);
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "INPUT_031 - Cập nhật trạng thái phiếu nhập thiếu quyền ChangeStatus")]
    public async Task UpdateInputStatus_MissingPermission_ReturnsForbidden()
    {
        int inputId = 1;
        var request = new UpdateInputStatusCommand { StatusId = "finished" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateInputStatusCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new UnauthorizedAccessException(
                    "User does not have permission Domain.Constants.Permission.Permissions.Inputs.ChangeStatus"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.UpdateInputStatusAsync(inputId, request, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "INPUT_035 - Xóa phiếu nhập ở trạng thái finished (không cho phép)")]
    public async Task DeleteInput_FinishedStatus_ReturnsBadRequest()
    {
        int inputId = 1;
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteInputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(Error.BadRequest("Cannot delete finished input")));
        var result = await _controller.DeleteInputAsync(inputId, CancellationToken.None).ConfigureAwait(true);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "INPUT_036 - Xóa phiếu nhập thiếu quyền Delete")]
    public async Task DeleteInput_MissingPermission_ReturnsForbidden()
    {
        int inputId = 1;
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteInputCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new UnauthorizedAccessException(
                    "User does not have permission Domain.Constants.Permission.Permissions.Inputs.Delete"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.DeleteInputAsync(inputId, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "INPUT_037 - Xóa phiếu nhập không tồn tại")]
    public async Task DeleteInput_NotFound_ReturnsNotFound()
    {
        int inputId = 9999;
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteInputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(Error.NotFound("Input not found")));
        var result = await _controller.DeleteInputAsync(inputId, CancellationToken.None).ConfigureAwait(true);
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "INPUT_040 - Khôi phục phiếu nhập chưa bị xóa")]
    public async Task RestoreInput_NotDeleted_ReturnsBadRequest()
    {
        int inputId = 1;
        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreInputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InputDetailResponse>.Failure(Error.BadRequest("Input is not deleted")));
        var result = await _controller.RestoreInputAsync(inputId, CancellationToken.None).ConfigureAwait(true);
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "INPUT_044 - Clone phiếu nhập thiếu quyền Create")]
    public async Task CloneInput_MissingPermission_ReturnsForbidden()
    {
        int inputId = 1;
        _mediatorMock.Setup(m => m.Send(It.IsAny<CloneInputCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new UnauthorizedAccessException(
                    "User does not have permission Domain.Constants.Permission.Permissions.Inputs.Create"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.CloneInputAsync(inputId, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "INPUT_045 - Clone phiếu nhập không tồn tại")]
    public async Task CloneInput_NotFound_ReturnsNotFound()
    {
        int inputId = 9999;
        _mediatorMock.Setup(m => m.Send(It.IsAny<CloneInputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InputDetailResponse?>.Failure(Error.NotFound("Input not found")));
        var result = await _controller.CloneInputAsync(inputId, CancellationToken.None).ConfigureAwait(true);
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "INPUT_C_001 - Xóa nhiều phiếu nhập với danh sách rỗng")]
    public async Task DeleteManyInputs_EmptyList_ReturnsBadRequest()
    {
        var request = new DeleteManyInputsCommand { Ids = [] };
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteManyInputsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("InputIds cannot be empty"));
        await Assert.ThrowsAsync<ValidationException>(
            () => _controller.DeleteManyInputsAsync(request, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "INPUT_C_002 - Cập nhật trạng thái nhiều phiếu nhập với danh sách rỗng")]
    public async Task UpdateManyInputStatus_EmptyList_ReturnsBadRequest()
    {
        var request = new UpdateManyInputStatusCommand { Ids = [], StatusId = "finished" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateManyInputStatusCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("InputIds cannot be empty"));
        await Assert.ThrowsAsync<ValidationException>(
            () => _controller.UpdateManyInputStatusAsync(request, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "INPUT_C_003 - Khôi phục nhiều phiếu nhập với danh sách rỗng")]
    public async Task RestoreManyInputs_EmptyList_ReturnsBadRequest()
    {
        var request = new RestoreManyInputsCommand { Ids = [] };
        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreManyInputsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("InputIds cannot be empty"));
        await Assert.ThrowsAsync<ValidationException>(
            () => _controller.RestoreManyInputsAsync(request, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "INPUT_C_004 - Lấy danh sách phiếu nhập đã xóa với phân trang")]
    public async Task GetDeletedInputs_ValidRequest_ReturnsSuccess()
    {
        var sieveModel = new SieveModel { Page = 1, PageSize = 10 };
        var expectedResponse = new PagedResult<InputListResponse>([], 0, 1, 10);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDeletedInputsListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<InputListResponse>>.Success(expectedResponse));
        var result = await _controller.GetDeletedInputsAsync(sieveModel, CancellationToken.None).ConfigureAwait(true);
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact(DisplayName = "INPUT_C_005 - Lấy danh sách phiếu nhập theo SupplierId hợp lệ")]
    public async Task GetInputsBySupplierId_ValidSupplierId_ReturnsSuccess()
    {
        int supplierId = 1;
        var sieveModel = new SieveModel { Page = 1, PageSize = 10 };
        var expectedResponse = new PagedResult<InputListResponse>([], 0, 1, 10);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetInputsBySupplierIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<InputListResponse>>.Success(expectedResponse));
        var result = await _controller.GetInputsBySupplierIdAsync(supplierId, sieveModel, CancellationToken.None)
            .ConfigureAwait(true);
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact(DisplayName = "INPUT_C_006 - Tạo phiếu nhập với request hợp lệ")]
    public async Task CreateInput_ValidRequest_CallsMediator()
    {
        var request = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products = [new CreateInputInfoRequest { ProductVariantId = 1, Count = 10, InputPrice = 100000 }]
        };
        var expectedResponse = new InputDetailResponse { Id = 1, StatusId = "working" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateInputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InputDetailResponse?>.Success(expectedResponse));
        var result = await _controller.CreateInputAsync(request, CancellationToken.None).ConfigureAwait(true);
        result.Should().BeOfType<CreatedAtRouteResult>();
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateInputCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "INPUT_C_007 - Cập nhật phiếu nhập với request hợp lệ")]
    public async Task UpdateInput_ValidRequest_CallsMediator()
    {
        int inputId = 1;
        var request = new UpdateInputCommand { Notes = "Updated", SupplierId = 2, Products = [] };
        var expectedResponse = new InputDetailResponse { Id = inputId, Notes = "Updated" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateInputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InputDetailResponse?>.Success(expectedResponse));
        var result = await _controller.UpdateInputAsync(inputId, request, CancellationToken.None).ConfigureAwait(true);
        result.Should().BeOfType<OkObjectResult>();
        _mediatorMock.Verify(m => m.Send(It.IsAny<UpdateInputCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "INPUT_C_008 - Cập nhật trạng thái phiếu nhập hợp lệ")]
    public async Task UpdateInputStatus_ValidRequest_CallsMediator()
    {
        int inputId = 1;
        var request = new UpdateInputStatusCommand { StatusId = "finished" };
        var expectedResponse = new InputDetailResponse { Id = inputId, StatusId = "finished" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateInputStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InputDetailResponse>.Success(expectedResponse));
        var result = await _controller.UpdateInputStatusAsync(inputId, request, CancellationToken.None)
            .ConfigureAwait(true);
        result.Should().BeOfType<OkObjectResult>();
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<UpdateInputStatusCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "INPUT_072 - Lấy danh sách trạng thái phiếu nhập khi thiếu quyền trả 403")]
    public async Task GetInputStatuses_MissingPermission_ThrowsUnauthorized()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetInputStatusListQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new UnauthorizedAccessException(
                    "User does not have permission Domain.Constants.Permission.Permissions.Inputs.View"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetInputStatusesAsync(CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "INPUT_073 - Controller gọi MediatR đúng 1 lần khi lấy danh sách trạng thái phiếu nhập")]
    public async Task GetInputStatuses_ValidRequest_CallsMediatorOnce()
    {
        var expectedStatuses = new Dictionary<string, string>
        {
            { InputStatus.Working, "Phiếu tạm" },
            { InputStatus.Finish, "Hoàn thành" },
            { InputStatus.Cancel, "Đã hủy" },
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetInputStatusListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Dictionary<string, string>>.Success(expectedStatuses));
        var result = await _controller.GetInputStatusesAsync(CancellationToken.None).ConfigureAwait(true);
        result.Should().BeOfType<OkObjectResult>();
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<GetInputStatusListQuery>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "INPUT_074 - Controller trả đúng dữ liệu từ Handler khi lấy trạng thái phiếu nhập")]
    public async Task GetInputStatuses_ValidRequest_ReturnsExpectedData()
    {
        var expectedStatuses = new Dictionary<string, string> { { InputStatus.Working, "Phiếu tạm" } };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetInputStatusListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Dictionary<string, string>>.Success(expectedStatuses));
        var result = await _controller.GetInputStatusesAsync(CancellationToken.None).ConfigureAwait(true);
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().BeEquivalentTo(expectedStatuses);
    }
    #pragma warning restore CRR0035
    #pragma warning restore IDE0079
}

