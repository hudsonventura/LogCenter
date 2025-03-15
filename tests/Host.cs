using DotNetEnv;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace tests;

public class Host : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("DB_HOST", "localhost");
        Environment.SetEnvironmentVariable("DB_PORT", "5432");
        Environment.SetEnvironmentVariable("DB_NAME", "logcenter");
        Environment.SetEnvironmentVariable("DB_USER", "logcenter");
        Environment.SetEnvironmentVariable("DB_PASSWORD", "MyS3cr3tP@ssw0rd");

        builder.ConfigureServices(services =>
        {

        });

        return base.CreateHost(builder);
    }
}