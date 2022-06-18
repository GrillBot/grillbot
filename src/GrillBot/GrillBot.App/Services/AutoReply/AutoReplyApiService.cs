using AutoMapper;
using GrillBot.Data.Models.API.AutoReply;

namespace GrillBot.App.Services.AutoReply;

public class AutoReplyApiService
{
    private AutoReplyService AutoReplyService { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IMapper Mapper { get; }

    public AutoReplyApiService(AutoReplyService autoReplyService, GrillBotDatabaseBuilder databaseBuilder, IMapper mapper)
    {
        AutoReplyService = autoReplyService;
        DatabaseBuilder = databaseBuilder;
        Mapper = mapper;
    }

    public async Task<List<AutoReplyItem>> GetListAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var data = await repository.AutoReply.GetAllAsync();
        return Mapper.Map<List<AutoReplyItem>>(data);
    }

    public async Task<AutoReplyItem> GetItemAsync(long id)
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var entity = await repository.AutoReply.FindReplyByIdAsync(id);

        return Mapper.Map<AutoReplyItem>(entity);
    }

    public async Task<AutoReplyItem> CreateItemAsync(AutoReplyItemParams parameters)
    {
        var entity = new Database.Entity.AutoReplyItem
        {
            Flags = parameters.Flags,
            Reply = parameters.Reply,
            Template = parameters.Template
        };

        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.AddAsync(entity);
        await repository.CommitAsync();
        await AutoReplyService.InitAsync();

        return Mapper.Map<AutoReplyItem>(entity);
    }

    public async Task<AutoReplyItem> UpdateItemAsync(long id, AutoReplyItemParams parameters)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var entity = await repository.AutoReply.FindReplyByIdAsync(id);
        if (entity == null)
            return null;

        entity.Template = parameters.Template;
        entity.Flags = parameters.Flags;
        entity.Reply = parameters.Reply;

        await repository.CommitAsync();
        await AutoReplyService.InitAsync();

        return Mapper.Map<AutoReplyItem>(entity);
    }

    public async Task<bool> RemoveItemAsync(long id)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var entity = await repository.AutoReply.FindReplyByIdAsync(id);
        if (entity == null) return false;

        repository.Remove(entity);
        await repository.CommitAsync();
        await AutoReplyService.InitAsync();

        return true;
    }
}
