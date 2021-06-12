using System.ComponentModel.DataAnnotations;

namespace GrillBot.Data.Models.API.Params
{
    /// <summary>
    /// Parameters for user note update.
    /// </summary>
    public class UpdateUserNoteParams
    {
        /// <summary>
        /// Content of note.
        /// </summary>
        [MinLength(5, ErrorMessage = "Minimální délka obsahu poznámky je 5 znaků.")]
        public string Content { get; set; }
    }
}
