using MediatR;

namespace SIGTI.Application.Features.Tickets.Commands.TransferTicket
{
    public sealed record TransferTicketCommand(
        Guid TicketId,
        Guid? TargetQueueId,
        Guid? TargetTechnicianId,
        Guid TransferredById,
        string Reason
    ) : IRequest<TransferTicketResponse>;
}
