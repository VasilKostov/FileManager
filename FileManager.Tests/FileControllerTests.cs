using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FileManager.Models.DTOs;
using FileManager.Models.Responses;
using FluentAssertions;
using Moq;
using Xunit;

namespace FileManager.Tests;

public class FileControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public FileControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAllFiles_ReturnsListOfFileRecords()
    {
        // Arrange
        var expected = new List<FileRecordDTO>
        {
            new(1, "TestFile", "txt", "1.5 MB")
        };

        _factory.FileServiceMock
            .Setup(s => s.GetFiles())
            .ReturnsAsync(expected);

        // Act
        var response = await _client.GetAsync("/api/file/get-files");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<List<FileRecordDTO>>();
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result![0].Name.Should().Be("TestFile");
        result[0].Extension.Should().Be("txt");
        result[0].Size.Should().Be("1.5 MB");
    }

    [Fact]
    public async Task DeleteFile_ReturnsOk_WhenFileIsDeleted()
    {
        // Arrange
        _factory.FileServiceMock
            .Setup(s => s.DeleteFile(1))
            .ReturnsAsync(true);

        // Act
        var response = await _client.DeleteAsync("/api/file/delete?id=1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteFile_ReturnsNotFound_WhenFileDoesNotExist()
    {
        // Arrange
        _factory.FileServiceMock
            .Setup(s => s.DeleteFile(999))
            .ReturnsAsync(false);

        // Act
        var response = await _client.DeleteAsync("/api/file/delete?id=999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteFile_ReturnsBadRequest_WhenIdIsNegative()
    {
        // Act
        var response = await _client.DeleteAsync("/api/file/delete?id=-5");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Upload_ReturnsOk_WhenFilesAreUploaded()
    {
        // Arrange
        var mockResponse = new UploadFilesResponse(
            totalFilesUploaded: 2,
            totalSizeUploaded: "2.4 MB",
            notUploadedFiles: ["badfile.exe"]);

        _factory.FileServiceMock
            .Setup(s => s.UploadFiles(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync(mockResponse);

        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(System.Text.Encoding.UTF8.GetBytes("Fake content"));
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
        content.Add(fileContent, "files", "test1.txt");

        // Act
        var response = await _client.PostAsync("/api/file/upload", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UploadFilesResponse>();

        result.Should().NotBeNull();
        result!.TotalFilesUploaded.Should().Be(2);
        result.TotalSizeUploaded.Should().Be("2.4 MB");
        result.NotUploadedFiles.Should().Contain("badfile.exe");
    }
}
