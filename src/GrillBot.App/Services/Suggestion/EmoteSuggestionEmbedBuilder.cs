using GrillBot.Common;
using GrillBot.Common.Extensions;
using GrillBot.Common.Helpers;
using GrillBot.Database.Entity;

namespace GrillBot.App.Services.Suggestion;

public class EmoteSuggestionEmbedBuilder
{
    private EmoteSuggestion Entity { get; }
    private IUser Author { get; }

    public EmoteSuggestionEmbedBuilder(EmoteSuggestion? entity, IUser? author)
    {
        Entity = entity ?? throw new ArgumentNullException(nameof(entity));
        Author = author ?? throw new ArgumentNullException(nameof(author));
    }

    public Embed Build()
    {
        var builder = new EmbedBuilder()
            .WithAuthor(Author)
            .WithColor(Color.Blue)
            .WithTitle("Nový návrh na emote")
            .WithTimestamp(Entity.CreatedAt)
            .AddField("Název emote", Entity.EmoteName);

        if (!string.IsNullOrEmpty(Entity.Description))
            builder = builder.AddField("Popis", Entity.Description);

        if (Entity.VoteMessageId != null)
        {
            builder.WithDescription(
                (Entity.VoteFinished ? "Hlasování skončilo " : "Hlasování běží, skončí ") + Entity.VoteEndsAt!.Value.ToCzechFormat()
            );

            if (Entity.VoteFinished)
            {
                builder
                    .WithTitle("Dokončeno hlasování o novém emote")
                    .AddField("Komunitou schválen", FormatHelper.FormatBooleanToCzech(Entity.CommunityApproved), true)
                    .AddField(Emojis.ThumbsUp.ToString(), Entity.UpVotes, true)
                    .AddField(Emojis.ThumbsDown.ToString(), Entity.DownVotes, true)
                    .WithColor(Entity.CommunityApproved ? Color.Green : Color.Red);
            }
        }
        else if (Entity.ApprovedForVote != null)
        {
            builder.WithDescription(Entity.ApprovedForVote.Value ? "Schválen k hlasování" : "Zamítnut k hlasování");
        }

        return builder.Build();
    }
}
