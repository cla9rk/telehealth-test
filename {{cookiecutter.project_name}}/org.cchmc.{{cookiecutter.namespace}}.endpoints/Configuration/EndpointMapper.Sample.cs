using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using org.cchmc.{{cookiecutter.namespace}}.endpoints.Endpoints;

namespace org.cchmc.{{cookiecutter.namespace}}.endpoints.Configuration
{
    public static partial class EndpointMapper
    {
        public static void PrepareApis(this IServiceCollection services)
        {
            services.AddValidation(); // requires the InterceptorsNamespace in the .csproj
        }

        public static void MapSampleEndpoints(this WebApplication app)
        {
            var sampleEndpoints = app.MapGroup("/api/sample"); // you can use .RequireAuthorization() here to make all endpoints in this collection require authorization

            sampleEndpoints.MapGet("/", SampleEndpoints.SampleGet)
                           .WithName("GetSample") // optional, displays in Swagger
                           .WithDescription("Description goes here") // optional, displays in Swagger
                           .WithSummary("Longer summary goes here") // optional, displays in Swagger
                           .RequireAuthorization() // if you want to require authorization on this endpoint specifically
                           .Produces<string>(200)
                           .Produces<object>(400)
                           .ProducesProblem(500);

            sampleEndpoints.MapPost("/logging", SampleEndpoints.SampleLogging)
                           .WithName("LoggingExample")
                           .WithDescription("This endpoint demonstrates logging with redaction.")
                           .WithSummary("Logs a message to graylog.")
                           .Produces<string>(200)
                           .ProducesProblem(500);

            // you can also use .MapPost, .MapPut, .MapDelete, etc etc
        }
    }
}
