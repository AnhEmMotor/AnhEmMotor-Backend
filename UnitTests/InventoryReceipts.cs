using Application.ApiContracts.InventoryReceipt.Requests;
using Application.Features.InventoryOnHand.Notifications;

using Application.Features.InventoryReceipts.Commands.CreateInventoryReceipt;
using Application.Features.InventoryReceipts.Commands.SendInventoryReceipt;
using Application.Features.InventoryReceipts.Commands.UpdateInventoryReceipt;
using Application.Features.InventoryReceipts.Commands.UpdateInventoryReceiptNotes;
using Application.Features.InventoryReceipts.Commands.UpdateInventoryReceiptStatus;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.InventoryLedger;
using Application.Interfaces.Repositories.InventoryOnHand;
using Application.Interfaces.Repositories.InventoryReceipt;
using Application.Interfaces.Repositories.Permission;
using Application.Interfaces.Repositories.ProductQuotations;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.PurchaseRequest;
using Application.Interfaces.Repositories.Supplier;
using Application.Interfaces.Repositories.SupplierDebt;
using Application.Interfaces.Repositories.Vehicle;
using Application.Interfaces.Services;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using MediatR;
using Moq;
using System;
using System.Linq;
using InventoryReceiptEntity = Domain.Entities.InventoryReceipt;
using InventoryReceiptInfoEntity = Domain.Entities.InventoryReceiptInfo;
using ProductVariant = Domain.Entities.ProductVariant;

namespace UnitTests;

public class InventoryReceipts
{
    private readonly Mock<IInventoryReceiptInsertRepository> _insertRepoMock;
    private readonly Mock<IInventoryReceiptReadRepository> _readRepoMock;
    private readonly Mock<IInventoryReceiptUpdateRepository> _updateRepoMock;
    private readonly Mock<IInventoryReceiptDeleteRepository> _deleteRepoMock;
    private readonly Mock<IPurchaseRequestReadRepository> _prReadRepoMock;
    private readonly Mock<IProductQuotationReadRepository> _quotationRepoMock;
    private readonly Mock<ISupplierReadRepository> _supplierRepoMock;
    private readonly Mock<IProductVariantReadRepository> _variantRepoMock;
    private readonly Mock<IVehicleReadRepository> _vehicleReadRepoMock;
    private readonly Mock<ICurrentUserContext> _currentUserContextMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPermissionReadRepository> _permissionRepoMock;
    private readonly Mock<IInventoryLedgerRepository> _ledgerRepoMock;
    private readonly Mock<ISupplierDebtInsertRepository> _supplierDebtRepoMock;
    private readonly Mock<IVehicleUpdateRepository> _vehicleUpdateRepoMock;
    private readonly Mock<IProductQuotationReadRepository> _ProductQuotationRepoMock;

    public InventoryReceipts()
    {
        _insertRepoMock = new Mock<IInventoryReceiptInsertRepository>();
        _readRepoMock = new Mock<IInventoryReceiptReadRepository>();
        _updateRepoMock = new Mock<IInventoryReceiptUpdateRepository>();
        _deleteRepoMock = new Mock<IInventoryReceiptDeleteRepository>();
        _prReadRepoMock = new Mock<IPurchaseRequestReadRepository>();
        _quotationRepoMock = new Mock<IProductQuotationReadRepository>();
        _supplierRepoMock = new Mock<ISupplierReadRepository>();
        _variantRepoMock = new Mock<IProductVariantReadRepository>();
        _vehicleReadRepoMock = new Mock<IVehicleReadRepository>();
        _currentUserContextMock = new Mock<ICurrentUserContext>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _permissionRepoMock = new Mock<IPermissionReadRepository>();
        _ledgerRepoMock = new Mock<IInventoryLedgerRepository>();
        _supplierDebtRepoMock = new Mock<ISupplierDebtInsertRepository>();
        _vehicleUpdateRepoMock = new Mock<IVehicleUpdateRepository>();
        _ProductQuotationRepoMock = new Mock<IProductQuotationReadRepository>();
    }

    #pragma warning disable IDE0079 
    #pragma warning disable CRR0035

