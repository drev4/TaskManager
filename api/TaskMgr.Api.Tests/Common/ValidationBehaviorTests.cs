using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using TaskMgr.Api.Application.Common.Behaviors;
using Xunit;

namespace TaskMgr.Api.Tests.Common;

public record SampleRequest(string Name) : IRequest<string>;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_NoValidators_CallsNext()
    {
        var behavior = new ValidationBehavior<SampleRequest, string>(Array.Empty<IValidator<SampleRequest>>());
        var next = new Mock<RequestHandlerDelegate<string>>();
        next.Setup(n => n(It.IsAny<CancellationToken>())).ReturnsAsync("ok");

        var result = await behavior.Handle(new SampleRequest("x"), next.Object, CancellationToken.None);

        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task Handle_ValidatorPasses_CallsNext()
    {
        var validator = new Mock<IValidator<SampleRequest>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<SampleRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new ValidationBehavior<SampleRequest, string>(new[] { validator.Object });
        var next = new Mock<RequestHandlerDelegate<string>>();
        next.Setup(n => n(It.IsAny<CancellationToken>())).ReturnsAsync("ok");

        var result = await behavior.Handle(new SampleRequest("x"), next.Object, CancellationToken.None);

        Assert.Equal("ok", result);
    }

    [Fact]
    public async Task Handle_ValidatorFails_ThrowsValidationExceptionAndSkipsNext()
    {
        var validator = new Mock<IValidator<SampleRequest>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<SampleRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Name", "Name is required") }));

        var behavior = new ValidationBehavior<SampleRequest, string>(new[] { validator.Object });
        var next = new Mock<RequestHandlerDelegate<string>>();

        await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(new SampleRequest(""), next.Object, CancellationToken.None));

        next.Verify(n => n(It.IsAny<CancellationToken>()), Times.Never);
    }
}
