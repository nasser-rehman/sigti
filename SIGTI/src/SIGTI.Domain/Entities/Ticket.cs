using SIGTI.Domain.Common;
using SIGTI.Domain.Enums;
using SIGTI.Domain.Exceptions;

namespace SIGTI.Domain.Entities
{
    public sealed class Ticket : BaseEntity
    {
        private const int MaxTitleLength = 200;
        private const int MaxDescriptionLength = 5000;

        public int Number { get; private set; }

        public string Code => $"SIG-{Number:D6}";

        public string Title { get; private set; } = null!;

        public string Description { get; private set; } = null!;

        public TicketStatus Status { get; private set; }

        public TicketPriority Priority { get; private set; }

        public TicketCategory Category { get; private set; }

        public Guid DepartmentId { get; private set; }

        public string DepartmentName { get; private set; } = null!;

        public Guid CreatedById { get; private set; }

        public Guid QueueId { get; private set; }

        public DateTime? FirstResponseAt { get; private set; }

        public DateTime? ResolvedAt { get; private set; }

        public DateTime? ClosedAt { get; private set; }

        public Department Department { get; private set; } = null!;

        public User CreatedBy { get; private set; } = null!;

        public SupportQueue Queue { get; private set; } = null!;

        private readonly List<Comment> _comments = [];
        public IReadOnlyCollection<Comment> Comments => _comments;

        private readonly List<TicketAssignment> _assignments = [];
        public IReadOnlyCollection<TicketAssignment> Assignments =>
            _assignments;

        public TicketAssignment? CurrentAssignment =>
            _assignments.FirstOrDefault(x => x.FinishedAt == null);

        private Ticket() { }

        public Ticket(
            int number,
            string title,
            string description,
            TicketPriority priority,
            TicketCategory category,
            Department department,
            User createdBy,
            SupportQueue queue
        )
        {
            SetNumber(number);

            UpdateTitle(title);

            UpdateDescription(description);

            ChangePriority(priority);

            ChangeCategory(category);

            ChangeDepartment(department);

            SetCreatedBy(createdBy);

            SetQueue(queue);

            Status = TicketStatus.New;
        }

        internal void SetNumber(int number)
        {
            if (number <= 0)
                throw new DomainException(
                    "O número do ticket não pode ser negativo ou igual a zero."
                );

            Number = number;
            UpdateTimestamp();
        }

        public void UpdateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("O título do ticket é obrigatório.");

            title = title.Trim();
            if (title.Length > MaxTitleLength)
                throw new DomainException(
                    $"O título do ticket não pode ter mais de {MaxTitleLength} caracteres."
                );
            Title = title;
            UpdateTimestamp();
        }

        public void UpdateDescription(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new DomainException(
                    "A descrição do ticket é obrigatória."
                );

            description = description.Trim();
            if (description.Length > MaxDescriptionLength)
                throw new DomainException(
                    $"A descrição do ticket não pode ter mais de {MaxDescriptionLength} caracteres."
                );
            Description = description;
            UpdateTimestamp();
        }

        public void SendToQueue()
        {
            if (Status != TicketStatus.New)
                throw new DomainException(
                    "O ticket deve estar em status 'Novo' para ser enviado para a fila."
                );
            Status = TicketStatus.WaitingQueue;
            UpdateTimestamp();
        }

        public void StartService()
        {
            if (Status != TicketStatus.Assigned)
                throw new DomainException(
                    "O ticket deve estar atribuído a um técnico para iniciar o atendimento."
                );

            Status = TicketStatus.InProgress;
            FirstResponseAt = DateTime.UtcNow;
            UpdateTimestamp();
        }

        public void WaitCustomer()
        {
            if (Status != TicketStatus.InProgress)
                throw new DomainException(
                    "O ticket deve estar em andamento para aguardar o cliente."
                );
            Status = TicketStatus.WaitingCustomer;
            UpdateTimestamp();
        }

