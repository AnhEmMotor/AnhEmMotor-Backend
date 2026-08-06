using Application.ApiContracts.Contacts.Responses;
using Application.Common.Models;
using MediatR;

namespace Application.Features.Contacts.Queries.GetSupportRequestTracking;

public record GetSupportRequestTrackingQuery(int SupportRequestId, Guid TrackingToken) : IRequest<Result<SupportRequestTrackingResponse>>;
