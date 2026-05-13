using Application.Common.Models;
using MediatR;

namespace Application.Features.News.Commands.DeleteNews;

public sealed record DeleteNewsCommand(int Id) : IRequest<Result<Unit>>;
