using IdeasToVote.Api.Data;
using IdeasToVote.Api.Constants;
using IdeasToVote.Api.DTOs;
using IdeasToVote.Api.Services;
using IdeasToVote.Api.Settings;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register JWT settings and authentication services
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IIdeaService, IdeaService>();

// Configure JWT authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
var key = Encoding.UTF8.GetBytes(jwtSettings?.Secret ?? "your-super-secret-key-should-be-at-least-32-characters-long-for-hs256");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings?.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings?.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "IdeasToVote API";
    });
}

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (exceptionFeature is not null)
        {
            var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("GlobalExceptionHandler");
            logger.LogError(exceptionFeature.Error, "Unhandled exception while processing request.");
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new ApiMessageResponse
        {
            Message = ApiMessages.UnexpectedError
        });
    });
});

app.UseStatusCodePages(async statusCodeContext =>
{
    var response = statusCodeContext.HttpContext.Response;

    if (response.HasStarted || response.ContentLength.HasValue)
    {
        return;
    }

    var message = response.StatusCode switch
    {
        StatusCodes.Status400BadRequest => ApiMessages.BadRequest,
        StatusCodes.Status401Unauthorized => ApiMessages.Unauthorized,
        StatusCodes.Status403Forbidden => ApiMessages.Forbidden,
        StatusCodes.Status404NotFound => ApiMessages.NotFound,
        _ => null
    };

    if (message is null)
    {
        return;
    }

    await response.WriteAsJsonAsync(new ApiMessageResponse
    {
        Message = message
    });
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Ensure DB schema is aligned with migrations and seed baseline data.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
    DataSeeder.Seed(dbContext);
}

app.MapGet("/", () => Results.Ok("IdeasToVote API is running."));
app.MapControllers();

app.Run();
