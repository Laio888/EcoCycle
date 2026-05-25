using asp_servicios;

using System.IO;

var builder = WebApplication.CreateBuilder(args);

var rootSecretsPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "Secrets.json"));
if (File.Exists(rootSecretsPath))
{
    builder.Configuration.AddJsonFile(rootSecretsPath, optional: true, reloadOnChange: true);
}

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder, builder.Services);

var app = builder.Build();
startup.Configure(app, app.Environment);

app.Run();
