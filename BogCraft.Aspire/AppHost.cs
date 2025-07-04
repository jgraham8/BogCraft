var builder = DistributedApplication.CreateBuilder(args);

var ui = builder.AddProject<Projects.BogCraft_UI>("bogcraft-ui");

builder.Build().Run();