using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Zonit.Extensions.Databases.Examples.Data;
using Zonit.Extensions.Databases.Examples.Repositories;
using Microsoft.Extensions.Configuration;
using Zonit.Extensions.Databases.Examples.Backgrounds;
using Zonit.Extensions.Databases.Examples.Extensions;
using Zonit.Extensions.Databases.Examples.Entities;

namespace Zonit.Extensions.Databases.Examples;

internal class Program
{
    public static IConfiguration CreateConfiguration(string[] args)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
#if DEBUG
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
#else
        .AddJsonFile("appsettings.release.json", optional: true, reloadOnChange: true)
#endif
            .AddCommandLine(args);

        var configuration = builder.Build();

#if DEBUG
        if (!File.Exists("appsettings.json"))
        {
            throw new FileNotFoundException("Nie znaleziono pliku ustawień appsettings.json.");
        }
#else
        if (!File.Exists("appsettings.release.json"))
        {
            throw new FileNotFoundException("Nie znaleziono pliku ustawień appsettings.release.json.");
        }
#endif

        return configuration;
    }

    static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            Args = args
        });

        builder.Configuration.AddConfiguration(CreateConfiguration(args));

        //builder.Services.AddLogs();

        builder.Services.AddDbSqlServer<DatabaseContext>();

        builder.Services.AddHostedService<DatabaseInitialize>();

        builder.Services.AddTransient<IBlogRepository, BlogRepository>();
        builder.Services.AddTransient<IBlogsRepository, BlogsRepository>();

        builder.Services.AddHostedService<BlogBackground>();
        builder.Services.AddHostedService<BlogsBackground>();

        builder.Services.AddHostedService<BlogExtensionBackground>();
        builder.Services.AddScoped<IDatabaseExtension<UserModel>, UserExtension>();
        builder.Services.AddScoped<IDatabaseExtension<OrganizationModel>, OrganizationExtension>();

        var app = builder.Build();
        app.Run();
    }
}