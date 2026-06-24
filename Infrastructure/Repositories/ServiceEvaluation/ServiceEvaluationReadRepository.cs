using Application.ApiContracts.Evaluation.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.ServiceEvaluation;
using Domain.Constants;
using Domain.Primitives;
using Infrastructure.DBContexts;
using Microsoft.EntityFrameworkCore;
using Sieve.Models;

namespace Infrastructure.Repositories.ServiceEvaluation;

public class ServiceEvaluationReadRepository(ApplicationDBContext dbContext, ISievePaginator paginator) : IServiceEvaluationReadRepository
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
        var result = await paginator
            .ApplyAsync<Domain.Entities.ServiceEvaluation, ServiceEvaluationListRowResponse>(
                query,
                sieveModel,
                DataFetchMode.ActiveOnly,
                cancellationToken)
            .ConfigureAwait(false);
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
