using System.Collections.ObjectModel;
using System.Linq;
using Discord;
using GrillBot.App.Actions.Commands.Guild;
using GrillBot.App.Managers;
using GrillBot.Common.Helpers;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands.Guild;

[TestClass]
public class GuildInfoTests : CommandActionTest<GuildInfo>
{
    private static readonly GuildEmote[] Emotes = { EmoteHelper.CreateGuildEmote(Consts.OnlineEmote) };
    private static readonly IGuildUser GuildUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

    private static readonly ITextChannel[] TextChannels =
    {
        new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).Build(),
        new ThreadBuilder(Consts.ThreadId, Consts.ThreadName).Build()
    };

    protected override IGuildUser User => GuildUser;

    protected override IGuild Guild => new GuildBuilder(Consts.GuildId, Consts.GuildName)
        .SetEmotes(Emotes)
        .SetGetBansAction(new Collection<IBan> { new BanBuilder().SetUser(GuildUser).Build() })
        .SetGetTextChannelsAction(TextChannels)
        .SetGetCategoriesAction(Enumerable.Empty<ICategoryChannel>())
        .SetGetVoiceChannelsAction(Enumerable.Empty<IVoiceChannel>())
        .SetOwner(GuildUser)
        .SetFeatures(GuildFeature.Partnered | GuildFeature.Banner | GuildFeature.Commerce | GuildFeature.Discoverable)
        .SetDescription(Consts.GuildDescription)
        .SetBanner(Consts.BannerId, Consts.BannerUrl)
        .Build();

    protected override GuildInfo CreateInstance()
    {
        var texts = TestServices.Texts.Value;
        var guildHelper = new GuildHelper(texts);
        var userManager = new UserManager(DatabaseBuilder);

        return InitAction(new GuildInfo(guildHelper, userManager, texts));
    }

    private async Task InitDataAsync()
    {
        var user = Database.Entity.User.FromDiscord(GuildUser);
        user.Flags |= (int)UserFlags.WebAdmin;

        await Repository.AddAsync(user);
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        await InitDataAsync();
        var result = await Instance.ProcessAsync();

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Footer);
        Assert.IsFalse(string.IsNullOrEmpty(result.Title));
        Assert.IsNotNull(result.Thumbnail);
        Assert.IsNotNull(result.Color);
        Assert.IsNotNull(result.Timestamp);
    }
}
