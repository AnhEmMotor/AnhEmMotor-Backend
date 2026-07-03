using Application.ApiContracts.InventoryReceipt.Responses;
using Application.Features.InventoryReceipts.Queries.GetInventoryReceiptsBySupplierId;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using Domain.Primitives;
using FluentAssertions;
using Moq;
using Sieve.Models;
using System.Linq.Expressions;

namespace UnitTests;

public class InventoryReceiptsSupplierHistory
{
    [Fact(DisplayName = "IR_050 - GetInventoryReceiptsBySupplierId returns paged list when supplier exists")]
    public async Task GetInventoryReceiptsBySupplierId_SupplierExists_ReturnsPagedList()
    {
        var supplierRepoMock = new Mock<ISupplierReadRepository>();
        var receiptRepoMock = new Mock<IInventoryReceiptReadRepository>();
        var query = new GetInventoryReceiptsBySupplierIdQuery
        {
            SupplierId = 1,
            SieveModel = new SieveModel { Page = 1, PageSize = 10 }
        };
        var supplier = new Domain.Entities.Supplier { Id = 1, Name = "Supplier A" };
        var paged = new PagedResult<InventoryReceiptListResponse>(
            new List<InventoryReceiptListResponse> { new() { Id = 1, SupplierId = 1 } },
            1,
            1,
            10);
        supplierRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(supplier);
        receiptRepoMock.Setup(
            x => x.GetPagedAsync<InventoryReceiptListResponse>(
                It.IsAny<SieveModel>(),
                It.IsAny<DataFetchMode>(),
                It.IsAny<Expression<Func<Domain.Entities.InventoryReceipt, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);
        var handler = new GetInventoryReceiptsBySupplierIdQueryHandler(receiptRepoMock.Object, supplierRepoMock.Object);
        var result = await handler.Handle(query, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(paged);
    }

    [Fact(
        DisplayName = "IR_051 - GetInventoryReceiptsBySupplierId returns empty list when supplier exists but has no receipts")]
    public async Task GetInventoryReceiptsBySupplierId_SupplierExistsNoData_ReturnsEmptyList()
    {
        var supplierRepoMock = new Mock<ISupplierReadRepository>();
        var receiptRepoMock = new Mock<IInventoryReceiptReadRepository>();
        var query = new GetInventoryReceiptsBySupplierIdQuery
        {
            SupplierId = 1,
            SieveModel = new SieveModel { Page = 1, PageSize = 10 }
        };
        var supplier = new Domain.Entities.Supplier { Id = 1, Name = "Supplier A" };
        var paged = new PagedResult<InventoryReceiptListResponse>(new List<InventoryReceiptListResponse>(), 0, 1, 10);
        supplierRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(supplier);
        receiptRepoMock.Setup(
            x => x.GetPagedAsync<InventoryReceiptListResponse>(
                It.IsAny<SieveModel>(),
                It.IsAny<DataFetchMode>(),
                It.IsAny<Expression<Func<Domain.Entities.InventoryReceipt, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);
        var handler = new GetInventoryReceiptsBySupplierIdQueryHandler(receiptRepoMock.Object, supplierRepoMock.Object);
        var result = await handler.Handle(query, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "IR_052 - GetInventoryReceiptsBySupplierId returns NotFound when supplier does not exist")]
    public async Task GetInventoryReceiptsBySupplierId_SupplierNotFound_ReturnsNotFound()
    {
        var supplierRepoMock = new Mock<ISupplierReadRepository>();
        var receiptRepoMock = new Mock<IInventoryReceiptReadRepository>();
        var query = new GetInventoryReceiptsBySupplierIdQuery
        {
            SupplierId = 999,
            SieveModel = new SieveModel { Page = 1, PageSize = 10 }
        };
        supplierRepoMock.Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync((Domain.Entities.Supplier?)null);
        var handler = new GetInventoryReceiptsBySupplierIdQueryHandler(receiptRepoMock.Object, supplierRepoMock.Object);
        var result = await handler.Handle(query, CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        result.Error?.Code.Should().Be("NotFound");
    }
}
