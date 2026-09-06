namespace SIGTI.Application.Features.Tickets.Queries.ListTicketComments
{
    public sealed record TicketCommentResponse(
        Guid Id,
        Guid TicketId,
        Guid AuthorId,
        string AuthorName,
        string Content,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );
}
