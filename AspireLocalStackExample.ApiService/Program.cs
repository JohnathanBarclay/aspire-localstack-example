using AspireLocalStackExample.ApiService;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.Configure<AWSResources>(builder.Configuration.GetSection("AWS:Resources"));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapDefaultEndpoints();

app.MapGet("aws-resources", (IOptions<AWSResources> awsResources) => 
{
    return awsResources.Value;
});

app.Run();