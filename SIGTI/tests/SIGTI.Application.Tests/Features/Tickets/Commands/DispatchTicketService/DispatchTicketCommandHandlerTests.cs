using FluentAssertions;
using Moq;
using SIGTI.Application.Common.Enums;
using SIGTI.Application.Common.Exceptions;
using SIGTI.Application.Common.Interfaces.Persistence;
using SIGTI.Application.Common.Interfaces.Services;
using SIGTI.Application.Features.Tickets.Commands.DispatchTicket;
using SIGTI.Application.Features.Tickets.Queries.ListTickets;
using SIGTI.Domain.Entities;
using SIGTI.Domain.Interfaces.Services;
using SIGTI.Domain.Tests.Builders;

namespace SIGTI.Application.Tests.Features.Tickets.Commands.DispatchTicketService
{
    public class DispatchTicketCommandHandlerTests
    {
        private readonly Mock<IEntityReferenceService> _entityReferenceServiceMock;
        private readonly Mock<ITicketRepository> _ticketRepository;
        private readonly Mock<ITicketAssignmentStrategy> _assignmentStrategyMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly DispatchTicketHandler _handler;

        public DispatchTicketCommandHandlerTests()
        {
            _entityReferenceServiceMock = new Mock<IEntityReferenceService>();
            _ticketRepository = new Mock<ITicketRepository>();
            _assignmentStrategyMock = new Mock<ITicketAssignmentStrategy>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _handler = new DispatchTicketHandler(
                _entityReferenceServiceMock.Object,
                _ticketRepository.Object,
                _assignmentStrategyMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Fact]
        public async Task Handle_WhenTechnicianIdIsProvided_ShouldPerformManualAssignment()
        {
            // Arrange
            var department = new DepartmentBuilder().Build();
            var queue = new SupportQueueBuilder().Build();
            var createdBy = new UserBuilder()
                .WithDepartment(department)
                .WithEmail("creator@sigti.local")
                .Build();
            var tech = new UserBuilder()
                .WithDepartment(department)
                .WithEmail("tech@sigti.local")
                .Build();
            var assignedBy = new UserBuilder()
                .WithDepartment(department)
                .WithEmail("administrator@sigti.local")
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
                        assignedBy.Id,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(assignedBy);

            _entityReferenceServiceMock
                .Setup(s =>
                    s.GetRequiredUserAsync(
                        tech.Id,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(tech);

            var command = new DispatchTicketCommand(
                ticket.Id,
                tech.Id,
                assignedBy.Id
            );

            // Act
            var response = await _handler.Handle(
                command,
                CancellationToken.None
            );

            // Assert
            response.Should().NotBeNull();
            response.TicketId.Should().Be(ticket.Id);
            response.TechnicianId.Should().Be(tech.Id);
            response.TechnicianName.Should().Be(tech.Name);

            ticket.Assignments.Should().HaveCount(1);
            ticket
                .Assignments.First(assignment => !assignment.IsFinished)
                .TechnicianId.Should()
                .Be(tech.Id);

            _unitOfWorkMock.Verify(
                uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
            _assignmentStrategyMock.Verify(
                strategy =>
                    strategy.SelectTechnician(
                        It.IsAny<SupportQueue>(),
                        It.IsAny<IReadOnlyDictionary<Guid, int>>()
                    ),
                Times.Never
            );
        }

        [Fact]
        public async Task Handle_WhenTechnicianIdIsNull_ShouldExecuteAssignmentStrategy()
        {
            // Arrange
            var department = new DepartmentBuilder().Build();
            var queue = new SupportQueueBuilder().Build();
            var createdBy = new UserBuilder()
                .WithDepartment(department)
                .WithEmail("creator@sigti.local")
                .Build();
            var tech = new UserBuilder()
                .WithDepartment(department)
                .WithEmail("tech@sigti.local")
                .Build();
            var assignedBy = new UserBuilder()
                .WithDepartment(department)
                .WithEmail("administrator@sigti.local")
                .Build();

            queue.AddMember(technician: tech, maxConcurrentTickets: 5);
            var member = queue.Members.First(m => m.TechnicianId == tech.Id);

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
                        assignedBy.Id,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(assignedBy);

            _entityReferenceServiceMock
                .Setup(s =>
                    s.GetRequiredQueueAsync(
                        queue.Id,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(queue);

            _ticketRepository
                .Setup(r =>
                    r.ListAsync(
                        It.IsAny<TicketListFilter>(),
                        It.IsAny<TicketSortField>(),
                        It.IsAny<SortDirection>(),
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(new List<Ticket>());

            _assignmentStrategyMock
                .Setup(s =>
                    s.SelectTechnician(
                        queue,
                        It.IsAny<IReadOnlyDictionary<Guid, int>>()
                    )
                )
                .Returns(member);

            _entityReferenceServiceMock
                .Setup(s =>
                    s.GetRequiredUserAsync(
                        tech.Id,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(tech);

            var command = new DispatchTicketCommand(
                ticket.Id,
                null,
                assignedBy.Id,
                null
            );

            // Act
            var response = await _handler.Handle(
                command,
                CancellationToken.None
            );

            // Assert
            response.Should().NotBeNull();
            response.TechnicianId.Should().Be(tech.Id);
            response.TechnicianName.Should().Be(tech.Name);

            ticket.Assignments.Should().HaveCount(1);
            ticket
                .Assignments.First(assignment => !assignment.IsFinished)
                .TechnicianId.Should()
                .Be(tech.Id);

            _assignmentStrategyMock.Verify(
                s =>
                    s.SelectTechnician(
                        queue,
                        It.IsAny<IReadOnlyDictionary<Guid, int>>()
                    ),
                Times.Once
            );

            _unitOfWorkMock.Verify(
                uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_WhenTicketDoesNotExist_ShouldPropagateNotFoundException()
        {
            // Arrange
            var nonExistenteTicket = Guid.NewGuid();

            _entityReferenceServiceMock
                .Setup(s =>
                    s.GetRequiredTicketAsync(
                        nonExistenteTicket,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ThrowsAsync(
                    new NotFoundException(nameof(Ticket), nonExistenteTicket)
                );

            var command = new DispatchTicketCommand(
                nonExistenteTicket,
                Guid.NewGuid(),
                Guid.NewGuid(),
                null
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
