using FluentAssertions;
using Moq;
using SIGTI.Application.Common.Exceptions;
using SIGTI.Application.Common.Interfaces.Persistence;
using SIGTI.Application.Common.Interfaces.Services;
using SIGTI.Application.Features.Tickets.Commands.CloseTicket;
using SIGTI.Domain.Entities;
using SIGTI.Domain.Enums;
using SIGTI.Domain.Exceptions;
using SIGTI.Domain.Tests.Builders;

namespace SIGTI.Application.Tests.Features.Tickets.Commands.CloseTicketService
{
    public class CloseTicketCommandHandlerTests
    {
        private readonly Mock<IEntityReferenceService> _entityReferenceServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly CloseTicketCommandHandler _handler;

        public CloseTicketCommandHandlerTests()
        {
            _entityReferenceServiceMock = new Mock<IEntityReferenceService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _handler = new CloseTicketCommandHandler(
                _entityReferenceServiceMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Fact]
        public async Task Handle_WhenTicketIsResolved_ShouldCloseTicketAndCommit()
        {
            // Arrange
            var department = new DepartmentBuilder().Build();
            var queue = new SupportQueueBuilder().Build();
            var createdBy = new UserBuilder()
                .WithDepartment(department)
                .WithEmail("user@sigti.local")
                .Build();
            var technician = new UserBuilder()
                .WithDepartment(department)
                .WithEmail("tech@sigti.local")
                .Build();

            var ticket = new TicketBuilder()
                .WithDepartment(department)
                .WithQueue(queue)
                .WithCreatedBy(createdBy)
                .Build();

            ticket.AssignTechnician(
                technician,
                createdBy,
                "Atribuição para atendimento."
            );
            ticket.StartService();
            ticket.Resolve();

            _entityReferenceServiceMock
                .Setup(s =>
                    s.GetRequiredTicketAsync(
                        ticket.Id,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(ticket);

            var command = new CloseTicketCommand(ticket.Id);

            // Act
            var response = await _handler.Handle(
                command,
                CancellationToken.None
            );

            // Assert
            response.Should().NotBeNull();
            response.Id.Should().Be(ticket.Id);
            response.Status.Should().Be(TicketStatus.Closed);
            response.ClosedAt.Should().NotBeNull();

            ticket.Status.Should().Be(TicketStatus.Closed);
            _unitOfWorkMock.Verify(
                u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_WhenTicketIsNotResolved_ShouldThrowDomainExceptionAndNeverCommit()
        {
            // Arrange: Um chamado em progresso (ou recém-criado) não pode ser fechado
            var department = new DepartmentBuilder().Build();
            var queue = new SupportQueueBuilder().Build();
            var createdBy = new UserBuilder()
                .WithDepartment(department)
                .WithEmail("user@sigti.local")
                .Build();
            var technician = new UserBuilder()
                .WithDepartment(department)
                .WithEmail("tech@sigti.local")
                .Build();

            var ticket = new TicketBuilder()
                .WithDepartment(department)
                .WithQueue(queue)
                .WithCreatedBy(createdBy)
                .Build();

            ticket.AssignTechnician(
                technician,
                createdBy,
                "Atribuição para atendimento."
            );
            ticket.StartService();

            _entityReferenceServiceMock
                .Setup(s =>
                    s.GetRequiredTicketAsync(
                        ticket.Id,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(ticket);

            var command = new CloseTicketCommand(ticket.Id);

            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage("Somente tickets resolvidos podem ser fechados.");
            _unitOfWorkMock.Verify(
                uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Fact]
        public async Task Handle_WhenTicketDoesNotExist_ShouldPropagateNotFoundExceptionAndNeverCommit()
        {
            // Arrange
            var nonExistentTicketId = Guid.NewGuid();

            _entityReferenceServiceMock
                .Setup(s =>
                    s.GetRequiredTicketAsync(
                        nonExistentTicketId,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ThrowsAsync(
                    new NotFoundException(nameof(Ticket), nonExistentTicketId)
                );

            var command = new CloseTicketCommand(nonExistentTicketId);

            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
            _unitOfWorkMock.Verify(
                uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never
            );
        }
    }
}
