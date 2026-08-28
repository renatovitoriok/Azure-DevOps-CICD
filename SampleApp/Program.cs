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

app.Run();

public partial class Program { }