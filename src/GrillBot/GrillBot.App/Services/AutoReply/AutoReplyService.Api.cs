using GrillBot.Data.Models.API.AutoReply;

namespace GrillBot.App.Services.AutoReply;

public partial class AutoReplyService
{
    public async Task<List<AutoReplyItem>> GetListAsync(CancellationToken cancellationToken = default)
    {
        using var dbContext = DbFactory.Create();

        var query = dbContext.AutoReplies.AsNoTracking().OrderBy(o => o.Id);
        var data = await query.ToListAsync(cancellationToken);
        return data.ConvertAll(o => new AutoReplyItem(o));
    }

    public async Task<AutoReplyItem> GetItemAsync(long id, CancellationToken cancellationToken)
    {
        using var dbContext = DbFactory.Create();

        var entity = await dbContext.AutoReplies.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        return entity == null ? null : new AutoReplyItem(entity);
    }

    public async Task<AutoReplyItem> CreateItemAsync(AutoReplyItemParams parameters, CancellationToken cancellationToken)
    {
        var entity = new Database.Entity.AutoReplyItem()
        {
            Flags = parameters.Flags,
            Reply = parameters.Reply,
            Template = parameters.Template
        };

        using var dbContext = DbFactory.Create();

        await dbContext.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await InitAsync(cancellationToken);

        return new AutoReplyItem(entity);
    }

    public async Task<AutoReplyItem> UpdateItemAsync(long id, AutoReplyItemParams parameters, CancellationToken cancellationToken)
    {
        using var dbContext = DbFactory.Create();

        var entity = await dbContext.AutoReplies.AsQueryable()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (entity == null)
            return null;

        entity.Template = parameters.Template;
        entity.Flags = parameters.Flags;
        entity.Reply = parameters.Reply;

        await dbContext.SaveChangesAsync(cancellationToken);
        await InitAsync(cancellationToken);

        return new AutoReplyItem(entity);
    }

    public async Task<bool> RemoveItemAsync(long id, CancellationToken cancellationToken)
    {
        using var dbContext = DbFactory.Create();

        var entity = await dbContext.AutoReplies.AsQueryable()
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (entity == null) return false;

        dbContext.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        await InitAsync(cancellationToken);
        return true;
    }
}
