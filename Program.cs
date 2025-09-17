using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuestLocalBackend.Data;
using QuestLocalBackend.Models;

namespace QuestLocalBackend
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Database connection
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // JSON settings to avoid circular reference issues
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });

            // ✅ CORS Setup for React/React Native Web/Expo
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins(
                            "http://localhost:3000",    // React App
                            "http://localhost:8081",    // React Native Web (Expo for Web)
                            "http://localhost:19006",   // Expo Dev Web
                            "http://192.168.1.8:19000", // Expo Go (Replace with your actual IP if needed)
                            "exp://localhost:19000"     // Expo Dev App (mobile)
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials(); // Optional for cookies, tokens, etc.
                });
            });

            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseStaticFiles();

            // Swagger for development
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Disable HTTPS redirection for local dev if needed
            // app.UseHttpsRedirection();

            // 🔥 Use the correct CORS policy
            app.UseCors("AllowFrontend");

            app.UseAuthorization();

            app.MapControllers();

            //// Seed test data into DB
            //using (var scope = app.Services.CreateScope())
            //{
            //    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            //    context.Database.EnsureCreated();

            //    // Seed Users
            //    if (!context.Users.Any())
            //    {
            //        context.Users.AddRange(
            //            new User { Username = "john_doe", PasswordHash = "hashed123", Email = "john@example.com", Coins = 1000, Tickets = 0, CreatedAt = DateTime.Now },
            //            new User { Username = "jane_smith", PasswordHash = "hashed456", Email = "jane@example.com", Coins = 1500, Tickets = 0, CreatedAt = DateTime.Now }
            //        );
            //        await context.SaveChangesAsync();
            //    }

            //    // Seed Quests
            //    if (!context.Quests.Any())
            //    {
            //        context.Quests.AddRange(
            //            new Quest
            //            {
            //                IssuerId = 1,
            //                Heading = "Clean the Park",
            //                Description = "Pick up trash in the local park.",
            //                CoinReward = 500,
            //                VerificationType = "Photo",
            //                DueDate = DateTime.Now.AddDays(7),
            //                IsActive = true,
            //                CreatedAt = DateTime.Now
            //            },
            //            new Quest
            //            {
            //                IssuerId = 2,
            //                Heading = "Plant Trees",
            //                Description = "Plant 10 trees in the community garden.",
            //                CoinReward = 700,
            //                VerificationType = "PDF",
            //                DueDate = DateTime.Now.AddDays(10),
            //                IsActive = true,
            //                CreatedAt = DateTime.Now
            //            }
            //        );
            //        await context.SaveChangesAsync();
            //    }
            //}


            // Seed test data into DB
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureCreated();

                // Seed Users first
                if (!context.Users.Any())
                {
                    var user1 = new User
                    {
                        Username = "john_doe",
                        PasswordHash = "hashed123",
                        Email = "john@example.com",
                        Coins = 1000,
                        Tickets = 0,
                        CreatedAt = DateTime.Now
                    };
                    var user2 = new User
                    {
                        Username = "jane_smith",
                        PasswordHash = "hashed456",
                        Email = "jane@example.com",
                        Coins = 1500,
                        Tickets = 0,
                        CreatedAt = DateTime.Now
                    };

                    context.Users.AddRange(user1, user2);
                    await context.SaveChangesAsync(); // Save users first to get their IDs

                    // Now seed Quests using the actual user objects
                    if (!context.Quests.Any())
                    {
                        context.Quests.AddRange(
                            new Quest
                            {
                                Issuer = user1, // Use the actual user object
                                Heading = "Clean the Park",
                                Description = "Pick up trash in the local park.",
                                CoinReward = 500,
                                VerificationType = "Photo",
                                DueDate = DateTime.Now.AddDays(7),
                                IsActive = true,
                                CreatedAt = DateTime.Now
                            },
                            new Quest
                            {
                                Issuer = user2, // Use the actual user object
                                Heading = "Plant Trees",
                                Description = "Plant 10 trees in the community garden.",
                                CoinReward = 700,
                                VerificationType = "PDF",
                                DueDate = DateTime.Now.AddDays(10),
                                IsActive = true,
                                CreatedAt = DateTime.Now
                            }
                        );
                        await context.SaveChangesAsync();
                    }
                }
                else if (!context.Quests.Any())
                {
                    // If users already exist but quests don't, get the first two users
                    var existingUsers = context.Users.Take(2).ToList();
                    if (existingUsers.Count >= 2)
                    {
                        context.Quests.AddRange(
                            new Quest
                            {
                                Issuer = existingUsers[0],
                                Heading = "Clean the Park",
                                Description = "Pick up trash in the local park.",
                                CoinReward = 500,
                                VerificationType = "Photo",
                                DueDate = DateTime.Now.AddDays(7),
                                IsActive = true,
                                CreatedAt = DateTime.Now
                            },
                            new Quest
                            {
                                Issuer = existingUsers[1],
                                Heading = "Plant Trees",
                                Description = "Plant 10 trees in the community garden.",
                                CoinReward = 700,
                                VerificationType = "PDF",
                                DueDate = DateTime.Now.AddDays(10),
                                IsActive = true,
                                CreatedAt = DateTime.Now
                            }
                        );
                        await context.SaveChangesAsync();
                    }
                }
            }
            // Force backend to run at port 5000
            app.Urls.Add("http://localhost:5000");
            app.Run();
        }
    }
}
