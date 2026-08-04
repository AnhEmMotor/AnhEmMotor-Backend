using Application.ApiContracts.Contacts.Requests;
using Application.Features.Contacts.Commands.AssignSupportRequest;
using Application.Features.Contacts.Commands.CreateSupportRequest;
using Application.Features.Contacts.Commands.RateSupportCustomer;
using Application.Features.Contacts.Commands.RateSupportEmployee;
using Application.Features.Contacts.Commands.UpdateContactStatus;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Contact;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using FluentAssertions;
using Moq;

namespace UnitTests;

public class SupportRequestWorkflow
{
    private readonly Mock<ISupportRequestRepository> _supportRequests = new();
    private readonly Mock<IContactInsertRepository> _contacts = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ICurrentUserContext> _currentUser = new();

    [Fact(DisplayName = "SUPPORT_FLOW_001 - Yêu cầu mới cấp mã theo dõi bí mật")]
    public async Task CreateSupportRequest_IssuesTrackingToken()
    {
        SupportRequest? persisted = null;
        _supportRequests
            .Setup(repository => repository.AddAsync(It.IsAny<SupportRequest>(), It.IsAny<CancellationToken>()))
            .Callback<SupportRequest, CancellationToken>((request, _) => persisted = request)
            .Returns(Task.CompletedTask);
        var handler = new CreateSupportRequestCommandHandler(
            _supportRequests.Object,
            _contacts.Object,
            _unitOfWork.Object);

        var result = await handler.Handle(
            new CreateSupportRequestCommand(
                new CreateSupportRequestRequest
                {
                    FullName = "Nguyễn Văn A",
                    PhoneNumber = "0901234567",
                    Email = "customer@example.com",
                    Subject = "Cần hỗ trợ",
                    Category = "Service",
                    Content = "Nội dung"
                }),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TrackingToken.Should().NotBe(Guid.Empty);
        persisted.Should().NotBeNull();
        persisted!.CustomerTrackingToken.Should().Be(result.Value.TrackingToken);
    }

    [Fact(DisplayName = "SUPPORT_FLOW_002 - Phân công ghi nhận nhân viên và thời điểm")]
    public async Task AssignSupportRequest_AssignsEmployeeAndTimestamp()
    {
        var request = new SupportRequest { Id = 10, Status = SupportRequestStatus.New };
        var employeeId = Guid.NewGuid();
        _supportRequests.Setup(repository => repository.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        var handler = new AssignSupportRequestCommandHandler(_supportRequests.Object, _unitOfWork.Object);

        var result = await handler.Handle(new AssignSupportRequestCommand(10, employeeId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        request.AssignedUserId.Should().Be(employeeId);
        request.Status.Should().Be(SupportRequestStatus.Assigned);
        request.AssignedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "SUPPORT_FLOW_003 - Chỉ nhân viên được phân công mới bắt đầu hỗ trợ")]
    public async Task UpdateStatus_DifferentEmployee_CannotAdvanceWorkflow()
    {
        var assignedEmployeeId = Guid.NewGuid();
        var request = new SupportRequest
        {
            Id = 11,
            Status = SupportRequestStatus.Assigned,
            AssignedUserId = assignedEmployeeId
        };
        _supportRequests.Setup(repository => repository.GetByIdAsync(11, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        _currentUser.Setup(context => context.GetUserId()).Returns(Guid.NewGuid());
        var handler = new UpdateContactStatusCommandHandler(
            _supportRequests.Object,
            Mock.Of<ICustomerFeedbackRepository>(),
            Mock.Of<IJobApplicationRepository>(),
            _unitOfWork.Object,
            _currentUser.Object);

        var result = await handler.Handle(
            new UpdateContactStatusCommand(
                "support",
                11,
                new UpdateContactStatusRequest { Status = SupportRequestStatus.InProgress }),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        request.Status.Should().Be(SupportRequestStatus.Assigned);
    }

    [Fact(DisplayName = "SUPPORT_FLOW_004 - Nhân viên phụ trách đánh giá khách sau khi hoàn tất")]
    public async Task RateCustomer_AssignedEmployeeRatesClosedRequest()
    {
        var employeeId = Guid.NewGuid();
        var request = new SupportRequest
        {
            Id = 12,
            Status = SupportRequestStatus.Closed,
            AssignedUserId = employeeId
        };
        _supportRequests.Setup(repository => repository.GetByIdAsync(12, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        _currentUser.Setup(context => context.GetUserId()).Returns(employeeId);
        var handler = new RateSupportCustomerCommandHandler(
            _supportRequests.Object,
            _unitOfWork.Object,
            _currentUser.Object);

        var result = await handler.Handle(
            new RateSupportCustomerCommand(12, new SupportRatingRequest { Rating = 4, Comment = "Hợp tác tốt" }),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        request.EmployeeRatingOfCustomer.Should().Be(4);
        request.EmployeeRatingComment.Should().Be("Hợp tác tốt");
        request.EmployeeRatedAt.Should().NotBeNull();
    }

    [Fact(DisplayName = "SUPPORT_FLOW_005 - Khách chỉ đánh giá được bằng đúng mã theo dõi")]
    public async Task RateEmployee_WrongTrackingToken_IsRejected()
    {
        var request = new SupportRequest
        {
            Id = 13,
            Status = SupportRequestStatus.Closed,
            AssignedUserId = Guid.NewGuid(),
            CustomerTrackingToken = Guid.NewGuid()
        };
        _supportRequests.Setup(repository => repository.GetByIdAsync(13, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        var handler = new RateSupportEmployeeCommandHandler(_supportRequests.Object, _unitOfWork.Object);

        var result = await handler.Handle(
            new RateSupportEmployeeCommand(
                13,
                new CustomerSupportRatingRequest
                {
                    TrackingToken = Guid.NewGuid(),
                    Rating = 5,
                    Comment = "Rất tận tâm"
                }),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        request.CustomerRatingOfEmployee.Should().BeNull();
    }

    [Fact(DisplayName = "SUPPORT_FLOW_006 - Nhân viên phụ trách đi đúng hai bước xử lý và hoàn tất")]
    public async Task UpdateStatus_AssignedEmployee_AdvancesWorkflowInOrder()
    {
        var employeeId = Guid.NewGuid();
        var request = new SupportRequest
        {
            Id = 14,
            Status = SupportRequestStatus.Assigned,
            AssignedUserId = employeeId
        };
        _supportRequests.Setup(repository => repository.GetByIdAsync(14, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        _currentUser.Setup(context => context.GetUserId()).Returns(employeeId);
        var handler = new UpdateContactStatusCommandHandler(
            _supportRequests.Object,
            Mock.Of<ICustomerFeedbackRepository>(),
            Mock.Of<IJobApplicationRepository>(),
            _unitOfWork.Object,
            _currentUser.Object);

        var started = await handler.Handle(
            new UpdateContactStatusCommand(
                "support",
                14,
                new UpdateContactStatusRequest { Status = SupportRequestStatus.InProgress }),
            CancellationToken.None);
        var closed = await handler.Handle(
            new UpdateContactStatusCommand(
                "support",
                14,
                new UpdateContactStatusRequest { Status = SupportRequestStatus.Closed }),
            CancellationToken.None);

        started.IsSuccess.Should().BeTrue();
        closed.IsSuccess.Should().BeTrue();
        request.StartedAt.Should().NotBeNull();
        request.ClosedAt.Should().NotBeNull();
        request.Status.Should().Be(SupportRequestStatus.Closed);
    }

    [Fact(DisplayName = "SUPPORT_FLOW_007 - Khách đánh giá đúng nhân viên bằng mã theo dõi")]
    public async Task RateEmployee_ValidTrackingToken_PersistsRating()
    {
        var trackingToken = Guid.NewGuid();
        var request = new SupportRequest
        {
            Id = 15,
            Status = SupportRequestStatus.Closed,
            AssignedUserId = Guid.NewGuid(),
            CustomerTrackingToken = trackingToken
        };
        _supportRequests.Setup(repository => repository.GetByIdAsync(15, It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        var handler = new RateSupportEmployeeCommandHandler(_supportRequests.Object, _unitOfWork.Object);

        var result = await handler.Handle(
            new RateSupportEmployeeCommand(
                15,
                new CustomerSupportRatingRequest
                {
                    TrackingToken = trackingToken,
                    Rating = 5,
                    Comment = "Tư vấn rõ ràng"
                }),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        request.CustomerRatingOfEmployee.Should().Be(5);
        request.CustomerRatingComment.Should().Be("Tư vấn rõ ràng");
        request.CustomerRatedAt.Should().NotBeNull();
    }
}
