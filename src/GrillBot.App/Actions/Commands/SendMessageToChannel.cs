using System.Net.Http;
using GrillBot.Common.Managers.Localization;
using GrillBot.Data.Models.API.Channels;

namespace GrillBot.App.Actions.Commands;

public class SendMessageToChannel : CommandAction
{
    private HttpClient Client { get; }
    private Actions.Api.V1.Channel.SendMessageToChannel SendAction { get; }
    private ITextsManager Texts { get; }

    /// <summary>
    /// A list of objects that must be freed only after all actions have been completed.
    /// </summary>
    private List<IDisposable> Disposables { get; } = new();

    public SendMessageToChannel(IHttpClientFactory factory, Actions.Api.V1.Channel.SendMessageToChannel sendAction, ITextsManager texts)
    {
        Client = factory.CreateClient();
        SendAction = sendAction;
        Texts = texts;
    }

    public async Task ProcessAsync(ITextChannel channel, string? reference, string? content, IEnumerable<IAttachment?> attachments)
    {
        SendAction.UpdateContext(Locale, Context.User);
        var parameters = new SendMessageToChannelParams
        {
            Content = content?.Trim() ?? "",
            Reference = reference
        };

        try
        {
            await DownloadAttachmentsAsync(parameters, attachments);
            CheckParameters(parameters);
            await SendAction.ProcessAsync(channel.GuildId, channel.Id, parameters);
        }
        finally
        {
            Disposables.ForEach(o => o.Dispose());
        }
    }

    private async Task DownloadAttachmentsAsync(SendMessageToChannelParams parameters, IEnumerable<IAttachment?> attachments)
    {
        foreach (var attachment in attachments.Where(o => o != null))
        {
            var fileAttachment = await CreateAttachmentAsync(attachment);
            if (fileAttachment != null) parameters.Attachments.Add(fileAttachment.Value);
        }
    }

    private void CheckParameters(SendMessageToChannelParams parameters)
    {
        if (string.IsNullOrEmpty(parameters.Content) && parameters.Attachments.Count == 0)
            throw new ValidationException(Texts["ChannelModule/PostMessage/NoContent", Locale]);
    }

    private async Task<FileAttachment?> CreateAttachmentAsync(IAttachment attachment)
    {
        var response = await Client.GetAsync(attachment.Url);
        Disposables.Add(response);

        if (!response.IsSuccessStatusCode)
        {
            response = await Client.GetAsync(attachment.ProxyUrl);
            Disposables.Add(response);
        }

        if (!response.IsSuccessStatusCode) return null;

        var stream = await response.Content.ReadAsStreamAsync();
        var spoiler = attachment.IsSpoiler();
        var fileAttachment = new FileAttachment(stream, attachment.Filename, attachment.Description, spoiler);
        Disposables.Add(fileAttachment);

        return fileAttachment;
    }
}
