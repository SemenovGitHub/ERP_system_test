using Domain.Rules;

namespace ERP.Tests;

public class BudgetRulesTests
{
    [Fact]
    public void Zero_budget_with_cost_is_over_budget_and_risk()
    {
        Assert.True(BudgetRules.IsOverBudget(cost: 150, budget: 0));
        Assert.True(BudgetRules.IsRisk(cost: 150, budget: 0));
    }

    [Fact]
    public void Zero_budget_with_zero_cost_is_not_over_budget()
    {
        Assert.False(BudgetRules.IsOverBudget(cost: 0, budget: 0));
        Assert.False(BudgetRules.IsRisk(cost: 0, budget: 0));
    }

    [Fact]
    public void Usage_above_80_percent_is_risk_but_not_over_budget()
    {
        Assert.False(BudgetRules.IsOverBudget(cost: 90, budget: 100));
        Assert.True(BudgetRules.IsRisk(cost: 90, budget: 100));
        Assert.Equal(90, BudgetRules.UsagePercent(90, 100));
    }

    [Fact]
    public void Cost_above_budget_is_over_budget()
    {
        Assert.True(BudgetRules.IsOverBudget(cost: 101, budget: 100));
        Assert.True(BudgetRules.IsRisk(cost: 101, budget: 100));
    }
}
