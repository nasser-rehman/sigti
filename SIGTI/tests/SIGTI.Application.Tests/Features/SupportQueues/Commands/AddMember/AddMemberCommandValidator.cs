using FluentAssertions;
using SIGTI.Application.Features.SupportQueues.Commands.AddMember;

namespace SIGTI.Application.Tests.Features.SupportQueues.Commands.AddMember
{
    public class AddMemberCommandValidatorTests
    {
        private readonly AddMemberCommandValidator _validator = new();

        [Fact]
        public void Validate_WhenCommandIsValid_ShouldNotHaveValidationError()
        {
            var command = new AddMemberCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                5
            );

            var result = _validator.Validate(command);

            result.IsValid.Should().BeTrue();
            result.Errors.Should().BeEmpty();
        }

        [Fact]
        public void Validate_WhenQueueIdIsEmpty_ShouldHaveValidationError()
        {
            var command = new AddMemberCommand(Guid.Empty, Guid.NewGuid(), 5);

            var result = _validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result
                .Errors.Should()
                .Contain(error =>
                    error.PropertyName == nameof(AddMemberCommand.QueueId)
                );
        }

        [Fact]
        public void Validate_WhenTechnicianIdIsEmpty_ShouldHaveValidationError()
        {
            var command = new AddMemberCommand(Guid.NewGuid(), Guid.Empty, 5);

            var result = _validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result
                .Errors.Should()
                .Contain(error =>
                    error.PropertyName == nameof(AddMemberCommand.TechnicianId)
                );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WhenMaxConcurrentTicketsIsInvalid_ShouldHaveValidationError(
            int maxConcurrentTickets
        )
        {
            var command = new AddMemberCommand(
                Guid.NewGuid(),
                Guid.NewGuid(),
                maxConcurrentTickets!
            );

            var result = _validator.Validate(command);

            result.IsValid.Should().BeFalse();
            result
                .Errors.Should()
                .Contain(error =>
                    error.PropertyName
                    == nameof(AddMemberCommand.MaxConcurrentTickets)
                );
        }
    }
}
