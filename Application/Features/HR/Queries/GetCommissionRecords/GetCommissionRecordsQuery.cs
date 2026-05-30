using Application.Common.Models;
using Domain.Entities;
using MediatR;

namespace Application.Features.HR.Queries.GetCommissionRecords;

public record GetCommissionRecordsQuery : IRequest<Result<List<CommissionRecord>>>;

