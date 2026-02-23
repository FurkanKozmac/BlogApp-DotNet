using MediatR;

namespace BlogApp.Application.Features.Posts.Commands.DeletePost;

public record DeletePostCommand(int Id): IRequest<bool>;