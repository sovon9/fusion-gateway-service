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