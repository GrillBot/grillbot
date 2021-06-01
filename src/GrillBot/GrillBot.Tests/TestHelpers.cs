using Discord;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Threading.Tasks;

namespace GrillBot.Tests
{
    public enum SomeEnum
    {
        [Description("A")]
        X,

        [Description]
        Y,

        [Localizable(true)]
        Z,

        A
    }
}
