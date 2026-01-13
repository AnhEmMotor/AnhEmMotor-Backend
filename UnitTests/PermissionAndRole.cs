using Application.ApiContracts.Permission.Responses;
using Application.Features.Permissions.Commands.CreateRole;
using Application.Features.Permissions.Commands.DeleteMultipleRoles;
using Application.Features.Permissions.Commands.DeleteRole;
using Application.Features.Permissions.Queries.GetAllPermissions;
using Application.Features.Permissions.Queries.GetAllRoles;
using Application.Features.Permissions.Queries.GetMyPermissions;
using Application.Features.Permissions.Queries.GetRolePermissions;
using Application.Features.Permissions.Queries.GetUserPermissionsById;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Repositories.Role;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Domain.Constants.Permission;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using PermissionEntity = Domain.Entities.Permission;

namespace UnitTests;

public class PermissionAndRole
{
#pragma warning disable CRR0035
    [Fact(DisplayName = "PERM_001 - Lấy tất cả permissions thành công")]
    public async Task GetAllPermissions_NoParams_ReturnsGroupedPermissions()
    {
        // Arrange
        var handler = new GetAllPermissionsQueryHandler();
        var query = new GetAllPermissionsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().BeOfType<Dictionary<string, List<PermissionResponse>>>();
        result.Value.Should().ContainKey("Brands");
        result.Value.Should().ContainKey("Products");
        result.Value.Should().ContainKey("Roles");
    }

    [Fact(DisplayName = "PERM_002 - Lấy permissions của user hiện tại thành công")]
    public async Task GetMyPermissions_UserWithRoleAndPermissions_ReturnsPermissions()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var userReadRepoMock = new Mock<IUserReadRepository>();
        var roleReadRepoMock = new Mock<IRoleReadRepository>();

        var user = new ApplicationUser { Id = userId, UserName = "testuser", Email = "test@example.com" };
        userReadRepoMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        userReadRepoMock.Setup(x => x.GetRolesOfUserAsync(user, CancellationToken.None))
            .ReturnsAsync(["Manager"]);

