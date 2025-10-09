using System.Globalization;
using System.Text.RegularExpressions;
using GrillBot.Common.Extensions;
using GrillBot.Core.Extensions;
using GrillBot.Models;
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
        => this[new LocalizedMessageContent(id, []), locale];

    public string this[LocalizedMessageContent content, string locale]
        => Get(content, locale) ?? throw new ArgumentException($"Localized text with id {content.Key} for locale {locale} is missing and there is no default locale either.");

    public string? GetIfExists(string id, string locale)
        => Get(new(id, []), locale);

    public string? GetIfExists(LocalizedMessageContent content, string locale)
        => Get(content, locale) ?? Get(content, DefaultLocale);

    private string? Get(LocalizedMessageContent content, string locale)
    {
        Init();

        var key = GetKey(content.Key, locale);
        if (!Data.TryGetValue(key, out var value))
            return null;

        var args = new List<string>();
        foreach (var arg in content.Args)
        {
            var transformedValue = arg;
            if (transformedValue.StartsWith("DateTime:"))
            {
                if (DateTime.TryParseExact(arg.Replace("DateTime:", ""), "o", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
                    transformedValue = (dateTime.Kind == DateTimeKind.Unspecified ? dateTime.WithKind(DateTimeKind.Utc) : dateTime).ToLocalTime().ToCzechFormat();
            }

            args.Add(transformedValue);
        }

        return args.Count == 0 ? value : value.FormatWith([.. args]);
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
