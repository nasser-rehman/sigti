using MediatR;

namespace SIGTI.Application.Features.Tickets.Commands.CloseTicket
{
    public sealed record CloseTicketCommand(Guid TicketId)
        : IRequest<CloseTicketResponse>;
}
