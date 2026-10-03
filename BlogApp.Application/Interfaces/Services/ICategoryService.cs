using BlogApp.Application.Models.Requests;

namespace BlogApp.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<int> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
}
