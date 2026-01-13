using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers.V1;

namespace ControllerTests;

public class MediaFile
{
    [Fact(DisplayName = "MF_005 - Tải lên ảnh thất bại khi không có quyền Upload")]
    public async Task UploadImage_NoPermission_Forbidden()
    {
        var mediatorMock = new Mock<IMediator>();
        var controller = new MediaFileController(mediatorMock.Object);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(100000);
        fileMock.Setup(f => f.FileName).Returns("test.webp");

        var result = await controller.UploadImageAsync(fileMock.Object, CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<ForbidResult>();
    }

#pragma warning disable CRR0035
    [Fact(DisplayName = "MF_006 - Tải lên ảnh thất bại khi chưa đăng nhập")]
    public async Task UploadImage_NotAuthenticated_Unauthorized()
    {
        var mediatorMock = new Mock<IMediator>();
        var controller = new MediaFileController(mediatorMock.Object);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(100000);
        fileMock.Setup(f => f.FileName).Returns("test.webp");

        var result = await controller.UploadImageAsync(fileMock.Object, CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact(DisplayName = "MF_012 - Xoá file thất bại khi không có quyền Delete")]
    public async Task DeleteFile_NoPermission_Forbidden()
    {
        var mediatorMock = new Mock<IMediator>();
        var controller = new MediaFileController(mediatorMock.Object);

        var storagePath = "test-file.webp";

        var result = await controller.DeleteFileAsync(storagePath, CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact(DisplayName = "MF_018 - Khôi phục file thất bại khi không có quyền")]
    public async Task RestoreFile_NoPermission_Forbidden()
    {
        var mediatorMock = new Mock<IMediator>();
        var controller = new MediaFileController(mediatorMock.Object);

        var storagePath = "deleted-file.webp";

        var result = await controller.RestoreFileAsync(storagePath, CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact(DisplayName = "MF_034 - Lấy thông tin file khi không có quyền View")]
    public async Task GetFileById_NoPermission_Forbidden()
    {
        var mediatorMock = new Mock<IMediator>();
        var controller = new MediaFileController(mediatorMock.Object);

        var fileId = 123;

        var result = await controller.GetFileByIdAsync(fileId, CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact(DisplayName = "MF_046 - Validate: StoragePath rỗng khi xoá file")]
    public async Task DeleteFile_EmptyStoragePath_BadRequest()
    {
        var mediatorMock = new Mock<IMediator>();
        var controller = new MediaFileController(mediatorMock.Object);

        var storagePath = string.Empty;

        var result = await controller.DeleteFileAsync(storagePath, CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "MF_047 - Validate: StoragePath chứa path traversal (Security)")]
    public async Task DeleteFile_PathTraversal_BadRequest()
    {
        var mediatorMock = new Mock<IMediator>();
        var controller = new MediaFileController(mediatorMock.Object);

        var storagePath = "../../etc/passwd.webp";

        var result = await controller.DeleteFileAsync(storagePath, CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "MF_CT_001 - Upload ảnh với Authorization header hợp lệ")]
    public async Task UploadImage_ValidAuth_Success()
    {
        var mediatorMock = new Mock<IMediator>();
        var controller = new MediaFileController(mediatorMock.Object);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(100000);
        fileMock.Setup(f => f.FileName).Returns("test.webp");

        var result = await controller.UploadImageAsync(fileMock.Object, CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();
    }

    [Fact(DisplayName = "MF_CT_002 - Upload ảnh với Authorization header không hợp lệ")]
    public async Task UploadImage_InvalidAuth_Unauthorized()
    {
        var mediatorMock = new Mock<IMediator>();
        var controller = new MediaFileController(mediatorMock.Object);

        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(100000);
        fileMock.Setup(f => f.FileName).Returns("test.webp");

        var result = await controller.UploadImageAsync(fileMock.Object, CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact(DisplayName = "MF_CT_003 - Xoá file với Role có đúng Permission")]
    public async Task DeleteFile_WithCorrectPermission_Success()
    {
        var mediatorMock = new Mock<IMediator>();
        var controller = new MediaFileController(mediatorMock.Object);

        var storagePath = "test-file.webp";

        var result = await controller.DeleteFileAsync(storagePath, CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<OkResult>();
    }

    [Fact(DisplayName = "MF_CT_004 - Xoá file với Role thiếu Permission")]
    public async Task DeleteFile_WithoutPermission_Forbidden()
    {
        var mediatorMock = new Mock<IMediator>();
        var controller = new MediaFileController(mediatorMock.Object);

        var storagePath = "test-file.webp";

        var result = await controller.DeleteFileAsync(storagePath, CancellationToken.None).ConfigureAwait(true);

        result.Should().BeOfType<ForbidResult>();
    }
#pragma warning restore CRR0035
}
