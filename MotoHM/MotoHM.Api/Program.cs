using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MotoHM.Api.Data;
using MotoHM.Api.Data.Interceptors;
using MotoHM.Api.Modules.Accessories;
using MotoHM.Api.Modules.Admin;
using MotoHM.Api.Modules.Cart;
using MotoHM.Api.Modules.Motorcycles;
using MotoHM.Api.Modules.Orders;
using MotoHM.Api.Modules.PartCategories;
using MotoHM.Api.Modules.Parts;
using MotoHM.Api.Modules.Rentals;
using MotoHM.Api.Modules.Service;
using MotoHM.Api.Modules.Testimonials;
using MotoHM.Api.Modules.Users;
using MotoHM.Api.Shared.Behaviors;
using MotoHM.Api.Shared.Handlers;
using Serilog;
using System.Reflection;
using System.Text;

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
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Token-i belə yaz: Bearer {sənin tokenin}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ---- Database ----
builder.Services.AddSingleton<BackupWriteInterceptor>();

builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
    options.AddInterceptors(sp.GetRequiredService<BackupWriteInterceptor>());
});

builder.Services.AddDbContext<BackupDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Backup")));

builder.Services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
builder.Services.AddScoped<IDbConnectionFactory, SqlConnectionFactory>();

// ---- MediatR ----
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// ---- Validation ----
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

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

// ---- Postgres backup DB-nin cədvəllərini (yoxdursa) avtomatik yarat ----
using (var scope = app.Services.CreateScope())
{
    var backupDb = scope.ServiceProvider.GetRequiredService<BackupDbContext>();
    backupDb.Database.EnsureCreated();
}

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
GetPendingAccessories.MapEndpoint(app);
ApproveAccessory.MapEndpoint(app);
RejectAccessory.MapEndpoint(app);

// Auth / Users
Register.MapEndpoint(app);
Login.MapEndpoint(app);

// Cart
GetMyCart.MapEndpoint(app);
AddToCart.MapEndpoint(app);
UpdateCartItemQuantity.MapEndpoint(app);
RemoveFromCart.MapEndpoint(app);

// Orders
CreateOrder.MapEndpoint(app);
GetMyOrders.MapEndpoint(app);
GetOrderById.MapEndpoint(app);
UpdateOrderStatus.MapEndpoint(app);

// Accessories
GetAllAccessories.MapEndpoint(app);
GetAccessoryById.MapEndpoint(app);
CreateAccessory.MapEndpoint(app);
UpdateAccessory.MapEndpoint(app);
DeleteAccessory.MapEndpoint(app);

app.Run();