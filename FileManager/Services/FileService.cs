using FileManager.DB.Entities;
using FileManager.DB.Manager;
using FileManager.Helpers;
using FileManager.Interfaces;
using FileManager.Models.DTOs;
using FileManager.Models.Responses;
using FileManager.Utilities;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace FileManager.Services;

public class FileService : IFileService
{
    private readonly FileManagerQueries _db;
    private readonly string _uploadRootFolder;

    public FileService(FileManagerQueries fileManagerQueries, IOptions<Configuration> _cfg)
    {
        _db = fileManagerQueries;
        _uploadRootFolder = _cfg.Value.UploadPath;
    }

    public async Task<List<FileRecordDTO>> GetFiles()
    {
        return await _db.GetFileRecords();
    }

    public async Task<UploadFilesResponse> UploadFiles(Stream stream, string contentType)
    {
        int fileCount = 0;
        long totalSize = 0;

        string boundary = GetBoundary(MediaTypeHeaderValue.Parse(contentType));
        MultipartReader reader = new(boundary, stream);
        MultipartSection? section;
        List<string> notUploadedFiles = new();
        List<FileRecord> files = new();

        while ((section = await reader.ReadNextSectionAsync()) is not null)
        {
            FileMultipartSection? fileSection = section.AsFileSection();

            if (fileSection is null)
            {
                continue;
            }

            string[] splittedName = fileSection.FileName.Split('.', StringSplitOptions.RemoveEmptyEntries);

            if (splittedName.Length <= 1) // No separation
            {
                notUploadedFiles.Add(fileSection.FileName);

                continue;
            }

            bool isValidName = false;
            string currName = splittedName[0];
            string currExtention = string.Empty;
            List<string> existingCombination = await _db.GetNamesAndExtentions(fileSection.FileName);

            for (int i = 0; i < splittedName.Length - 1; i++)
            {
                currExtention = fileSection.FileName.Substring(currName.Length + 1);

                if (!existingCombination.Contains(currName + '|' + currExtention))
                {
                    isValidName = true;
                    break;
                }

                currName = currName + '.' + splittedName[i + 1];
            }

            if (!isValidName)
            {
                notUploadedFiles.Add(fileSection.FileName);

                section = await reader.ReadNextSectionAsync();
                continue;
            }


            if (fileSection.FileStream is null)
            {
                notUploadedFiles.Add(fileSection.FileName);

                continue;
            }

            using var memoryStream = new MemoryStream();
            await fileSection.FileStream.CopyToAsync(memoryStream);

            if (memoryStream.Length is 0)
            {
                notUploadedFiles.Add(fileSection.FileName);

                continue;
            }

            totalSize += memoryStream.Length;
            fileCount++;

            string filePath = string.Empty;

            if (memoryStream.Length >= 50 * Helper.BytesInMb)
            {
                filePath = Path.Combine(_uploadRootFolder, fileSection.FileName);

                using FileStream fileStream = new(filePath, FileMode.Create);

                memoryStream.Position = 0;

                await memoryStream.CopyToAsync(fileStream);
            }

            files.Add(new FileRecord
            {
                Name = currName,
                Extension = currExtention,
                Size = memoryStream.Length,
                UploadDate = DateTime.UtcNow,
                FileContent = filePath == string.Empty ? memoryStream.ToArray() : null,
                FilePath = filePath
            });
        }

        int batchSize = 50;
        for (int i = 0; i < files.Count; i += batchSize)
        {
            List<FileRecord> batch = files.Skip(i).Take(batchSize).ToList();
            await _db.InsertFileRecords(batch);
        }

        return new UploadFilesResponse(fileCount, Helper.GetMb(totalSize), notUploadedFiles);
    }

    public async Task<bool> DeleteFile(int id)
    {
        return await _db.DeleteFileRecord(id);
    }

    private string GetBoundary(MediaTypeHeaderValue mediaTypeHeaderValue)
    {
        string? boundary = HeaderUtilities.RemoveQuotes(mediaTypeHeaderValue.Boundary).Value;

        if (string.IsNullOrWhiteSpace(boundary))
        {
            throw new InvalidDataException("Missing boundary");
        }

        return boundary;
    }
}
