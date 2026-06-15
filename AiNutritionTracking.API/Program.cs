using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using AiNutritionTracking.API.Data;
using AiNutritionTracking.API.DTOs.AI;
using AiNutritionTracking.API.Helpers;
using AiNutritionTracking.API.Repositories;
using AiNutritionTracking.API.Services;
using AiNutritionTracking.API.Validators.AI;
using FluentValidation;
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
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
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

// Configure Groq settings
builder.Services.Configure<GroqSettings>(builder.Configuration.GetSection("GroqSettings"));
builder.Services.AddHttpClient<IAIService, AIService>();
builder.Services.Configure<PexelsSettings>(builder.Configuration.GetSection("PexelsSettings"));
builder.Services.AddHttpClient<IPexelsService, PexelsService>();

// AI Repository
builder.Services.AddScoped<IAIRepository, AIRepository>();

// FluentValidation
builder.Services.AddScoped<IValidator<ChatRequestDto>, ChatRequestValidator>();
builder.Services.AddScoped<IValidator<CalorieEstimateRequestDto>, CalorieEstimateRequestValidator>();
builder.Services.AddScoped<IValidator<MealRecommendationRequestDto>, MealRecommendationRequestValidator>();
builder.Services.AddScoped<IValidator<MealPlanRequestDto>, MealPlanRequestValidator>();
builder.Services.AddScoped<IValidator<BodyFatImageRequestDto>, BodyFatImageRequestValidator>();
builder.Services.AddScoped<IValidator<BodyFatMeasurementRequestDto>, BodyFatMeasurementRequestValidator>();

// Rate limiting — 10 requests per minute per user/IP on AI endpoints
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("ai-policy", cfg =>
    {
        cfg.PermitLimit         = 10;
        cfg.Window              = TimeSpan.FromMinutes(1);
        cfg.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        cfg.QueueLimit          = 2;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

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
builder.Services.AddHttpClient<IBodyFatAnalysisService, BodyFatAnalysisService>();

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
            // L?y ID c?a Token (JTI)
            var jti = ctx.Principal?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;
            if (!string.IsNullOrEmpty(jti))
            {
                // Ki?m tra xem Token này có n?m trong "Danh sách ?en" (MemoryCache) không
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
    //c.RoutePrefix = string.Empty;
});

app.UseCors("AllowAll");
//app.UseHttpsRedirection();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// 2. CẤU HÌNH NHẬN ĐỘNG PORT TỪ RENDER
var port = Environment.GetEnvironmentVariable("PORT") ?? "1000";
app.Urls.Add($"http://*:{port}");

app.Run();
