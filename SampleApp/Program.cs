using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var version = Assembly.GetExecutingAssembly()
    .GetName()
    .Version?
    .ToString(3) ?? "unknown";

app.MapGet("/", () => new
{
    application = "Azure DevOps CI/CD",
    status = "Running",
    version,
    environment = app.Environment.EnvironmentName
});

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy"
}));

app.MapGet("/info", () => new
{
    application = "Azure DevOps CI/CD",
    version,
    description = "Laboratório de CI/CD com Azure DevOps"
});

app.Run();

public partial class Program { }
