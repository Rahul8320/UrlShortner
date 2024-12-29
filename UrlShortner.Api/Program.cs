using UrlShortner.Api.Data;
using UrlShortner.Api.Services;
using UrlShortner.Api.Services.Interface;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddScoped<IUrlShortnerService, UrlShortnerService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
