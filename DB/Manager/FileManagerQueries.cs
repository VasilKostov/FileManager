using FileManager.DB.Entities;
using FileManager.Helpers;
using FileManager.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace FileManager.DB.Manager;

public class FileManagerQueries
{
    private readonly FileManagerDbContext _context;

    public FileManagerQueries(FileManagerDbContext context)
    {
        _context = context;
    }

    public async Task InsertFileRecords(List<FileRecord> files)
    {
        await _context.FileRecords.AddRangeAsync(files);

        await _context.SaveChangesAsync();
    }

    public async Task RemoveFileRecords(List<string> names)
    {
        List<FileRecord> files = await _context.FileRecords.Where(x => names.Contains(x.Name + '|' + x.Extension)).ToListAsync();

        _context.FileRecords.RemoveRange(files);

        await _context.SaveChangesAsync();
    }

    public async Task<List<FileRecordDTO>> GetFileRecords()
    {
        return await _context.FileRecords.Select(x=> new FileRecordDTO(x.Name, x.Extension, Helper.GetMb(x.Size))).ToListAsync();
    }

    public async Task<List<string>> GetNamesAndExtentions(string name)
    {
        return await _context.FileRecords.Where(x => name == x.Name + '.' + x.Extension).Select(y => y.Name + "|" + y.Extension).ToListAsync();
    }
}
