using Domain.Entities.HR;
using MediatR;

namespace Application.Features.HR.Queries.GetCommissionRecords;

public record GetCommissionRecordsQuery : IRequest<List<CommissionRecord>>;
