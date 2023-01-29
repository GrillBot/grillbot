using Discord.Interactions;
using GrillBot.App.Infrastructure.TypeReaders.Interactions;

namespace GrillBot.App.Infrastructure.TypeReaders;

public static class TypeReadersExtensions
{
    public static void RegisterTypeConverters(this InteractionService service)
    {
        service.AddTypeConverter<IMessage>(new MessageTypeConverter());
        service.AddTypeConverter<IEmote>(new EmotesTypeConverter());
        service.AddTypeConverter<bool>(new BooleanTypeConverter());
        service.AddTypeConverter<DateTime>(new DateTimeTypeConverter());
        service.AddTypeConverter<IUser[]>(new UsersTypeConverter());
    }
}
