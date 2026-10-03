using BlogApp.Application.Models.Requests;
using FluentValidation;

namespace BlogApp.Application.Validators;

public class GetPostsRequestValidator : AbstractValidator<GetPostsRequest>
{
    public const int MaximumPageNumber = 1_000_000;
    public const int MaximumPageSize = 100;

    public GetPostsRequestValidator()
    {
        RuleFor(request => request.PageNumber)
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(MaximumPageNumber);
        RuleFor(request => request.PageSize)
            .GreaterThanOrEqualTo(1)
            .LessThanOrEqualTo(MaximumPageSize);
    }
}
