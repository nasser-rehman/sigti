namespace SIGTI.Application.Features.Tickets.Commands.TransferTicket
{
    public sealed record TransferTicketResponse(
        Guid TicketId,
        int TicketNumber,
        string TicketCode,
        Guid QueueId,
        string QueueName,
        Guid TechnicianId,
        string TechnicianName,
        string Reason,
        string Status,
        DateTime TransferredAt
    );
}
