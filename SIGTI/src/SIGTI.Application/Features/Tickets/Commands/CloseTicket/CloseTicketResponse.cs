using SIGTI.Domain.Enums;

namespace SIGTI.Application.Features.Tickets.Commands.CloseTicket
{
    public sealed record CloseTicketResponse(
        Guid Id,
        TicketStatus Status,
        DateTime? ClosedAt,
        DateTime? UpdatedAt
    );
}
