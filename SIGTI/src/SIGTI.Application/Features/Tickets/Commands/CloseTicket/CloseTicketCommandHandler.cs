using MediatR;
using SIGTI.Application.Common.Interfaces.Persistence;
using SIGTI.Application.Common.Interfaces.Services;

namespace SIGTI.Application.Features.Tickets.Commands.CloseTicket
{
    public class CloseTicketCommandHandler
        : IRequestHandler<CloseTicketCommand, CloseTicketResponse>
    {
        private readonly IEntityReferenceService _entityReferenceService;
        private readonly IUnitOfWork _unitOfWork;

        public CloseTicketCommandHandler(
            IEntityReferenceService entityReferenceService,
            IUnitOfWork unitOfWork
        )
        {
            _entityReferenceService = entityReferenceService;
            _unitOfWork = unitOfWork;
        }

        public async Task<CloseTicketResponse> Handle(
            CloseTicketCommand request,
            CancellationToken cancellationToken
        )
        {
            var ticket = await _entityReferenceService.GetRequiredTicketAsync(
                request.TicketId,
                cancellationToken
            );

            ticket.Close();

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CloseTicketResponse(
                ticket.Id,
                ticket.Status,
                ticket.ClosedAt,
                ticket.UpdatedAt
            );
        }
    }
}
