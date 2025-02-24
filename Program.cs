using Data_Organizer_Server.Interfaces;
using Data_Organizer_Server.Services;
using FirebaseAdmin;
using FirebaseAdminAuthentication.DependencyInjection.Extensions;
using Google.Apis.Auth.OAuth2;

namespace Data_Organizer_Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.AddDebug();

            using var loggerFactory = LoggerFactory.Create(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });

            var logger = loggerFactory.CreateLogger<Program>();
            logger.LogInformation("Початок ініціалізації...");

            var firebaseConfigJson = Environment.GetEnvironmentVariable("FIREBASE_CONFIG");
            logger.LogInformation("Отримано Firebase конфіг з змінної середовища");

            if (string.IsNullOrEmpty(firebaseConfigJson))
            {
                logger.LogCritical("FIREBASE_CONFIG не встановлено!");
                throw new InvalidOperationException("Змінна середовища FIREBASE_CONFIG не встановлена!");
            }

            logger.LogInformation("Ініціалізація Firebase...");
            builder.Services.AddSingleton(FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromJson(firebaseConfigJson)
            }));
            logger.LogInformation("Firebase ініціалізовано");

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddAuthorization();
            builder.Services.AddFirebaseAuthentication();
            builder.Services.AddScoped<IOpenAIService, OpenAIService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Logger.LogInformation("Запуск сервера...");
            app.Run();
        }
    }
}