using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace ERP.Tests;

public abstract class HandlerTestBase : IDisposable
{
    protected ServiceCollection Services { get; } = new();

    private ServiceProvider? _provider;

    protected Mock<T> RegisterMock<T>() where T : class
    {
        var mock = new Mock<T>();
        Services.AddSingleton(mock.Object);
        return mock;
    }

    protected THandler CreateHandler<THandler>() where THandler : class
    {
        Services.AddTransient<THandler>();
        _provider = Services.BuildServiceProvider();
        return _provider.GetRequiredService<THandler>();
    }

    public void Dispose()
    {
        _provider?.Dispose();
    }
}
