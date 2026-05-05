using Application.ApiContracts.Banner.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Banners.Queries.GetActiveBanners;

public sealed record GetActiveBannersQuery : IRequest<Result<List<BannerResponse>>>;
