using FluentAssertions;
using Moq;
using SIGTI.Application.Common.Exceptions;
using SIGTI.Application.Common.Interfaces.Persistence;
using SIGTI.Application.Common.Interfaces.Services;
using SIGTI.Application.Features.SupportQueues.Commands.AddMember;
using SIGTI.Domain.Entities;
using SIGTI.Domain.Enums;
using SIGTI.Domain.Exceptions;
using SIGTI.Domain.Tests.Builders;

namespace SIGTI.Application.Tests.Features.SupportQueues.Commands.AddMember
{
    public class AddMemberCommandHandlerTests
    {
        private readonly Mock<IEntityReferenceService> _entityReferenceServiceMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly AddMemberCommandHandler _handler;

        public AddMemberCommandHandlerTests()
        {
            _entityReferenceServiceMock = new Mock<IEntityReferenceService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _handler = new AddMemberCommandHandler(
                _entityReferenceServiceMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Fact]
        public async Task Handle_WhenDataIsValid_ShouldAddMemberAndCommit()
        {
            // Arrange
            var queue = new SupportQueueBuilder().Build();
            var technician = new UserBuilder()
                .WithRole(Role.Technician)
                .Build();

            _entityReferenceServiceMock
                .Setup(x =>
                    x.GetRequiredQueueAsync(queue.Id, CancellationToken.None)
                )
                .ReturnsAsync(queue);

            _entityReferenceServiceMock
                .Setup(x =>
                    x.GetRequiredUserAsync(
                        technician.Id,
                        CancellationToken.None
                    )
                )
                .ReturnsAsync(technician);

            var command = new AddMemberCommand(queue.Id, technician.Id, 5);

            // Act

            var response = await _handler.Handle(
                command,
                CancellationToken.None
            );

            // Assert
            response.Should().NotBeNull();
            response.Id.Should().Be(queue.Id);

            queue
                .Members.Should()
                .ContainSingle(member =>
                    member.TechnicianId == technician.Id
                    && member.MaxConcurrentTickets == 5
                    && member.IsActive
                );

            _unitOfWorkMock.Verify(
                uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_WhenQueueNotFound_ShouldThrowNotFoundExcetion()
        {
            // Arrange
            var nonExistentQueueId = Guid.NewGuid();
            var technician = new UserBuilder()
                .WithRole(Role.Technician)
                .Build();

            _entityReferenceServiceMock
                .Setup(x =>
                    x.GetRequiredQueueAsync(
                        nonExistentQueueId,
                        CancellationToken.None
                    )
                )
                .ThrowsAsync(
                    new NotFoundException(
                        nameof(SupportQueue),
                        nonExistentQueueId
                    )
                );

            _entityReferenceServiceMock
                .Setup(x =>
                    x.GetRequiredUserAsync(
                        technician.Id,
                        CancellationToken.None
                    )
                )
                .ReturnsAsync(technician);

            var command = new AddMemberCommand(
                nonExistentQueueId,
                technician.Id,
                5
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

        [Fact]
        public async Task Handle_WhenTechnicianNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var queue = new SupportQueueBuilder().Build();
            var nonExistentTechnician = Guid.NewGuid();

            _entityReferenceServiceMock
                .Setup(x =>
                    x.GetRequiredQueueAsync(queue.Id, CancellationToken.None)
                )
                .ReturnsAsync(queue);

            _entityReferenceServiceMock
                .Setup(x =>
                    x.GetRequiredUserAsync(
                        nonExistentTechnician,
                        CancellationToken.None
                    )
                )
                .ThrowsAsync(
                    new NotFoundException(nameof(User), nonExistentTechnician)
                );

            var command = new AddMemberCommand(
                queue.Id,
                nonExistentTechnician,
                5
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

        [Fact]
        public async Task Handle_WhenUserIsNotTechnician_ShouldThrowDomainException()
        {
            // Arrange
            var queue = new SupportQueueBuilder().Build();
            var commonUser = new UserBuilder().WithRole(Role.User).Build();

            _entityReferenceServiceMock
                .Setup(x =>
                    x.GetRequiredQueueAsync(queue.Id, CancellationToken.None)
                )
                .ReturnsAsync(queue);

            _entityReferenceServiceMock
                .Setup(x =>
                    x.GetRequiredUserAsync(
                        commonUser.Id,
                        CancellationToken.None
                    )
                )
                .ReturnsAsync(commonUser);

            var command = new AddMemberCommand(queue.Id, commonUser.Id, 5);

            // Act
            var act = () => _handler.Handle(command, CancellationToken.None);
            // Assert

            await act.Should().ThrowAsync<DomainException>();
            _unitOfWorkMock.Verify(
                uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never
            );
        }
    }
}
