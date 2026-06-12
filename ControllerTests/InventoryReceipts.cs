using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Common.Models;
using Application.Features.InventoryReceipts.Commands.CreateInventoryReceipt;
using Application.Features.InventoryReceipts.Commands.UpdateInventoryReceiptStatus;
using Application.Features.InventoryReceipts.Queries.GetInventoryReceiptById;
using Application.Features.InventoryReceipts.Queries.GetInventoryReceiptsList;
using Application.Features.InventoryReceipts.Queries.GetInventoryReceiptStats;
using Domain.Primitives;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sieve.Models;
using System;
using WebAPI.Controllers.V1;

namespace ControllerTests;

public class InventoryReceipts
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly InventoryReceiptsController _controller;

    public InventoryReceipts()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new InventoryReceiptsController(_mediatorMock.Object);
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    #pragma warning disable IDE0079 
    #pragma warning disable CRR0035
    [Fact(
        DisplayName = "IR_018 - Lấy danh sách phiếu nhập kho có phân trang, lọc và sắp xếp theo điều kiện truyền vào.")]
    public async Task IR_018_GetInventoryReceipts_ReturnsPagedList()
    {
        var pagedResult = new PagedResult<InventoryReceiptListResponse>([], 0, 1, 10);
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetInventoryReceiptsListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedResult<InventoryReceiptListResponse>>.Success(pagedResult));
        var result = await _controller.GetInventoryReceiptsAsync(new SieveModel(), CancellationToken.None)
            .ConfigureAwait(true);
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(pagedResult);
    }

    [Fact(DisplayName = "IR_019 - Lấy thông tin chi tiết của một phiếu nhập kho dựa trên mã định danh hợp lệ.")]
    public async Task IR_019_GetInventoryReceiptById_ReturnsDetail()
    {
        var responseDetail = new InventoryReceiptDetailResponse { Id = 1, Notes = "Test details" };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetInventoryReceiptByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InventoryReceiptDetailResponse?>.Success(responseDetail));
        var result = await _controller.GetInventoryReceiptByIdAsync(1, CancellationToken.None).ConfigureAwait(true);
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(responseDetail);
    }

    [Fact(DisplayName = "IR_020 - Lấy thống kê số lượng phiếu nhập kho theo từng trạng thái thành công.")]
    public async Task IR_020_GetInventoryReceiptStats_ReturnsStats()
    {
        var stats = new InventoryReceiptStatsResponse();
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetInventoryReceiptStatsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InventoryReceiptStatsResponse>.Success(stats));
        var result = await _controller.GetInventoryReceiptStatsAsync(CancellationToken.None).ConfigureAwait(true);
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(stats);
    }

    [Fact(DisplayName = "IR_021 - Chặn người dùng không có quyền truy cập khi thực hiện tạo mới phiếu nhập kho.")]
    public async Task IR_021_CreateInventoryReceipt_Forbidden()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateInventoryReceiptCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.CreateInventoryReceiptAsync(new CreateInventoryReceiptCommand(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(
        DisplayName = "IR_022 - Chặn người dùng không có quyền phê duyệt khi thực hiện cập nhật trạng thái phiếu nhập kho.")]
    public async Task IR_022_UpdateInventoryReceiptStatus_Forbidden()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdateInventoryReceiptStatusCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.UpdateInventoryReceiptStatusAsync(
                1,
                new UpdateInventoryReceiptStatusCommand(),
                CancellationToken.None))
            .ConfigureAwait(true);
    }
    #pragma warning restore CRR0035
    #pragma warning restore IDE0079
}
