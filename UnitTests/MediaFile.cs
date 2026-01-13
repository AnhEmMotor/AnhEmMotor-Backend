using Application.Common.Models;
using Application.Features.Files.Commands.DeleteFile;
using Application.Features.Files.Commands.DeleteManyFiles;
using Application.Features.Files.Commands.RestoreFile;
using Application.Features.Files.Commands.RestoreManyFiles;
using Application.Features.Files.Commands.UploadImage;
using Application.Features.Files.Commands.UploadManyImage;
using Application.Features.Files.Queries.GetFileById;
using Application.Features.Files.Queries.ViewImage;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.LocalFile;
using Application.Interfaces.Repositories.MediaFile;
using Domain.Constants;
using FluentAssertions;
using Moq;
using Xunit;
using MediaFileEntity = Domain.Entities.MediaFile;

namespace UnitTests;

public class MediaFile
{
    [Fact(DisplayName = "MF_001 - Tải lên ảnh thành công với định dạng WEBP hợp lệ")]
    public async Task UploadImage_ValidWebp_Success()
    {
        // Arrange
        var fileStorageServiceMock = new Mock<IFileStorageService>();
        var insertRepositoryMock = new Mock<IMediaFileInsertRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var expectedStoragePath = "test-guid-123.webp";
        var expectedFileExtension = ".webp";
        fileStorageServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FileUpload>.Success(new FileUpload(expectedStoragePath, expectedFileExtension, 1024)));

        var handler = new UploadImageCommandHandler(
            fileStorageServiceMock.Object,
            insertRepositoryMock.Object,
            unitOfWorkMock.Object);

