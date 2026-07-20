using System.Text;
using HospitalApp.Data;
using HospitalApp.Repositories;
using HospitalApp.Repositories.Interfaces;
using HospitalApp.Services;
using HospitalApp.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace HospitalApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ─── 1. Database: EF Core + SQL Server ────────────────────────
            builder.Services.AddDbContext<HospitalDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("HospitalDB"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null)
                )
            );

            // ─── 2. Repository Layer ───────────────────────────────────────
            builder.Services.AddScoped<IPatientRepository, PatientRepository>();
            builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
            builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();

            // Task 2 — Stored Procedure repository
            builder.Services.AddScoped<IPatientSpRepository, PatientSpRepository>();

            // ─── 3. Service Layer ──────────────────────────────────────────
            builder.Services.AddScoped<IPatientService, PatientService>();
            builder.Services.AddScoped<IDoctorService, DoctorService>();
            builder.Services.AddScoped<IAppointmentService, AppointmentService>();

            // ─── 4. Auth Service ───────────────────────────────────────────
            builder.Services.AddScoped<IAuthService, AuthService>();

            // ─── 5. Controllers ────────────────────────────────────────────
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler =
                        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                });

            // ─── 6. JWT Authentication ─────────────────────────────────────
            var jwtConfig = builder.Configuration.GetSection("Jwt");
            var jwtKey    = Encoding.UTF8.GetBytes(jwtConfig["Key"]!);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = jwtConfig["Issuer"],
                    ValidAudience            = jwtConfig["Audience"],
                    IssuerSigningKey         = new SymmetricSecurityKey(jwtKey),
                    ClockSkew                = TimeSpan.Zero
                };
            });

            builder.Services.AddAuthorization();

            // ─── 7. Swagger / OpenAPI ──────────────────────────────────────
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title       = "Hospital Management System API",
                    Version     = "v1",
                    Description = "RESTful API for Patients, Doctors & Appointments. " +
                                  "ASP.NET Core 10 | EF Core 9 | SQL Server | JWT Auth. " +
                                  "Architecture: Controller → Service → Repository → DB."
                });

                // JWT Security definition for Swagger UI
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name         = "Authorization",
                    Type         = SecuritySchemeType.Http,
                    Scheme       = "bearer",
                    BearerFormat = "JWT",
                    In           = ParameterLocation.Header,
                    Description  = "Enter your JWT token below (without the 'Bearer ' prefix)."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id   = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // ─── 8. CORS ──────────────────────────────────────────────────
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularApp", policy =>
                {
                    policy.WithOrigins(
                            "http://localhost:4200",
                            "http://localhost:3000"
                          )
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // ─── 9. Logging ───────────────────────────────────────────────
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            // ══════════════════════════════════════════════════════════════
            var app = builder.Build();
            // ══════════════════════════════════════════════════════════════

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Hospital Management System API v1");
                    options.RoutePrefix    = "swagger";
                    options.DocumentTitle  = "HMS API — Swagger UI";
                    options.DisplayRequestDuration();
                });
            }

            // Only redirect to HTTPS in production — in development the Angular
            // app calls http://localhost:5011 and the self-signed cert causes failures.
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            app.UseCors("AllowAngularApp");

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

            // ─── Seed default admin account (runs once on first startup) ──
            await SeedAdminAsync(app);

            app.Run();
        }

        private static async Task SeedAdminAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<HospitalApp.Data.HospitalDbContext>();

            if (!db.AppUsers.Any())
            {
                db.AppUsers.Add(new HospitalApp.Models.AppUser
                {
                    Username     = "admin",
                    Email        = "admin@hospital.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@1234"),
                    Role         = "Admin",
                    CreatedAt    = DateTime.UtcNow
                });
                await db.SaveChangesAsync();
                Console.WriteLine("✅ Default admin account created: admin@hospital.com / Admin@1234");
            }
        }
    }
}
