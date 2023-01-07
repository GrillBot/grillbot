using Discord;
using System;
using System.Collections.Generic;

namespace GrillBot.Data.Models.API.Users;

public class UserListItem
{
    public string Id { get; set; }
    public int Flags { get; set; }
    public bool HaveBirthday { get; set; }
    public string Username { get; set; }
    public UserStatus DiscordStatus { get; set; }
    public DateTime? RegisteredAt { get; set; }

    /// <summary>
    /// Guild names where user is/was.
    /// </summary>
    public Dictionary<string, bool> Guilds { get; set; } = new();
}
