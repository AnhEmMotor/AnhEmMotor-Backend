using Application.Common.Models;
using MediatR;

namespace Application.Features.Products.Commands.TrackProductView;

public record TrackProductViewCommand(int ProductId, int DwellTimeMs, string? VisitorKey, int? VariantId, int? VariantColorId) : IRequest<Result>;
