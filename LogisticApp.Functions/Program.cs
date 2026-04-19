using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LogisticApp.Functions.Data;
using LogisticApp.Functions.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        var connectionString = context.Configuration["SqlConnectionString"]
            ?? throw new InvalidOperationException("SqlConnectionString non configurata");

        services.AddDbContext<FunctionsDbContext>(opt =>
            opt.UseSqlServer(connectionString));

        services.AddScoped<IVeconLoginService, VeconLoginService>();
    })
    .Build();

await host.RunAsync();
