using AutoMapper;
using BlogApp.Application.Common;
using BlogApp.Application.DTOs;
using BlogApp.Application.Interfaces.Repositories;
using BlogApp.Domain.Entities;
using MediatR;

namespace BlogApp.Application.Features.Posts.Queries.GetAllPosts;

public class GetAllPostsQueryHandler : IRequestHandler<GetAllPostsQuery, PagedResult<PostDto>>
{
    private readonly IPostRepository _postRepository;
    private readonly IMapper _mapper;

    public GetAllPostsQueryHandler(IPostRepository postRepository, IMapper mapper)
    {
        _postRepository = postRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<PostDto>> Handle(GetAllPostsQuery request, CancellationToken cancellationToken)
    {

        var allPosts = await _postRepository.GetAllWithCategoryAsync();
        var queryablePosts = allPosts.Where(x => !x.IsDeleted).AsQueryable();

    
        var totalCount = queryablePosts.Count();
        
        var pagedPosts = queryablePosts
            .OrderByDescending(x => x.CreatedDate)
            .Skip((request.PageNumber - 1) * request.PageSize) 
            .Take(request.PageSize) 
            .ToList();
        
        var dtos = _mapper.Map<List<PostDto>>(pagedPosts);
        
        return new PagedResult<PostDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }
}