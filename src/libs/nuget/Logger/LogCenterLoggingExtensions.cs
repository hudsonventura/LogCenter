using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LogCenter;

/// <summary>Registra o <see cref="ILoggerProvider"/> que envia entradas de log via HTTP.</summary>
public static class LogCenterLoggingExtensions
{
    /// <summary>
    /// Adiciona o provider LogCenter. Exige apenas que <c>LogCenterOptions</c> esteja registrado
    /// no serviço de injeção de dependências (AddSingleton ou via configuration).
    /// </summary>
    public static ILoggingBuilder AddLogCenter(this ILoggingBuilder builder, LogCenterOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);

        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<ILoggerProvider>(sp => 
            new LogCenterLoggerProvider(sp.GetRequiredService<LogCenterOptions>()));
        return builder;
    }
}
