using FluentValidation;

namespace SIGTI.Application.Features.Tickets.Commands.ResolveTicket
{
    public class ResolveTicketCommandValidator
        : AbstractValidator<ResolveTicketCommand>
    {
        public ResolveTicketCommandValidator()
        {
            RuleFor(x => x.TicketId)
                .NotEmpty()
                .WithMessage("O identificador do ticket é obrigatório.");
        }
    }
}
