using System.Globalization;
using System.Text.RegularExpressions;
using GrillBot.Core.Services.GrillBot.Models;
using Humanizer;
using Newtonsoft.Json.Linq;

namespace GrillBot.Common.Managers.Localization;

public partial class TextsManager(string _basePath, string _fileMask) : ITextsManager
{
    public const string DefaultLocale = "en-US";
    private static readonly string[] SupportedLocales = ["cs", DefaultLocale];

    // Dictionary<Id#Locale, Value>
    private Dictionary<string, string> Data { get; set; } = [];

    private void Init()
    {
        if (Data.Count > 0) return;
        var files = Directory.GetFiles(_basePath, $"{_fileMask}.*.json", SearchOption.AllDirectories);
        var result = new Dictionary<string, string>();

        foreach (var file in files)
        {
            var locale = LocaleParserRegex().Match(file).Groups["locale"].Value;
            var jsonData = JObject.Parse(File.ReadAllText(file));
            Add(result, jsonData, "", locale);
        }

        Data = result;
    }

    private static void Add(IDictionary<string, string> data, JToken token, string prefix, string locale)
    {
        static string Join(string prefix, string name)
            => string.IsNullOrEmpty(prefix) ? name : prefix + "/" + name;

        switch (token.Type)
        {
            case JTokenType.Object:
                foreach (var property in token.Children<JProperty>())
                    Add(data, property.Value, Join(prefix, property.Name), locale);
                break;
            case JTokenType.Array:
                var i = 0;
                foreach (var item in token.Children())
                {
                    Add(data, item, Join(prefix, i.ToString()), locale);
                    i++;
                }

                break;
            default:
                data.Add(GetKey(prefix, locale), (token as JValue)?.Value?.ToString() ?? "");
                break;
        }
    }

    public string this[string id, string locale]
        => Get(id, locale);

    public string this[LocalizedMessageContent content, string locale]
        => Get(content, locale);

    public string? GetIfExists(string id, string locale)
        => Get(new(GetKey(id, locale), [])) ?? Get(new(GetKey(id, DefaultLocale), []));

    public string? GetIfExists(LocalizedMessageContent content, string locale)
        => Get(content, locale) ?? Get(content, DefaultLocale);

    private string Get(string id, string locale)
    {
        return GetIfExists(id, locale)
            ?? throw new ArgumentException($"Localized text with id {id} for locale {locale} is missing and there is no default locale either.");
    }

    private string? Get(LocalizedMessageContent content)
    {
        Init();
        return !Data.TryGetValue(content.Key, out var value) ? null : value.FormatWith(content.Args);
    }

    private static string GetKey(string id, string locale) => $"{id}#{FixLocale(locale)}";

    public CultureInfo GetCulture(string locale)
    {
        return !IsSupportedLocale(locale) ? new CultureInfo(DefaultLocale) : new CultureInfo(FixLocale(locale));
    }

    public static bool IsSupportedLocale(string locale)
        => SupportedLocales.Contains(FixLocale(locale));

    public static string FixLocale(string? locale)
    {
        if (string.IsNullOrEmpty(locale))
            locale = DefaultLocale;

        return locale switch
        {
            "cs-CZ" => "cs",
            "en-GB" => "en-US",
            _ => locale
        };
    }

    [GeneratedRegex("\\w+.(?<locale>\\w{2}(?:-\\w{2})?).json", RegexOptions.Compiled | RegexOptions.Singleline)]
    private static partial Regex LocaleParserRegex();
}
