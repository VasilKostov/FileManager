namespace FileManager.Models.DTOs;

public class FileRecordDTO
{
    public FileRecordDTO(string name, string extention, string size)
    {
        Name = name;
        Extention = extention;
        Size = size;
    }
    public string Name { get; set; }
    public string Extention { get; set; }
    public string Size { get; set; }
}