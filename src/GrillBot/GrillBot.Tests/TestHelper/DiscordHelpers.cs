using Discord;
using Moq;

namespace GrillBot.Tests.TestHelper
{
    public static class DiscordHelpers
    {
        public static Mock<IUser> CreateUserMock(ulong id, string username)
        {
            var user = new Mock<IUser>();

            user.Setup(o => o.Id).Returns(id);
            user.Setup(o => o.Username).Returns(username);
            user.Setup(o => o.Discriminator).Returns("9999");

            return user;
        }

        public static Mock<IGuildUser> CreateGuildUserMock(ulong id, string username, string nickname = null)
        {
            var user = new Mock<IGuildUser>();

            user.Setup(o => o.Id).Returns(id);
            user.Setup(o => o.Username).Returns(username);

            if (!string.IsNullOrEmpty(nickname))
                user.Setup(o => o.Nickname).Returns(nickname);

            return user;
        }
    }
}
