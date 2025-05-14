using System.Diagnostics;
using AspireLocalStackExample.AppHost.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AspireLocalStackExample.AppHost;

public static class DistributedApplicationBuilderExtensions
{
    public static IResourceBuilder<IAWSCloudResource> AddAWS(this IDistributedApplicationBuilder builder)
    {
        if (builder.Environment.IsDevelopment())
        {
            var localStackVersion = builder.Configuration["LocalStackVersion"]!;

            // Here is our current workaround.
            // We need to ensure the LocalStack image is available, because the AWS resources don't currently have wait support.
            // Without this, Aspire will attempt to provision AWS resources before LocalStack is running, and will fail.
            EnsureLocalStackDockerImageHasBeenPulled(localStackVersion);
            
            return builder.AddLocalStack("localstack")
                .WithImageTag(localStackVersion)
                .WithLifetime(ContainerLifetime.Persistent);
        }
        
        var options = builder.Configuration.GetAWSOptions();
        var awsResource = new AWSCloudResource("aws", options);
        builder.Eventing.Subscribe<AfterEndpointsAllocatedEvent>(
            (evt, cancellationToken) =>
            {
                var notificationService = evt.Services.GetRequiredService<ResourceNotificationService>();
                var checks = awsResource.StartStatusChecks(notificationService);
                cancellationToken.Register(checks.Dispose);
                return Task.CompletedTask;
            }
        );

        return builder.AddResource(awsResource);
    }

    public static IResourceBuilder<LocalStackResource> AddLocalStack(this IDistributedApplicationBuilder builder, string name)
    {
        var awsOptions = builder.Configuration.GetAWSOptions();
        var localStack = new LocalStackResource(name, awsOptions);
        return builder.AddResource(localStack)
            .WithImage("localstack/localstack")
            .WithHttpEndpoint(port: localStack.Port, targetPort: 4566, isProxied: false)
            .WithEnvironment("DOCKER_HOST", "unix:///var/run/docker.sock")
            .WithEnvironment("DEBUG", "1");
    }

    private static void EnsureLocalStackDockerImageHasBeenPulled(string tag)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"pull localstack/localstack:{tag}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();

        process.StandardOutput.ReadToEnd();
        process.StandardError.ReadToEnd();
        process.WaitForExit();
    }
}
