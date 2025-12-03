using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using org.cchmc.{{cookiecutter.namespace}}.data.Logging;
using org.cchmc.{{cookiecutter.namespace}}.models.ViewModels;

namespace org.cchmc.{{cookiecutter.namespace}}.endpoints.Endpoints
{
    public class SampleEndpoints
    {
        /* You can use attributes like these to decorate your parameters:
         * [FromRoute] - variables injected in the route itself
         * [FromQuery] - optional querystring parameters
         * [FromBody] - objects posted/put in the body of the request
         * [FromHeader] - items in the header
         * [FromServices] - specify from DI
         * 
         * You don't NEED to use these in most circumstances (.NET does a pretty good job of figuring it out) but it might be helpful to other developers!
         */
        public static Results<Ok<string>, BadRequest, ProblemHttpResult> SampleGet(string input)
        {
            return TypedResults.Ok($"This is an output: {input}.");
        }

        public static Results<Ok<string>, ProblemHttpResult> SampleLogging(ILogger<SampleEndpoints> _logger, [FromBody] ExampleInput input)
        {
            _logger.LogReceivedData(input);
            return TypedResults.Ok("Message logged.");
        }
    }
}
