using AspireLocalStackExample.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var aws = builder.AddAWS();

var awsResources = builder.AddAWSCloudFormationTemplate("AspireSampleDevResources", "app-resources.template")
    .WithParameter("DefaultVisibilityTimeout", "30")
    // Ideally, we'd like to be able to do this, so that the provisioning is delayed until LocalStack is available:
    //.Waitfor(aws)
    .WithReference(aws)
    .WithParentRelationship(aws);

builder.AddProject<Projects.AspireLocalStackExample_ApiService>("apiservice")
    .WithReference(aws)
    .WithReference(awsResources);

builder.Build().Run();
