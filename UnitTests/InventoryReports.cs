using Application.ApiContracts.InventoryReport.Responses;
using Application.Common.Models;
using Application.Features.InventoryReports.Queries.GetInventoryReportDetail;
using Application.Interfaces.Repositories.Product;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ProductVariant = Domain.Entities.ProductVariant;

namespace UnitTests
{
    public class InventoryReports
    {
        private readonly Mock<IProductReadRepository> _productRepoMock;

        public InventoryReports()
        {
            _productRepoMock = new Mock<IProductReadRepository>();
        }

        [Fact(DisplayName = "IRP_004 - Ngăn chặn lấy chi tiết biến thể có màu sắc nhưng thiếu tham số colorId")]
        public async Task IRP_004_GetReportDetail_MissingColorId_BadRequest()
        {
            var handler = new GetInventoryReportDetailQueryHandler(_productRepoMock.Object);

            var query = new GetInventoryReportDetailQuery
            {
                VariantId = 1,
                ColorId = null
            };

            var mockVariant = new ProductVariant
            {
                Id = 1,
                ProductVariantColors = new List<ProductVariantColor>
                {
                    new() { Id = 10, ColorName = "Red" }
                }
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
            var handler = new GetInventoryReportDetailQueryHandler(_productRepoMock.Object);

            var query = new GetInventoryReportDetailQuery
            {
                VariantId = 1,
                ColorId = null
            };

            var mockVariant = new ProductVariant
            {
                Id = 1,
                ProductVariantColors = new List<ProductVariantColor>(), // No colors
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
            var handler = new GetInventoryReportDetailQueryHandler(_productRepoMock.Object);

            var query = new GetInventoryReportDetailQuery
            {
                VariantId = 999,
                ColorId = null
            };

            _productRepoMock.Setup(x => x.GetVariantByIdWithDetailsAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((ProductVariant?)null);

            var result = await handler.Handle(query, CancellationToken.None).ConfigureAwait(true);

            result.IsFailure.Should().BeTrue();
            result.Error?.Code.Should().Be("NotFound");
        }
    }
}
