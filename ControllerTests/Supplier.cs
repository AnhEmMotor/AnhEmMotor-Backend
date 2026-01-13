using Application.ApiContracts.Supplier.Responses;
using Application.Common.Models;
using Application.Features.Suppliers.Commands.CreateSupplier;
using Application.Features.Suppliers.Commands.DeleteManySuppliers;
using Application.Features.Suppliers.Commands.DeleteSupplier;
using Application.Features.Suppliers.Commands.RestoreManySuppliers;
using Application.Features.Suppliers.Commands.RestoreSupplier;
using Application.Features.Suppliers.Commands.UpdateManySupplierStatus;
using Application.Features.Suppliers.Commands.UpdateSupplier;
using Application.Features.Suppliers.Commands.UpdateSupplierStatus;
using Application.Features.Suppliers.Queries.GetDeletedSuppliersList;
using Application.Features.Suppliers.Queries.GetSupplierById;
using Application.Features.Suppliers.Queries.GetSuppliersList;

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

public class Supplier
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly SupplierController _controller;

    public Supplier()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new SupplierController(_mediatorMock.Object);
        
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

#pragma warning disable CRR0035
    [Fact(DisplayName = "SUP_046 - Tạo Supplier thành công qua API")]
    public async Task CreateSupplier_Success_ReturnsCreatedSupplier()
    {
        // Arrange
        var request = new CreateSupplierCommand
        {
            Name = "API Supplier",
            Phone = "0123456789",
            Address = "API Street"
        };

        var expectedResponse = new SupplierResponse
        {
            Id = 1,
            Name = "API Supplier",
            Phone = "0123456789",
            Address = "API Street",
            StatusId = "active"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateSupplierCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SupplierResponse>.Success(expectedResponse));

        // Act
        var result = await _controller.CreateSupplierAsync(request, CancellationToken.None).ConfigureAwait(true);

        // Assert
        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        var response = createdResult.Value.Should().BeOfType<SupplierResponse>().Subject;
        response.Id.Should().Be(1);
        response.Name.Should().Be("API Supplier");
        response.StatusId.Should().Be("active");
    }

    [Fact(DisplayName = "SUP_047 - Tạo Supplier thất bại khi không có quyền")]
    public async Task CreateSupplier_NoPermission_ReturnsForbidden()
    {
        // Arrange
        var request = new CreateSupplierCommand
        {
            Name = "No Permission",
            Phone = "0123456789",
            Address = "123 Street"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateSupplierCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("No permission"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.CreateSupplierAsync(request, CancellationToken.None)).ConfigureAwait(true);
    }

    [Fact(DisplayName = "SUP_048 - Tạo Supplier thất bại khi chưa đăng nhập")]
    public async Task CreateSupplier_Unauthorized_ReturnsUnauthorized()
    {
        // Arrange
        var request = new CreateSupplierCommand
        {
            Name = "Unauthorized",
            Phone = "0123456789",
            Address = "123 Street"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateSupplierCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Not authenticated"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.CreateSupplierAsync(request, CancellationToken.None)).ConfigureAwait(true);
    }

    [Fact(DisplayName = "SUP_049 - Lấy danh sách Supplier thành công với quyền View Supplier")]
    public async Task GetSuppliers_WithViewPermission_ReturnsSupplierList()
    {
        // Arrange
        var sieveModel = new SieveModel();
        var items = new List<SupplierResponse>
        {
            new() { Id = 1, Name = "Supplier 1", StatusId = "active", TotalInput = 1000000 },
            new() { Id = 2, Name = "Supplier 2", StatusId = "inactive", TotalInput = 2000000 }
        };
        var expectedResponse = new PagedResult<SupplierResponse>(items, 15, 1, 10);

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetSuppliersListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<SupplierResponse>>.Success(expectedResponse));

        // Act
        var result = await _controller.GetSuppliersAsync(sieveModel, CancellationToken.None).ConfigureAwait(true);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<PagedResult<SupplierResponse>>().Subject;
        response.Items.Should().HaveCount(2);
        response.TotalCount.Should().Be(15);
        response.Items.First().TotalInput.Should().NotBeNull();
    }

    [Fact(DisplayName = "SUP_050 - Lấy danh sách Supplier for-input chỉ trả về active")]
    public async Task GetSuppliersForInput_ReturnsOnlyActiveSuppliers()
    {
        // Arrange
        var sieveModel = new SieveModel();
        var items = new List<SupplierResponse>
        {
            new() { Id = 1, Name = "Supplier 1", StatusId = "active" },
            new() { Id = 2, Name = "Supplier 2", StatusId = "active" }
        };
        var expectedResponse = new PagedResult<SupplierResponse>(items, 10, 1, 10);

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetSuppliersListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<SupplierResponse>>.Success(expectedResponse));

        // Act
        var result = await _controller.GetSuppliersForInputAsync(sieveModel, CancellationToken.None).ConfigureAwait(true);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<PagedResult<SupplierResponse>>().Subject;
        response.Items.Should().HaveCount(2);
        response.Items.Should().OnlyContain(s => string.Compare(s.StatusId, "active") == 0);
        response.Items.First().TotalInput.Should().BeNull();
    }

    [Fact(DisplayName = "SUP_051 - Lấy danh sách Supplier thành công khi không có quyền (chỉ active, không có TotalInputValue)")]
    public async Task GetSuppliers_NoPermission_ReturnsOnlyActiveWithoutTotalInput()
    {
        // Arrange
        var sieveModel = new SieveModel();
        var items = new List<SupplierResponse>
        {
            new() { Id = 1, Name = "Supplier 1", StatusId = "active" },
            new() { Id = 2, Name = "Supplier 2", StatusId = "active" }
        };
        var expectedResponse = new PagedResult<SupplierResponse>(items, 10, 1, 10);

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetSuppliersListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<SupplierResponse>>.Success(expectedResponse));

        // Act
        var result = await _controller.GetSuppliersAsync(sieveModel, CancellationToken.None).ConfigureAwait(true);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<PagedResult<SupplierResponse>>().Subject;
        response.Items.Should().OnlyContain(s => string.Compare(s.StatusId, "active") == 0);
        response.Items.Should().OnlyContain(s => s.TotalInput == null);
    }

    [Fact(DisplayName = "SUP_052 - Lấy chi tiết Supplier thành công với quyền View Supplier")]
    public async Task GetSupplierById_WithViewPermission_ReturnsSupplierWithTotalInput()
    {
        // Arrange
        var expectedResponse = new SupplierResponse
        {
            Id = 1,
            Name = "Supplier Detail",
            StatusId = "active",
            TotalInput = 5000000
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetSupplierByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SupplierResponse?>.Success(expectedResponse));

        // Act
        var result = await _controller.GetSupplierByIdAsync(1, CancellationToken.None).ConfigureAwait(true);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<SupplierResponse>().Subject;
        response.Id.Should().Be(1);
        response.TotalInput.Should().Be(5000000);
    }

    [Fact(DisplayName = "SUP_053 - Lấy chi tiết Supplier thành công khi không có quyền (chỉ active)")]
    public async Task GetSupplierById_NoPermission_ReturnsActiveSupplierWithTotalInput()
    {
        // Arrange
        var expectedResponse = new SupplierResponse
        {
            Id = 1,
            Name = "Supplier Detail",
            StatusId = "active",
            TotalInput = 5000000
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetSupplierByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SupplierResponse?>.Success(expectedResponse));

        // Act
        var result = await _controller.GetSupplierByIdAsync(1, CancellationToken.None).ConfigureAwait(true);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<SupplierResponse>().Subject;
        response.StatusId.Should().Be("active");
        response.TotalInput.Should().NotBeNull();
    }

    [Fact(DisplayName = "SUP_054 - Lấy chi tiết Supplier thất bại khi không có quyền và Supplier không active")]
    public async Task GetSupplierById_NoPermissionAndInactive_ReturnsForbidden()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetSupplierByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("No permission to view inactive supplier"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetSupplierByIdAsync(1, CancellationToken.None)).ConfigureAwait(true);
    }

    [Fact(DisplayName = "SUP_055 - Cập nhật Supplier thất bại khi không có quyền")]
    public async Task UpdateSupplier_NoPermission_ReturnsForbidden()
    {
        // Arrange
        var request = new UpdateSupplierCommand { Name = "Updated" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateSupplierCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("No permission"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.UpdateSupplierAsync(1, request, CancellationToken.None)).ConfigureAwait(true);
    }

    [Fact(DisplayName = "SUP_056 - Xóa Supplier thất bại khi không có quyền")]
    public async Task DeleteSupplier_NoPermission_ReturnsForbidden()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteSupplierCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("No permission"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.DeleteSupplierAsync(1, CancellationToken.None)).ConfigureAwait(true);
    }

    [Fact(DisplayName = "SUP_057 - Lấy danh sách deleted Supplier thất bại khi không có quyền")]
    public async Task GetDeletedSuppliers_NoPermission_ReturnsForbidden()
    {
        // Arrange
        var sieveModel = new SieveModel();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetDeletedSuppliersListQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("No permission"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.GetDeletedSuppliersAsync(sieveModel, CancellationToken.None)).ConfigureAwait(true);
    }

    [Fact(DisplayName = "SUP_058 - Xóa nhiều Supplier với một phần thành công và một phần thất bại")]
    public async Task DeleteManySuppliers_PartialFailure_ReturnsError()
    {
        // Arrange
        var request = new DeleteManySuppliersCommand
        {
            Ids = [1, 2, 3]
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteManySuppliersCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Cannot delete supplier with working input receipts"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.DeleteSuppliersAsync(request, CancellationToken.None)).ConfigureAwait(true);
    }

    [Fact(DisplayName = "SUP_059 - Khôi phục Supplier thất bại khi không có quyền")]
    public async Task RestoreSupplier_NoPermission_ReturnsForbidden()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<RestoreSupplierCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("No permission"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.RestoreSupplierAsync(1, CancellationToken.None)).ConfigureAwait(true);
    }

    [Fact(DisplayName = "SUP_060 - Cập nhật trạng thái nhiều Supplier thất bại khi không có quyền")]
    public async Task UpdateManySupplierStatus_NoPermission_ReturnsForbidden()
    {
        // Arrange
        var request = new UpdateManySupplierStatusCommand
        {
            Ids = [1, 2],
            StatusId = "inactive"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateManySupplierStatusCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("No permission"));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.UpdateManySupplierStatusAsync(request, CancellationToken.None)).ConfigureAwait(true);
    }
#pragma warning restore CRR0035
}
