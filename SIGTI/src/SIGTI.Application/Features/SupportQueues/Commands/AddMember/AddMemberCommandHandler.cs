using MediatR;
using SIGTI.Application.Common.Interfaces.Persistence;
using SIGTI.Application.Common.Interfaces.Services;

namespace SIGTI.Application.Features.SupportQueues.Commands.AddMember
{
    public sealed class AddMemberCommandHandler
        : IRequestHandler<AddMemberCommand, AddMemberResponse>
    {
        private readonly IEntityReferenceService _entityReferenceService;
        private readonly IUnitOfWork _unitOfWork;

        public AddMemberCommandHandler(
            IEntityReferenceService entityReferenceService,
            IUnitOfWork unitOfWork
        )
        {
            _entityReferenceService = entityReferenceService;
            _unitOfWork = unitOfWork;
        }

        public async Task<AddMemberResponse> Handle(
            AddMemberCommand request,
            CancellationToken cancellationToken
        )
        {
            var queue = await _entityReferenceService.GetRequiredQueueAsync(
                request.QueueId,
                cancellationToken
            );

            var technician = await _entityReferenceService.GetRequiredUserAsync(
                request.TechnicianId,
                cancellationToken
            );

            queue.AddMember(technician, request.MaxConcurrentTickets);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AddMemberResponse { Id = queue.Id };
        }
    }
}
