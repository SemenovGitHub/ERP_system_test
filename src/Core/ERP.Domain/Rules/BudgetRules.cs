namespace ERP.Domain.Rules;

public static class BudgetRules
{
    public const decimal RiskThresholdPercent = 80m;

    public static decimal UsagePercent(decimal cost, decimal budget) =>
        budget == 0
            ? 0
            : Money.Round(cost / budget * 100);

    public static bool IsOverBudget(decimal cost, decimal budget) =>
        cost > budget;

    public static bool IsRisk(decimal cost, decimal budget) =>
        IsOverBudget(cost, budget)
        || UsagePercent(cost, budget) > RiskThresholdPercent;
}
