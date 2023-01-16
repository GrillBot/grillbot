using Discord;

namespace GrillBot.Tests.TestHelpers;

public static class RoleHelper
{
    public static RoleTags CreateTags(ulong? botId, ulong? integrationId, bool isPremiumSubscriber)
    {
        var constructorParameters = new object[] { botId!, integrationId!, isPremiumSubscriber };
        return ReflectionHelper.CreateWithInternalConstructor<RoleTags>(constructorParameters);
    }
}
