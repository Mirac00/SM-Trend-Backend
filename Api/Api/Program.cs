using Api.Authorization;
using Api.Helpers;
using Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Add services to DI container
{
    var services = builder.Services;
    var env = builder.Environment;

    // Configure to handle large files
    services.Configure<FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 1073741824; // 1GB
    });

    // Use SQL Server DB in production and SQLite DB in development
    if (env.IsProduction())
        services.AddDbContext<DataContext>();
    else
        services.AddDbContext<DataContext, SqliteDataContext>();

    // Configure CORS
    services.AddCors(options =>
    {
        options.AddPolicy("AllowAllOrigins", builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    });

    services.AddControllers();

    // Configure AutoMapper with all AutoMapper profiles from this assembly
    services.AddAutoMapper(typeof(Program));

    // Configure strongly typed settings object
    services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

    // Configure DI for application services
    services.AddScoped<IJwtUtils, JwtUtils>();
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<IPostService, PostService>();

    // Add authentication services
    services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["AppSettings:Secret"])),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

    // Add Swagger generation
    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

        // Add security definition and requirement
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
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
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header
                },
                new List<string>()
            }
        });
    });
}

var app = builder.Build();

// Migrate any database changes on startup (includes initial db creation)
using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<DataContext>();
    dataContext.Database.Migrate();
}

// Configure HTTP request pipeline
{
    // Global CORS policy
    app.UseCors("AllowAllOrigins");

    // Global error handler
    app.UseMiddleware<ErrorHandlerMiddleware>();

    // Enable routing
    app.UseRouting();

    // Enable authentication and authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Custom JWT auth middleware
    app.UseMiddleware<JwtMiddleware>();

    // Add Swagger middleware
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = "swagger";
        c.DocExpansion(DocExpansion.None);
    });

    // Enable endpoints
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
}

app.Run();
