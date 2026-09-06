using FluentValidation;

namespace SIGTI.Application.Features.Tickets.Queries.ListTicketComments
{
    public class ListTicketCommentsQueryValidator
        : AbstractValidator<ListTicketCommentsQuery>
    {
        public ListTicketCommentsQueryValidator()
        {
            RuleFor(x => x.TicketId)
                .NotEmpty()
                .WithMessage("O identificador do ticket é obrigatório.");
        }
    }
}
