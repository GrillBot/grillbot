using Discord;
using GrillBot.Database.Entity;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.Tests
{
    public enum SomeEnum
    {
        [Description("A")]
        X,

        [Description]
        Y,

        [Localizable(true)]
        Z,

        A,

        [Display(Name = "ABCD")]
        T
    }

    public class TestingGrillBotContext : GrillBotContext
    {
        public TestingGrillBotContext(DbContextOptions options) : base(options)
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
