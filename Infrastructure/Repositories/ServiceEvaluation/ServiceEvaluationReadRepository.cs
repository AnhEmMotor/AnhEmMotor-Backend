using Application.ApiContracts.Evaluation.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.ServiceEvaluation;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;
using Sieve.Services;

namespace Infrastructure.Repositories.ServiceEvaluation;

public class ServiceEvaluationReadRepository(ApplicationDBContext dbContext, ISieveProcessor sieveProcessor) : IServiceEvaluationReadRepository
{
    public Task<Domain.Entities.ServiceEvaluation?> GetByIdAsync(int evaluationId, CancellationToken cancellationToken)
    {
        return dbContext.ServiceEvaluations
            .AsNoTracking()
            .Where(e => e.Id == evaluationId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Result<PagedResult<ServiceEvaluationListRowResponse>>> GetPagedEvaluationsAsync(
        SieveModel sieveModel,
        CancellationToken cancellationToken)
    {
        IQueryable<Domain.Entities.ServiceEvaluation> query = dbContext.ServiceEvaluations
            .AsNoTracking()
            .Include(e => e.Contact)
            .Include(e => e.ServiceBooking)
            .ThenInclude(sb => sb.AssignedSale);
        var total = await sieveProcessor
            .Apply(sieveModel, query, applyPagination: false)
            .CountAsync(cancellationToken)
            .ConfigureAwait(false);
        var items = await sieveProcessor
            .Apply(sieveModel, query)
            .Select(
                e => new ServiceEvaluationListRowResponse
                {
                    Id = e.Id,
                    CustomerName = e.Contact.FullName ?? string.Empty,
                    CustomerPhone = e.Contact.PhoneNumber ?? string.Empty,
                    Rating = e.Rating,
                    ReviewMessage = e.Review,
                    Criteria = e.Criteria,
                    ProcessingStatus = e.ProcessingStatus,
                    TechnicianName =
                        e.ServiceBooking.AssignedSale != null ? e.ServiceBooking.AssignedSale.UserName : null,
                    RepairOrderCode = null,
                })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        var result = new PagedResult<ServiceEvaluationListRowResponse>(
            items,
            total,
            sieveModel.Page ?? 1,
            sieveModel.PageSize ?? 10);
        return Result<PagedResult<ServiceEvaluationListRowResponse>>.Success(result);
    }

    public async Task<ServiceEvaluationDetailResponse> GetEvaluationDetailAsync(
        int evaluationId,
        CancellationToken cancellationToken)
    {
        var evaluation = await dbContext.ServiceEvaluations
                .AsNoTracking()
                .Include(e => e.Contact)
                .Include(e => e.Contact.Replies)
                .Include(e => e.ServiceBooking)
                .ThenInclude(sb => sb.AssignedSale)
                .Where(e => e.Id == evaluationId)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false) ??
            throw new KeyNotFoundException("ServiceEvaluation not found");
        return new ServiceEvaluationDetailResponse
        {
            Id = evaluation.Id,
            CustomerName = evaluation.Contact.FullName ?? string.Empty,
            CustomerPhone = evaluation.Contact.PhoneNumber ?? string.Empty,
            Rating = evaluation.Rating,
            ReviewMessage = evaluation.Review,
            Criteria = evaluation.Criteria,
            ProcessingStatus = evaluation.ProcessingStatus,
            TechnicianName = evaluation.ServiceBooking.AssignedSale?.UserName,
            RepairOrderCode = null,
            ChatHistory =
                [.. evaluation.Contact.Replies
                    .OrderBy(r => r.CreatedAt)
                    .Select(
                        r => new ServiceEvaluationChatMessageResponse
                    {
                        Id = r.Id.ToString(),
                        Sender = r.RepliedById == null ? "Customer" : "Admin",
                        Content = r.Message,
                        CreatedAt = r.CreatedAt ?? DateTimeOffset.UtcNow,
                    })],
            DirectReplyText = evaluation.DirectReplyText,
            InternalNotes = evaluation.InternalNotes,
        };
    }
}
