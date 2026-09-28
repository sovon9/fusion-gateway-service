using HotChocolate.AspNetCore;
using System;

var builder = WebApplication.CreateBuilder(args);
//var fusionTimeout = builder.Configuration.GetValue<int>("Fusion:ExecutionTimeoutSeconds");

builder.Services
    .AddCors()
    .AddHeaderPropagation(options=>
    {
        options.Headers.Add("Site-Id");
        options.Headers.Add("Authorization");
    });

builder.Services
    .AddHttpClient("fusion");

// Attaches the HeaderPropagationMessageHandler to every HttpClient the DI container creates —
// including whichever per-subgraph clients HotChocolate Fusion builds internally from
// subgraph-config.json — so the Authorization/Site-Id headers captured below actually go out
// on subgraph calls, regardless of what Fusion names those clients.
builder.Services.ConfigureHttpClientDefaults(b => b.AddHeaderPropagation());

builder.Services
    .AddFusionGatewayServer()
    .ModifyRequestOptions(opt=>
    {
        opt.IncludeExceptionDetails = true;
        opt.ExecutionTimeout = TimeSpan.FromSeconds(30);
    })
    .AddErrorFilter(error =>
    {
        if(error.Exception is { } ex)
        {
            Console.WriteLine(ex.Message);
        }
        return error;
    })
    .ModifyFusionOptions(opt =>
    {
        opt.AllowQueryPlan = false;
    })
    .ConfigureFromFile("./supergraph/gateway.fgp", watchFileForUpdates: true);

var app = builder.Build();

// Must run before anything that makes outgoing subgraph calls: this is what actually captures
// the configured headers off each incoming request into HeaderPropagationValues.
app.UseHeaderPropagation();

var graphQLServerOptions = new GraphQLServerOptions
{
    EnableBatching = true,
    EnableSchemaRequests = true,
    Tool =
    {
        Title = "Plantapps",
        Enable = true,
        ServeMode = GraphQLToolServeMode.Embedded
    }
};

app.MapGraphQL("/graphql")
    .WithOptions(graphQLServerOptions);

app.RunWithGraphQLCommands(args);