using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace GrillBot.App.Infrastructure.TypeReaders.Implementations;

public class GuidConverter : ConverterBase<Guid?>
{
    public GuidConverter(IServiceProvider provider, ICommandContext context) : base(provider, context)
    {
    }

    public override Task<Guid?> ConvertAsync(string value)
    {
        return Task.FromResult<Guid?>(Guid.TryParse(value, out var guid) ? guid : null);
    }
}
