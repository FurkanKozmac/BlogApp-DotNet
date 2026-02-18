using AutoMapper;
using BlogApp.Application.DTOs;
using BlogApp.Application.Interfaces.Repositories;
using BlogApp.Domain.Entities;
using MediatR;

namespace BlogApp.Application.Features.Comments.Commands.CreateComment;

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, CommentDto>
{
    private readonly IGenericRepository<Comment> _commentRepository;
    private readonly IMapper _mapper;

    public CreateCommentCommandHandler(IGenericRepository<Comment> commentRepository, IMapper mapper)
    {
        _commentRepository = commentRepository;
        _mapper = mapper;
    }

    public async Task<CommentDto> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = _mapper.Map<Comment>(request);
        
        await _commentRepository.AddAsync(comment);
        
        return _mapper.Map<CommentDto>(comment);
    }
}