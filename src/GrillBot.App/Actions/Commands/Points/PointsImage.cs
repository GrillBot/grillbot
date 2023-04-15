using GrillBot.App.Infrastructure.IO;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Services.Graphics;
using GrillBot.Common.Services.Graphics.Models.Images;
using GrillBot.Common.Services.PointsService;
using GrillBot.Core.Exceptions;
using ImageMagick;

namespace GrillBot.App.Actions.Commands.Points;

public sealed class PointsImage : CommandAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ProfilePictureManager ProfilePictureManager { get; }
    private IGraphicsClient GraphicsClient { get; }
    private ITextsManager Texts { get; }
    private IPointsServiceClient PointsServiceClient { get; }

    public PointsImage(GrillBotDatabaseBuilder databaseBuilder, ProfilePictureManager profilePictureManager, IGraphicsClient graphicsClient, ITextsManager texts,
        IPointsServiceClient pointsServiceClient)
    {
        ProfilePictureManager = profilePictureManager;
        DatabaseBuilder = databaseBuilder;
        GraphicsClient = graphicsClient;
        Texts = texts;
        PointsServiceClient = pointsServiceClient;
    }

    public async Task<TemporaryFile> ProcessAsync(IGuild guild, IUser user)
    {
        var guildUser = user as IGuildUser ?? await guild.GetUserAsync(user.Id);
        if (guildUser == null)
            throw new NotFoundException(Texts["Points/Image/NotFound", Locale]);

        await using var repository = DatabaseBuilder.CreateRepository();
        if (!await repository.GuildUser.ExistsAsync(guildUser))
            throw new NotFoundException(Texts["Points/Image/NoActivity", Locale].FormatWith(user.GetDisplayName()));

        var status = await PointsServiceClient.GetImagePointsStatusAsync(guildUser.GuildId.ToString(), guildUser.Id.ToString());

        using var profilePicture = await GetProfilePictureAsync(user);
        var dominantColor = profilePicture.GetDominantColor();
        var textBackground = dominantColor.CreateDarkerBackgroundColor();

        var request = new PointsImageRequest
        {
            Points = status.Points,
            Position = status.Position,
            Nickname = user.GetDisplayName(false),
            BackgroundColor = dominantColor.ToHexString(),
            TextBackground = textBackground.ToHexString(),
            ProfilePicture = profilePicture.ToBase64()
        };

        var image = await GraphicsClient.CreatePointsImageAsync(request);

        var tmpFile = new TemporaryFile("png");
        await File.WriteAllBytesAsync(tmpFile.Path, image);
        return tmpFile;
    }

    private async Task<MagickImage> GetProfilePictureAsync(IUser user)
    {
        var profilePicture = await ProfilePictureManager.GetOrCreatePictureAsync(user, 256);
        return new MagickImage(profilePicture.Data);
    }
}
