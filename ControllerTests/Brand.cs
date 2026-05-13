using Application.Features.Brands.Commands.CreateBrand;
using Application.Features.Brands.Commands.DeleteBrand;
using Application.Features.Brands.Commands.UpdateBrand;
using Application.Features.Brands.Queries.GetBrandById;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers.V1;

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
        _controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };
    }

    #pragma warning disable IDE0079 
    #pragma warning disable CRR0035
    [Fact(DisplayName = "BRAND_005 - CreateBrand - Forbidden")]
    public async Task BRAND_005_CreateBrand_Forbidden()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateBrandCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.CreateBrandAsync(new CreateBrandCommand(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "BRAND_009 - GetBrandById - NotFound")]
    public async Task BRAND_009_GetBrandById_NotFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetBrandByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _controller.GetBrandByIdAsync(9999, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "BRAND_012 - UpdateBrand - NotFound")]
    public async Task BRAND_012_UpdateBrand_NotFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateBrandCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _controller.UpdateBrandAsync(9999, new UpdateBrandCommand(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "BRAND_014 - DeleteBrand - NotFound")]
    public async Task BRAND_014_DeleteBrand_NotFound()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteBrandCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException());
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _controller.DeleteBrandAsync(9999, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "BRAND_044 - CreateBrand - Exception")]
    public async Task BRAND_044_CreateBrand_Exception()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateBrandCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Internal Error"));
        await Assert.ThrowsAsync<Exception>(
            () => _controller.CreateBrandAsync(new CreateBrandCommand(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "BRAND_045 - UpdateBrand - Exception")]
    public async Task BRAND_045_UpdateBrand_Exception()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateBrandCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Internal Error"));
        await Assert.ThrowsAsync<Exception>(
            () => _controller.UpdateBrandAsync(1, new UpdateBrandCommand(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "BRAND_046 - DeleteBrand - Exception")]
    public async Task BRAND_046_DeleteBrand_Exception()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteBrandCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Internal Error"));
        await Assert.ThrowsAsync<Exception>(() => _controller.DeleteBrandAsync(1, CancellationToken.None))
            .ConfigureAwait(true);
    }
    #pragma warning restore CRR0035
    #pragma warning restore IDE0079
}
