using Application.Features.Suppliers.Commands.CreateSupplier;
using Application.Features.Suppliers.Commands.DeleteSupplier;
using Application.Features.Suppliers.Commands.RestoreSupplier;
using Application.Features.Suppliers.Commands.UpdateSupplier;
using Application.Features.Suppliers.Commands.UpdateSupplierStatus;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using SupplierEntity = Domain.Entities.Supplier;

namespace UnitTests;

public class Supplier
{
    private readonly Mock<ISupplierInsertRepository> _insertRepoMock;
    private readonly Mock<ISupplierUpdateRepository> _updateRepoMock;
    private readonly Mock<ISupplierDeleteRepository> _deleteRepoMock;
    private readonly Mock<ISupplierReadRepository> _readRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    public Supplier()
    {
        _insertRepoMock = new Mock<ISupplierInsertRepository>();
        _updateRepoMock = new Mock<ISupplierUpdateRepository>();
        _deleteRepoMock = new Mock<ISupplierDeleteRepository>();
        _readRepoMock = new Mock<ISupplierReadRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
    }

#pragma warning disable IDE0079 
#pragma warning disable CRR0035
    [Fact(DisplayName = "SUP_001 - Tạo Supplier thành công với đầy đủ thông tin")]
    public async Task CreateSupplier_WithFullInformation_Success()
    {
        var handler = new CreateSupplierCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreateSupplierCommand
        {
            Name = "Supplier A",
            Phone = "0123456789",
            Email = "supplier@test.com",
            Address = "123 Street",
            TaxIdentificationNumber = "0123456789",
            Notes = "Test notes"
        };

        var emptySuppliers = new List<SupplierEntity>().AsQueryable();
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(emptySuppliers);

        _insertRepoMock.Setup(x => x.Add(It.IsAny<SupplierEntity>()));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        _insertRepoMock.Verify(x => x.Add(It.IsAny<SupplierEntity>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SUP_002 - Tạo Supplier thành công với thông tin tối thiểu (chỉ Name, Phone, Address)")]
    public async Task CreateSupplier_WithMinimalInfo_Success()
    {
        var handler = new CreateSupplierCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreateSupplierCommand { Name = "Supplier B", Phone = "0987654321", Address = "456 Street" };

        var emptySuppliers = new List<SupplierEntity>().AsQueryable();
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(emptySuppliers);

        _insertRepoMock.Setup(x => x.Add(It.IsAny<SupplierEntity>()));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        _insertRepoMock.Verify(x => x.Add(It.IsAny<SupplierEntity>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SUP_003 - Tạo Supplier thành công với Email thay vì Phone")]
    public async Task CreateSupplier_WithEmailInsteadOfPhone_Success()
    {
        var handler = new CreateSupplierCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreateSupplierCommand
        {
            Name = "Supplier C",
            Email = "test@example.com",
            Address = "789 Street"
        };

        var emptySuppliers = new List<SupplierEntity>().AsQueryable();
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(emptySuppliers);

        _insertRepoMock.Setup(x => x.Add(It.IsAny<SupplierEntity>()));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        _insertRepoMock.Verify(x => x.Add(It.IsAny<SupplierEntity>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SUP_004 - Tạo Supplier thất bại khi thiếu Name")]
    public void CreateSupplier_MissingName_FailsValidation()
    {
        var validator = new CreateSupplierCommandValidator();
        var command = new CreateSupplierCommand
        {
            Phone = "0123456789",
            Email = "test@test.com",
            Address = "123 Street"
        };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact(DisplayName = "SUP_005 - Tạo Supplier thất bại khi thiếu cả Phone và Email")]
    public void CreateSupplier_MissingPhoneAndEmail_FailsValidation()
    {
        var validator = new CreateSupplierCommandValidator();
        var command = new CreateSupplierCommand { Name = "Supplier D", Address = "123 Street" };

        var result = validator.TestValidate(command);

        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Phone") || e.ErrorMessage.Contains("Email"));
    }

    [Fact(DisplayName = "SUP_006 - Tạo Supplier thất bại khi thiếu Address")]
    public void CreateSupplier_MissingAddress_FailsValidation()
    {
        var validator = new CreateSupplierCommandValidator();
        var command = new CreateSupplierCommand { Name = "Supplier E", Phone = "0123456789" };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Address);
    }

    [Fact(DisplayName = "SUP_007 - Tạo Supplier thất bại khi Name đã tồn tại")]
    public async Task CreateSupplier_DuplicateName_ThrowsException()
    {
        var handler = new CreateSupplierCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateSupplierCommand { Name = "Supplier Existing", };

        _readRepoMock.Setup(x => x.IsNameExistsAsync(command.Name, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsFailure.Should().BeTrue();

        _insertRepoMock.Verify(x => x.Add(It.IsAny<SupplierEntity>()), Times.Never);
    }

    [Fact(DisplayName = "SUP_008 - Tạo Supplier thất bại khi Phone đã tồn tại")]
    public async Task CreateSupplier_DuplicatePhone_ThrowsException()
    {
        var handler = new CreateSupplierCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateSupplierCommand { Name = "Supplier New", Phone = "0123456789", Address = "123 Street" };

        _readRepoMock.Setup(x => x.IsPhoneExistsAsync(command.Phone, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _readRepoMock.Setup(
            x => x.IsNameExistsAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsFailure.Should().BeTrue();
        _insertRepoMock.Verify(x => x.Add(It.IsAny<SupplierEntity>()), Times.Never);
    }

    [Fact(DisplayName = "SUP_009 - Tạo Supplier thất bại khi TaxIdentificationNumber đã tồn tại")]
    public async Task CreateSupplier_DuplicateTaxId_ThrowsException()
    {
        var handler = new CreateSupplierCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _unitOfWorkMock.Object);

        var command = new CreateSupplierCommand
        {
            Name = "Supplier Tax",
            Phone = "0999999999",
            Address = "123 Street",
            TaxIdentificationNumber = "1234567890"
        };

        _readRepoMock.Setup(
            x => x.IsTaxIdExistsAsync(command.TaxIdentificationNumber, It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _readRepoMock.Setup(
            x => x.IsNameExistsAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _readRepoMock.Setup(
            x => x.IsPhoneExistsAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsFailure.Should().BeTrue();
        _insertRepoMock.Verify(x => x.Add(It.IsAny<SupplierEntity>()), Times.Never);
    }

    [Fact(DisplayName = "SUP_VAL_001 - Thêm nhà cung cấp mới với Mã số thuế hợp lệ")]
    public async Task CreateSupplier_ValidTaxId_Success()
    {
        var handler = new CreateSupplierCommandHandler(
            _readRepoMock.Object,
            _insertRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new CreateSupplierCommand
        {
            Name = "Supplier Tax Valid",
            Phone = "0123456789",
            TaxIdentificationNumber = "0123456789"
        };

        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>()))
            .Returns(new List<SupplierEntity>().AsQueryable());
        _insertRepoMock.Setup(x => x.Add(It.IsAny<SupplierEntity>()));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "SUP_VAL_002 - Thêm nhà cung cấp mới với Mã số thuế không hợp lệ (có chữ cái)")]
    public void CreateSupplier_InvalidTaxId_FailsValidation()
    {
        var validator = new CreateSupplierCommandValidator();
        var command = new CreateSupplierCommand
        {
            Name = "Supplier Tax Invalid",
            Phone = "0123456789",
            TaxIdentificationNumber = "TAX123"
        };

        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TaxIdentificationNumber);
    }

    [Fact(DisplayName = "SUP_VAL_003 - Cập nhật nhà cung cấp với Số điện thoại hợp lệ")]
    public async Task UpdateSupplier_ValidPhone_Success()
    {
        var handler = new UpdateSupplierCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var existingSupplier = new SupplierEntity
        {
            Id = 1,
            Name = "Original",
            Phone = "0123456789",
            StatusId = "active"
        };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingSupplier);

        var command = new UpdateSupplierCommand { Id = 1, Phone = "0987654321" };

        _readRepoMock.Setup(x => x.IsPhoneExistsAsync("0987654321", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value!.Phone.Should().Be("0987654321");
    }

    [Fact(DisplayName = "SUP_VAL_004 - Cập nhật nhà cung cấp với Số điện thoại không hợp lệ (có chữ cái)")]
    public void UpdateSupplier_InvalidPhone_FailsValidation()
    {
        var validator = new UpdateSupplierCommandValidator();
        var command = new UpdateSupplierCommand { Id = 1, Phone = "PHONE123" };

        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Fact(DisplayName = "SUP_VAL_005 - Cập nhật nhà cung cấp với Email hợp lệ")]
    public async Task UpdateSupplier_ValidEmail_Success()
    {
        var handler = new UpdateSupplierCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);

        var existingSupplier = new SupplierEntity
        {
            Id = 1,
            Name = "Original",
            Email = "old@test.com",
            StatusId = "active"
        };
        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingSupplier);

        var command = new UpdateSupplierCommand { Id = 1, Email = "update@test.com" };

        _readRepoMock.Setup(x => x.IsEmailExistsAsync("update@test.com", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
        result.Value!.Email.Should().Be("update@test.com");
    }

    [Fact(DisplayName = "SUP_VAL_006 - Cập nhật Supplier thất bại với Email không hợp lệ")]
    public void UpdateSupplier_InvalidEmail_FailsValidation()
    {
        var validator = new UpdateSupplierCommandValidator();
        var command = new UpdateSupplierCommand { Id = 1, Email = "invalid-email" };

        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact(DisplayName = "SUP_VAL_007 - Cập nhật Supplier thất bại khi Email đã tồn tại")]
    public async Task UpdateSupplier_DuplicateEmail_ThrowsException()
    {
        var handler = new UpdateSupplierCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdateSupplierCommand { Id = 1, Email = "existing@test.com" };

        var existingSupplier = new SupplierEntity
        {
            Id = 1,
            Name = "Original",
            Email = "old@test.com",
            StatusId = "active"
        };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingSupplier);

        _readRepoMock.Setup(x => x.IsEmailExistsAsync("existing@test.com", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        result.Error?.Code.Should().Be("Conflict");
    }

    [Fact(DisplayName = "SUP_VAL_008 - Cập nhật Supplier thất bại khi Phone đã tồn tại")]
    public async Task UpdateSupplier_DuplicatePhone_ThrowsException()
    {
        var handler = new UpdateSupplierCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdateSupplierCommand { Id = 1, Phone = "0912345678" };

        var existingSupplier = new SupplierEntity
        {
            Id = 1,
            Name = "Original",
            Phone = "0900000000",
            StatusId = "active"
        };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingSupplier);

        _readRepoMock.Setup(x => x.IsPhoneExistsAsync("0912345678", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        result.Error?.Code.Should().Be("Conflict");
    }

    [Fact(DisplayName = "SUP_VAL_009 - Cập nhật Supplier thất bại khi Mã số thuế đã tồn tại")]
    public async Task UpdateSupplier_DuplicateTaxId_ThrowsException()
    {
        var handler = new UpdateSupplierCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdateSupplierCommand { Id = 1, TaxIdentificationNumber = "0123456789" };

        var existingSupplier = new SupplierEntity
        {
            Id = 1,
            Name = "Original",
            TaxIdentificationNumber = "0000000000",
            StatusId = "active"
        };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingSupplier);

        _readRepoMock.Setup(x => x.IsTaxIdExistsAsync("0123456789", 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        result.Error?.Code.Should().Be("Conflict");
    }

    [Fact(DisplayName = "SUP_VAL_010 - Cập nhật Supplier thất bại với Mã số thuế không hợp lệ (chứa chữ cái)")]
    public void UpdateSupplier_InvalidTaxId_FailsValidation()
    {
        var validator = new UpdateSupplierCommandValidator();
        var command = new UpdateSupplierCommand { Id = 1, TaxIdentificationNumber = "TAX123" };

        var result = validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TaxIdentificationNumber);
    }


    [Fact(DisplayName = "SUP_011 - Tạo Supplier với Email không hợp lệ")]
    public void CreateSupplier_InvalidEmail_FailsValidation()
    {
        var validator = new CreateSupplierCommandValidator();
        var command = new CreateSupplierCommand
        {
            Name = "Supplier Invalid",
            Phone = "0123456789",
            Email = "invalid-email",
            Address = "123 Street"
        };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact(DisplayName = "SUP_012 - Tạo Supplier với Phone không hợp lệ (chứa ký tự đặc biệt)")]
    public void CreateSupplier_InvalidPhone_FailsValidation()
    {
        var validator = new CreateSupplierCommandValidator();
        var command = new CreateSupplierCommand
        {
            Name = "Supplier Phone",
            Phone = "012-345-6789",
            Address = "123 Street"
        };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Fact(DisplayName = "SUP_013 - Tạo Supplier với Name vượt quá độ dài tối đa (>100 ký tự)")]
    public void CreateSupplier_NameTooLong_FailsValidation()
    {
        var validator = new CreateSupplierCommandValidator();
        var command = new CreateSupplierCommand
        {
            Name = new string('A', 101),
            Phone = "0123456789",
            Address = "123 Street"
        };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact(DisplayName = "SUP_015 - Cập nhật Supplier thành công với tất cả các trường")]
    public async Task UpdateSupplier_WithAllFields_Success()
    {
        var handler = new UpdateSupplierCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdateSupplierCommand
        {
            Id = 1,
            Name = "Updated Name",
            Phone = "0999999999",
            Email = "updated@test.com",
            Address = "Updated Address",
            TaxIdentificationNumber = "9876543210",
            Notes = "Updated notes"
        };

        var existingSupplier = new SupplierEntity
        {
            Id = 1,
            Name = "Original Name",
            Phone = "0123456789",
            StatusId = "active"
        };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingSupplier);

        var emptySuppliers = new List<SupplierEntity>().AsQueryable();
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(emptySuppliers);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Updated Name");
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SUP_016 - Cập nhật Supplier thành công với một trường duy nhất (Name)")]
    public async Task UpdateSupplier_WithSingleField_Success()
    {
        var handler = new UpdateSupplierCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdateSupplierCommand { Id = 1, Name = "Only Name Updated" };

        var existingSupplier = new SupplierEntity
        {
            Id = 1,
            Name = "Original Name",
            Phone = "0123456789",
            Email = "original@test.com",
            StatusId = "active"
        };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingSupplier);

        var emptySuppliers = new List<SupplierEntity>().AsQueryable();
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(emptySuppliers);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Value.Should().NotBeNull();
        result.Value!.Name.Should().Be("Only Name Updated");
        result.Value.Phone.Should().Be("0123456789");
        result.Value.Email.Should().Be("original@test.com");
    }

    [Fact(DisplayName = "SUP_017 - Cập nhật Supplier thất bại khi body rỗng")]

    public void UpdateSupplier_WithEmptyBody_ThrowsException()
    {
        var command = new UpdateSupplierCommand { Id = 1 };
        var validator = new UpdateSupplierCommandValidator();

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage.Contains("At least one field must be provided"));
    }

    [Fact(DisplayName = "SUP_018 - Cập nhật Supplier thất bại khi Name trùng với Supplier khác")]
    public async Task UpdateSupplier_DuplicateName_ThrowsException()
    {
        var handler = new UpdateSupplierCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdateSupplierCommand { Id = 1, Name = "Supplier Existing" };

        var existingSupplier = new SupplierEntity { Id = 1, Name = "Original", StatusId = "active" };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingSupplier);

        _readRepoMock.Setup(
            x => x.IsNameExistsAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SUP_019 - Cập nhật Supplier thất bại khi Supplier đã bị xóa")]
    public async Task UpdateSupplier_DeletedSupplier_ThrowsNotFoundException()
    {
        var handler = new UpdateSupplierCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdateSupplierCommand { Id = 1, Name = "Updated Name" };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync((SupplierEntity?)null);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SUP_021 - Cập nhật trạng thái Supplier từ active sang inactive thành công")]
    public async Task UpdateSupplierStatus_ActiveToInactive_Success()
    {
        var handler = new UpdateSupplierStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdateSupplierStatusCommand { Id = 1, StatusId = "inactive" };

        var existingSupplier = new SupplierEntity { Id = 1, Name = "Supplier", StatusId = "active" };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingSupplier);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SUP_022 - Cập nhật trạng thái Supplier từ inactive sang active thành công")]
    public async Task UpdateSupplierStatus_InactiveToActive_Success()
    {
        var handler = new UpdateSupplierStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdateSupplierStatusCommand { Id = 1, StatusId = "active" };

        var existingSupplier = new SupplierEntity { Id = 1, Name = "Supplier", StatusId = "inactive" };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingSupplier);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.Should().NotBeNull();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SUP_023 - Cập nhật trạng thái thất bại với StatusId không hợp lệ")]
    public void UpdateSupplierStatus_InvalidStatus_FailsValidation()
    {
        var validator = new UpdateSupplierStatusCommandValidator();
        var command = new UpdateSupplierStatusCommand { Id = 1, StatusId = "invalid_status" };

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.StatusId);
    }

    [Fact(DisplayName = "SUP_024 - Cập nhật trạng thái thất bại khi Supplier đã bị xóa")]
    public async Task UpdateSupplierStatus_DeletedSupplier_ThrowsNotFoundException()
    {
        var handler = new UpdateSupplierStatusCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new UpdateSupplierStatusCommand { Id = 1, StatusId = "inactive" };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync((SupplierEntity?)null);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SUP_025 - Xóa Supplier thành công khi không có Input Receipt nào")]
    public async Task DeleteSupplier_NoInputReceipts_Success()
    {
        var handler = new DeleteSupplierCommandHandler(
            _readRepoMock.Object,
            _deleteRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new DeleteSupplierCommand { Id = 1 };

        var existingSupplier = new SupplierEntity { Id = 1, Name = "Supplier", StatusId = "active", InputReceipts = [] };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingSupplier);

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SUP_026 - Xóa Supplier thất bại khi còn Input Receipt ở trạng thái Working")]
    public async Task DeleteSupplier_HasWorkingInputReceipt_ThrowsException()
    {
        var handler = new DeleteSupplierCommandHandler(
            _readRepoMock.Object,
            _deleteRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new DeleteSupplierCommand { Id = 1 };

        var existingSupplier = new SupplierEntity
        {
            Id = 1,
            Name = "Supplier",
            StatusId = "active",
            InputReceipts = [ new() { StatusId = "working" } ]
        };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingSupplier);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SUP_027 - Xóa Supplier thành công khi có Input Receipt nhưng không ở trạng thái Working")]
    public async Task DeleteSupplier_HasCompletedInputReceipt_Success()
    {
        var handler = new DeleteSupplierCommandHandler(
            _readRepoMock.Object,
            _deleteRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new DeleteSupplierCommand { Id = 1 };

        var existingSupplier = new SupplierEntity
        {
            Id = 1,
            Name = "Supplier",
            StatusId = "active",
            InputReceipts = [ new() { StatusId = "completed" } ]
        };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingSupplier);

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SUP_028 - Xóa Supplier thất bại khi Supplier đã bị xóa trước đó")]
    public async Task DeleteSupplier_AlreadyDeleted_ThrowsNotFoundException()
    {
        var handler = new DeleteSupplierCommandHandler(
            _readRepoMock.Object,
            _deleteRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new DeleteSupplierCommand { Id = 1 };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync((SupplierEntity?)null);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "SUP_029 - Khôi phục Supplier thành công")]
    public async Task RestoreSupplier_DeletedSupplier_Success()
    {
        var handler = new RestoreSupplierCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new RestoreSupplierCommand { Id = 1 };

        var deletedSupplier = new SupplierEntity
        {
            Id = 1,
            Name = "Supplier",
            StatusId = "active",
            DeletedAt = DateTime.UtcNow
        };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), DataFetchMode.All))
            .ReturnsAsync(deletedSupplier);

        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SUP_030 - Khôi phục Supplier thất bại khi Supplier chưa bị xóa")]
    public async Task RestoreSupplier_NotDeleted_ThrowsException()
    {
        var handler = new RestoreSupplierCommandHandler(
            _readRepoMock.Object,
            _updateRepoMock.Object,
            _unitOfWorkMock.Object);
        var command = new RestoreSupplierCommand { Id = 1 };

        var activeSupplier = new SupplierEntity { Id = 1, Name = "Supplier", StatusId = "active", DeletedAt = null };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), DataFetchMode.All))
            .ReturnsAsync(activeSupplier);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
    }
#pragma warning restore CRR0035
#pragma warning restore IDE0079
}
