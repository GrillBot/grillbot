using Discord;
using GrillBot.Database.Entity;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
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

        A
    }

    public class TestingGrillBotContext : GrillBotContext
    {
        private readonly ValueConverter JsonConverter = new ValueConverter<List<string>, string>(o => string.Join(";", o), o => o.Split(";", StringSplitOptions.None).ToList());

        public TestingGrillBotContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Unverify>(builder =>
            {
                builder.Property(o => o.Roles).HasConversion(JsonConverter);
                builder.Property(o => o.Channels).HasConversion(JsonConverter);
            });
        }
    }
}
