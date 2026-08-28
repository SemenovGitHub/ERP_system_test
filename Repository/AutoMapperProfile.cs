using AutoMapper;
using Domain.Models;
using Repository.Documents;

namespace Repository;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        MapDateProfile();
        MapEmployeeProfile();
        MapProjectProfile();
        MapTimeEntryProfile();
    }

    private void MapDateProfile()
    {
        CreateMap<DateOnly, DateTime>()
            .ConvertUsing(date => DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc));

        CreateMap<DateTime, DateOnly>()
            .ConvertUsing(date => DateOnly.FromDateTime(DateTime.SpecifyKind(date, DateTimeKind.Utc)));

        CreateMap<DateOnly?, DateTime?>()
            .ConvertUsing(date => date.HasValue
                ? DateTime.SpecifyKind(date.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc)
                : null);

        CreateMap<DateTime?, DateOnly?>()
            .ConvertUsing(date => date.HasValue
                ? DateOnly.FromDateTime(DateTime.SpecifyKind(date.Value, DateTimeKind.Utc))
                : null);
    }

    private void MapEmployeeProfile()
    {
        CreateMap<RateDocument, RateModel>();
        CreateMap<RateModel, RateDocument>();

        CreateMap<EmployeeDocument, EmployeeModel>();
        CreateMap<EmployeeModel, EmployeeDocument>();
    }

    private void MapProjectProfile()
    {
        CreateMap<ProjectDocument, ProjectModel>();
        CreateMap<ProjectModel, ProjectDocument>();
    }

    private void MapTimeEntryProfile()
    {
        CreateMap<TimeEntryDocument, TimeEntryModel>();

        CreateMap<TimeEntryModel, TimeEntryDocument>()
            .ForMember(
                document => document.CreatedAt,
                options => options.MapFrom(entry => DateTime.SpecifyKind(entry.CreatedAt, DateTimeKind.Utc)))
            .ForMember(
                document => document.UpdatedAt,
                options => options.MapFrom(entry => ToUtc(entry.UpdatedAt)));
    }

    private static DateTime? ToUtc(DateTime? value) =>
        value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;
}
