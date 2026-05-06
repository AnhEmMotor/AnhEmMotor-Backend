using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Option;
using Application.Interfaces.Repositories.OptionValue;
using Domain.Entities;
using Domain.Primitives;
using MediatR;
using OptionEntity = Domain.Entities.Option;
using OptionValueEntity = Domain.Entities.OptionValue;

namespace Application.Features.OptionValues.Commands.CreateOptionValue;

public record CreateOptionValueCommand : IRequest<Result<int>>
{
    public int OptionId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public string? SeoTitle { get; init; }
    public string? SeoDescription { get; init; }
    public bool IsActive { get; init; } = true;
    public string? ColorCode { get; init; }
}

public class CreateOptionValueCommandHandler(
    IOptionReadRepository optionReadRepository,
    IOptionInsertRepository optionInsertRepository,
    IOptionValueInsertRepository optionValueInsertRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateOptionValueCommand, Result<int>>
{
    public async Task<Result<int>> Handle(CreateOptionValueCommand request, CancellationToken cancellationToken)
    {
        int targetOptionId = request.OptionId;

        if (targetOptionId <= 0)
        {
            var options = await optionReadRepository.GetByNamesAsync(
                ["VehicleType", "Loại xe"], 
                cancellationToken);

            var vehicleTypeOption = options.FirstOrDefault();

            if (vehicleTypeOption == null)
            {
                vehicleTypeOption = new OptionEntity { Name = "VehicleType" };
                optionInsertRepository.Add(vehicleTypeOption);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            
            targetOptionId = vehicleTypeOption.Id;
        }

        var optionValue = new OptionValueEntity
        {
            OptionId = targetOptionId,
            Name = request.Name,
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            SeoTitle = request.SeoTitle,
            SeoDescription = request.SeoDescription,
            IsActive = request.IsActive,
            ColorCode = request.ColorCode
        };

        optionValueInsertRepository.Add(optionValue);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(optionValue.Id);
    }
}
