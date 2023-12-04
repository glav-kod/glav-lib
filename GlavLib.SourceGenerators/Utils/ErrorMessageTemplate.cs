using System.Text.RegularExpressions;
using Humanizer;

namespace GlavLib.SourceGenerators.Utils;

public sealed class ErrorMessageTemplate
{
    public string InterpolatedMessage { get; private set; } = null!;

    public IList<string> ArgumentNames { get; private set; } = null!;

    public IList<string> ParameterNames { get; private set; } = null!;

    public IList<string> ParameterTypes { get; private set; } = null!;

    private ErrorMessageTemplate()
    {
    }

    public static ErrorMessageTemplate Parse(string template)
    {
        var argumentNames = new List<string>();
        var parametersNames = new List<string>();
        var parameterTypes = new List<string>();

        var replaceRegex = new Regex(@"{(?<name>\w+):(?<type>[\w?]+)}", RegexOptions.Compiled);

        var interpolatedString = replaceRegex.Replace(template, v =>
        {
            var nameGroup = v.Groups["name"];
            var typeGroup = v.Groups["type"];

            var argumentName = nameGroup.Value.Camelize();
            var parameterName = nameGroup.Value.Pascalize();

            argumentNames.Add(argumentName);
            parametersNames.Add(parameterName);
            parameterTypes.Add(typeGroup.Value);

            return $"{{{argumentName}}}";
        });

        return new ErrorMessageTemplate
        {
            InterpolatedMessage = interpolatedString,
            ArgumentNames = argumentNames,
            ParameterNames = parametersNames,
            ParameterTypes = parameterTypes
        };
    }
}