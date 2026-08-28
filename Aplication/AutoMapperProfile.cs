using Application.Models.Employees.Commands;
using Application.Models.Periods.Commands;
using Application.Models.TimeEntries.Commands;
using AutoMapper;
using Domain.Models;

namespace Application;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        MapTimeEntryProfile();
        MapEmployeeProfile();
        MapPeriodProfile();
    }

    private void MapTimeEntryProfile()
    {
        CreateMap<CreateTimeEntryCommand, TimeEntryModel>()
            .ForMember(model => model.Id, options => options.Ignore())
            .ForMember(model => model.Version, options => options.Ignore())
            .ForMember(model => model.CreatedAt, options => options.Ignore())
            .ForMember(model => model.UpdatedAt, options => options.Ignore());

        CreateMap<UpdateTimeEntryCommand, TimeEntryModel>()
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
    }

    private void MapEmployeeProfile()
    {
        CreateMap<RateItem, RateModel>();

        CreateMap<UpdateEmployeeRatesCommand, EmployeeModel>()
            .ForMember(model => model.FullName, options => options.MapFrom(command => string.Empty))
            .ForMember(model => model.Department, options => options.MapFrom(command => string.Empty))
            .ForMember(model => model.Rates, options => options.MapFrom(command => command.Rates));
    }

    private void MapPeriodProfile()
    {
        CreateMap<ClosePeriodCommand, PeriodModel>();
        CreateMap<OpenPeriodCommand, PeriodModel>();
    }
}
