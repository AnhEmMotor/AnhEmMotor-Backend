using FluentValidation;

namespace Application.Features.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdQueryValidator()
    { RuleFor(x => x.Id).GreaterThan(0).WithMessage("Product ID must be greater than 0."); }
}
