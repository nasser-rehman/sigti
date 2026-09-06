using System.ComponentModel;
using MediatR;
using SIGTI.Application.Common.Interfaces.Persistence;
using SIGTI.Application.Common.Interfaces.Services;

namespace SIGTI.Application.Features.Tickets.Commands.ResolveTicket
{
    public class ResolveTicketCommandHandler
        : IRequestHandler<ResolveTicketCommand, ResolveTicketResponse>
    {
        private readonly IEntityReferenceService _entityReferenceService;
        private readonly IUnitOfWork _unitOfWork;

        public ResolveTicketCommandHandler(
            IEntityReferenceService entityReferenceService,
            IUnitOfWork unitOfWork
        )
        {
            _entityReferenceService = entityReferenceService;
            _unitOfWork = unitOfWork;
        }

        public async Task<ResolveTicketResponse> Handle(
            ResolveTicketCommand request,
            CancellationToken cancellationToken
        )
        {
            var ticket = await _entityReferenceService.GetRequiredTicketAsync(
                request.TicketId,
                cancellationToken
            );

            ticket.Resolve();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ResolveTicketResponse(
                ticket.Id,
                ticket.Status,
                ticket.ResolvedAt,
                ticket.UpdatedAt
            );
        }
    }
}
