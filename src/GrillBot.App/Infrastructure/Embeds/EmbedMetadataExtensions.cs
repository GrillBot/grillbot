using System.Collections.Specialized;
using System.Web;

namespace GrillBot.App.Infrastructure.Embeds;

// Source: https://github.com/Misha12/Inkluzitron/blob/master/src/Inkluzitron/Extensions/EmbedMetadataExtensions.cs
// Credits to Khub
public static class EmbedMetadataExtensions
{
    /// <summary>
    /// <para>Adjusts the embed under construction so that it includes recoverable representation of <paramref name="embedMetadata"/>.</para>
    /// <para>For optimal results, call this method after EmbedBuilder.WithAuthor" or <see cref="EmbedBuilder.WithImageUrl"/>.</para>
    /// </summary>
    /// <param name="embedBuilder"></param>
    /// <param name="embedMetadata">The metadata to store inside the embed.</param>
    /// <returns></returns>
    public static EmbedBuilder WithMetadata(this EmbedBuilder embedBuilder, IEmbedMetadata embedMetadata)
    {
        if (embedBuilder.Author?.IconUrl is { } authorIconUrl)
        {
            authorIconUrl = StealthInto(authorIconUrl, embedMetadata);
            return embedBuilder.WithAuthor(embedBuilder.Author.Name, authorIconUrl, embedBuilder.Author.Url);
        }

        switch (embedBuilder.ImageUrl)
        {
            case { } imageUrl:
                imageUrl = StealthInto(imageUrl, embedMetadata);
                return embedBuilder.WithImageUrl(imageUrl);
            default:
            {
                var footerIcon = StealthInto(embedBuilder.Footer.IconUrl, embedMetadata);
                return embedBuilder.WithFooter(embedBuilder.Footer.Text, footerIcon);
            }
        }
    }

    /// <summary>
    /// Serializes the provided instance of <paramref name="embedMetadata"/> and returns an updated version of <paramref name="uri"/>
    /// that contains the serialized string in the URI fragment.
    /// </summary>
    private static string StealthInto(string uri, IEmbedMetadata embedMetadata)
    {
        var oldUri = new Uri(uri);
        var existingFragmentData = HttpUtility.ParseQueryString(oldUri.Fragment);
        var newFragment = SerializeMetadata(embedMetadata, existingFragmentData);
        var newUriBuilder = new UriBuilder(uri) { Fragment = newFragment };
        return newUriBuilder.ToString();
    }

    /// <summary>
    /// Serializes the provided instance of <paramref name="embedMetadata"/> into a URL query string-ish representation.
    /// </summary>
    private static string SerializeMetadata(IEmbedMetadata embedMetadata, NameValueCollection? existingFragmentData = null)
    {
        var fragmentData = existingFragmentData ?? new NameValueCollection();
        var fragmentDict = new Dictionary<string, string>();

        // Update it with the required values
        embedMetadata.SaveInto(fragmentDict);
        foreach (var (key, value) in fragmentDict)
            fragmentData[key] = value;

        fragmentData["_k"] = embedMetadata.EmbedKind;

        // Make a list of query pairs
        var keyValuePairs = fragmentData.AllKeys
            .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)
            .Select(key => Escape(key!) + "=" + Escape(fragmentData[key]!));

        return string.Join("&", keyValuePairs);
    }

    private static string Escape(string s) => Uri.EscapeDataString(s);

    /// <summary>
    /// Attempts to recover <typeparamref name="TMetadata"/> from the provided <paramref name="embedMetadata"/>
    /// </summary>
    /// <typeparam name="TMetadata">The type of metadata to look for and restore.</typeparam>
    /// <param name="embed">The embed to search for serialized metadata.</param>
    /// <param name="embedMetadata">The recovered metadata.</param>
    /// <returns>Whether an instance of <typeparamref name="TMetadata"/> was found and recovered from the embed.</returns>
    public static bool TryParseMetadata<TMetadata>(this IEmbed embed, out TMetadata embedMetadata)
        where TMetadata : IEmbedMetadata, new()
    {
        embedMetadata = new TMetadata();

        NameValueCollection metadata;
        var sourceUrl = embed.Author?.IconUrl ?? embed.Image?.Url;
        if (sourceUrl != null)
            metadata = HttpUtility.ParseQueryString(new UriBuilder(sourceUrl).Fragment.TrimStart('#'));
        else if (embed.Footer?.IconUrl is { } footerUrl)
            metadata = HttpUtility.ParseQueryString(new UriBuilder(footerUrl).Fragment.TrimStart('#'));
        else
            return false;

        var fragmentDict = new Dictionary<string, string>();
        foreach (var key in metadata.AllKeys.Where(o => o != null))
            fragmentDict[key!] = metadata[key]!;

        if (!fragmentDict.TryGetValue("_k", out var embedKind))
            return false;
        if (!embedKind.Equals(embedMetadata.EmbedKind))
            return false;

        fragmentDict.Remove("_k");
        return embedMetadata.TryLoadFrom(fragmentDict);
    }
}
