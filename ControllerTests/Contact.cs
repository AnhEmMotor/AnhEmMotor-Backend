using Application.Common.Models;
using Application.Features.Contacts.Commands.CreateContact;
using MediatR;
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
}
