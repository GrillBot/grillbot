using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;

namespace GrillBot.Database.Services.Repository;

public sealed class GrillBotRepository : RepositoryBase<GrillBotContext>, IDisposable, IAsyncDisposable
{
    private List<SubRepositoryBase<GrillBotContext>> Repositories { get; set; } = new();

    public GrillBotRepository(GrillBotContext context, ICounterManager counterManager) : base(context, counterManager)
    {
    }

    public ChannelRepository Channel => GetOrCreateRepository<ChannelRepository>();
    public UserRepository User => GetOrCreateRepository<UserRepository>();
    public UnverifyRepository Unverify => GetOrCreateRepository<UnverifyRepository>();
    public RemindRepository Remind => GetOrCreateRepository<RemindRepository>();
    public GuildRepository Guild => GetOrCreateRepository<GuildRepository>();
    public GuildUserRepository GuildUser => GetOrCreateRepository<GuildUserRepository>();
    public InviteRepository Invite => GetOrCreateRepository<InviteRepository>();
    public SearchingRepository Searching => GetOrCreateRepository<SearchingRepository>();
    public SelfUnverifyRepository SelfUnverify => GetOrCreateRepository<SelfUnverifyRepository>();
    public AutoReplyRepository AutoReply => GetOrCreateRepository<AutoReplyRepository>();
    public StatisticsRepository Statistics => GetOrCreateRepository<StatisticsRepository>();
    public ApiClientRepository ApiClientRepository => GetOrCreateRepository<ApiClientRepository>();
    public NicknameRepository Nickname => GetOrCreateRepository<NicknameRepository>();

    private TRepository GetOrCreateRepository<TRepository>() where TRepository : SubRepositoryBase<GrillBotContext>
    {
        var repository = Repositories.OfType<TRepository>().FirstOrDefault();
        if (repository != null)
            return repository;

        repository = Activator.CreateInstance(typeof(TRepository), Context, CounterManager) as TRepository;
        if (repository == null)
            throw new InvalidOperationException($"Error while creating repository {typeof(TRepository).Name}");

        Repositories.Add(repository);
        return repository;
    }

    public void Dispose()
    {
        Context.ChangeTracker.Clear();
        Context.Dispose();

        Repositories.Clear();
        Repositories = null!;
    }

    public async ValueTask DisposeAsync()
    {
        Repositories.Clear();
        Repositories = null!;

        Context.ChangeTracker.Clear();
        await Context.DisposeAsync();
    }
}
