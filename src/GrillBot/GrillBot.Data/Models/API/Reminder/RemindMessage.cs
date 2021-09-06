using GrillBot.Data.Models.API.Users;
using System;

namespace GrillBot.Data.Models.API.Reminder
{
    public class RemindMessage
    {
        public long Id { get; set; }
        public User FromUser { get; set; }
        public User ToUser { get; set; }
        public DateTime At { get; set; }
        public string Message { get; set; }
        public int Postpone { get; set; }

        public RemindMessage() { }

        public RemindMessage(Database.Entity.RemindMessage entity)
        {
            Id = entity.Id;
            FromUser = new User(entity.FromUser);
            ToUser = new User(entity.ToUser);
            At = entity.At;
            Message = entity.Message;
            Postpone = entity.Postpone;
        }
    }
}
