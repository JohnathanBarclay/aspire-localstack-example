using Amazon.CloudFormation;
using Amazon.Extensions.NETCore.Setup;

namespace AspireLocalStackExample.AppHost;

public interface IAWSCloudResource : IResource
{
    public AWSOptions AWSOptions { get; }
}

public class AWSCloudResource(string name, AWSOptions options) : Resource(name), IAWSCloudResource
{
    public AWSOptions AWSOptions { get; } = options;

    public IDisposable StartStatusChecks(ResourceNotificationService notificationService, int intervalMilliseconds = 60_000)
    {
        var timer = new Timer(
            delegate { PerformStatusCheck(notificationService); },
            state: null,
            dueTime: 0,
            period: intervalMilliseconds);

        return timer;
    }

    private async void PerformStatusCheck(ResourceNotificationService notificationService)
    {
        var client = AWSOptions.CreateServiceClient<IAmazonCloudFormation>();
        try
        {
            await client.DescribeStacksAsync();
            await notificationService.PublishUpdateAsync(this, snapshot => snapshot with
            {
                State = new ResourceStateSnapshot(KnownResourceStates.Running, KnownResourceStateStyles.Success)
            });
        }
        catch
        {
            await notificationService.PublishUpdateAsync(this, snapshot => snapshot with
            {
                State = new ResourceStateSnapshot("Unavailable", KnownResourceStateStyles.Error)
            });
        }
    }
}