using System.Collections.Frozen;

namespace GlavLib.Basics.MultiLang;

public sealed class LanguageContext
{
    public FrozenDictionary<string, MultiLangMessage> Messages { get; init; }

    public LanguageContext(IDictionary<string, MultiLangMessage> messages)
    {
        Messages = messages.ToFrozenDictionary();
    }

    public static LanguageContext Build(IList<LanguagePack> languagePacks)
    {
        var messageTemplates = new Dictionary<string, IDictionary<string, string>>();

        foreach (var languagePack in languagePacks)
        {
            var lang = languagePack.Language;

            foreach (var message in languagePack.Messages)
            {
                if (!messageTemplates.TryGetValue(message.Key, out var langTemplates))
                {
                    langTemplates                 = new Dictionary<string, string>();
                    messageTemplates[message.Key] = langTemplates;
                }

                langTemplates[lang] = message.Value;
            }
        }

        var messages = messageTemplates.ToDictionary(kvp => kvp.Key,
                                                     kvp => new MultiLangMessage(kvp.Value));

        return new LanguageContext(messages);
    }
}