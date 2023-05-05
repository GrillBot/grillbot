using System.Net.Http;
using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Common.FileStorage;
using GrillBot.Common.Services.FileService;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
public class AuditLogFilesMigrationJob : Job
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private FileStorageFactory FileStorageFactory { get; }
    private IFileServiceClient FileServiceClient { get; }
    private IContentTypeProvider ContentTypeProvider { get; }

    public AuditLogFilesMigrationJob(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        DatabaseBuilder = serviceProvider.GetRequiredService<GrillBotDatabaseBuilder>();
        FileStorageFactory = serviceProvider.GetRequiredService<FileStorageFactory>();
        FileServiceClient = serviceProvider.GetRequiredService<IFileServiceClient>();
        ContentTypeProvider = new FileExtensionContentTypeProvider();
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var fileStorage = FileStorageFactory.Create("Audit");

        await using var repository = DatabaseBuilder.CreateRepository();

        var files = await repository.AuditLog.GetAllFilesAsync();
        var uploaded = 0;
        
        foreach (var file in files)
        {
            var fileInfo = await fileStorage.GetFileInfoAsync("DeletedAttachments", file.Filename);
            if (!fileInfo.Exists) continue;

            if (!ContentTypeProvider.TryGetContentType(fileInfo.Name, out var contentType)) continue;

            var content = await File.ReadAllBytesAsync(fileInfo.FullName, context.CancellationToken);
            try
            {
                await FileServiceClient.UploadFileAsync(fileInfo.Name, content, contentType);
                uploaded++;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
            }
        }

        context.Result = $"Finished. Files for migration: {files.Count}. Migrated: {uploaded}";
    }
}
