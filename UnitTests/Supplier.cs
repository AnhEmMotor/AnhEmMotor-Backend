using Application.Common.Exceptions;
using Application.Features.Suppliers.Commands.CreateSupplier;
using Application.Features.Suppliers.Commands.DeleteManySuppliers;
using Application.Features.Suppliers.Commands.DeleteSupplier;
using Application.Features.Suppliers.Commands.RestoreManySuppliers;
using Application.Features.Suppliers.Commands.RestoreSupplier;
using Application.Features.Suppliers.Commands.UpdateManySupplierStatus;
using Application.Features.Suppliers.Commands.UpdateSupplier;
using Application.Features.Suppliers.Commands.UpdateSupplierStatus;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Supplier;
using Domain.Constants;
using Domain.Entities;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using Xunit;
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

    [Fact(DisplayName = "SUP_001 - Tạo Supplier thành công với đầy đủ thông tin")]
    public async Task CreateSupplier_WithFullInformation_Success()
    {
        // Arrange
        var handler = new CreateSupplierCommandHandler(_insertRepoMock.Object, _readRepoMock.Object, _unitOfWorkMock.Object);
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

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _insertRepoMock.Verify(x => x.Add(It.IsAny<SupplierEntity>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SUP_002 - Tạo Supplier thành công với thông tin tối thiểu (chỉ Name, Phone, Address)")]
    public async Task CreateSupplier_WithMinimalInfo_Success()
    {
        // Arrange
        var handler = new CreateSupplierCommandHandler(_insertRepoMock.Object, _readRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateSupplierCommand
        {
            Name = "Supplier B",
            Phone = "0987654321",
            Address = "456 Street"
        };

        var emptySuppliers = new List<SupplierEntity>().AsQueryable();
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(emptySuppliers);
        
        _insertRepoMock.Setup(x => x.Add(It.IsAny<SupplierEntity>()));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _insertRepoMock.Verify(x => x.Add(It.IsAny<SupplierEntity>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SUP_003 - Tạo Supplier thành công với Email thay vì Phone")]
    public async Task CreateSupplier_WithEmailInsteadOfPhone_Success()
    {
        // Arrange
        var handler = new CreateSupplierCommandHandler(_insertRepoMock.Object, _readRepoMock.Object, _unitOfWorkMock.Object);
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

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _insertRepoMock.Verify(x => x.Add(It.IsAny<SupplierEntity>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SUP_004 - Tạo Supplier thất bại khi thiếu Name")]
    public void CreateSupplier_MissingName_FailsValidation()
    {
        // Arrange
        var validator = new CreateSupplierCommandValidator();
        var command = new CreateSupplierCommand
        {
            Phone = "0123456789",
            Email = "test@test.com",
            Address = "123 Street"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact(DisplayName = "SUP_005 - Tạo Supplier thất bại khi thiếu cả Phone và Email")]
    public void CreateSupplier_MissingPhoneAndEmail_FailsValidation()
    {
        // Arrange
        var validator = new CreateSupplierCommandValidator();
        var command = new CreateSupplierCommand
        {
            Name = "Supplier D",
            Address = "123 Street"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Phone") || e.ErrorMessage.Contains("Email"));
    }

    [Fact(DisplayName = "SUP_006 - Tạo Supplier thất bại khi thiếu Address")]
    public void CreateSupplier_MissingAddress_FailsValidation()
    {
        // Arrange
        var validator = new CreateSupplierCommandValidator();
        var command = new CreateSupplierCommand
        {
            Name = "Supplier E",
            Phone = "0123456789"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Address);
    }

    [Fact(DisplayName = "SUP_007 - Tạo Supplier thất bại khi Name đã tồn tại")]
    public async Task CreateSupplier_DuplicateName_ThrowsException()
    {
        // Arrange
        var handler = new CreateSupplierCommandHandler(_insertRepoMock.Object, _readRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateSupplierCommand
        {
            Name = "Supplier Existing",
            Phone = "0111111111",
            Address = "123 Street"
        };

        var existingSuppliers = new List<SupplierEntity>
        {
            new() { Name = "Supplier Existing", DeletedAt = null }
        }.AsQueryable();
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(existingSuppliers);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact(DisplayName = "SUP_008 - Tạo Supplier thất bại khi Phone đã tồn tại")]
    public async Task CreateSupplier_DuplicatePhone_ThrowsException()
    {
        // Arrange
        var handler = new CreateSupplierCommandHandler(_insertRepoMock.Object, _readRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateSupplierCommand
        {
            Name = "Supplier New",
            Phone = "0123456789",
            Address = "123 Street"
        };

        var existingSuppliers = new List<SupplierEntity>
        {
            new() { Phone = "0123456789", DeletedAt = null }
        }.AsQueryable();
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(existingSuppliers);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact(DisplayName = "SUP_009 - Tạo Supplier thất bại khi TaxIdentificationNumber đã tồn tại")]
    public async Task CreateSupplier_DuplicateTaxId_ThrowsException()
    {
        // Arrange
        var handler = new CreateSupplierCommandHandler(_insertRepoMock.Object, _readRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateSupplierCommand
        {
            Name = "Supplier Tax",
            Phone = "0999999999",
            Address = "123 Street",
            TaxIdentificationNumber = "1234567890"
        };

        var existingSuppliers = new List<SupplierEntity>
        {
            new() { TaxIdentificationNumber = "1234567890", DeletedAt = null }
        }.AsQueryable();
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(existingSuppliers);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact(DisplayName = "SUP_010 - Tạo Supplier với Name có khoảng trắng đầu cuối")]
    public async Task CreateSupplier_NameWithWhitespace_TrimmedSuccessfully()
    {
        // Arrange
        var handler = new CreateSupplierCommandHandler(_insertRepoMock.Object, _readRepoMock.Object, _unitOfWorkMock.Object);
        var command = new CreateSupplierCommand
        {
            Name = "  Supplier Trim  ",
            Phone = "0123456789",
            Address = "123 Street"
        };

        var emptySuppliers = new List<SupplierEntity>().AsQueryable();
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(emptySuppliers);
        
        _insertRepoMock.Setup(x => x.Add(It.IsAny<SupplierEntity>()));
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _insertRepoMock.Verify(x => x.Add(It.Is<SupplierEntity>(s => s.Name != null && !s.Name.StartsWith(' ') && !s.Name.EndsWith(' '))), Times.Once);
    }

    [Fact(DisplayName = "SUP_011 - Tạo Supplier với Email không hợp lệ")]
    public void CreateSupplier_InvalidEmail_FailsValidation()
    {
        // Arrange
        var validator = new CreateSupplierCommandValidator();
        var command = new CreateSupplierCommand
        {
            Name = "Supplier Invalid",
            Phone = "0123456789",
            Email = "invalid-email",
            Address = "123 Street"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact(DisplayName = "SUP_012 - Tạo Supplier với Phone không hợp lệ (chứa ký tự đặc biệt)")]
    public void CreateSupplier_InvalidPhone_FailsValidation()
    {
        // Arrange
        var validator = new CreateSupplierCommandValidator();
        var command = new CreateSupplierCommand
        {
            Name = "Supplier Phone",
            Phone = "012-345-6789",
            Address = "123 Street"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Phone);
    }

    [Fact(DisplayName = "SUP_013 - Tạo Supplier với Name vượt quá độ dài tối đa (>100 ký tự)")]
    public void CreateSupplier_NameTooLong_FailsValidation()
    {
        // Arrange
        var validator = new CreateSupplierCommandValidator();
        var command = new CreateSupplierCommand
        {
            Name = new string('A', 101),
            Phone = "0123456789",
            Address = "123 Street"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact(DisplayName = "SUP_015 - Cập nhật Supplier thành công với tất cả các trường")]
    public async Task UpdateSupplier_WithAllFields_Success()
    {
        // Arrange
        var handler = new UpdateSupplierCommandHandler(_readRepoMock.Object, _updateRepoMock.Object, _unitOfWorkMock.Object);
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

        // Act
        var (Data, Error) = await handler.Handle(command, CancellationToken.None);

        // Assert
        Data.Should().NotBeNull();
        Data!.Name.Should().Be("Updated Name");
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SUP_016 - Cập nhật Supplier thành công với một trường duy nhất (Name)")]
    public async Task UpdateSupplier_WithSingleField_Success()
    {
        // Arrange
        var handler = new UpdateSupplierCommandHandler(_readRepoMock.Object, _updateRepoMock.Object, _unitOfWorkMock.Object);
        var command = new UpdateSupplierCommand
        {
            Id = 1,
            Name = "Only Name Updated"
        };

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

        // Act
        var (Data, Error) = await handler.Handle(command, CancellationToken.None);

        // Assert
        Data.Should().NotBeNull();
        Data!.Name.Should().Be("Only Name Updated");
        Data.Phone.Should().Be("0123456789");
        Data.Email.Should().Be("original@test.com");
    }

    [Fact(DisplayName = "SUP_017 - Cập nhật Supplier thất bại khi body rỗng")]
    public async Task UpdateSupplier_WithEmptyBody_ThrowsException()
    {
        // Arrange
        var handler = new UpdateSupplierCommandHandler(_readRepoMock.Object, _updateRepoMock.Object, _unitOfWorkMock.Object);
        var command = new UpdateSupplierCommand { Id = 1 };

        var existingSupplier = new SupplierEntity
        {
            Id = 1,
            Name = "Original Name",
            StatusId = "active"
        };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingSupplier);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact(DisplayName = "SUP_018 - Cập nhật Supplier thất bại khi Name trùng với Supplier khác")]
    public async Task UpdateSupplier_DuplicateName_ThrowsException()
    {
        // Arrange
        var handler = new UpdateSupplierCommandHandler(_readRepoMock.Object, _updateRepoMock.Object, _unitOfWorkMock.Object);
        var command = new UpdateSupplierCommand
        {
            Id = 1,
            Name = "Supplier Existing"
        };

        var existingSupplier = new SupplierEntity
        {
            Id = 1,
            Name = "Original",
            StatusId = "active"
        };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingSupplier);

        var existingSuppliers = new List<SupplierEntity>
        {
            new() { Id = 2, Name = "Supplier Existing" }
        }.AsQueryable();
        _readRepoMock.Setup(x => x.GetQueryable(It.IsAny<DataFetchMode>())).Returns(existingSuppliers);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact(DisplayName = "SUP_019 - Cập nhật Supplier thất bại khi Supplier đã bị xóa")]
    public async Task UpdateSupplier_DeletedSupplier_ThrowsNotFoundException()
    {
        // Arrange
        var handler = new UpdateSupplierCommandHandler(_readRepoMock.Object, _updateRepoMock.Object, _unitOfWorkMock.Object);
        var command = new UpdateSupplierCommand
        {
            Id = 1,
            Name = "Updated Name"
        };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync((SupplierEntity?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact(DisplayName = "SUP_021 - Cập nhật trạng thái Supplier từ active sang inactive thành công")]
    public async Task UpdateSupplierStatus_ActiveToInactive_Success()
    {
        // Arrange
        var handler = new UpdateSupplierStatusCommandHandler(_readRepoMock.Object, _updateRepoMock.Object, _unitOfWorkMock.Object);
        var command = new UpdateSupplierStatusCommand
        {
            Id = 1,
            StatusId = "inactive"
        };

        var existingSupplier = new SupplierEntity
        {
            Id = 1,
            Name = "Supplier",
            StatusId = "active"
        };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingSupplier);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SUP_022 - Cập nhật trạng thái Supplier từ inactive sang active thành công")]
    public async Task UpdateSupplierStatus_InactiveToActive_Success()
    {
        // Arrange
        var handler = new UpdateSupplierStatusCommandHandler(_readRepoMock.Object, _updateRepoMock.Object, _unitOfWorkMock.Object);
        var command = new UpdateSupplierStatusCommand
        {
            Id = 1,
            StatusId = "active"
        };

        var existingSupplier = new SupplierEntity
        {
            Id = 1,
            Name = "Supplier",
            StatusId = "inactive"
        };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingSupplier);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SUP_023 - Cập nhật trạng thái thất bại với StatusId không hợp lệ")]
    public void UpdateSupplierStatus_InvalidStatus_FailsValidation()
    {
        // Arrange
        var validator = new UpdateSupplierStatusCommandValidator();
        var command = new UpdateSupplierStatusCommand
        {
            Id = 1,
            StatusId = "invalid_status"
        };

        // Act
        var result = validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StatusId);
    }

    [Fact(DisplayName = "SUP_024 - Cập nhật trạng thái thất bại khi Supplier đã bị xóa")]
    public async Task UpdateSupplierStatus_DeletedSupplier_ThrowsNotFoundException()
    {
        // Arrange
        var handler = new UpdateSupplierStatusCommandHandler(_readRepoMock.Object, _updateRepoMock.Object, _unitOfWorkMock.Object);
        var command = new UpdateSupplierStatusCommand
        {
            Id = 1,
            StatusId = "inactive"
        };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync((SupplierEntity?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact(DisplayName = "SUP_025 - Xóa Supplier thành công khi không có Input Receipt nào")]
    public async Task DeleteSupplier_NoInputReceipts_Success()
    {
        // Arrange
        var handler = new DeleteSupplierCommandHandler(_readRepoMock.Object, _deleteRepoMock.Object, _unitOfWorkMock.Object);
        var command = new DeleteSupplierCommand { Id = 1 };

        var existingSupplier = new SupplierEntity
        {
            Id = 1,
            Name = "Supplier",
            StatusId = "active",
            InputReceipts = []
        };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingSupplier);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SUP_026 - Xóa Supplier thất bại khi còn Input Receipt ở trạng thái Working")]
    public async Task DeleteSupplier_HasWorkingInputReceipt_ThrowsException()
    {
        // Arrange
        var handler = new DeleteSupplierCommandHandler(_readRepoMock.Object, _deleteRepoMock.Object, _unitOfWorkMock.Object);
        var command = new DeleteSupplierCommand { Id = 1 };

        var existingSupplier = new SupplierEntity
        {
            Id = 1,
            Name = "Supplier",
            StatusId = "active",
            InputReceipts =
            [
                new() { StatusId = "working" }
            ]
        };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingSupplier);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact(DisplayName = "SUP_027 - Xóa Supplier thành công khi có Input Receipt nhưng không ở trạng thái Working")]
    public async Task DeleteSupplier_HasCompletedInputReceipt_Success()
    {
        // Arrange
        var handler = new DeleteSupplierCommandHandler(_readRepoMock.Object, _deleteRepoMock.Object, _unitOfWorkMock.Object);
        var command = new DeleteSupplierCommand { Id = 1 };

        var existingSupplier = new SupplierEntity
        {
            Id = 1,
            Name = "Supplier",
            StatusId = "active",
            InputReceipts =
            [
                new() { StatusId = "completed" }
            ]
        };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync(existingSupplier);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SUP_028 - Xóa Supplier thất bại khi Supplier đã bị xóa trước đó")]
    public async Task DeleteSupplier_AlreadyDeleted_ThrowsNotFoundException()
    {
        // Arrange
        var handler = new DeleteSupplierCommandHandler(_readRepoMock.Object, _deleteRepoMock.Object, _unitOfWorkMock.Object);
        var command = new DeleteSupplierCommand { Id = 1 };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), It.IsAny<DataFetchMode>()))
            .ReturnsAsync((SupplierEntity?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact(DisplayName = "SUP_029 - Khôi phục Supplier thành công")]
    public async Task RestoreSupplier_DeletedSupplier_Success()
    {
        // Arrange
        var handler = new RestoreSupplierCommandHandler(_readRepoMock.Object, _updateRepoMock.Object, _unitOfWorkMock.Object);
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

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "SUP_030 - Khôi phục Supplier thất bại khi Supplier chưa bị xóa")]
    public async Task RestoreSupplier_NotDeleted_ThrowsException()
    {
        // Arrange
        var handler = new RestoreSupplierCommandHandler(_readRepoMock.Object, _updateRepoMock.Object, _unitOfWorkMock.Object);
        var command = new RestoreSupplierCommand { Id = 1 };

        var activeSupplier = new SupplierEntity
        {
            Id = 1,
            Name = "Supplier",
            StatusId = "active",
            DeletedAt = null
        };

        _readRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>(), DataFetchMode.All))
            .ReturnsAsync(activeSupplier);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));
    }
}
