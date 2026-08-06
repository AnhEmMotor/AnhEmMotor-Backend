using Application.Common.Models;
using MediatR;

namespace Application.Features.ManagerChat.Queries.GetChatToolCatalog;

public record GetChatToolCatalogQuery() : IRequest<Result<List<ChatToolLabelDto>>>;
