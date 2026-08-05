using Application.Common.Models;
using Application.Interfaces.Services;
using MediatR;

namespace Application.Features.ManagerChat.Queries.GetChatToolCatalog;

public class GetChatToolCatalogQueryHandler(IChatToolCatalogProvider catalogProvider) : IRequestHandler<GetChatToolCatalogQuery, Result<List<ChatToolLabelDto>>>
{
    public Task<Result<List<ChatToolLabelDto>>> Handle(
        GetChatToolCatalogQuery request,
        CancellationToken cancellationToken)
    {
        var dtos = catalogProvider.GetCatalog().Select(e => new ChatToolLabelDto(e.Name, e.Label)).ToList();
        return Task.FromResult(Result<List<ChatToolLabelDto>>.Success(dtos));
    }
}
