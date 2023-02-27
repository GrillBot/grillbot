using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Discord;
using GrillBot.App.Actions.Api.V1.AuditLog;
using GrillBot.App.Managers;
using GrillBot.Data.Models;
using GrillBot.Data.Models.API.AuditLog.Filters;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Models;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;
using Newtonsoft.Json;

namespace GrillBot.Tests.App.Actions.Api.V1.AuditLog;

[TestClass]
public class GetAuditLogListTests : ApiActionTest<GetAuditLogList>
{
    protected override GetAuditLogList CreateInstance()
    {
        return new GetAuditLogList(ApiRequestContext, DatabaseBuilder, TestServices.AutoMapper.Value, TestServices.Texts.Value);
    }

    [TestMethod]
    public async Task ProcessAsync_EmptyFilter()
    {
        var filter = new AuditLogListParams();
        var result = await Instance.ProcessAsync(filter);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.TotalItemsCount);
    }

    [TestMethod]
    public async Task ProcessAsync_WithFilter()
    {
        await InitAllTypesAsync();

        var filter = new AuditLogListParams
        {
            Types = Enum.GetValues<AuditLogItemType>().ToList(),
            ChannelId = Consts.ChannelId.ToString(),
            CreatedFrom = DateTime.MinValue,
            CreatedTo = DateTime.MaxValue,
            GuildId = Consts.GuildId.ToString(),
            IgnoreBots = true,
            ProcessedUserIds = new List<string> { Consts.UserId.ToString() }
        };

        var result = await Instance.ProcessAsync(filter);

        Assert.IsNotNull(result);
        Assert.AreNotEqual(0, result.TotalItemsCount);
    }

    [TestMethod]
    public async Task ProcessAsync_FilterWithExcludes()
    {
        await InitAllTypesAsync();

        var filter = new AuditLogListParams
        {
            ChannelId = Consts.ChannelId.ToString(),
            CreatedFrom = DateTime.MinValue,
            CreatedTo = DateTime.MaxValue,
            GuildId = Consts.GuildId.ToString(),
            IgnoreBots = true,
            ProcessedUserIds = new List<string> { Consts.UserId.ToString() },
            Types = new List<AuditLogItemType> { AuditLogItemType.Info },
            ExcludedTypes = new List<AuditLogItemType> { AuditLogItemType.MemberRoleUpdated }
        };

        var result = await Instance.ProcessAsync(filter);

        Assert.IsNotNull(result);
        Assert.IsTrue(result.TotalItemsCount > 0);
    }

    [TestMethod]
    public async Task ProcessAsync_FilterWithExtendedFilters()
    {
        await InitAllTypesAsync();

        var filter = new AuditLogListParams
        {
            Types = Enum.GetValues<AuditLogItemType>().ToList(),
            CommandFilter = new ExecutionFilter
            {
                Duration = new RangeParams<int> { From = int.MinValue, To = int.MaxValue },
                Name = "dd",
                WasSuccess = true
            },
            ErrorFilter = new TextFilter { Text = "T" },
            InfoFilter = new TextFilter { Text = "T" },
            InteractionFilter = new ExecutionFilter
            {
                WasSuccess = true,
                Name = "ddddddd",
                Duration = new RangeParams<int> { From = int.MinValue, To = int.MaxValue }
            },
            JobFilter = new ExecutionFilter
            {
                WasSuccess = true,
                Name = "ddddddd",
                Duration = new RangeParams<int> { From = int.MinValue, To = int.MaxValue }
            },
            WarningFilter = new TextFilter { Text = "T" },
            Ids = string.Join(", ", Enumerable.Range(0, 1000).Select(o => o.ToString())),
            ApiRequestFilter = new ApiRequestFilter
            {
                Duration = new RangeParams<int> { From = int.MinValue, To = int.MaxValue },
                Method = "Method",
                ActionName = "Action",
                ControllerName = "Controller",
                PathTemplate = "PathTemplate",
                LoggedUserRole = "User",
                ApiGroupName = "V1"
            },
            MemberUpdatedFilter = new TargetIdFilter { TargetId = Consts.UserId.ToString() },
            OverwriteCreatedFilter = new TargetIdFilter { TargetId = Consts.UserId.ToString() },
            OverwriteUpdatedFilter = new TargetIdFilter { TargetId = Consts.UserId.ToString() },
            OverwriteDeletedFilter = new TargetIdFilter { TargetId = Consts.UserId.ToString() },
            MessageDeletedFilter = new MessageDeletedFilter
            {
                AuthorId = Consts.UserId.ToString(),
                ContainsEmbed = false,
                ContentContains = "Test"
            }
        };

        var result = await Instance.ProcessAsync(filter);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_Validation_Ids()
    {
        var filter = new AuditLogListParams { Ids = "abcd" };
        await Instance.ProcessAsync(filter);
    }

    [TestMethod]
    [ExpectedException(typeof(ValidationException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_Validation_TypesCombination()
    {
        var filter = new AuditLogListParams
        {
            Types = new List<AuditLogItemType> { AuditLogItemType.Api },
            ExcludedTypes = new List<AuditLogItemType> { AuditLogItemType.Api }
        };

        await Instance.ProcessAsync(filter);
    }

    private async Task InitAllTypesAsync()
    {
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
        var channel = new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(guild).Build();
        var user = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        var items = new (AuditLogItemType, object)[]
        {
            (AuditLogItemType.Info, ""),
            (AuditLogItemType.Info, "Info"),
            (AuditLogItemType.Warning, "Warning"),
            (AuditLogItemType.Error, "Error"),
            (AuditLogItemType.Command, new CommandExecution { Command = "C" }),
            (AuditLogItemType.ChannelCreated, new AuditChannelInfo()),
            (AuditLogItemType.ChannelDeleted, new AuditChannelInfo()),
            (AuditLogItemType.ChannelUpdated, new Diff<AuditChannelInfo>(new AuditChannelInfo(), new AuditChannelInfo())),
            (AuditLogItemType.EmojiDeleted, new AuditEmoteInfo()),
            (AuditLogItemType.OverwriteCreated, new AuditOverwriteInfo()),
            (AuditLogItemType.OverwriteDeleted, new AuditOverwriteInfo()),
            (AuditLogItemType.OverwriteUpdated, new Diff<AuditOverwriteInfo>(new AuditOverwriteInfo(), new AuditOverwriteInfo())),
            (AuditLogItemType.Unban, new AuditUserInfo()),
            (AuditLogItemType.MemberUpdated, new MemberUpdatedData { Target = new AuditUserInfo { Id = Consts.UserId + 1, UserId = (Consts.UserId + 1).ToString() } }),
            (AuditLogItemType.MemberRoleUpdated, new MemberUpdatedData { Target = new AuditUserInfo { Id = Consts.UserId + 1, UserId = (Consts.UserId + 1).ToString() } }),
            (AuditLogItemType.GuildUpdated, new GuildUpdatedData()),
            (AuditLogItemType.UserLeft, new UserLeftGuildData()),
            (AuditLogItemType.UserJoined, new UserJoinedAuditData()),
            (AuditLogItemType.MessageEdited, new MessageEditedData()),
            (AuditLogItemType.MessageDeleted, new MessageDeletedData { Data = new MessageData() }),
            (AuditLogItemType.InteractionCommand, new InteractionCommandExecuted()),
            (AuditLogItemType.ThreadDeleted, new AuditThreadInfo()),
            (AuditLogItemType.JobCompleted, new JobExecutionData { JobName = "Job" }),
            (AuditLogItemType.Api, new ApiRequest { ControllerName = "C", ActionName = "A" })
        };
        foreach (var item in items)
        {
            await Repository.AddAsync(new AuditLogItem
            {
                ChannelId = Consts.ChannelId.ToString(),
                CreatedAt = DateTime.UtcNow,
                Data = (item.Item1 > AuditLogItemType.Error ? JsonConvert.SerializeObject(item.Item2, AuditLogWriteManager.SerializerSettings) : item.Item2.ToString()) ?? string.Empty,
                GuildId = Consts.GuildId.ToString(),
                ProcessedUserId = Consts.UserId.ToString(),
                Type = item.Item1
            });
        }

        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(guild));
        await Repository.AddAsync(GuildChannel.FromDiscord(channel, ChannelType.Text));
        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        await Repository.CommitAsync();
    }
}
