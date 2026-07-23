using Application.ApiContracts.Vehicle.Responses;
using Application.Common.Models;
using Application.Features.Vehicles.Queries.GetVehicles;
using Domain.Primitives;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sieve.Models;
using WebAPI.Controllers.V1;

namespace ControllerTests;

public class VehicleAsset
{
    private readonly VehicleController _controller;

    public VehicleAsset()
    {
        var httpContext = new DefaultHttpContext();
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(m => m.Send(It.IsAny<GetVehiclesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<VehicleResponse>>.Success(new PagedResult<VehicleResponse>([], 0, 1, 10)));
        _controller = new VehicleController(mediatorMock.Object)
        {
            ControllerContext = new ControllerContext() { HttpContext = httpContext }
        };
    }

    [Fact(DisplayName = "VAS_001 - Lay chi tiet xe")]
    public async Task GetByIdAsync_ReturnsOk()
    {
        var result = await _controller.GetByIdAsync(1, CancellationToken.None).ConfigureAwait(true);
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "VAS_002 - Lay danh sach xe")]
    public async Task GetListAsync_ReturnsResult()
    {
        var result = await _controller.GetListAsync(new SieveModel(), CancellationToken.None).ConfigureAwait(true);
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
    }
}
