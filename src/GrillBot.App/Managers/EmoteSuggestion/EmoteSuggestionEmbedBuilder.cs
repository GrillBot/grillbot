using GrillBot.Common;
using GrillBot.Common.Extensions;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;

namespace GrillBot.App.Managers.EmoteSuggestion;

public class EmoteSuggestionEmbedBuilder
{
    private ITextsManager Texts { get; }
    private FormatHelper FormatHelper { get; }

    public EmoteSuggestionEmbedBuilder(ITextsManager texts)
    {
        Texts = texts;
        FormatHelper = new FormatHelper(texts);
    }

    public Embed Build(Database.Entity.EmoteSuggestion entity, IUser author, string locale)
    {
        var builder = new EmbedBuilder()
            .WithAuthor(author)
            .WithColor(Color.Blue)
            .WithTitle(Texts["SuggestionModule/SuggestionEmbed/Title", locale])
            .WithTimestamp(entity.CreatedAt)
            .AddField(Texts["SuggestionModule/SuggestionEmbed/EmoteNameTitle", locale], entity.EmoteName);

        if (!string.IsNullOrEmpty(entity.Description))
            builder = builder.AddField(Texts["SuggestionModule/SuggestionEmbed/EmoteDescriptionTitle", locale], entity.Description);

        if (entity.VoteMessageId != null)
        {
            builder.WithDescription(
                Texts[$"SuggestionModule/SuggestionEmbed/{(entity.VoteFinished ? "VoteFinished" : "VoteRunning")}", locale].FormatWith(entity.VoteEndsAt!.Value.ToCzechFormat())
            );

            if (entity.VoteFinished)
            {
                var communityApproved = FormatHelper.FormatBoolean("SuggestionModule/SuggestionEmbed/Boolean", locale, entity.CommunityApproved);

                builder
                    .WithTitle(Texts["SuggestionModule/SuggestionEmbed/VoteFinishedTitle", locale])
                    .AddField(Texts["SuggestionModule/SuggestionEmbed/CommunityApproved", locale], communityApproved, true)
                    .AddField(Emojis.ThumbsUp.ToString(), entity.UpVotes, true)
                    .AddField(Emojis.ThumbsDown.ToString(), entity.DownVotes, true)
                    .WithColor(entity.CommunityApproved ? Color.Green : Color.Red);
            }
        }
        else if (entity.ApprovedForVote != null)
        {
            var approveForVote = FormatHelper.FormatBoolean("SuggestionModule/SuggestionEmbed/ApproveForVote", locale, entity.ApprovedForVote.Value);
            builder.WithDescription(approveForVote);
        }

        return builder.Build();
    }
}
