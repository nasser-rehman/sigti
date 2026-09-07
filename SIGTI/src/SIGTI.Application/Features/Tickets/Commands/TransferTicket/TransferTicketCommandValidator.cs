using FluentValidation;

namespace SIGTI.Application.Features.Tickets.Commands.TransferTicket
{
    public class TransferTicketCommandValidator
        : AbstractValidator<TransferTicketCommand>
    {
        public TransferTicketCommandValidator()
        {
            RuleFor(x => x.TicketId)
                .NotEmpty()
                .WithMessage("O identificador do ticket é obrigatório.");

            RuleFor(x => x.TransferredById)
                .NotEmpty()
                .WithMessage(
                    "O identificador do usuário responsável pela transferência é obrigatório."
                );

            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage("O motivo da transferência é obrigatório.")
                .MinimumLength(3)
                .WithMessage("O motivo deve conter no mínimo 3 caracteres.")
                .MaximumLength(500)
                .WithMessage("O motivo não pode exceder 500 caracteres.");

            RuleFor(x => x)
                .Must(x =>
                    x.TargetQueueId.HasValue || x.TargetTechnicianId.HasValue
                )
                .WithMessage(
                    "É necessário informar uma nova fila ou um novo técnico."
                );
        }
    }
}
