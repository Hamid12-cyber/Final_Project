using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;
using MediatR;
using MotoHM.Api.Modules.Motorcycles;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();   
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

CreateMotorcycle.MapEndpoint(app);
GetAllMotorcycles.MapEndpoint(app);
GetMotorcycleById.MapEndpoint(app);
UpdateMotorcycle.MapEndpoint(app);
DeleteMotorcycle.MapEndpoint(app);

app.Run();

