using GlobExpressions;

namespace GlavLib.Basics.MultiLang;

public sealed class LanguageContextBuilder
{
    private readonly List<LanguagePack> _languagePacks = new();

    public LanguageContextBuilder FromYaml(string yaml)
    {
        var languagePack = LanguagePack.FromYaml(yaml);
        _languagePacks.Add(languagePack);
        return this;
    }

    public LanguageContextBuilder FromDirectory(string directory, string globPattern)
    {
        var glob = new Glob(globPattern);

        foreach (var filePath in Directory.EnumerateFiles(directory))
        {
            if (!glob.IsMatch(filePath))
                continue;

            var yaml         = File.ReadAllText(filePath);
            var languagePack = LanguagePack.FromYaml(yaml);
            _languagePacks.Add(languagePack);
        }

        return this;
    }

    public LanguageContext Build()
    {
        return LanguageContext.Build(_languagePacks);
    }
}