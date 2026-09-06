using MediatR;
using SIGTI.Application.Common.Interfaces.Services;

namespace SIGTI.Application.Features.Tickets.Queries.ListTicketComments
{
    public class ListTicketCommentsQueryHandler
        : IRequestHandler<
            ListTicketCommentsQuery,
            IReadOnlyList<TicketCommentResponse>
        >
    {
        private readonly IEntityReferenceService _entityReferenceService;

        public ListTicketCommentsQueryHandler(
            IEntityReferenceService entityReferenceService
        )
        {
            _entityReferenceService = entityReferenceService;
        }

        public async Task<IReadOnlyList<TicketCommentResponse>> Handle(
            ListTicketCommentsQuery request,
            CancellationToken cancellationToken
        )
        {
            var ticket = await _entityReferenceService.GetRequiredTicketAsync(
                request.TicketId,
                cancellationToken
            );

            return ticket
                .Comments.OrderBy(comment => comment.CreatedAt)
                .Select(comment => new TicketCommentResponse(
                    comment.Id,
                    comment.TicketId,
                    comment.AuthorId,
                    comment.Author.Name,
                    comment.Content,
                    comment.CreatedAt,
                    comment.UpdatedAt
                ))
                .ToList()
                .AsReadOnly();
        }
    }
}
