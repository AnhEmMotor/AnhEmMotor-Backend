using Application.Common.Models;
using Application.Features.ChatTools.Common;
using MediatR;

namespace Application.Features.ChatTools.Queries.GetEmployeeKpiForChat;

public sealed record GetEmployeeKpiForChatQuery(int EmployeeId) : IRequest<Result<ChatToolEnvelope<ChatEmployeeKpiDto>>>;
