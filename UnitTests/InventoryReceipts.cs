using Application.ApiContracts.Input.Requests;
using Application.Features.Inputs.Commands.CreateInput;
using Application.Features.Inputs.Commands.UpdateInput;
using Application.Features.Inputs.Commands.UpdateInputStatus;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Input;
using Application.Interfaces.Repositories.ProductVariant;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using Xunit;

namespace UnitTests;

public class InventoryReceipts
{
    [Fact(DisplayName = "INPUT_007 - Tạo phiếu nhập với Quantity là số âm")]
    public void CreateInputProductValidator_NegativeQuantity_ReturnsValidationError()
    {
        // Arrange
        var validator = new CreateInputInfoCommandValidator();
        var command = new CreateInputProductCommand
        {
            ProductId = 1,
            Count = -5,
            InputPrice = 100000
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Count);
    }

    [Fact(DisplayName = "INPUT_008 - Tạo phiếu nhập với Quantity là 0")]
    public void CreateInputProductValidator_ZeroQuantity_ReturnsValidationError()
    {
        // Arrange
        var validator = new CreateInputInfoCommandValidator();
        var command = new CreateInputProductCommand
        {
            ProductId = 1,
            Count = 0,
            InputPrice = 100000
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Count);
    }

    [Fact(DisplayName = "INPUT_010 - Tạo phiếu nhập với InputPrice là số âm")]
    public void CreateInputProductValidator_NegativeInputPrice_ReturnsValidationError()
    {
        // Arrange
        var validator = new CreateInputInfoCommandValidator();
        var command = new CreateInputProductCommand
        {
            ProductId = 1,
            Count = 10,
            InputPrice = -100000
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.InputPrice);
    }

    [Fact(DisplayName = "INPUT_011 - Tạo phiếu nhập với InputPrice vượt quá số chữ số thập phân cho phép")]
    public void CreateInputProductValidator_ExcessiveDecimalPlaces_ReturnsValidationError()
    {
        // Arrange
        var validator = new CreateInputInfoCommandValidator();
        var command = new CreateInputProductCommand
        {
            ProductId = 1,
            Count = 10,
            InputPrice = 100000123456
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.InputPrice);
    }

