using GrillBot.App.Helpers;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Services.PointsService;
using GrillBot.Core.Services.PointsService.Models;

namespace GrillBot.App.Handlers.MessageReceived;

public class PointsMessageReceivedHandler : IMessageReceivedEvent
{
    private PointsHelper Helper { get; }
    private IPointsServiceClient PointsServiceClient { get; }

    public PointsMessageReceivedHandler(PointsHelper helper, IPointsServiceClient pointsServiceClient)
    {
        Helper = helper;
        PointsServiceClient = pointsServiceClient;
    }

    public async Task ProcessAsync(IMessage message)
    {
        if (!Helper.CanIncrementPoints(message) || message.Channel is not ITextChannel textChannel) return;

        var request = new TransactionRequest
        {
            GuildId = textChannel.GuildId.ToString(),
            ChannelId = textChannel.Id.ToString(),
            MessageInfo = new MessageInfo
            {
                Id = message.Id.ToString(),
                AuthorId = message.Author.Id.ToString(),
                ContentLength = message.Content.Length,
                MessageType = message.Type
            }
        };

        var validationErrors = await PointsServiceClient.CreateTransactionAsync(request);
        if (PointsHelper.CanSyncData(validationErrors))
        {
            await Helper.SyncDataWithServiceAsync(textChannel.Guild, new[] { message.Author }, new[] { textChannel });
            validationErrors = await PointsServiceClient.CreateTransactionAsync(request);
        }

        if (validationErrors is not null)
            throw new ValidationException(JsonConvert.SerializeObject(validationErrors));
    }
}
