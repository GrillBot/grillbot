using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace GrillBot.Data.Models.API
{
    public class Role
    {
        /// <summary>
        /// Id of role.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Role name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Hexadecimal color of role.
        /// </summary>
        public string Color { get; set; }

        public Role() { }

        public Role(ulong id, string name, Color color)
        {
            Id = id.ToString();
            Name = name;
            Color = color.ToString();
        }

        public Role(IRole role) : this(role.Id, role.Name, role.Color) { }
    }
}
