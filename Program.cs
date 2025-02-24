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
            logger.LogInformation("������� ������������...");

            var firebaseConfigJson = Environment.GetEnvironmentVariable("FIREBASE_CONFIG");
            logger.LogInformation("�������� Firebase ������ � ����� ����������");

            if (string.IsNullOrEmpty(firebaseConfigJson))
            {
                logger.LogCritical("FIREBASE_CONFIG �� �����������!");
                throw new InvalidOperationException("����� ���������� FIREBASE_CONFIG �� �����������!");
            }

            logger.LogInformation("������������ Firebase...");
            builder.Services.AddSingleton(FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromJson(firebaseConfigJson)
            }));
            logger.LogInformation("Firebase �������������");

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

            app.Logger.LogInformation("������ �������...");
            app.Run();
        }
    }
}