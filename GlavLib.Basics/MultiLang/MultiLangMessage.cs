using System.Collections.Frozen;
using System.Text.RegularExpressions;

namespace GlavLib.Basics.MultiLang;

public partial class MultiLangMessage(IDictionary<string, string> templates)
{
    //Грамматика повторяет ErrorMessageTemplate в GlavLib.SourceGenerators: расходиться они не должны
    [GeneratedRegex(@"{(?<arg>\w+):[\w?]+(?::\w+)?}")]
    private static partial Regex ReplaceRegex();

    private readonly FrozenDictionary<string, string> _templates = templates.ToFrozenDictionary();

    /// <summary>
    /// Substitutes arguments into the template of the first supported language.
    /// A placeholder is written as <c>{name:type}</c>, where the type may be optional
    /// (<c>{name:type?}</c>) and may be followed by a format string (<c>{name:type:format}</c>) —
    /// the same grammar the errors source generator reads from <c>_errors/*.errors.yaml</c>.
    /// An argument that is missing from <paramref name="args"/> is replaced with an empty string;
    /// when <paramref name="args"/> is <c>null</c> the template is returned untouched.
    /// </summary>
    public string? Format(string[] langs, IDictionary<string, string>? args)
    {
        foreach (var lang in langs)
        {
            if (!_templates.TryGetValue(lang, out var template))
                continue;

            if (args is null)
                return template;

            return ReplaceRegex().Replace(
                    input: template,
                    evaluator: MatchEvaluator
                );
        }

        return null;

        string MatchEvaluator(Match v)
        {
            var arg = v.Groups["arg"].Value;

            if (args.TryGetValue(arg, out var value))
                return value;

            return string.Empty;
        }
    }
}
