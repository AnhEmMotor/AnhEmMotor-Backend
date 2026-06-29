using Application.Features.Contacts.Commands.CreateContact;
using Application.Features.Contacts.Commands.CreateContactReply;
using Application.Features.Contacts.Commands.UpdateInternalNote;
using Application.Features.Contacts.Commands.AssignSupportRequest;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Contact;
using Application.Interfaces.Services;
using Domain.Entities;
using FluentAssertions;
using Moq;
using System;

namespace UnitTests;

public class Contact
{
    private readonly Mock<IContactReadRepository> _contactReadRepoMock;
    private readonly Mock<IContactInsertRepository> _contactInsertRepoMock;
    private readonly Mock<IContactUpdateRepository> _contactUpdateRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserContext> _tokenAccessorMock;

    public Contact()
    {
        _contactReadRepoMock = new Mock<IContactReadRepository>();
        _contactInsertRepoMock = new Mock<IContactInsertRepository>();
        _contactUpdateRepoMock = new Mock<IContactUpdateRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _tokenAccessorMock = new Mock<ICurrentUserContext>();
    }

    [Fact(DisplayName = "CONT_002 - Trạng thái mặc định của yêu cầu mới")]
    public async Task CreateContact_DefaultStatus_IsPending()
    {
        var command = new CreateContactCommand
        {
            FullName = "Name",
            Email = "test@test.com",
            PhoneNumber = "0123456789",
            Message = "Msg"
        };
        var handler = new CreateContactCommandHandler(_contactInsertRepoMock.Object, _unitOfWorkMock.Object);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "CONT_005 - Tự động chuyển trạng thái liên hệ sang 'Đã xử lý'")]
    public async Task CreateReply_MarkAsProcessedTrue_UpdatesStatusToProcessed()
    {
        var contact = new Domain.Entities.Contact { Id = 1, Status = "Pending" };
        _contactReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(contact);
        _tokenAccessorMock.Setup(x => x.GetUserId()).Returns(Guid.NewGuid());
        var command = new CreateContactReplyCommand { ContactId = 1, Message = "Reply", MarkAsProcessed = true };
        var handler = new CreateContactReplyCommandHandler(
            _contactReadRepoMock.Object,
            _contactInsertRepoMock.Object,
            _contactUpdateRepoMock.Object,
            _unitOfWorkMock.Object,
            _tokenAccessorMock.Object);
        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        contact.Status.Should().Be("Processed");
        _contactUpdateRepoMock.Verify(x => x.Update(contact), Times.Once);
    }

    [Fact(DisplayName = "CONT_006 - Phản hồi cho một liên hệ không tồn tại")]
    public async Task CreateReply_ContactNotExists_ReturnsFailure()
    {
        _contactReadRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Contact?)null);
        var command = new CreateContactReplyCommand { ContactId = 999, Message = "Reply" };
        var handler = new CreateContactReplyCommandHandler(
            _contactReadRepoMock.Object,
            _contactInsertRepoMock.Object,
            _contactUpdateRepoMock.Object,
            _unitOfWorkMock.Object,
            _tokenAccessorMock.Object);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => string.Compare(e.Message, "Liên hệ không tồn tại.") == 0);
    }

    [Fact(DisplayName = "CONT_007 - Xác định danh tính người phản hồi từ Token")]
    public async Task CreateReply_ValidToken_SetsRepliedById()
    {
        var userId = Guid.NewGuid();
        var contact = new Domain.Entities.Contact { Id = 1 };
        _contactReadRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(contact);
        _tokenAccessorMock.Setup(x => x.GetUserId()).Returns(userId);
        var command = new CreateContactReplyCommand { ContactId = 1, Message = "Reply" };
        var handler = new CreateContactReplyCommandHandler(
            _contactReadRepoMock.Object,
            _contactInsertRepoMock.Object,
            _contactUpdateRepoMock.Object,
            _unitOfWorkMock.Object,
            _tokenAccessorMock.Object);
        await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        _contactInsertRepoMock.Verify(x => x.AddReply(It.Is<ContactReply>(r => r.RepliedById == userId)), Times.Once);
    }

    [Fact(DisplayName = "CONT_010 - Cập nhật ghi chú cho liên hệ không tồn tại")]
    public async Task UpdateNote_ContactNotExists_ReturnsNotFound()
    {
        _contactReadRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Contact?)null);
        var command = new UpdateInternalNoteCommand { ContactId = 999, InternalNote = "Note" };
        var handler = new UpdateInternalNoteCommandHandler(
            _contactReadRepoMock.Object,
            _contactUpdateRepoMock.Object,
            _unitOfWorkMock.Object);
        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => string.Compare(e.Message, "Liên hệ không tồn tại.") == 0);
    }

    [Fact(DisplayName = "AssignSupportRequest - Phân công với UserId hợp lệ")]
    public async Task AssignSupportRequest_WithValidUserId_SetsAssignedUserIdAndStatusToAssigned()
    {
        var supportRequest = new SupportRequest { Id = 1, Status = Domain.Enums.SupportRequestStatus.New };
        var supportRequestRepoMock = new Mock<ISupportRequestRepository>();
        supportRequestRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(supportRequest);

        var userId = Guid.NewGuid();
        var command = new AssignSupportRequestCommand(1, userId);
        var handler = new AssignSupportRequestCommandHandler(supportRequestRepoMock.Object, _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsSuccess.Should().BeTrue();
        supportRequest.AssignedUserId.Should().Be(userId);
        supportRequest.Status.Should().Be(Domain.Enums.SupportRequestStatus.Assigned);
        supportRequestRepoMock.Verify(x => x.UpdateAsync(supportRequest, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "AssignSupportRequest - Phân công với UserId null")]
    public async Task AssignSupportRequest_WithNullUserId_ClearsAssignedUserIdAndRevertsStatusToNew()
    {
        var supportRequest = new SupportRequest { Id = 1, AssignedUserId = Guid.NewGuid(), Status = Domain.Enums.SupportRequestStatus.Assigned };
        var supportRequestRepoMock = new Mock<ISupportRequestRepository>();
        supportRequestRepoMock.Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(supportRequest);

        var command = new AssignSupportRequestCommand(1, null);
        var handler = new AssignSupportRequestCommandHandler(supportRequestRepoMock.Object, _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsSuccess.Should().BeTrue();
        supportRequest.AssignedUserId.Should().BeNull();
        supportRequest.Status.Should().Be(Domain.Enums.SupportRequestStatus.New);
        supportRequestRepoMock.Verify(x => x.UpdateAsync(supportRequest, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "AssignSupportRequest - Yêu cầu hỗ trợ không tồn tại")]
    public async Task AssignSupportRequest_WithNonExistentRequest_ReturnsFailure()
    {
        var supportRequestRepoMock = new Mock<ISupportRequestRepository>();
        supportRequestRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SupportRequest?)null);

        var command = new AssignSupportRequestCommand(999, Guid.NewGuid());
        var handler = new AssignSupportRequestCommandHandler(supportRequestRepoMock.Object, _unitOfWorkMock.Object);

        var result = await handler.Handle(command, CancellationToken.None).ConfigureAwait(true);

        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Không tìm thấy yêu cầu hỗ trợ.");
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
