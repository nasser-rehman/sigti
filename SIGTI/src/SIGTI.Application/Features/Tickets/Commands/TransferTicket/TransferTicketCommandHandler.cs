using MediatR;
using SIGTI.Application.Common.Enums;
using SIGTI.Application.Common.Interfaces.Persistence;
using SIGTI.Application.Common.Interfaces.Services;
using SIGTI.Application.Features.Tickets.Queries.ListTickets;
using SIGTI.Domain.Entities;
using SIGTI.Domain.Enums;
using SIGTI.Domain.Exceptions;
using SIGTI.Domain.Interfaces.Services;

namespace SIGTI.Application.Features.Tickets.Commands.TransferTicket
{
    public class TransferTicketCommandHandler
        : IRequestHandler<TransferTicketCommand, TransferTicketResponse>
    {
        private readonly IEntityReferenceService _entityReferenceService;
        private readonly ITicketRepository _ticketRepository;
        private readonly ITicketAssignmentStrategy _assignmentStrategy;
        private readonly IUnitOfWork _unitOfWork;

        public TransferTicketCommandHandler(
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

        public async Task<TransferTicketResponse> Handle(
            TransferTicketCommand request,
            CancellationToken cancellationToken
        )
        {
            var ticket = await _entityReferenceService.GetRequiredTicketAsync(
                request.TicketId,
                cancellationToken
            );
            var transferredBy =
                await _entityReferenceService.GetRequiredUserAsync(
                    request.TransferredById,
                    cancellationToken
                );

            // 1. Resolve a fila de destino (ou mantém a fila atual caso a
            // transferência seja só de técnico)
            var targetQueueId = request.TargetQueueId ?? ticket.QueueId;
            var targetQueue =
                await _entityReferenceService.GetRequiredQueueAsync(
                    targetQueueId,
                    cancellationToken
                );

            // 2. Resolve o técnico: manual se informado, ou automático
            User technician;
            if (request.TargetTechnicianId.HasValue)
            {
                technician = await _entityReferenceService.GetRequiredUserAsync(
                    request.TargetTechnicianId.Value,
                    cancellationToken
                );
            }
            else
            {
                var activeWorkloads =
                    await _ticketRepository.GetActiveTicketCountsByTechniciansAsync(
                        targetQueue.Id,
                        cancellationToken
                    );

                var selectedMember = _assignmentStrategy.SelectTechnician(
                    targetQueue,
                    activeWorkloads
                );

                if (selectedMember is null)
                {
                    throw new DomainException(
                        "Não há técnicos disponíveis na  fila de destino para realizar o atendimento."
                    );
                }

                technician =
                    selectedMember.Technician
                    ?? await _entityReferenceService.GetRequiredUserAsync(
                        selectedMember.TechnicianId,
                        cancellationToken
                    );
            }

            ticket.TransferToQueue(
                targetQueue,
                technician,
                transferredBy,
                request.Reason
            );

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var activeAssignment =
                ticket.CurrentAssignment
                ?? throw new InvalidOperationException(
                    "O ticket transferido deve possuir uma atribuição ativa."
                );

            return new TransferTicketResponse(
                ticket.Id,
                ticket.Number,
                ticket.Code,
                ticket.QueueId,
                targetQueue.Name,
                technician.Id,
                technician.Name,
                request.Reason,
                ticket.Status.ToString(),
                activeAssignment.AssignedAt
            );
        }
    }
}
