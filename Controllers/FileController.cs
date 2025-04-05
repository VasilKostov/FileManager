using Microsoft.AspNetCore.Mvc;
using FileManager.Utilities;
using FileManager.Interfaces;
using FileManager.Models.Responses;
using FileManager.Models.DTOs;
using FileManager.Utilities.Attributes;

namespace FileManager.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FileController : ControllerBase
{
    private readonly IFileService _fileService;
    public FileController(IFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost("upload")]
    [MultipartFormData]
    [DisableFormValueModelBinding]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status415UnsupportedMediaType)]
    public async Task<IActionResult> Upload()
    {
        UploadFilesResponse response = await _fileService.UploadFiles(Request.Body, Request.ContentType!);

        return Ok(response);
    }

    [HttpGet("get-files")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllFiles()
    {
        List<FileRecordDTO> files = await _fileService.GetFiles();

        return Ok(files);
    }

    [HttpDelete("delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteFile([FromQuery] int id)
    {
        if (id < 0)
        {
            return BadRequest("Invalid file ID.");
        }

        bool deleted = await _fileService.DeleteFile(id);
        if (deleted)
        {
            return Ok(new { Message = "File deleted successfully." });
        }

        return NotFound("File not found.");
    }

}
