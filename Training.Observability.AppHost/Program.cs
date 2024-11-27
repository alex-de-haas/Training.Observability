var builder = DistributedApplication.CreateBuilder(args);

var jaeger = builder
    .AddContainer("jaeger", "jaegertracing/all-in-one")
    .WithHttpEndpoint(targetPort: 16686, name: "HTTP")
    .WithHttpEndpoint(targetPort: 4317, name: "OTLP");

//var prometheus = builder
//    .AddContainer("prometheus", "prom/prometheus")
//    .WithHttpEndpoint(targetPort: 9090, name: "HTTP");

//var loki = builder
//    .AddContainer("loki", "grafana/loki")
//    .WithHttpEndpoint(targetPort: 3100, name: "HTTP");

//builder
//    .AddContainer("grafana", "grafana/grafana")
//    .WithHttpEndpoint(targetPort: 3000, name: "HTTP")
//    .WithReference(jaeger.GetEndpoint("HTTP"))
//    .WithReference(prometheus.GetEndpoint("HTTP"))
//    .WithReference(loki.GetEndpoint("HTTP"));

var insights = builder.AddConnectionString(
    "ApplicationInsights",
    "APPLICATIONINSIGHTS_CONNECTION_STRING");

var seq = builder.AddSeq("seq");

var apiService = builder
    .AddProject<Projects.Training_Observability_ApiService>("apiservice")
    .WithOtlpExporter()
    .WithReference(seq)
.WithReference(insights)
.WithEnvironment("JAEGER_ENDPOINT", jaeger.GetEndpoint("OTLP"));
//.WithEnvironment("LOKI_ENDPOINT", loki.GetEndpoint("HTTP"));

builder
    .AddProject<Projects.Training_Observability_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WithOtlpExporter()
    .WithReference(seq)
.WithReference(insights)
.WithEnvironment("JAEGER_ENDPOINT", jaeger.GetEndpoint("OTLP"));
//.WithEnvironment("LOKI_ENDPOINT", loki.GetEndpoint("HTTP"));

builder.Build().Run();
