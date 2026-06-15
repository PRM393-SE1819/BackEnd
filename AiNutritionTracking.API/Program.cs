using System.Text;
using AiNutritionTracking.API.Data;
using AiNutritionTracking.API.Models;
using AiNutritionTracking.API.Services;
using AiNutritionTracking.API.Services.Admin.FoodManagement;
using AiNutritionTracking.API.Services.Admin.UserManagement;
using AiNutritionTracking.API.Services.Cloudinary;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger with Bearer auth
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "AiNutritionTracking API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nh?p: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<AinutritiontrackingContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Application services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IHealthProfileService, HealthProfileService>();
builder.Services.AddScoped<IFoodService, FoodService>();
builder.Services.AddScoped<IMealService, MealService>();
builder.Services.AddScoped<INutritionService, NutritionService>();
builder.Services.AddScoped<IWaterService, WaterService>();
builder.Services.AddScoped<IWeightService, WeightService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IAdminFoodService, AdminFoodService>();
builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();

// In-memory cache (used for OTP storage)
builder.Services.AddMemoryCache();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var jwtKey = builder.Configuration["Jwt:Key"];
    var jwtIssuer = builder.Configuration["Jwt:Issuer"];
    var jwtAudience = builder.Configuration["Jwt:Audience"];
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = !string.IsNullOrEmpty(jwtIssuer),
        ValidateAudience = !string.IsNullOrEmpty(jwtAudience),
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey ?? string.Empty))
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = ctx =>
        {
            // lay id token
            var jti = ctx.Principal?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;
            if (!string.IsNullOrEmpty(jti))
            {
              //kt token cos nam trong danh sach den khong (memortcache)
                var cache = ctx.HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
                if (cache.TryGetValue($"revoked:{jti}", out _))
                {
                    ctx.Fail("Token revoked.");
                }
            }
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()   
              .AllowAnyMethod()  
              .AllowAnyHeader();  
    });
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AiNutritionTracking API v1");
});
app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed Admin
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AinutritiontrackingContext>();
    var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

    var adminEmail = configuration["AdminSeed:Email"]!;
    var adminPassword = configuration["AdminSeed:Password"]!;
    var adminFullName = configuration["AdminSeed:FullName"]!;

    if (!context.Roles.Any(r => r.RoleId == 1))
    {
        context.Roles.Add(new Role { RoleId = 1, RoleName = "Admin", Description = "Administrator", CreatedAt = DateTime.UtcNow });
        context.SaveChanges();
    }

    if (!context.Roles.Any(r => r.RoleId == 2))
    {
        context.Roles.Add(new Role { RoleId = 2, RoleName = "User", Description = "Normal User", CreatedAt = DateTime.UtcNow });
        context.SaveChanges();
    }

    if (!context.Users.Any(u => u.Email == adminEmail))
    {
        context.Users.Add(new User
        {
            Username = "admin",
            FullName = adminFullName,
            Email = adminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            RoleId = 1,
            EmailVerified = true,
            Status = "Active",
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        });
        context.SaveChanges();
    }
}

// Cấu hình nhận động PORT từ Render
var port = Environment.GetEnvironmentVariable("PORT") ?? "1000";
app.Urls.Add($"http://*:{port}");
app.Run();