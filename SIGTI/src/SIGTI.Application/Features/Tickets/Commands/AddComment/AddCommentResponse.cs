namespace SIGTI.Application.Features.Tickets.Commands.AddComment
{
    public sealed record AddCommentResponse(
        Guid Id,
        Guid TicketId,
        Guid CreatedById,
        string CreatedByName,
        string Content,
        DateTime CreatedAt
    );
}
