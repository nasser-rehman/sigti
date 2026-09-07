using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using SIGTI.Application.Common.Enums;
using SIGTI.Application.Common.Interfaces.Persistence;
using SIGTI.Application.Features.Tickets.Queries.ListTickets;
using SIGTI.Domain.Entities;
using SIGTI.Domain.Enums;
using SIGTI.Infrastructure.Persistence.Context;

namespace SIGTI.Infrastructure.Persistence.Repositories
{
    public class TicketRepository : ITicketRepository
    {
        private readonly ApplicationDbContext _context;

        public TicketRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            Ticket ticket,
            CancellationToken cancellationToken
        )
        {
            await _context.Tickets.AddAsync(ticket, cancellationToken);
        }

        public async Task<Ticket?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken
        )
        {
            return await _context
                .Tickets.Include(ticket => ticket.Department)
                .Include(ticket => ticket.CreatedBy)
                .Include(ticket => ticket.Queue)
                .Include(ticket => ticket.Comments)
                    .ThenInclude(comment => comment.Author)
                .Include(ticket => ticket.Assignments)
                    .ThenInclude(assignment => assignment.Technician)
                .Include(ticket => ticket.Assignments)
                    .ThenInclude(assignment => assignment.AssignedBy)
                .FirstOrDefaultAsync(
                    ticket => ticket.Id == id,
                    cancellationToken
                );
        }

        public async Task<Ticket?> GetByNumberAsync(
            int number,
            CancellationToken cancellationToken
        )
        {
            return await _context.Tickets.FirstOrDefaultAsync(
                ticket => ticket.Number == number,
                cancellationToken
            );
        }

        public async Task<
            IReadOnlyCollection<Ticket>
        > ListAssignedToTechnicianAsync(
            Guid technicianId,
            CancellationToken cancellationToken
        )
        {
            return await _context
                .Tickets.AsNoTracking()
                .Where(ticket =>
                    ticket.Assignments.Any(assignment =>
                        assignment.TechnicianId == technicianId
                        && assignment.FinishedAt == null
                    )
                )
                .OrderByDescending(ticket => ticket.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<Ticket>> ListCreatedByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken
        )
        {
            return await _context
                .Tickets.AsNoTracking()
                .Where(ticket => ticket.CreatedById == userId)
                .OrderByDescending(ticket => ticket.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyCollection<Ticket>> GetActiveByQueueAsync(
            Guid queueId,
            CancellationToken cancellationToken
        )
        {
            var tickets = await _context
                .Tickets.Include(ticket => ticket.Assignments)
                    .ThenInclude(assignment => assignment.Technician)
                .Where(ticket =>
                    ticket.QueueId == queueId
                    && (
                        ticket.Status == TicketStatus.Assigned
                        || ticket.Status == TicketStatus.InProgress
                        || ticket.Status == TicketStatus.WaitingCustomer
                    )
                )
                .ToListAsync(cancellationToken);

            return tickets;
        }

        public async Task<
            Dictionary<Guid, int>
        > GetActiveTicketCountsByTechniciansAsync(
            Guid queueId,
            CancellationToken cancellationToken
        )
        {
            var activeStatuses = new[]
            {
                TicketStatus.Assigned,
                TicketStatus.InProgress,
                TicketStatus.WaitingCustomer,
            };

            return await _context
                .Tickets.Where(ticket =>
                    ticket.QueueId == queueId
                    && activeStatuses.Contains(ticket.Status)
                )
                .SelectMany(ticket => ticket.Assignments)
                .Where(assignment => assignment.FinishedAt == null)
                .GroupBy(assignment => assignment.TechnicianId)
                .Select(g => new { TechnicianId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(
                    x => x.TechnicianId,
                    x => x.Count,
                    cancellationToken
                );
        }

        public async Task<Ticket?> GetDetailsByIdAsync(
            Guid ticketId,
            CancellationToken cancellationToken
        )
        {
            return await _context
                .Tickets.AsNoTracking()
                .Include(ticket => ticket.Department)
                .Include(ticket => ticket.Queue)
                .Include(ticket => ticket.CreatedBy)
                .Include(ticket => ticket.Assignments)
                    .ThenInclude(assignment => assignment.Technician)
                .FirstOrDefaultAsync(
                    ticket => ticket.Id == ticketId,
                    cancellationToken
                );
        }

        // ----------- Paged Functions Block -----------
        public async Task<IReadOnlyCollection<Ticket>> ListAsync(
            TicketListFilter filter,
            TicketSortField sortBy,
            SortDirection sortDirection,
            int skip,
            int take,
            CancellationToken cancellationToken
        )
        {
            IQueryable<Ticket> query = _context.Tickets.AsNoTracking();

            if (filter.Status.HasValue)
            {
                query = query.Where(ticket =>
                    ticket.Status == filter.Status.Value
                );
            }

            if (filter.Priority.HasValue)
            {
                query = query.Where(ticket =>
                    ticket.Priority == filter.Priority.Value
                );
            }

            if (filter.Category.HasValue)
            {
                query = query.Where(ticket =>
                    ticket.Category == filter.Category.Value
                );
            }

            if (filter.DepartmentId.HasValue)
            {
                query = query.Where(ticket =>
                    ticket.DepartmentId == filter.DepartmentId.Value
                );
            }

            if (filter.QueueId.HasValue)
            {
                query = query.Where(ticket =>
                    ticket.QueueId == filter.QueueId.Value
                );
            }

            if (filter.TechnicianId.HasValue)
            {
                query = query.Where(ticket =>
                    ticket.Assignments.Any(assigned =>
                        assigned.TechnicianId == filter.TechnicianId.Value
                        && assigned.FinishedAt == null
                    )
                );
            }

            query = ApplyOrdering(query, sortBy, sortDirection);

            return await query
                .Include(ticket => ticket.Department)
                .Include(ticket => ticket.Queue)
                .Include(ticket => ticket.Assignments)
                    .ThenInclude(assignment => assignment.Technician)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> CountAsync(
            TicketListFilter filter,
            CancellationToken cancellationToken
        )
        {
            IQueryable<Ticket> query = _context.Tickets.AsNoTracking();

            if (filter.Status.HasValue)
            {
                query = query.Where(ticket =>
                    ticket.Status == filter.Status.Value
                );
            }

            if (filter.Priority.HasValue)
            {
                query = query.Where(ticket =>
                    ticket.Priority == filter.Priority.Value
                );
            }

            if (filter.Category.HasValue)
            {
                query = query.Where(ticket =>
                    ticket.Category == filter.Category.Value
                );
            }

            if (filter.DepartmentId.HasValue)
            {
                query = query.Where(ticket =>
                    ticket.DepartmentId == filter.DepartmentId.Value
                );
            }

            if (filter.QueueId.HasValue)
            {
                query = query.Where(ticket =>
                    ticket.QueueId == filter.QueueId.Value
                );
            }

            if (filter.TechnicianId.HasValue)
            {
                query = query.Where(ticket =>
                    ticket.Assignments.Any(assigned =>
                        assigned.TechnicianId == filter.TechnicianId.Value
                        && assigned.FinishedAt == null
                    )
                );
            }
            return await query.CountAsync(cancellationToken);
        }

        // ----------- End Paged Functions Block -----------

        private static IQueryable<Ticket> ApplyOrdering(
            IQueryable<Ticket> query,
            TicketSortField sortBy,
            SortDirection sortDirection
        )
        {
            return sortBy switch
            {
                TicketSortField.Number => sortDirection
                == SortDirection.Ascending
                    ? query
                        .OrderBy(ticket => ticket.Number)
                        .ThenByDescending(ticket => ticket.CreatedAt)
                    : query
                        .OrderByDescending(ticket => ticket.Number)
                        .ThenByDescending(ticket => ticket.CreatedAt),

                TicketSortField.Priority => sortDirection
                == SortDirection.Ascending
                    ? query
                        .OrderBy(ticket => ticket.Priority)
                        .ThenByDescending(ticket => ticket.Number)
                    : query
                        .OrderByDescending(ticket => ticket.Priority)
                        .ThenByDescending(ticket => ticket.Number),

                _ => sortDirection == SortDirection.Ascending
                    ? query
                        .OrderBy(ticket => ticket.CreatedAt)
                        .ThenBy(ticket => ticket.Number)
                    : query
                        .OrderByDescending(ticket => ticket.CreatedAt)
                        .ThenByDescending(ticket => ticket.Number),
            };
        }
    }
}
