using Amazon.Extensions.NETCore.Setup;

namespace AspireLocalStackExample.AppHost.Resources;

public class LocalStackResource(string name, AWSOptions options) : ContainerResource(name), IAWSCloudResource
{
    public AWSOptions AWSOptions { get; } = options;
    
    public int Port { get; } = new Uri(options.DefaultClientConfig.ServiceURL).Port;
}
