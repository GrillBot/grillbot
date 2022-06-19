namespace GrillBot.App.Infrastructure.Embeds;

public abstract class PaginatedMetadataBase : IEmbedMetadata
{
    public abstract string EmbedKind { get; }

    public int Page { get; set; }

    public void SaveInto(IDictionary<string, string> destination)
    {
        Save(destination);

        destination[nameof(Page)] = Page.ToString();
    }

    protected abstract void Save(IDictionary<string, string> destination);
    protected abstract void Reset();

    public bool TryLoadFrom(IReadOnlyDictionary<string, string> values)
    {
        if (!TryLoad(values)) return false;

        if (values.TryGetValue(nameof(Page), out var pageId) && int.TryParse(pageId, out var page))
        {
            Page = page;
            return true;
        }

        Reset();
        return false;
    }

    protected abstract bool TryLoad(IReadOnlyDictionary<string, string> values);
}
