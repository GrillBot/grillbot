using Discord;
using GrillBot.App.Infrastructure.Embeds;
using GrillBot.Tests.TestHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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

        [TestMethod]
        public void TryParseMetadata_AuthorIconUrl()
        {
            var metadata = new EmbedMetadata() { LoadResult = true };
            var embed = new EmbedBuilder()
                .WithAuthor("d", "http://test.com")
                .WithMetadata(metadata)
                .Build();

            var result = embed.TryParseMetadata<EmbedMetadata>(out var _);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TryParseMetadata_ImageUrl()
        {
            var metadata = new EmbedMetadata() { LoadResult = true };
            var embed = new EmbedBuilder()
                .WithImageUrl("http://test.com")
                .WithMetadata(metadata)
                .Build();

            var result = embed.TryParseMetadata<EmbedMetadata>(out var _);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TryParseMetadata_FooterIconUrl()
        {
            var metadata = new EmbedMetadata() { LoadResult = true };
            var embed = new EmbedBuilder()
                .WithFooter("X", "http://test.com")
                .WithMetadata(metadata)
                .Build();

            var result = embed.TryParseMetadata<EmbedMetadata>(out var _);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TryParseMetadata_InvalidUrl()
        {
            var embed = new EmbedBuilder()
                .Build();

            var result = embed.TryParseMetadata<EmbedMetadata>(out var _);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryParseMetadata_InvalidKind()
        {
            var metadata = new EmbedMetadata() { LoadResult = true };
            var embed = new EmbedBuilder()
                .WithFooter("X", "http://test.com")
                .WithMetadata(metadata);

            embed.Footer.IconUrl = embed.Footer.IconUrl.Replace("_k=Embed", "_k=Embeds");
            var result = embed.Build().TryParseMetadata<EmbedMetadata>(out var _);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryParseMetadata_MissingKind()
        {
            var metadata = new EmbedMetadata() { LoadResult = true };
            var embed = new EmbedBuilder()
                .WithFooter("X", "http://test.com")
                .WithMetadata(metadata);

            embed.Footer.IconUrl = embed.Footer.IconUrl.Replace("&_k=Embed", "");
            var result = embed.Build().TryParseMetadata<EmbedMetadata>(out var _);
            Assert.IsFalse(result);
        }
    }
}
