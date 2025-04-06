using FileManager;
using FileManager.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace FileManager.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<IFileService> FileServiceMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(IFileService));
            services.AddSingleton(FileServiceMock.Object);
        });
    }
}