        var roles = new List<ApplicationRole> { new() { Id = Guid.NewGuid(), Name = "Manager" } };
        roleReadRepoMock.Setup(x => x.GetRolesByNameAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        var permissions = new List<string>
        {
            PermissionsList.Brands.View,
            PermissionsList.Brands.Create,
            PermissionsList.Products.View,
            PermissionsList.Products.Create,
            PermissionsList.Roles.View
        };

        roleReadRepoMock.Setup(x => x.GetPermissionsNameByRoleIdAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var handler = new GetMyPermissionsQueryHandler(roleReadRepoMock.Object, userReadRepoMock.Object);
        var query = new GetMyPermissionsQuery { UserId = userId.ToString() };

        // Act
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.Should().NotBeNull();
        result.Value.Permissions.Should().HaveCount(5);
        result.Value.UserId.Should().Be(userId);
    }

    [Fact(DisplayName = "PERM_003 - Lấy permissions của user không có role nào")]
    public async Task GetMyPermissions_UserWithoutRoles_ReturnsEmptyPermissions()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var userReadRepoMock = new Mock<IUserReadRepository>();
        var roleReadRepoMock = new Mock<IRoleReadRepository>();

        var user = new ApplicationUser { Id = userId, UserName = "testuser", Email = "test@example.com" };
        userReadRepoMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        userReadRepoMock.Setup(x => x.GetRolesOfUserAsync(user, CancellationToken.None))
            .ReturnsAsync([]);

        roleReadRepoMock.Setup(x => x.GetRolesByNameAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetMyPermissionsQueryHandler(roleReadRepoMock.Object, userReadRepoMock.Object);
        var query = new GetMyPermissionsQuery { UserId = userId.ToString() };

        // Act
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.Should().NotBeNull();
        result.Value.Permissions.Should().BeEmpty();
        result.Value.UserId.Should().Be(userId);
    }

    [Fact(DisplayName = "PERM_004 - Lấy permissions của user bằng userId hợp lệ")]
    public async Task GetUserPermissionsById_ValidUserId_ReturnsPermissions()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var userReadRepoMock = new Mock<IUserReadRepository>();
        var roleReadRepoMock = new Mock<IRoleReadRepository>();

        var user = new ApplicationUser { Id = userId, UserName = "staffuser", Email = "staff@test.com" };
        userReadRepoMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        userReadRepoMock.Setup(x => x.GetRolesOfUserAsync(user, CancellationToken.None))
            .ReturnsAsync(["Staff"]);

        var roles = new List<ApplicationRole> { new() { Id = Guid.NewGuid(), Name = "Staff" } };
        roleReadRepoMock.Setup(x => x.GetRolesByNameAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        var permissions = new List<string>
        {
            PermissionsList.Products.View,
            PermissionsList.Brands.View,
            PermissionsList.Files.View
        };

        roleReadRepoMock.Setup(x => x.GetPermissionsNameByRoleIdAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var handler = new GetUserPermissionsByIdQueryHandler(userReadRepoMock.Object, roleReadRepoMock.Object);
        var query = new GetUserPermissionsByIdQuery { UserId = userId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.Should().NotBeNull();
        result.Value.UserId.Should().Be(userId);
        result.Value.Permissions.Should().HaveCount(3);
        result.Value.Email.Should().Be("staff@test.com");
    }

    [Fact(DisplayName = "PERM_005 - Lấy permissions của user bằng userId không tồn tại")]
    public async Task GetUserPermissionsById_InvalidUserId_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var userReadRepoMock = new Mock<IUserReadRepository>();
        var roleReadRepoMock = new Mock<IRoleReadRepository>();

        userReadRepoMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var handler = new GetUserPermissionsByIdQueryHandler(userReadRepoMock.Object, roleReadRepoMock.Object);
        var query = new GetUserPermissionsByIdQuery { UserId = userId };

        // Act
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PERM_006 - Lấy permissions của role hợp lệ")]
    public async Task GetRolePermissions_ValidRoleName_ReturnsPermissions()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var roleReadRepoMock = new Mock<IRoleReadRepository>();

        var role = new ApplicationRole { Id = roleId, Name = "Manager" };
        roleReadRepoMock.Setup(x => x.GetRoleByNameAsync("Manager", CancellationToken.None))
            .ReturnsAsync(role);

        var permissions = new List<string>
        {
            PermissionsList.Brands.View,
            PermissionsList.Brands.Create,
            PermissionsList.Brands.Edit,
            PermissionsList.Brands.Delete
        };

        roleReadRepoMock.Setup(x => x.GetPermissionsNameByRoleIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var handler = new GetRolePermissionsQueryHandler(roleReadRepoMock.Object);
        var query = new GetRolePermissionsQuery { RoleName = "Manager" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().HaveCount(4);
    }

    [Fact(DisplayName = "PERM_007 - Lấy permissions của role không tồn tại")]
    public async Task GetRolePermissions_InvalidRoleName_ThrowsNotFoundException()
    {
        // Arrange
        var roleReadRepoMock = new Mock<IRoleReadRepository>();

        roleReadRepoMock.Setup(x => x.GetRoleByNameAsync("NonExistentRole", CancellationToken.None))
            .ReturnsAsync((ApplicationRole?)null);

        var handler = new GetRolePermissionsQueryHandler(roleReadRepoMock.Object);
        var query = new GetRolePermissionsQuery { RoleName = "NonExistentRole" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PERM_008 - Lấy permissions của role với tên có khoảng trắng đầu/cuối")]
    public async Task GetRolePermissions_RoleNameWithWhitespace_TrimsAndReturnsPermissions()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var roleReadRepoMock = new Mock<IRoleReadRepository>();

        var role = new ApplicationRole { Id = roleId, Name = "Manager" };
        roleReadRepoMock.Setup(x => x.GetRoleByNameAsync("Manager", CancellationToken.None))
            .ReturnsAsync(role);

        var permissions = new List<string>
        {
            PermissionsList.Brands.View,
            PermissionsList.Brands.Create
        };

        roleReadRepoMock.Setup(x => x.GetPermissionsNameByRoleIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var handler = new GetRolePermissionsQueryHandler(roleReadRepoMock.Object);
        var query = new GetRolePermissionsQuery { RoleName = "  Manager  " };

        // Act
        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
    }

    [Fact(DisplayName = "PERM_009 - Tạo role mới thành công với tên và permissions hợp lệ")]
    public async Task CreateRole_ValidData_ReturnsSuccess()
    {
        // Arrange
        var roleReadRepoMock = new Mock<IRoleReadRepository>();
        var roleInsertRepoMock = new Mock<IRoleInsertRepository>();
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

        roleReadRepoMock.Setup(x => x.IsRoleExistAsync("NewRole", CancellationToken.None))
            .ReturnsAsync(false);

        var createdRole = new ApplicationRole { Id = Guid.NewGuid(), Name = "NewRole", Description = "Test role" };
        roleInsertRepoMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationRole>(), CancellationToken.None))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<ApplicationRole>(r => { r.Id = createdRole.Id; });

        var handler = new CreateRoleCommandHandler(roleReadRepoMock.Object, roleInsertRepoMock.Object, permissionRepoMock.Object, roleUpdateRepoMock.Object, unitOfWorkMock.Object);
        var command = new CreateRoleCommand { RoleName = "NewRole", Description = "Test role", Permissions = [PermissionsList.Brands.View, PermissionsList.Products.View] };

        // Act
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.Should().NotBeNull();
        result.Value.RoleName.Should().Be("NewRole");
        result.Value.Description.Should().Be("Test role");
        result.Value.Permissions.Should().HaveCount(2);
        result.Value.RoleId.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "PERM_010 - Tạo role mới với tên đã tồn tại")]
    public async Task CreateRole_DuplicateName_ThrowsException()
    {
        // Arrange
        var roleReadRepoMock = new Mock<IRoleReadRepository>();
        var roleInsertRepoMock = new Mock<IRoleInsertRepository>();
        var permissionRepoMock = new Mock<IPermissionReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        roleReadRepoMock.Setup(x => x.IsRoleExistAsync("Manager", CancellationToken.None))
            .ReturnsAsync(true);

        var handler = new CreateRoleCommandHandler(roleReadRepoMock.Object, roleInsertRepoMock.Object, permissionRepoMock.Object, roleUpdateRepoMock.Object, unitOfWorkMock.Object);
        var command = new CreateRoleCommand {
            RoleName = "Manager",
            Description = "Duplicate",
            Permissions = [PermissionsList.Brands.View]
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PERM_011 - Tạo role mới với tên rỗng hoặc null - Trường hợp 1: RoleName rỗng")]
    public async Task CreateRole_EmptyRoleName_ThrowsValidationException()
    {
        // Arrange
        var roleReadRepoMock = new Mock<IRoleReadRepository>();
        var roleInsertRepoMock = new Mock<IRoleInsertRepository>();
        var permissionRepoMock = new Mock<IPermissionReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new CreateRoleCommandHandler(roleReadRepoMock.Object, roleInsertRepoMock.Object, permissionRepoMock.Object, roleUpdateRepoMock.Object, unitOfWorkMock.Object);
        var command = new CreateRoleCommand {
            RoleName = "",
            Description = "Test",
            Permissions = [PermissionsList.Brands.View]
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PERM_011 - Tạo role mới với tên rỗng hoặc null - Trường hợp 2: RoleName null")]
    public async Task CreateRole_NullRoleName_ThrowsValidationException()
    {
        // Arrange
        var roleReadRepoMock = new Mock<IRoleReadRepository>();
        var roleInsertRepoMock = new Mock<IRoleInsertRepository>();
        var permissionRepoMock = new Mock<IPermissionReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new CreateRoleCommandHandler(roleReadRepoMock.Object, roleInsertRepoMock.Object, permissionRepoMock.Object, roleUpdateRepoMock.Object, unitOfWorkMock.Object);
        var command = new CreateRoleCommand
        {
            RoleName = null!,
            Description = "Test",
            Permissions = [PermissionsList.Brands.View]
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PERM_012 - Tạo role mới với danh sách permissions rỗng")]
    public async Task CreateRole_EmptyPermissions_ThrowsValidationException()
    {
        // Arrange
        var roleReadRepoMock = new Mock<IRoleReadRepository>();
        var roleInsertRepoMock = new Mock<IRoleInsertRepository>();
        var permissionRepoMock = new Mock<IPermissionReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new CreateRoleCommandHandler(roleReadRepoMock.Object, roleInsertRepoMock.Object, permissionRepoMock.Object, roleUpdateRepoMock.Object, unitOfWorkMock.Object);
        var command = new CreateRoleCommand {
            RoleName = "EmptyPermRole",
            Description = "Test",
            Permissions = []
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PERM_013 - Tạo role mới với permissions không tồn tại")]
    public async Task CreateRole_InvalidPermissions_ThrowsValidationException()
    {
        // Arrange
        var roleReadRepoMock = new Mock<IRoleReadRepository>();
        var roleInsertRepoMock = new Mock<IRoleInsertRepository>();
        var permissionRepoMock = new Mock<IPermissionReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        roleReadRepoMock.Setup(x => x.IsRoleExistAsync("InvalidPermRole", CancellationToken.None))
            .ReturnsAsync(false);

        permissionRepoMock.Setup(x => x.GetPermissionsByNamesAsync(
            It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new CreateRoleCommandHandler(roleReadRepoMock.Object, roleInsertRepoMock.Object, permissionRepoMock.Object, roleUpdateRepoMock.Object, unitOfWorkMock.Object);
        var command = new CreateRoleCommand {
            RoleName = "InvalidPermRole",
            Description = "Test",
            Permissions = ["Permissions.Invalid.Permission", "Permissions.NotExist.Test"]
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PERM_014 - Tạo role mới với tên chứa ký tự đặc biệt")]
    public async Task CreateRole_RoleNameWithSpecialCharacters_ThrowsValidationException()
    {
        // Arrange
        var roleReadRepoMock = new Mock<IRoleReadRepository>();
        var roleInsertRepoMock = new Mock<IRoleInsertRepository>();
        var permissionRepoMock = new Mock<IPermissionReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new CreateRoleCommandHandler(roleReadRepoMock.Object, roleInsertRepoMock.Object, permissionRepoMock.Object, roleUpdateRepoMock.Object, unitOfWorkMock.Object);
        var command = new CreateRoleCommand {
            RoleName = "Role@#$%",
            Description = "Test",
            Permissions = [PermissionsList.Brands.View]
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PERM_015 - Cập nhật description của role thành công")]
    public async Task UpdateRole_UpdateDescription_ReturnsSuccess()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var roleReadRepoMock = new Mock<IRoleReadRepository>();
        var permissionRepoMock = new Mock<IPermissionReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var role = new ApplicationRole { Id = roleId, Name = "Manager", Description = "Old description" };
        roleReadRepoMock.Setup(x => x.GetRoleByNameAsync("Manager", CancellationToken.None))
            .ReturnsAsync(role);

        var handler = new Application.Features.Permissions.Commands.UpdateRole.UpdateRoleCommandHandler(roleReadRepoMock.Object, roleUpdateRepoMock.Object, permissionRepoMock.Object, unitOfWorkMock.Object);
        var command = new Application.Features.Permissions.Commands.UpdateRole.UpdateRoleCommand() { RoleName = "Manager",
            Description = "Updated description",
            Permissions = null!
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PERM_016 - Cập nhật permissions của role thành công")]
    public async Task UpdateRole_UpdatePermissions_ReturnsSuccess()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var roleReadRepoMock = new Mock<IRoleReadRepository>();
        var permissionRepoMock = new Mock<IPermissionReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var role = new ApplicationRole { Id = roleId, Name = "Manager", Description = "Test" };
        roleReadRepoMock.Setup(x => x.GetRoleByNameAsync("Manager", CancellationToken.None))
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

        roleReadRepoMock.Setup(x => x.GetRolesPermissionByRoleIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldPermissions);

        permissionRepoMock.Setup(x => x.GetPermissionsByNamesAsync(
            It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newPermissions);

        var handler = new Application.Features.Permissions.Commands.UpdateRole.UpdateRoleCommandHandler(roleReadRepoMock.Object, roleUpdateRepoMock.Object, permissionRepoMock.Object, unitOfWorkMock.Object);
        
        var command = new Application.Features.Permissions.Commands.UpdateRole.UpdateRoleCommand()
        {
            RoleName = "Manager",
            Description = null,
            Permissions = [PermissionsList.Products.View, PermissionsList.Products.Create]
        };


        // Act
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.Should().NotBeNull();
        roleUpdateRepoMock.Verify(x => x.RemovePermissionsFromRole(It.IsAny<IEnumerable<RolePermission>>()), Times.Once);
        roleUpdateRepoMock.Verify(x => x.AddPermissionsToRoleAsync(It.IsAny<IEnumerable<RolePermission>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "PERM_017 - Cập nhật role với body rỗng")]
    public async Task UpdateRole_EmptyBody_KeepsExistingData()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var roleReadRepoMock = new Mock<IRoleReadRepository>();
        var permissionRepoMock = new Mock<IPermissionReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var role = new ApplicationRole { Id = roleId, Name = "Manager", Description = "Original description" };
        roleReadRepoMock.Setup(x => x.GetRoleByNameAsync("Manager", CancellationToken.None))
            .ReturnsAsync(role);

        var handler = new Application.Features.Permissions.Commands.UpdateRole.UpdateRoleCommandHandler(roleReadRepoMock.Object, roleUpdateRepoMock.Object, permissionRepoMock.Object, unitOfWorkMock.Object);
        var command = new Application.Features.Permissions.Commands.UpdateRole.UpdateRoleCommand()
        {
            RoleName = "Manager",
            Description = null,
            Permissions = []
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.Should().NotBeNull();
        roleUpdateRepoMock.Verify(x => x.RemovePermissionsFromRole(It.IsAny<IEnumerable<RolePermission>>()), Times.Never);
        roleUpdateRepoMock.Verify(x => x.AddPermissionsToRoleAsync(It.IsAny<IEnumerable<RolePermission>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "PERM_018 - Cập nhật role không tồn tại")]
    public async Task UpdateRole_NonExistentRole_ThrowsNotFoundException()
    {
        // Arrange
        var roleReadRepoMock = new Mock<IRoleReadRepository>();
        var permissionRepoMock = new Mock<IPermissionReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        roleReadRepoMock.Setup(x => x.GetRoleByNameAsync("NonExistentRole", CancellationToken.None))
            .ReturnsAsync((ApplicationRole?)null);

        var handler = new Application.Features.Permissions.Commands.UpdateRole.UpdateRoleCommandHandler(roleReadRepoMock.Object, roleUpdateRepoMock.Object, permissionRepoMock.Object, unitOfWorkMock.Object);
        var command = new Application.Features.Permissions.Commands.UpdateRole.UpdateRoleCommand()
        {
            RoleName = "NonExistentRole",
            Description = "Test"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PERM_019 - Xóa role thành công")]
    public async Task DeleteRole_ValidRole_ReturnsSuccess()
    {
        // Arrange
        var roleId = Guid.NewGuid();
        var roleReadRepoMock = new Mock<IRoleReadRepository>();
        var roleDeleteRepoMock = new Mock<IRoleDeleteRepository>();
        var protectedEntityServiceMock = new Mock<IProtectedEntityManagerService>();

        var role = new ApplicationRole { Id = roleId, Name = "OldRole" };
        roleReadRepoMock.Setup(x => x.GetRoleByNameAsync("OldRole", CancellationToken.None))
            .ReturnsAsync(role);
        roleReadRepoMock.Setup(x => x.GetUsersInRoleAsync("OldRole", CancellationToken.None))
            .ReturnsAsync([]);
        roleDeleteRepoMock.Setup(x => x.DeleteAsync(role, CancellationToken.None))
            .ReturnsAsync(IdentityResult.Success);

        var handler = new DeleteRoleCommandHandler(roleReadRepoMock.Object, roleDeleteRepoMock.Object, protectedEntityServiceMock.Object);
        var command = new DeleteRoleCommand() { RoleName = "OldRole" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.Should().NotBeNull();
        result.Value.Message.Should().Contain("successfully");
        roleDeleteRepoMock.Verify(x => x.DeleteAsync(role, CancellationToken.None), Times.Once);
    }

    [Fact(DisplayName = "PERM_020 - Xóa nhiều roles thành công")]
    public async Task DeleteMultipleRoles_ValidRoles_ReturnsSuccess()
    {
        // Arrange
        var roleReadRepoMock = new Mock<IRoleReadRepository>();
        var userReadRepoMock = new Mock<IUserReadRepository>();
        var roleDeleteRepoMock = new Mock<IRoleDeleteRepository>();
        var protectedEntityServiceMock = new Mock<IProtectedEntityManagerService>();

        var role1 = new ApplicationRole { Id = Guid.NewGuid(), Name = "Role1" };
        var role2 = new ApplicationRole { Id = Guid.NewGuid(), Name = "Role2" };
        var role3 = new ApplicationRole { Id = Guid.NewGuid(), Name = "Role3" };

        roleReadRepoMock.Setup(x => x.GetRoleByNameAsync("Role1", CancellationToken.None)).ReturnsAsync(role1);
        roleReadRepoMock.Setup(x => x.GetRoleByNameAsync("Role2", CancellationToken.None)).ReturnsAsync(role2);
        roleReadRepoMock.Setup(x => x.GetRoleByNameAsync("Role3", CancellationToken.None)).ReturnsAsync(role3);

        userReadRepoMock.Setup(x => x.GetUsersInRoleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        roleDeleteRepoMock.Setup(x => x.DeleteAsync(It.IsAny<ApplicationRole>(), CancellationToken.None))
            .ReturnsAsync(IdentityResult.Success);

        var handler = new DeleteMultipleRolesCommandHandler(roleReadRepoMock.Object, userReadRepoMock.Object, roleDeleteRepoMock.Object, protectedEntityServiceMock.Object);
        var command = new DeleteMultipleRolesCommand() { RoleNames = ["Role1", "Role2", "Role3"] };

        // Act
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.Should().NotBeNull();
        result.Value.Message.Should().Contain("3");
        result.Value.Message.Should().Contain("successfully");
        roleDeleteRepoMock.Verify(x => x.DeleteAsync(It.IsAny<ApplicationRole>(), CancellationToken.None), Times.Exactly(3));
    }
#pragma warning restore CRR0035
}
