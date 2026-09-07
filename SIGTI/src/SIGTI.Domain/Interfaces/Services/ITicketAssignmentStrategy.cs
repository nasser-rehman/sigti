using SIGTI.Domain.Entities;

namespace SIGTI.Domain.Interfaces.Services
{
    public interface ITicketAssignmentStrategy
    {
        SupportQueueMember SelectTechnician(
            SupportQueue queue,
            IReadOnlyDictionary<Guid, int> activeWorkloads
        );
    }
}
