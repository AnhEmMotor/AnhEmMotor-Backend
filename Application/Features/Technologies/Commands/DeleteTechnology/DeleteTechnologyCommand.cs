using Application.Common.Models;
using MediatR;

namespace Application.Features.Technologies.Commands.DeleteTechnology
{
    public sealed record DeleteTechnologyCommand(int Id) : IRequest<Result>;
}
