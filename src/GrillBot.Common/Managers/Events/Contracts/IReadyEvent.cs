namespace GrillBot.Common.Managers.Events.Contracts;

public interface IReadyEvent
{
    Task ProcessAsync();
}
