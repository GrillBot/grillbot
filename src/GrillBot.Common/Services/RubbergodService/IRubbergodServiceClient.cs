﻿using GrillBot.Common.Services.Common;
using GrillBot.Common.Services.RubbergodService.Models.Help;
using GrillBot.Common.Services.RubbergodService.Models.Karma;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.Diagnostics.Models;

namespace GrillBot.Common.Services.RubbergodService;

public interface IRubbergodServiceClient : IClient
{
    Task<DiagnosticInfo> GetDiagAsync();
    Task<PaginatedResponse<UserKarma>> GetKarmaPageAsync(PaginatedParams parameters);
    Task StoreKarmaAsync(List<KarmaItem> items);
    Task InvalidatePinCacheAsync(ulong guildId, ulong channelId);
    Task<byte[]> GetPinsAsync(ulong guildId, ulong channelId, bool markdown);
    Task<Dictionary<string, Cog>> GetSlashCommandsAsync();
}
