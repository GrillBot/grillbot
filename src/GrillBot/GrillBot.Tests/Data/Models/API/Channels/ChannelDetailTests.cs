﻿using GrillBot.Data.Models.API.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GrillBot.Tests.Data.Models.API.Channels
{
    [TestClass]
    public class ChannelDetailTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            TestHelpers.CheckDefaultPropertyValues(new ChannelDetail());
        }

        [TestMethod]
        public void FilledConstructor()
        {
            var entity = new GrillBot.Database.Entity.GuildChannel()
            {
                ChannelId = "Id",
                ChannelType = Discord.ChannelType.Category,
                Flags = 50,
                Guild = new(),
                GuildId = "Id",
                Name = "Name"
            };

            var detail = new ChannelDetail(entity)
            {
                FirstMessageAt = DateTime.MaxValue,
                MostActiveUser = new(),
                MessagesCount = 50,
                LastMessageFrom = new(),
                LastMessageAt = DateTime.UtcNow
            };

            TestHelpers.CheckNonDefaultPropertyValues(detail);
        }
    }
}