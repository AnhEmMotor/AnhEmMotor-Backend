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
using Domain.Primitives;
using FluentAssertions;
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
            Products = [ new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 } ]
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateInputCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not have permission Permissions.Inputs.Create"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.CreateInputAsync(request, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "INPUT_019 - Lấy danh sách phiếu nhập thiếu quyền View")]
    public async Task GetInputs_MissingPermission_ReturnsForbidden()
    {
        var sieveModel = new SieveModel { Page = 1, PageSize = 10 };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetInputsListQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not have permission Permissions.Inputs.View"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetInputsAsync(sieveModel, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "INPUT_022 - L?y chi ti?t phi?u nh?p thi?u quy?n View")]
    public async Task GetInputById_MissingPermission_ReturnsForbidden()
    {
        int inputId = 1;

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetInputByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not have permission Permissions.Inputs.View"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetInputByIdAsync(inputId, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "INPUT_026 - C?p nh?t phi?u nh?p thi?u quy?n Edit")]
    public async Task UpdateInput_MissingPermission_ReturnsForbidden()
    {
        int inputId = 1;
        var request = new UpdateInputCommand { Notes = "Updated", SupplierId = 2, Products = [] };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateInputCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not have permission Permissions.Inputs.Edit"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.UpdateInputAsync(inputId, request, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "INPUT_027 - C?p nh?t phi?u nh?p kh�ng t?n t?i")]
    public async Task UpdateInput_NotFound_ReturnsNotFound()
    {
        int inputId = 9999;
        var request = new UpdateInputCommand { Notes = "Updated", SupplierId = 2, Products = [] };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateInputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InputDetailResponse?>.Failure(Error.NotFound("Input not found")));

        var result = await _controller.UpdateInputAsync(inputId, request, CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "INPUT_031 - C?p nh?t tr?ng th�i phi?u nh?p thi?u quy?n ChangeStatus")]
    public async Task UpdateInputStatus_MissingPermission_ReturnsForbidden()
    {
        int inputId = 1;
        var request = new UpdateInputStatusCommand { StatusId = "finished" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateInputStatusCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new UnauthorizedAccessException("User does not have permission Permissions.Inputs.ChangeStatus"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.UpdateInputStatusAsync(inputId, request, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "INPUT_035 - X�a phi?u nh?p ? tr?ng th�i finished (kh�ng cho ph�p)")]
    public async Task DeleteInput_FinishedStatus_ReturnsBadRequest()
    {
        int inputId = 1;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteInputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(Error.BadRequest("Cannot delete finished input")));

        var result = await _controller.DeleteInputAsync(inputId, CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "INPUT_036 - X�a phi?u nh?p thi?u quy?n Delete")]
    public async Task DeleteInput_MissingPermission_ReturnsForbidden()
    {
        int inputId = 1;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteInputCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not have permission Permissions.Inputs.Delete"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.DeleteInputAsync(inputId, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "INPUT_037 - X�a phi?u nh?p kh�ng t?n t?i")]
    public async Task DeleteInput_NotFound_ReturnsNotFound()
    {
        int inputId = 9999;

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteInputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(Error.NotFound("Input not found")));

        var result = await _controller.DeleteInputAsync(inputId, CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "INPUT_040 - Kh�i ph?c phi?u nh?p chua b? x�a")]
    public async Task RestoreInput_NotDeleted_ReturnsBadRequest()
    {
        int inputId = 1;

        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreInputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InputResponse>.Failure(Error.BadRequest("Input is not deleted")));

        var result = await _controller.RestoreInputAsync(inputId, CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "INPUT_044 - Clone phi?u nh?p thi?u quy?n Create")]
    public async Task CloneInput_MissingPermission_ReturnsForbidden()
    {
        int inputId = 1;

        _mediatorMock.Setup(m => m.Send(It.IsAny<CloneInputCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not have permission Permissions.Inputs.Create"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.CloneInputAsync(inputId, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "INPUT_045 - Clone phi?u nh?p kh�ng t?n t?i")]
    public async Task CloneInput_NotFound_ReturnsNotFound()
    {
        int inputId = 9999;

        _mediatorMock.Setup(m => m.Send(It.IsAny<CloneInputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InputDetailResponse?>.Failure(Error.NotFound("Input not found")));

        var result = await _controller.CloneInputAsync(inputId, CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "INPUT_C_001 - X�a nhi?u phi?u nh?p v?i danh s�ch r?ng")]
    public async Task DeleteManyInputs_EmptyList_ReturnsBadRequest()
    {
        var request = new DeleteManyInputsCommand { Ids = [] };

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteManyInputsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("InputIds cannot be empty"));

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => _controller.DeleteManyInputsAsync(request, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "INPUT_C_002 - C?p nh?t tr?ng th�i nhi?u phi?u nh?p v?i danh s�ch r?ng")]
    public async Task UpdateManyInputStatus_EmptyList_ReturnsBadRequest()
    {
        var request = new UpdateManyInputStatusCommand { Ids = [], StatusId = "finished" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateManyInputStatusCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("InputIds cannot be empty"));

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => _controller.UpdateManyInputStatusAsync(request, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "INPUT_C_003 - Kh�i ph?c nhi?u phi?u nh?p v?i danh s�ch r?ng")]
    public async Task RestoreManyInputs_EmptyList_ReturnsBadRequest()
    {
        var request = new RestoreManyInputsCommand { Ids = [] };

        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreManyInputsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("InputIds cannot be empty"));

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => _controller.RestoreManyInputsAsync(request, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "INPUT_C_004 - L?y danh s�ch phi?u nh?p d� x�a v?i ph�n trang")]
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

    [Fact(DisplayName = "INPUT_C_005 - L?y danh s�ch phi?u nh?p theo SupplierId h?p l?")]
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

    [Fact(DisplayName = "INPUT_C_006 - T?o phi?u nh?p v?i request h?p l?")]
    public async Task CreateInput_ValidRequest_CallsMediator()
    {
        var request = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products = [ new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 } ]
        };

        var expectedResponse = new InputDetailResponse { Id = 1, StatusId = "working" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateInputCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InputDetailResponse?>.Success(expectedResponse));

        var result = await _controller.CreateInputAsync(request, CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<CreatedAtRouteResult>();
        _mediatorMock.Verify(m => m.Send(It.IsAny<CreateInputCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "INPUT_C_007 - C?p nh?t phi?u nh?p v?i request h?p l?")]
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

    [Fact(DisplayName = "INPUT_C_008 - C?p nh?t tr?ng th�i phi?u nh?p h?p l?")]
    public async Task UpdateInputStatus_ValidRequest_CallsMediator()
    {
        int inputId = 1;
        var request = new UpdateInputStatusCommand { StatusId = "finished" };

        var expectedResponse = new InputDetailResponse { Id = inputId, StatusId = "finished" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateInputStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InputResponse>.Success(expectedResponse));

        var result = await _controller.UpdateInputStatusAsync(inputId, request, CancellationToken.None)
            .ConfigureAwait(true);

        result.Should().BeOfType<OkObjectResult>();
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<UpdateInputStatusCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "INPUT_072 - L?y danh s�ch tr?ng th�i phi?u nh?p khi thi?u quy?n tr? 403")]
    public async Task GetInputStatuses_MissingPermission_ThrowsUnauthorized()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetInputStatusListQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not have permission Permissions.Inputs.View"));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetInputStatusesAsync(CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "INPUT_073 - Controller g?i MediatR d�ng 1 l?n khi l?y danh s�ch tr?ng th�i phi?u nh?p")]
    public async Task GetInputStatuses_ValidRequest_CallsMediatorOnce()
    {
        var expectedStatuses = new Dictionary<string, string>
        {
            { Domain.Constants.Input.InputStatus.Working, "Phi?u t?m" },
            { Domain.Constants.Input.InputStatus.Finish, "Ho�n th�nh" },
            { Domain.Constants.Input.InputStatus.Cancel, "�� hu?" },
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetInputStatusListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Dictionary<string, string>>.Success(expectedStatuses));

        var result = await _controller.GetInputStatusesAsync(CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<OkObjectResult>();
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetInputStatusListQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "INPUT_074 - Controller tr? d�ng d? li?u t? Handler khi l?y tr?ng th�i phi?u nh?p")]
    public async Task GetInputStatuses_ValidRequest_ReturnsExpectedData()
    {
        var expectedStatuses = new Dictionary<string, string> { { Domain.Constants.Input.InputStatus.Working, "Phi?u t?m" } };
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
