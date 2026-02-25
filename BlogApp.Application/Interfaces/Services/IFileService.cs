using Microsoft.AspNetCore.Http;

namespace BlogApp.Application.Interfaces.Services;

public interface IFileService
{
    Task<string> UploadFileAsync(IFormFile file);
}