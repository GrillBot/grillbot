﻿using GrillBot.Cache.Entity;

namespace GrillBot.Cache.Services.Managers;

public class EmoteSuggestionManager
{
    private const int ValidHours = 24;

    private GrillBotCacheBuilder CacheBuilder { get; }
    private static SemaphoreSlim Semaphore { get; }

    static EmoteSuggestionManager()
    {
        Semaphore = new SemaphoreSlim(1);
    }

    public EmoteSuggestionManager(GrillBotCacheBuilder cacheBuilder)
    {
        CacheBuilder = cacheBuilder;
    }

    public async Task<string> InitAsync(string filename, byte[] dataContent)
    {
        var entity = new EmoteSuggestionMetadata
        {
            Filename = filename,
            Id = Guid.NewGuid().ToString(),
            CreatedAt = DateTime.Now,
            DataContent = dataContent
        };

        await Semaphore.WaitAsync();
        try
        {
            await using var repository = CacheBuilder.CreateRepository();
            await repository.EmoteSuggestion.PurgeExpiredAsync(ValidHours);

            await repository.AddAsync(entity);
            await repository.CommitAsync();
        }
        finally
        {
            Semaphore.Release();
        }

        return entity.Id;
    }

    public async Task<(string filename, byte[] dataContent)?> PopAsync(string id)
    {
        await Semaphore.WaitAsync();
        try
        {
            await using var repository = CacheBuilder.CreateRepository();
            await repository.EmoteSuggestion.PurgeExpiredAsync(ValidHours);

            var item = await repository.EmoteSuggestion.FindByIdAsync(id);
            if (item == null) return null;

            return (item.Filename, item.DataContent);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task PurgeExpiredAsync()
    {
        await Semaphore.WaitAsync();
        try
        {
            await using var repository = CacheBuilder.CreateRepository();
            await repository.EmoteSuggestion.PurgeExpiredAsync(ValidHours);
        }
        finally
        {
            Semaphore.Release();
        }
    }
}
