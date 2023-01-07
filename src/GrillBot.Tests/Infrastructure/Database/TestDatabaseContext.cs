using System.Diagnostics.CodeAnalysis;
using GrillBot.Database.Entity;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GrillBot.Tests.Infrastructure.Database;

[ExcludeFromCodeCoverage]
public class DatabaseContext : GrillBotContext
{
    public DatabaseContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Unverify>(builder =>
        {
            builder.Property(o => o.Roles).HasConversion(o => JsonConvert.SerializeObject(o), o => JsonConvert.DeserializeObject<List<string>>(o));
            builder.Property(o => o.Channels).HasConversion(o => JsonConvert.SerializeObject(o), o => JsonConvert.DeserializeObject<List<GuildChannelOverride>>(o));
        });

        modelBuilder.Entity<ApiClient>(builder => builder.Property(o => o.AllowedMethods).HasConversion(o => JsonConvert.SerializeObject(o), o => JsonConvert.DeserializeObject<List<string>>(o)));
    }
}
