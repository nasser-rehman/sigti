namespace SIGTI.Application.Features.Tickets.Commands.AddComment
{
    public sealed record AddCommentRequest(Guid AuthorId, string Content);
}
