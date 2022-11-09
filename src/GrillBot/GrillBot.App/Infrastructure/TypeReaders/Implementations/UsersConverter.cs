using GrillBot.Data.Exceptions;

namespace GrillBot.App.Infrastructure.TypeReaders.Implementations;

public class UsersConverter : ConverterBase<IEnumerable<IUser>>
{
    private UserConverter UserConverter { get; }

    public UsersConverter(IServiceProvider provider, IInteractionContext context) : base(provider, context)
    {
        UserConverter = new UserConverter(provider, context);
    }

    public override async Task<IEnumerable<IUser>> ConvertAsync(string value)
    {
        var result = new List<IUser>();

        foreach (var userIdent in value.Split(' '))
        {
            var user = await UserConverter.ConvertAsync(userIdent);
            if (user == null)
                throw new NotFoundException(GetLocalizedText("UserNotFound").FormatWith(userIdent));
            result.Add(user);
        }

        return result;
    }
}
