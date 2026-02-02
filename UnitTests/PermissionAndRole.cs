using Application.ApiContracts.Permission.Responses;
using Application.Features.Permissions.Commands.CreateRole;
using Application.Features.Permissions.Commands.DeleteMultipleRoles;
using Application.Features.Permissions.Commands.DeleteRole;
using Application.Features.Permissions.Queries.GetAllPermissions;
using Application.Features.Permissions.Queries.GetMyPermissions;
using Application.Features.Permissions.Queries.GetRolePermissions;
using Application.Features.Permissions.Queries.GetUserPermissionsById;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Repositories.Role;
using Application.Interfaces.Repositories.User;
using Application.Interfaces.Services;
using Application.Features.Permissions.Commands.UpdateRole;
using Domain.Constants.Permission;
using Domain.Entities;
using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.ComponentModel.DataAnnotations;
using PermissionEntity = Domain.Entities.Permission;

namespace UnitTests;

public class PermissionAndRole
{
#pragma warning disable IDE0079 
#pragma warning disable CRR0035
    [Fact(DisplayName = "PERM_001 - Lấy tất cả permissions thành công")]
    public async Task GetAllPermissions_NoParams_ReturnsGroupedPermissions()
    {
        var handler = new GetAllPermissionsQueryHandler();
        var query = new GetAllPermissionsQuery();

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        result.Value.Should().BeOfType<Dictionary<string, List<PermissionResponse>>>();
        result.Value.Should().ContainKey("Brands");
        result.Value.Should().ContainKey("Products");
        result.Value.Should().ContainKey("Roles");
    }

