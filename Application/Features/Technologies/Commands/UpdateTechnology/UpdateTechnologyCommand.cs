using Application.ApiContracts.Technology.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Technologies.Commands.UpdateTechnology
{
    public sealed record UpdateTechnologyCommand(
        int Id,
        string Name,
        int? CategoryId,
        int? BrandId,
        string? DefaultTitle,
        string? DefaultDescription,
        string? DefaultImageUrl) : IRequest<Result<TechnologyResponse>>;
}
