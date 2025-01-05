namespace GrillBot.Common.Managers.Cooldown;

public class CooldownItem
{
    public int Used { get; set; }
    public int Max { get; set; }
    public DateTime? Until { get; set; }
}