    [Fact(DisplayName = "IR_001 - Tạo phiếu nhập kho thành công với dữ liệu hợp lệ và lưu ở trạng thái nháp.")]
    public async Task IR_001_CreateInventoryReceipt_Success()
    {
        var handler = new CreateInventoryReceiptCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _prReadRepoMock.Object,
            _supplierRepoMock.Object,
            _variantRepoMock.Object,
            _vehicleReadRepoMock.Object,
            _vehicleUpdateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreateInventoryReceiptCommand
        {
            Notes = "Valid notes",
            Products = [new CreateInventoryReceiptInfoRequest { PurchaseRequestItemId = 10, Count = 5 }]
        };
        var mockPrItems = new List<PurchaseRequestItem> { new() { Id = 10, ProductVariantId = 1, Quantity = 100 } };
        var mockVariants = new List<ProductVariant>
        {
            new() { Id = 1, VariantName = "Variant 1", ProductVariantColors = [] }
        };
        _prReadRepoMock.Setup(x => x.GetItemsByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockPrItems);
        _variantRepoMock.Setup(
            x => x.GetByIdAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(mockVariants);
        var createdReceipt = new InventoryReceiptEntity { Id = 10, Notes = "Valid notes", StatusId = "draft" };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdReceipt);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue(result.Error?.Message);
        result.Value.Should().NotBeNull();
        string.Compare(result.Value!.Notes, "Valid notes").Should().Be(0);
        string.Compare(result.Value.StatusId, "draft").Should().Be(0);
    }

    [Fact(DisplayName = "IR_002 - Ngăn chặn tạo phiếu nhập kho khi mã yêu cầu mua hàng không tồn tại trên hệ thống.")]
    public async Task IR_002_CreateInventoryReceipt_PurchaseRequestNotFound()
    {
<<<<<<< HEAD
<<<<<<< HEAD
        var validator = new CreateInputInfoCommandValidator();
        var command = new CreateInputInfoRequest { ProductVarientId = null, Count = 10, InputPrice = 100000 };
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ProductVarientId);
    }

    [Fact(DisplayName = "INPUT_050 - Validator kiểm tra UpdateInputRequest với Products chứa Quantity âm")]
    public void UpdateInputProductValidator_NegativeQuantity_ReturnsValidationError()
    {
        var validator = new UpdateInputInfoCommandValidator();
        var command = new UpdateInputInfoRequest { ProductId = 1, Count = -10, InputPrice = 100000 };
        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Count);
    }

    [Fact(DisplayName = "INPUT_051 - Validator kiểm tra UpdateInputStatusRequest với StatusId không hợp lệ")]
    public void UpdateInputStatusValidator_InvalidStatusId_ReturnsValidationError()
    {
        var statusId = "invalid_status";
        bool isValid = Domain.Constants.Input.InputStatus.IsValid(statusId);
        isValid.Should().BeFalse();
    }

    [Fact(DisplayName = "INPUT_053 - Handler xử lý CreateInput ném ngoại lệ khi DB connection fail")]
    public async Task CreateInputHandler_DbConnectionFails_ThrowsException()
    {
        var mockInsertRepo = new Mock<IInputInsertRepository>();
        var mockReadRepo = new Mock<IInputReadRepository>();
        mockInsertRepo.Setup(x => x.Add(It.IsAny<Input>())).Throws(new Exception("DB Connection Failed"));
        var handler = new CreateInputCommandHandler(
            mockInsertRepo.Object,
            mockReadRepo.Object,
            Mock.Of<ISupplierReadRepository>(),
            Mock.Of<IProductVariantReadRepository>(),
            Mock.Of<IVehicleReadRepository>(),
            Mock.Of<Application.Interfaces.Repositories.SupplierContract.ISupplierContractReadRepository>(),
            Mock.Of<IUnitOfWork>());

        var command = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products = [new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 }]
        };
=======
=======
>>>>>>> main
        var handler = new CreateInventoryReceiptCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _prReadRepoMock.Object,
            _supplierRepoMock.Object,
            _variantRepoMock.Object,
            _vehicleReadRepoMock.Object,
            _vehicleUpdateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreateInventoryReceiptCommand { PurchaseRequestId = 99, Products = [] };
        _prReadRepoMock.Setup(x => x.GetByIdWithDetailsAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PurchaseRequest?)null);
