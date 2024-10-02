using Serilog.Core;
using Serilog.Events;

namespace GlavLib.Basics.Logging.Enrichers;

public sealed class ClassNameEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (!logEvent.Properties.TryGetValue("SourceContext", out var sourceContextValue))
            return;

        if (sourceContextValue is not ScalarValue scalarContext)
            return;

        var sourceContext = scalarContext.Value?.ToString();
        if (sourceContext is null)
            return;

        var lastDotIndex = sourceContext.LastIndexOf('.') + 1;
        var className = lastDotIndex != 0
            ? sourceContext[lastDotIndex..]
            : sourceContext;

        var logEventProperty = propertyFactory.CreateProperty("ClassName", className);
        logEvent.AddOrUpdateProperty(logEventProperty);
    }
}