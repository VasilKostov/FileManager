using FileManager.DB.Entities;
using FileManager.Helpers;
using FileManager.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Net.WebSockets;

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

    public async Task<List<FileRecordDTO>> GetFileRecords()
    {
        return await _context.FileRecords.Select(x => new FileRecordDTO(x.Id, x.Name, x.Extension, Helper.GetMb(x.Size))).ToListAsync();
    }

    public async Task<List<string>> GetNamesAndExtentions(string name)
    {
        return await _context.FileRecords.Where(x => name == x.Name + '.' + x.Extension).Select(y => y.Name + "|" + y.Extension).ToListAsync();
    }

    public async Task<bool> DeleteFileRecord(int id)
    {
        var res = await _context.FileRecords.Where(x => x.Id == id).Select(x=> new { x.FilePath, x.Name}).FirstOrDefaultAsync();
        if (string.IsNullOrEmpty(res?.Name))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(res.FilePath))
        {
            File.Delete(res.FilePath);
        }

        FileRecord fileRecord = new() { Id = id };

        _context.FileRecords.Attach(fileRecord);
        _context.FileRecords.Remove(fileRecord);

        await _context.SaveChangesAsync();

        return true;
    }
}
