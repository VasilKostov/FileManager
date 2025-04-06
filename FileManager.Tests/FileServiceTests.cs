using FileManager.DB;
using FileManager.DB.Entities;
using FileManager.DB.Manager;
using FileManager.Services;
using FileManager.Utilities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Serialization;
using System.Text;

namespace FileManager.Tests
{
    public class FileServiceTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly FileManagerDbContext _dbContext;
        private readonly FileService _fileService;

        public FileServiceTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            var dbOptions = new DbContextOptionsBuilder<FileManagerDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;
            _dbContext = new FileManagerDbContext(dbOptions);
            _fileService = new FileService(new FileManagerQueries(_dbContext), Options.Create(new Configuration { UploadPath = "C:\\Temp" }));
        }

        [Fact]
        public async Task GetFiles_ReturnsCorrectFileRecords()
        {
            // Arrange
            var fileRecord1 = new FileRecord { Name = "File1", Extension = "txt", Size = 1024, UploadDate = DateTime.UtcNow };
            var fileRecord2 = new FileRecord { Name = "File2", Extension = "jpg", Size = 2048, UploadDate = DateTime.UtcNow };

            _dbContext.FileRecords.AddRange(fileRecord1, fileRecord2);
            await _dbContext.SaveChangesAsync();

            // Act
            var files = await _fileService.GetFiles();

            // Assert
            Assert.NotEmpty(files);
            Assert.Equal(2, files.Count);
            Assert.Contains(files, f => f.Name == "File1" && f.Extension == "txt");
            Assert.Contains(files, f => f.Name == "File2" && f.Extension == "jpg");
        }

        [Fact]
        public async Task UploadFiles_ValidFiles_ReturnsCorrectResponse()
        {
            // Arrange
            var boundary = "----WebKitFormBoundary7MA4YWxkTrZu0gW";
            var fileName = "test.txt";
            var fileContent = "Hello World!";
            var contentType = $"multipart/form-data; boundary={boundary}";

            var multipartBody = $@"
--{boundary}
Content-Disposition: form-data; name=""files""; filename=""{fileName}""
Content-Type: text/plain

{fileContent}
--{boundary}--";

            var bodyBytes = Encoding.UTF8.GetBytes(multipartBody.Trim());
            using var stream = new MemoryStream(bodyBytes);

            // Act
            var result = await _fileService.UploadFiles(stream, contentType);

            // Assert
            Assert.Equal(1, result.TotalFilesUploaded);
            Assert.Empty(result.NotUploadedFiles);
        }

        [Fact]
        public async Task UploadFiles_ValidFiles_ReturnsCorrectResponse_ForTwoFiles()
        {
            // Arrange
            var boundary = "----WebKitFormBoundary7MA4YWxkTrZu0gW";
            var fileName1 = "test1.txt";
            var fileContent1 = "Hello World 1!";
            var fileName2 = "test2.txt";
            var fileContent2 = "Hello World 2!";
            var contentType = $"multipart/form-data; boundary={boundary}";

            var multipartBody = $@"
--{boundary}
Content-Disposition: form-data; name=""files""; filename=""{fileName1}""
Content-Type: text/plain

{fileContent1}
--{boundary}
Content-Disposition: form-data; name=""files""; filename=""{fileName2}""
Content-Type: text/plain

{fileContent2}
--{boundary}--";

            var bodyBytes = Encoding.UTF8.GetBytes(multipartBody.Trim());
            using var stream = new MemoryStream(bodyBytes);

            // Act
            var result = await _fileService.UploadFiles(stream, contentType);

            // Assert
            Assert.Equal(2, result.TotalFilesUploaded);
            Assert.Empty(result.NotUploadedFiles);
        }

        [Fact]
        public async Task UploadFiles_ValidFiles_ReturnsCorrectResponse_ForTwoFilesButOneExists()
        {
            // Arrange
            var fileRecord1 = new FileRecord { Name = "test1", Extension = "txt", Size = 1024, UploadDate = DateTime.UtcNow };

            _dbContext.FileRecords.AddRange(fileRecord1);
            await _dbContext.SaveChangesAsync();

            var boundary = "----WebKitFormBoundary7MA4YWxkTrZu0gW";
            var fileName1 = "test3.txt";
            var fileContent1 = "Hello World 1!";
            var fileName2 = "test1.txt";
            var fileContent2 = "Hello World 2!";
            var contentType = $"multipart/form-data; boundary={boundary}";

            var multipartBody = $@"
--{boundary}
Content-Disposition: form-data; name=""files""; filename=""{fileName1}""
Content-Type: text/plain

{fileContent1}
--{boundary}
Content-Disposition: form-data; name=""files""; filename=""{fileName2}""
Content-Type: text/plain

{fileContent2}
--{boundary}--";

            var bodyBytes = Encoding.UTF8.GetBytes(multipartBody.Trim());
            using var stream = new MemoryStream(bodyBytes);

            // Act
            var result = await _fileService.UploadFiles(stream, contentType);

            // Assert
            Assert.Equal(1, result.TotalFilesUploaded);
            Assert.Equal(new List<string>() { "test1.txt" }, result.NotUploadedFiles);
        }

        [Fact]
        public async Task DeleteFile_ValidId_ReturnsTrue()
        {
            // Arrange
            var fileRecord = new FileRecord { Name = "FileToDelete", Extension = "txt", Size = 1024, UploadDate = DateTime.UtcNow };
            _dbContext.FileRecords.Add(fileRecord);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _fileService.DeleteFile(fileRecord.Id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteFile_InvalidId_ReturnsFalse()
        {
            // Act
            var result = await _fileService.DeleteFile(9999);

            // Assert
            Assert.False(result);
        }

        public void Dispose()
        {
            _dbContext.FileRecords.RemoveRange(_dbContext.FileRecords);
            _dbContext.SaveChanges();
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}
