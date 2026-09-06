using MediatR;

namespace SIGTI.Application.Features.Tickets.Commands.ResolveTicket
{
    public sealed record ResolveTicketCommand(Guid TicketId)
        : IRequest<ResolveTicketResponse>;
}
