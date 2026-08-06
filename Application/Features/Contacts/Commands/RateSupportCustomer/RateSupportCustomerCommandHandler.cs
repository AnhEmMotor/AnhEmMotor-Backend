using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Contact;
using Application.Interfaces.Services;
using Domain.Enums;
using MediatR;

namespace Application.Features.Contacts.Commands.RateSupportCustomer;

public class RateSupportCustomerCommandHandler(
    ISupportRequestRepository supportRequestRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUserContext) : IRequestHandler<RateSupportCustomerCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(RateSupportCustomerCommand request, CancellationToken cancellationToken)
    {
        var supportRequest = await supportRequestRepository
            .GetByIdAsync(request.SupportRequestId, cancellationToken)
            .ConfigureAwait(false);
        if (supportRequest is null)
            return Result<bool>.Failure("Không tìm thấy yêu cầu hỗ trợ.");
        if (request.Request.Rating is < 1 or > 5)
            return Result<bool>.Failure("Điểm đánh giá phải từ 1 đến 5.");
        if (supportRequest.Status != SupportRequestStatus.Closed)
            return Result<bool>.Failure("Chỉ có thể đánh giá sau khi yêu cầu hoàn tất.");
        if (supportRequest.AssignedUserId != currentUserContext.GetUserId())
            return Result<bool>.Failure("Chỉ nhân viên được phân công mới có thể đánh giá khách hàng.");
        supportRequest.EmployeeRatingOfCustomer = request.Request.Rating;
        supportRequest.EmployeeRatingComment = request.Request.Comment?.Trim();
        supportRequest.EmployeeRatedAt = DateTimeOffset.UtcNow;
        await supportRequestRepository.UpdateAsync(supportRequest, cancellationToken).ConfigureAwait(false);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<bool>.Success(true);
    }
}