        public void Resolve()
        {
            if (
                Status != TicketStatus.InProgress
                && Status != TicketStatus.WaitingCustomer
            )
                throw new DomainException(
                    "O ticket deve estar em andamento ou aguardando o cliente para ser resolvido."
                );
            Status = TicketStatus.Resolved;
            ResolvedAt = DateTime.UtcNow;
            UpdateTimestamp();
        }

        public void Close()
        {
            if (Status != TicketStatus.Resolved)
                throw new DomainException(
                    "Somente tickets resolvidos podem ser fechados."
                );

            Status = TicketStatus.Closed;
            ClosedAt = DateTime.UtcNow;
            UpdateTimestamp();
        }

        public void ChangePriority(TicketPriority priority)
        {
            if (!Enum.IsDefined(typeof(TicketPriority), priority))
                throw new DomainException("Prioridade do ticket inválida.");
            Priority = priority;
            UpdateTimestamp();
        }

        public void ChangeCategory(TicketCategory category)
        {
            if (!Enum.IsDefined(typeof(TicketCategory), category))
                throw new DomainException("Categoria do ticket inválida.");
            Category = category;
            UpdateTimestamp();
        }

        internal void ChangeDepartment(Department department)
        {
            if (department is null)
                throw new DomainException("O departamento é obrigatório.");
            if (!department.IsActive)
                throw new DomainException("O departamento deve estar ativo.");

            Department = department;
            DepartmentId = department.Id;
            DepartmentName = department.Name;
            UpdateTimestamp();
        }

        internal void SetCreatedBy(User user)
        {
            if (user is null)
                throw new DomainException("O usuário é obrigatório.");

            CreatedBy = user;
            CreatedById = user.Id;
            UpdateTimestamp();
        }

        internal void SetQueue(SupportQueue queue)
        {
            if (queue is null)
                throw new DomainException(
                    "A fila de suporte do ticket é obrigatória."
                );
            if (!queue.IsActive)
                throw new DomainException(
                    "A fila de suporte do ticket deve estar ativa."
                );
            Queue = queue;
            QueueId = queue.Id;
            UpdateTimestamp();
        }

        public void TransferToQueue(
            SupportQueue targetQueue,
            User newTechnician,
            User transferredBy,
            string reason
        )
        {
            if (
                Status == TicketStatus.Closed
                || Status == TicketStatus.Resolved
            )
                throw new DomainException(
                    "Não é possível transferir tickets resolvidos ou fechados."
                );

            if (newTechnician is null)
                throw new DomainException("O técnico é obrigatório.");

            if (transferredBy is null)
                throw new DomainException(
                    "O usuário responsável pela transferência é obrigatório."
                );

            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException(
                    "O motivo da transferência é obrigatório."
                );

            SetQueue(targetQueue);
            AssignTechnician(newTechnician, transferredBy, reason);
        }

        public void AddComment(Comment comment)
        {
            if (Status == TicketStatus.Closed)
                throw new DomainException(
                    "Não é possível efetuar comentários em tickets fechados."
                );
            if (comment is null)
                throw new DomainException("O comentário é obrigatório.");
            _comments.Add(comment);
            UpdateTimestamp();
        }

        public void AssignTechnician(
            User technician,
            User assignedBy,
            string reason
        )
        {
            if (technician is null)
                throw new DomainException("O técnico é obrigatório.");
            if (assignedBy is null)
                throw new DomainException(
                    "O usuário que atribuiu o técnico é obrigatório."
                );
            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException(
                    "A razão da atribuição é obrigatória."
                );
            if (Status == TicketStatus.Closed)
                throw new DomainException(
                    "Não é possível atribuir técnicos a tickets fechados."
                );

            CurrentAssignment?.MarkAsFinished();

            var assignment = new TicketAssignment(
                this,
                technician,
                assignedBy,
                reason
            );

            _assignments.Add(assignment);

            Status = TicketStatus.Assigned;

            UpdateTimestamp();
        }
    }
}
