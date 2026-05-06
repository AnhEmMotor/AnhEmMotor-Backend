using Application.Common.Models;
using Application.Features.Banners.Commands.CreateBanner;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using WebAPI.Controllers.V1;
using Xunit;

namespace ControllerTests;

public class Banner
{
    private readonly Mock<ISender> _senderMock;
    private readonly BannerController _bannerController;

    public Banner()
    {
        _senderMock = new Mock<ISender>();
        _bannerController = new BannerController(_senderMock.Object);
        
        var httpContext = new DefaultHttpContext();
        _bannerController.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    [Fact(DisplayName = "BANN_001 - Tạo Banner mới với đầy đủ thông tin (API)")]
    public async Task CreateBanner_ValidRequest_ReturnsBannerId()
    {
        // Arrange
        var command = new CreateBannerCommand 
        { 
            Title = "Banner KM", 
            ImageUrl = "http://anh-em.com/km.jpg", 
            LinkUrl = "http://anh-em.com/km", 
            Position = "Home",
            DisplayOrder = 1
        };
        _senderMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Success(50));

        // Act
        var result = await _bannerController.CreateAsync(command, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<Result<int>>(okResult.Value);
        Assert.Equal(50, response.Value);
    }
}
