using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FileManager.DB.Entities;

public class FileRecord
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Extension { get; set; } = string.Empty;

    [Required]
    public long Size { get; set; }

    [Required]
    public DateTime UploadDate { get; set; }

    public byte[]? FileContent { get; set; }

    [MaxLength(254)]
    public string? FilePath { get; set; }
}

