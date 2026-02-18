using AutoMapper;
using BlogApp.Application.DTOs;
using BlogApp.Application.Features.Comments.Commands.CreateComment;
using BlogApp.Application.Features.Posts.Commands.CreatePost;
using BlogApp.Domain.Entities;

namespace BlogApp.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Post, PostDto>().ReverseMap();

        CreateMap<CreatePostCommand, Post>();

        CreateMap<Comment, CommentDto>().ReverseMap();

        CreateMap<CreateCommentCommand, Comment>();
        
        CreateMap<Post, PostDetailDto>();
    }
}