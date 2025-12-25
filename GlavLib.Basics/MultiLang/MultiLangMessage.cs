using System.Collections.Frozen;
using System.Text.RegularExpressions;

namespace GlavLib.Basics.MultiLang;

public partial class MultiLangMessage(IDictionary<string, string> templates)
{
    [GeneratedRegex(@"{(?<arg>\w+):\w+}")]
    private static partial Regex ReplaceRegex();

    private readonly FrozenDictionary<string, string> _templates = templates.ToFrozenDictionary();

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
