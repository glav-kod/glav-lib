using System.Text.RegularExpressions;

namespace GlavLib.SourceGenerators.Utils;

public struct ErrorArgument
{
    public string Name { get; set; }

    public string Type { get; set; }

    public string? FormatString { get; set; }

    public bool IsOptional { get; set; }
}

public sealed class ErrorMessageTemplate
{
    private static readonly Regex ReplaceRegex = new(@"{(?<name>\w+):(?<type>[\w?]+)(:(?<format>\w+))?}", RegexOptions.Compiled);

    public string InterpolatedMessage { get; private set; } = null!;

    public IList<ErrorArgument> Arguments { get; private set; } = null!;

    private ErrorMessageTemplate()
    {
    }

    public static ErrorMessageTemplate Parse(string template)
    {
        var arguments = new List<ErrorArgument>();

        var interpolatedString = ReplaceRegex.Replace(template, v =>
        {
            var nameGroup   = v.Groups["name"];
            var typeGroup   = v.Groups["type"];
            var formatGroup = v.Groups["format"];

            var argumentName = nameGroup.Value;
            var typeName     = typeGroup.Value;
            var formatString = string.IsNullOrWhiteSpace(formatGroup.Value) ? null : formatGroup.Value;

            arguments.Add(new ErrorArgument
            {
                Name = argumentName,
                Type = typeName,
                FormatString = formatString,
                IsOptional = typeName.EndsWith("?")
            });

            return formatString is null
                ? $"{{{argumentName}}}"
                : $"{{{argumentName}:{formatString}}}";
        });

        return new ErrorMessageTemplate
        {
            InterpolatedMessage = interpolatedString,
            Arguments = arguments
        };
    }
}