using GrillBot.Data.Models.API.Users;

namespace GrillBot.Data.Models.API.Permissions
{
    public class ExplicitPermission
    {
        public string Command { get; set; }

        public User User { get; set; }
        public Role Role { get; set; }

        public ExplicitPermission() { }

        public ExplicitPermission(Database.Entity.ExplicitPermission entity, Database.Entity.User user, Role role)
        {
            Command = entity.Command;
            User = user == null ? null : new User(user);
            Role = role;
        }
    }
}
