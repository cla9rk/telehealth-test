using Microsoft.Extensions.Logging;
using org.cchmc.{{cookiecutter.namespace}}.models.ViewModels;

namespace org.cchmc.{{cookiecutter.namespace}}.data.Logging
{
    public static partial class LoggerMessages
    {
        // This is just an example. You can create your own log messages using the LoggerMessage attribute.
        [LoggerMessage(LogLevel.Information, Message = "Received ExampleInput")]
        public static partial void LogReceivedData(this ILogger logger, [LogProperties] ExampleInput input);
    }
}
