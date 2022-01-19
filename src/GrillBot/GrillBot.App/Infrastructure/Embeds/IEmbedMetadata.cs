namespace GrillBot.App.Infrastructure.Embeds
{
    // Source: https://github.com/Misha12/Inkluzitron/blob/master/src/Inkluzitron/Contracts/IEmbedMetadata.cs
    public interface IEmbedMetadata
    {
        string EmbedKind { get; }
        bool TryLoadFrom(IReadOnlyDictionary<string, string> values);
        void SaveInto(IDictionary<string, string> destination);
    }
}
