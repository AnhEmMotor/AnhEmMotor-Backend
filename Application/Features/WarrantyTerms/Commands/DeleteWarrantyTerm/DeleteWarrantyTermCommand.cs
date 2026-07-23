using Application.Common.Models;
using MediatR;

namespace Application.Features.WarrantyTerms.Commands.DeleteWarrantyTerm;

public sealed record DeleteWarrantyTermCommand(int Id) : IRequest<Result<int>>;
