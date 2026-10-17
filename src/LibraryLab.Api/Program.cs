using LibraryLab.Api.Middleware;
using LibraryLab.Infrastructure;
using LibraryLab.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
// La ejecución académica usa la consola y no requiere permisos sobre el registro de eventos de Windows.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<LibraryLab.Application.Services.LibraryApplicationService>();
builder.Services.AddLibraryLabInfrastructure(builder.Configuration);

var app = builder.Build();
app.UseMiddleware<ApiExceptionMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
if (app.Configuration.GetValue<bool>("Database:InitializeOnStartup"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var database = scope.ServiceProvider.GetRequiredService<LibraryLabDbContext>();
    await DatabaseInitializer.InitializeAsync(database);
}
app.Run();

public partial class Program { }
