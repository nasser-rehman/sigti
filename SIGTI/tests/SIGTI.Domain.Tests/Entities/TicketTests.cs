using FluentAssertions;
using SIGTI.Domain.Entities;
using SIGTI.Domain.Enums;
using SIGTI.Domain.Exceptions;
using SIGTI.Domain.Tests.Builders;
using SIGTI.Domain.ValueObjects;
using Xunit;

namespace SIGTI.Domain.Tests.Entities;

public class TicketTests
{
    [Fact]
    public void Should_Create_A_New_Ticket()
    {
        var ticket = new TicketBuilder().WithNumber(1).Build();

        ticket.Number.Should().Be(1);
        ticket.Title.Should().Be("Erro no computador");
        ticket.Description.Should().Be("Tela azul ao iniciar.");
        ticket.Priority.Should().Be(TicketPriority.Medium);
        ticket.Category.Should().Be(TicketCategory.Hardware);
        ticket.Department.Name.Should().Be("Departamento de TI");
        ticket.Status.Should().Be(TicketStatus.New);
        ticket.DepartmentId.Should().NotBeEmpty();
        ticket.CreatedById.Should().NotBeEmpty();
        ticket.QueueId.Should().NotBeEmpty();
    }

    [Fact]
    public void Should_Not_Close_A_New_Ticket()
    {
        // Arrange
        var ticket = new TicketBuilder().Build();

        // Act
        Action action = () => ticket.Close();

        // Assert
        action
            .Should()
            .Throw<DomainException>()
            .WithMessage("Somente tickets resolvidos podem ser fechados.");
    }

    [Fact]
    public void Should_Close_A_Resolved_Ticket()
    {
        // Arrange
        var ticket = new TicketBuilder().BuildAsResolved();
        // Act
        ticket.Close();
        // Assert
        ticket.Status.Should().Be(TicketStatus.Closed);
    }

    [Fact]
    public void Should_Not_Resolve_A_Closed_Ticket()
    {
        // Arrange
        var ticket = new TicketBuilder().BuildAsClosed();

        // Act
        Action action = () => ticket.Resolve();

        // Assert
        action
            .Should()
            .Throw<DomainException>()
            .WithMessage(
                "O ticket deve estar em andamento ou aguardando o cliente para ser resolvido."
            );
    }

    [Fact]
    public void Should_Not_Start_Service_A_Closed_Ticket()
    {
        // Arrange
        var ticket = new TicketBuilder().BuildAsClosed();

        // Act
        Action action = () => ticket.StartService();

        // Assert
        action
            .Should()
            .Throw<DomainException>()
            .WithMessage(
                "O ticket deve estar atribuído a um técnico para iniciar o atendimento."
            );
    }

    [Fact]
    public void Should_Not_Assign_Technician_To_A_Closed_Ticket()
    {
        // Arrange
        var ticket = new TicketBuilder().BuildAsClosed();
        var technician = new UserBuilder().Build();
        var assignedBy = new UserBuilder().Build();

        // Act
        Action action = () =>
            ticket.AssignTechnician(
                technician,
                assignedBy,
                "Atribuição inicial ao N1"
            );

        // Assert
        action
            .Should()
            .Throw<DomainException>()
            .WithMessage(
                "Não é possível atribuir técnicos a tickets fechados."
            );
    }

    [Fact]
    public void Should_Set_FirstResponse_When_Service_Start()
    {
        var ticket = new TicketBuilder().Build();
        var technician = new UserBuilder().WithRole(Role.Technician).Build();
        var assignedBy = new UserBuilder().WithRole(Role.Administrator).Build();
        ticket.SendToQueue();
        ticket.AssignTechnician(
            technician,
            assignedBy,
            "Atribuição inicial ao N1"
        );
        ticket.StartService();

        ticket.FirstResponseAt.Should().NotBeNull();
    }

    [Fact]
    public void Should_Not_Start_Service_Without_Assignment()
    {
        var ticket = new TicketBuilder().Build();

        Action action = () => ticket.StartService();

        action
            .Should()
            .Throw<DomainException>()
            .WithMessage(
                "O ticket deve estar atribuído a um técnico para iniciar o atendimento."
            );
    }

    [Fact]
    public void Should_Not_Assign_Closed_Ticket()
    {
        var ticket = new TicketBuilder().BuildAsClosed();
        var technician = new UserBuilder().WithRole(Role.Technician).Build();
        var assignedBy = new UserBuilder().WithRole(Role.Administrator).Build();

        Action action = () =>
            ticket.AssignTechnician(
                technician,
                assignedBy,
                "Atribuição inicial ao N1"
            );

        action
            .Should()
            .Throw<DomainException>()
            .WithMessage(
                "Não é possível atribuir técnicos a tickets fechados."
            );
    }

    [Fact]
    public void Should_Add_Comment()
    {
        var ticket = new TicketBuilder().Build();
        var user = new UserBuilder().Build();
        var comment = new Comment("Olá isso é um comentário", ticket, user);

        ticket.AddComment(comment);

        ticket.Comments.Should().Contain(comment);
    }

    [Fact]
    public void Should_Not_Add_Comment_To_Closed_Ticket()
    {
        var ticket = new TicketBuilder().BuildAsClosed();
        var user = new UserBuilder().Build();
        var comment = new Comment("Olá isso é um comentário", ticket, user);

        Action action = () => ticket.AddComment(comment);

        action
            .Should()
            .Throw<DomainException>()
            .WithMessage(
                "Não é possível efetuar comentários em tickets fechados."
            );
    }

    [Fact]
    public void Should_Transfer_Queue()
    {
        var queue = new SupportQueueBuilder().Build();
        var ticket = new TicketBuilder().WithQueue(queue).Build();

        ticket.QueueId.Should().Be(queue.Id);
    }

    [Fact]
    public void Should_Not_Resolve_Without_Service()
    {
        var ticket = new TicketBuilder().Build();

        Action action = () => ticket.Resolve();

        action
            .Should()
            .Throw<DomainException>()
            .WithMessage(
                "O ticket deve estar em andamento ou aguardando o cliente para ser resolvido."
            );
    }

    [Fact]
    public void Should_Not_Create_Ticket_Without_Queue()
    {
        Action action = () => new TicketBuilder().BuildWithoutQueue();

        action
            .Should()
            .Throw<DomainException>()
            .WithMessage("A fila de suporte do ticket é obrigatória.");
    }

    [Fact]
    public void Should_Not_Resolve_New_Ticket()
    {
        var ticket = new TicketBuilder().Build();
        Action action = () => ticket.Resolve();

        action
            .Should()
            .Throw<DomainException>()
            .WithMessage(
                "O ticket deve estar em andamento ou aguardando o cliente para ser resolvido."
            );
    }
}
