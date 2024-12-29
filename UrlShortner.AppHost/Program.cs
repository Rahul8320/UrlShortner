var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.UrlShortner_Api>("urlshortner-api");

builder.Build().Run();
