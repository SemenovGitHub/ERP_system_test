using Api.Middleware;
using Application;
using Microsoft.AspNetCore.Mvc;
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
        builder.Services.AddAutoMapper(typeof(Api.AutoMapperProfile), typeof(Repository.AutoMapperProfile));
        builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(entry => entry.Value is { Errors.Count: > 0 })
                        .ToDictionary(
                            entry => entry.Key,
                            entry => entry.Value!.Errors
                                .Select(error => HumanizeModelError(error.ErrorMessage))
                                .Distinct()
                                .ToArray());

                    var message = errors.SelectMany(pair => pair.Value).FirstOrDefault()
                        ?? "Ошибка валидации запроса.";

                    return new BadRequestObjectResult(
                        new ErrorResponse("VALIDATION_ERROR", message, errors));
                };
            });
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Настройка CORS для фронтенда
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend", policy =>
            {
                policy.WithOrigins("http://localhost:3000")
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
        });

        var app = builder.Build();

        var indexes = app.Services.GetRequiredService<MongoIndexInitializer>();
        await indexes.EnsureIndexesAsync(CancellationToken.None);

        app.UseMiddleware<GlobalExceptionMiddleware>();

        // Включаем CORS
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

    private static string HumanizeModelError(string? errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            return "Некорректное значение.";
        }

        if (errorMessage.Contains("DateOnly", StringComparison.OrdinalIgnoreCase)
            || errorMessage.Contains("date", StringComparison.OrdinalIgnoreCase))
        {
            return "Некорректная дата.";
        }

        if (errorMessage.Contains("Guid", StringComparison.OrdinalIgnoreCase))
        {
            return "Не выбран сотрудник или проект.";
        }

        return errorMessage;
    }
}
