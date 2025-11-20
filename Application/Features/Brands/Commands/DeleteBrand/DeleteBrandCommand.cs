using Domain.Helpers;
using MediatR;

namespace Application.Features.Brands.Commands.DeleteBrand;

public sealed record DeleteBrandCommand(int Id) : IRequest<ErrorResponse?>;
