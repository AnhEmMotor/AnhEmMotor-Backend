using Application.ApiContracts.Permission.Requests;
using Application.ApiContracts.Permission.Responses;
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
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Repositories.Role;
using Application.Interfaces.Services;
using Domain.Constants.Permission;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using PermissionEntity = Domain.Entities.Permission;

namespace UnitTests;

public class PermissionAndRole
{
    [Fact(DisplayName = "PERM_001 - Lấy tất cả permissions thành công")]
    public async Task GetAllPermissions_NoParams_ReturnsGroupedPermissions()
    {
        // Arrange
        var handler = new GetAllPermissionsQueryHandler();
        var query = new GetAllPermissionsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Dictionary<string, List<PermissionResponse>>>();
        result.Should().ContainKey("Brands");
        result.Should().ContainKey("Products");
        result.Should().ContainKey("Roles");
    }

    [Fact(DisplayName = "PERM_002 - Lấy permissions của user hiện tại thành công")]
    public async Task GetMyPermissions_UserWithRoleAndPermissions_ReturnsPermissions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        var roleReadRepoMock = new Mock<IRoleReadRepository>();

        var user = new ApplicationUser { Id = userId, UserName = "testuser" };
        userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(["Manager"]);

        var permissions = new List<string>
        {
            PermissionsList.Brands.View,
            PermissionsList.Brands.Create,
            PermissionsList.Products.View,
            PermissionsList.Products.Create,
            PermissionsList.Roles.View
        };

        roleReadRepoMock.Setup(x => x.GetPermissionNamesByRoleIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var handler = new GetMyPermissionsQueryHandler(userManagerMock.Object, roleReadRepoMock.Object);
        var query = new GetMyPermissionsQuery(userId.ToString());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Permissions.Should().HaveCount(5);
        result.UserId.Should().Be(userId);
    }

    [Fact(DisplayName = "PERM_003 - Lấy permissions của user không có role nào")]
    public async Task GetMyPermissions_UserWithoutRoles_ReturnsEmptyPermissions()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        var roleReadRepoMock = new Mock<IRoleReadRepository>();

        var user = new ApplicationUser { Id = userId, UserName = "testuser" };
        userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync([]);

