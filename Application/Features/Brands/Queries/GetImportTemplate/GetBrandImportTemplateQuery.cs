using Application.Common.Models;
using MediatR;

namespace Application.Features.Brands.Queries.GetImportTemplate;

public sealed record GetBrandImportTemplateQuery : IRequest<Result<FileStreamResult>>;
