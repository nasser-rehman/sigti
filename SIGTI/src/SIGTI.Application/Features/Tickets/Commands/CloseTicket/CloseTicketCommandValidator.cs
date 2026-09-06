using FluentValidation;

namespace SIGTI.Application.Features.Tickets.Commands.CloseTicket
{
    public class CloseTicketCommandValidator
        : AbstractValidator<CloseTicketCommand>
    {
        public CloseTicketCommandValidator()
        {
            RuleFor(x => x.TicketId)
                .NotEmpty()
                .WithMessage("O identificador do ticket é obrigatório.");
        }
    }
}