    [Fact(DisplayName = "INPUT_014 - Tạo phiếu nhập với danh sách Products rỗng")]
    public void CreateInputValidator_EmptyProductsList_ReturnsValidationError()
    {
        // Arrange
        var validator = new CreateInputCommandValidator();
        var command = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = 1,
            Products = []
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Products);
    }

    [Fact(DisplayName = "INPUT_015 - Tạo phiếu nhập với SupplierId null")]
    public void CreateInputValidator_NullSupplierId_ReturnsValidationError()
    {
        // Arrange
        var validator = new CreateInputCommandValidator();
        var command = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = null,
            Products =
            [
                new CreateInputProductCommand { ProductId = 1, Count = 10, InputPrice = 100000 }
            ]
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SupplierId);
    }

    [Fact(DisplayName = "INPUT_030 - Cập nhật trạng thái phiếu nhập với transition không hợp lệ")]
    public void UpdateInputStatus_InvalidTransition_ThrowsException()
    {
        // Arrange
        var currentStatus = Domain.Constants.InputStatus.Finish;
        var newStatus = Domain.Constants.InputStatus.Working;

        // Act
        bool isAllowed = InputStatusTransitions.IsTransitionAllowed(currentStatus, newStatus);

        // Assert
        isAllowed.Should().BeFalse();
    }

    [Fact(DisplayName = "INPUT_048 - Validator kiểm tra CreateInputRequest với Notes quá dài")]
    public void CreateInputValidator_NotesTooLong_ReturnsValidationError()
    {
        // Arrange
        var validator = new CreateInputCommandValidator();
        var longNotes = new string('a', 5000);
        var command = new CreateInputCommand
        {
            Notes = longNotes,
            SupplierId = 1,
            Products =
            [
                new CreateInputProductCommand { ProductId = 1, Count = 10, InputPrice = 100000 }
            ]
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }

    [Fact(DisplayName = "INPUT_049 - Validator kiểm tra CreateInputProductCommand với ProductId null")]
    public void CreateInputProductValidator_NullProductId_ReturnsValidationError()
    {
        // Arrange
        var validator = new CreateInputInfoCommandValidator();
        var command = new CreateInputProductCommand
        {
            ProductId = null,
            Count = 10,
            InputPrice = 100000
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    [Fact(DisplayName = "INPUT_050 - Validator kiểm tra UpdateInputRequest với Products chứa Quantity âm")]
    public void UpdateInputProductValidator_NegativeQuantity_ReturnsValidationError()
    {
        // Arrange
        var validator = new UpdateInputProductCommandValidator();
        var command = new UpdateInputProductCommand
        {
            ProductId = 1,
            Count = -10,
            InputPrice = 100000
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Count);
    }

    [Fact(DisplayName = "INPUT_051 - Validator kiểm tra UpdateInputStatusRequest với StatusId không hợp lệ")]
    public void UpdateInputStatusValidator_InvalidStatusId_ReturnsValidationError()
    {
        // Arrange
        var statusId = "invalid_status";

        // Act
        bool isValid = Domain.Constants.InputStatus.IsValid(statusId);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact(DisplayName = "INPUT_053 - Handler xử lý CreateInput ném ngoại lệ khi DB connection fail")]
    public async Task CreateInputHandler_DbConnectionFails_ThrowsException()
    {
        // Arrange
        var mockInsertRepo = new Mock<IInputInsertRepository>();
        var mockReadRepo = new Mock<IInputReadRepository>();

        mockInsertRepo.Setup(x => x.Add(It.IsAny<Input>()))
            .Throws(new Exception("DB Connection Failed"));

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
            Products =
            [
                new CreateInputProductCommand { ProductId = 1, Count = 10, InputPrice = 100000 }
            ]
        };

        // Act & Assert
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "INPUT_054 - Handler xử lý UpdateInput ném ngoại lệ khi không tìm thấy Input")]
    public async Task UpdateInputHandler_InputNotFound_ThrowsException()
    {
        // Arrange
        var mockReadRepo = new Mock<IInputReadRepository>();
        var mockUpdateRepo = new Mock<IInputUpdateRepository>();

        mockReadRepo.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Input?)null);

        var handler = new UpdateInputCommandHandler(
            mockReadRepo.Object, 
            mockUpdateRepo.Object, 
            Mock.Of<IInputDeleteRepository>(), 
            Mock.Of<ISupplierReadRepository>(), 
            Mock.Of<IProductVariantReadRepository>(), 
            Mock.Of<IUnitOfWork>());
        var command = new UpdateInputCommand
        {
            Id = 9999,
            Notes = "Updated",
            SupplierId = 2,
            Products = []
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        result.Value.Should().BeNull();
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "INPUT_055 - Handler xử lý UpdateInputStatus kiểm tra transition hợp lệ")]
    public void InputStatusTransitions_WorkingToFinished_IsAllowed()
    {
        // Arrange
        var currentStatus = Domain.Constants.InputStatus.Working;
        var newStatus = Domain.Constants.InputStatus.Finish;

        // Act
        bool isAllowed = InputStatusTransitions.IsTransitionAllowed(currentStatus, newStatus);

        // Assert
        isAllowed.Should().BeTrue();
    }

    [Fact(DisplayName = "INPUT_056 - Handler xử lý DeleteInput kiểm tra trạng thái trước khi xóa")]
    public void InputStatus_FinishedInput_CannotDelete()
    {
        // Arrange
        var statusId = Domain.Constants.InputStatus.Finish;

        // Act
        bool cannotDelete = Domain.Constants.InputStatus.IsCannotDelete(statusId);

        // Assert
        cannotDelete.Should().BeTrue();
    }

    [Fact(DisplayName = "INPUT_059 - Kiểm tra Domain.Constants.InputStatus.IsValid với giá trị hợp lệ")]
    public void InputStatus_ValidStatuses_ReturnsTrue()
    {
        // Arrange & Act & Assert
        Domain.Constants.InputStatus.IsValid(Domain.Constants.InputStatus.Working).Should().BeTrue();
        Domain.Constants.InputStatus.IsValid(Domain.Constants.InputStatus.Finish).Should().BeTrue();
        Domain.Constants.InputStatus.IsValid(Domain.Constants.InputStatus.Cancel).Should().BeTrue();
    }

    [Fact(DisplayName = "INPUT_060 - Kiểm tra Domain.Constants.InputStatus.IsValid với giá trị không hợp lệ")]
    public void InputStatus_InvalidStatus_ReturnsFalse()
    {
        // Arrange
        var invalidStatus = "invalid";

        // Act
        bool isValid = Domain.Constants.InputStatus.IsValid(invalidStatus);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact(DisplayName = "INPUT_061 - Kiểm tra Domain.Constants.InputStatus.IsCanEdit với trạng thái working")]
    public void InputStatus_WorkingStatus_CanEdit()
    {
        // Arrange
        var statusId = Domain.Constants.InputStatus.Working;

        // Act
        bool canEdit = Domain.Constants.InputStatus.IsCanEdit(statusId);

        // Assert
        canEdit.Should().BeTrue();
    }

    [Fact(DisplayName = "INPUT_062 - Kiểm tra Domain.Constants.InputStatus.IsCanEdit với trạng thái finished")]
    public void InputStatus_FinishedStatus_CannotEdit()
    {
        // Arrange
        var statusId = Domain.Constants.InputStatus.Finish;

        // Act
        bool canEdit = Domain.Constants.InputStatus.IsCanEdit(statusId);

        // Assert
        canEdit.Should().BeFalse();
    }

    [Fact(DisplayName = "INPUT_063 - Kiểm tra Domain.Constants.InputStatus.IsCannotDelete với trạng thái finished")]
    public void InputStatus_FinishedStatus_CannotDelete()
    {
        // Arrange
        var statusId = Domain.Constants.InputStatus.Finish;

        // Act
        bool cannotDelete = Domain.Constants.InputStatus.IsCannotDelete(statusId);

        // Assert
        cannotDelete.Should().BeTrue();
    }

    [Fact(DisplayName = "INPUT_064 - Validator kiểm tra SupplierId với giá trị âm")]
    public void CreateInputValidator_NegativeSupplierId_ReturnsValidationError()
    {
        // Arrange
        var validator = new CreateInputCommandValidator();
        var command = new CreateInputCommand
        {
            Notes = "Test",
            SupplierId = -1,
            Products =
            [
                new CreateInputProductCommand { ProductId = 1, Count = 10, InputPrice = 100000 }
            ]
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SupplierId);
    }

    [Fact(DisplayName = "INPUT_065 - Cập nhật phiếu nhập với danh sách Products trùng ProductId")]
    public void UpdateInputValidator_DuplicateProductIds_ReturnsValidationError()
    {
        // Arrange
        var validator = new UpdateInputCommandValidator();
        var command = new UpdateInputCommand
        {
            Id = 1,
            Notes = "Test",
            SupplierId = 1,
            Products =
            [
                new UpdateInputProductCommand { ProductId = 1, Count = 10, InputPrice = 100 },
                new UpdateInputProductCommand { ProductId = 1, Count = 5, InputPrice = 200 }
            ]
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Products);
    }
}
