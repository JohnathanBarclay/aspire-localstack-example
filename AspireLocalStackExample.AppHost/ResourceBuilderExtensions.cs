using Amazon.CloudFormation;
using Aspire.Hosting.AWS.CloudFormation;

namespace AspireLocalStackExample.AppHost;

public static class ResourceBuilderExtensions
{
    public static IResourceBuilder<TCloudFormationResource> WithReference<TCloudFormationResource>(
        this IResourceBuilder<TCloudFormationResource> builder,
        IResourceBuilder<IAWSCloudResource> aws
    )
    where TCloudFormationResource : ICloudFormationResource
    {
        var cloudFormation = aws.Resource.AWSOptions.CreateServiceClient<IAmazonCloudFormation>();
        builder.WithReference(cloudFormation);

        return builder;
    }
    
    public static IResourceBuilder<ProjectResource> WithReference(
        this IResourceBuilder<ProjectResource> builder,
        IResourceBuilder<IAWSCloudResource> aws
    )
    {
        var awsOptions = aws.Resource.AWSOptions;

        if (awsOptions.Profile is string profile)
        {
            builder.WithEnvironment("AWS__Profile", profile);
        }
        
        if (awsOptions.Region?.SystemName is string region)
        {
            builder.WithEnvironment("AWS__Region", region);
        }
        
        if (awsOptions.DefaultClientConfig?.AuthenticationRegion is string authRegion)
        {
            builder.WithEnvironment("AWS__AuthenticationRegion", authRegion);
        }

        if (awsOptions.DefaultClientConfig?.ServiceURL is string serviceUrl)
        {
            builder.WithEnvironment("AWS__ServiceURL", serviceUrl);
        }

        return builder;
    }
}
