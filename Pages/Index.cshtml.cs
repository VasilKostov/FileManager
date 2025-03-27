using Azure.Core;
using FileManager.Models.DTOs;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace FileManager.Pages
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        public List<FileRecordDTO> Files { get; set; }

        public IndexModel(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public async Task OnGetAsync()
        {
            await RefreshFiles();
        }

        public async Task RefreshFiles()
        {
            // Call your API to get the files list
            var response = await _httpClient.GetAsync("https://localhost:7105/api/file/get-files");

            if (response.IsSuccessStatusCode)
            {
                Files = await response.Content.ReadFromJsonAsync<List<FileRecordDTO>>();
            }
            else
            {
                Files = new List<FileRecordDTO>();
            }
        }

        public async Task<IActionResult> OnPostUploadAsync()
        {
            var file = Request.Form.Files.FirstOrDefault();

            if (file == null)
            {
                return BadRequest("No file uploaded.");
            }

            var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.FileName);

            var response = await _httpClient.PostAsync("https://localhost:7105/api/file/upload", content);

            if (response.IsSuccessStatusCode)
            {
                await RefreshFiles();
                return RedirectToPage();
            }

            return StatusCode((int)response.StatusCode, "Failed to upload file.");
        }

        public async Task<IActionResult> OnPostDeleteFileAsync(string fileName)
        {
            var response = await _httpClient.DeleteAsync($"/api/file/delete?fileName={fileName}");

            if (response.IsSuccessStatusCode)
            {
                await RefreshFiles();
                return RedirectToPage();
            }

            return StatusCode((int)response.StatusCode, "Failed to delete file.");
        }

        public async Task<IActionResult> OnPostDownloadFileAsync(string fileName)
        {
            var response = await _httpClient.GetAsync($"/api/file/download?fileName={fileName}");

            if (response.IsSuccessStatusCode)
            {
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                var fileNameFromHeader = response.Content.Headers.ContentDisposition?.FileNameStar;

                // Triggering download using JS interop
                await _jsRuntime.InvokeVoidAsync("downloadFile", fileBytes, fileNameFromHeader);
                return Page();
            }

            return StatusCode((int)response.StatusCode, "Failed to download file.");
        }
    }
}
