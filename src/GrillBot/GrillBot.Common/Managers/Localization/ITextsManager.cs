using System.Globalization;

namespace GrillBot.Common.Managers.Localization;

public interface ITextsManager
{
    string this[string id, string locale] { get; }
    CultureInfo GetCulture(string locale);
}
