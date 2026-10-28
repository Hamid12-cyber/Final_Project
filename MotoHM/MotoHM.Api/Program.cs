using Serilog;
using Microsoft.OpenApi;
using MotoHM.Api.Modules.Accessories;
using MotoHM.Api.Modules.Admin;
using MotoHM.Api.Modules.Cart;
using MotoHM.Api.Modules.Motorcycles;
using MotoHM.Api.Modules.Orders;
using MotoHM.Api.Modules.PartCategories;
using MotoHM.Api.Modules.Parts;
using MotoHM.Api.Modules.Rentals;
using MotoHM.Api.Modules.Seller;
using MotoHM.Api.Modules.Service;
using MotoHM.Api.Modules.Testimonials;
using MotoHM.Api.Modules.Uploads;
using MotoHM.Api.Modules.Users;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using MotoHM.Api.Shared.Handlers;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using MotoHM.Api.Modules.Uploads;
using MotoHM.Api.Modules.Reviews;
using MotoHM.Api.Modules.Favorites;
using Microsoft.AspNetCore.HttpOverrides;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// ---- Swagger ----
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    // Bütün enum-lar (UserRole, OrderStatus, RentalPeriod, ServiceType və s.) JSON-da
    // rəqəm (1,2,3) yox, oxunaqlı ad ("Customer","Pending" və s.) kimi göndərilir/qəbul edilir.
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddSwaggerGen(c =>
{
    // Eyni adlı nested tiplər (RejectPart.RejectBody, RejectMotorcycle.RejectBody və s.)
    // Swagger-də schemaId toqquşması yaradırdı -> tam ad ilə fərqləndiririk.
    c.CustomSchemaIds(type => type.FullName!.Replace("+", "."));

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
            new List<string>()
        }
    });
});

// ---- Database ----
builder.Services.AddSingleton<BackupWriteInterceptor>();
builder.Services.AddSingleton<IEmailSender, SmtpEmailSender>();

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

// ---- Reverse proxy (Nginx / Azure / Render / Railway ...) arxasında düzgün scheme və client IP üçün ----
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    // Proxy-nin IP-si əvvəlcədən məlum deyil (PaaS), ona görə default etibarlı siyahını təmizləyirik.
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// ---- CORS ----
// Frontend:BaseUrl bir və ya vergüllə ayrılmış bir neçə ünvan ola bilər:
// "https://motohm.az,https://www.motohm.az"
var frontendOrigins = (builder.Configuration["Frontend:BaseUrl"] ?? "http://localhost:5173")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Select(o => o.TrimEnd('/'))
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(frontendOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});


// ---- JWT Authentication ----
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSettings["Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
    throw new InvalidOperationException(
        "Jwt:Key təyin olunmayıb və ya 32 simvoldan qısadır. " +
        "dotnet user-secrets set \"Jwt:Key\" \"...\" ilə təyin et.");

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

// ---- Rate limiting (brute-force qarşısı: auth endpoint-ləri üçün IP başına dəqiqədə 10 sorğu) ----
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    // Açıq (login tələb etməyən) yazma endpoint-ləri üçün spam qarşısı: IP başına dəqiqədə 5 sorğu
    options.AddPolicy("public-write", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var app = builder.Build();

// ---- Əsas baza migration-larını avtomatik tətbiq et (Database:AutoMigrate=true olduqda) ----
// Prod-da env ilə: Database__AutoMigrate=true
if (app.Configuration.GetValue<bool>("Database:AutoMigrate"))
{
    using var migrateScope = app.Services.CreateScope();
    var mainDb = migrateScope.ServiceProvider.GetRequiredService<AppDbContext>();
    mainDb.Database.Migrate();
    app.Logger.LogInformation("Əsas baza migration-ları tətbiq olundu.");
}

// ---- İlk Admin istifadəçisi (Admin:Email və Admin:Password verilibsə və belə admin yoxdursa) ----
{
    var adminEmail = app.Configuration["Admin:Email"];
    var adminPassword = app.Configuration["Admin:Password"];
    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
    {
        using var seedScope = app.Services.CreateScope();
        var db = seedScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var normalized = adminEmail.Trim().ToLowerInvariant();
        var exists = await db.Set<MotoHM.Api.Entites.UserEntity>()
            .AnyAsync(u => u.Email.ToLower() == normalized);
        if (!exists)
        {
            db.Set<MotoHM.Api.Entites.UserEntity>().Add(new MotoHM.Api.Entites.UserEntity
            {
                FullName = app.Configuration["Admin:FullName"] ?? "Admin",
                Email = normalized,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                Role = MotoHM.Api.Entites.Enums.UserRole.Admin
            });
            await db.SaveChangesAsync();
            app.Logger.LogInformation("İlk admin istifadəçisi yaradıldı: {Email}", normalized);
        }
    }
}

// ---- Postgres backup DB-nin cədvəllərini (yoxdursa) avtomatik yarat ----
using (var scope = app.Services.CreateScope())
{
    var backupDb = scope.ServiceProvider.GetRequiredService<BackupDbContext>();
    try
    {
        backupDb.Database.Migrate();
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Postgres backup bazası əlçatmazdır, migration tətbiq olunmadı.");
    }
}

// ---- Pipeline ----
app.UseForwardedHeaders();

// Development-də http və https ikisi də işləsin; production-da http -> https yönləndirilir
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseExceptionHandler();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

// Uploads:Path ilə persistent volume göstərmək olar (məs. /data/uploads)
var uploadsPath = app.Configuration["Uploads:Path"] ?? Path.Combine(app.Environment.ContentRootPath, "uploads");
Directory.CreateDirectory(uploadsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// ---- Endpoints ----

// Health check (deploy platformaları üçün)
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).WithTags("Health");

// Motorcycles
CreateMotorcycle.MapEndpoint(app);
GetAllMotorcycles.MapEndpoint(app);
GetMotorcycleFilters.MapEndpoint(app);
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
GetBookedRentalDates.MapEndpoint(app);
GetMyListings.MapEndpoint(app);
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
GetAllOrders.MapEndpoint(app);
GetOrderByIdAdmin.MapEndpoint(app);
GetAdminStats.MapEndpoint(app);
ResyncBackup.MapEndpoint(app);

// Auth / Users
Register.MapEndpoint(app);
Login.MapEndpoint(app);
ForgotPassword.MapEndpoint(app);
ResetPassword.MapEndpoint(app);
UpdateAvatar.MapEndpoint(app);

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
CancelOrder.MapEndpoint(app);

// Accessories
GetAllAccessories.MapEndpoint(app);
GetAccessoryById.MapEndpoint(app);
CreateAccessory.MapEndpoint(app);
UpdateAccessory.MapEndpoint(app);
DeleteAccessory.MapEndpoint(app);

// Uploads
UploadImage.MapEndpoint(app);

// Reviews
CreateOrUpdateReview.MapEndpoint(app);
GetReviews.MapEndpoint(app);
DeleteReview.MapEndpoint(app);

// Favorites
AddFavorite.MapEndpoint(app);
RemoveFavorite.MapEndpoint(app);
GetMyFavorites.MapEndpoint(app);


app.Run();