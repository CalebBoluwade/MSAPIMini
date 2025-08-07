using System.Net.Http.Headers;
using Asp.Versioning;
using FluentValidation;
using FluentValidation.AspNetCore;
using MS.API.Mini.Configuration;
using MS.API.Mini.Contracts;
using MS.API.Mini.Data;
using MS.API.Mini.Data.Models;
using MS.API.Mini.Data.Models.Validations;
using MS.API.Mini.Extensions;
using MS.API.Mini.Middleware;
using MS.API.Mini.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:7104");

builder.Host.UseSerilog((_, _, lc) => lc.ReadFrom.Configuration(new ConfigurationBuilder()
    .AddJsonFile("Serilog.json")
    .Build()));

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddDbContext<MonitorDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<AgentConfiguration>(builder.Configuration.GetSection(nameof(AgentConfiguration)));

builder.Services.AddOptions<AgentConfiguration>()
    .Bind(builder.Configuration.GetSection("AgentConfiguration"))
    .ValidateDataAnnotations()
    .Validate(config => !string.IsNullOrEmpty(config.GitToken))
    .Validate(config => config.LicenseOptions is { Count: > 0 })
    .ValidateOnStart();

builder.Services.AddScoped<IAnsibleDeploymentService, AnsibleDeploymentService>();
builder.Services.AddHttpClient<GitHubService>(x =>
{
    // Configure HttpClient for GitHub API
    x.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
    x.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("MS", "1.0"));

    // Add GitHub token if available
    var githubToken = builder.Configuration["Git:Token"];
    if (string.IsNullOrEmpty(githubToken)) throw new ArgumentException("GitHub token is missing");
    if (!string.IsNullOrEmpty(githubToken))
    {
        x.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", githubToken);
    }
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("*",
        api =>
        {
            api.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1);
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-API-VERSION")
    );
}).AddMvc().AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddScoped<IValidator<SystemMonitor>, SystemMonitorDTOValidator>();

// builder.Services.Scan(scan => scan
//     .fromAssemblyOf<Program>()
//     .AddClasses()
//     .AsMatchingInterface()
//     .WithScopedLifetime()
// );

builder.Services.AddScoped<IActiveDirectoryService, ActiveDirectoryService>();
builder.Services.AddScoped<IAgentContract, AgentContractor>();
builder.Services.AddScoped<IDBContract, DBContractor>();

// Add Microsoft Graph
builder.Services.AddMicrosoftGraph(builder.Configuration);

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<MonitorDBContext>();
        await DatabaseInitializer.SeedPlugins(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// Configure the HTTP request pipeline.
app.UseStaticFiles();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "API v2");
    });
}

app.UseCors("*");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseSerilogRequestLogging();

await app.RunAsync();