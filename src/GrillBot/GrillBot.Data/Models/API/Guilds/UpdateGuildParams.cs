namespace GrillBot.Data.Models.API.Guilds
{
    /// <summary>
    /// Parameters for guild update.
    /// </summary>
    public class UpdateGuildParams
    {
        /// <summary>
        /// Mute role ID
        /// </summary>
        public string MuteRoleId { get; set; }

        /// <summary>
        /// AdminChannelId
        /// </summary>
        public string AdminChannelId { get; set; }
    }
}
