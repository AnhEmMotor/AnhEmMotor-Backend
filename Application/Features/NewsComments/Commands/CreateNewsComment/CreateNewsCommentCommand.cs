using Application.Common.Models;
using MediatR;

namespace Application.Features.NewsComments.Commands.CreateNewsComment;

public sealed record CreateNewsCommentCommand : IRequest<Result<int>>
{
    public int? NewsId { get; set; }
    public string? ArticleType { get; set; }
    public string? ArticleSlug { get; set; }
    public Guid? UserId { get; set; }
    public string? AuthorName { get; set; }
    public string? AuthorEmail { get; set; }
    public string Content { get; set; } = string.Empty;
}
