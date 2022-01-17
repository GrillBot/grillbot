using Discord;
using Discord.Commands;
using GrillBot.Data.Infrastructure.TypeReaders.Implementations;
using System;
using System.Threading.Tasks;

namespace GrillBot.Data.Infrastructure.TypeReaders.TextBased
{
    public class UserTypeReader : TextBasedTypeReader<UserConverter>
    {
        protected override async Task<TypeReaderResult> ProcessAsync(UserConverter converter, string input, ICommandContext context, IServiceProvider provider)
        {
            var result = await converter.ConvertAsync(input);

            if (result != null)
                return TypeReaderResult.FromSuccess(result);

            var reader = new UserTypeReader<IUser>();
            return await reader.ReadAsync(context, input, provider);
        }
    }
}
