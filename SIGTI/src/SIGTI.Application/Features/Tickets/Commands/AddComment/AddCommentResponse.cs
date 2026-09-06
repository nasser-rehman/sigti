namespace SIGTI.Application.Features.Tickets.Commands.AddComment
{
    public sealed record AddCommentResponse(
        Guid Id,
        Guid TicketId,
        Guid AuthorId,
        string AuthorName,
        string Content,
        DateTime CreatedAt
    );
}
