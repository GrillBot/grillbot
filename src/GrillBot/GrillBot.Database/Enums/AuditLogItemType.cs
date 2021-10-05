using System.ComponentModel.DataAnnotations;

namespace GrillBot.Database.Enums
{
    public enum AuditLogItemType
    {
        None = 0,

        /// <summary>
        /// Information text.
        /// </summary>
        [Display(Name = "Informační")]
        Info = 1,

        /// <summary>
        /// Warning text.
        /// </summary>
        [Display(Name = "Varování")]
        Warning = 2,

        /// <summary>
        /// Errors
        /// </summary>
        [Display(Name = "Chyba")]
        Error = 3,

        /// <summary>
        /// Executed command.
        /// </summary>
        [Display(Name = "Příkaz")]
        Command = 4,

        /// <summary>
        /// Channel created.
        /// </summary>
        [Display(Name = "Vytvořen kanál")]
        ChannelCreated = 5,

        /// <summary>
        /// Channel deleted.
        /// </summary>
        [Display(Name = "Smazán kanál")]
        ChannelDeleted = 6,

        /// <summary>
        /// Channel updated.
        /// </summary>
        [Display(Name = "Upraven kanál")]
        ChannelUpdated = 7,

        /// <summary>
        /// Emote deleted.
        /// </summary>
        [Display(Name = "Smazán emote")]
        EmojiDeleted = 8,

        /// <summary>
        /// Channel overwrite created.
        /// </summary>
        [Display(Name = "Vytvořena výjimka do kanálu")]
        OverwriteCreated = 9,

        /// <summary>
        /// Channel overwrite deleted.
        /// </summary>
        [Display(Name = "Smazána výjimka do kanálu")]
        OverwriteDeleted = 10,

        /// <summary>
        /// Channel overwrite updated.
        /// </summary>
        [Display(Name = "Upravena výjimka do kanálu")]
        OverwriteUpdated = 11,

        /// <summary>
        /// User unbanned.
        /// </summary>
        [Display(Name = "Odblokován uživatel")]
        Unban = 12,

        /// <summary>
        /// Guild user updated.
        /// </summary>
        [Display(Name = "Upraven uživatel")]
        MemberUpdated = 13,

        /// <summary>
        /// Member role updated.
        /// </summary>
        [Display(Name = "Role uživatele upraveny.")]
        MemberRoleUpdated = 14,

        /// <summary>
        /// Guild updated.
        /// </summary>
        [Display(Name = "Server upraven")]
        GuildUpdated = 15,

        /// <summary>
        /// User left guild.
        /// </summary>
        [Display(Name = "Uživatel opustil server")]
        UserLeft = 16,

        /// <summary>
        /// User joined guild.
        /// </summary>
        [Display(Name = "Uživatel se připojil na server")]
        UserJoined = 17,

        /// <summary>
        /// Message modified.
        /// </summary>
        [Display(Name = "Zpráva upravena")]
        MessageEdited = 18,

        /// <summary>
        /// Message removed.
        /// </summary>
        [Display(Name = "Zpráva odebrána")]
        MessageDeleted = 19
    }
}
