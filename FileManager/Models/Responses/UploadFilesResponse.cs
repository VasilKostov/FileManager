namespace FileManager.Models.Responses;

public class UploadFilesResponse
{
    public UploadFilesResponse(int totalFilesUploaded, string totalSizeUploaded, List<string> notUploadedFiles)
    {
        NotUploadedFiles = notUploadedFiles;
        TotalFilesUploaded = totalFilesUploaded;
        TotalSizeUploaded = totalSizeUploaded;
    }

    public int TotalFilesUploaded { get; set; }
    public string TotalSizeUploaded { get; set; }
    public List<string> NotUploadedFiles { get; set; }

}
