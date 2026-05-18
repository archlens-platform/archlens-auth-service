using ArchLens.Auth.Application.Behaviors;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;

namespace ArchLens.Auth.Tests.Application.Behaviors;

public record TestBehaviorRequest(string Name) : IRequest<string>;

public class PassingValidator : AbstractValidator<TestBehaviorRequest>
{
    public PassingValidator() { }
}

public class FailingValidator : AbstractValidator<TestBehaviorRequest>
{
    public FailingValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name is required");
    }
}

public class NameTooShortValidator : AbstractValidator<TestBehaviorRequest>
{
    public NameTooShortValidator()
    {
        RuleFor(x => x.Name).MinimumLength(5).WithMessage("Too short");
    }
}

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_NoValidators_ShouldCallNext()
    {
        var validators = Enumerable.Empty<IValidator<TestBehaviorRequest>>();
        var behavior = new ValidationBehavior<TestBehaviorRequest, string>(validators);
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next(Arg.Any<CancellationToken>()).Returns("result");

        var result = await behavior.Handle(new TestBehaviorRequest("test"), next, CancellationToken.None);

        result.Should().Be("result");
        await next.Received(1)(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidatorsPass_ShouldCallNext()
    {
        var validators = new IValidator<TestBehaviorRequest>[] { new PassingValidator() };
        var behavior = new ValidationBehavior<TestBehaviorRequest, string>(validators);
        var next = Substitute.For<RequestHandlerDelegate<string>>();
        next(Arg.Any<CancellationToken>()).Returns("ok");

        var result = await behavior.Handle(new TestBehaviorRequest("valid"), next, CancellationToken.None);

        result.Should().Be("ok");
    }

    [Fact]
    public async Task Handle_ValidatorFails_ShouldThrowValidationException()
    {
        var validators = new IValidator<TestBehaviorRequest>[] { new FailingValidator() };
        var behavior = new ValidationBehavior<TestBehaviorRequest, string>(validators);
        var next = Substitute.For<RequestHandlerDelegate<string>>();

        var act = () => behavior.Handle(new TestBehaviorRequest(""), next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await next.DidNotReceive()(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_MultipleValidators_OneFailure_ShouldThrow()
    {
        var validators = new IValidator<TestBehaviorRequest>[]
        {
            new PassingValidator(),
            new FailingValidator()
        };
        var behavior = new ValidationBehavior<TestBehaviorRequest, string>(validators);
        var next = Substitute.For<RequestHandlerDelegate<string>>();

        var act = () => behavior.Handle(new TestBehaviorRequest(""), next, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_MultipleValidators_MultipleFailures_ShouldAggregateErrors()
    {
        var validators = new IValidator<TestBehaviorRequest>[]
        {
            new FailingValidator(),
            new NameTooShortValidator()
        };
        var behavior = new ValidationBehavior<TestBehaviorRequest, string>(validators);
        var next = Substitute.For<RequestHandlerDelegate<string>>();

        var act = () => behavior.Handle(new TestBehaviorRequest(""), next, CancellationToken.None);

        var ex = await act.Should().ThrowAsync<ValidationException>();
        ex.Which.Errors.Should().HaveCountGreaterOrEqualTo(2);
    }
}
