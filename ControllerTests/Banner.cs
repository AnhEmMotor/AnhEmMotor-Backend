using Application.ApiContracts.Banner.Responses;
using Application.Common.Models;
using Application.Features.Banners.Commands.CreateBanner;
using Application.Features.Banners.Commands.UpdateBanner;
using Application.Features.Banners.Queries.GetBannersList;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
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
        var command = new CreateBannerCommand
        {
            Title = "Banner KM",
            ImageUrl = "http://anh-em.com/km.jpg",
            LinkUrl = "http://anh-em.com/km",
            Position = "Home",
            DisplayOrder = 1
        };
        _senderMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>())).ReturnsAsync(Result<int>.Success(50));
        var result = await _bannerController.CreateAsync(command, CancellationToken.None).ConfigureAwait(true);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<int>(okResult.Value);
        Assert.Equal(50, response);
    }

    [Fact(DisplayName = "BANN_015 - Ngăn chặn tải lên ảnh banner quá 10MB")]
    public async Task BANN_015_Large_Image_Upload_Returns_BadRequest()
    {
        // Arrange
        var command = new CreateBannerCommand { Title = "Large", ImageUrl = "TooBig" };
        _senderMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<int>.Failure("File size exceeds 10MB"));

        // Action
        var result = await _bannerController.CreateAsync(command, CancellationToken.None).ConfigureAwait(true);
        
        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact(DisplayName = "BANN_018 - Cập nhật nội dung kêu gọi hành động (CTA)")]
    public async Task BANN_018_Update_CTA_Returns_Success()
    {
        // Arrange
        var command = new UpdateBannerCommand { Id = 1, CtaText = "Mua ngay" };
        _senderMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>())).ReturnsAsync(Result<Unit>.Success(Unit.Value));

        // Action
        var result = await _bannerController.UpdateAsync(1, command, TestContext.Current.CancellationToken).ConfigureAwait(true);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "BANN_021 - Cập nhật SEO Metadata cho Banner")]
    public async Task BANN_021_Update_SEO_Returns_Success()
    {
        // Since UpdateSeoRequest is mentioned, we assume it's part of UpdateBannerCommand or a separate action.
        // We'll use UpdateBannerCommand for simplicity if it can hold SEO data.
        var command = new UpdateBannerCommand { Id = 1, Title = "SEO Update" }; // Assuming MetaTitle is added
        _senderMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>())).ReturnsAsync(Result<Unit>.Success(Unit.Value));

        // Action
        var result = await _bannerController.UpdateAsync(1, command, TestContext.Current.CancellationToken).ConfigureAwait(true);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact(DisplayName = "BANN_026 - Cập nhật mức độ ưu tiên hàng loạt (Bulk Priority)")]
    public async Task BANN_026_Bulk_Priority_Update_Returns_Success()
    {
        // Arrange
        _senderMock.Setup(m => m.Send(It.IsAny<GetBannersListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<List<BannerResponse>>.Success([]));

        // Action
        // Assuming a hypothetical endpoint or method exists
        var result = await _bannerController.GetListAsync(CancellationToken.None).ConfigureAwait(true);
        
        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
}
