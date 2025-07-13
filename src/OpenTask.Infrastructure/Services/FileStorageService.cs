using Microsoft.Extensions.Configuration;
using OpenTask.Application.Interfaces;

namespace OpenTask.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly string _uploadPath;
    private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".txt" };

    public FileStorageService(IConfiguration configuration)
    {
        _uploadPath = configuration["FileStorage:UploadPath"] ?? "uploads";
        Directory.CreateDirectory(_uploadPath);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        if (fileStream.Length > _maxFileSize)
            throw new InvalidOperationException("File size exceeds maximum allowed size");

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            throw new InvalidOperationException("File type not allowed");

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(_uploadPath, uniqueFileName);

        using var fileStreamOutput = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(fileStreamOutput);

        return uniqueFileName;
    }

    public async Task<Stream> DownloadFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_uploadPath, filePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("File not found");

        return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_uploadPath, filePath);
        if (!File.Exists(fullPath))
            return false;

        File.Delete(fullPath);
        return true;
    }
}
