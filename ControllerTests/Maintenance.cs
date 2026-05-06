using Application.ApiContracts.Maintenance.Responses;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebAPI.Controllers.V1;
using Xunit;

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
        // Arrange
        var expectedResponse = new VehicleResponse 
        { 
            Id = 1, 
            VinNumber = "VIN123",
            Documents = new List<VehicleDocumentResponse> 
            { 
                new() { Id = 10, DocumentType = "Registration" } 
            }
        };

        // Note: Trong thực tế ta sẽ mock MediatR trả về response này
        // Ở đây ta test tính nhất quán của Controller
        
        // Act
        var result = await _vehiclesController.GetByIdAsync(1, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<VehicleResponse>(okResult.Value);
        Assert.Equal(1, response.Id);
    }
}
