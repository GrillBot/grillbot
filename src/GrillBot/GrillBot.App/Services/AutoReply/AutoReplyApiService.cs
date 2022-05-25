using AutoMapper;
using GrillBot.App.Infrastructure;
using GrillBot.Data.Models.API.AutoReply;

namespace GrillBot.App.Services.AutoReply;

public class AutoReplyApiService : ServiceBase
{
    private AutoReplyService AutoReplyService { get; }

    public AutoReplyApiService(AutoReplyService autoReplyService, GrillBotContextFactory dbFactory, IMapper mapper) : base(null, dbFactory, null, mapper)
    {
        AutoReplyService = autoReplyService;
    }

    public async Task<List<AutoReplyItem>> GetListAsync(CancellationToken cancellationToken = default)
    {
        using var dbContext = DbFactory.Create();

        var query = dbContext.AutoReplies.AsNoTracking().OrderBy(o => o.Id);
        var data = await query.ToListAsync(cancellationToken);
        return Mapper.Map<List<AutoReplyItem>>(data);
    }

    public async Task<AutoReplyItem> GetItemAsync(long id, CancellationToken cancellationToken)
    {
        using var dbContext = DbFactory.Create();

        var entity = await dbContext.AutoReplies.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        return Mapper.Map<AutoReplyItem>(entity);
    }

    public async Task<AutoReplyItem> CreateItemAsync(AutoReplyItemParams parameters)
    {
        var entity = new Database.Entity.AutoReplyItem()
        {
            Flags = parameters.Flags,
            Reply = parameters.Reply,
            Template = parameters.Template
        };

        using var dbContext = DbFactory.Create();

        await dbContext.AddAsync(entity);
        await dbContext.SaveChangesAsync();
        await AutoReplyService.InitAsync();

        return Mapper.Map<AutoReplyItem>(entity);
    }

    public async Task<AutoReplyItem> UpdateItemAsync(long id, AutoReplyItemParams parameters)
    {
        using var dbContext = DbFactory.Create();

        var entity = await dbContext.AutoReplies.AsQueryable()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (entity == null)
            return null;

        entity.Template = parameters.Template;
        entity.Flags = parameters.Flags;
        entity.Reply = parameters.Reply;

        await dbContext.SaveChangesAsync();
        await AutoReplyService.InitAsync();

        return Mapper.Map<AutoReplyItem>(entity);
    }

    public async Task<bool> RemoveItemAsync(long id)
    {
        using var dbContext = DbFactory.Create();

        var entity = await dbContext.AutoReplies.AsQueryable()
            .FirstOrDefaultAsync(o => o.Id == id);

        if (entity == null) return false;

        dbContext.Remove(entity);
        await dbContext.SaveChangesAsync();
        await AutoReplyService.InitAsync();
        return true;
    }
}
