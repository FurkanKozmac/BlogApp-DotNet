using MediatR;

namespace BlogApp.Application.Features.Categories.Commands;

public record CreateCategoryCommand(string Name) : IRequest<int>;