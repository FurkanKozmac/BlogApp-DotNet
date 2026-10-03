using BlogApp.Application.Models.Requests;
using BlogApp.Application.Validators;
using Xunit;

namespace BlogApp.Tests.Validation;

public class RequestValidatorTests
{
    [Fact]
    public void Register_WhenEmailOrPasswordIsInvalid_ValidatorReturnsErrors()
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
    public void Login_WhenCredentialsAreMissing_ValidatorReturnsErrors()
    {
        var result = new LoginRequestValidator().Validate(new LoginRequest());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(LoginRequest.UserName));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(LoginRequest.Password));
    }

    [Fact]
    public void CreateCategory_WhenNameIsEmpty_ValidatorReturnsError()
    {
        var result = new CreateCategoryRequestValidator().Validate(new CreateCategoryRequest());

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateCategoryRequest.Name));
    }

    [Fact]
    public void CreateComment_WhenTextIsEmpty_ValidatorReturnsError()
    {
        var result = new CreateCommentRequestValidator().Validate(new CreateCommentRequest { PostId = 1 });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateCommentRequest.Text));
    }
}
