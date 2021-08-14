using Discord;
using GrillBot.Database.Entity;
using GrillBot.Database.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

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

    public static class TestHelpers
    {
        public static void CheckDefaultPropertyValues<TClass>(TClass item, Action<object, object, string> checker)
        {
            foreach (var property in item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Where(o => o.CanRead))
            {
                var value = property.GetValue(item, null);
                var defaultValue = property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null;

                checker(defaultValue, value, property.Name);
            }
        }

        public static TestingGrillBotContext CreateDbContext()
        {
            var opt = new DbContextOptionsBuilder()
                .UseInMemoryDatabase("GrillBot");

            return new TestingGrillBotContext(opt.Options);
        }
    }
}
