using Api.Abstractions.Models.Employees;
using Application.Models.Employees.Commands;
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
using AutoMapper;
using ERP.Abstractions.Models.Employees;
using ERP.Abstractions.Models.Periods;
using ERP.Abstractions.Models.Projects;
using ERP.Abstractions.Models.Reports;
using ERP.Abstractions.Models.TimeEntries;

namespace Api;

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

        CreateMap<TimeEntryResponse, TimeEntryDto>()
            .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Date.ToDateTime(TimeOnly.MinValue)));

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

        CreateMap<RateResponse, RateDto>();

        CreateMap<EmployeeResponse, EmployeeDto>()
            .ForMember(dest => dest.Rates, opt => opt.MapFrom(src => src.Rates));

        CreateMap<PagedEmployeesResponse, PagedEmployeesDto>();

        CreateMap<RateDto, RateItem>();

        CreateMap<UpdateEmployeeRatesDto, UpdateEmployeeRatesCommand>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }

    private void MapProjectProfile()
    {
        CreateMap<GetProjectsDto, GetProjectsQuery>();

        CreateMap<ProjectResponse, ProjectDto>()
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.ToDateTime(TimeOnly.MinValue)))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.HasValue ? src.EndDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null));

        CreateMap<PagedProjectsResponse, PagedProjectsDto>();
    }

    private void MapPeriodProfile()
    {
        // API DTO -> Application

        CreateMap<PeriodDto, ClosePeriodCommand>();

        CreateMap<PeriodDto, OpenPeriodCommand>();
    }
}
