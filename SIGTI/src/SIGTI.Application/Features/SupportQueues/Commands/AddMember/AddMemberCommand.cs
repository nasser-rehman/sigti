using MediatR;

namespace SIGTI.Application.Features.SupportQueues.Commands.AddMember
{
    public sealed record AddMemberCommand(
        Guid QueueId,
        Guid TechnicianId,
        int MaxConcurrentTickets
    ) : IRequest<AddMemberResponse>;
}
