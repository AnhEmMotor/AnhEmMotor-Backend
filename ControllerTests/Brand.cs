using System.Reflection;
using Application.ApiContracts.Brand.Responses;
using Application.Common.Models;
using Application.Features.Brands.Commands.CreateBrand;
using Application.Features.Brands.Commands.DeleteBrand;
using Application.Features.Brands.Commands.DeleteManyBrands;
using Application.Features.Brands.Commands.RestoreBrand;
using Application.Features.Brands.Commands.RestoreManyBrands;
using Application.Features.Brands.Commands.UpdateBrand;
using Application.Features.Brands.Queries.GetBrandById;
using Application.Features.Brands.Queries.GetBrandsList;
using Application.Features.Brands.Queries.GetDeletedBrandsList;
using Domain.Primitives;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sieve.Models;
using WebAPI.Controllers.V1;
using Xunit;

namespace ControllerTests;

public class Brand
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly BrandController _controller;

    public Brand()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new BrandController(_mediatorMock.Object);
        
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

#pragma warning disable CRR0035
    [Fact(DisplayName = "BRAND_005 - CreateBrand - Forbidden")]
    public async Task BRAND_005_CreateBrand_Forbidden()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateBrandCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.CreateBrandAsync(new CreateBrandCommand(), CancellationToken.None)).ConfigureAwait(true);
    }

    [Fact(DisplayName = "BRAND_009 - GetBrandById - NotFound")]
    public async Task BRAND_009_GetBrandById_NotFound()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetBrandByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());

        // Act
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.GetBrandByIdAsync(9999, CancellationToken.None)).ConfigureAwait(true);
    }

    [Fact(DisplayName = "BRAND_012 - UpdateBrand - NotFound")]
    public async Task BRAND_012_UpdateBrand_NotFound()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateBrandCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());

        // Act
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.UpdateBrandAsync(9999, new UpdateBrandCommand(), CancellationToken.None)).ConfigureAwait(true);
    }

    [Fact(DisplayName = "BRAND_014 - DeleteBrand - NotFound")]
    public async Task BRAND_014_DeleteBrand_NotFound()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteBrandCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());

        // Act
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.DeleteBrandAsync(9999, CancellationToken.None)).ConfigureAwait(true);
    }

    [Fact(DisplayName = "BRAND_032 - CreateBrand - Check Authorize Attribute")]
    public void BRAND_032_CreateBrand_CheckAuthorize()
    {
        var method = typeof(BrandController).GetMethod("CreateBrand");
        var attribute = method?.GetCustomAttribute<AuthorizeAttribute>();
        attribute.Should().NotBeNull();
    }

    [Fact(DisplayName = "BRAND_033 - UpdateBrand - Unauthorized")]
    public void BRAND_033_UpdateBrand_Unauthorized()
    {
        var method = typeof(BrandController).GetMethod("UpdateBrand");
        var attribute = method?.GetCustomAttribute<AuthorizeAttribute>();
        attribute.Should().NotBeNull();
    }

    [Fact(DisplayName = "BRAND_034 - DeleteBrand - Unauthorized")]
    public void BRAND_034_DeleteBrand_Unauthorized()
    {
        var method = typeof(BrandController).GetMethod("DeleteBrand");
        var attribute = method?.GetCustomAttribute<AuthorizeAttribute>();
        attribute.Should().NotBeNull();
    }

    [Fact(DisplayName = "BRAND_035 - RestoreBrand - Unauthorized")]
    public void BRAND_035_RestoreBrand_Unauthorized()
    {
        var method = typeof(BrandController).GetMethod("RestoreBrand");
        var attribute = method?.GetCustomAttribute<AuthorizeAttribute>();
        attribute.Should().NotBeNull();
    }

    [Fact(DisplayName = "BRAND_036 - DeleteMany - Unauthorized")]
    public void BRAND_036_DeleteMany_Unauthorized()
    {
        var method = typeof(BrandController).GetMethod("DeleteBrands");
        var attribute = method?.GetCustomAttribute<AuthorizeAttribute>();
        attribute.Should().NotBeNull();
    }

    [Fact(DisplayName = "BRAND_037 - RestoreMany - Unauthorized")]
    public void BRAND_037_RestoreMany_Unauthorized()
    {
        var method = typeof(BrandController).GetMethod("RestoreBrands");
        var attribute = method?.GetCustomAttribute<AuthorizeAttribute>();
        attribute.Should().NotBeNull();
    }

    [Fact(DisplayName = "BRAND_038 - GetDeletedBrands - Unauthorized")]
    public void BRAND_038_GetDeletedBrands_Unauthorized()
    {
        var method = typeof(BrandController).GetMethod("GetDeletedBrands");
        var attribute = method?.GetCustomAttribute<AuthorizeAttribute>();
        attribute.Should().NotBeNull();
    }

    [Fact(DisplayName = "BRAND_039 - GetBrandsForManager - Unauthorized")]
    public void BRAND_039_GetBrandsForManager_Unauthorized()
    {
        var method = typeof(BrandController).GetMethod("GetBrandsForManager");
        var attribute = method?.GetCustomAttribute<AuthorizeAttribute>();
        attribute.Should().NotBeNull();
    }

    [Fact(DisplayName = "BRAND_044 - CreateBrand - Exception")]
    public async Task BRAND_044_CreateBrand_Exception()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateBrandCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Internal Error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _controller.CreateBrandAsync(new CreateBrandCommand(), CancellationToken.None)).ConfigureAwait(true);
    }

    [Fact(DisplayName = "BRAND_045 - UpdateBrand - Exception")]
    public async Task BRAND_045_UpdateBrand_Exception()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateBrandCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Internal Error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _controller.UpdateBrandAsync(1, new UpdateBrandCommand(), CancellationToken.None)).ConfigureAwait(true);
    }

    [Fact(DisplayName = "BRAND_046 - DeleteBrand - Exception")]
    public async Task BRAND_046_DeleteBrand_Exception()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteBrandCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Internal Error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _controller.DeleteBrandAsync(1, CancellationToken.None)).ConfigureAwait(true)  ;
    }
#pragma warning restore CRR0035
}
