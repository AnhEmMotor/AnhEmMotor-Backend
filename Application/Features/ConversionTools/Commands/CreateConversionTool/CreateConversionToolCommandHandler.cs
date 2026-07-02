using Application.ApiContracts.ConversionTools.Responses;
using Application.Common.Models;
using Application.Features.ConversionTools.Commands.CreateConversionTool;
using Application.Interfaces.Repositories.ConversionTool;
using Domain.Entities;
using MediatR;
using Mapster;

namespace Application.Features.ConversionTools.Commands.CreateConversionTool;

public class CreateConversionToolCommandHandler(IConversionToolWriteRepository repository)
    : IRequestHandler<CreateConversionToolCommand, Result<ConversionToolResponse>>
{
    public async Task<Result<ConversionToolResponse>> Handle(
        CreateConversionToolCommand request,
        CancellationToken cancellationToken)
    {
        var entity = new ConversionTool
        {
            Type = request.Type,
            Name = request.Name,
            Content = request.Content,
            DelaySeconds = request.DelaySeconds,
            Pages = request.Pages,
            IsActive = request.IsActive,
            ImageUrl = request.ImageUrl,
            Url = request.Url,
            Status = request.Status,
        };

        await repository.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var response = entity.Adapt<ConversionToolResponse>();
        return Result<ConversionToolResponse>.Success(response);
    }
}
