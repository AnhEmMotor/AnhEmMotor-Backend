using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers.V1;

namespace ControllerTests;

public class VehicleAsset
{
private readonly VehicleController _controller;

public VehicleAsset()
{
var httpContext = new DefaultHttpContext();
var mediatorMock = new Mock<MediatR.IMediator>();
mediatorMock.Setup(m => m.Send(It.IsAny<Application.Features.Vehicles.Queries.GetVehicles.GetVehiclesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Application.Common.Models.Result<Domain.Primitives.PagedResult<Application.ApiContracts.Vehicle.Responses.VehicleResponse>>.Success(new Domain.Primitives.PagedResult<Application.ApiContracts.Vehicle.Responses.VehicleResponse>([], 0, 1, 10)));
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
var result = await _controller.GetListAsync(new Sieve.Models.SieveModel(), CancellationToken.None)
.ConfigureAwait(true);
result.Should().NotBeNull();
result.Should().BeOfType<OkObjectResult>();
}
}
