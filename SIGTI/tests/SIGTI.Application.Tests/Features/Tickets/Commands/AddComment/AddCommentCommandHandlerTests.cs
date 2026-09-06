using FluentAssertions;
using Moq;
using SIGTI.Application.Common.Exceptions;
using SIGTI.Application.Common.Interfaces.Persistence;
using SIGTI.Application.Common.Interfaces.Services;
using SIGTI.Application.Features.Tickets.Commands.AddComment;
using SIGTI.Domain.Entities;
using SIGTI.Domain.Exceptions;
using SIGTI.Domain.Tests.Builders;

namespace SIGTI.Application.Tests.Features.Tickets.Commands.AddComment
{
    public class AddCommentCommandHandlerTests
    {
        private readonly Mock<IEntityReferenceService> _entityReferenceServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly AddCommentCommandHandler _handler;

        public AddCommentCommandHandlerTests()
        {
            _entityReferenceServiceMock = new Mock<IEntityReferenceService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _handler = new AddCommentCommandHandler(
                _entityReferenceServiceMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Fact]
        public async Task Handle_WhenValid_ShouldAddCommentAndCommit()
        {
            // Arrange
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

            _entityReferenceServiceMock
                .Setup(s =>
                    s.GetRequiredUserAsync(
                        createdBy.Id,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(createdBy);

            var command = new AddCommentCommand(
                ticket.Id,
                createdBy.Id,
                "Mensagem de atualização sobre o chamado."
            );

            // Act
            var response = await _handler.Handle(
                command,
                CancellationToken.None
            );

            // Assert
            response.Should().NotBeNull();
            response.TicketId.Should().Be(ticket.Id);
            response.CreatedById.Should().Be(createdBy.Id);
            response.CreatedByName.Should().Be(createdBy.Name);
            response
                .Content.Should()
                .Be("Mensagem de atualização sobre o chamado.");

            ticket.Comments.Should().HaveCount(1);
            ticket
                .Comments.First()
                .Content.Should()
                .Be("Mensagem de atualização sobre o chamado.");

            _unitOfWorkMock.Verify(
                uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_WhenTicketIsClosed_ShouldThrowDomainExceptionAndNeverCommit()
        {
            // Arrange: Closed Tickets rejects comments
            var department = new DepartmentBuilder().Build();
            var queue = new SupportQueueBuilder().Build();
            var createdBy = new UserBuilder()
                .WithDepartment(department)
                .WithEmail("user@sigti.local")
                .Build();z
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
                "Atribuição para atendimento"
            );
            ticket.StartService();
            ticket.Resolve();
            ticket.Close();

            _entityReferenceServiceMock
                .Setup(s =>
                    s.GetRequiredTicketAsync(
                        ticket.Id,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(ticket);

            _entityReferenceServiceMock
                .Setup(s =>
                    s.GetRequiredUserAsync(
                        createdBy.Id,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(createdBy);

            var command = new AddCommentCommand(
                ticket.Id,
                createdBy.Id,
                "Tentativa de comentário em ticket fechado."
            );

            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<DomainException>()
                .WithMessage(
                    "Não é possível efetuar comentários em tickets fechados."
                );

            ticket.Comments.Should().BeEmpty();

            _unitOfWorkMock.Verify(
                uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()),
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

            var command = new AddCommentCommand(
                nonExistentTicketId,
                Guid.NewGuid(),
                "Texto de teste."
            );

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
