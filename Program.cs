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

            var firebaseConfigJson = Environment.GetEnvironmentVariable("FIREBASE_CONFIG");

            if (string.IsNullOrEmpty(firebaseConfigJson))
                throw new InvalidOperationException("FIREBASE_CONFIG environment variable is not set!");

            builder.Services.AddSingleton(FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromJson(firebaseConfigJson)
            }));

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

            app.Run();
        }
    }
}