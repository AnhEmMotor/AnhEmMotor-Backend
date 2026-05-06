using Application.ApiContracts.Maintenance.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers.V1;

namespace ControllerTests;

public class Maintenance
{
    private readonly Mock<ISender> _senderMock;
    private readonly VehiclesController _vehiclesController;

    public Maintenance()
    {
        _senderMock = new Mock<ISender>();
        _vehiclesController = new VehiclesController();
        var httpContext = new DefaultHttpContext();
        _vehiclesController.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    [Fact(DisplayName = "MAINT_015 - Mapping dữ liệu phương tiện ra API")]
    public async Task GetVehicle_ReturnsVehicleResponse()
    {
        var result = await _vehiclesController.GetByIdAsync(1, CancellationToken.None).ConfigureAwait(true);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<VehicleResponse>(okResult.Value);
        Assert.Equal(1, response.Id);
    }
}
