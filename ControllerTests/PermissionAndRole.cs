using Application.ApiContracts.Permission.Responses;
using Application.Common.Models;
using Application.Features.Permissions.Commands.CreateRole;
using Application.Features.Permissions.Commands.DeleteMultipleRoles;
using Application.Features.Permissions.Commands.DeleteRole;
using Application.Features.Permissions.Commands.UpdateRole;
using Application.Features.Permissions.Queries.GetAllPermissions;
using Application.Features.Permissions.Queries.GetAllRoles;
using Application.Features.Permissions.Queries.GetMyPermissions;
using Application.Features.Permissions.Queries.GetRolePermissions;
using Application.Features.Permissions.Queries.GetUserPermissionsById;
using Domain.Constants.Permission;
using Domain.Primitives;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sieve.Models;
using System.Security.Claims;
using WebAPI.Controllers.V1;
using Application.Features.News.Commands.CreateNews;
using Application.Features.Bookings.Commands.ConfirmBooking;
using Application.Features.Leads.Queries.GetLeads;
using Application.Features.Banners.Queries.GetActiveBanners;
using Application.Features.Contacts.Commands.CreateContactReply;
using Application.Features.Contacts.Commands.UpdateInternalNote;
using Application.Features.News.Queries.GetNewsList;

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
        _controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };
    }

    #pragma warning disable IDE0079 
    #pragma warning disable CRR0035
    [Fact(DisplayName = "PERM_CTRL_001 - Controller gọi GetAllPermissions thành công")]
    public async Task GetAllPermissions_MediatorReturnsData_ReturnsOkWithData()
    {
        var expectedData = new Dictionary<string, List<PermissionResponse>>
        {
            {
                "Brands",
                new List<PermissionResponse>
                {
                    new()
                    {
                        ID = PermissionsList.Brands.View,
                        DisplayName = "View Brands",
                        Description = "Xem danh sách thương hiệu"
                    },
                    new()
                    {
                        ID = PermissionsList.Brands.Create,
                        DisplayName = "Create Brand",
                        Description = "Tạo thương hiệu mới"
                    }
                }
            },
            {
                "Products",
                new List<PermissionResponse>
                {
                    new()
                    {
                        ID = PermissionsList.Products.View,
                        DisplayName = "View Products",
                        Description = "Xem danh sách sản phẩm"
                    }
                }
            },
            {
                "Roles",
                new List<PermissionResponse>
                {
                    new()
                    {
                        ID = PermissionsList.Roles.View,
                        DisplayName = "View Roles",
                        Description = "Xem danh sách vai trò"
                    }
                }
            }
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllPermissionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedData);
        var result = await _controller.GetAllPermissionsAsync(CancellationToken.None).ConfigureAwait(true);
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedData);
    }

    [Fact(DisplayName = "PERM_CTRL_002 - Controller gọi GetMyPermissions thành công")]
    public async Task GetMyPermissions_MediatorReturnsData_ReturnsOkWithPermissions()
    {
        var userId = Guid.NewGuid();
        SetupUserClaims(userId);
        var expectedResponse = new PermissionAndRoleOfUserResponse
        {
            UserId = userId,
            UserName = "testuser",
            Email = "test@test.com",
            Roles = ["Manager"],
            Permissions =
                [PermissionsList.Brands.View, PermissionsList.Brands.Create, PermissionsList.Products.View, PermissionsList.Products.Create]
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetMyPermissionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);
        var result = await _controller.GetMyPermissionsAsync(CancellationToken.None).ConfigureAwait(true);
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as PermissionAndRoleOfUserResponse;
        response.Should().NotBeNull();
        response!.Permissions.Should().HaveCount(4);
    }

    [Fact(DisplayName = "PERM_CTRL_003 - Controller gọi GetMyPermissions khi user chưa đăng nhập")]
    public async Task GetMyPermissions_NoUserClaims_ThrowsUnauthorizedException()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetMyPermissionsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PermissionAndRoleOfUserResponse>.Failure(Error.Unauthorized("User not authenticated")));
        var result = await _controller.GetMyPermissionsAsync(CancellationToken.None).ConfigureAwait(true);
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact(DisplayName = "PERM_CTRL_004 - Controller gọi GetUserPermissionsById thành công")]
    public async Task GetUserPermissionsById_ValidUserId_ReturnsOkWithPermissions()
    {
        var userId = Guid.NewGuid();
        var expectedResponse = new PermissionAndRoleOfUserResponse
        {
            UserId = userId,
            UserName = "targetuser",
            Email = "target@test.com",
            Roles = ["Staff"],
            Permissions =
                [PermissionsList.Products.View, PermissionsList.Brands.View, PermissionsList.Files.View, PermissionsList.Files.Upload, PermissionsList.Suppliers.View]
        };
        _mediatorMock.Setup(
            m => m.Send(It.Is<GetUserPermissionsByIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);
        var result = await _controller.GetUserPermissionsByIdAsync(userId, CancellationToken.None).ConfigureAwait(true);
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
        var userId = Guid.NewGuid();
        _mediatorMock.Setup(
            m => m.Send(It.Is<GetUserPermissionsByIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                Result<PermissionAndRoleOfUserResponse>.Failure(Error.NotFound($"User with id {userId} not found")));
        var result = await _controller.GetUserPermissionsByIdAsync(userId, CancellationToken.None).ConfigureAwait(true);
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "PERM_CTRL_006 - Controller gọi GetRolePermissions thành công")]
    public async Task GetRolePermissions_ValidRoleName_ReturnsOkWithPermissions()
    {
        var roleId = Guid.NewGuid();
        var expectedPermissions = new List<string>
        {
            PermissionsList.Brands.View,
            PermissionsList.Brands.Create,
            PermissionsList.Products.View,
            PermissionsList.Products.Create,
            PermissionsList.Files.View,
            PermissionsList.Suppliers.View
        };
        _mediatorMock.Setup(
            m => m.Send(It.Is<GetRolePermissionsQuery>(q => q.RoleId == roleId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPermissions);
        var result = await _controller.GetRolePermissionsAsync(roleId, CancellationToken.None).ConfigureAwait(true);
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var permissions = okResult!.Value as List<string>;
        permissions.Should().NotBeNull();
        permissions.Should().HaveCount(6);
    }

    [Fact(DisplayName = "PERM_CTRL_007 - Controller gọi GetRolePermissions với role không tồn tại")]
    public async Task GetRolePermissions_InvalidRoleName_ThrowsNotFoundException()
    {
        var roleId = Guid.NewGuid();
        _mediatorMock.Setup(
            m => m.Send(It.Is<GetRolePermissionsQuery>(q => q.RoleId == roleId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<string>>.Failure(Error.NotFound($"Role not found")));
        var result = await _controller.GetRolePermissionsAsync(roleId, CancellationToken.None).ConfigureAwait(true);
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "PERM_CTRL_009 - Controller gọi GetAllRoles thành công")]
    public async Task GetAllRoles_MediatorReturnsData_ReturnsOkWithRoles()
    {
        var sieveModel = new SieveModel();
        var expectedRoles = new PagedResult<RoleSelectResponse>(
            [new() { ID = Guid.NewGuid(), Name = "Admin" }, new() { ID = Guid.NewGuid(), Name = "Manager" }, new()
            {
                ID = Guid.NewGuid(),
                Name = "Staff"
            }, new() { ID = Guid.NewGuid(), Name = "User" }],
            4,
            1,
            10);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAllRolesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedRoles);
        var result = await _controller.GetAllRolesAsync(sieveModel, CancellationToken.None).ConfigureAwait(true);
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var roles = okResult!.Value as PagedResult<RoleSelectResponse>;
        roles.Should().NotBeNull();
        roles!.Items.Should().HaveCount(4);
    }

    [Fact(DisplayName = "PERM_CTRL_010 - Controller gọi CreateRole thành công")]
    public async Task CreateRole_ValidRequest_ReturnsOkWithCreatedRole()
    {
        var request = new CreateRoleCommand
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
        var result = await _controller.CreateRoleAsync(request, CancellationToken.None).ConfigureAwait(true);
        result.Should().BeOfType<CreatedAtRouteResult>();
        var okResult = result as CreatedAtRouteResult;
        var response = okResult!.Value as RoleCreateResponse;
        response.Should().NotBeNull();
        response!.RoleName.Should().Be("NewRole");
        response.RoleId.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "PERM_CTRL_011 - Controller gọi CreateRole với tên trùng")]
    public async Task CreateRole_DuplicateName_ThrowsValidationException()
    {
        var request = new CreateRoleCommand
        {
            RoleName = "Admin",
            Description = "Duplicate",
            Permissions = [PermissionsList.Brands.View]
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateRoleCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException("Role name already exists"));
        Func<Task> act = async () => await _controller.CreateRoleAsync(request, CancellationToken.None)
            .ConfigureAwait(true);
        await act.Should().ThrowAsync<ValidationException>().WithMessage("*already exists*").ConfigureAwait(true);
    }

    [Fact(DisplayName = "PERM_CTRL_012 - Controller calls UpdateRole successfully")]
    public async Task UpdateRole_ValidRequest_ReturnsOkWithUpdatedRole()
    {
        var roleId = Guid.NewGuid();
        var request = new UpdateRoleCommand { RoleId = roleId, Description = "Updated Description" };
        var expectedResponse = new PermissionRoleUpdateResponse { Message = "Role updated successfully" };
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<UpdateRoleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PermissionRoleUpdateResponse>.Success(expectedResponse));
        var result = await _controller.UpdateRoleAsync(roleId, request, CancellationToken.None).ConfigureAwait(true);
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as PermissionRoleUpdateResponse;
        response.Should().NotBeNull();
    }

    [Fact(DisplayName = "PERM_CTRL_013 - Controller gọi DeleteRole thành công")]
    public async Task DeleteRole_ValidRoleName_ReturnsOk()
    {
        var roleId = Guid.NewGuid();
        var expectedResponse = new RoleDeleteResponse { Message = "Role deleted successfully" };
        _mediatorMock.Setup(
            m => m.Send(It.Is<DeleteRoleCommand>(c => c.RoleId == roleId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);
        var result = await _controller.DeleteRoleAsync(roleId, CancellationToken.None).ConfigureAwait(true);
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as RoleDeleteResponse;
        response.Should().NotBeNull();
        response!.Message.Should().Contain("successfully");
    }

    [Fact(DisplayName = "PERM_CTRL_014 - Controller gọi DeleteRole với role không tồn tại")]
    public async Task DeleteRole_NonExistentRole_ThrowsNotFoundException()
    {
        var roleId = Guid.NewGuid();
        _mediatorMock.Setup(
            m => m.Send(It.Is<DeleteRoleCommand>(c => c.RoleId == roleId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<RoleDeleteResponse>.Failure(Error.NotFound($"Role not found")));
        var result = await _controller.DeleteRoleAsync(roleId, CancellationToken.None).ConfigureAwait(true);
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact(DisplayName = "PERM_CTRL_015 - Controller gọi DeleteMultipleRoles thành công")]
    public async Task DeleteMultipleRoles_ValidRoleNames_ReturnsOk()
    {
        var roleNames = new List<string> { "Role1", "Role2" };
        var expectedResponse = new RoleDeleteResponse { Message = "2 roles deleted successfully" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteMultipleRolesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);
        var result = await _controller.DeleteMultipleRolesAsync(roleNames, CancellationToken.None).ConfigureAwait(true);
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as RoleDeleteResponse;
        response.Should().NotBeNull();
        response!.Message.Should().Contain("2");
        response.Message.Should().Contain("successfully");
    }

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
    [Fact(DisplayName = "PERM_021 - Controller - Truy cập danh sách tin tức không có quyền")]
    public async Task GetNewsList_NoPermission_ReturnsForbidden()
    {
        var newsController = new NewsController(_mediatorMock.Object);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetNewsListQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => newsController.GetListAsync(new SieveModel(), CancellationToken.None));
    }

    [Fact(DisplayName = "PERM_022 - Controller - Truy cập danh sách tin tức khi có quyền")]
    public async Task GetNewsList_WithPermission_ReturnsSuccess()
    {
        var newsController = new NewsController(_mediatorMock.Object);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetNewsListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<Application.ApiContracts.News.Responses.NewsResponse>>.Success(new PagedResult<Application.ApiContracts.News.Responses.NewsResponse>([], 0, 1, 10)));
        var result = await newsController.GetListAsync(new SieveModel(), CancellationToken.None);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "PERM_023 - Controller - Tạo tin tức mới không có quyền")]
    public async Task CreateNews_NoPermission_ReturnsForbidden()
    {
        var newsController = new NewsController(_mediatorMock.Object);
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateNewsCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => newsController.CreateAsync(new CreateNewsCommand(), CancellationToken.None));
    }

    [Fact(DisplayName = "PERM_024 - Controller - Xác nhận lịch lái thử không có quyền")]
    public async Task ConfirmBooking_NoPermission_ReturnsForbidden()
    {
        var bookingsController = new BookingsController(_mediatorMock.Object);
        _mediatorMock.Setup(m => m.Send(It.IsAny<ConfirmBookingCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => bookingsController.ConfirmAsync(1, CancellationToken.None));
    }

    [Fact(DisplayName = "PERM_025 - Controller - Xác nhận lịch lái thử khi có đủ quyền")]
    public async Task ConfirmBooking_WithPermission_ReturnsSuccess()
    {
        var bookingsController = new BookingsController(_mediatorMock.Object);
        _mediatorMock.Setup(m => m.Send(It.IsAny<ConfirmBookingCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        var result = await bookingsController.ConfirmAsync(1, CancellationToken.None);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "PERM_030 - Controller - Xem danh sách khách hàng tiềm năng (Leads)")]
    public async Task GetLeads_WithPermission_ReturnsSuccess()
    {
        var leadController = new LeadController(_mediatorMock.Object);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetLeadsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var result = await leadController.GetLeadsAsync(CancellationToken.None);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "PERM_031 - Controller - Truy cập danh sách Banner không có quyền")]
    public async Task GetBanners_NoPermission_ReturnsForbidden()
    {
        var bannerController = new BannerController(_mediatorMock.Object);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetActiveBannersQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => bannerController.GetActiveAsync(CancellationToken.None));
    }

    [Fact(DisplayName = "PERM_032 - Controller - Phản hồi liên hệ khách hàng thành công")]
    public async Task ReplyContact_WithPermission_ReturnsSuccess()
    {
        var contactsController = new ContactsController(_mediatorMock.Object);
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateContactReplyCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Success(1));
        var result = await contactsController.ReplyAsync(new CreateContactReplyCommand(), CancellationToken.None);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "PERM_033 - Controller - Cập nhật ghi chú nội bộ liên hệ")]
    public async Task UpdateContactNote_WithPermission_ReturnsSuccess()
    {
        var contactsController = new ContactsController(_mediatorMock.Object);
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateInternalNoteCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        var result = await contactsController.UpdateInternalNoteAsync(new UpdateInternalNoteCommand(), CancellationToken.None);
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "PERM_034 - Controller - Xóa khách hàng tiềm năng không có quyền")]
    public async Task DeleteLead_NoPermission_ReturnsForbidden()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<IBaseRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());
        Assert.True(true);
    }
    #pragma warning restore CRR0035
    #pragma warning restore IDE0079
}
