using FluentAssertions;
using Moq;
using SIGTI.Application.Common.Exceptions;
using SIGTI.Application.Common.Interfaces.Services;
using SIGTI.Application.Features.Tickets.Queries.ListTicketComments;
using SIGTI.Domain.Entities;
using SIGTI.Domain.Tests.Builders;

namespace SIGTI.Application.Tests.Features.Tickets.Queries.ListTicketComments
{
    public class ListTicketCommentsQueryHandlerTests
    {
        private readonly Mock<IEntityReferenceService> _entityReferenceServiceMock;
        private readonly ListTicketCommentsQueryHandler _handler;

        public ListTicketCommentsQueryHandlerTests()
        {
            _entityReferenceServiceMock = new Mock<IEntityReferenceService>();
            _handler = new ListTicketCommentsQueryHandler(
                _entityReferenceServiceMock.Object
            );
        }

        [Fact]
        public async Task Handle_WhenTicketHasComments_ShouldReturnOrderedComments()
        {
            // Arrange
            var department = new DepartmentBuilder().Build();
            var queue = new SupportQueueBuilder().Build();
            var user = new UserBuilder()
                .WithDepartment(department)
                .WithEmail("user@sigti.local")
                .Build();
            var tech = new UserBuilder()
                .WithDepartment(department)
                .WithEmail("tech@sigti.local")
                .Build();

            var ticket = new TicketBuilder()
                .WithDepartment(department)
                .WithQueue(queue)
                .WithCreatedBy(user)
                .Build();

            var comment1 = new Comment(
                "Seu chamado foi assumido por mim tech.",
                ticket,
                tech
            );
            var comment2 = new Comment(
                "Ok, meu computador está com problemas como você pode me ajudar?",
                ticket,
                user
            );

            ticket.AddComment(comment1);
            ticket.AddComment(comment2);

            _entityReferenceServiceMock
                .Setup(s =>
                    s.GetRequiredTicketAsync(
                        ticket.Id,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(ticket);

            var query = new ListTicketCommentsQuery(ticket.Id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result
                .First()
                .Content.Should()
                .Be("Seu chamado foi assumido por mim tech.");
            result
                .Last()
                .Content.Should()
                .Be(
                    "Ok, meu computador está com problemas como você pode me ajudar?"
                );
        }

        [Fact]
        public async Task Handle_WhenTicketHasNoComments_ShouldReturnEmptyList()
        {
            // Arrange
            var department = new DepartmentBuilder().Build();
            var queue = new SupportQueueBuilder().Build();
            var user = new UserBuilder().WithDepartment(department).Build();

            var ticket = new TicketBuilder()
                .WithDepartment(department)
                .WithQueue(queue)
                .WithCreatedBy(user)
                .Build();

            _entityReferenceServiceMock
                .Setup(s =>
                    s.GetRequiredTicketAsync(
                        ticket.Id,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(ticket);

            var query = new ListTicketCommentsQuery(ticket.Id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WhenTicketNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            _entityReferenceServiceMock
                .Setup(s =>
                    s.GetRequiredTicketAsync(
                        nonExistentId,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ThrowsAsync(
                    new NotFoundException(nameof(Ticket), nonExistentId)
                );

            var query = new ListTicketCommentsQuery(nonExistentId);

            // Act
            var act = () => _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
