using System.ComponentModel.DataAnnotations;

namespace GrillBot.Database.Enums
{
    public enum AuditLogItemType
    {
        /// <summary>
        /// Information text.
        /// </summary>
        [Display(Name = "Informační")]
        Info = 1,

        /// <summary>
        /// Warning text.
        /// </summary>
        [Display(Name = "Varování")]
        Warning = 2,

        /// <summary>
        /// Errors
        /// </summary>
        [Display(Name = "Chyba")]
        Error = 3
    }
}
