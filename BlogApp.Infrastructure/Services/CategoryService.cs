using BlogApp.Application.Interfaces.Repositories;
using BlogApp.Application.Interfaces.Services;
using BlogApp.Application.Models.Requests;
using BlogApp.Domain.Entities;
using FluentValidation;

namespace BlogApp.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly IGenericRepository<Category> _categoryRepository;
    private readonly IValidator<CreateCategoryRequest> _validator;

    public CategoryService(
        IGenericRepository<Category> categoryRepository,
        IValidator<CreateCategoryRequest> validator)
    {
        _categoryRepository = categoryRepository;
        _validator = validator;
    }

    public async Task<int> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var category = new Category { Name = request.Name };
        await _categoryRepository.AddAsync(category);
        return category.Id;
    }
}
