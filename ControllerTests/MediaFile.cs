using Application.Common.Exceptions;
using Application.Features.Files.Commands.DeleteFile;
using Application.Features.Files.Commands.DeleteManyFiles;
using Application.Features.Files.Commands.RestoreFile;
using Application.Features.Files.Commands.RestoreManyFiles;
using Application.Features.Files.Commands.UploadImage;
using Application.Features.Files.Queries.GetFileById;
using Application.Features.Files.Queries.GetDeletedFilesList;
using Application.Features.Files.Queries.GetFilesList;
using Application.Features.Files.Queries.ViewImage;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers.V1;
using Xunit;

namespace ControllerTests;

public class MediaFile
{
    [Fact(DisplayName = "MF_005 - Tải lên ảnh thất bại khi không có quyền Upload")]
    public async Task UploadImage_NoPermission_Forbidden()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var controller = new MediaFileController(mediatorMock.Object);
        
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(100000);
        fileMock.Setup(f => f.FileName).Returns("test.webp");

        // Act
        var act = async () => await controller.UploadImage(fileMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("*permission*");
    }

    [Fact(DisplayName = "MF_006 - Tải lên ảnh thất bại khi chưa đăng nhập")]
    public async Task UploadImage_NotAuthenticated_Unauthorized()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var controller = new MediaFileController(mediatorMock.Object);
        
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(100000);
        fileMock.Setup(f => f.FileName).Returns("test.webp");

        // Act
        var act = async () => await controller.UploadImage(fileMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*authenticated*");
    }

    [Fact(DisplayName = "MF_012 - Xoá file thất bại khi không có quyền Delete")]
    public async Task DeleteFile_NoPermission_Forbidden()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var controller = new MediaFileController(mediatorMock.Object);
        
        var storagePath = "test-file.webp";

        // Act
        var act = async () => await controller.DeleteFile(storagePath, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("*permission*");
    }

    [Fact(DisplayName = "MF_018 - Khôi phục file thất bại khi không có quyền")]
    public async Task RestoreFile_NoPermission_Forbidden()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var controller = new MediaFileController(mediatorMock.Object);
        
        var storagePath = "deleted-file.webp";

        // Act
        var act = async () => await controller.RestoreFile(storagePath, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("*permission*");
    }

    [Fact(DisplayName = "MF_034 - Lấy thông tin file khi không có quyền View")]
    public async Task GetFileById_NoPermission_Forbidden()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var controller = new MediaFileController(mediatorMock.Object);
        
        var fileId = 123;

        // Act
        var act = async () => await controller.GetFileByIdAsync(fileId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("*permission*");
    }

    [Fact(DisplayName = "MF_046 - Validate: StoragePath rỗng khi xoá file")]
    public async Task DeleteFile_EmptyStoragePath_BadRequest()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var controller = new MediaFileController(mediatorMock.Object);
        
        var storagePath = "";

        // Act
        var act = async () => await controller.DeleteFile(storagePath, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*required*empty*");
    }

    [Fact(DisplayName = "MF_047 - Validate: StoragePath chứa path traversal (Security)")]
    public async Task DeleteFile_PathTraversal_BadRequest()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var controller = new MediaFileController(mediatorMock.Object);
        
        var storagePath = "../../etc/passwd.webp";

        // Act
        var act = async () => await controller.DeleteFile(storagePath, CancellationToken.None);

        // Assert - Security: Path traversal phải bị reject
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*invalid*path*");
    }

    [Fact(DisplayName = "MF_CT_001 - Upload ảnh với Authorization header hợp lệ")]
    public async Task UploadImage_ValidAuth_Success()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var controller = new MediaFileController(mediatorMock.Object);
        
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(100000);
        fileMock.Setup(f => f.FileName).Returns("test.webp");
        
        // Mock authentication context
        // controller.ControllerContext = new ControllerContext();
        // controller.ControllerContext.HttpContext = new DefaultHttpContext();
        // controller.ControllerContext.HttpContext.User = ...

        // Act
        var result = await controller.UploadImage(fileMock.Object, CancellationToken.None);

        // Assert - Upload thành công
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
    }

    [Fact(DisplayName = "MF_CT_002 - Upload ảnh với Authorization header không hợp lệ")]
    public async Task UploadImage_InvalidAuth_Unauthorized()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var controller = new MediaFileController(mediatorMock.Object);
        
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(100000);
        fileMock.Setup(f => f.FileName).Returns("test.webp");

        // Act
        var act = async () => await controller.UploadImage(fileMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("*invalid*token*");
    }

    [Fact(DisplayName = "MF_CT_003 - Xoá file với Role có đúng Permission")]
    public async Task DeleteFile_WithCorrectPermission_Success()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var controller = new MediaFileController(mediatorMock.Object);
        
        var storagePath = "test-file.webp";

        // Act
        var result = await controller.DeleteFile(storagePath, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkResult>();
    }

    [Fact(DisplayName = "MF_CT_004 - Xoá file với Role thiếu Permission")]
    public async Task DeleteFile_WithoutPermission_Forbidden()
    {
        // Arrange
        var mediatorMock = new Mock<IMediator>();
        var controller = new MediaFileController(mediatorMock.Object);
        
        var storagePath = "test-file.webp";

        // Act
        var act = async () => await controller.DeleteFile(storagePath, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>()
            .WithMessage("*permission*");
    }
}
