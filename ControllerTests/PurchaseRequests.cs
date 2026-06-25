using Application.ApiContracts.PurchaseRequest.Requests;
using Application.Features.PurchaseRequests.Commands.ApproveRejectPurchaseRequest;
using Application.Features.PurchaseRequests.Commands.CreatePurchaseRequest;
using Application.Features.PurchaseRequests.Commands.DeletePurchaseRequest;
using Application.Features.PurchaseRequests.Commands.SendPurchaseRequest;
using Application.Features.PurchaseRequests.Commands.UpdatePurchaseRequest;
using Application.Features.PurchaseRequests.Queries.GetPurchaseRequestById;
using Application.Features.PurchaseRequests.Queries.GetPurchaseRequests;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Sieve.Models;
using WebAPI.Controllers.V1;

namespace ControllerTests;

public class PurchaseRequests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly PurchaseRequestsController _controller;

    public PurchaseRequests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new PurchaseRequestsController(_mediatorMock.Object);
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    #pragma warning disable IDE0079 
    #pragma warning disable CRR0035

    [Fact(DisplayName = "PR_025 - API Create PR yêu cầu quyền tương ứng")]
    public async Task PR_025_CreatePurchaseRequest_Forbidden()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreatePurchaseRequestCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.CreateAsync(new CreatePurchaseRequestCommand(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PR_026 - API Update PR yêu cầu quyền tương ứng")]
    public async Task PR_026_UpdatePurchaseRequest_Forbidden()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdatePurchaseRequestCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.UpdateAsync(1, new UpdatePurchaseRequestCommand(), CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PR_027 - API Delete PR yêu cầu quyền tương ứng")]
    public async Task PR_027_DeletePurchaseRequest_Forbidden()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeletePurchaseRequestCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.DeleteAsync(1, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PR_028 - API Send PR yêu cầu quyền tương ứng")]
    public async Task PR_028_SendPurchaseRequest_Forbidden()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<SendPurchaseRequestCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _controller.SendAsync(1, CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Fact(DisplayName = "PR_029 - API Approve/Reject PR yêu cầu quyền tương ứng")]
    public async Task PR_029_ApproveRejectPurchaseRequest_Forbidden()
    {
        _mediatorMock.Setup(m => m.Send(It.IsAny<ApproveRejectPurchaseRequestCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.ApproveRejectAsync(
                1,
                new ApproveRejectPurchaseRequestRequest { Status = "approve" },
                CancellationToken.None))
            .ConfigureAwait(true);
    }

    [Theory(DisplayName = "PR_030 - API View (Get & GetById) yêu cầu quyền tương ứng")]
    [InlineData("getall")]
    [InlineData("getbyid")]
    public async Task PR_030_ViewPurchaseRequest_Forbidden(string operation)
    {
        if (string.Compare(operation, "getall") == 0)
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetPurchaseRequestsQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new UnauthorizedAccessException());
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _controller.GetAllAsync(new SieveModel(), CancellationToken.None))
                .ConfigureAwait(true);
        } else
        {
            _mediatorMock.Setup(m => m.Send(It.IsAny<GetPurchaseRequestByIdQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new UnauthorizedAccessException());
            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _controller.GetByIdAsync(1, CancellationToken.None))
                .ConfigureAwait(true);
        }
    }
    #pragma warning restore CRR0035
    #pragma warning restore IDE0079
}
