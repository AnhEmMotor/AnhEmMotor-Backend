using Application.Common.Models;
using Domain.Primitives;
using MediatR;

namespace Application.Features.WarrantyClaims.Commands;

public record DeleteWarrantyClaimCommand(int Id) : IRequest<Result>;
