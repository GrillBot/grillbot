﻿using Discord.Commands;
using GrillBot.Data.Infrastructure.TypeReaders.Implementations;
using System;
using System.Threading.Tasks;

namespace GrillBot.Data.Infrastructure.TypeReaders.TextBased;

public abstract class TextBasedTypeReader<TConverter> : TypeReader where TConverter : ConverterBase
{
    public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
    {
        var converter = CreateConverter(context, services);
        return ProcessAsync(converter, input, context, services);
    }

    private static TConverter CreateConverter(ICommandContext context, IServiceProvider services)
        => (TConverter)Activator.CreateInstance(typeof(TConverter), new object[] { services, context });

    protected abstract Task<TypeReaderResult> ProcessAsync(TConverter converter, string input, ICommandContext context, IServiceProvider provider);
}
