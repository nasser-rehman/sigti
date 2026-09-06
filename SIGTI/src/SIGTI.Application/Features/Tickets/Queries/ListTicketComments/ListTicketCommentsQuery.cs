using MediatR;

namespace SIGTI.Application.Features.Tickets.Queries.ListTicketComments
{
    public sealed record ListTicketCommentsQuery(Guid TicketId)
        : IRequest<IReadOnlyList<TicketCommentResponse>>;
}
