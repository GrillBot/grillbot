using GrillBot.Models;
using System.Globalization;

namespace GrillBot.Common.Managers.Localization;

public interface ITextsManager
{
    string this[string id, string locale] { get; }
    string this[LocalizedMessageContent content, string locale] { get; }
    string? GetIfExists(string id, string locale);
    string? GetIfExists(LocalizedMessageContent content, string locale);
    CultureInfo GetCulture(string locale);
}
