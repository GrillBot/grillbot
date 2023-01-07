using Discord;

namespace GrillBot.Data.Models.AuditLog;

public class EmbedFieldInfo
{
    public string Name { get; set; }
    public string Value { get; set; }
    public bool Inline { get; set; }

    public EmbedFieldInfo()
    {
    }

    public EmbedFieldInfo(EmbedField field)
    {
        Name = field.Name;
        Value = field.Value;
        Inline = field.Inline;
    }
}
