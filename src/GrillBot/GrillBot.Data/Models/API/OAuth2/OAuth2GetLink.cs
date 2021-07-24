namespace GrillBot.Data.Models.API.OAuth2
{
    /// <summary>
    /// OAuth2 redirect link data.
    /// </summary>
    public class OAuth2GetLink
    {
        /// <summary>
        /// OAuth2 redirect uri.
        /// </summary>
        public string Url { get; set; }

        public OAuth2GetLink() { }

        public OAuth2GetLink(string url)
        {
            Url = url;
        }
    }
}
