using FluentValidation;
using FluentValidation.Results;

using Lynkly.Shared.Kernel.Core.Exceptions;
using Lynkly.Shared.Kernel.Core.Validation;

namespace Lynkly.Resolver.UnitTests.SharedKernel.Validation;

public sealed class ValidationFrameworkTests
{
    [Fact]
    public void ValidateAndThrowAppException_Should_Throw_BaseAppException_With_ErrorDetails()
    {
        var validator = new TestRequestValidator();
        var request = new TestRequest(string.Empty, -1);

        var exception = Assert.Throws<ValidationAppException>(() => validator.ValidateAndThrowAppException(request));

        Assert.Equal(ExceptionCodes.ValidationFailed, exception.Code);
        Assert.Equal(400, exception.StatusCode);
        Assert.Equal("Validation failed for the request.", exception.Message);
        Assert.Equal(2, exception.Errors.Count);
        Assert.Contains(exception.Errors, e => e.Field == nameof(TestRequest.Name));
        Assert.Contains(exception.Errors, e => e.Field == nameof(TestRequest.Age));

        var response = ErrorResponseFactory.Create(exception);
        Assert.Equal(ExceptionCodes.ValidationFailed, response.Code);
        Assert.Equal(exception.Errors, response.Errors);
    }

    [Fact]
    public async Task ValidateAndThrowAppExceptionAsync_Should_Throw_For_Invalid_Request()
    {
        var validator = new TestRequestValidator();
        var request = new TestRequest(string.Empty, 0);

        var exception = await Assert.ThrowsAsync<ValidationAppException>(
            () => validator.ValidateAndThrowAppExceptionAsync(request));

        Assert.Equal(ExceptionCodes.ValidationFailed, exception.Code);
        Assert.Equal(2, exception.Errors.Count);
    }

    [Fact]
    public void ValidateAndThrowAppException_Should_Not_Throw_For_Valid_Request()
    {
        var validator = new TestRequestValidator();
        var request = new TestRequest("Lynkly", 10);

        var exception = Record.Exception(() => validator.ValidateAndThrowAppException(request));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidationAppException_Should_Map_ValidationFailures_To_ErrorDetails()
    {
        var errors = new[]
        {
            new ErrorDetail("name", "Name is required", "NotEmptyValidator")
        };

        var exception = new ValidationAppException(errors, "Invalid request payload.");

        Assert.Equal("Invalid request payload.", exception.Message);
        Assert.Single(exception.Errors);
        Assert.Equal("name", exception.Errors[0].Field);
        Assert.Equal("Name is required", exception.Errors[0].Message);
        Assert.Equal("NotEmptyValidator", exception.Errors[0].Code);
    }

    private sealed record TestRequest(string Name, int Age);

    private sealed class TestRequestValidator : AbstractValidator<TestRequest>
    {
        public TestRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Age).GreaterThan(0);
        }
    }
}