    [Fact(DisplayName = "PERM_002 - Lấy permissions của user hiện tại thành công")]
    public async Task GetMyPermissions_UserWithRoleAndPermissions_ReturnsPermissions()
    {
        var userId = Guid.NewGuid();

        var userReadRepoMock = new Mock<IUserReadRepository>();
        var roleReadRepoMock = new Mock<IRoleReadRepository>();

        var user = new ApplicationUser { Id = userId, UserName = "testuser", Email = "test@example.com" };
        userReadRepoMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        userReadRepoMock.Setup(x => x.GetRolesOfUserAsync(user, CancellationToken.None)).ReturnsAsync([ "Manager" ]);

        var roles = new List<ApplicationRole> { new() { Id = Guid.NewGuid(), Name = "Manager" } };
        roleReadRepoMock.Setup(
            x => x.GetRolesByNameAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        var permissions = new List<string>
        {
            PermissionsList.Brands.View,
            PermissionsList.Brands.Create,
            PermissionsList.Products.View,
            PermissionsList.Products.Create,
            PermissionsList.Roles.View
        };

        roleReadRepoMock.Setup(
            x => x.GetPermissionsNameByRoleIdAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var handler = new GetMyPermissionsQueryHandler(roleReadRepoMock.Object, userReadRepoMock.Object);
        var query = new GetMyPermissionsQuery { UserId = userId.ToString() };

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        result.Value.Permissions.Should().HaveCount(5);
        result.Value.UserId.Should().Be(userId);
    }

    [Fact(DisplayName = "PERM_003 - Lấy permissions của user không có role nào")]
    public async Task GetMyPermissions_UserWithoutRoles_ReturnsEmptyPermissions()
    {
        var userId = Guid.NewGuid();

        var userReadRepoMock = new Mock<IUserReadRepository>();
        var roleReadRepoMock = new Mock<IRoleReadRepository>();

        var user = new ApplicationUser { Id = userId, UserName = "testuser", Email = "test@example.com" };
        userReadRepoMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        userReadRepoMock.Setup(x => x.GetRolesOfUserAsync(user, CancellationToken.None)).ReturnsAsync([]);

        roleReadRepoMock.Setup(
            x => x.GetRolesByNameAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        roleReadRepoMock.Setup(
            x => x.GetPermissionsNameByRoleIdAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var handler = new GetMyPermissionsQueryHandler(roleReadRepoMock.Object, userReadRepoMock.Object);
        var query = new GetMyPermissionsQuery { UserId = userId.ToString() };

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        result.Value.Permissions.Should().BeEmpty();
        result.Value.UserId.Should().Be(userId);
    }

    [Fact(DisplayName = "PERM_004 - Lấy permissions của user bằng userId hợp lệ")]
    public async Task GetUserPermissionsById_ValidUserId_ReturnsPermissions()
    {
        var userId = Guid.NewGuid();

        var userReadRepoMock = new Mock<IUserReadRepository>();
        var roleReadRepoMock = new Mock<IRoleReadRepository>();

        var user = new ApplicationUser { Id = userId, UserName = "staffuser", Email = "staff@test.com" };
        userReadRepoMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        userReadRepoMock.Setup(x => x.GetRolesOfUserAsync(user, CancellationToken.None)).ReturnsAsync([ "Staff" ]);

        var roles = new List<ApplicationRole> { new() { Id = Guid.NewGuid(), Name = "Staff" } };
        roleReadRepoMock.Setup(
            x => x.GetRolesByNameAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        var permissions = new List<string>
        {
            PermissionsList.Products.View,
            PermissionsList.Brands.View,
            PermissionsList.Files.View
        };

        roleReadRepoMock.Setup(
            x => x.GetPermissionsNameByRoleIdAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var handler = new GetUserPermissionsByIdQueryHandler(userReadRepoMock.Object, roleReadRepoMock.Object);
        var query = new GetUserPermissionsByIdQuery { UserId = userId };

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        result.Value.UserId.Should().Be(userId);
        result.Value.Permissions.Should().HaveCount(3);
        result.Value.Email.Should().Be("staff@test.com");
    }

    [Fact(DisplayName = "PERM_005 - Lấy permissions của user bằng userId không tồn tại")]
    public async Task GetUserPermissionsById_InvalidUserId_ThrowsNotFoundException()
    {
        var userId = Guid.NewGuid();

        var userReadRepoMock = new Mock<IUserReadRepository>();
        var roleReadRepoMock = new Mock<IRoleReadRepository>();

        userReadRepoMock.Setup(x => x.FindUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApplicationUser?)null);

        var handler = new GetUserPermissionsByIdQueryHandler(userReadRepoMock.Object, roleReadRepoMock.Object);
        var query = new GetUserPermissionsByIdQuery { UserId = userId };

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PERM_006 - Lấy permissions của role hợp lệ")]
    public async Task GetRolePermissions_ValidRoleName_ReturnsPermissions()
    {
        var roleId = Guid.NewGuid();
        var roleReadRepoMock = new Mock<IRoleReadRepository>();

        var role = new ApplicationRole { Id = roleId, Name = "Manager" };
        roleReadRepoMock.Setup(x => x.GetRoleByNameAsync("Manager", CancellationToken.None)).ReturnsAsync(role);

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

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        result.Value.Should().HaveCount(4);
    }

    [Fact(DisplayName = "PERM_007 - Lấy permissions của role không tồn tại")]
    public async Task GetRolePermissions_InvalidRoleName_ThrowsNotFoundException()
    {
        var roleReadRepoMock = new Mock<IRoleReadRepository>();

        roleReadRepoMock.Setup(x => x.GetRoleByNameAsync("NonExistentRole", CancellationToken.None))
            .ReturnsAsync((ApplicationRole?)null);

        var handler = new GetRolePermissionsQueryHandler(roleReadRepoMock.Object);
        var query = new GetRolePermissionsQuery { RoleName = "NonExistentRole" };

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PERM_008 - Lấy permissions của role với tên có khoảng trắng đầu/cuối")]
    public async Task GetRolePermissions_RoleNameWithWhitespace_TrimsAndReturnsPermissions()
    {
        var roleId = Guid.NewGuid();
        var roleReadRepoMock = new Mock<IRoleReadRepository>();

        var role = new ApplicationRole { Id = roleId, Name = "Manager" };
        roleReadRepoMock.Setup(x => x.GetRoleByNameAsync("Manager", CancellationToken.None)).ReturnsAsync(role);

        var permissions = new List<string> { PermissionsList.Brands.View, PermissionsList.Brands.Create };

        roleReadRepoMock.Setup(x => x.GetPermissionsNameByRoleIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        var handler = new GetRolePermissionsQueryHandler(roleReadRepoMock.Object);
        var query = new GetRolePermissionsQuery { RoleName = "  Manager  " };

        var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
    }

    [Fact(DisplayName = "PERM_009 - Tạo role mới thành công với tên và permissions hợp lệ")]
    public async Task CreateRole_ValidData_ReturnsSuccess()
    {
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

        permissionRepoMock.Setup(
            x => x.GetPermissionsByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissions);

        roleReadRepoMock.Setup(x => x.IsRoleExistAsync("NewRole", CancellationToken.None)).ReturnsAsync(false);

        var createdRole = new ApplicationRole { Id = Guid.NewGuid(), Name = "NewRole", Description = "Test role" };
        roleInsertRepoMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationRole>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(IdentityResult.Success)
            .Callback<ApplicationRole, CancellationToken>(
                (role, token) =>
                {
                    role.Id = createdRole.Id;
                });

        var handler = new CreateRoleCommandHandler(
            roleReadRepoMock.Object,
            roleInsertRepoMock.Object,
            permissionRepoMock.Object,
            roleUpdateRepoMock.Object,
            unitOfWorkMock.Object);
        var command = new CreateRoleCommand
        {
            RoleName = "NewRole",
            Description = "Test role",
            Permissions = [ PermissionsList.Brands.View, PermissionsList.Products.View ]
        };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        result.Value.RoleName.Should().Be("NewRole");
        result.Value.Description.Should().Be("Test role");
        result.Value.Permissions.Should().HaveCount(2);
        result.Value.RoleId.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "PERM_010 - Tạo role mới với tên đã tồn tại")]
    public async Task CreateRole_DuplicateName_ThrowsException()
    {
        var roleReadRepoMock = new Mock<IRoleReadRepository>();
        var roleInsertRepoMock = new Mock<IRoleInsertRepository>();
        var permissionRepoMock = new Mock<IPermissionReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        roleReadRepoMock.Setup(x => x.IsRoleExistAsync("Manager", CancellationToken.None)).ReturnsAsync(true);

        var handler = new CreateRoleCommandHandler(
            roleReadRepoMock.Object,
            roleInsertRepoMock.Object,
            permissionRepoMock.Object,
            roleUpdateRepoMock.Object,
            unitOfWorkMock.Object);
        var command = new CreateRoleCommand
        {
            RoleName = "Manager",
            Description = "Duplicate",
            Permissions = [ PermissionsList.Brands.View ]
        };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PERM_011 - Trường hợp 1: RoleName rỗng")]
    public void RoleName_Empty_ShouldHaveError()
    {
        CreateRoleCommandValidator validator = new();
        var command = new CreateRoleCommand { RoleName = string.Empty };
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.RoleName);
    }

    [Fact(DisplayName = "PERM_011 - Trường hợp 2: RoleName null")]
    public void RoleName_Null_ShouldHaveError()
    {
        CreateRoleCommandValidator validator = new();
        var command = new CreateRoleCommand { RoleName = null! };
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.RoleName);
    }

    [Fact(DisplayName = "PERM_012 - Danh sách permissions rỗng")]
    public void Permissions_Empty_ShouldHaveError()
    {
        CreateRoleCommandValidator validator = new();
        var command = new CreateRoleCommand { RoleName = "ValidRole", Permissions = [] };
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Permissions);
    }

    [Fact(DisplayName = "PERM_013 - Permissions không tồn tại trong hệ thống")]
    public void Permissions_Invalid_ShouldHaveError()
    {
        CreateRoleCommandValidator validator = new();
        var command = new CreateRoleCommand
        {
            RoleName = "ValidRole",
            Permissions = ["Invalid.Permission.Name"]
        };
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Permissions);
    }

    [Fact(DisplayName = "PERM_014 - Tên chứa ký tự đặc biệt")]
    public void RoleName_SpecialChars_ShouldHaveError()
    {
        CreateRoleCommandValidator validator = new();
        var command = new CreateRoleCommand { RoleName = "Role@#$%", Permissions = ["Some.Valid.Perm"] };
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.RoleName);
    }

    [Fact(DisplayName = "PERM_015 - Cập nhật description của role thành công")]
    public async Task UpdateRole_UpdateDescription_ReturnsSuccess()
    {
        var roleId = Guid.NewGuid();
        var roleReadRepoMock = new Mock<IRoleReadRepository>();
        var permissionRepoMock = new Mock<IPermissionReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var role = new ApplicationRole { Id = roleId, Name = "Manager", Description = "Old description" };
        roleReadRepoMock.Setup(x => x.GetRoleByNameAsync("Manager", CancellationToken.None)).ReturnsAsync(role);

        var handler = new UpdateRoleCommandHandler(
            roleReadRepoMock.Object,
            roleUpdateRepoMock.Object,
            permissionRepoMock.Object,
            unitOfWorkMock.Object);
        var command = new UpdateRoleCommand()
        {
            RoleName = "Manager",
            Description = "Updated description",
            Permissions = null!
        };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "PERM_016 - Cập nhật permissions của role thành công")]
    public async Task UpdateRole_UpdatePermissions_ReturnsSuccess()
    {
        var roleId = Guid.NewGuid();
        var roleReadRepoMock = new Mock<IRoleReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        // Chỉ dùng DUY NHẤT một mock này
        var permissionReadRepoMock = new Mock<IPermissionReadRepository>();

        var role = new ApplicationRole { Id = roleId, Name = "Manager", Description = "Test" };
        roleReadRepoMock.Setup(x => x.GetRoleByNameAsync("Manager", CancellationToken.None)).ReturnsAsync(role);

        var oldPermissions = new List<RolePermission>
    {
        new() { RoleId = roleId, PermissionId = 1, Permission = new PermissionEntity { Id = 1, Name = "Old.Perm.1" } },
        new() { RoleId = roleId, PermissionId = 2, Permission = new PermissionEntity { Id = 2, Name = "Old.Perm.2" } }
    };

        roleReadRepoMock.Setup(x => x.GetRolesPermissionByRoleIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldPermissions);

        // Setup trả về danh sách permission mới khi Handler gọi GetPermissionsByNamesAsync
        var newPermissions = new List<PermissionEntity>
    {
        new() { Id = 3, Name = PermissionsList.Products.View },
        new() { Id = 4, Name = PermissionsList.Products.Create }
    };
        permissionReadRepoMock.Setup(x => x.GetPermissionsByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newPermissions);

        // Setup để vượt qua kiểm tra Orphaned (Dòng gây lỗi GroupBy)
        permissionReadRepoMock.Setup(x => x.GetRolePermissionsByPermissionIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([
                new() { PermissionId = 1, Permission = new PermissionEntity { Name = "Old.Perm.1" } },
            new() { PermissionId = 1 }, // Hai bản ghi -> không bị orphaned
            new() { PermissionId = 2, Permission = new PermissionEntity { Name = "Old.Perm.2" } },
            new() { PermissionId = 2 }
            ]);

        var handler = new UpdateRoleCommandHandler(
            roleReadRepoMock.Object,
            roleUpdateRepoMock.Object,
            permissionReadRepoMock.Object, // Truyền đúng đứa đã được Setup
            unitOfWorkMock.Object);

        var command = new UpdateRoleCommand()
        {
            RoleName = "Manager",
            Description = null,
            Permissions = [PermissionsList.Products.View, PermissionsList.Products.Create]
        };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        roleUpdateRepoMock.Verify(x => x.RemovePermissionsFromRole(It.IsAny<IEnumerable<RolePermission>>()), Times.Once);
        roleUpdateRepoMock.Verify(
            x => x.AddPermissionsToRoleAsync(It.IsAny<IEnumerable<RolePermission>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "PERM_017 - Cập nhật role với body rỗng")]
    public async Task UpdateRole_EmptyBody_KeepsExistingData()
    {
        var roleId = Guid.NewGuid();
        var roleReadRepoMock = new Mock<IRoleReadRepository>();
        var permissionRepoMock = new Mock<IPermissionReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var role = new ApplicationRole { Id = roleId, Name = "Manager", Description = "Original description" };
        roleReadRepoMock.Setup(x => x.GetRoleByNameAsync("Manager", CancellationToken.None)).ReturnsAsync(role);

        var handler = new UpdateRoleCommandHandler(
            roleReadRepoMock.Object,
            roleUpdateRepoMock.Object,
            permissionRepoMock.Object,
            unitOfWorkMock.Object);
        var command = new UpdateRoleCommand()
        {
            RoleName = "Manager",
            Description = null,
            Permissions = []
        };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        roleUpdateRepoMock.Verify(
            x => x.RemovePermissionsFromRole(It.IsAny<IEnumerable<RolePermission>>()),
            Times.Never);
        roleUpdateRepoMock.Verify(
            x => x.AddPermissionsToRoleAsync(It.IsAny<IEnumerable<RolePermission>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact(DisplayName = "PERM_018 - Cập nhật role không tồn tại")]
    public async Task UpdateRole_NonExistentRole_ThrowsNotFoundException()
    {
        var roleReadRepoMock = new Mock<IRoleReadRepository>();
        var permissionRepoMock = new Mock<IPermissionReadRepository>();
        var roleUpdateRepoMock = new Mock<IRoleUpdateRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        roleReadRepoMock.Setup(x => x.GetRoleByNameAsync("NonExistentRole", CancellationToken.None))
            .ReturnsAsync((ApplicationRole?)null);

        var handler = new UpdateRoleCommandHandler(
            roleReadRepoMock.Object,
            roleUpdateRepoMock.Object,
            permissionRepoMock.Object,
            unitOfWorkMock.Object);
        var command = new UpdateRoleCommand()
        {
            RoleName = "NonExistentRole",
            Description = "Test"
        };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "PERM_019 - Xóa role thành công")]
    public async Task DeleteRole_ValidRole_ReturnsSuccess()
    {
        var roleId = Guid.NewGuid();
        var roleReadRepoMock = new Mock<IRoleReadRepository>();
        var roleDeleteRepoMock = new Mock<IRoleDeleteRepository>();
        var protectedEntityServiceMock = new Mock<IProtectedEntityManagerService>();

        var role = new ApplicationRole { Id = roleId, Name = "OldRole" };
        roleReadRepoMock.Setup(x => x.GetRoleByNameAsync("OldRole", CancellationToken.None)).ReturnsAsync(role);
        roleReadRepoMock.Setup(x => x.GetUsersInRoleAsync("OldRole", CancellationToken.None)).ReturnsAsync([]);
        roleDeleteRepoMock.Setup(x => x.DeleteAsync(role, CancellationToken.None)).ReturnsAsync(IdentityResult.Success);

        var handler = new DeleteRoleCommandHandler(
            roleReadRepoMock.Object,
            roleDeleteRepoMock.Object,
            protectedEntityServiceMock.Object);
        var command = new DeleteRoleCommand() { RoleName = "OldRole" };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        result.Value.Message.Should().Contain("successfully");
        roleDeleteRepoMock.Verify(x => x.DeleteAsync(role, CancellationToken.None), Times.Once);
    }

    [Fact(DisplayName = "PERM_020 - Xóa nhiều roles thành công")]
    public async Task DeleteMultipleRoles_ValidRoles_ReturnsSuccess()
    {
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

        var handler = new DeleteMultipleRolesCommandHandler(
            roleReadRepoMock.Object,
            userReadRepoMock.Object,
            roleDeleteRepoMock.Object,
            protectedEntityServiceMock.Object);
        var command = new DeleteMultipleRolesCommand() { RoleNames = [ "Role1", "Role2", "Role3" ] };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        result.Value.Message.Should().Contain("3");
        result.Value.Message.Should().Contain("successfully");
        roleDeleteRepoMock.Verify(
            x => x.DeleteAsync(It.IsAny<ApplicationRole>(), CancellationToken.None),
            Times.Exactly(3));
    }
#pragma warning restore CRR0035
#pragma warning restore IDE0079
}
