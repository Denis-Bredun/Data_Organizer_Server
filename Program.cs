using Data_Organizer_Server.Firestore_Converters;
using Data_Organizer_Server.Interfaces;
using Data_Organizer_Server.Repositories;
using Data_Organizer_Server.Services;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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
            var serviceProvider = builder.Services.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Initializing Firebase...");

            var firebaseConfigJson = Environment.GetEnvironmentVariable("FIREBASE_CONFIG");
            if (string.IsNullOrEmpty(firebaseConfigJson))
            {
                logger.LogCritical("FIREBASE_CONFIG is not set!");
                throw new InvalidOperationException("Environment variable FIREBASE_CONFIG is not set!");
            }

            InitializeFirebaseApp(builder, firebaseConfigJson, logger);
            ConfigureFirestore(builder, firebaseConfigJson, logger);

            logger.LogInformation("Firebase initialized successfully");
        }

        private static void InitializeFirebaseApp(WebApplicationBuilder builder, string firebaseConfigJson, ILogger logger)
        {
            logger.LogInformation("Initializing FirebaseApp...");
            var firebaseApp = FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromJson(firebaseConfigJson)
            });
            builder.Services.AddSingleton(firebaseApp);
            logger.LogInformation("FirebaseApp initialized successfully");
        }

        private static void ConfigureFirestore(WebApplicationBuilder builder, string firebaseConfigJson, ILogger logger)
        {
            logger.LogInformation("Initializing Firestore...");
            var firestoreDbBuilder = new FirestoreDbBuilder
            {
                ProjectId = "data-organizer-eaa8f",
                ConverterRegistry = new ConverterRegistry()
                {
                    new DateTimeTimestampConverter()
                },
                JsonCredentials = firebaseConfigJson
            };
            var firestoreDb = firestoreDbBuilder.Build();
            builder.Services.AddSingleton(firestoreDb);
            logger.LogInformation("Firestore initialized successfully");
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = "https://securetoken.google.com/data-organizer-eaa8f";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "https://securetoken.google.com/data-organizer-eaa8f",
                    ValidateAudience = true,
                    ValidAudience = "data-organizer-eaa8f",
                    ValidateLifetime = true
                };
            });
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            //services.AddSwaggerGen();
            services.AddAuthorization();
            services.AddHttpClient<IOpenAIService, OpenAIService>();
            services.AddScoped<IAzureService, AzureService>();
            services.AddScoped<ICollectionFactory, CollectionFactory>();
            services.AddScoped<IFirestoreDbService, FirestoreDbService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUsersMetadataRepository, UsersMetadataRepository>();
            services.AddScoped<INoteRepository, NoteRepository>();
            services.AddScoped<IDeviceInfoRepository, DeviceInfoRepository>();
            services.AddScoped<IChangePasswordRepository, ChangePasswordRepository>();
            services.AddScoped<IAccountLoginRepository, AccountLoginRepository>();
            services.AddScoped<IAccountLogoutRepository, AccountLogoutRepository>();
            services.AddScoped<IMappingService, MappingService>();
            services.AddScoped<IEncryptionService, EncryptionService>();
        }

        private static void ConfigureMiddleware(WebApplication app)
        {
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
        }
    }
}
