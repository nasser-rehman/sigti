using MediatR;
using SIGTI.Application.Common.Interfaces.Persistence;
using SIGTI.Application.Common.Interfaces.Services;
using SIGTI.Domain.Entities;

namespace SIGTI.Application.Features.Tickets.Commands.AddComment
{
    public class AddCommentCommandHandler
        : IRequestHandler<AddCommentCommand, AddCommentResponse>
    {
        private readonly IEntityReferenceService _entityReferenceService;
        private readonly IUnitOfWork _unitOfWork;

        public AddCommentCommandHandler(
            IEntityReferenceService entityReferenceService,
            IUnitOfWork unitOfWork
        )
        {
            _entityReferenceService = entityReferenceService;
            _unitOfWork = unitOfWork;
        }

        public async Task<AddCommentResponse> Handle(
            AddCommentCommand request,
            CancellationToken cancellationToken
        )
        {
            var ticket = await _entityReferenceService.GetRequiredTicketAsync(
                request.TicketId,
                cancellationToken
            );
            var createdBy = await _entityReferenceService.GetRequiredUserAsync(
                request.AuthorId,
                cancellationToken
            );

            var comment = new Comment(request.Content, ticket, createdBy);

            ticket.AddComment(comment);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AddCommentResponse(
                comment.Id,
                ticket.Id,
                createdBy.Id,
                createdBy.Name,
                comment.Content,
                comment.CreatedAt
            );
        }
    }
}
