namespace SIGTI.Application.Features.Tickets.Commands.AddComment
{
    public sealed record AddCommentRequest(Guid CreatedById, string Content);
}
