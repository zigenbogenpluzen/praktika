var builder = DistributedApplication.CreateBuilder(args);

// API сервис
var apiService = builder.AddProject<Projects.ResoConsultant_ApiService>("apiservice");

// Веб‑фронтенд (Blazor)
builder.AddProject<Projects.ResoConsultant_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
