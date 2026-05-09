using Application.ApiContracts.Technology.Responses;
using Application.Common.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Technologies.Commands.CreateTechnologyCategory
{
    public sealed record CreateTechnologyCategoryCommand(string Name) : IRequest<Result<TechnologyCategoryResponse>>;
}