<<<<<<< HEAD
>>>>>>> main
=======
>>>>>>> main
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        string.Compare(result.Error?.Code ?? string.Empty, "NotFound").Should().Be(0);
    }

    [Fact(DisplayName = "IR_003 - Ngăn chặn tạo phiếu nhập kho khi yêu cầu mua hàng liên kết chưa được phê duyệt.")]
    public async Task IR_003_CreateInventoryReceipt_PurchaseRequestNotApproved()
    {
        var handler = new CreateInventoryReceiptCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _prReadRepoMock.Object,
            _supplierRepoMock.Object,
            _variantRepoMock.Object,
            _vehicleReadRepoMock.Object,
            _vehicleUpdateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreateInventoryReceiptCommand { PurchaseRequestId = 1, Products = [] };
        var pr = new PurchaseRequest { Id = 1, Status = "draft" };
        _prReadRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(pr);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        string.Compare(result.Error?.Code ?? string.Empty, "BadRequest").Should().Be(0);
    }

    [Fact(DisplayName = "IR_004 - Ngăn chặn tạo phiếu nhập kho khi biến thể sản phẩm không tồn tại hoặc đã bị xóa.")]
    public async Task IR_004_CreateInventoryReceipt_VariantNotFound()
    {
        var handler = new CreateInventoryReceiptCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _prReadRepoMock.Object,
            _supplierRepoMock.Object,
            _variantRepoMock.Object,
            _vehicleReadRepoMock.Object,
            _vehicleUpdateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreateInventoryReceiptCommand
        {
            Products = [new CreateInventoryReceiptInfoRequest { PurchaseRequestItemId = 10, Count = 1 }]
        };
        var mockPrItems = new List<PurchaseRequestItem> { new() { Id = 10, ProductVariantId = 99, Quantity = 100 } };
        _prReadRepoMock.Setup(x => x.GetItemsByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockPrItems);
        _variantRepoMock.Setup(
            x => x.GetByIdAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync([]);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        string.Compare(result.Error?.Code ?? string.Empty, "BadRequest").Should().Be(0);
    }

    [Fact(DisplayName = "IR_005 - Ngăn chặn tạo phiếu nhập kho khi màu sắc được chọn không thuộc biến thể sản phẩm.")]
    public async Task IR_005_CreateInventoryReceipt_InvalidColorForVariant()
    {
        var handler = new CreateInventoryReceiptCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _prReadRepoMock.Object,
            _supplierRepoMock.Object,
            _variantRepoMock.Object,
            _vehicleReadRepoMock.Object,
            _vehicleUpdateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreateInventoryReceiptCommand
        {
            Products = [new CreateInventoryReceiptInfoRequest { PurchaseRequestItemId = 10, Count = 1 }]
        };
        var mockPrItems = new List<PurchaseRequestItem>
        {
            new() { Id = 10, ProductVariantId = 1, ProductVariantColorId = 99, Quantity = 100 }
        };
        var mockVariants = new List<ProductVariant>
        {
            new() { Id = 1, VariantName = "Variant 1", ProductVariantColors = [new ProductVariantColor { Id = 2 }] }
        };
        _prReadRepoMock.Setup(x => x.GetItemsByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockPrItems);
        _variantRepoMock.Setup(
            x => x.GetByIdAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(mockVariants);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        string.Compare(result.Error?.Code ?? string.Empty, "BadRequest").Should().Be(0);
    }

    [Theory(
        DisplayName = "IR_006 - Ngăn chặn tạo phiếu nhập kho khi danh sách xe máy có số khung hoặc số máy bị trùng lặp ngay trong yêu cầu gửi lên.")]
    [InlineData("VIN123", "ENG123", "VIN123", "ENG456")]
    [InlineData("VIN123", "ENG123", "VIN456", "ENG123")]
    public async Task IR_006_CreateInventoryReceipt_DuplicateVehiclesInRequestAsync(
        string vin1,
        string eng1,
        string vin2,
        string eng2)
    {
        var handler = new CreateInventoryReceiptCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _prReadRepoMock.Object,
            _supplierRepoMock.Object,
            _variantRepoMock.Object,
            _vehicleReadRepoMock.Object,
            _vehicleUpdateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreateInventoryReceiptCommand
        {
            Products =
                [new CreateInventoryReceiptInfoRequest
                {
                    PurchaseRequestItemId = 10,
                    Count = 2,
                    Vehicles =
                        [new VehicleInventoryReceiptRequest { VinNumber = vin1, EngineNumber = eng1 }, new VehicleInventoryReceiptRequest
                            {
                                VinNumber = vin2,
                                EngineNumber = eng2
                            }]
                }]
        };
        var mockPrItems = new List<PurchaseRequestItem> { new() { Id = 10, ProductVariantId = 1, Quantity = 100 } };
        var mockVariants = new List<ProductVariant>
        {
            new()
            {
                Id = 1,
                VariantName = "Variant 1",
                ProductVariantColors = [],
                Product =
                    new Domain.Entities.Product
                    {
                        ProductCategory = new Domain.Entities.ProductCategory { ManagementType = "vin_number" }
                    }
            }
        };
        _prReadRepoMock.Setup(x => x.GetItemsByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockPrItems);
        _variantRepoMock.Setup(
            x => x.GetByIdAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(mockVariants);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        string.Compare(result.Error?.Code ?? string.Empty, "BadRequest").Should().Be(0);
    }

    [Theory(
        DisplayName = "IR_007 - Ngăn chặn tạo phiếu nhập kho khi số khung hoặc số máy của xe máy đã tồn tại trong cơ sở dữ liệu.")]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task IR_007_CreateInventoryReceipt_ExistingVehiclesInDatabase(bool vinExists, bool engineExists)
    {
        var handler = new CreateInventoryReceiptCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _prReadRepoMock.Object,
            _supplierRepoMock.Object,
            _variantRepoMock.Object,
            _vehicleReadRepoMock.Object,
            _vehicleUpdateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreateInventoryReceiptCommand
        {
            Products =
                [new CreateInventoryReceiptInfoRequest
                {
                    PurchaseRequestItemId = 10,
                    Count = 1,
                    Vehicles = [new VehicleInventoryReceiptRequest { VinNumber = "VIN123", EngineNumber = "ENG123" }]
                }]
        };
        var mockPrItems = new List<PurchaseRequestItem> { new() { Id = 10, ProductVariantId = 1, Quantity = 100 } };
        var mockVariants = new List<ProductVariant>
        {
            new()
            {
                Id = 1,
                VariantName = "Variant 1",
                ProductVariantColors = [],
                Product =
                    new Domain.Entities.Product
                    {
                        ProductCategory = new Domain.Entities.ProductCategory { ManagementType = "vin_number" }
                    }
            }
        };
        _prReadRepoMock.Setup(x => x.GetItemsByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockPrItems);
        _variantRepoMock.Setup(
            x => x.GetByIdAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(mockVariants);
        _vehicleReadRepoMock.Setup(x => x.ExistsByVinAsync("VIN123", 1, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vinExists);
        _vehicleReadRepoMock.Setup(x => x.ExistsByEngineNumberAsync("ENG123", 1, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(engineExists);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        string.Compare(result.Error?.Code ?? string.Empty, "BadRequest").Should().Be(0);
    }

    [Fact(
        DisplayName = "IR_008 - Cập nhật thông tin phiếu nhập kho thành công khi phiếu đang ở trạng thái nháp hoặc chờ duyệt.")]
    public async Task IR_008_UpdateInventoryReceipt_Success()
    {
        var handler = new UpdateInventoryReceiptCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _deleteRepoMock.Object,
            _prReadRepoMock.Object,
            _variantRepoMock.Object,
            _permissionRepoMock.Object,
            _vehicleUpdateRepoMock.Object,
            _vehicleReadRepoMock.Object,
            _unitOfWorkMock.Object,
            _currentUserContextMock.Object);
        var command = new UpdateInventoryReceiptCommand { Id = 1, Notes = "Updated notes", Products = [] };
        var existingReceipt = new InventoryReceiptEntity
        {
            Id = 1,
            StatusId = "draft",
            Notes = "Old notes",
            InventoryReceiptInfos = []
        };
<<<<<<< HEAD
<<<<<<< HEAD
        var response = input.Adapt<InputDetailResponse>(config);
        response.TotalPayable.Should().Be((2 * 150000) + (3 * 50000) + 0 + 0);
    }
    

    [Fact(DisplayName = "PRODUCT_196 - Tạo phiếu nhập với sản phẩm quản lý theo số khung nhưng thiếu thông tin xe")]
    public async Task CreateInputHandler_VinManagedProduct_MissingVehicles_ReturnsError()
    {
        var mockInsertRepo = new Mock<IInputInsertRepository>();
        var mockReadRepo = new Mock<IInputReadRepository>();
        var mockVariantRepo = new Mock<IProductVariantReadRepository>();
        var mockVehicleReadRepo = new Mock<IVehicleReadRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();

        var variant = new ProductVariant
        {
            ProductId = 1,
            Product = new Domain.Entities.Product
            {
                Name = "Xe máy Test",
                ProductCategory = new Domain.Entities.ProductCategory
                {
                    ManagementType = "vin_number"
                }
            }
        };

        mockVariantRepo.Setup(x => x.GetByIdAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync([variant]);

        var handler = new CreateInputCommandHandler(
            mockInsertRepo.Object,
            mockReadRepo.Object,
            Mock.Of<ISupplierReadRepository>(),
            mockVariantRepo.Object,
            mockVehicleReadRepo.Object,
            Mock.Of<Application.Interfaces.Repositories.SupplierContract.ISupplierContractReadRepository>(),
            mockUnitOfWork.Object);

        var command = new CreateInputCommand
        {
            SupplierId = null,
            Products = [new CreateInputInfoRequest { ProductId = 1, Count = 2, InputPrice = 100000, Vehicles = null }]
        };

=======
=======
>>>>>>> main
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingReceipt);
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReceipt);
<<<<<<< HEAD
>>>>>>> main
=======
>>>>>>> main
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        _updateRepoMock.Verify(x => x.Update(It.IsAny<InventoryReceiptEntity>()), Times.Once);
    }

    [Theory(
        DisplayName = "IR_009 - Ngăn chặn cập nhật phiếu nhập kho khi phiếu đã ở trạng thái phê duyệt hoặc bị từ chối.")]
    [InlineData("approve")]
    [InlineData("reject")]
    public async Task IR_009_UpdateInventoryReceipt_Failed_WhenCannotEdit(string status)
    {
        var handler = new UpdateInventoryReceiptCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _deleteRepoMock.Object,
            _prReadRepoMock.Object,
            _variantRepoMock.Object,
            _permissionRepoMock.Object,
            _vehicleUpdateRepoMock.Object,
            _vehicleReadRepoMock.Object,
            _unitOfWorkMock.Object,
            _currentUserContextMock.Object);
        var command = new UpdateInventoryReceiptCommand
        {
            Id = 1,
            Notes = "Updated notes",
            Products = [new UpdateInventoryReceiptInfoRequest { Count = 5 }]
        };
<<<<<<< HEAD
<<<<<<< HEAD

        mockVariantRepo.Setup(x => x.GetByIdAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync([variant]);

        mockVehicleReadRepo.Setup(x => x.ExistsByVinAsync("VIN123", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var handler = new CreateInputCommandHandler(
            mockInsertRepo.Object,
            mockReadRepo.Object,
            Mock.Of<ISupplierReadRepository>(),
            mockVariantRepo.Object,
            mockVehicleReadRepo.Object,
            Mock.Of<Application.Interfaces.Repositories.SupplierContract.ISupplierContractReadRepository>(),
            mockUnitOfWork.Object);

        var command = new CreateInputCommand
        {
            SupplierId = null,
            Products = [
                new CreateInputInfoRequest { 
                    ProductId = 1, 
                    Count = 2, 
                    InputPrice = 100000, 
                    Vehicles = [
                        new VehicleInputRequest { VinNumber = "VIN123", EngineNumber = "ENG123" },
                        new VehicleInputRequest { VinNumber = "VIN456", EngineNumber = "ENG456" }
                    ] 
                }
            ]
        };

=======
        var existingReceipt = new InventoryReceiptEntity { Id = 1, StatusId = status, Notes = "Old notes" };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingReceipt);
>>>>>>> main
=======
        var existingReceipt = new InventoryReceiptEntity { Id = 1, StatusId = status, Notes = "Old notes" };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingReceipt);
>>>>>>> main
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        string.Compare(result.Error?.Code ?? string.Empty, "BadRequest").Should().Be(0);
    }

    [Fact(
        DisplayName = "IR_010 - Phê duyệt phiếu nhập kho thành công, chuyển trạng thái sang Approve và tự động tăng số lượng tồn kho sản phẩm.")]
    public async Task IR_010_ApproveInventoryReceipt_Success()
    {
        var handler = new UpdateInventoryReceiptStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _insertRepoMock.Object,
            _currentUserContextMock.Object,
            _ledgerRepoMock.Object,
            _supplierDebtRepoMock.Object,
            _unitOfWorkMock.Object,
            new Mock<IPublisher>().Object);
        var command = new UpdateInventoryReceiptStatusCommand { Id = 1, StatusId = "approve" };
        var existingReceipt = new InventoryReceiptEntity { Id = 1, StatusId = "sent" };
        var currentUserId = Guid.NewGuid();
        _currentUserContextMock.Setup(x => x.GetUserId()).Returns(currentUserId);
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingReceipt);
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReceipt);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        string.Compare(existingReceipt.StatusId, "approve").Should().Be(0);
        existingReceipt.ConfirmedBy.Should().Be(currentUserId);
        _updateRepoMock.Verify(x => x.Update(existingReceipt), Times.Once);
    }

    [Fact(
        DisplayName = "IR_011 - Từ chối phê duyệt phiếu nhập kho thành công, chuyển trạng thái sang Reject mà không thay đổi tồn kho.")]
    public async Task IR_011_RejectInventoryReceipt_Success()
    {
        var handler = new UpdateInventoryReceiptStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _insertRepoMock.Object,
            _currentUserContextMock.Object,
            _ledgerRepoMock.Object,
            _supplierDebtRepoMock.Object,
            _unitOfWorkMock.Object,
            new Mock<IPublisher>().Object);
        var command = new UpdateInventoryReceiptStatusCommand { Id = 1, StatusId = "reject" };
        var existingReceipt = new InventoryReceiptEntity { Id = 1, StatusId = "sent" };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingReceipt);
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReceipt);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        string.Compare(existingReceipt.StatusId, "reject").Should().Be(0);
        _updateRepoMock.Verify(x => x.Update(existingReceipt), Times.Once);
    }

    [Fact(
        DisplayName = "IR_012 - Cập nhật nhanh ghi chú của phiếu nhập kho thành công và tự động loại bỏ các thẻ HTML nguy hại.")]
    public async Task IR_012_UpdateInventoryReceiptNotes_Success()
    {
        var handler = new UpdateInventoryReceiptNotesCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdateInventoryReceiptNotesCommand { Id = 1, Notes = "New Notes" };
        var existingReceipt = new InventoryReceiptEntity { Id = 1, Notes = "Old Notes" };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingReceipt);
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReceipt);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        string.Compare(existingReceipt.Notes, "New Notes").Should().Be(0);
        _updateRepoMock.Verify(x => x.Update(existingReceipt), Times.Once);
    }



    [Fact(DisplayName = "IR_023 - Tạo phiếu nhập kho thành công và lưu người tạo")]
    public async Task IR_023_CreateInventoryReceipt_SaveCreatedBy_Success()
    {
        var currentUserId = Guid.NewGuid();
        _currentUserContextMock.Setup(x => x.GetUserId()).Returns(currentUserId);
        var handler = new CreateInventoryReceiptCommandHandler(
            _insertRepoMock.Object,
            _readRepoMock.Object,
            _prReadRepoMock.Object,
            _supplierRepoMock.Object,
            _variantRepoMock.Object,
            _vehicleReadRepoMock.Object,
            _vehicleUpdateRepoMock.Object,
            _unitOfWorkMock.Object,
            _currentUserContextMock.Object);
        var command = new CreateInventoryReceiptCommand
        {
            Notes = "Valid notes",
            Products = [new CreateInventoryReceiptInfoRequest { PurchaseRequestItemId = 10, Count = 5 }]
        };
        var mockPrItems = new List<PurchaseRequestItem> { new() { Id = 10, ProductVariantId = 1, Quantity = 100 } };
        var mockVariants = new List<ProductVariant>
        {
            new() { Id = 1, VariantName = "Variant 1", ProductVariantColors = [] }
        };
        _prReadRepoMock.Setup(x => x.GetItemsByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockPrItems);
        _variantRepoMock.Setup(
            x => x.GetByIdAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(mockVariants);
        InventoryReceiptEntity? capturedReceipt = null;
        _insertRepoMock.Setup(x => x.Add(It.IsAny<InventoryReceiptEntity>()))
            .Callback<InventoryReceiptEntity>(r => capturedReceipt = r);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new InventoryReceiptEntity
                {
                    Id = 10,
                    Notes = "Valid notes",
                    StatusId = "draft",
                    CreatedBy = currentUserId
                });
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        capturedReceipt.Should().NotBeNull();
        capturedReceipt!.CreatedBy.Should().Be(currentUserId);
    }

    [Fact(DisplayName = "IR_024 - Gửi phiếu nhập kho thành công và lưu người gửi")]
    public async Task IR_024_SendInventoryReceipt_SaveSentBy_Success()
    {
        var currentUserId = Guid.NewGuid();
        _currentUserContextMock.Setup(x => x.GetUserId()).Returns(currentUserId);
        var handler = new SendInventoryReceiptCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _currentUserContextMock.Object,
            _unitOfWorkMock.Object);
        var command = new SendInventoryReceiptCommand { Id = 1 };
        var receipt = new InventoryReceiptEntity { Id = 1, StatusId = "draft" };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(receipt);
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(receipt);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        receipt.StatusId.Should().Be("sent");
        receipt.SentBy.Should().Be(currentUserId);
        _updateRepoMock.Verify(x => x.Update(receipt), Times.Once);
    }

    [Fact(DisplayName = "IR_025 - Phê duyệt phiếu nhập kho thành công và lưu người duyệt")]
    public async Task IR_025_ApproveInventoryReceipt_SaveApprovedBy_Success()
    {
        var currentUserId = Guid.NewGuid();
        _currentUserContextMock.Setup(x => x.GetUserId()).Returns(currentUserId);
        var handler = new UpdateInventoryReceiptStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _insertRepoMock.Object,
            _currentUserContextMock.Object,
            _ledgerRepoMock.Object,
            _supplierDebtRepoMock.Object,
            _unitOfWorkMock.Object,
            new Mock<IPublisher>().Object);
        var command = new UpdateInventoryReceiptStatusCommand { Id = 1, StatusId = "approve" };
        var receipt = new InventoryReceiptEntity { Id = 1, StatusId = "sent" };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(receipt);
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(receipt);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        receipt.StatusId.Should().Be("approve");
        receipt.ApprovedBy.Should().Be(currentUserId);
        receipt.RejectedBy.Should().BeNull();
        _updateRepoMock.Verify(x => x.Update(receipt), Times.Once);
    }

    [Fact(DisplayName = "IR_026 - Từ chối phiếu nhập kho thành công và lưu người từ chối")]
    public async Task IR_026_RejectInventoryReceipt_SaveRejectedBy_Success()
    {
        var currentUserId = Guid.NewGuid();
        _currentUserContextMock.Setup(x => x.GetUserId()).Returns(currentUserId);
        var handler = new UpdateInventoryReceiptStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _insertRepoMock.Object,
            _currentUserContextMock.Object,
            _ledgerRepoMock.Object,
            _supplierDebtRepoMock.Object,
            _unitOfWorkMock.Object,
            new Mock<IPublisher>().Object);
        var command = new UpdateInventoryReceiptStatusCommand { Id = 1, StatusId = "reject" };
        var receipt = new InventoryReceiptEntity { Id = 1, StatusId = "sent" };
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(receipt);
        _readRepoMock.Setup(x => x.GetByIdWithDetailsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(receipt);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        receipt.StatusId.Should().Be("reject");
        receipt.RejectedBy.Should().Be(currentUserId);
        receipt.ApprovedBy.Should().BeNull();
        receipt.ConfirmedBy.Should().BeNull();
        _updateRepoMock.Verify(x => x.Update(receipt), Times.Once);
    }

    [Fact(DisplayName = "IR_029 - Handler gọi đúng hàm tính tồn kho")]
    public async Task IR_029_Handler_CallsRecalculateAsync()
    {
        var repoMock = new Mock<IInventoryOnHandUpdateRepository>();
        var handler = new InventoryChangedNotificationHandler(repoMock.Object);
        var combos = new HashSet<(int VariantId, int? ColorId)> { (1, null), (2, 5) };
        var notification = new InventoryChangedNotification(combos);
        await handler.Handle(notification, CancellationToken.None).ConfigureAwait(true);
        repoMock.Verify(r => r.RecalculateAsync(1, null, It.IsAny<CancellationToken>()), Times.Once);
        repoMock.Verify(r => r.RecalculateAsync(2, 5, It.IsAny<CancellationToken>()), Times.Once);
    }
    #pragma warning restore CRR0035
    #pragma warning restore IDE0079
}

