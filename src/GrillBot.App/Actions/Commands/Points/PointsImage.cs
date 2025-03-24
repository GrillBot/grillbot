using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Services.ImageProcessing;
using GrillBot.Core.Services.ImageProcessing.Models;
using GrillBot.Core.Services.PointsService;
using GrillBot.Core.Exceptions;
using GrillBot.Core.IO;
using GrillBot.Core.Services.Common.Executor;

namespace GrillBot.App.Actions.Commands.Points;

public sealed class PointsImage : CommandAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ProfilePictureManager ProfilePictureManager { get; }
    private ITextsManager Texts { get; }
    private readonly IServiceClientExecutor<IPointsServiceClient> _pointsServiceClient;
    private readonly IServiceClientExecutor<IImageProcessingClient> _imageProcessingClient;

    public PointsImage(GrillBotDatabaseBuilder databaseBuilder, ProfilePictureManager profilePictureManager, ITextsManager texts, IServiceClientExecutor<IPointsServiceClient> pointsServiceClient,
         IServiceClientExecutor<IImageProcessingClient> imageProcessingClient)
    {
        ProfilePictureManager = profilePictureManager;
        DatabaseBuilder = databaseBuilder;
        Texts = texts;
        _pointsServiceClient = pointsServiceClient;
        _imageProcessingClient = imageProcessingClient;
    }

    public async Task<TemporaryFile> ProcessAsync(IGuild guild, IUser user)
    {
        var guildUser = user as IGuildUser ?? await guild.GetUserAsync(user.Id);
        if (guildUser == null)
            throw new NotFoundException(Texts["Points/Image/NotFound", Locale]);

        if (!guildUser.IsUser())
            throw new InvalidOperationException(Texts["Points/Image/IsBot", Locale]);

        using var repository = DatabaseBuilder.CreateRepository();
        if (!await repository.GuildUser.ExistsAsync(guildUser))
            throw new NotFoundException(Texts["Points/Image/NoActivity", Locale].FormatWith(user.GetDisplayName()));

        var profilePicture = await ProfilePictureManager.GetOrCreatePictureAsync(user, 256);
        var status = await _pointsServiceClient.ExecuteRequestAsync((c, cancellationToken) => c.GetImagePointsStatusAsync(guildUser.GuildId.ToString(), guildUser.Id.ToString(), cancellationToken));
        if (status is null)
            throw new NotFoundException(Texts["Points/Image/NoActivity", Locale].FormatWith(user.GetDisplayName()));

        var request = new PointsRequest
        {
            Position = status.Position,
            Username = guildUser.GetDisplayName(),
            AvatarInfo = new AvatarInfo
            {
                Type = "png",
                AvatarContent = profilePicture.Data,
                AvatarId = profilePicture.AvatarId
            },
            PointsValue = status.Points,
            UserId = user.Id.ToString()
        };

        var image = await _imageProcessingClient.ExecuteRequestAsync((c, cancellationToken) => c.CreatePointsImageAsync(request, cancellationToken));
        var result = new TemporaryFile("png");

        await result.WriteStreamAsync(image);
        return result;
    }
}
