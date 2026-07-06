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
_controller = new VehicleController(Mock.Of<MediatR.IMediator>());
_controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };
}

[Fact(DisplayName = "VAS_001 - Lay chi tiet xe")]
public async Task GetByIdAsync_ReturnsOk()
{
var result = await _controller.GetByIdAsync(1, CancellationToken.None).ConfigureAwait(true);
result.Should().NotBeNull();
var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
}

[Fact(DisplayName = "VAS_002 - Lay danh sach xe")]
public async Task GetListAsync_ReturnsResult()
{
var result = await _controller.GetListAsync(new Sieve.Models.SieveModel(), CancellationToken.None)
.ConfigureAwait(true);
result.Should().NotBeNull();
result.Should().BeOfType<ActionResult<IEnumerable<Application.ApiContracts.Vehicle.Responses.VehicleResponse>>>();
}
}
