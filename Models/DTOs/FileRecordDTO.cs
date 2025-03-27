namespace FileManager.Models.DTOs;

public class FileRecordDTO
{
    public FileRecordDTO(string name, string extension, string size)
    {
        Name = name;
        Extension = extension;
        Size = size;
    }
    public string Name { get; set; }
    public string Extension { get; set; }
    public string Size { get; set; }
}