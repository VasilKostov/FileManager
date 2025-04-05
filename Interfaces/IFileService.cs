
using FileManager.Models.DTOs;
using FileManager.Models.Responses;

namespace FileManager.Interfaces;

public interface IFileService
{
    Task<UploadFilesResponse> UploadFiles(Stream stream, string contentType);
    Task<List<FileRecordDTO>> GetFiles();
    Task<bool> DeleteFile(int id);
}
