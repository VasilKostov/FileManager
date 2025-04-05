using FileManager.DB;
using FileManager.DB.Manager;
using FileManager.Interfaces;
using FileManager.Services;
using FileManager.Utilities;
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
        builder.Services.AddHttpClient();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddTransient<FileManagerQueries>();
        builder.Services.AddTransient<IFileService, FileService>();

        builder.Services.Configure<Configuration>(builder.Configuration.GetSection("Configuration"));

        Configuration? config = builder.Configuration
                            .GetSection("Configuration")
                            .Get<Configuration>();

        if (!string.IsNullOrEmpty(config?.UploadPath) && !Directory.Exists(config.UploadPath))
        {
            Directory.CreateDirectory(config.UploadPath);
        }

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
