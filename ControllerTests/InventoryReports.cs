using Application.ApiContracts.InventoryReport.Responses;
using Application.Common.Models;
using Application.Features.InventoryReports.Queries.GetInventoryReportDetail;
using Application.Features.InventoryReports.Queries.GetInventoryReportSummary;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebAPI.Controllers.V1;
using Xunit;

namespace ControllerTests
{
    public class InventoryReports
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly InventoryReportController _controller;

        public InventoryReports()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new InventoryReportController(_mediatorMock.Object);
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        }

        [Fact(DisplayName = "IRP_001 - Lấy báo cáo tổng hợp xuất nhập tồn thành công")]
        public async Task IRP_001_GetSummaryReport_Success()
        {
            var mockResponse = new List<InventoryReportSummaryResponse>
            {
                new() 
                { 
                    ProductId = 1, 
                    ProductName = "Honda Winner X", 
                    ImportedQty = 10, 
                    ExportedQty = 2,
                    Variants = new List<InventoryReportVariantResponse>
                    {
                        new() { VariantId = 2, ImportedQty = 10, ExportedQty = 2 }
                    }
                }
            };
            var pagedResponse = new Domain.Primitives.PagedResult<InventoryReportSummaryResponse>(mockResponse, 1, 1, 10);

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetInventoryReportSummaryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Domain.Primitives.PagedResult<InventoryReportSummaryResponse>>.Success(pagedResponse));

            var result = await _controller.GetInventoryReportSummaryAsync(new GetInventoryReportSummaryQuery(), CancellationToken.None).ConfigureAwait(true);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(pagedResponse);
        }

        [Fact(DisplayName = "IRP_003 - Lấy chi tiết lịch sử nhập xuất của dòng có màu sắc thành công")]
        public async Task IRP_003_GetReportDetail_Success()
        {
            var mockResponse = new InventoryReportDetailResponse
            {
                Imports = [new InventoryTransactionResponse { PartnerName = "Supplier A", Qty = 5, Price = 12000000 }],
                Exports = [new InventoryTransactionResponse { PartnerName = "Customer B", Qty = 1, Price = 15000000 }]
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetInventoryReportDetailQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<InventoryReportDetailResponse>.Success(mockResponse));

            var result = await _controller.GetInventoryReportDetailAsync(2, 10, CancellationToken.None).ConfigureAwait(true);

            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult!.Value.Should().BeEquivalentTo(mockResponse);
        }
    }
}
