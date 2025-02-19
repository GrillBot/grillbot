using System;

namespace GrillBot.Data.Models.API.Filters;

public record StoredFilterInfo(Guid Id, DateTime ExpiresAtUtc);
