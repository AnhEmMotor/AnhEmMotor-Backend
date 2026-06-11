using Application.Features.InventoryReceipts.Commands.UpdateInventoryReceiptStatus;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryLedger;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.ProductQuotations;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using Moq;
using System;
using InventoryReceiptEntity = Domain.Entities.InventoryReceipt;
using InventoryReceiptInfoEntity = Domain.Entities.InventoryReceiptInfo;

namespace UnitTests
{
    public class InventoryLedgers
    {
        private readonly Mock<IInventoryReceiptReadRepository> _readRepoMock;
        private readonly Mock<IInventoryReceiptUpdateRepository> _updateRepoMock;
        private readonly Mock<ICurrentUserContext> _currentUserContextMock;
        private readonly Mock<IInventoryLedgerRepository> _ledgerRepoMock;
        private readonly Mock<ISupplierDebtRepository> _supplierDebtRepoMock;
        private readonly Mock<IProductQuotationReadRepository> _ProductQuotationRepoMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;

        public InventoryLedgers()
        {
            _readRepoMock = new Mock<IInventoryReceiptReadRepository>();
            _updateRepoMock = new Mock<IInventoryReceiptUpdateRepository>();
            _currentUserContextMock = new Mock<ICurrentUserContext>();
            _ledgerRepoMock = new Mock<IInventoryLedgerRepository>();
            _supplierDebtRepoMock = new Mock<ISupplierDebtRepository>();
            _ProductQuotationRepoMock = new Mock<IProductQuotationReadRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
        }

