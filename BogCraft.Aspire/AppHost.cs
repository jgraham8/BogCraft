var builder = DistributedApplication.CreateBuilder(args);

// Add the UI project as a service
var ui = builder.AddProject<Projects.BogCraft_UI>("bogcraft-ui")
    .WithHttpsEndpoint(port: 7019, name: "https")
    .WithHttpEndpoint(port: 5091, name: "http")
    .WithExternalHttpEndpoints();

builder.Build().Run();