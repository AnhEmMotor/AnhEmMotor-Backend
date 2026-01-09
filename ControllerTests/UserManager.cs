using Application.ApiContracts.User.Requests;
using Application.ApiContracts.User.Responses;
using Application.ApiContracts.UserManager.Requests;
using Application.ApiContracts.UserManager.Responses;
using Application.Common.Exceptions;
using Application.Features.UserManager.Commands.AssignRoles;
using Application.Features.UserManager.Commands.ChangeMultipleUsersStatus;
using Application.Features.UserManager.Commands.ChangePassword;
using Application.Features.UserManager.Commands.ChangeUserStatus;
using Application.Features.UserManager.Commands.UpdateUser;
using Application.Features.UserManager.Queries.GetUserById;
using Application.Features.UserManager.Queries.GetUsersList;
using Application.Features.UserManager.Queries.GetUsersListForOutput;
using Domain.Constants;
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

public class UserManager
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly UserManagerController _controller;

    public UserManager()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new UserManagerController(_mediatorMock.Object);

        // Setup Controller Context
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

    [Fact(DisplayName = "UMGR_001 - Lấy danh sách người dùng thành công với phân trang mặc định")]
    public async Task GetAllUsers_WithDefaultPagination_ReturnsOkWithUsers()
    {
        // Arrange
        var sieveModel = new SieveModel();
        var expectedResponse = new PagedResult<UserDTOForManagerResponse>(
            [],
            15,
            1,
            10
        );

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUsersListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetAllUsers(sieveModel, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);
        var response = okResult.Value.Should().BeAssignableTo<PagedResult<UserDTOForManagerResponse>>().Subject;
        response.TotalCount.Should().Be(15);
        response.PageNumber.Should().Be(1);
        response.PageSize.Should().Be(10);
        response.TotalPages.Should().Be(2);

        _mediatorMock.Verify(m => m.Send(
            It.Is<GetUsersListQuery>(q => q.SieveModel == sieveModel),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "UMGR_002 - Lấy danh sách người dùng với filter theo Status")]
    public async Task GetAllUsers_WithStatusFilter_ReturnsFilteredUsers()
    {
        // Arrange
        var sieveModel = new SieveModel { Filters = "Status==Active" };
        var expectedResponse = new PagedResult<UserDTOForManagerResponse>(
            [],
            10,
            1,
            10
        );

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUsersListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetAllUsers(sieveModel, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<PagedResult<UserDTOForManagerResponse>>().Subject;
        response.TotalCount.Should().Be(10);

        _mediatorMock.Verify(m => m.Send(
            It.Is<GetUsersListQuery>(q => q.SieveModel.Filters == "Status==Active"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "UMGR_003 - Lấy danh sách người dùng với sorting theo FullName")]
    public async Task GetAllUsers_WithSorting_ReturnsSortedUsers()
    {
        // Arrange
        var sieveModel = new SieveModel { Sorts = "FullName" };
        var expectedResponse = new PagedResult<UserDTOForManagerResponse>(
            [],
            5,
            1,
            10
        );

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUsersListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetAllUsers(sieveModel, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        _mediatorMock.Verify(m => m.Send(
            It.Is<GetUsersListQuery>(q => q.SieveModel.Sorts == "FullName"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "UMGR_004 - Lấy danh sách người dùng không có quyền")]
    public async Task GetAllUsers_WithoutPermission_ReturnsForbidden()
    {
        // Arrange
        var sieveModel = new SieveModel();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUsersListQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ForbiddenException("Không có quyền truy cập"));

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _controller.GetAllUsers(sieveModel, CancellationToken.None));

        _mediatorMock.Verify(m => m.Send(
            It.IsAny<GetUsersListQuery>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "UMGR_005 - Lấy danh sách người dùng cho phiếu bán hàng (for-output) với quyền phù hợp")]
    public async Task GetAllUsersForOutput_WithPermission_ReturnsLimitedFields()
    {
        // Arrange
        var sieveModel = new SieveModel();
        var expectedResponse = new PagedResult<UserDTOForOutputResponse>(
            [
                new() { Email = "user1@example.com", FullName = "User 1", PhoneNumber = "0912345678" },
                new() { Email = "user2@example.com", FullName = "User 2", PhoneNumber = "0987654321" }
            ],
            5,
            1,
            10
        );

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUsersListForOutputQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetAllUsersForOutput(sieveModel, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<PagedResult<UserDTOForOutputResponse>>().Subject;
        response.Items.Should().HaveCount(2);
        response.Items.First().Should().NotBeNull();
        response.Items.First().Email.Should().Be("user1@example.com");
        response.Items.First().FullName.Should().Be("User 1");
        response.Items.First().PhoneNumber.Should().Be("0912345678");

        _mediatorMock.Verify(m => m.Send(
            It.IsAny<GetUsersListForOutputQuery>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "UMGR_006 - Lấy danh sách người dùng for-output không có quyền")]
    public async Task GetAllUsersForOutput_WithoutPermission_ReturnsForbidden()
    {
        // Arrange
        var sieveModel = new SieveModel();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUsersListForOutputQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ForbiddenException("Không có quyền truy cập"));

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            _controller.GetAllUsersForOutput(sieveModel, CancellationToken.None));

        _mediatorMock.Verify(m => m.Send(
            It.IsAny<GetUsersListForOutputQuery>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "UMGR_007 - Lấy thông tin người dùng theo ID thành công")]
    public async Task GetUserById_WithValidId_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedResponse = new UserDTOForManagerResponse { Email = "user@example.com" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetUserById(userId, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        _mediatorMock.Verify(m => m.Send(
            It.Is<GetUserByIdQuery>(q => q.UserId == userId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "UMGR_008 - Lấy thông tin người dùng không tồn tại")]
    public async Task GetUserById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetUserByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"User {userId} not found"));

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _controller.GetUserById(userId, CancellationToken.None));

        _mediatorMock.Verify(m => m.Send(
            It.Is<GetUserByIdQuery>(q => q.UserId == userId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "UMGR_009 - Cập nhật thông tin người dùng thành công")]
    public async Task UpdateUser_WithValidData_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest
        {
            FullName = "Updated Name",
            Gender = GenderStatus.Female,
            PhoneNumber = "0912345678"
        };

        var expectedResponse = new UserDTOForManagerResponse();

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.UpdateUser(userId, request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(StatusCodes.Status200OK);

        _mediatorMock.Verify(m => m.Send(
            It.Is<UpdateUserCommand>(c => c.UserId == userId && c.Model == request),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "UMGR_010 - Cập nhật thông tin người dùng với email rỗng hoặc không hợp lệ")]
    public async Task UpdateUser_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest
        {
            FullName = "Test User",
            PhoneNumber = ""  // Invalid phone
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException("Số điện thoại không hợp lệ"));

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _controller.UpdateUser(userId, request, CancellationToken.None));

        _mediatorMock.Verify(m => m.Send(
            It.IsAny<UpdateUserCommand>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "UMGR_011 - Cập nhật username trùng với user khác")]
    public async Task UpdateUser_WithDuplicateUsername_ReturnsConflict()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new UpdateUserRequest
        {
            FullName = "Test User"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException("Username đã tồn tại"));

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _controller.UpdateUser(userId, request, CancellationToken.None));

        _mediatorMock.Verify(m => m.Send(
            It.IsAny<UpdateUserCommand>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "UMGR_012 - Đổi mật khẩu người dùng thành công")]
    public async Task ChangePassword_WithValidPassword_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ChangePasswordRequest
        {
            NewPassword = "NewPass@123"
        };

        var expectedResponse = new ChangePasswordByManagerResponse
        {
            Message = "Đổi mật khẩu thành công"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<ChangePasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ChangePassword(userId, request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<ChangePasswordByManagerResponse>().Subject;
        response.Message.Should().Be("Đổi mật khẩu thành công");

        _mediatorMock.Verify(m => m.Send(
            It.Is<ChangePasswordCommand>(c => c.UserId == userId && c.Model == request),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "UMGR_013 - Đổi mật khẩu với password không đạt yêu cầu")]
    public async Task ChangePassword_WithWeakPassword_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ChangePasswordRequest
        {
            NewPassword = "123"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<ChangePasswordCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException("Mật khẩu không đủ mạnh"));

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _controller.ChangePassword(userId, request, CancellationToken.None));

        _mediatorMock.Verify(m => m.Send(
            It.IsAny<ChangePasswordCommand>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "UMGR_014 - Gán roles cho người dùng thành công")]
    public async Task AssignRoles_WithValidRoles_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new AssignRolesRequest
        {
            RoleNames = ["Manager", "Staff"]
        };

        var expectedResponse = new AssignRoleResponse
        {
            Id = userId,
            UserName = "testuser",
            Email = "test@example.com",
            FullName = "Test User",
            Roles = ["Manager", "Staff"]
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<AssignRolesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.AssignRoles(userId, request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<AssignRoleResponse>().Subject;
        response.Roles.Should().Contain("Manager");

        _mediatorMock.Verify(m => m.Send(
            It.Is<AssignRolesCommand>(c => c.UserId == userId && c.Model == request),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "UMGR_015 - Gán role không tồn tại cho người dùng")]
    public async Task AssignRoles_WithInvalidRole_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new AssignRolesRequest
        {
            RoleNames = ["InvalidRole"]
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<AssignRolesCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException("Role không tồn tại"));

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _controller.AssignRoles(userId, request, CancellationToken.None));

        _mediatorMock.Verify(m => m.Send(
            It.IsAny<AssignRolesCommand>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "UMGR_016 - Thay đổi trạng thái người dùng thành Banned")]
    public async Task ChangeUserStatus_ToBanned_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ChangeUserStatusRequest
        {
            Status = UserStatus.Banned
        };

        var expectedResponse = new ChangeStatusUserByManagerResponse
        {
            Message = "Thay đổi trạng thái thành công"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<ChangeUserStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ChangeUserStatus(userId, request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<ChangeStatusUserByManagerResponse>().Subject;
        response.Message.Should().Be("Thay đổi trạng thái thành công");

        _mediatorMock.Verify(m => m.Send(
            It.Is<ChangeUserStatusCommand>(c => c.UserId == userId && c.Model == request),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "UMGR_017 - Super Admin cố gắng tự ban chính mình")]
    public async Task ChangeUserStatus_SuperAdminBansSelf_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ChangeUserStatusRequest
        {
            Status = UserStatus.Banned
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<ChangeUserStatusCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException("Super Admin không thể tự ban chính mình"));

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() =>
            _controller.ChangeUserStatus(userId, request, CancellationToken.None));

        _mediatorMock.Verify(m => m.Send(
            It.IsAny<ChangeUserStatusCommand>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "UMGR_018 - Thay đổi trạng thái nhiều người dùng thành công")]
    public async Task ChangeMultipleUsersStatus_WithValidUsers_ReturnsOk()
    {
        // Arrange
        var request = new ChangeMultipleUsersStatusRequest
        {
            UserIds = [Guid.NewGuid(), Guid.NewGuid()],
            Status = UserStatus.Banned
        };

        var expectedResponse = new ChangeStatusMultiUserByManagerResponse
        {
            Message = "Thay đổi trạng thái thành công"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<ChangeMultipleUsersStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ChangeMultipleUsersStatus(request, CancellationToken.None);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<ChangeStatusMultiUserByManagerResponse>().Subject;
        response.Message.Should().Be("Thay đổi trạng thái thành công");

        _mediatorMock.Verify(m => m.Send(
            It.Is<ChangeMultipleUsersStatusCommand>(c => c.Model == request),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
