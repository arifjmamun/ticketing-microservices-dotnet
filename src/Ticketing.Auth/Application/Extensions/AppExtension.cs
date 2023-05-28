using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Ticketing.Auth.Application.Commands;
using Ticketing.Auth.Application.DTO;
using Ticketing.Auth.Application.Mapping;
using Ticketing.Auth.Application.Validators;
using Ticketing.Auth.Persistence;
using Ticketing.Auth.Persistence.Repositories;

namespace Ticketing.Auth.Application.Extensions;

public static class AppExtension
{
    public static void AddMapping(this WebApplicationBuilder builder)
    {
        MappingConfig.Configure();
    }

    public static void AddFluentValidation(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IValidator<SignupRequest>, SignupCommandValidator>();
    }

    public static void AddMediatR(this WebApplicationBuilder builder)
    {
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SignupCommandHandler).Assembly));
    }

    public static void AddDbContext(this WebApplicationBuilder builder)
    {
        if (builder.Environment.IsEnvironment("Test"))
            builder.Services.AddDbContext<AuthDbContext>(options =>
                options.UseInMemoryDatabase(nameof(AuthDbContext)));
        else
            builder.Services.AddDbContext<AuthDbContext>(
                options => options.UseNpgsql(builder.Configuration.GetConnectionString(nameof(AuthDbContext))));
    }

    public static void ConfigureRoute(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<RouteOptions>(options => { options.LowercaseUrls = true; });
    }

    public static void RegisterServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUserRepository, UserRepository>();
    }

    public static void MigrateDatabase(this WebApplication? app)
    {
        using var scope = app?.Services.CreateScope();
        if (scope?.ServiceProvider is null) return;
        var serviceProvider = scope.ServiceProvider;

        var context = serviceProvider?.GetRequiredService<AuthDbContext>();
        context?.Database.Migrate();
    }
}