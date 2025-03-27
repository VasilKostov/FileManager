using FileManager.DB.Entities;
using FileManager.DB.Manager;
using FileManager.Interfaces;
using FileManager.Models.DTOs;
using FileManager.Models.Responses;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace FileManager.Services;

public class FileService : IFileService
{
    private readonly FileManagerQueries _db;
    
    private const double BytesInKb = 1024;
    private const double BytesInMb = 1024 * 1024;
    private const double BytesInGb = 1024 * 1024 * 1024;

    public FileService(FileManagerQueries fileManagerQueries)
    {
        _db = fileManagerQueries;
    }

    public async Task<List<FileRecordDTO>> GetFiles()
    {
        List<FileRecord> files = await _db.GetFileRecords();
        List<FileRecordDTO> result = new();

        foreach (FileRecord file in files)
        {
            result.Add(new(file.Name, file.Extension, GetMb(file.Size)));
        }

        return result;
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

        string fileDirectory = Path.Combine("C:\\Uploads");
        if (!Directory.Exists(fileDirectory))
        {
            Directory.CreateDirectory(fileDirectory);
        }

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

            if (memoryStream.Length >= 2 * BytesInGb)
            {
                filePath = Path.Combine(fileDirectory, fileSection.FileName);

                using FileStream fileStream = new(filePath, FileMode.Create);
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

        await _db.InsertFileRecords(files);

        return new UploadFilesResponse(fileCount, GetMb(totalSize), notUploadedFiles);
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

    private string GetMb(long size)
    {
        if (size < BytesInMb)
        {
            double sizeInKb = size / BytesInKb;

            return $"{sizeInKb:F2} KB";
        }
        else
        {
            double sizeInMb = size / BytesInMb;

            return $"{sizeInMb:F2} MB";
        }
    }
}
