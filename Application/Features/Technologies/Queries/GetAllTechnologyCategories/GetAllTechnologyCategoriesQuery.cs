using Application.ApiContracts.Technology.Responses;
using Application.Common.Models;
using MediatR;
using System;

namespace Application.Features.Technologies.Queries.GetAllTechnologyCategories
{
    public sealed record GetAllTechnologyCategoriesQuery() : IRequest<Result<List<TechnologyCategoryResponse>>>;
}
