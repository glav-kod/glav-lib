using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace GlavLib.Basics.MultiLang;

public sealed class LanguagePack
{
    private static readonly Deserializer Deserializer = new();

    public required string Language { get; init; }

    public required IDictionary<string, string> Messages { get; init; }

    public static LanguagePack FromYaml(string yaml)
    {
        var yamlLanguagePack = Deserializer.Deserialize<YamlLanguagePack>(yaml);

        var messages = new Dictionary<string, string>();

        foreach (var languagePackBundle in yamlLanguagePack.Bundles)
        {
            foreach (var (key, message) in languagePackBundle.Messages)
            {
                messages[$"{languagePackBundle.Prefix}.{key}"] = message;
            }
        }

        return new LanguagePack
        {
            Language = yamlLanguagePack.Language,
            Messages = messages
        };
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    private sealed class YamlLanguagePackBundle
    {
        public required string Prefix { get; init; }

        public required IDictionary<string, string> Messages { get; init; }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    private sealed class YamlLanguagePack
    {
        public required string Language { get; init; }

        public required IList<YamlLanguagePackBundle> Bundles { get; init; }
    }
}