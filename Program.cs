//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.OpenApi.Models;
//using QuestLocalBackend.Data;
//using System.Text;

//namespace QuestLocalBackend
//{
//    public class Program
//    {
//        public static async Task Main(string[] args)
//        {
//            var builder = WebApplication.CreateBuilder(args);

//            // =========================
//            // Database
//            // =========================
//            builder.Services.AddDbContext<ApplicationDbContext>(options =>
//                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//            // =========================
//            // Controllers / JSON
//            // =========================
//            builder.Services.AddControllers()
//                .AddJsonOptions(o =>
//                {
//                    o.JsonSerializerOptions.ReferenceHandler =
//                        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
//                });

//            // =========================
//            // CORS
//            // =========================
//            builder.Services.AddCors(options =>
//            {
//                options.AddPolicy("AllowFrontend", policy =>
//                {
//                    policy.WithOrigins(
//                            "http://localhost:3000",
//                            "http://localhost:8081",
//                            "http://localhost:19006",
//                            "http://192.168.1.8:19000",
//                            "exp://localhost:19000"
//                        )
//                        .AllowAnyHeader()
//                        .AllowAnyMethod();
//                });
//            });

//            // =========================
//            // JWT Auth
//            // =========================
//            var jwtSection = builder.Configuration.GetSection("Jwt");
//            var jwtKey = jwtSection.GetValue<string>("Key");
//            var jwtIssuer = jwtSection.GetValue<string>("Issuer");
//            var jwtAudience = jwtSection.GetValue<string>("Audience");

//            if (string.IsNullOrWhiteSpace(jwtKey))
//                throw new InvalidOperationException("Missing configuration: Jwt:Key");
//            if (string.IsNullOrWhiteSpace(jwtIssuer))
//                throw new InvalidOperationException("Missing configuration: Jwt:Issuer");
//            if (string.IsNullOrWhiteSpace(jwtAudience))
//                throw new InvalidOperationException("Missing configuration: Jwt:Audience");

//            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

//            builder.Services
//                .AddAuthentication(options =>
//                {
//                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//                })
//                .AddJwtBearer(options =>
//                {
//                    options.TokenValidationParameters = new TokenValidationParameters
//                    {
//                        ValidateIssuer = true,
//                        ValidateAudience = true,
//                        ValidateLifetime = true,
//                        ValidateIssuerSigningKey = true,
//                        ValidIssuer = jwtIssuer,
//                        ValidAudience = jwtAudience,
//                        IssuerSigningKey = signingKey
//                    };
//                });

//            builder.Services.AddAuthorization();

//            // =========================
//            // Swagger (+ Bearer)
//            // =========================
//            builder.Services.AddEndpointsApiExplorer();
//            builder.Services.AddSwaggerGen(c =>
//            {
//                c.SwaggerDoc("v1", new OpenApiInfo { Title = "QuestLocal API", Version = "v1" });
//                var scheme = new OpenApiSecurityScheme
//                {
//                    Name = "Authorization",
//                    Type = SecuritySchemeType.Http,
//                    Scheme = "bearer",
//                    BearerFormat = "JWT",
//                    In = ParameterLocation.Header,
//                    Description = "Enter: Bearer {your JWT}"
//                };
//                c.AddSecurityDefinition("Bearer", scheme);
//                c.AddSecurityRequirement(new OpenApiSecurityRequirement { { scheme, Array.Empty<string>() } });
//            });

//            var app = builder.Build();

//            // =========================
//            // Middleware
//            // =========================
//            // app.UseHttpsRedirection(); // optional in local dev
//            app.UseStaticFiles();
//            app.UseCors("AllowFrontend");

//            if (app.Environment.IsDevelopment())
//            {
//                app.UseSwagger();
//                app.UseSwaggerUI();
//            }

//            app.UseAuthentication();   // must be before UseAuthorization
//            app.UseAuthorization();

//            app.MapControllers();

//            // =========================
//            // (Optional) DB seed hook
//            // =========================
//            using (var scope = app.Services.CreateScope())
//            {
//                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
//                context.Database.EnsureCreated();
//                // seed if needed...
//            }

//            app.Urls.Add("http://localhost:5000");
//            app.Run();
//        }
//    }
//}
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestLocalBackend.Data;
using System.Text;

namespace QuestLocalBackend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =============================
            // Database
            // =============================
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // =============================
            // Controllers / JSON
            // =============================
            builder.Services.AddControllers()
                .AddJsonOptions(o =>
                {
                    // prevent circular refs in EF Core nav properties
                    o.JsonSerializerOptions.ReferenceHandler =
                        System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });

            // =============================
            // CORS (allow your local frontends)
            // =============================
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(
                            "http://localhost:3000",
                            "http://localhost:8081",
                            "http://localhost:19006",
                            "http://192.168.1.8:19000",
                            "exp://localhost:19000"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // =============================
            // JWT Authentication
            // =============================
            var jwtSection = builder.Configuration.GetSection("Jwt");
            var jwtKey = jwtSection.GetValue<string>("Key");
            var jwtIssuer = jwtSection.GetValue<string>("Issuer");
            var jwtAudience = jwtSection.GetValue<string>("Audience");

            if (string.IsNullOrWhiteSpace(jwtKey))
                throw new InvalidOperationException("Missing configuration: Jwt:Key");
            if (string.IsNullOrWhiteSpace(jwtIssuer))
                throw new InvalidOperationException("Missing configuration: Jwt:Issuer");
            if (string.IsNullOrWhiteSpace(jwtAudience))
                throw new InvalidOperationException("Missing configuration: Jwt:Audience");

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    // Dev-friendly (no HTTPS required) — do NOT use in production
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtIssuer,         // must equal token 'iss'
                        ValidAudience = jwtAudience,       // must equal token 'aud'
                        IssuerSigningKey = signingKey,

                        ClockSkew = TimeSpan.Zero             // no grace period on exp
                    };
                });

            builder.Services.AddAuthorization();

            // =============================
            // Swagger (+ Bearer support)
            // =============================
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "QuestLocal API",
                    Version = "v1"
                });

                var bearerScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Paste your token as: **Bearer {your_jwt_here}**"
                };

                c.AddSecurityDefinition("Bearer", bearerScheme);
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { bearerScheme, Array.Empty<string>() }
                });
            });

            var app = builder.Build();

            // =============================
            // Middleware order
            // =============================
            // app.UseHttpsRedirection(); // optional for local dev
            app.UseStaticFiles();
            app.UseCors("AllowFrontend");

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();   // MUST come before authorization
            app.UseAuthorization();

            app.MapControllers();

            // =============================
            // Ensure DB exists / optional seeding
            // =============================
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureCreated();
                // Add seeding here if needed
            }

            app.Urls.Add("http://localhost:5000");
            app.Run();
        }
    }
}

