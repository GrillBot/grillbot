using Discord;
using GrillBot.App.Infrastructure.Embeds;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Infrastructure.Embeds
{
    [TestClass]
    public class EmbedMetadataExtensionsTests
    {
        [TestMethod]
        public void WithMetadata_AuthorUrl()
        {
            var embed = new EmbedBuilder()
                .WithAuthor("Author", "https://google.cz");

            var metadata = new Mock<IEmbedMetadata>();
            metadata.Setup(o => o.EmbedKind).Returns("Some");
            var newEmbed = embed.WithMetadata(metadata.Object);

            Assert.IsNotNull(newEmbed);
        }

        [TestMethod]
        public void WithMetadata_ImageUrl()
        {
            var embed = new EmbedBuilder()
                .WithImageUrl("https://google.cz");

            var metadata = new Mock<IEmbedMetadata>();
            metadata.Setup(o => o.EmbedKind).Returns("Image");
            var newEmbed = embed.WithMetadata(metadata.Object);

            Assert.IsNotNull(newEmbed);
        }
    }
}
