using AutoMapper;
using BlogApp.Application.DTOs;
using BlogApp.Application.Interfaces.Repositories;
using MediatR;

namespace BlogApp.Application.Features.Posts.Queries.GetPostDetail;

public class GetPostDetailQueryHandler : IRequestHandler<GetPostDetailQuery, PostDetailDto>
{
    private readonly IPostRepository _postRepository;
    private readonly IMapper _mapper;

    public GetPostDetailQueryHandler(IPostRepository postRepository, IMapper mapper)
    {
        _postRepository = postRepository;
        _mapper = mapper;
    }

    public async Task<PostDetailDto> Handle(GetPostDetailQuery request, CancellationToken cancellationToken)
    {
        var post = await _postRepository.GetByIdWithCommentsAsync(request.PostId);

        if (post == null) 
            throw new Exception("Post no exists."); 

        return _mapper.Map<PostDetailDto>(post);
    }
}