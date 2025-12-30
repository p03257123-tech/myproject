using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WGCCI.Accounting.Api.Data;
using WGCCI.Accounting.Api.Services;
using WGCCI.Accounting.Api.Swagger;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine("=== PROGRAM.CS LOADED ===");
Console.WriteLine("ENV: " + builder.Environment.EnvironmentName);


// --------------------
// Config
// --------------------
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-secret";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "wgcci";

// Prefer appsettings ConnectionStrings:DefaultConnection.
// If missing, allow DATABASE_URL as a fallback.
// Finally, fall back to LocalDB.
var connStr =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["DATABASE_URL"]
    ?? "Server=(localdb)\\MSSQLLocalDB;Database=WGCCIAccounting;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

Console.WriteLine("USING CONNECTION STRING: " + connStr);

// --------------------
// Services
// --------------------
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(connStr));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false, // not enforced in the current API
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// App services
builder.Services.AddScoped<AccountingService>();
builder.Services.AddScoped<BankService>();
builder.Services.AddScoped<FXService>();
builder.Services.AddScoped<TaxService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WGCCI Accounting API", Version = "v1" });

    // Fix IFormFile + [FromForm] in Swagger generation
    c.OperationFilter<FileUploadOperationFilter>();

    // JWT Bearer support in Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
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

var app = builder.Build();

// --------------------
// Auto-migrate DB on startup (safe)
// --------------------

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("Startup");

    try
    {
        logger.LogInformation("Attempting DB migration...");
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        logger.LogInformation("got required service db");
        db.Database.Migrate();
        logger.LogInformation("DB migration complete.");
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "DB migration failed — API will still start.");
    }
}



if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "WGCCI Accounting API v1");
        // Optional: show Swagger UI at root instead of /swagger
        //c.RoutePrefix = string.Empty;
    });
}

// If you don't have HTTPS set up locally, you can comment this out.
// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
