using Data_Organizer_Server.Interfaces;
using Data_Organizer_Server.Services;
using FirebaseAdmin;
using FirebaseAdminAuthentication.DependencyInjection.Extensions;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.DependencyInjection;

namespace Data_Organizer_Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureLogging(builder);
            ConfigureFirebase(builder);
            ConfigureServices(builder.Services);

            var app = builder.Build();

            ConfigureMiddleware(app);

            app.Logger.LogInformation("Starting the server...");
            app.Run();
        }

        private static void ConfigureLogging(WebApplicationBuilder builder)
        {
            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();
        }

        private static void ConfigureFirebase(WebApplicationBuilder builder)
        {
            var logger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Initializing Firebase...");
            
            var firebaseConfigJson = Environment.GetEnvironmentVariable("FIREBASE_CONFIG");
            if (string.IsNullOrEmpty(firebaseConfigJson))
            {
                logger.LogCritical("FIREBASE_CONFIG is not set!");
                throw new InvalidOperationException("Environment variable FIREBASE_CONFIG is not set!");
            }

            builder.Services.AddSingleton(FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromJson(firebaseConfigJson)
            }));
            
            builder.Services.AddFirebaseAuthentication();
            logger.LogInformation("Firebase initialized successfully");
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddAuthorization();
            services.AddScoped<IOpenAIService, OpenAIService>();
            services.AddScoped<IAzureService, AzureService>();
            services.AddScoped<IPayPalService, PayPalService>();
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
        }
    }
}
