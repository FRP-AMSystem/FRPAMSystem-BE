using FRPAMSystem.BusinessTier;
using FRPAMSystem.BusinessTier.SignalR;
using FRPAMSystem.DataTier;
using FRPAMSystem_BE.Extensions;
using FRPAMSystem_BE.Filters;
using System.Text.Json.Serialization;
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers(options =>
    {
        options.Filters.Add<AuditLogActionFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddConfigSwagger();

builder.Services.AddJwtValidation(builder.Configuration);

builder.Services.AddDataServices(builder.Configuration);
builder.Services.AddBusinessServices(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .SetIsOriginAllowed(origin => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddSignalR();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Bật CORS trước khi map Hub và Controllers
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notification");

app.Run();

public partial class Program { }