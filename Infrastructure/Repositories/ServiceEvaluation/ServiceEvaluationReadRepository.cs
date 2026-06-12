using Application.ApiContracts.Evaluation.Responses;
using Application.Common.Models;
using Application.Interfaces.Repositories.ServiceEvaluation;
using Domain.Entities;
using Domain.Primitives;
using Microsoft.EntityFrameworkCore;
using Infrastructure.DBContexts;

namespace Infrastructure.Repositories.ServiceEvaluation;

public class ServiceEvaluationReadRepository(ApplicationDBContext dbContext) : IServiceEvaluationReadRepository
{
    public async Task<Domain.Entities.ServiceEvaluation?> GetByIdAsync(int evaluationId, CancellationToken cancellationToken)
    {
        return await dbContext.ServiceEvaluations
            .AsNoTracking()
            .Where(e => e.Id == evaluationId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Result<PagedResult<ServiceEvaluationListRowResponse>>> GetPagedEvaluationsAsync(
        object filter,
        CancellationToken cancellationToken)
    {
        var f = filter as dynamic;
        string? status = f?.Status;
        string? criteria = f?.Criteria;
        string? search = f?.Search;
        int page = f?.Page ?? 1;
        int pageSize = f?.PageSize ?? 20;

        IQueryable<Domain.Entities.ServiceEvaluation> query = dbContext.ServiceEvaluations
            .AsNoTracking()
            .Include(e => e.Contact)
            .Include(e => e.ServiceBooking)
                .ThenInclude(sb => sb.Technician);

        if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(e => e.ProcessingStatus == status);
        }

        if (!string.IsNullOrWhiteSpace(criteria))
        {
            query = query.Where(e => e.Criteria == criteria);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(e =>
                e.Contact.FullName!.Contains(search) ||
                e.Contact.PhoneNumber!.Contains(search));
        }

        var total = await query.CountAsync(cancellationToken).ConfigureAwait(false);
        var items = await query
            .OrderByDescending(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new ServiceEvaluationListRowResponse
            {
                Id = e.Id,
                CustomerName = e.Contact.FullName ?? string.Empty,
                CustomerPhone = e.Contact.PhoneNumber ?? string.Empty,
                Rating = e.Rating,
                ReviewMessage = e.Review,
                Criteria = e.Criteria,
                ProcessingStatus = e.ProcessingStatus,
                TechnicianName = e.ServiceBooking.Technician != null ? e.ServiceBooking.Technician.User.UserName : null,
                RepairOrderCode = null,
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return Result<PagedResult<ServiceEvaluationListRowResponse>>.Success(new PagedResult<ServiceEvaluationListRowResponse>(items, total, page, pageSize));
    }

    public async Task<ServiceEvaluationDetailResponse> GetEvaluationDetailAsync(int evaluationId, CancellationToken cancellationToken)
    {
        var evaluation = await dbContext.ServiceEvaluations
            .AsNoTracking()
            .Include(e => e.Contact)
            .Include(e => e.Contact.Replies)
            .Include(e => e.ServiceBooking)
                .ThenInclude(sb => sb.Technician)
            .Where(e => e.Id == evaluationId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        if (evaluation == null)
        {
            throw new KeyNotFoundException("ServiceEvaluation not found");
        }

        return new ServiceEvaluationDetailResponse
        {
            Id = evaluation.Id,
            CustomerName = evaluation.Contact.FullName ?? string.Empty,
            CustomerPhone = evaluation.Contact.PhoneNumber ?? string.Empty,
            Rating = evaluation.Rating,
            ReviewMessage = evaluation.Review,
            Criteria = evaluation.Criteria,
            ProcessingStatus = evaluation.ProcessingStatus,
            TechnicianName = evaluation.ServiceBooking.Technician?.User?.UserName,
            RepairOrderCode = null,
            ChatHistory = evaluation.Contact.Replies
                .OrderBy(r => r.CreatedAt)
                .Select(r => new ServiceEvaluationChatMessageResponse
                {
                    Id = r.Id.ToString(),
                    Sender = r.RepliedById == null ? "Customer" : "Admin",
                    Content = r.Message,
                    CreatedAt = r.CreatedAt ?? DateTimeOffset.UtcNow,
                })
                .ToList(),
            DirectReplyText = evaluation.DirectReplyText,
            InternalNotes = evaluation.InternalNotes,
        };
    }
}
