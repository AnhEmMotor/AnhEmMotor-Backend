using System;
using System.Threading;
using System.Threading.Tasks;
using Application.ApiContracts.Logistics.CarrierSettings.Responses;
using Application.Common.Interfaces;
using Domain.Entities.Logistics;
using MediatR;

namespace Application.Features.Logistics.Commands.TestCarrierConnection;

public class TestCarrierConnectionCommandHandler(IApplicationDbContext db)
    : IRequestHandler<TestCarrierConnectionCommand, TestCarrierConnectionResponse>
{
    public async Task<TestCarrierConnectionResponse> Handle(TestCarrierConnectionCommand request, CancellationToken cancellationToken)
    {
        var entity = await db.CarrierPartners.FindAsync(new object?[] { request.Id }, cancellationToken);
        if (entity == null)
            return new TestCarrierConnectionResponse { IsSuccess = false, Message = "Không tìm thấy đối tác vận chuyển" };

        // Placeholder implementation: validate basic fields.
        if (string.IsNullOrWhiteSpace(entity.ApiBaseUrl) || string.IsNullOrWhiteSpace(entity.ApiToken))
            return new TestCarrierConnectionResponse { IsSuccess = false, Message = "Thiếu ApiBaseUrl hoặc ApiToken" };

        // In real implementation: HTTP ping with provider SDK.
        return await Task.FromResult(new TestCarrierConnectionResponse
        {
            IsSuccess = true,
            Message = "✓ Kết nối thành công (demo)"
        });
    }
}

