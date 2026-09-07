using MediatR;
using SIGTI.Application.Common.Enums;
using SIGTI.Application.Common.Interfaces.Persistence;
using SIGTI.Application.Common.Interfaces.Services;
using SIGTI.Application.Features.Tickets.Queries.ListTickets;
using SIGTI.Domain.Entities;
using SIGTI.Domain.Enums;
using SIGTI.Domain.Interfaces.Services;

namespace SIGTI.Application.Features.Tickets.Commands.DispatchTicket
{
    public sealed class DispatchTicketHandler
        : IRequestHandler<DispatchTicketCommand, DispatchTicketResponse>
    {
        private readonly IEntityReferenceService _entityReferenceService;
        private readonly ITicketRepository _ticketRepository;
        private readonly ITicketAssignmentStrategy _assignmentStrategy;
        private readonly IUnitOfWork _unitOfWork;

        public DispatchTicketHandler(
            IEntityReferenceService entityReferenceService,
            ITicketRepository ticketRepository,
            ITicketAssignmentStrategy assignmentStrategy,
            IUnitOfWork unitOfWork
        )
        {
            _entityReferenceService = entityReferenceService;
            _ticketRepository = ticketRepository;
            _assignmentStrategy = assignmentStrategy;
            _unitOfWork = unitOfWork;
        }

        public async Task<DispatchTicketResponse> Handle(
            DispatchTicketCommand request,
            CancellationToken cancellationToken
        )
        {
            // Search for required entities
            var ticket = await _entityReferenceService.GetRequiredTicketAsync(
                request.TicketId,
                cancellationToken
            );
            var assignedBy = await _entityReferenceService.GetRequiredUserAsync(
                request.AssignedById,
                cancellationToken
            );

            User technician;
            string reason = !string.IsNullOrWhiteSpace(request.Reason)
                ? request.Reason
                : (
                    request.TechnicianId.HasValue
                        ? "Atribuição Manual de técnico"
                        : "Atribuição automática pela fila de atendimento"
                );

            if (request.TechnicianId.HasValue)
            {
                technician = await _entityReferenceService.GetRequiredUserAsync(
                    request.TechnicianId.Value,
                    cancellationToken
                );
            }
            else
            {
                var queue = await _entityReferenceService.GetRequiredQueueAsync(
                    ticket.QueueId,
                    cancellationToken
                );

                var activeWorkloads =
                    await _ticketRepository.GetActiveTicketCountsByTechniciansAsync(
                        queue.Id,
                        cancellationToken
                    );

                var selectedMember = _assignmentStrategy.SelectTechnician(
                    queue,
                    activeWorkloads
                );

                technician =
                    selectedMember.Technician
                    ?? await _entityReferenceService.GetRequiredUserAsync(
                        selectedMember.TechnicianId,
                        cancellationToken
                    );
            }

            ticket.AssignTechnician(technician, assignedBy, reason);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var activeAssignment = ticket.Assignments.Last(a => !a.IsFinished);

            return new DispatchTicketResponse(
                ticket.Id,
                technician.Id,
                technician.Name,
                ticket.Status,
                activeAssignment.AssignedAt
            );
        }
    }
}
