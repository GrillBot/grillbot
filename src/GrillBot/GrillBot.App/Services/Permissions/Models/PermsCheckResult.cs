namespace GrillBot.App.Services.Permissions;

/// <summary>
/// Class that returns
/// </summary>
public class PermsCheckResult
{
    /// <summary>
    /// Channel is banned to use some commands.
    /// </summary>
    public bool ChannelDisabled { get; set; }

    /// <summary>
    /// Context (Guild/DM) check result. Null means check not processed.
    /// </summary>
    public bool? ContextCheck { get; set; }

    /// <summary>
    /// Check if user is administrator.
    /// </summary>
    public bool IsAdmin { get; set; }

    /// <summary>
    /// Check if caller have explicit ban to command.
    /// </summary>
    public bool ExplicitBan { get; set; }

    /// <summary>
    /// Check if caller have explicit allow to command. Have lower priority than ExplicitBan.
    /// </summary>
    public bool? ExplicitAllow { get; set; }

    public bool? GuildPermissions { get; set; }
    public bool? ChannelPermissions { get; set; }
    public bool? BoosterAllowed { get; set; }

    public bool IsAllowed()
    {
        if (ContextCheck == false) return false;
        if (IsAdmin) return true;
        if (ChannelDisabled || ExplicitBan) return false;

        return ExplicitAllow == true || BoosterAllowed == true || GuildPermissions == true || ChannelPermissions == true;
    }

    public override string ToString()
    {
        if (IsAllowed()) return "";

        if (ExplicitBan) return "Byl ti zakázán přístup k tomuto příkazu.";
        if (ChannelDisabled) return "V tomto kanálu byly příkazy deaktivovány.";
        if (ContextCheck == false) return "Voláš příkaz tam, kde jej nelze spustit.";

        if (GuildPermissions != null && GuildPermissions == false)
            return "Nesplňuješ podmínky pro spuštění příkazu na serveru.";

        if (ChannelPermissions != null && ChannelPermissions == false)
            return "Nesplňuješ podmínky pro spuštění příkazu v kanálu.";

        return "Nesplňuješ nějakou jinou nespecifikovanou podmínku.";
    }
}
