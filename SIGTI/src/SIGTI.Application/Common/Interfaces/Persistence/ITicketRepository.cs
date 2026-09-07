using SIGTI.Application.Common.Enums;
using SIGTI.Application.Common.Models;
using SIGTI.Application.Features.Tickets.Queries.ListTickets;
using SIGTI.Domain.Entities;

namespace SIGTI.Application.Common.Interfaces.Persistence
{
    public interface ITicketRepository
    {
        Task AddAsync(Ticket ticket, CancellationToken cancellationToken);

        Task<Ticket?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken
        );

        Task<Ticket?> GetByNumberAsync(
            int number,
            CancellationToken cancellationToken
        );

        // List all tickets created by a specific user
        Task<IReadOnlyCollection<Ticket>> ListCreatedByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken
        );

        // List all tickets assigned to a specific technician
        Task<IReadOnlyCollection<Ticket>> ListAssignedToTechnicianAsync(
            Guid technicianId,
            CancellationToken cancellationToken
        );

        Task<IReadOnlyCollection<Ticket>> GetActiveByQueueAsync(
            Guid queueId,
            CancellationToken cancellationToken
        );

        Task<Dictionary<Guid, int>> GetActiveTicketCountsByTechniciansAsync(
            Guid queueId,
            CancellationToken cancellationToken
        );

        Task<Ticket?> GetDetailsByIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken
        );

        // PAGED FUNCTION BLOCK
        Task<IReadOnlyCollection<Ticket>> ListAsync(
            TicketListFilter filter,
            TicketSortField sortBy,
            SortDirection sortDirection,
            int skip,
            int take,
            CancellationToken cancellationToken
        );

        Task<int> CountAsync(
            TicketListFilter filter,
            CancellationToken cancellationToken
        );

        // END PAGED FUNCTION BLOCK
    }
}
