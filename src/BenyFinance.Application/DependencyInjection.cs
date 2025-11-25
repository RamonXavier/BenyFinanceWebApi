using BenyFinance.Application.Interfaces;
using BenyFinance.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BenyFinance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICreditCardService, CreditCardService>();
        services.AddScoped<IRecurringTemplateService, RecurringTemplateService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
