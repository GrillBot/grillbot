using System.ComponentModel;

namespace GrillBot.Data.Enums;

public enum UselessPermissionType
{
    [Description("Administrator")]
    Administrator,

    [Description("Neutrální oprávnění")]
    Neutral,

    [Description("Dostupné přes roli")]
    AvailableFromRole
}
