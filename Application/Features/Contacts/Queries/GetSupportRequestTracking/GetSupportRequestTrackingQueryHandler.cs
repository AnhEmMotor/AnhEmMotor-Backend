using Application.ApiContracts.Contacts.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.Contact;
using MediatR;

namespace Application.Features.Contacts.Queries.GetSupportRequestTracking;

public class GetSupportRequestTrackingQueryHandler(ISupportRequestRepository supportRequestRepository) : IRequestHandler<GetSupportRequestTrackingQuery, Result<SupportRequestTrackingResponse>>
{
    public async Task<Result<SupportRequestTrackingResponse>> Handle(
        GetSupportRequestTrackingQuery request,
        CancellationToken cancellationToken)
    {
        var supportRequest = await supportRequestRepository
            .GetByIdAsync(request.SupportRequestId, cancellationToken)
            .ConfigureAwait(false);
        if (supportRequest is null ||
            supportRequest.CustomerTrackingToken is null ||
            supportRequest.CustomerTrackingToken != request.TrackingToken)
            return Result<SupportRequestTrackingResponse>.Failure("Không tìm thấy yêu cầu hỗ trợ.");
        return Result<SupportRequestTrackingResponse>.Success(
            new SupportRequestTrackingResponse
            {
                Id = supportRequest.Id,
                Subject = supportRequest.Subject,
                Status = supportRequest.Status,
                AssignedUserName = supportRequest.AssignedUser?.FullName,
                CreatedAt = supportRequest.CreatedAt,
                AssignedAt = supportRequest.AssignedAt,
                StartedAt = supportRequest.StartedAt,
                ClosedAt = supportRequest.ClosedAt,
                CustomerRatingOfEmployee = supportRequest.CustomerRatingOfEmployee,
                CustomerRatingComment = supportRequest.CustomerRatingComment
            });
    }
}
