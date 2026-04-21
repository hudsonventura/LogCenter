using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace LogCenter;

/// <summary>Registra o <see cref="ILoggerProvider"/> que envia entradas de log via HTTP.</summary>
public static class LogCenterLoggingExtensions
{
    /// <summary>
    /// Adiciona o provider LogCenter. Exige <c>AddHttpClient(LogCenterOptions.HttpClientName, ...)</c>
    /// e em geral <c>Configure&lt;LogCenterOptions&gt;</c> já registrados antes do build da aplicação.
    /// </summary>
    public static ILoggingBuilder AddLogCenter(this ILoggingBuilder builder)
    {
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, LogCenterLoggerProvider>());
        return builder;
    }
}
