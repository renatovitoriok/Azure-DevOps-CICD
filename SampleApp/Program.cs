var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => new
{
    application = "Azure DevOps CI/CD",
    status = "Running",
    version = "1.0.0",
    environment = app.Environment.EnvironmentName
});

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy"
}));

app.MapGet("/info", () => new
{
    application = "Azure DevOps CI/CD",
    version = "1.0.0",
    description = "Laboratório de CI/CD com Azure DevOps"
});

app.Run();