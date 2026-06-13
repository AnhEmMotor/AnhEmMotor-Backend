using Application.ApiContracts.Logistics.CarrierSettings.Responses;
using Application.Interfaces.Repositories.CarrierPartner;
using MediatR;
using System;

namespace Application.Features.Logistics.Commands.TestCarrierConnection;

public class TestCarrierConnectionCommandHandler(ICarrierPartnerReadRepository carrierPartnerReadRepository) : IRequestHandler<TestCarrierConnectionCommand, TestCarrierConnectionResponse>
{
    public async Task<TestCarrierConnectionResponse> Handle(
        TestCarrierConnectionCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await carrierPartnerReadRepository.GetByIdAsync(request.Id, cancellationToken)
            .ConfigureAwait(false);
        if (entity == null)
            return new TestCarrierConnectionResponse
            {
                IsSuccess = false,
                Message = "Không tìm thấy đối tác vận chuyển"
            };
        if (string.IsNullOrWhiteSpace(entity.ApiBaseUrl) || string.IsNullOrWhiteSpace(entity.ApiToken))
            return new TestCarrierConnectionResponse { IsSuccess = false, Message = "Thiếu ApiBaseUrl hoặc ApiToken" };
        return new TestCarrierConnectionResponse { IsSuccess = true, Message = "✓ Kết nối thành công (demo)" };
    }
}

