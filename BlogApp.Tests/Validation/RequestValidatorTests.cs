using BlogApp.Application.Models.Requests;
using BlogApp.Application.Validators;
using Xunit;

namespace BlogApp.Tests.Validation;

public class RequestValidatorTests
{
    [Fact]
    public void RegisterValidator_RejectsInvalidEmailAndShortPassword()
    {
        var validator = new RegisterRequestValidator();
        var result = validator.Validate(new RegisterRequest
        {
            FullName = "Jane Doe",
            Email = "not-an-email",
            UserName = "jane",
            Password = "short"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterRequest.Email));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterRequest.Password));
    }

    [Fact]
    public void LoginValidator_RejectsMissingCredentials()
    {
        var result = new LoginRequestValidator().Validate(new LoginRequest());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(LoginRequest.UserName));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(LoginRequest.Password));
    }

    [Fact]
    public void CategoryValidator_RejectsEmptyName()
    {
        var result = new CreateCategoryRequestValidator().Validate(new CreateCategoryRequest());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateCategoryRequest.Name));
    }
}