        [Fact(DisplayName = "IL_001 - Tính toán tồn lũy kế chính xác cho giao dịch đầu tiên")]
        public async Task IL_001_FirstLedgerEntry_StockAfterEqualsImportQty()
        {
            var handler = new UpdateInventoryReceiptStatusCommandHandler(
                _readRepoMock.Object,
                _updateRepoMock.Object,
                _currentUserContextMock.Object,
                _ledgerRepoMock.Object,
                _ProductQuotationRepoMock.Object,
                null!,
                null!,
                _supplierDebtRepoMock.Object,
                _unitOfWorkMock.Object,
                new Mock<MediatR.IPublisher>().Object);
            var command = new UpdateInventoryReceiptStatusCommand { Id = 1, StatusId = "approve" };
            var existingReceipt = new InventoryReceiptEntity
            {
                Id = 1,
                StatusId = "sent",
                InventoryReceiptInfos =
                    new List<InventoryReceiptInfoEntity>
                    {
                        new()
                        {
                            Count = 5,
                            UnitPrice = 100000,
                            PurchaseRequestItem = new PurchaseRequestItem { ProductVariantId = 10, Quantity = 100 }
                        }
                    }
            };
            _readRepoMock.Setup(
                x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
                .ReturnsAsync(existingReceipt);
            _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingReceipt);
            _ledgerRepoMock.Setup(x => x.GetLastEntryAsync(10, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync((InventoryLedger?)null);
            InventoryLedger? savedLedger = null;
            _ledgerRepoMock.Setup(x => x.AddAsync(It.IsAny<InventoryLedger>(), It.IsAny<CancellationToken>()))
                .Callback<InventoryLedger, CancellationToken>((l, c) => savedLedger = l)
                .Returns(Task.CompletedTask);
            var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
            result.IsSuccess.Should().BeTrue();
            savedLedger.Should().NotBeNull();
            savedLedger!.StockAfter.Should().Be(5);
        }

        [Fact(DisplayName = "IL_002 - Tính toán tồn lũy kế chính xác khi đã có các giao dịch trước đó")]
        public async Task IL_002_SubsequentLedgerEntry_StockAfterCalculatedCorrectly()
        {
            var handler = new UpdateInventoryReceiptStatusCommandHandler(
                _readRepoMock.Object,
                _updateRepoMock.Object,
                _currentUserContextMock.Object,
                _ledgerRepoMock.Object,
                _ProductQuotationRepoMock.Object,
                null!,
                null!,
                _supplierDebtRepoMock.Object,
                _unitOfWorkMock.Object,
                new Mock<MediatR.IPublisher>().Object);
            var command = new UpdateInventoryReceiptStatusCommand { Id = 1, StatusId = "approve" };
            var existingReceipt = new InventoryReceiptEntity
            {
                Id = 1,
                StatusId = "sent",
                InventoryReceiptInfos =
                    new List<InventoryReceiptInfoEntity>
                    {
                        new()
                        {
                            Count = 5,
                            UnitPrice = 100000,
                            PurchaseRequestItem = new PurchaseRequestItem { ProductVariantId = 10, Quantity = 100 }
                        }
                    }
            };
            _readRepoMock.Setup(
                x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
                .ReturnsAsync(existingReceipt);
            _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingReceipt);
            var lastEntry = new InventoryLedger { StockAfter = 12 };
            _ledgerRepoMock.Setup(x => x.GetLastEntryAsync(10, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lastEntry);
            InventoryLedger? savedLedger = null;
            _ledgerRepoMock.Setup(x => x.AddAsync(It.IsAny<InventoryLedger>(), It.IsAny<CancellationToken>()))
                .Callback<InventoryLedger, CancellationToken>((l, c) => savedLedger = l)
                .Returns(Task.CompletedTask);
            var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
            result.IsSuccess.Should().BeTrue();
            savedLedger.Should().NotBeNull();
            savedLedger!.StockAfter.Should().Be(17);
        }

        [Fact(DisplayName = "IL_003 - Điền đầy đủ thông tin giao dịch vào sổ cái khi tạo mới")]
        public async Task IL_003_LedgerEntry_ContainsAllDetails()
        {
            var handler = new UpdateInventoryReceiptStatusCommandHandler(
                _readRepoMock.Object,
                _updateRepoMock.Object,
                _currentUserContextMock.Object,
                _ledgerRepoMock.Object,
                _ProductQuotationRepoMock.Object,
                null!,
                null!,
                _supplierDebtRepoMock.Object,
                _unitOfWorkMock.Object,
                new Mock<MediatR.IPublisher>().Object);
            var command = new UpdateInventoryReceiptStatusCommand { Id = 42, StatusId = "approve" };
            var existingReceipt = new InventoryReceiptEntity
            {
                Id = 42,
                StatusId = "sent",
                InventoryReceiptInfos =
                    new List<InventoryReceiptInfoEntity>
                    {
                        new()
                        {
                            Count = 3,
                            UnitPrice = 150000,
                            PurchaseRequestItem = new PurchaseRequestItem { ProductVariantId = 10, Quantity = 100 },
                            Supplier = new Domain.Entities.Supplier { Name = "Supplier Auto" }
                        }
                    }
            };
            _readRepoMock.Setup(
                x => x.GetByIdWithDetailsAsync(42, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
                .ReturnsAsync(existingReceipt);
            _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(42, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingReceipt);
            _ledgerRepoMock.Setup(x => x.GetLastEntryAsync(10, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync((InventoryLedger?)null);
            InventoryLedger? savedLedger = null;
            _ledgerRepoMock.Setup(x => x.AddAsync(It.IsAny<InventoryLedger>(), It.IsAny<CancellationToken>()))
                .Callback<InventoryLedger, CancellationToken>((l, c) => savedLedger = l)
                .Returns(Task.CompletedTask);
            var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
            result.IsSuccess.Should().BeTrue();
            savedLedger.Should().NotBeNull();
            savedLedger!.DocumentCode.Should().Be("IR-42");
            savedLedger.TransactionType.Should().Be("Nhập kho");
            savedLedger.ProductVariantId.Should().Be(10);
            savedLedger.PartnerName.Should().Be("Supplier Auto");
            savedLedger.ImportQty.Should().Be(3);
            savedLedger.ExportQty.Should().Be(0);
            savedLedger.UnitPrice.Should().Be(150000);
            savedLedger.TotalAmount.Should().Be(450000);
        }
    }
}

