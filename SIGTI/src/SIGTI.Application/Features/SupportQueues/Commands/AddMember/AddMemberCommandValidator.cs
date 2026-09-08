using FluentValidation;

namespace SIGTI.Application.Features.SupportQueues.Commands.AddMember
{
    public sealed class AddMemberCommandValidator
        : AbstractValidator<AddMemberCommand>
    {
        public AddMemberCommandValidator()
        {
            RuleFor(x => x.QueueId)
                .NotEmpty()
                .WithMessage(
                    "O identificador da fila de suporte deve ser informado."
                );

            RuleFor(x => x.TechnicianId)
                .NotEmpty()
                .WithMessage("O identificador do técnico deve ser informado.");

            RuleFor(x => x.MaxConcurrentTickets)
                .NotEmpty()
                .WithMessage(
                    "A quantidade de tickets em paralelo do técnico deve ser informado."
                )
                .GreaterThan(0)
                .WithMessage(
                    "A quantidade de tickets em paralelo do técnico deve ser maior que zero."
                );
        }
    }
}
