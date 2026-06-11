using Application.Features.DebtPayments.Commands.RecordDebtPayment;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using FluentAssertions;
using Moq;
using InventoryReceiptEntity = Domain.Entities.InventoryReceipt;
using SupplierDebtEntity = Domain.Entities.SupplierDebt;

namespace UnitTests
{
    public class DebtPayments
    {
        private readonly Mock<IInventoryReceiptReadRepository> _readRepoMock;
        private readonly Mock<ISupplierDebtRepository> _supplierDebtRepoMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;

        public DebtPayments()
        {
            _readRepoMock = new Mock<IInventoryReceiptReadRepository>();
            _supplierDebtRepoMock = new Mock<ISupplierDebtRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
        }

        [Fact(DisplayName = "DP_005 - Thanh toán trả nợ một phần phiếu nhập thành công")]
        public async Task DP_005_PayDebt_PartialPayment_Success()
        {
            var handler = new RecordDebtPaymentCommandHandler(
                _supplierDebtRepoMock.Object,
                _readRepoMock.Object,
                _unitOfWorkMock.Object);
            var command = new RecordDebtPaymentCommand { LineId = 1, Amount = 1000000 };
            var existingReceipt = new InventoryReceiptEntity
            {
                Id = 1,
                StatusId = "approve",
                SupplierDebts = new List<SupplierDebtEntity>()
            };
            var supplierDebt = new SupplierDebtEntity
            {
                Id = 1,
                InventoryReceiptId = 1,
                SupplierId = 1,
                TotalAmount = 2000000,
                PaidAmount = 500000
            };
            existingReceipt.SupplierDebts.Add(supplierDebt);
            _supplierDebtRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(supplierDebt);
            _readRepoMock.Setup(
                x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
                .ReturnsAsync(existingReceipt);
            _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingReceipt);
            var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
            result.IsSuccess.Should().BeTrue();
            supplierDebt.PaidAmount.Should().Be(1500000);
            _supplierDebtRepoMock.Verify(x => x.Update(supplierDebt), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "DP_006 - Thanh toán hết toàn bộ khoản nợ còn lại thành công")]
        public async Task DP_006_PayDebt_FullPayment_Success()
        {
            var handler = new RecordDebtPaymentCommandHandler(
                _supplierDebtRepoMock.Object,
                _readRepoMock.Object,
                _unitOfWorkMock.Object);
            var command = new RecordDebtPaymentCommand { LineId = 1, Amount = 1500000 };
            var existingReceipt = new InventoryReceiptEntity
            {
                Id = 1,
                StatusId = "approve",
                SupplierDebts = new List<SupplierDebtEntity>()
            };
            var supplierDebt = new SupplierDebtEntity
            {
                Id = 1,
                InventoryReceiptId = 1,
                SupplierId = 1,
                TotalAmount = 2000000,
                PaidAmount = 500000
            };
            existingReceipt.SupplierDebts.Add(supplierDebt);
            _supplierDebtRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(supplierDebt);
            _readRepoMock.Setup(
                x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
                .ReturnsAsync(existingReceipt);
            _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingReceipt);
            var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
            result.IsSuccess.Should().BeTrue();
            supplierDebt.PaidAmount.Should().Be(2000000);
            _supplierDebtRepoMock.Verify(x => x.Update(supplierDebt), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "DP_007 - Ngăn chặn thanh toán khi số tiền gửi lên nhỏ hơn hoặc bằng 0")]
        public async Task DP_007_PayDebt_InvalidAmount()
        {
            var handler = new RecordDebtPaymentCommandHandler(
                _supplierDebtRepoMock.Object,
                _readRepoMock.Object,
                _unitOfWorkMock.Object);
            var command = new RecordDebtPaymentCommand { LineId = 1, Amount = 0 };
            var existingReceipt = new InventoryReceiptEntity
            {
                Id = 1,
                StatusId = "approve",
                SupplierDebts = new List<SupplierDebtEntity>()
            };
            var supplierDebt = new SupplierDebtEntity
            {
                Id = 1,
                InventoryReceiptId = 1,
                SupplierId = 1,
                TotalAmount = 2000000,
                PaidAmount = 500000
            };
            existingReceipt.SupplierDebts.Add(supplierDebt);
            _supplierDebtRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(supplierDebt);
            _readRepoMock.Setup(
                x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
                .ReturnsAsync(existingReceipt);
            var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
            result.IsFailure.Should().BeTrue();
            result.Error?.Code.Should().Be("BadRequest");
        }

        [Fact(DisplayName = "DP_008 - Ngăn chặn thanh toán khi số tiền vượt quá khoản nợ còn lại")]
        public async Task DP_008_PayDebt_AmountExceedsDebt()
        {
            var handler = new RecordDebtPaymentCommandHandler(
                _supplierDebtRepoMock.Object,
                _readRepoMock.Object,
                _unitOfWorkMock.Object);
            var command = new RecordDebtPaymentCommand { LineId = 1, Amount = 2000000 };
            var existingReceipt = new InventoryReceiptEntity
            {
                Id = 1,
                StatusId = "approve",
                SupplierDebts = new List<SupplierDebtEntity>()
            };
            var supplierDebt = new SupplierDebtEntity
            {
                Id = 1,
                InventoryReceiptId = 1,
                SupplierId = 1,
                TotalAmount = 2000000,
                PaidAmount = 500000
            };
            existingReceipt.SupplierDebts.Add(supplierDebt);
            _supplierDebtRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(supplierDebt);
            _readRepoMock.Setup(
                x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
                .ReturnsAsync(existingReceipt);
            var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
            result.IsFailure.Should().BeTrue();
            result.Error?.Code.Should().Be("BadRequest");
        }

        [Fact(DisplayName = "DP_009 - Ngăn chặn thanh toán nợ cho phiếu nhập chưa được phê duyệt")]
        public async Task DP_009_PayDebt_ReceiptNotApproved()
        {
            var handler = new RecordDebtPaymentCommandHandler(
                _supplierDebtRepoMock.Object,
                _readRepoMock.Object,
                _unitOfWorkMock.Object);
            var command = new RecordDebtPaymentCommand { LineId = 1, Amount = 1000000 };
            var existingReceipt = new InventoryReceiptEntity
            {
                Id = 1,
                StatusId = "draft",
                SupplierDebts = new List<SupplierDebtEntity>()
            };
            var supplierDebt = new SupplierDebtEntity
            {
                Id = 1,
                InventoryReceiptId = 1,
                SupplierId = 1,
                TotalAmount = 2000000,
                PaidAmount = 500000
            };
            existingReceipt.SupplierDebts.Add(supplierDebt);
            _supplierDebtRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(supplierDebt);
            _readRepoMock.Setup(
                x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
                .ReturnsAsync(existingReceipt);
            var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
            result.IsFailure.Should().BeTrue();
            result.Error?.Code.Should().Be("BadRequest");
        }
    }
}
