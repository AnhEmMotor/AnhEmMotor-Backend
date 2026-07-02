using Application.Common.Models;
using MediatR;
using Domain.Entities;

namespace Application.Features.NewsComments.Queries.GetNewsComments;

public sealed record GetNewsCommentsQuery(int? NewsId = null) : IRequest<Result<List<NewsCommentResponse>>>;

public class NewsCommentResponse
{
    public int Id { get; set; }
    public int NewsId { get; set; }
    public string? NewsTitle { get; set; }
    public Guid? UserId { get; set; }
    public string? AuthorName { get; set; }
    public string? AuthorEmail { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
