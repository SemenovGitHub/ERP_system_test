using AutoMapper;
using ERP.Abstractions.Models.Employees;
using ERP.Abstractions.Models.Periods;
using ERP.Abstractions.Models.Projects;
using ERP.Abstractions.Models.Reports;
using ERP.Abstractions.Models.TimeEntries;
using Application.Models.Employees.Queries;
using Application.Models.Employees.Responses;
using Application.Models.Periods.Commands;
using Application.Models.Projects.Queries;
using Application.Models.Projects.Responses;
using Application.Models.Reports.Queries;
using Application.Models.Reports.Responses;
using Application.Models.TimeEntries.Commands;
using Application.Models.TimeEntries.Queries;
using Application.Models.TimeEntries.Responses;

namespace ERP;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        MapTimeEntryProfile();
        MapReportProfile();
        MapEmployeeProfile();
        MapProjectProfile();
        MapPeriodProfile();
    }

    private void MapTimeEntryProfile()
    {
        // API DTO -> Application

        CreateMap<GetTimeEntriesDto, GetTimeEntriesQuery>();

        CreateMap<CreateTimeEntryDto, CreateTimeEntryCommand>();

        CreateMap<UpdateTimeEntryDto, UpdateTimeEntryCommand>();

        // Application -> API DTO

        CreateMap<TimeEntryResponse, TimeEntryDto>();

        CreateMap<PagedTimeEntriesResponse, PagedTimeEntriesDto>();
    }

    private void MapReportProfile()
    {
        // API DTO -> Application

        CreateMap<GetProjectsReportDto, GetProjectsReportQuery>();

        // Application -> API DTO

        CreateMap<ProjectReportItemResponse, ProjectReportItemDto>();

        CreateMap<ProjectReportTotalResponse, ProjectReportTotalDto>();

        CreateMap<ProjectReportResponse, ProjectReportDto>();
    }

    private void MapEmployeeProfile()
    {
        CreateMap<GetEmployeesDto, GetEmployeesQuery>();

        CreateMap<EmployeeResponse, EmployeeDto>();

        CreateMap<PagedEmployeesResponse, PagedEmployeesDto>();
    }

    private void MapProjectProfile()
    {
        CreateMap<GetProjectsDto, GetProjectsQuery>();

        CreateMap<ProjectResponse, ProjectDto>();

        CreateMap<PagedProjectsResponse, PagedProjectsDto>();
    }

    private void MapPeriodProfile()
    {
        // API DTO -> Application

        CreateMap<PeriodDto, ClosePeriodCommand>();

        CreateMap<PeriodDto, OpenPeriodCommand>();
    }
}