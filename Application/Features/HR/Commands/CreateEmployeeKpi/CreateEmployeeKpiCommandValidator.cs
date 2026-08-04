using FluentValidation;

namespace Application.Features.HR.Commands.CreateEmployeeKpi;

public sealed class CreateEmployeeKpiCommandValidator : AbstractValidator<CreateEmployeeKpiCommand>
{
    public CreateEmployeeKpiCommandValidator()
    {
        RuleFor(command => command.EmployeeProfileId).GreaterThan(0);
        RuleFor(command => command.MetricName).NotEmpty().MaximumLength(100);
        RuleFor(command => command.TargetValue).GreaterThan(0);
        RuleFor(command => command.ActualValue).GreaterThanOrEqualTo(0);
        RuleFor(command => command.PeriodStart).NotEmpty();
        RuleFor(command => command.PeriodEnd).NotEmpty().GreaterThanOrEqualTo(command => command.PeriodStart);
        RuleFor(command => command.Description).MaximumLength(2000);
    }
}
