namespace GrillBot.App.Services.Permissions.Models;

public abstract class CheckRequestBase
{
    public GuildPermission[] GuildPermissions { get; set; }
    public ChannelPermission[] ChannelPermissions { get; set; }
    public bool AllowBooster { get; set; }

    public abstract IUser User { get; }
    public abstract IGuild Guild { get; }
    public abstract IMessageChannel Channel { get; }
    public abstract IDiscordClient DiscordClient { get; }
    public abstract string CommandName { get; }

    /// <summary>
    /// Oprava implicitních oprávnění. Pokud není nastaveno žádné pravidlo, tak se nastaví výchozí "ViewChannel".
    /// </summary>
    public void FixImplicitPermissions()
    {
        if (!AnySet())
            ChannelPermissions = new[] { ChannelPermission.ViewChannel };
    }

    /// <summary>
    /// Kontrola, zda lze nastaveno nějaké pravidlo.
    /// </summary>
    protected virtual bool AnySet()
    {
        var notSet = (GuildPermissions == null || GuildPermissions.Length == 0) &&
            (ChannelPermissions == null || GuildPermissions.Length == 0) &&
            !AllowBooster;

        return !notSet;
    }
}
