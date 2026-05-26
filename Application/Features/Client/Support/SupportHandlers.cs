using MediatR;
using AnhEmMotor.Application.ApiContracts.Client.Support;
using AnhEmMotor.Application.Interfaces.Repositories.Lead;
using AnhEmMotor.Domain.Entities;
using AnhEmMotor.Domain.Constants;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace AnhEmMotor.Application.Features.Client.Support
{
    public record GetFaqsQuery(string Search) : IRequest<List<FaqResponse>>;
    public record RequestCallbackCommand(CallbackRequest Request) : IRequest<bool>;
    public record SubmitFeedbackCommand(FeedbackRequest Request) : IRequest<bool>;

    public class GetFaqsHandler : IRequestHandler<GetFaqsQuery, List<FaqResponse>>
    {
        public async Task<List<FaqResponse>> Handle(GetFaqsQuery request, CancellationToken cancellationToken)
        {
            return await Task.FromResult(new List<FaqResponse>
            {
                new FaqResponse("Lịch bảo dưỡng xe SH là bao lâu?", "Thông thường là mỗi 4000km hoặc 6 tháng một lần."),
                new FaqResponse("Có hỗ trợ cứu hộ tại chỗ không?", "AnhEmMotor hỗ trợ cứu hộ 24/7 trong khu vực Biên Hòa.")
            });
        }
    }

    public class RequestCallbackHandler : IRequestHandler<RequestCallbackCommand, bool>
    {
        private readonly ILeadInsertRepository _leadRepo;
        public RequestCallbackHandler(ILeadInsertRepository leadRepo) => _leadRepo = leadRepo;

        public async Task<bool> Handle(RequestCallbackCommand request, CancellationToken cancellationToken)
        {
            var lead = new Lead
            {
                PhoneNumber = request.Request.PhoneNumber,
                Notes = request.Request.IssueDescription,
                Source = "Callback",
                CreatedAt = DateTime.UtcNow,
                Status = "New",
                Priority = "Hot"
            };
            await _leadRepo.AddAsync(lead, cancellationToken);
            return true;
        }
    }

    public class SubmitFeedbackHandler : IRequestHandler<SubmitFeedbackCommand, bool>
    {
        // In a real app, use a TicketRepository here. 
        // For now, mocking the result to avoid missing entity references.
        public async Task<bool> Handle(SubmitFeedbackCommand request, CancellationToken cancellationToken)
        {
            if (request.Request.Rating < 3)
            {
                // Logic to create ticket via ISupportTicketRepository
            }
            return await Task.FromResult(true);
        }
    }
}