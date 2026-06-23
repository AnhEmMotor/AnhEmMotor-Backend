using Application.Common.Models;
using Application.Features.Contacts.Commands.CreateContact;
using MediatR;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebAPI.Controllers.V1;

namespace ControllerTests;

public class Contact
{
    private readonly Mock<ISender> _senderMock;
    private readonly ContactsController _contactsController;

    public Contact()
    {
        _senderMock = new Mock<ISender>();
        _contactsController = new ContactsController(_senderMock.Object);
        var httpContext = new DefaultHttpContext();
        _contactsController.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    [Fact(DisplayName = "CONT_001 - Gửi yêu cầu liên hệ hợp lệ (API)")]
    public async Task CreateContact_ValidRequest_ReturnsContactId()
    {
        var command = new CreateContactCommand
        {
            FullName = "Test User",
            Email = "test@gmail.com",
            PhoneNumber = "0909123456",
            Subject = "Cần hỗ trợ",
            Message = "Nội dung cần hỗ trợ"
        };
        _senderMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>())).ReturnsAsync(Result<int>.Success(100));
        var result = await _contactsController.CreateAsync(command, CancellationToken.None).ConfigureAwait(true);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<int>(okResult.Value);
        Assert.Equal(100, response);
    }

    [Fact(DisplayName = "CONT_002 - Tải lên file CV ứng viên hợp lệ")]
    public async Task UploadCv_ValidFile_ReturnsStoragePath()
    {
        var fileMock = new Mock<IFormFile>();
        var content = "test pdf file content";
        var fileName = "my_cv.pdf";
        var ms = new MemoryStream();
        using (var writer = new StreamWriter(ms, leaveOpen: true))
        {
            writer.Write(content);
            writer.Flush();
        }
        ms.Position = 0;

        fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(ms.Length);

        var expectedPath = "cv/unique-guid.pdf";
        _senderMock.Setup(m => m.Send(It.IsAny<Application.Features.Contacts.Commands.UploadCv.UploadCvCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(expectedPath));

        var result = await _contactsController.UploadCvAsync(fileMock.Object, CancellationToken.None).ConfigureAwait(true);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<string>(okResult.Value);
        Assert.Equal(expectedPath, response);
    }
}
