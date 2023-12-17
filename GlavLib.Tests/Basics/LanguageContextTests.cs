using FluentAssertions;
using GlavLib.Basics.MultiLang;

namespace GlavLib.Tests.Basics;

public sealed class LanguageContextTests
{
    [Fact]
    public void It_should_load_language_packs()
    {
        //language=yaml
        const string kyLanguagePack = """
                                      Language: ky
                                      Bundles:
                                        - Prefix: Spectr.Core.SystemErrors
                                          Messages:
                                            InternalServerError: Ички система катасы
                                        - Prefix: Spectr.Core.OtherErrors
                                          Messages:
                                            OtherError: Дагы бир ката
                                      """;

        var languageContext = new LanguageContextBuilder()
                              .AddFromDirectory("./LanguagePacks", "language-pack.*.yaml")
                              .AddFromYaml(kyLanguagePack)
                              .Build();

        languageContext.Messages.Should().BeEquivalentTo(new Dictionary<string, MultiLangMessage>
        {
            ["Spectr.Core.SystemErrors.InternalServerError"] = new(new Dictionary<string, string>
            {
                ["en"] = "Internal server error",
                ["ru"] = "Внутренняя ошибка системы",
                ["ky"] = "Ички система катасы"
            }),
            ["Spectr.Core.OtherErrors.OtherError"] = new(new Dictionary<string, string>
            {
                ["en"] = "Other error",
                ["ru"] = "Другая ошибка",
                ["ky"] = "Дагы бир ката"
            }),
        });
    }
}