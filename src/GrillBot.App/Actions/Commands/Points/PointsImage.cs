using GrillBot.App.Infrastructure.IO;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Services.Graphics;
using GrillBot.Common.Services.Graphics.Models.Images;
using GrillBot.Data.Exceptions;
using ImageMagick;

namespace GrillBot.App.Actions.Commands.Points;

public sealed class PointsImage : CommandAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ProfilePictureManager ProfilePictureManager { get; }
    private IGraphicsClient GraphicsClient { get; }

    public PointsImage(GrillBotDatabaseBuilder databaseBuilder, ProfilePictureManager profilePictureManager, IGraphicsClient graphicsClient)
    {
        ProfilePictureManager = profilePictureManager;
        DatabaseBuilder = databaseBuilder;
        GraphicsClient = graphicsClient;
    }

    public async Task<TemporaryFile> ProcessAsync(IGuild guild, IUser user)
    {
        var guildUser = user as IGuildUser ?? await guild.GetUserAsync(user.Id);
        if (guildUser == null)
            throw new NotFoundException("Uživatel nebyl nalezen na serveru.");

        await using var repository = DatabaseBuilder.CreateRepository();
        if (!await repository.GuildUser.ExistsAsync(guildUser))
            throw new NotFoundException($"{user.GetDisplayName()} ještě neprojevil na serveru žádnou aktivitu.");

        var userPoints = await repository.Points.ComputePointsOfUserAsync(guildUser.GuildId, guildUser.Id);
        var position = await repository.Points.CalculatePointsPositionAsync(guildUser, userPoints);

        using var profilePicture = await GetProfilePictureAsync(user);
        var dominantColor = profilePicture.GetDominantColor();
        var textBackground = dominantColor.CreateDarkerBackgroundColor();

        profilePicture.Resize(250, 250);
        profilePicture.RoundImage();

        var request = new PointsImageRequest
        {
            Points = userPoints,
            Position = position,
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