        var handler = new GetMyPermissionsQueryHandler(userManagerMock.Object, roleReadRepoMock.Object);
        var query = new GetMyPermissionsQuery(userId.ToString());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Permissions.Should().BeEmpty();
        result.UserId.Should().Be(userId);
    }

    [Fact(DisplayName = "PERM_004 - Lấy permissions của user bằng userId hợp lệ")]
    public async Task GetUserPermissionsById_ValidUserId_ReturnsPermissions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        var roleReadRepoMock = new Mock<IRoleReadRepository>();

        var user = new ApplicationUser { Id = userId, UserName = "staffuser", Email = "staff@test.com" };
        userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync(user);
        userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(["Staff"]);

        var permissions = new List<string>
        {
            PermissionsList.Products.View,
            PermissionsList.Brands.View,
            PermissionsList.Files.View
        };

        roleReadRepoMock.Setup(x => x.GetPermissionNamesByRoleIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var handler = new GetUserPermissionsByIdQueryHandler(userManagerMock.Object, roleReadRepoMock.Object);
        var query = new GetUserPermissionsByIdQuery(userId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Permissions.Should().HaveCount(3);
        result.Email.Should().Be("staff@test.com");
    }

    [Fact(DisplayName = "PERM_005 - Lấy permissions của user bằng userId không tồn tại")]
    public async Task GetUserPermissionsById_InvalidUserId_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        var roleReadRepoMock = new Mock<IRoleReadRepository>();

        userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
            .ReturnsAsync((ApplicationUser?)null);

        var handler = new GetUserPermissionsByIdQueryHandler(userManagerMock.Object, roleReadRepoMock.Object);
        var query = new GetUserPermissionsByIdQuery(userId);

        // Act
        Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*User not found*");
    }

    [Fact(DisplayName = "PERM_006 - Lấy permissions của role hợp lệ")]
    public async Task GetRolePermissions_ValidRoleName_ReturnsPermissions()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
        var roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStoreMock.Object, null, null, null, null);

        var roleReadRepoMock = new Mock<IRoleReadRepository>();

        var role = new ApplicationRole { Id = roleId, Name = "Manager" };
        roleManagerMock.Setup(x => x.FindByNameAsync("Manager"))
            .ReturnsAsync(role);

        var permissions = new List<string>
        {
            PermissionsList.Brands.View,
            PermissionsList.Brands.Create,
            PermissionsList.Brands.Edit,
            PermissionsList.Brands.Delete
        };

        roleReadRepoMock.Setup(x => x.GetPermissionNamesByRoleIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var handler = new GetRolePermissionsQueryHandler(roleManagerMock.Object, roleReadRepoMock.Object);
        var query = new GetRolePermissionsQuery("Manager");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(4);
    }

    [Fact(DisplayName = "PERM_007 - Lấy permissions của role không tồn tại")]
    public async Task GetRolePermissions_InvalidRoleName_ThrowsNotFoundException()
    {
        // Arrange
        var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
        var roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStoreMock.Object, null, null, null, null);

        var roleReadRepoMock = new Mock<IRoleReadRepository>();

        roleManagerMock.Setup(x => x.FindByNameAsync("NonExistentRole"))
            .ReturnsAsync((ApplicationRole?)null);

        var handler = new GetRolePermissionsQueryHandler(roleManagerMock.Object, roleReadRepoMock.Object);
        var query = new GetRolePermissionsQuery("NonExistentRole");

        // Act
        Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Role not found*");
    }

    [Fact(DisplayName = "PERM_008 - Lấy permissions của role với tên có khoảng trắng đầu/cuối")]
    public async Task GetRolePermissions_RoleNameWithWhitespace_TrimsAndReturnsPermissions()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
        var roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStoreMock.Object, null, null, null, null);

        var roleReadRepoMock = new Mock<IRoleReadRepository>();

        var role = new ApplicationRole { Id = roleId, Name = "Manager" };
        roleManagerMock.Setup(x => x.FindByNameAsync("Manager"))
            .ReturnsAsync(role);

        var permissions = new List<string>
        {
            PermissionsList.Brands.View,
            PermissionsList.Brands.Create
        };

        roleReadRepoMock.Setup(x => x.GetPermissionNamesByRoleIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var handler = new GetRolePermissionsQueryHandler(roleManagerMock.Object, roleReadRepoMock.Object);
        var query = new GetRolePermissionsQuery("  Manager  ");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact(DisplayName = "PERM_009 - Tạo role mới thành công với tên và permissions hợp lệ")]
    public async Task CreateRole_ValidData_ReturnsSuccess()
    {
        // Arrange
        var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
        var roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStoreMock.Object, null, null, null, null);

        var permissionRepoMock = new Mock<IPermissionReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var permissions = new List<PermissionEntity>
        {
            new() { Id = 1, Name = PermissionsList.Brands.View },
            new() { Id = 2, Name = PermissionsList.Products.View }
        };

        permissionRepoMock.Setup(x => x.GetPermissionsByNamesAsync(
            It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        roleManagerMock.Setup(x => x.RoleExistsAsync("NewRole"))
            .ReturnsAsync(false);

        var createdRole = new ApplicationRole { Id = Guid.NewGuid(), Name = "NewRole", Description = "Test role" };
        roleManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<ApplicationRole>(r => { r.Id = createdRole.Id; });

        var handler = new CreateRoleCommandHandler(roleManagerMock.Object, permissionRepoMock.Object, roleUpdateRepoMock.Object, unitOfWorkMock.Object);
        var request = new CreateRoleRequest
        {
            RoleName = "NewRole",
            Description = "Test role",
            Permissions = [PermissionsList.Brands.View, PermissionsList.Products.View]
        };
        var command = new CreateRoleCommand(request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RoleName.Should().Be("NewRole");
        result.Description.Should().Be("Test role");
        result.Permissions.Should().HaveCount(2);
        result.RoleId.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "PERM_010 - Tạo role mới với tên đã tồn tại")]
    public async Task CreateRole_DuplicateName_ThrowsException()
    {
        // Arrange
        var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
        var roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStoreMock.Object, null, null, null, null);

        var permissionRepoMock = new Mock<IPermissionReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        roleManagerMock.Setup(x => x.RoleExistsAsync("Manager"))
            .ReturnsAsync(true);

        var handler = new CreateRoleCommandHandler(roleManagerMock.Object, permissionRepoMock.Object, roleUpdateRepoMock.Object, unitOfWorkMock.Object);
        var request = new CreateRoleRequest
        {
            RoleName = "Manager",
            Description = "Duplicate",
            Permissions = [PermissionsList.Brands.View]
        };
        var command = new CreateRoleCommand(request);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Role name already exists*");
    }

    [Fact(DisplayName = "PERM_011 - Tạo role mới với tên rỗng hoặc null - Trường hợp 1: RoleName rỗng")]
    public async Task CreateRole_EmptyRoleName_ThrowsValidationException()
    {
        // Arrange
        var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
        var roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStoreMock.Object, null, null, null, null);

        var permissionRepoMock = new Mock<IPermissionReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new CreateRoleCommandHandler(roleManagerMock.Object, permissionRepoMock.Object, roleUpdateRepoMock.Object, unitOfWorkMock.Object);
        var request = new CreateRoleRequest
        {
            RoleName = "",
            Description = "Test",
            Permissions = [PermissionsList.Brands.View]
        };
        var command = new CreateRoleCommand(request);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Role name is required*");
    }

    [Fact(DisplayName = "PERM_011 - Tạo role mới với tên rỗng hoặc null - Trường hợp 2: RoleName null")]
    public async Task CreateRole_NullRoleName_ThrowsValidationException()
    {
        // Arrange
        var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
        var roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStoreMock.Object, null, null, null, null);

        var permissionRepoMock = new Mock<IPermissionReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new CreateRoleCommandHandler(roleManagerMock.Object, permissionRepoMock.Object, roleUpdateRepoMock.Object, unitOfWorkMock.Object);
        var request = new CreateRoleRequest
        {
            RoleName = null!,
            Description = "Test",
            Permissions = [PermissionsList.Brands.View]
        };
        var command = new CreateRoleCommand(request);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Role name is required*");
    }

    [Fact(DisplayName = "PERM_012 - Tạo role mới với danh sách permissions rỗng")]
    public async Task CreateRole_EmptyPermissions_ThrowsValidationException()
    {
        // Arrange
        var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
        var roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStoreMock.Object, null, null, null, null);

        var permissionRepoMock = new Mock<IPermissionReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new CreateRoleCommandHandler(roleManagerMock.Object, permissionRepoMock.Object, roleUpdateRepoMock.Object, unitOfWorkMock.Object);
        var request = new CreateRoleRequest
        {
            RoleName = "EmptyPermRole",
            Description = "Test",
            Permissions = []
        };
        var command = new CreateRoleCommand(request);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*At least one permission is required*");
    }

    [Fact(DisplayName = "PERM_013 - Tạo role mới với permissions không tồn tại")]
    public async Task CreateRole_InvalidPermissions_ThrowsValidationException()
    {
        // Arrange
        var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
        var roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStoreMock.Object, null, null, null, null);

        var permissionRepoMock = new Mock<IPermissionReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        roleManagerMock.Setup(x => x.RoleExistsAsync("InvalidPermRole"))
            .ReturnsAsync(false);

        permissionRepoMock.Setup(x => x.GetPermissionsByNamesAsync(
            It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new CreateRoleCommandHandler(roleManagerMock.Object, permissionRepoMock.Object, roleUpdateRepoMock.Object, unitOfWorkMock.Object);
        var request = new CreateRoleRequest
        {
            RoleName = "InvalidPermRole",
            Description = "Test",
            Permissions = ["Permissions.Invalid.Permission", "Permissions.NotExist.Test"]
        };
        var command = new CreateRoleCommand(request);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Invalid permissions*");
    }

    [Fact(DisplayName = "PERM_014 - Tạo role mới với tên chứa ký tự đặc biệt")]
    public async Task CreateRole_RoleNameWithSpecialCharacters_ThrowsValidationException()
    {
        // Arrange
        var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
        var roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStoreMock.Object, null, null, null, null);

        var permissionRepoMock = new Mock<IPermissionReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new CreateRoleCommandHandler(roleManagerMock.Object, permissionRepoMock.Object, roleUpdateRepoMock.Object, unitOfWorkMock.Object);
        var request = new CreateRoleRequest
        {
            RoleName = "Role@#$%",
            Description = "Test",
            Permissions = [PermissionsList.Brands.View]
        };
        var command = new CreateRoleCommand(request);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Role name contains invalid characters*");
    }

    [Fact(DisplayName = "PERM_015 - Cập nhật description của role thành công")]
    public async Task UpdateRole_UpdateDescription_ReturnsSuccess()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
        var roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStoreMock.Object, null, null, null, null);

        var permissionRepoMock = new Mock<IPermissionReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var role = new ApplicationRole { Id = roleId, Name = "Manager", Description = "Old description" };
        roleManagerMock.Setup(x => x.FindByNameAsync("Manager"))
            .ReturnsAsync(role);
        roleManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);

        var handler = new UpdateRoleCommandHandler(roleManagerMock.Object, permissionRepoMock.Object, roleUpdateRepoMock.Object, unitOfWorkMock.Object);
        var request = new UpdateRoleRequest
        {
            Description = "Updated description",
            Permissions = null!
        };
        var command = new UpdateRoleCommand("Manager", request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RoleName.Should().Be("Manager");
        result.Description.Should().Be("Updated description");
    }

    [Fact(DisplayName = "PERM_016 - Cập nhật permissions của role thành công")]
    public async Task UpdateRole_UpdatePermissions_ReturnsSuccess()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
        var roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStoreMock.Object, null, null, null, null);

        var permissionRepoMock = new Mock<IPermissionReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var role = new ApplicationRole { Id = roleId, Name = "Manager", Description = "Test" };
        roleManagerMock.Setup(x => x.FindByNameAsync("Manager"))
            .ReturnsAsync(role);

        var oldPermissions = new List<RolePermission>
        {
            new() { RoleId = roleId, PermissionId = 1 },
            new() { RoleId = roleId, PermissionId = 2 }
        };

        var newPermissions = new List<PermissionEntity>
        {
            new() { Id = 3, Name = PermissionsList.Products.View },
            new() { Id = 4, Name = PermissionsList.Products.Create }
        };

        var roleReadRepoMock = new Mock<IRoleReadRepository>();
        roleReadRepoMock.Setup(x => x.GetRolePermissionsByRoleIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldPermissions);

        permissionRepoMock.Setup(x => x.GetPermissionsByNamesAsync(
            It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newPermissions);

        var handler = new UpdateRoleCommandHandler(roleManagerMock.Object, permissionRepoMock.Object, roleUpdateRepoMock.Object, unitOfWorkMock.Object);
        var request = new UpdateRoleRequest
        {
            Description = null,
            Permissions = [PermissionsList.Products.View, PermissionsList.Products.Create]
        };
        var command = new UpdateRoleCommand("Manager", request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RoleName.Should().Be("Manager");
        roleUpdateRepoMock.Verify(x => x.RemovePermissionsFromRole(It.IsAny<IEnumerable<RolePermission>>()), Times.Once);
        roleUpdateRepoMock.Verify(x => x.AddPermissionsToRoleAsync(It.IsAny<IEnumerable<RolePermission>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PERM_017 - Cập nhật role với body rỗng")]
    public async Task UpdateRole_EmptyBody_KeepsExistingData()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
        var roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStoreMock.Object, null, null, null, null);

        var permissionRepoMock = new Mock<IPermissionReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var role = new ApplicationRole { Id = roleId, Name = "Manager", Description = "Original description" };
        roleManagerMock.Setup(x => x.FindByNameAsync("Manager"))
            .ReturnsAsync(role);

        var handler = new UpdateRoleCommandHandler(roleManagerMock.Object, permissionRepoMock.Object, roleUpdateRepoMock.Object, unitOfWorkMock.Object);
        var request = new UpdateRoleRequest
        {
            Description = null,
            Permissions = []
        };
        var command = new UpdateRoleCommand("Manager", request);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.RoleName.Should().Be("Manager");
        result.Description.Should().Be("Original description");
        roleUpdateRepoMock.Verify(x => x.RemovePermissionsFromRole(It.IsAny<IEnumerable<RolePermission>>()), Times.Never);
        roleUpdateRepoMock.Verify(x => x.AddPermissionsToRoleAsync(It.IsAny<IEnumerable<RolePermission>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PERM_018 - Cập nhật role không tồn tại")]
    public async Task UpdateRole_NonExistentRole_ThrowsNotFoundException()
    {
        // Arrange
        var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
        var roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStoreMock.Object, null, null, null, null);

        var permissionRepoMock = new Mock<IPermissionReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        roleManagerMock.Setup(x => x.FindByNameAsync("NonExistentRole"))
            .ReturnsAsync((ApplicationRole?)null);

        var handler = new UpdateRoleCommandHandler(roleManagerMock.Object, permissionRepoMock.Object, roleUpdateRepoMock.Object, unitOfWorkMock.Object);
        var request = new UpdateRoleRequest { Description = "Test" };
        var command = new UpdateRoleCommand("NonExistentRole", request);

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*Role not found*");
    }

    [Fact(DisplayName = "PERM_019 - Xóa role thành công")]
    public async Task DeleteRole_ValidRole_ReturnsSuccess()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
        var roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStoreMock.Object, null, null, null, null);

        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        var protectedEntityServiceMock = new Mock<IProtectedEntityManagerService>();

        var role = new ApplicationRole { Id = roleId, Name = "OldRole" };
        roleManagerMock.Setup(x => x.FindByNameAsync("OldRole"))
            .ReturnsAsync(role);
        roleManagerMock.Setup(x => x.DeleteAsync(role))
            .ReturnsAsync(IdentityResult.Success);

        var handler = new DeleteRoleCommandHandler(roleManagerMock.Object, userManagerMock.Object, protectedEntityServiceMock.Object);
        var command = new DeleteRoleCommand("OldRole");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Contain("successfully");
        roleManagerMock.Verify(x => x.DeleteAsync(role), Times.Once);
    }

    [Fact(DisplayName = "PERM_020 - Xóa nhiều roles thành công")]
    public async Task DeleteMultipleRoles_ValidRoles_ReturnsSuccess()
    {
        // Arrange
        var roleStoreMock = new Mock<IRoleStore<ApplicationRole>>();
        var roleManagerMock = new Mock<RoleManager<ApplicationRole>>(
            roleStoreMock.Object, null, null, null, null);

        var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        var userManagerMock = new Mock<UserManager<ApplicationUser>>(
            userStoreMock.Object, null, null, null, null, null, null, null, null);

        var protectedEntityServiceMock = new Mock<IProtectedEntityManagerService>();

        var role1 = new ApplicationRole { Id = Guid.NewGuid(), Name = "Role1" };
        var role2 = new ApplicationRole { Id = Guid.NewGuid(), Name = "Role2" };
        var role3 = new ApplicationRole { Id = Guid.NewGuid(), Name = "Role3" };

        roleManagerMock.Setup(x => x.FindByNameAsync("Role1")).ReturnsAsync(role1);
        roleManagerMock.Setup(x => x.FindByNameAsync("Role2")).ReturnsAsync(role2);
        roleManagerMock.Setup(x => x.FindByNameAsync("Role3")).ReturnsAsync(role3);

        roleManagerMock.Setup(x => x.DeleteAsync(It.IsAny<ApplicationRole>()))
            .ReturnsAsync(IdentityResult.Success);

        var handler = new DeleteMultipleRolesCommandHandler(roleManagerMock.Object, userManagerMock.Object, protectedEntityServiceMock.Object);
        var command = new DeleteMultipleRolesCommand(["Role1", "Role2", "Role3"]);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Message.Should().Contain("3");
        result.Message.Should().Contain("successfully");
        roleManagerMock.Verify(x => x.DeleteAsync(It.IsAny<ApplicationRole>()), Times.Exactly(3));
    }
}
