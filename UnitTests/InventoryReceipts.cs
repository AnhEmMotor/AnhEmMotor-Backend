using Application.ApiContracts.Input.Requests;
using Application.Features.Inputs.Commands.CreateInput;
using Application.Features.Inputs.Commands.UpdateInput;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;

namespace UnitTests;

public class InventoryReceipts
{
#pragma warning disable IDE0079 
#pragma warning disable CRR0035
    [Fact(DisplayName = "INPUT_007 - Tạo phiếu nhập với Quantity là số âm")]
    public void CreateInputProductValidator_NegativeQuantity_ReturnsValidationError()
    {
        var validator = new CreateInputInfoCommandValidator();
        var command = new CreateInputInfoRequest { ProductId = 1, Count = -5, InputPrice = 100000 };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Count);
    }

    [Fact(DisplayName = "INPUT_008 - Tạo phiếu nhập với Quantity là 0")]
    public void CreateInputProductValidator_ZeroQuantity_ReturnsValidationError()
    {
        var validator = new CreateInputInfoCommandValidator();
        var command = new CreateInputInfoRequest { ProductId = 1, Count = 0, InputPrice = 100000 };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Count);
    }

    [Fact(DisplayName = "INPUT_010 - Tạo phiếu nhập với InputPrice là số âm")]
    public void CreateInputProductValidator_NegativeInputPrice_ReturnsValidationError()
    {
        var validator = new CreateInputInfoCommandValidator();
        var command = new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = -100000 };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.InputPrice);
    }

    [Fact(DisplayName = "INPUT_011 - Tạo phiếu nhập với InputPrice vượt quá số chữ số thập phân cho phép")]
    public void CreateInputProductValidator_ExcessiveDecimalPlaces_ReturnsValidationError()
    {
        var validator = new CreateInputInfoCommandValidator();
        var command = new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 10000012.3456m };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.InputPrice);
    }

    [Fact(DisplayName = "INPUT_014 - Tạo phiếu nhập với danh sách Products rỗng")]
    public void CreateInputValidator_EmptyProductsList_ReturnsValidationError()
    {
        var validator = new CreateInputCommandValidator();
        var command = new CreateInputCommand { Notes = "Test", SupplierId = 1, Products = [] };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Products);
    }

    [Fact(DisplayName = "INPUT_015 - Tạo phiếu nhập với SupplierId null")]
    public void CreateInputValidator_NullSupplierId_ReturnsValidationError()
    {
        var validator = new CreateInputCommandValidator();
        var command = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = null,
            Products = [ new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 } ]
        };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SupplierId);
    }

    [Fact(DisplayName = "INPUT_030 - Cập nhật trạng thái phiếu nhập với transition không hợp lệ")]
    public void UpdateInputStatus_InvalidTransition_ThrowsException()
    {
        var currentStatus = Domain.Constants.Input.InputStatus.Finish;
        var newStatus = Domain.Constants.Input.InputStatus.Working;

        bool isAllowed = Domain.Constants.Input.InputStatusTransitions.IsTransitionAllowed(currentStatus, newStatus);

        isAllowed.Should().BeFalse();
    }

    [Fact(DisplayName = "INPUT_048 - Validator kiểm tra CreateInputRequest với Notes quá dài")]
    public void CreateInputValidator_NotesTooLong_ReturnsValidationError()
    {
        var validator = new CreateInputCommandValidator();
        var longNotes = new string('a', 5000);
        var command = new CreateInputCommand
        {
            Notes = longNotes,
            SupplierId = 1,
            Products = [ new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 } ]
        };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }

    [Fact(DisplayName = "INPUT_049 - Validator kiểm tra CreateInputProductCommand với ProductId null")]
    public void CreateInputProductValidator_NullProductId_ReturnsValidationError()
    {
        var validator = new CreateInputInfoCommandValidator();
        var command = new CreateInputInfoRequest { ProductId = null, Count = 10, InputPrice = 100000 };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.ProductId);
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
            Mock.Of<IUnitOfWork>());
        var command = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products = [ new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 } ]
        };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "INPUT_054 - Handler xử lý UpdateInput ném ngoại lệ khi không tìm thấy Input")]
    public async Task UpdateInputHandler_InputNotFound_ThrowsException()
    {
        var mockReadRepo = new Mock<IInputReadRepository>();

        mockReadRepo.Setup(
            x => x.GetByIdWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync((Input?)null);

        var handler = new UpdateInputCommandHandler(
            mockReadRepo.Object,
            Mock.Of<IInputUpdateRepository>(),
            Mock.Of<IInputDeleteRepository>(),
            Mock.Of<ISupplierReadRepository>(),
            Mock.Of<IProductVariantReadRepository>(),
            Mock.Of<IUnitOfWork>());

        var command = new UpdateInputCommand { Id = 9999, Notes = "Updated", SupplierId = 2, Products = [] };

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsFailure.Should().BeTrue();

        result.Error?.Code.Should().Be("NotFound");
    }

    [Fact(DisplayName = "INPUT_055 - Handler xử lý UpdateInputStatus kiểm tra transition hợp lệ")]
    public void InputStatusTransitions_WorkingToFinished_IsAllowed()
    {
        var currentStatus = Domain.Constants.Input.InputStatus.Working;
        var newStatus = Domain.Constants.Input.InputStatus.Finish;

        bool isAllowed = Domain.Constants.Input.InputStatusTransitions.IsTransitionAllowed(currentStatus, newStatus);

        isAllowed.Should().BeTrue();
    }

    [Fact(DisplayName = "INPUT_056 - Handler xử lý DeleteInput kiểm tra trạng thái trước khi xóa")]
    public void InputStatus_FinishedInput_CannotDelete()
    {
        var statusId = Domain.Constants.Input.InputStatus.Finish;

        bool cannotDelete = Domain.Constants.Input.InputStatus.IsCannotDelete(statusId);

        cannotDelete.Should().BeTrue();
    }

    [Fact(DisplayName = "INPUT_059 - Kiểm tra Domain.Constants.InputStatus.IsValid với giá trị hợp lệ")]
    public void InputStatus_ValidStatuses_ReturnsTrue()
    {
        Domain.Constants.Input.InputStatus.IsValid(Domain.Constants.Input.InputStatus.Working).Should().BeTrue();
        Domain.Constants.Input.InputStatus.IsValid(Domain.Constants.Input.InputStatus.Finish).Should().BeTrue();
        Domain.Constants.Input.InputStatus.IsValid(Domain.Constants.Input.InputStatus.Cancel).Should().BeTrue();
    }

    [Fact(DisplayName = "INPUT_060 - Kiểm tra Domain.Constants.InputStatus.IsValid với giá trị không hợp lệ")]
    public void InputStatus_InvalidStatus_ReturnsFalse()
    {
        var invalidStatus = "invalid";

        bool isValid = Domain.Constants.Input.InputStatus.IsValid(invalidStatus);

        isValid.Should().BeFalse();
    }

    [Fact(DisplayName = "INPUT_061 - Kiểm tra Domain.Constants.InputStatus.IsCanEdit với trạng thái working")]
    public void InputStatus_WorkingStatus_CanEdit()
    {
        var statusId = Domain.Constants.Input.InputStatus.Working;

        bool canEdit = Domain.Constants.Input.InputStatus.IsCanEdit(statusId);

        canEdit.Should().BeTrue();
    }

    [Fact(DisplayName = "INPUT_062 - Kiểm tra Domain.Constants.InputStatus.IsCanEdit với trạng thái finished")]
    public void InputStatus_FinishedStatus_CannotEdit()
    {
        var statusId = Domain.Constants.Input.InputStatus.Finish;

        bool canEdit = Domain.Constants.Input.InputStatus.IsCanEdit(statusId);

        canEdit.Should().BeFalse();
    }

    [Fact(DisplayName = "INPUT_063 - Kiểm tra Domain.Constants.InputStatus.IsCannotDelete với trạng thái finished")]
    public void InputStatus_FinishedStatus_CannotDelete()
    {
        var statusId = Domain.Constants.Input.InputStatus.Finish;

        bool cannotDelete = Domain.Constants.Input.InputStatus.IsCannotDelete(statusId);

        cannotDelete.Should().BeTrue();
    }

    [Fact(DisplayName = "INPUT_064 - Validator kiểm tra SupplierId với giá trị âm")]
    public void CreateInputValidator_NegativeSupplierId_ReturnsValidationError()
    {
        var validator = new CreateInputCommandValidator();
        var command = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = -1,
            Products = [ new CreateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100000 } ]
        };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.SupplierId);
    }

    [Fact(DisplayName = "INPUT_065 - Cập nhật phiếu nhập với danh sách Products trùng ProductId")]
    public void UpdateInputValidator_DuplicateProductIds_ReturnsValidationError()
    {
        var validator = new UpdateInputCommandValidator();
        var command = new UpdateInputCommand
        {
            Id = 1,
            Notes = "Test",
            SupplierId = 1,
            Products =
                [ new UpdateInputInfoRequest { ProductId = 1, Count = 10, InputPrice = 100 }, new UpdateInputInfoRequest
                {
                    ProductId = 1,
                    Count = 5,
                    InputPrice = 200
                } ]
        };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Products);
    }
#pragma warning restore CRR0035
#pragma warning restore IDE0079
}
