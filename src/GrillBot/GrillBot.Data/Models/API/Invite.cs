﻿using System;

namespace GrillBot.Data.Models.API
{
    public class Invite
    {
        /// <summary>
        /// Invite code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Datetime when invite was created.
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// User that created the invite.
        /// </summary>
        public User Creator { get; set; }

        /// <summary>
        /// Use counter of invite.
        /// </summary>
        public int UsedUsersCount { get; set; }
    }
}
