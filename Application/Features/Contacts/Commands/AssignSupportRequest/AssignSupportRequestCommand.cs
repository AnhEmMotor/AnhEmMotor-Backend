using Application.Common.Models;
using MediatR;
using System;

namespace Application.Features.Contacts.Commands.AssignSupportRequest;

public record AssignSupportRequestCommand(int SupportRequestId, Guid? AssignedUserId) : IRequest<Result<int>>;
