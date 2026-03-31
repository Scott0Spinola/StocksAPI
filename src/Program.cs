using src.Data;
using src.Services;
using src.Auth;

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

using Serilog;
using System.Reflection;



var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    var logDirectory = Path.Combine(context.HostingEnvironment.ContentRootPath, "Logs");
    Directory.CreateDirectory(logDirectory);

    loggerConfiguration
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.File(
            Path.Combine(logDirectory, "log-.txt"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7);
});

builder.Services.AddDbContext<StocksContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<Cliente_Movimento_Services>();
builder.Services.AddScoped<Cliente_Tag_Services>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ErrorHandling>();


builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services
    .AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationHandler.SchemeName,
        _ => { });

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(ApiKeyAuthenticationHandler.SchemeName)
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddSwaggerGen(options =>
{
    var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFileName);

    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
      options.AddSecurityDefinition(ApiKeyAuthenticationHandler.SchemeName, new OpenApiSecurityScheme
    {
        Description = $"API Key via header {ApiKeyAuthenticationHandler.HeaderName}",
        Type = SecuritySchemeType.ApiKey,
        Name = ApiKeyAuthenticationHandler.HeaderName,
        In = ParameterLocation.Header
    });

    options.AddSecurityRequirement(hostDocument => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference(
                ApiKeyAuthenticationHandler.SchemeName,
                hostDocument,
                externalResource: null),
            new List<string>()
        }
    });
});
















var app = builder.Build();


app.UseStatusCodePages();
app.UseExceptionHandler();

// Ensure SQLite schema exists (esp. in Docker volumes) during development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<StocksContext>();
    db.Database.Migrate();
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireAuthorization();

app.Run();

