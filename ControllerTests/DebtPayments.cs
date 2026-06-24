using Application.ApiContracts.DebtPayment.Responses;
using Application.Common.Models;
using Application.Features.DebtPayments.Queries.GetReceiptsWithDebtBySupplierId;
using Application.Features.DebtPayments.Queries.GetSuppliersWithDebt;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using WebAPI.Controllers.V1;

using Domain.Primitives;

namespace ControllerTests
{
    public class DebtPayments
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly DebtPaymentsController _controller;

        public DebtPayments()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new DebtPaymentsController(_mediatorMock.Object);
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        [Fact(DisplayName = "DP_001 - Lấy danh sách nhà cung cấp đang còn nợ thành công")]
        public async Task DP_001_GetSuppliersWithDebt_ReturnsList()
        {
            var mockResponse = new List<SupplierDebtResponse>
            {
                new() { Id = 1, Name = "Supplier A", Phone = "0123", TotalDebt = 5000000 }
            };
            var pagedResponse = new PagedResult<SupplierDebtResponse>(mockResponse, mockResponse.Count, 1, 10);
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetSuppliersWithDebtQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<PagedResult<SupplierDebtResponse>>.Success(pagedResponse));
            var result = await _controller.GetSuppliersWithDebtAsync(null!, CancellationToken.None).ConfigureAwait(true);
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(pagedResponse);
        }

        [Fact(DisplayName = "DP_002 - Lấy danh sách nhà cung cấp còn nợ trả về rỗng khi không có nợ")]
        public async Task DP_002_GetSuppliersWithDebt_ReturnsEmpty()
        {
            var mockResponse = new List<SupplierDebtResponse>();
            var pagedResponse = new PagedResult<SupplierDebtResponse>(mockResponse, 0, 1, 10);
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetSuppliersWithDebtQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<PagedResult<SupplierDebtResponse>>.Success(pagedResponse));
            var result = await _controller.GetSuppliersWithDebtAsync(null!, CancellationToken.None).ConfigureAwait(true);
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(pagedResponse);
        }

        [Fact(DisplayName = "DP_003 - Lấy chi tiết các phiếu nhập còn nợ theo ID nhà cung cấp thành công")]
        public async Task DP_003_GetReceiptsWithDebt_ReturnsList()
        {
            var mockResponse = new List<InventoryReceiptDebtLineResponse>
            {
                new() { Id = 10, InventoryReceiptId = 1, PaidAmount = 1000000 }
            };
            _mediatorMock.Setup(
                m => m.Send(It.IsAny<GetReceiptsWithDebtBySupplierIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<InventoryReceiptDebtLineResponse>>.Success(mockResponse));
            var result = await _controller.GetReceiptsWithDebtBySupplierIdAsync(1, CancellationToken.None)
                .ConfigureAwait(true);
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(mockResponse);
        }

        [Fact(DisplayName = "DP_004 - Lấy chi tiết nợ trả về NotFound khi ID nhà cung cấp không tồn tại")]
        public async Task DP_004_GetReceiptsWithDebt_ReturnsNotFound()
        {
            _mediatorMock.Setup(
                m => m.Send(It.IsAny<GetReceiptsWithDebtBySupplierIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    Result<List<InventoryReceiptDebtLineResponse>>.Failure(Error.NotFound("Supplier not found")));
            var result = await _controller.GetReceiptsWithDebtBySupplierIdAsync(999, CancellationToken.None)
                .ConfigureAwait(true);
            result.Should().BeOfType<NotFoundObjectResult>();
        }

    }
}
