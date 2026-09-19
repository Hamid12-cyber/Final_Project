using Microsoft.EntityFrameworkCore;
using MotoHM.Api.Data;
using MediatR;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MotoHM.Api.Shared.Handlers;
using Serilog;

using MotoHM.Api.Modules.Motorcycles;
using MotoHM.Api.Modules.PartCategories;
using MotoHM.Api.Modules.Parts;
using MotoHM.Api.Modules.Testimonials;
using MotoHM.Api.Modules.Rentals;
using MotoHM.Api.Modules.Service;
using MotoHM.Api.Modules.Admin;
using MotoHM.Api.Modules.Users;
using MotoHM.Api.Modules.Cart;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// ---- Swagger / OpenAPI ----
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---- Database ----
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();

// ---- MediatR ----
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// ---- Exception handling ----
builder.Services.AddExceptionHandler<AppExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ---- CORS ----
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ---- JWT Authentication ----
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"]!;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// ---- Pipeline ----
app.UseExceptionHandler();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

// ---- Endpoints ----

// Motorcycles
CreateMotorcycle.MapEndpoint(app);
GetAllMotorcycles.MapEndpoint(app);
GetMotorcycleById.MapEndpoint(app);
UpdateMotorcycle.MapEndpoint(app);
DeleteMotorcycle.MapEndpoint(app);

// PartCategories
GetAllPartCategories.MapEndpoint(app);
GetPartCategoryById.MapEndpoint(app);
CreatePartCategory.MapEndpoint(app);
UpdatePartCategory.MapEndpoint(app);
DeletePartCategory.MapEndpoint(app);

// Parts
GetAllParts.MapEndpoint(app);
GetPartById.MapEndpoint(app);
CreatePart.MapEndpoint(app);
UpdatePart.MapEndpoint(app);
DeletePart.MapEndpoint(app);

// Testimonials
GetAllTestimonials.MapEndpoint(app);
CreateTestimonial.MapEndpoint(app);
DeleteTestimonial.MapEndpoint(app);

// Rentals
GetAllRentals.MapEndpoint(app);
GetRentalById.MapEndpoint(app);
CreateRental.MapEndpoint(app);
UpdateRental.MapEndpoint(app);
DeleteRental.MapEndpoint(app);

// Service
GetAllServiceBookings.MapEndpoint(app);
GetServiceBookingById.MapEndpoint(app);
CreateServiceBooking.MapEndpoint(app);
UpdateServiceBooking.MapEndpoint(app);
DeleteServiceBooking.MapEndpoint(app);
GetAvailableSlots.MapEndpoint(app);

// Admin (approval workflow)
GetPendingMotorcycles.MapEndpoint(app);
ApproveMotorcycle.MapEndpoint(app);
RejectMotorcycle.MapEndpoint(app);
GetPendingParts.MapEndpoint(app);
ApprovePart.MapEndpoint(app);
RejectPart.MapEndpoint(app);

// Auth / Users
Register.MapEndpoint(app);
Login.MapEndpoint(app);

// Cart
GetMyCart.MapEndpoint(app);
AddToCart.MapEndpoint(app);
UpdateCartItemQuantity.MapEndpoint(app);
RemoveFromCart.MapEndpoint(app);

app.Run();