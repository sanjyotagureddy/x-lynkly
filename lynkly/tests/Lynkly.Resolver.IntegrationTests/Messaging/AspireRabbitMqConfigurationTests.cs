namespace Lynkly.Resolver.IntegrationTests.Messaging;

public sealed class AspireRabbitMqConfigurationTests
{
    [Fact]
    public void AppHost_ConfiguresRabbitMq_AndReferencesItFromServices()
    {
        var appHostFile = FindRepositoryFile("aspire/lynkly.AppHost/AppHost.cs");
        var appHostContent = File.ReadAllText(appHostFile);

        Assert.Contains("AddRabbitMQ(\"lynkly-rabbitmq\")", appHostContent, StringComparison.Ordinal);
        Assert.Contains(".WithReference(rabbitMq)", appHostContent, StringComparison.Ordinal);
        Assert.Contains(".WaitFor(rabbitMq)", appHostContent, StringComparison.Ordinal);
    }

    private static string FindRepositoryFile(string relativePath)
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var candidate = Path.Combine(current.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            current = current.Parent;
        }

        throw new FileNotFoundException($"Could not locate repository file '{relativePath}'.");
    }
}
