using GrillBot.Data.Models;
using GrillBot.Data.Models.AuditLog;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace GrillBot.Tests.Data.Models.AuditLog
{
    [TestClass]
    public class GuildUpdatedDataTests
    {
        [TestMethod]
        public void EmptyConstructor()
        {
            Assert.IsNull(new GuildUpdatedData().AfkChannel);
        }

        [TestMethod]
        public void Initializer()
        {
            var data = new GuildUpdatedData()
            {
                AfkChannel = new Diff<AuditChannelInfo>(null, null),
                AfkTimeout = new Diff<int>(0, 0),
                BannerChanged = false,
                DefaultMessageNotifications = new Diff<Discord.DefaultMessageNotifications>(default, default),
                Description = new Diff<string>(null, null),
                DiscoverySplashChanged = false,
                IconChanged = false,
                MfaLevel = new Diff<Discord.MfaLevel>(default, default),
                Name = new Diff<string>(),
                Owner = new Diff<AuditUserInfo>(),
                PublicUpdatesChannel = new Diff<AuditChannelInfo>(),
                RulesChannel = new Diff<AuditChannelInfo>(),
                SplashChanged = false,
                SystemChannel = new Diff<AuditChannelInfo>(),
                VanityUrl = new Diff<string>(),
                VoiceRegionId = new Diff<string>()
            };

            foreach (var property in data.GetType().GetProperties())
                property.GetValue(data, null);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Serialization_WithData()
        {
            var item = new GuildUpdatedData()
            {
                AfkChannel = new(),
                AfkTimeout = new(),
                DefaultMessageNotifications = new(),
                Description = new(),
                VanityUrl = new(),
                VoiceRegionId = new(),
                Owner = new(),
                PublicUpdatesChannel = new(),
                RulesChannel = new(),
                SystemChannel = new(),
                Name = new(),
                MfaLevel = new()
            };

            var json = JsonConvert.SerializeObject(item);
            Assert.IsNotNull(json);
        }

        [TestMethod]
        public void Serialization_WithoutData()
        {
            var item = new GuildUpdatedData();
            var json = JsonConvert.SerializeObject(item);
            Assert.IsNotNull(json);
        }
    }
}
