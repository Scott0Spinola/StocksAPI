using src.Data;
using src.Services;
using src.Services.EntradasSaidasService;
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
        .Enrich.FromLogContext()
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.File(
            Path.Combine(logDirectory, "log-.txt"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 7);
});

builder.Services.AddDbContext<StocksContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions =>
        {
            sqlServerOptions.EnableRetryOnFailure();
        }));

builder.Services.AddScoped<Cliente_Movimento_Services>();
builder.Services.AddScoped<Cliente_Tag_Services>();
builder.Services.AddScoped<EntradasSaidasService>();
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

Log.Information(
    "Starting {Application} in {Environment}; logs at {LogDirectory}",
    app.Environment.ApplicationName,
    app.Environment.EnvironmentName,
    Path.Combine(app.Environment.ContentRootPath, "Logs"));


app.UseStatusCodePages();
app.UseExceptionHandler();
app.UseSerilogRequestLogging();

// Apply EF migrations only when explicitly enabled.
// This avoids trying to recreate tables when you point at an existing database.
if (app.Configuration.GetValue<bool>("Database:ApplyMigrations"))
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

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}

