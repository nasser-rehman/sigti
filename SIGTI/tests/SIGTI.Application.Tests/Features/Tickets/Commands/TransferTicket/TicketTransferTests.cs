using FluentAssertions;
using SIGTI.Domain.Entities;
using SIGTI.Domain.Enums;
using SIGTI.Domain.Exceptions;
using SIGTI.Domain.Tests.Builders;
using Xunit;

namespace SIGTI.Application.Tests.Features.Tickets.Commands.TransferTicket
{
    public class TicketTransferTests
    {
        [Fact]
        public void Transfer_WhenValid_ShouldFinalizePreviousAssignmentAndCreateNewOne()
        {
            // Arrange
            var department = new DepartmentBuilder().Build();
            var queue1 = new SupportQueueBuilder().WithName("Nível 1").Build();
            var queue2 = new SupportQueueBuilder().WithName("Nível 2").Build();
            var user = new UserBuilder().WithDepartment(department).Build();
            var tech1 = new UserBuilder()
                .WithDepartment(department)
                .WithRole(Role.Technician)
                .Build();
            var tech2 = new UserBuilder()
                .WithDepartment(department)
                .WithRole(Role.Technician)
                .Build();

            var ticket = new TicketBuilder()
                .WithDepartment(department)
                .WithQueue(queue1)
                .WithCreatedBy(user)
                .Build();

            ticket.AssignTechnician(tech1, user, "Primeiro atendimento");

            // Act
            ticket.TransferToQueue(
                queue2,
                tech2,
                user,
                "Escalado para Nível 2"
            );

            // Assert
            ticket.QueueId.Should().Be(queue2.Id);
            ticket.Status.Should().Be(TicketStatus.Assigned);
            ticket.Assignments.Should().HaveCount(2);

            var firstAssignment = ticket.Assignments.First();
            firstAssignment.FinishedAt.Should().NotBeNull();
            firstAssignment.TechnicianId.Should().Be(tech1.Id);

            var currentAssignment = ticket.CurrentAssignment;
            currentAssignment.Should().NotBeNull();
            currentAssignment!.TechnicianId.Should().Be(tech2.Id);
            currentAssignment.Reason.Should().Be("Escalado para Nível 2");
            currentAssignment.FinishedAt.Should().BeNull();
        }

        [Theory]
        [InlineData(TicketStatus.Resolved)]
        [InlineData(TicketStatus.Closed)]
        public void Transfer_WhenTicketIsResolvedOrClosed_ShouldThrowDomainException(
            TicketStatus terminalStatus
        )
        {
            // Arrange
            var department = new DepartmentBuilder().Build();
            var queue = new SupportQueueBuilder().Build();
            var user = new UserBuilder().WithDepartment(department).Build();
            var tech1 = new UserBuilder()
                .WithDepartment(department)
                .WithRole(Role.Technician)
                .Build();
            var tech2 = new UserBuilder()
                .WithDepartment(department)
                .WithRole(Role.Technician)
                .Build();

            var ticket = new TicketBuilder()
                .WithDepartment(department)
                .WithQueue(queue)
                .WithCreatedBy(user)
                .Build();

            ticket.AssignTechnician(tech1, user, "Atendimento inicial");
            ticket.StartService();
            ticket.Resolve();

            if (terminalStatus == TicketStatus.Closed)
                ticket.Close();

            // Act
            var act = () =>
                ticket.TransferToQueue(
                    queue,
                    tech2,
                    user,
                    "Tentativa em estado terminal"
                );

            // Assert
            act.Should()
                .Throw<DomainException>()
                .WithMessage(
                    "Não é possível transferir tickets resolvidos ou fechados."
                );
        }
    }
}
