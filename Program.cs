using FileManager.DB;
using FileManager.DB.Manager;
using FileManager.Interfaces;
using FileManager.Services;
using Microsoft.EntityFrameworkCore;

namespace FileManager;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = long.MaxValue;
        });
        builder.Services.AddDbContext<FileManagerDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("FileManager")));
        builder.Services.AddControllers();
        builder.Services.AddRazorPages();
        builder.Services.AddHttpClient();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddTransient<FileManagerQueries>();
        builder.Services.AddTransient<IFileService, FileService>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapRazorPages();
        app.MapControllers();

        app.Run();
    }
}
