using Microsoft.AspNetCore.Mvc;

namespace Api.Middleware;

internal static class InvalidModelStateFactory
{
    public static IActionResult Create(ActionContext context)
    {
        var errors = context.ModelState
            .Where(entry => entry.Value is { Errors.Count: > 0 })
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors
                    .Select(error => Humanize(error.ErrorMessage))
                    .Distinct()
                    .ToArray());

        var message = errors.SelectMany(pair => pair.Value).FirstOrDefault()
            ?? "Ошибка валидации запроса.";

        return new BadRequestObjectResult(
            new ErrorResponse("VALIDATION_ERROR", message, errors));
    }

    private static string Humanize(string? errorMessage)
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
