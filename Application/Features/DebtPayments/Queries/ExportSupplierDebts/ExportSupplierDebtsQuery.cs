using Application.Common.Models;
using MediatR;

namespace Application.Features.DebtPayments.Queries.ExportSupplierDebts;

public sealed record ExportSupplierDebtsQuery : IRequest<Result<FileStreamResult>>;
