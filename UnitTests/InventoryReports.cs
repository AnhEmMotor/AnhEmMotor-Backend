using Application.ApiContracts.InventoryReport.Responses;
using Application.Features.InventoryReports.Queries.ExportInventoryReport;
using Application.Features.InventoryReports.Queries.GetInventoryReportDetail;
using Application.Interfaces.Repositories.InventoryOnHand;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.Product;
using Application.Interfaces.Services.Excel;
using Domain.Entities;
using FluentAssertions;
using Moq;
using ProductVariant = Domain.Entities.ProductVariant;

namespace UnitTests
{
    public class InventoryReports
    {
        private readonly Mock<IProductReadRepository> _productRepoMock;
        private readonly Mock<IInventoryReceiptReadRepository> _receiptRepoMock;
        private readonly Mock<IInventoryOnHandReadRepository> _inventoryOnHandRepoMock;
        private readonly Mock<IInventoryReportExcelService> _excelServiceMock;

        public InventoryReports()
        {
            _productRepoMock = new Mock<IProductReadRepository>();
            _receiptRepoMock = new Mock<IInventoryReceiptReadRepository>();
            _receiptRepoMock.Setup(
                x => x.GetInfosByVariantAsync(It.IsAny<int>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            _inventoryOnHandRepoMock = new Mock<IInventoryOnHandReadRepository>();
            _excelServiceMock = new Mock<IInventoryReportExcelService>();
        }

        [Fact(DisplayName = "IRP_004 - Ngăn chặn lấy chi tiết biến thể có màu sắc nhưng thiếu tham số colorId")]
        public async Task IRP_004_GetReportDetail_MissingColorId_BadRequest()
        {
            var handler = new GetInventoryReportDetailQueryHandler(_productRepoMock.Object, _receiptRepoMock.Object);
            var query = new GetInventoryReportDetailQuery { VariantId = 1, ColorId = null };
            var mockVariant = new ProductVariant
            {
                Id = 1,
                ProductVariantColors = new List<ProductVariantColor> { new() { Id = 10, ColorName = "Red" } }
            };
            _productRepoMock.Setup(x => x.GetVariantByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockVariant);
            var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
            result.IsFailure.Should().BeTrue();
            result.Error?.Code.Should().Be("BadRequest");
        }

        [Fact(DisplayName = "IRP_005 - Cho phép lấy chi tiết biến thể không có màu sắc khi không truyền colorId")]
        public async Task IRP_005_GetReportDetail_NoColors_Success()
        {
            var handler = new GetInventoryReportDetailQueryHandler(_productRepoMock.Object, _receiptRepoMock.Object);
            var query = new GetInventoryReportDetailQuery { VariantId = 1, ColorId = null };
            var mockVariant = new ProductVariant
            {
                Id = 1,
                ProductVariantColors = new List<ProductVariantColor>(),
                InventoryReceiptInfos = [],
                OutputInfos = []
            };
            _productRepoMock.Setup(x => x.GetVariantByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockVariant);
            var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
        }

        [Fact(DisplayName = "IRP_006 - Lấy chi tiết báo cáo trả về NotFound khi ID biến thể không tồn tại")]
        public async Task IRP_006_GetReportDetail_VariantNotFound_NotFound()
        {
            var handler = new GetInventoryReportDetailQueryHandler(_productRepoMock.Object, _receiptRepoMock.Object);
            var query = new GetInventoryReportDetailQuery { VariantId = 999, ColorId = null };
            _productRepoMock.Setup(x => x.GetVariantByIdWithDetailsAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProductVariant?)null);
            var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);
            result.IsFailure.Should().BeTrue();
            result.Error?.Code.Should().Be("NotFound");
        }

        [Fact(DisplayName = "IRP_007 - Unit: ExportInventoryReportQueryHandler - Success")]
        public async Task IRP_007_ExportInventoryReport_Success()
        {
            var handler = new ExportInventoryReportQueryHandler(
                _inventoryOnHandRepoMock.Object,
                _excelServiceMock.Object);
            var items = new List<InventoryReportSummaryRowResponse> { new() };
            _inventoryOnHandRepoMock.Setup(
                x => x.GetInventoryReportSummaryRowsAsync(
                    It.IsAny<string?>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(items);
            var expectedBytes = new byte[] { 1, 2, 3 };
            _excelServiceMock.Setup(x => x.ExportInventoryReport(items, It.IsAny<int>(), It.IsAny<int>()))
                .Returns(expectedBytes);
            var result = await handler.Handle(new ExportInventoryReportQuery(), CancellationToken.None)
                .ConfigureAwait(true);
            result.IsSuccess.Should().BeTrue();
            result.Value!.FileContents.Should().BeSameAs(expectedBytes);
        }
    }
}