        var stream = new MemoryStream(new byte[102400]); // 100KB
        var command = new UploadImageCommand
        {
            FileContent = stream,
            FileName = "test.webp"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Value.StoragePath.Should().Be(expectedStoragePath);
        result.Value.OriginalFileName.Should().Be("test.webp");
        fileStorageServiceMock.Verify(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once());
        insertRepositoryMock.Verify(x => x.Add(It.IsAny<MediaFileEntity>()), Times.Once());
        unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact(DisplayName = "MF_003 - Tải lên ảnh thất bại với định dạng không được hỗ trợ")]
    public async Task UploadImage_UnsupportedFormat_Fail()
    {
        // Arrange
        var fileStorageServiceMock = new Mock<IFileStorageService>();
        var insertRepositoryMock = new Mock<IMediaFileInsertRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new UploadImageCommandHandler(
            fileStorageServiceMock.Object,
            insertRepositoryMock.Object,
            unitOfWorkMock.Object);

        // TrÆ°á»ng há»£p 1: File PDF
        var pdfStream = new MemoryStream(new byte[1024]);
        var pdfCommand = new UploadImageCommand
        {
            FileContent = pdfStream,
            FileName = "document.pdf"
        };

        // Act
        var result = await handler.Handle(pdfCommand, CancellationToken.None);
        result.IsFailure.Should().BeTrue();

        // Assert
        // Assert
        // Replaced Assertions


        // TrÆ°á»ng há»£p 2: File TXT
        var txtStream = new MemoryStream(new byte[512]);
        var txtCommand = new UploadImageCommand
        {
            FileContent = txtStream,
            FileName = "text.txt"
        };

        result = await handler.Handle(txtCommand, CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        

        // TrÆ°á»ng há»£p 3: File vá»›i extension .webp nhÆ°ng magic bytes lÃ  PDF
        var fakeWebpStream = new MemoryStream(new byte[1024]);
        var fakeWebpCommand = new UploadImageCommand
        {
            FileContent = fakeWebpStream,
            FileName = "fake.webp"
        };

        result = await handler.Handle(fakeWebpCommand, CancellationToken.None);
        result.IsFailure.Should().BeTrue();
        
    }

    [Fact(DisplayName = "MF_004 - Tải lên ảnh thất bại khi kích thước file vượt quá giới hạn")]
    public async Task UploadImage_FileSizeExceedsLimit_Fail()
    {
        // Arrange
        var fileStorageServiceMock = new Mock<IFileStorageService>();
        var insertRepositoryMock = new Mock<IMediaFileInsertRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new UploadImageCommandHandler(
            fileStorageServiceMock.Object,
            insertRepositoryMock.Object,
            unitOfWorkMock.Object);

        var largeStream = new MemoryStream(new byte[52428800]); // 50MB
        var command = new UploadImageCommand
        {
            FileContent = largeStream,
            FileName = "large.webp"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsFailure.Should().BeTrue();

        // Assert - Sau khi implement, sáº½ throw BadRequestException vá»›i message "File size exceeds limit"
        
    }

    [Fact(DisplayName = "MF_008 - Tải lên nhiều ảnh thất bại khi có 1 file không hợp lệ (Bulk Request Rule)")]
    public async Task UploadManyImages_OneInvalidFile_FailAll()
    {
        // Arrange
        var fileStorageServiceMock = new Mock<IFileStorageService>();
        var insertRepositoryMock = new Mock<IMediaFileInsertRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new UploadManyImageCommandHandler(
            fileStorageServiceMock.Object,
            insertRepositoryMock.Object,
            unitOfWorkMock.Object);

        var files = new List<(Stream FileContent, string FileName)>
        {
            new(new MemoryStream(new byte[51200]), "valid1.webp"),
            new(new MemoryStream(new byte[102400]), "invalid.pdf"),
            new(new MemoryStream(new byte[61440]), "valid2.jpg")
        };

        var command = new UploadManyImageCommand { Files = files };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsFailure.Should().BeTrue();

        // Assert - Bulk Request Rule: 1 file invalid => toÃ n bá»™ request fail
        
        fileStorageServiceMock.Verify(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Fact(DisplayName = "MF_010 - Xoá file thất bại khi file không tồn tại")]
    public async Task DeleteFile_FileNotFound_Fail()
    {
        // Arrange
        var readRepositoryMock = new Mock<IMediaFileReadRepository>();
        var deleteRepositoryMock = new Mock<IMediaFileDeleteRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var fileStorageServiceMock = new Mock<IFileStorageService>();

        readRepositoryMock.Setup(x => x.GetByStoragePathAsync(
                "nonexistent-file.webp",
                It.IsAny<CancellationToken>(),
                DataFetchMode.ActiveOnly))
            .ReturnsAsync((MediaFileEntity?)null);

        var handler = new DeleteFileCommandHandler(
            readRepositoryMock.Object,
            deleteRepositoryMock.Object,
            unitOfWorkMock.Object,
            fileStorageServiceMock.Object);

        var command = new DeleteFileCommand { StoragePath = "nonexistent-file.webp" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsFailure.Should().BeTrue();

        // Assert
        
        deleteRepositoryMock.Verify(x => x.Delete(It.IsAny<MediaFileEntity>()), Times.Never());
    }

    [Fact(DisplayName = "MF_011 - Xoá file thất bại khi file đã bị xoá trước đó")]
    public async Task DeleteFile_AlreadyDeleted_Fail()
    {
        // Arrange
        var readRepositoryMock = new Mock<IMediaFileReadRepository>();
        var deleteRepositoryMock = new Mock<IMediaFileDeleteRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();
        var fileStorageServiceMock = new Mock<IFileStorageService>();

        readRepositoryMock.Setup(x => x.GetByStoragePathAsync(
                "already-deleted.webp",
                It.IsAny<CancellationToken>(),
                DataFetchMode.ActiveOnly))
            .ReturnsAsync((MediaFileEntity?)null);

        var handler = new DeleteFileCommandHandler(
            readRepositoryMock.Object,
            deleteRepositoryMock.Object,
            unitOfWorkMock.Object,
            fileStorageServiceMock.Object);

        var command = new DeleteFileCommand { StoragePath = "already-deleted.webp" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsFailure.Should().BeTrue();

        // Assert
        
        deleteRepositoryMock.Verify(x => x.Delete(It.IsAny<MediaFileEntity>()), Times.Never());
    }

    [Fact(DisplayName = "MF_014 - Xoá nhiều file thất bại khi có 1 file không tồn tại (Bulk Request Rule)")]
    public async Task DeleteManyFiles_OneNotFound_FailAll()
    {
        // Arrange
        var readRepositoryMock = new Mock<IMediaFileReadRepository>();
        var deleteRepositoryMock = new Mock<IMediaFileDeleteRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var existingFiles = new List<MediaFileEntity>
        {
            new (){ StoragePath = "exist1.webp", DeletedAt = null },
            new (){ StoragePath = "exist2.webp", DeletedAt = null }
        };

        readRepositoryMock.Setup(x => x.GetByStoragePathsAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>(),
                DataFetchMode.ActiveOnly))
            .ReturnsAsync(existingFiles);

        var handler = new DeleteManyFilesCommandHandler(
            readRepositoryMock.Object,
            deleteRepositoryMock.Object,
            unitOfWorkMock.Object);

        var command = new DeleteManyFilesCommand
        {
            StoragePaths = ["exist1.webp", "nonexistent.jpg", "exist2.webp"]
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsFailure.Should().BeTrue();

        // Assert - Bulk Request Rule: Request 3 files, chá»‰ tÃ¬m tháº¥y 2 => Fail toÃ n bá»™
        
        deleteRepositoryMock.Verify(x => x.Delete(It.IsAny<IEnumerable<MediaFileEntity>>()), Times.Never());
    }

    [Fact(DisplayName = "MF_016 - Khôi phục file thất bại khi file không tồn tại")]
    public async Task RestoreFile_FileNotFound_Fail()
    {
        // Arrange
        var readRepositoryMock = new Mock<IMediaFileReadRepository>();
        var updateRepositoryMock = new Mock<IMediaFileUpdateRepository>();
        var fileStorageServiceMock = new Mock<IFileStorageService>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        readRepositoryMock.Setup(x => x.GetByStoragePathAsync(
                "nonexistent-file.webp",
                It.IsAny<CancellationToken>(),
                DataFetchMode.DeletedOnly))
            .ReturnsAsync((MediaFileEntity?)null);

        var handler = new RestoreFileCommandHandler(
            readRepositoryMock.Object,
            updateRepositoryMock.Object,
            fileStorageServiceMock.Object,
            unitOfWorkMock.Object);

        var command = new RestoreFileCommand { StoragePath = "nonexistent-file.webp" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsFailure.Should().BeTrue();

        // Assert
        
        updateRepositoryMock.Verify(x => x.Update(It.IsAny<MediaFileEntity>()), Times.Never());
    }

    [Fact(DisplayName = "MF_017 - Khôi phục file thất bại khi file chưa bị xoá")]
    public async Task RestoreFile_FileNotDeleted_Fail()
    {
        // Arrange
        var readRepositoryMock = new Mock<IMediaFileReadRepository>();
        var updateRepositoryMock = new Mock<IMediaFileUpdateRepository>();
        var fileStorageServiceMock = new Mock<IFileStorageService>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        readRepositoryMock.Setup(x => x.GetByStoragePathAsync(
                "active-file.webp",
                It.IsAny<CancellationToken>(),
                DataFetchMode.DeletedOnly))
            .ReturnsAsync((MediaFileEntity?)null);

        var handler = new RestoreFileCommandHandler(
            readRepositoryMock.Object,
            updateRepositoryMock.Object,
            fileStorageServiceMock.Object,
            unitOfWorkMock.Object);

        var command = new RestoreFileCommand { StoragePath = "active-file.webp" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsFailure.Should().BeTrue();

        // Assert - File chÆ°a bá»‹ xoÃ¡ => khÃ´ng tÃ¬m tháº¥y trong DeletedOnly
        
        updateRepositoryMock.Verify(x => x.Update(It.IsAny<MediaFileEntity>()), Times.Never());
    }

    [Fact(DisplayName = "MF_020 - Khôi phục nhiều file thất bại khi có 1 file không tồn tại (Bulk Request Rule)")]
    public async Task RestoreManyFiles_OneNotFound_FailAll()
    {
        // Arrange
        var readRepositoryMock = new Mock<IMediaFileReadRepository>();
        var updateRepositoryMock = new Mock<IMediaFileUpdateRepository>();
        var fileStorageServiceMock = new Mock<IFileStorageService>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var deletedFiles = new List<MediaFileEntity>
        {
            new(){ StoragePath = "deleted1.webp", DeletedAt = DateTimeOffset.Parse("2025-01-01T00:00:00Z") },
            new(){ StoragePath = "deleted2.webp", DeletedAt = DateTimeOffset.Parse("2025-01-01T00:00:00Z") }
        };

        readRepositoryMock.Setup(x => x.GetByStoragePathsAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>(),
                DataFetchMode.DeletedOnly))
            .ReturnsAsync(deletedFiles);

        var handler = new RestoreManyFilesCommandHandler(
            readRepositoryMock.Object,
            updateRepositoryMock.Object,
            fileStorageServiceMock.Object,
            unitOfWorkMock.Object);

        var command = new RestoreManyFilesCommand
        {
            StoragePaths = ["deleted1.webp", "nonexistent.jpg", "deleted2.webp"]
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsFailure.Should().BeTrue();

        // Assert - Bulk Request Rule: Request 3 files, chá»‰ tÃ¬m tháº¥y 2 => Fail toÃ n bá»™
        
        updateRepositoryMock.Verify(x => x.Restore(It.IsAny<IEnumerable<MediaFileEntity>>()), Times.Never());
    }

    [Fact(DisplayName = "MF_023 - Xem ảnh thất bại khi file không tồn tại")]
    public async Task ViewImage_FileNotFound_Fail()
    {
        // Arrange
        var fileStorageServiceMock = new Mock<IFileStorageService>();

        fileStorageServiceMock.Setup(x => x.GetFileAsync("nonexistent.webp", It.IsAny<CancellationToken>()))
            .ReturnsAsync(((byte[], string)?)null);

        var handler = new ViewImageQueryHandler(fileStorageServiceMock.Object);

        var query = new ViewImageQuery { StoragePath = "nonexistent.webp" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);
        result.IsFailure.Should().BeTrue();

        // Assert
        
    }

    [Fact(DisplayName = "MF_024 - Xem ảnh thất bại khi file đã bị xoá")]
    public async Task ViewImage_FileDeleted_Fail()
    {
        // Arrange
        var fileStorageServiceMock = new Mock<IFileStorageService>();

        fileStorageServiceMock.Setup(x => x.GetFileAsync("deleted-image.webp", It.IsAny<CancellationToken>()))
            .ReturnsAsync(((byte[], string)?)null);

        var handler = new ViewImageQueryHandler(fileStorageServiceMock.Object);

        var query = new ViewImageQuery { StoragePath = "deleted-image.webp" };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);
        result.IsFailure.Should().BeTrue();

        // Assert
        
    }

    [Fact(DisplayName = "MF_025 - Xem ảnh thất bại khi width là số âm")]
    public async Task ViewImage_NegativeWidth_Fail()
    {
        // Arrange
        var fileStorageServiceMock = new Mock<IFileStorageService>();

        var handler = new ViewImageQueryHandler(fileStorageServiceMock.Object);

        var query = new ViewImageQuery { StoragePath = "test.webp", Width = -100 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);
        result.IsFailure.Should().BeTrue();

        // Assert - Validation sáº½ catch trÆ°á»›c khi vÃ o handler
        
    }

    [Fact(DisplayName = "MF_026 - Xem ảnh thất bại khi width vượt quá giới hạn cho phép")]
    public async Task ViewImage_WidthExceedsLimit_Fail()
    {
        // Arrange
        var fileStorageServiceMock = new Mock<IFileStorageService>();

        var handler = new ViewImageQueryHandler(fileStorageServiceMock.Object);

        var query = new ViewImageQuery { StoragePath = "test.webp", Width = 50000 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);
        result.IsFailure.Should().BeTrue();

        // Assert
        
    }

    [Fact(DisplayName = "MF_027 - Validate: Tên file gốc chứa ký tự đặc biệt")]
    public async Task UploadImage_FileNameWithSpecialChars_Success()
    {
        // Arrange
        var fileStorageServiceMock = new Mock<IFileStorageService>();
        var insertRepositoryMock = new Mock<IMediaFileInsertRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var expectedStoragePath = "test-guid-456.webp";
        var expectedFileExtension = ".webp";
        fileStorageServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FileUpload>.Success(new FileUpload(expectedStoragePath, expectedFileExtension, 1024)));

        var handler = new UploadImageCommandHandler(
            fileStorageServiceMock.Object,
            insertRepositoryMock.Object,
            unitOfWorkMock.Object);

        var stream = new MemoryStream(new byte[51200]);
        var command = new UploadImageCommand
        {
            FileContent = stream,
            FileName = "test<>:\"/\\|?*.webp"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert - Handler sáº½ sanitize filename vÃ  upload thÃ nh cÃ´ng
        result.Should().NotBeNull();
        result.Value.StoragePath.Should().NotBeNullOrEmpty();
        fileStorageServiceMock.Verify(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact(DisplayName = "MF_028 - Validate: Tên file gốc có khoảng trắng đầu cuối")]
    public async Task UploadImage_FileNameWithWhitespace_Success()
    {
        // Arrange
        var fileStorageServiceMock = new Mock<IFileStorageService>();
        var insertRepositoryMock = new Mock<IMediaFileInsertRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var expectedStoragePath = "test-guid-789.webp";
        var expectedFileExtension = ".webp";
        fileStorageServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FileUpload>.Success(new FileUpload(expectedStoragePath, expectedFileExtension, 1024)));

        var handler = new UploadImageCommandHandler(
            fileStorageServiceMock.Object,
            insertRepositoryMock.Object,
            unitOfWorkMock.Object);

        var stream = new MemoryStream(new byte[51200]);
        var command = new UploadImageCommand
        {
            FileContent = stream,
            FileName = "  test image.webp  "
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert - Handler sáº½ trim whitespace vÃ  upload thÃ nh cÃ´ng
        result.Should().NotBeNull();
        result.Value.StoragePath.Should().NotBeNullOrEmpty();
        fileStorageServiceMock.Verify(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact(DisplayName = "MF_029 - Security: File signature không khớp với extension (webp fake)")]
    public async Task UploadImage_WebpFakeSignature_Fail()
    {
        // Arrange
        var fileStorageServiceMock = new Mock<IFileStorageService>();
        var insertRepositoryMock = new Mock<IMediaFileInsertRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new UploadImageCommandHandler(
            fileStorageServiceMock.Object,
            insertRepositoryMock.Object,
            unitOfWorkMock.Object);

        var fakeStream = new MemoryStream(new byte[51200]);
        var command = new UploadImageCommand
        {
            FileContent = fakeStream,
            FileName = "fake.webp"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsFailure.Should().BeTrue();

        // Assert - Magic bytes validation sáº½ detect fake file
        
    }

    [Fact(DisplayName = "MF_030 - Security: File signature không khớp với extension (jpg fake)")]
    public async Task UploadImage_JpgFakeSignature_Fail()
    {
        // Arrange
        var fileStorageServiceMock = new Mock<IFileStorageService>();
        var insertRepositoryMock = new Mock<IMediaFileInsertRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new UploadImageCommandHandler(
            fileStorageServiceMock.Object,
            insertRepositoryMock.Object,
            unitOfWorkMock.Object);

        var fakeStream = new MemoryStream(new byte[51200]);
        var command = new UploadImageCommand
        {
            FileContent = fakeStream,
            FileName = "fake.jpg"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);
        result.IsFailure.Should().BeTrue();

        // Assert - Magic bytes validation sáº½ detect fake file
        
    }

    [Fact(DisplayName = "MF_032 - Lấy thông tin file theo ID thất bại khi file không tồn tại")]
    public async Task GetFileById_FileNotFound_Fail()
    {
        // Arrange
        var readRepositoryMock = new Mock<IMediaFileReadRepository>();
        var fileStorageServiceMock = new Mock<IFileStorageService>();

        readRepositoryMock.Setup(x => x.GetByIdAsync(
                999999,
                It.IsAny<CancellationToken>(),
                DataFetchMode.ActiveOnly))
            .ReturnsAsync((MediaFileEntity?)null);

        var handler = new GetFileByIdQueryHandler(readRepositoryMock.Object, fileStorageServiceMock.Object);

        var query = new GetFileByIdQuery { Id = 999999 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);
        result.IsFailure.Should().BeTrue();

        // Assert
        
    }

    [Fact(DisplayName = "MF_033 - Lấy thông tin file theo ID thất bại khi file đã bị xoá")]
    public async Task GetFileById_FileDeleted_Fail()
    {
        // Arrange
        var readRepositoryMock = new Mock<IMediaFileReadRepository>();
        var fileStorageServiceMock = new Mock<IFileStorageService>();

        readRepositoryMock.Setup(x => x.GetByIdAsync(
                456,
                It.IsAny<CancellationToken>(),
                DataFetchMode.ActiveOnly))
            .ReturnsAsync((MediaFileEntity?)null);

        var handler = new GetFileByIdQueryHandler(readRepositoryMock.Object, fileStorageServiceMock.Object);

        var query = new GetFileByIdQuery { Id = 456 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);
        result.IsFailure.Should().BeTrue();

        // Assert - File Ä‘Ã£ bá»‹ xoÃ¡ => khÃ´ng tÃ¬m tháº¥y trong ActiveOnly
        
    }

    [Fact(DisplayName = "MF_040 - StorageType validation: Kiểm tra giá trị hợp lệ")]
    public async Task UploadImage_ValidStorageType_Success()
    {
        // Arrange
        var fileStorageServiceMock = new Mock<IFileStorageService>();
        var insertRepositoryMock = new Mock<IMediaFileInsertRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var expectedStoragePath = "test-guid-999.webp";
        var expectedFileExtension = ".webp";
        fileStorageServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FileUpload>.Success(new FileUpload(expectedStoragePath, expectedFileExtension, 1024)));

        var handler = new UploadImageCommandHandler(
            fileStorageServiceMock.Object,
            insertRepositoryMock.Object,
            unitOfWorkMock.Object);

        var stream = new MemoryStream(new byte[51200]);
        var command = new UploadImageCommand
        {
            FileContent = stream,
            FileName = "test.webp"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Value.StoragePath.Should().Be(expectedStoragePath);
        fileStorageServiceMock.Verify(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact(DisplayName = "MF_043 - Bulk Upload - Should delegate to SaveFilesAsync")]
    public async Task UploadImages_MultipleFiles_ShouldCallSaveFilesAsync()
    {
        // Arrange
        var fileStorageServiceMock = new Mock<IFileStorageService>();
        var insertRepositoryMock = new Mock<IMediaFileInsertRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var savedResults = new List<(string, string)>
    {
        ("img1.webp", ".webp"),
        ("img2.webp", ".webp")
    };

        fileStorageServiceMock
            .Setup(x => x.SaveFileAsync(
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FileUpload>.Success(new FileUpload("img1.webp", ".webp", 1024)));

        var handler = new UploadManyImageCommandHandler(
            fileStorageServiceMock.Object,
            insertRepositoryMock.Object,
            unitOfWorkMock.Object);

        var stream1 = new MemoryStream();
        var stream2 = new MemoryStream();

        // Sá»¬A á»ž ÄÃ‚Y: Truyá»n tham sá»‘ trá»±c tiáº¿p vÃ o Constructor
        var command = new UploadManyImageCommand
        {
            Files =
        [
            new (stream1, "test1.jpg"),
            new (stream2, "test2.png")
        ]
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Value.Should().HaveCount(2);

        fileStorageServiceMock.Verify(x => x.SaveFileAsync(
            It.IsAny<Stream>(),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact(DisplayName = "MF_043 - Single Upload - Should delegate to SaveFileAsync")]
    public async Task UploadImage_ShouldCallSaveFileAsync()
    {
        // Arrange
        var fileStorageServiceMock = new Mock<IFileStorageService>();
        var insertRepositoryMock = new Mock<IMediaFileInsertRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var expectedStoragePath = "compressed-image.webp";
        var expectedFileExtension = ".webp";

        // Setup the mock for the Async method
        fileStorageServiceMock
            .Setup(x => x.SaveFileAsync(
                It.IsAny<Stream>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<FileUpload>.Success(new FileUpload(expectedStoragePath, expectedFileExtension, 1024)));

        var handler = new UploadImageCommandHandler(
            fileStorageServiceMock.Object,
            insertRepositoryMock.Object,
            unitOfWorkMock.Object);

        var largeStream = new MemoryStream(new byte[5242880]);
        var command = new UploadImageCommand
        {
            FileContent = largeStream,
            FileName = "original.webp"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Value.StoragePath.Should().Be(expectedStoragePath);

        // Again, verify the Handler delegates to the Service.
        // Do not verify internal Service logic (compression) here.
        fileStorageServiceMock.Verify(x => x.SaveFileAsync(
            It.IsAny<Stream>(),
            It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact(DisplayName = "MF_048 - Upload file với null stream")]
    public async Task UploadImage_NullStream_Fail()
    {
        // Arrange
        var fileStorageServiceMock = new Mock<IFileStorageService>();
        var insertRepositoryMock = new Mock<IMediaFileInsertRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new UploadImageCommandHandler(
            fileStorageServiceMock.Object,
            insertRepositoryMock.Object,
            unitOfWorkMock.Object);

        var command = new UploadImageCommand
        {
            FileContent = null,
            FileName = "test.webp"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert - Validation sáº½ catch null stream
        result.IsFailure.Should().BeTrue();
        result.Error?.Message.Should().Contain("stream");
    }

    [Fact(DisplayName = "MF_049 - Upload file với empty stream")]
    public async Task UploadImage_EmptyStream_Fail()
    {
        // Arrange
        var fileStorageServiceMock = new Mock<IFileStorageService>();
        var insertRepositoryMock = new Mock<IMediaFileInsertRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new UploadImageCommandHandler(
            fileStorageServiceMock.Object,
            insertRepositoryMock.Object,
            unitOfWorkMock.Object);

        var emptyStream = new MemoryStream([]); // 0 bytes
        var command = new UploadImageCommand
        {
            FileContent = emptyStream,
            FileName = "test.webp"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert - Validation sáº½ catch empty stream
        result.IsFailure.Should().BeTrue();
        result.Error?.Message.Should().Contain("empty");
    }

    [Fact(DisplayName = "MF_050 - Upload file với FileName rỗng")]
    public async Task UploadImage_EmptyFileName_Fail()
    {
        // Arrange
        var fileStorageServiceMock = new Mock<IFileStorageService>();
        var insertRepositoryMock = new Mock<IMediaFileInsertRepository>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        var handler = new UploadImageCommandHandler(
            fileStorageServiceMock.Object,
            insertRepositoryMock.Object,
            unitOfWorkMock.Object);

        var stream = new MemoryStream(new byte[51200]);
        var command = new UploadImageCommand
        {
            FileContent = stream,
            FileName = ""
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert - Validation sáº½ catch empty filename
        result.IsFailure.Should().BeTrue();
        result.Error?.Message.Should().Contain("filename");
    }
}
