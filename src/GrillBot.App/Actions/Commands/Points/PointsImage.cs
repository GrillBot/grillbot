using GrillBot.App.Infrastructure.IO;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Resources.Misc;
using ImageMagick;

namespace GrillBot.App.Actions.Commands.Points;

public sealed class PointsImage : CommandAction, IDisposable
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ProfilePictureManager ProfilePictureManager { get; }
    
    private MagickImage TrophyImage { get; }

    public PointsImage(GrillBotDatabaseBuilder databaseBuilder, ProfilePictureManager profilePictureManager)
    {
        ProfilePictureManager = profilePictureManager;
        DatabaseBuilder = databaseBuilder;
        
        TrophyImage = new MagickImage(MiscResources.trophy, MagickFormat.Png);
    }

    public async Task<TemporaryFile> ProcessAsync(IGuild guild, IUser user)
    {
        var guildUser = user as IGuildUser ?? await guild.GetUserAsync(user.Id);

        await using var repository = DatabaseBuilder.CreateRepository();
        if (!await repository.GuildUser.ExistsAsync(guildUser))
            throw new NotFoundException($"{user.GetDisplayName()} ještě neprojevil na serveru žádnou aktivitu.");

        const int height = 340;
        const int width = 1000;
        const int border = 25;
        const double nicknameFontSize = 80;
        const int profilePictureSize = 250;
        const string fontName = "Open Sans";

        var userPoints = await repository.Points.ComputePointsOfUserAsync(guildUser.GuildId, guildUser.Id);
        var position = await repository.Points.CalculatePointsPositionAsync(guildUser, userPoints);
        var nickname = user.GetDisplayName(false);

        using var profilePicture = await GetProfilePictureAsync(user);
        var cuttedNickname = nickname.CutToImageWidth(width - border * 4 - profilePictureSize, fontName, nicknameFontSize);

        var dominantColor = profilePicture.GetDominantColor();
        var textBackground = dominantColor.CreateDarkerBackgroundColor();

        using var image = new MagickImage(dominantColor, width, height);

        var drawable = new Drawables()
            .StrokeAntialias(true)
            .TextAntialias(true)
            .FillColor(textBackground)
            .RoundRectangle(border, border, width - border, height - border, 20, 20)
            .TextAlignment(TextAlignment.Left)
            .Font(fontName)
            .FontPointSize(nicknameFontSize)
            .FillColor(MagickColors.White)
            .Text(320, 130, cuttedNickname);

        // Profile picture operations
        profilePicture.Resize(profilePictureSize, profilePictureSize);
        profilePicture.RoundImage();
        drawable.Composite(border * 2, border * 2, CompositeOperator.Over, profilePicture);

        double pointsInfoX = 320;
        if (position == 1)
        {
            drawable.Composite(pointsInfoX, 170, CompositeOperator.Over, TrophyImage);
            pointsInfoX += TrophyImage.Width + 10;
        }

        var pointsInfo = $"{position}. místo\n{FormatHelper.FormatPointsToCzech(userPoints)}";
        drawable
            .Font("Arial")
            .FontPointSize(60)
            .Text(pointsInfoX, 210, pointsInfo);

        drawable.Draw(image);

        var tmpFile = new TemporaryFile("png");
        await image.WriteAsync(tmpFile.Path, MagickFormat.Png);
        return tmpFile;
    }

    private async Task<MagickImage> GetProfilePictureAsync(IUser user)
    {
        var profilePicture = await ProfilePictureManager.GetOrCreatePictureAsync(user, 256);
        return new MagickImage(profilePicture.Data);
    }

    public void Dispose()
    {
        TrophyImage.Dispose();
    }
}
