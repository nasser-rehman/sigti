using MediatR;

namespace SIGTI.Application.Features.Tickets.Commands.AddComment
{
    public sealed record AddCommentCommand(
        Guid TicketId,
        Guid CreatedById,
        string Content
    ) : IRequest<AddCommentResponse>;
}
