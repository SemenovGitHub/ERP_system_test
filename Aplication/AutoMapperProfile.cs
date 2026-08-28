using Application.Models.Employees.Commands;
using Application.Models.Employees.Queries;
using Application.Models.Employees.Responses;
using Application.Models.Periods.Commands;
using Application.Models.Projects.Queries;
using Application.Models.Projects.Responses;
using Application.Models.Reports.Responses;
using Application.Models.TimeEntries.Commands;
using Application.Models.TimeEntries.Responses;
using Domain.Validators;
using AutoMapper;
using Domain.Interfaces;
using Domain.Models;

namespace Application;

public class AutoMapperProfile : Profile
{
    private const decimal RiskThresholdPercent = 80m;

    public AutoMapperProfile()
    {
        MapTimeEntryProfile();
        MapEmployeeProfile();
        MapProjectProfile();
        MapPeriodProfile();
        MapReportProfile();
    }

    private void MapTimeEntryProfile()
    {
        CreateMap<CreateTimeEntryCommand, TimeEntryModel>()
            .ForMember(model => model.Id, options => options.Ignore())
            .ForMember(model => model.Version, options => options.Ignore())
            .ForMember(model => model.CreatedAt, options => options.Ignore())
            .ForMember(model => model.UpdatedAt, options => options.Ignore());

        CreateMap<UpdateTimeEntryCommand, TimeEntryModel>()
            .ForMember(model => model.Version, options => options.Ignore())
            .ForMember(model => model.CreatedAt, options => options.Ignore())
            .ForMember(model => model.UpdatedAt, options => options.Ignore());

        CreateMap<DeleteTimeEntryCommand, TimeEntryModel>()
            .ForMember(model => model.EmployeeId, options => options.Ignore())
            .ForMember(model => model.ProjectId, options => options.Ignore())
            .ForMember(model => model.Date, options => options.Ignore())
            .ForMember(model => model.Hours, options => options.Ignore())
            .ForMember(model => model.Comment, options => options.Ignore())
            .ForMember(model => model.Version, options => options.Ignore())
            .ForMember(model => model.CreatedAt, options => options.Ignore())
            .ForMember(model => model.UpdatedAt, options => options.Ignore());

        CreateMap<TimeEntryModel, TimeEntryResponse>()
            .ForMember(response => response.EmployeeFullName, options => options.Ignore())
            .ForMember(response => response.ProjectCode, options => options.Ignore())
            .ForMember(response => response.ProjectName, options => options.Ignore())
            .ForMember(response => response.Rate, options => options.Ignore())
            .ForMember(response => response.Cost, options => options.Ignore())
            .ForMember(response => response.IsOvertime, options => options.Ignore());

        CreateMap<PagedTimeEntries, PagedTimeEntriesResponse>()
            .ForMember(response => response.Items, options => options.Ignore())
            .ForMember(response => response.Page, options => options.Ignore())
            .ForMember(response => response.PageSize, options => options.Ignore());
    }

    private void MapEmployeeProfile()
    {
        CreateMap<RateItem, RateModel>();
        CreateMap<RateModel, RateResponse>();
        CreateMap<EmployeeModel, EmployeeResponse>();

        CreateMap<UpdateEmployeeRatesCommand, EmployeeModel>()
            .ForMember(model => model.FullName, options => options.MapFrom(_ => string.Empty))
            .ForMember(model => model.Department, options => options.MapFrom(_ => string.Empty))
            .ForMember(model => model.Rates, options => options.MapFrom(command => command.Rates));

        CreateMap<PagedResult<EmployeeModel>, PagedEmployeesResponse>()
            .ForMember(response => response.Page, options => options.Ignore())
            .ForMember(response => response.PageSize, options => options.Ignore());
    }

    private void MapProjectProfile()
    {
        CreateMap<ProjectModel, ProjectResponse>();

        CreateMap<PagedResult<ProjectModel>, PagedProjectsResponse>()
            .ForMember(response => response.Page, options => options.Ignore())
            .ForMember(response => response.PageSize, options => options.Ignore());
    }

    private void MapPeriodProfile()
    {
        CreateMap<ClosePeriodCommand, PeriodModel>();
        CreateMap<OpenPeriodCommand, PeriodModel>();
    }

    private void MapReportProfile()
    {
        CreateMap<ProjectReportModel, ProjectReportItemResponse>()
            .ForMember(
                response => response.BudgetUsagePercent,
                options => options.MapFrom(row => BudgetUsagePercent(row)))
            .ForMember(
                response => response.IsOverBudget,
                options => options.MapFrom(row => row.Cost > row.Budget))
            .ForMember(
                response => response.IsRisk,
                options => options.MapFrom(row =>
                    row.Cost > row.Budget || BudgetUsagePercent(row) > RiskThresholdPercent));

        CreateMap<IReadOnlyList<ProjectReportModel>, ProjectReportResponse>()
            .ConvertUsing((rows, _, context) =>
            {
                var items = context.Mapper.Map<List<ProjectReportItemResponse>>(rows);
                return new ProjectReportResponse
                {
                    Items = items,
                    Total = new ProjectReportTotalResponse
                    {
                        Hours = items.Sum(item => item.Hours),
                        Cost = items.Sum(item => item.Cost)
                    }
                };
            });
    }

    private static decimal BudgetUsagePercent(ProjectReportModel row) =>
        row.Budget == 0 ? 0 : MoneyValidator.Round(row.Cost / row.Budget * 100);
}
