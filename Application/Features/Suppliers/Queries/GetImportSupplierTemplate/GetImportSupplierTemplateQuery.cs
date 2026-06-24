using Application.Common.Models;
using MediatR;

namespace Application.Features.Suppliers.Queries.GetImportSupplierTemplate;

public sealed record GetImportSupplierTemplateQuery : IRequest<Result<FileStreamResult>>;
