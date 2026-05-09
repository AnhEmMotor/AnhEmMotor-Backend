using Application.ApiContracts.Technology.Responses;
using Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Technologies.Queries.GetAllTechnologies
{
    public sealed record GetAllTechnologiesQuery(int? CategoryId = null, int? BrandId = null) : IRequest<Result<List<TechnologyResponse>>>;
}
