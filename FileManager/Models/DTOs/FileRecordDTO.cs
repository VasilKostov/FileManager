namespace FileManager.Models.DTOs;

public class FileRecordDTO
{
    public FileRecordDTO(int id, string name, string extension, string size)
    {
        Id = id;
        Name = name;
        Extension = extension;
        Size = size;
    }
    public int Id { get; set; }
    public string Name { get; set; }
    public string Extension { get; set; }
    public string Size { get; set; }
}