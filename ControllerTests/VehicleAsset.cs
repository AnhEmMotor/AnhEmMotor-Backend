using Application.Common.Models;
using Application.Features.Vehicles.Commands.UpdateLicensePlate;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using WebAPI.Controllers.V1;
using Application.ApiContracts.Vehicle.Responses;

namespace ControllerTests;

public class VehicleAsset
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly VehicleController _controller;

    public VehicleAsset()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new VehicleController(_mediatorMock.Object);
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext() { HttpContext = httpContext };
    }

    [Fact(DisplayName = "VAS_004 - Cập nhật biển số xe sau khi đăng ký")]
    public async Task UpdateLicensePlate_ValidRequest_ReturnsOk()
    {
        // Arrange
        var assetId = 1;
        var newPlate = "59-A1 12345";
        var command = new UpdateLicensePlateCommand { LicensePlate = newPlate };
        var response = new VehicleResponse { Id = assetId, LicensePlate = newPlate };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateLicensePlateCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<VehicleResponse?>.Success(response));

        // Act
        var result = await _controller.UpdateLicensePlateAsync(assetId, command, CancellationToken.None).ConfigureAwait(true);

        // Assert
        result.Should().NotBeNull();
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
        var returnedAsset = okResult.Value.Should().BeOfType<VehicleResponse>().Subject;
        returnedAsset.Should().NotBeNull();
        returnedAsset.LicensePlate.Should().Be(newPlate);
    }
}
