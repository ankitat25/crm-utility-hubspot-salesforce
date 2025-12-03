using CrmUtility.Backend.Data;
using CrmUtility.Backend.Options;
using CrmUtility.Backend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Http client and custom services
builder.Services.AddHttpClient();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<HubSpotAuthService>();
builder.Services.AddScoped<HubSpotCrmService>();
builder.Services.AddScoped<SalesforceAuthService>();
builder.Services.AddScoped<SalesforceCrmService>();

// Bind HubSpot options from appsettings.json
builder.Services.Configure<HubSpotOptions>(
    builder.Configuration.GetSection("HubSpot"));

builder.Services.Configure<SalesforceOptions>(
    builder.Configuration.GetSection("Salesforce"));


// SQL Server Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
