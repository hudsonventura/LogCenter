using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        => AddLogCenter(builder, options, drain: null);

    /// <summary>
    /// Adiciona o provider LogCenter e permite expor um <see cref="LogCenterDrain"/>
    /// para aguardar os logs pendentes no encerramento da aplicação.
    /// </summary>
    public static ILoggingBuilder AddLogCenter(
        this ILoggingBuilder builder,
        LogCenterOptions options,
        LogCenterDrain? drain)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);

        builder.Services.AddSingleton(options);
        if (drain is null)
            builder.Services.TryAddSingleton<LogCenterDrain>();
        else
            builder.Services.AddSingleton(drain);

        builder.Services.AddSingleton<ILoggerProvider>(sp => 
            new LogCenterLoggerProvider(
                sp.GetRequiredService<LogCenterOptions>(),
                sp.GetRequiredService<LogCenterDrain>()));
        return builder;
    }
}
