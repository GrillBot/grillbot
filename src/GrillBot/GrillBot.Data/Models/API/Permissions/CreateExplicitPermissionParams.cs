using GrillBot.Database.Enums;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Data.Models.API.Permissions
{
    public class CreateExplicitPermissionParams
    {
        [Required]
        public string Command { get; set; }

        public bool IsRole { get; set; }

        public string TargetId { get; set; }

        public ExplicitPermissionState State { get; set; }
    }
}
