var builder = DistributedApplication.CreateBuilder(args);

var mysql = builder.AddMySql("mysql")
    .WithDataVolume()
    .AddDatabase("url-shortener");

var redis = builder.AddRedis("redis");

builder.AddProject<Projects.UrlShortner_Api>("urlshortner-api")
    .WithReference(mysql)
    .WithReference(redis)
    .WaitFor(mysql)
    .WaitFor(redis);

builder.Build().Run();
