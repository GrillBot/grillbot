using GrillBot.Database.Entity;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GrillBot.Tests.TestHelpers
{
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
                builder.Property(o => o.Roles).HasConversion(o => string.Join(";", o), o => o.Split(";", StringSplitOptions.None).ToList());
                builder.Property(o => o.Channels).HasConversion(o => JsonConvert.SerializeObject(o), o => JsonConvert.DeserializeObject<List<GuildChannelOverride>>(o));
            });
        }
    }
}
