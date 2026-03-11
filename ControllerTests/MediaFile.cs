using Application.Common.Models;
using Application.Features.Files.Commands.DeleteFile;
using FluentAssertions;
using Infrastructure.Authorization.Attribute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Reflection;
using WebAPI.Controllers.V1;
using static Domain.Constants.Permission.PermissionsList;

namespace ControllerTests;

public class MediaFile
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly MediaFileController _controller;

    public MediaFile()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new MediaFileController(_mediatorMock.Object);

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };
    }

#pragma warning disable IDE0079 
#pragma warning disable CRR0035
    [Fact(DisplayName = "MF_006 - Tải lên ảnh thất bại khi chưa đăng nhập")]
    public void UploadImage_NotAuthenticated_Unauthorized()
    {
        var method = typeof(MediaFileController).GetMethod("UploadProductImageAsync");

        var hasAuthorize = method!.GetCustomAttributes(typeof(AuthorizeAttribute), true).Length != 0 ||
            typeof(MediaFileController).GetCustomAttributes(typeof(AuthorizeAttribute), true).Length != 0;

        hasAuthorize.Should().BeTrue("API này phải yêu cầu đăng nhập (Authorize)");
    }

    [Fact(DisplayName = "MF_005: UploadImageAsync has RequiresAnyPermissions with Edit/Create permissions")]
    public void UploadImageAsync_HasCorrectPermissionsAttribute()
    {
        var methodInfo = typeof(MediaFileController).GetMethod(nameof(MediaFileController.UploadProductImageAsync));
        var attribute = methodInfo!.GetCustomAttribute<RequiresAnyPermissionsAttribute>();

        Assert.NotNull(attribute);

        Assert.Contains(Products.Edit, attribute.Policy);
        Assert.Contains(Products.Create, attribute.Policy);
    }

    [Fact(DisplayName = "MF_CT_004: DeleteFileAsync has RequiresAnyPermissions with Edit/Create permissions")]
    public void DeleteFileAsync_HasCorrectPermissionsAttribute()
    {
        var methodInfo = typeof(MediaFileController).GetMethod(nameof(MediaFileController.DeleteProductFileAsync));
        var attribute = methodInfo!.GetCustomAttribute<RequiresAnyPermissionsAttribute>();

        Assert.NotNull(attribute);

        Assert.Contains(Products.Edit, attribute.Policy);
        Assert.Contains(Products.Create, attribute.Policy);
    }

    [Fact(DisplayName = "MF_012: DeleteFileAsync has RequiresAnyPermissions with Edit/Create permissions")]
    public void DeleteFileAsync_HasCorrectPermissionsAttribute1()
    {
        var methodInfo = typeof(MediaFileController).GetMethod(nameof(MediaFileController.DeleteProductFileAsync));
        var attribute = methodInfo!.GetCustomAttribute<RequiresAnyPermissionsAttribute>();

        Assert.NotNull(attribute);

        Assert.Contains(Products.Edit, attribute.Policy);
        Assert.Contains(Products.Create, attribute.Policy);
    }

    [Fact(DisplayName = "MF_018: RestoreFileAsync has RequiresAnyPermissions with Edit/Create permissions")]
    public void RestoreFileAsync_HasCorrectPermissionsAttribute()
    {
        var methodInfo = typeof(MediaFileController).GetMethod(nameof(MediaFileController.RestoreFileAsync));
        var attribute = methodInfo?.GetCustomAttribute<RequiresAnyPermissionsAttribute>();

        Assert.NotNull(attribute);

        Assert.Contains(Products.Edit, attribute.Policy);
        Assert.Contains(Products.Create, attribute.Policy);
    }

    [Fact(DisplayName = "MF_CT_002: UploadProductImageAsync throws ArgumentNullException when file is null")]
    public async Task UploadProductImageAsync_ThrowsArgumentNullException_WhenFileIsNull()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _controller.UploadProductImageAsync(null!, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "MF_CT_003: DeleteFileAsync returns NoContent when mediator succeeds")]
    public async Task DeleteFileAsync_ReturnsNoContent_WhenMediatorSucceeds()
    {
        var storagePath = "test-file.webp";
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteProductImageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var result = await _controller.DeleteProductFileAsync(storagePath, CancellationToken.None).ConfigureAwait(true);

        var statusCodeResult = Assert.IsType<NoContentResult>(result);
        Assert.Equal(StatusCodes.Status204NoContent, statusCodeResult.StatusCode);
    }

    [Fact(DisplayName = "MF_046: DeleteFileAsync returns BadRequest when validation fails")]
    public async Task DeleteFileAsync_ReturnsBadRequest_WhenValidationFails()
    {
        var storagePath = string.Empty;
        var validationError = Error.Validation("StoragePath is required", "StoragePath.Empty");

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteProductImageCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(validationError));

        var result = await _controller.DeleteProductFileAsync(storagePath, CancellationToken.None).ConfigureAwait(true);

        var objectResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }
#pragma warning restore CRR0035
#pragma warning restore IDE0079
}
