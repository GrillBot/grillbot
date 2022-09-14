using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace GrillBot.Common.Managers;

public class LocalizationManager
{
    private const string DefaultLocale = "en-US";
    private readonly string[] _supportedLocales = { "cs", DefaultLocale };

    // Dictionary<Id#Locale, Value>
    private Dictionary<string, string> Data { get; set; }

    private string BasePath { get; }
    private string FileMask { get; }

    private readonly Regex _localeParserRegex = new("\\w+.(?<locale>\\w{2}(?:-\\w{2})?).json", RegexOptions.Compiled | RegexOptions.Singleline);

    public LocalizationManager(string basePath, string fileMask)
    {
        Data = new Dictionary<string, string>();
        BasePath = basePath;
        FileMask = fileMask;
    }

    private void Init()
    {
        if (Data.Count > 0) return;
        var files = Directory.GetFiles(BasePath, $"{FileMask}.*.json", SearchOption.AllDirectories);
        var result = new Dictionary<string, string>();

        foreach (var file in files)
        {
            var locale = _localeParserRegex.Match(file).Groups["locale"].Value;
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

    public string Get(string id, string locale)
    {
        return Get(GetKey(id, locale)) ?? Get(GetKey(id, DefaultLocale))
            ?? throw new ArgumentException($"Localized text with id {id} for locale {locale} is missing and there is no default locale either.");
    }

    private string? Get(string textId)
    {
        Init();
        return Data.TryGetValue(textId, out var value) ? value : null;
    }

    private static string GetKey(string id, string locale) => $"{id}#{locale}";

    public CultureInfo GetCulture(string locale)
    {
        return !IsSupportedLocale(locale) ? new CultureInfo(DefaultLocale) : new CultureInfo(locale);
    }

    private bool IsSupportedLocale(string locale)
        => locale == "cs-CZ" ? _supportedLocales.Contains("cs") : _supportedLocales.Contains(locale);
}
