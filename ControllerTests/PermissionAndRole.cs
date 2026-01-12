using Application.ApiContracts.Permission.Requests;
using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using Application.Features.Permissions.Commands.CreateRole;
using Application.Features.Permissions.Commands.DeleteMultipleRoles;
using Application.Features.Permissions.Commands.DeleteRole;
using Application.Features.Permissions.Commands.UpdateRole;
using Application.Features.Permissions.Commands.UpdateRolePermissions;
using Application.Features.Permissions.Queries.GetAllPermissions;
using Application.Features.Permissions.Queries.GetAllRoles;
using Application.Features.Permissions.Queries.GetMyPermissions;
using Application.Features.Permissions.Queries.GetRolePermissions;
using Application.Features.Permissions.Queries.GetUserPermissionsById;
using Domain.Constants.Permission;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using WebAPI.Controllers.V1;
using Xunit;

namespace ControllerTests;

public class PermissionAndRole
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly PermissionController _controller;

    public PermissionAndRole()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new PermissionController(_mediatorMock.Object);

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

    [Fact(DisplayName = "PERM_CTRL_001 - Controller gọi GetAllPermissions thành công")]
    public async Task GetAllPermissions_MediatorReturnsData_ReturnsOkWithData()
    {
        // Arrange
        var expectedData = new Dictionary<string, List<PermissionResponse>>
        {
            { "Brands", new List<PermissionResponse>
                {
                    new() { ID = PermissionsList.Brands.View, DisplayName = "View Brands", Description = "Xem danh sách thương hiệu" },
                    new() { ID = PermissionsList.Brands.Create, DisplayName = "Create Brand", Description = "Tạo thương hiệu mới" }
                }
            },
            { "Products", new List<PermissionResponse>
                {
                    new() { ID = PermissionsList.Products.View, DisplayName = "View Products", Description = "Xem danh sách sản phẩm" }
                }
            },
            { "Roles", new List<PermissionResponse>
                {
                    new() { ID = PermissionsList.Roles.View, DisplayName = "View Roles", Description = "Xem danh sách vai trò" }
                }
            }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllPermissionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);

        // Act
        var result = await _controller.GetAllPermissions(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedData);
    }

    [Fact(DisplayName = "PERM_CTRL_002 - Controller gọi GetMyPermissions thành công")]
    public async Task GetMyPermissions_MediatorReturnsData_ReturnsOkWithPermissions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetupUserClaims(userId);

        var expectedResponse = new PermissionAndRoleOfUserResponse
        {
            UserId = userId,
            UserName = "testuser",
            Email = "test@test.com",
            Roles = ["Manager"],
            Permissions =
            [
                new() { ID = PermissionsList.Brands.View },
                new() { ID = PermissionsList.Brands.Create },
                new() { ID = PermissionsList.Products.View },
                new() { ID = PermissionsList.Products.Create }
            ]
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetMyPermissionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetMyPermissions(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as PermissionAndRoleOfUserResponse;
        response.Should().NotBeNull();
        response!.Permissions.Should().HaveCount(4);
    }

    [Fact(DisplayName = "PERM_CTRL_003 - Controller gọi GetMyPermissions khi user chưa đăng nhập")]
    public async Task GetMyPermissions_NoUserClaims_ThrowsUnauthorizedException()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetMyPermissionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PermissionAndRoleOfUserResponse>.Failure(Error.Unauthorized("User not authenticated")));

        // Act
        var result = await _controller.GetMyPermissions(CancellationToken.None);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact(DisplayName = "PERM_CTRL_004 - Controller gọi GetUserPermissionsById thành công")]
    public async Task GetUserPermissionsById_ValidUserId_ReturnsOkWithPermissions()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var expectedResponse = new PermissionAndRoleOfUserResponse
        {
            UserId = userId,
            UserName = "targetuser",
            Email = "target@test.com",
            Roles = ["Staff"],
            Permissions =
            [
                new() { ID = PermissionsList.Products.View },
                new() { ID = PermissionsList.Brands.View },
                new() { ID = PermissionsList.Files.View },
                new() { ID = PermissionsList.Files.Upload },
                new() { ID = PermissionsList.Suppliers.View }
            ]
        };

        _mediatorMock.Setup(m => m.Send(It.Is<GetUserPermissionsByIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetUserPermissionsById(userId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as PermissionAndRoleOfUserResponse;
        response.Should().NotBeNull();
        response!.Permissions.Should().HaveCount(5);
        response.UserId.Should().Be(userId);
    }

    [Fact(DisplayName = "PERM_CTRL_005 - Controller gọi GetUserPermissionsById với userId không hợp lệ")]
    public async Task GetUserPermissionsById_InvalidUserId_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mediatorMock.Setup(m => m.Send(It.Is<GetUserPermissionsByIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PermissionAndRoleOfUserResponse>.Failure(Error.NotFound($"User with id {userId} not found")));

        // Act
        var result = await _controller.GetUserPermissionsById(userId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "PERM_CTRL_006 - Controller gọi GetRolePermissions thành công")]
    public async Task GetRolePermissions_ValidRoleName_ReturnsOkWithPermissions()
    {
        // Arrange
        var roleName = "Manager";

        var expectedPermissions = new List<PermissionResponse>
        {
            new() { ID = PermissionsList.Brands.View },
            new() { ID = PermissionsList.Brands.Create },
            new() { ID = PermissionsList.Products.View },
            new() { ID = PermissionsList.Products.Create },
            new() { ID = PermissionsList.Files.View },
            new() { ID = PermissionsList.Suppliers.View }
        };

        _mediatorMock.Setup(m => m.Send(It.Is<GetRolePermissionsQuery>(q => q.RoleName == roleName), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPermissions);

        // Act
        var result = await _controller.GetRolePermissions(roleName, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var permissions = okResult!.Value as List<PermissionResponse>;
        permissions.Should().NotBeNull();
        permissions.Should().HaveCount(6);
    }

    [Fact(DisplayName = "PERM_CTRL_007 - Controller gọi GetRolePermissions với role không tồn tại")]
    public async Task GetRolePermissions_InvalidRoleName_ThrowsNotFoundException()
    {
        // Arrange
        var roleName = "InvalidRole";

        _mediatorMock.Setup(m => m.Send(It.Is<GetRolePermissionsQuery>(q => q.RoleName == roleName), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<PermissionResponse>>.Failure(Error.NotFound($"Role {roleName} not found")));

        // Act
        var result = await _controller.GetRolePermissions(roleName, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "PERM_CTRL_008 - Controller gọi UpdateRolePermissions thành công")]
    public async Task UpdateRolePermissions_ValidData_ReturnsOk()
    {
        // Arrange
        var roleName = "Staff";
        var request = new UpdateRoleRequest
        {
            Permissions = [PermissionsList.Products.View, PermissionsList.Brands.View]
        };

        var expectedResponse = new PermissionRoleUpdateResponse
        {
            Message = "Permission updated successfully."
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateRolePermissionsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.UpdateRolePermissions(roleName, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact(DisplayName = "PERM_CTRL_009 - Controller gọi GetAllRoles thành công")]
    public async Task GetAllRoles_MediatorReturnsData_ReturnsOkWithRoles()
    {
        // Arrange
        var expectedRoles = new List<RoleSelectResponse>
        {
            new() { ID = Guid.NewGuid(), Name = "Admin" },
            new() { ID = Guid.NewGuid(), Name = "Manager" },
            new() { ID = Guid.NewGuid(), Name = "Staff" },
            new() { ID = Guid.NewGuid(), Name = "User" }
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllRolesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRoles);

        // Act
        var result = await _controller.GetAllRoles(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var roles = okResult!.Value as List<RoleSelectResponse>;
        roles.Should().NotBeNull();
        roles.Should().HaveCount(4);
    }

    [Fact(DisplayName = "PERM_CTRL_010 - Controller gọi CreateRole thành công")]
    public async Task CreateRole_ValidRequest_ReturnsOkWithCreatedRole()
    {
        // Arrange
        var request = new CreateRoleRequest
        {
            RoleName = "NewRole",
            Description = "Test Role",
            Permissions = [PermissionsList.Brands.View, PermissionsList.Products.View]
        };

        var expectedResponse = new RoleCreateResponse
        {
            RoleId = Guid.NewGuid(),
            RoleName = "NewRole",
            Description = "Test Role",
            Permissions = request.Permissions,
            Message = "Role created successfully"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateRoleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.CreateRole(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as RoleCreateResponse;
        response.Should().NotBeNull();
        response!.RoleName.Should().Be("NewRole");
        response.RoleId.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "PERM_CTRL_011 - Controller gọi CreateRole với tên trùng")]
    public async Task CreateRole_DuplicateName_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateRoleRequest
        {
            RoleName = "Admin",
            Description = "Duplicate",
            Permissions = [PermissionsList.Brands.View]
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateRoleCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FluentValidation.ValidationException("Role name already exists"));

        // Act
        Func<Task> act = async () => await _controller.CreateRole(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage("*already exists*");
    }

    [Fact(DisplayName = "PERM_CTRL_012 - Controller gọi UpdateRole thành công")]
    public async Task UpdateRole_ValidRequest_ReturnsOkWithUpdatedRole()
    {
        // Arrange
        var roleName = "Editor";
        var request = new UpdateRoleRequest
        {
            Description = "Updated Description"
        };

        var expectedResponse = new RoleUpdateResponse
        {
            RoleId = Guid.NewGuid(),
            RoleName = "Editor",
            Description = "Updated Description",
            Message = "Role updated successfully"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateRoleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.UpdateRole(roleName, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as RoleUpdateResponse;
        response.Should().NotBeNull();
        response!.RoleName.Should().Be("Editor");
        response.Description.Should().Be("Updated Description");
    }

    [Fact(DisplayName = "PERM_CTRL_013 - Controller gọi DeleteRole thành công")]
    public async Task DeleteRole_ValidRoleName_ReturnsOk()
    {
        // Arrange
        var roleName = "OldRole";

        var expectedResponse = new RoleDeleteResponse
        {
            Message = "Role deleted successfully"
        };

        _mediatorMock.Setup(m => m.Send(It.Is<DeleteRoleCommand>(c => c.RoleName == roleName), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.DeleteRole(roleName, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as RoleDeleteResponse;
        response.Should().NotBeNull();
        response!.Message.Should().Contain("successfully");
    }

    [Fact(DisplayName = "PERM_CTRL_014 - Controller gọi DeleteRole với role không tồn tại")]
    public async Task DeleteRole_NonExistentRole_ThrowsNotFoundException()
    {
        // Arrange
        var roleName = "NonExistent";

        _mediatorMock.Setup(m => m.Send(It.Is<DeleteRoleCommand>(c => c.RoleName == roleName), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RoleDeleteResponse>.Failure(Error.NotFound($"Role {roleName} not found")));

        // Act
        var result = await _controller.DeleteRole(roleName, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "PERM_CTRL_015 - Controller gọi DeleteMultipleRoles thành công")]
    public async Task DeleteMultipleRoles_ValidRoleNames_ReturnsOk()
    {
        // Arrange
        var roleNames = new List<string> { "Role1", "Role2" };

        var expectedResponse = new RoleDeleteResponse
        {
            Message = "2 roles deleted successfully"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteMultipleRolesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.DeleteMultipleRoles(roleNames, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as RoleDeleteResponse;
        response.Should().NotBeNull();
        response!.Message.Should().Contain("2");
        response.Message.Should().Contain("successfully");
    }

    // Helper Methods

    private void SetupUserClaims(Guid userId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, "testuser"),
            new(ClaimTypes.Email, "test@test.com")
        };

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext.HttpContext.User = claimsPrincipal;
    }
}
