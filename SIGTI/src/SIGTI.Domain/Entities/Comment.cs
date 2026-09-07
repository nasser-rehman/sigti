using SIGTI.Domain.Common;
using SIGTI.Domain.Exceptions;

namespace SIGTI.Domain.Entities
{
    public sealed class Comment : BaseEntity
    {
        private const int MinContentLength = 1;
        private const int MaxContentLength = 5000;
        public string Content { get; private set; }
        public Guid TicketId { get; private set; }
        public Ticket Ticket { get; private set; } = null!;
        public Guid AuthorId { get; private set; }
        public User Author { get; private set; } = null!;

        private Comment() { } // For EF Core

        public Comment(string content, Ticket ticket, User author)
        {
            if (ticket is null)
                throw new DomainException("O ticket é obrigatório.");

            if (author is null)
                throw new DomainException(
                    "O autor do comentário é obrigatório."
                );

            UpdateContent(content);

            Ticket = ticket;
            TicketId = ticket.Id;

            Author = author;
            AuthorId = author.Id;
        }

        public void UpdateContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new DomainException(
                    "O conteúdo do comentário é obrigatório."
                );

            content = content.Trim();
            if (
                content.Length < MinContentLength
                || content.Length > MaxContentLength
            )
                throw new DomainException(
                    $"O conteúdo do comentário deve ter entre {MinContentLength} e {MaxContentLength} caracteres."
                );

            Content = content;
            UpdateTimestamp();
        }
    }
}
