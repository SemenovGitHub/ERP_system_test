using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Repository;
using Seeder;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddMongoRepositories(context.Configuration);
        services.AddTransient<DatabaseSeeder>();
    })
    .Build();

var seeder = host.Services.GetRequiredService<DatabaseSeeder>();
await seeder.RunAsync(CancellationToken.None);
