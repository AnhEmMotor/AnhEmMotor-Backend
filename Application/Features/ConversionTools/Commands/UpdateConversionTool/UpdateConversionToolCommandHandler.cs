using Application.ApiContracts.ConversionTools.Responses;
using Application.Common.Models;
using Application.Features.ConversionTools.Commands.UpdateConversionTool;
using Application.Interfaces.Repositories.ConversionTool;
using Domain.Entities;
using MediatR;
using Mapster;

namespace Application.Features.ConversionTools.Commands.UpdateConversionTool;

public class UpdateConversionToolCommandHandler(IConversionToolReadRepository readRepo, IConversionToolWriteRepository writeRepo)
    : IRequestHandler<UpdateConversionToolCommand, Result<ConversionToolResponse>>
{
    public async Task<Result<ConversionToolResponse>> Handle(
        UpdateConversionToolCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await readRepo.GetByIdAsync(request.Id, cancellationToken).ConfigureAwait(false);
        if (entity is null)
            return Result<ConversionToolResponse>.Failure("Không tìm thấy công cụ chuyển đổi");

        entity.Type = request.Type;
        entity.Name = request.Name;
        entity.Content = request.Content;
        entity.DelaySeconds = request.DelaySeconds;
        entity.Pages = request.Pages;
        entity.IsActive = request.IsActive;
        entity.ImageUrl = request.ImageUrl;
        entity.Url = request.Url;
        entity.Status = request.Status;

        await writeRepo.UpdateAsync(entity, cancellationToken);
        await writeRepo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var response = entity.Adapt<ConversionToolResponse>();
        return Result<ConversionToolResponse>.Success(response);
    }
}
