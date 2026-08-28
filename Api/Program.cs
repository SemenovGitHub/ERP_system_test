using Api.Middleware;
using Application;
using Repository;
using Repository.Mongo;

namespace Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddApplication();
        builder.Services.AddMongoRepositories(builder.Configuration);
        builder.Services.AddAutoMapper(
            typeof(Api.AutoMapperProfile),
            typeof(Application.AutoMapperProfile),
            typeof(Repository.AutoMapperProfile));
        builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = InvalidModelStateFactory.Create;
            });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var cors = builder.Configuration
            .GetSection(CorsSettings.SectionName)
            .Get<CorsSettings>()
            ?? new CorsSettings();

        if (cors.AllowedOrigins is not { Length: > 0 })
        {
            throw new InvalidOperationException(
                "Секция Cors:AllowedOrigins обязательна и задаётся в appsettings.");
        }

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins(cors.AllowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        var app = builder.Build();

        var indexes = app.Services.GetRequiredService<MongoIndexInitializer>();
        await indexes.EnsureIndexesAsync(CancellationToken.None);

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseCors("AllowFrontend");

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        if (!string.Equals(
                Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
                "true",
                StringComparison.OrdinalIgnoreCase))
        {
            app.UseHttpsRedirection();
        }

        app.UseAuthorization();
        app.MapControllers();
        await app.RunAsync();
    }
}
