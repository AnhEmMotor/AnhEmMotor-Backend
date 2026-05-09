using Domain.Entities.HR;
using MediatR;
using System.Collections.Generic;

namespace Application.Features.HR.Queries.GetCommissionRecords;

public record GetCommissionRecordsQuery : IRequest<List<CommissionRecord>>;
