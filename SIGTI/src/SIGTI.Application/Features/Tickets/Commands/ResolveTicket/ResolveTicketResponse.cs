using SIGTI.Domain.Enums;

namespace SIGTI.Application.Features.Tickets.Commands.ResolveTicket
{
    public sealed record ResolveTicketResponse(
        Guid Id,
        TicketStatus Status,
        DateTime? ResolvedAt,
        DateTime? UpdatedAt
    );
}
