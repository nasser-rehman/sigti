using FluentValidation;

namespace SIGTI.Application.Features.Tickets.Commands.AddComment
{
    public class AddCommentCommandValidator
        : AbstractValidator<AddCommentCommand>
    {
        public AddCommentCommandValidator()
        {
            RuleFor(x => x.TicketId)
                .NotEmpty()
                .WithMessage("O identificador do ticket é obrigatório.");

            RuleFor(x => x.CreatedById)
                .NotEmpty()
                .WithMessage("O identificador do autor é obrigatório.");

            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("O conteúdo do comentário é obrigatório.")
                .Length(1, 5000)
                .WithMessage(
                    "O conteúdo do comentário deve ser entre 1 e 5000 caracteres."
                );
        }
    }
}
