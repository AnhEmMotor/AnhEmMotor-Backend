using Application.ApiContracts.Technology.Responses;
using Application.Common.Models;
using MediatR;
using System;

namespace Application.Features.Technologies.Commands.CreateTechnology
{
    public sealed record CreateTechnologyCommand(
        string Name,
        int? CategoryId,
        int? BrandId,
        string? DefaultTitle,
        string? DefaultDescription,
        string? DefaultImageUrl) : IRequest<Result<TechnologyResponse>>;
}
