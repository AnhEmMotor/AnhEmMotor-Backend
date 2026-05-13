using Application.Common.Models;
using MediatR;

namespace Application.Features.HR.Commands.DeleteCommissionPolicy;

public record DeleteCommissionPolicyCommand(int Id) : IRequest<Result>;
