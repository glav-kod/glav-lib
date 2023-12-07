using System.Collections.Frozen;
using System.Text.RegularExpressions;

namespace GlavLib.Basics.MultiLang;

public partial class MultiLangMessage
{
    [GeneratedRegex(@"{(?<arg>\w+)}")]
    private static partial Regex ReplaceRegex();

    public FrozenDictionary<string, string> Templates { get; }

    public MultiLangMessage(IDictionary<string, string> templates)
    {
        Templates = templates.ToFrozenDictionary();
    }

    public string? Format(string[] langs, IDictionary<string, string>? args)
    {
        foreach (var lang in langs)
        {
            if (!Templates.TryGetValue(lang, out var template))
                continue;

            if (args is null)
                return template;

            return ReplaceRegex().Replace(template, v =>
            {
                var arg = v.Groups["arg"].Value;

                if (args.TryGetValue(arg, out var value))
                    return value;

                return string.Empty;
            });
        }

        return null;
    }
}