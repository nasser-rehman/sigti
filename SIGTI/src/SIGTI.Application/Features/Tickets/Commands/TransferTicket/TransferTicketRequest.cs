namespace SIGTI.Application.Features.Tickets.Commands.TransferTicket
{
    public sealed record TransferTicketRequest(
        Guid? TargetQueueId,
        Guid? TargetTechnicianId,
        Guid TransferredById,
        string Reason
    );
}
