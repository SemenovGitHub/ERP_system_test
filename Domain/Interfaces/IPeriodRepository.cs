namespace Domain.Interfaces;

public interface IPeriodRepository
{
    Task<bool> IsClosedAsync(int year, int month, CancellationToken cancellationToken);

    Task CloseAsync(int year, int month, CancellationToken cancellationToken);

    Task OpenAsync(int year, int month, CancellationToken cancellationToken);
}
