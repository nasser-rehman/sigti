using FluentAssertions;
using Moq;
using SIGTI.Application.Common.Exceptions;
using SIGTI.Application.Common.Interfaces.Persistence;
using SIGTI.Application.Common.Interfaces.Services;
using SIGTI.Application.Features.Tickets.Commands.ResolveTicket;
using SIGTI.Domain.Entities;
using SIGTI.Domain.Enums;
using SIGTI.Domain.Exceptions;
using SIGTI.Domain.Tests.Builders;

namespace SIGTI.Application.Tests.Features.Tickets.Commands.ResolveTicketService
{
    public class ResolveTicketCommandHandlerTests
    {
        private readonly Mock<IEntityReferenceService> _entityReferenceServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly ResolveTicketCommandHandler _handler;

        public ResolveTicketCommandHandlerTests()
        {
            _entityReferenceServiceMock = new Mock<IEntityReferenceService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _handler = new ResolveTicketCommandHandler(
                _entityReferenceServiceMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Fact]
        public async Task Handle_WhenTicketIsInProgress_ShouldResolveAndCommit()
        {
            // Arrange
            var department = new DepartmentBuilder().Build();
            var queue = new SupportQueueBuilder().Build();
            var createdBy = new UserBuilder()
                .WithDepartment(department)
                .WithEmail("createdBy@sigti.local")
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
                "Atribuição para atendimento inicial"
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

            var command = new ResolveTicketCommand(ticket.Id);

            //Act
            var response = await _handler.Handle(
                command,
                CancellationToken.None
            );

            // Assert
            response.Should().NotBeNull();
            response.Id.Should().Be(ticket.Id);
            response.Status.Should().Be(TicketStatus.Resolved);
            response.ResolvedAt.Should().NotBeNull();

            ticket.Status.Should().Be(TicketStatus.Resolved);
            _unitOfWorkMock.Verify(
                u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_WhenTicketIsNew_ShouldThrowDomainExceptionAndNeverCommit()
        {
            var department = new DepartmentBuilder().Build();
            var queue = new SupportQueueBuilder().Build();
            var createdBy = new UserBuilder()
                .WithDepartment(department)
                .WithEmail("user@sigti.local")
                .Build();

            var ticket = new TicketBuilder()
                .WithDepartment(department)
                .WithQueue(queue)
                .WithCreatedBy(createdBy)
                .Build();

            _entityReferenceServiceMock
                .Setup(s =>
                    s.GetRequiredTicketAsync(
                        ticket.Id,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(ticket);

            var command = new ResolveTicketCommand(ticket.Id);

            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage(
                    "*deve estar em andamento ou aguardando o cliente*"
                );
            _unitOfWorkMock.Verify(
                u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Fact]
        public async Task Handle_WhenTicketDoesNotExist_ShouldPropagateNotFoundException()
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

            var command = new ResolveTicketCommand(nonExistentTicketId);

            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
            _unitOfWorkMock.Verify(
                u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never
            );
        }
    }
}
