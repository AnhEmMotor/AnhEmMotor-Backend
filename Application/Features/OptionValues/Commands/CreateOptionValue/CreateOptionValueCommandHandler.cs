using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.Option;
using Application.Interfaces.Repositories.OptionValue;
using MediatR;
using OptionEntity = Domain.Entities.Option;
using OptionValueEntity = Domain.Entities.OptionValue;

namespace Application.Features.OptionValues.Commands.CreateOptionValue;

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
            var options = await optionReadRepository.GetByNamesAsync(["VehicleType", "Loại xe"], cancellationToken).ConfigureAwait(false)   ;
            var vehicleTypeOption = options.FirstOrDefault();
            if (vehicleTypeOption == null)
            {
                vehicleTypeOption = new OptionEntity { Name = "VehicleType" };
                optionInsertRepository.Add(vehicleTypeOption);
                await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false)  ;
            }
            targetOptionId = vehicleTypeOption.Id;
        }
        var optionValue = new OptionValueEntity
        {
            OptionId = targetOptionId,
            Name = request.Name,
            ColorCode = request.ColorCode
        };
        optionValueInsertRepository.Add(optionValue);
        await unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<int>.Success(optionValue.Id);
    }
}
