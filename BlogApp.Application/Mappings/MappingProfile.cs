using AutoMapper;
using BlogApp.Application.DTOs;
using BlogApp.Application.Models.Requests;
using BlogApp.Domain.Entities;

namespace BlogApp.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Post, PostDto>().ReverseMap();
        CreateMap<CreatePostRequest, Post>();
        CreateMap<Comment, CommentDto>().ReverseMap();
        CreateMap<CreateCommentRequest, Comment>();
        CreateMap<Post, PostDetailDto>();
        CreateMap<Category, CategoryDto>().ReverseMap();
        CreateMap<Post, PostDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category == null ? string.Empty : src.Category.Name));
    }
}