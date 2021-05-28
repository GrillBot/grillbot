using System.ComponentModel;

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
