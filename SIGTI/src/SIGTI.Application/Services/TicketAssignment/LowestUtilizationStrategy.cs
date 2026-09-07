using SIGTI.Application.Common.Interfaces.Persistence;
using SIGTI.Domain.Entities;
using SIGTI.Domain.Enums;
using SIGTI.Domain.Exceptions;
using SIGTI.Domain.Interfaces.Services;

namespace SIGTI.Application.Services.TicketAssignment
{
    public class LowestUtilizationStrategy : ITicketAssignmentStrategy
    {
        public SupportQueueMember SelectTechnician(
            SupportQueue queue,
            IReadOnlyDictionary<Guid, int> activeWorkloads
        )
        {
            var availableMembers = queue
                .Members.Where(m => m.IsActive)
                .ToList();

            if (!availableMembers.Any())
                throw new DomainException("A fila não possui técnicos ativos.");

            return availableMembers
                .OrderBy(m =>
                    activeWorkloads.GetValueOrDefault(m.TechnicianId, 0)
                )
                .ThenBy(m => m.CreatedAt)
                .FirstOrDefault();
        }
    }
}
