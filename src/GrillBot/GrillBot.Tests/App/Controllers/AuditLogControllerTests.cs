using Discord.WebSocket;
using GrillBot.App.Controllers;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.FileStorage;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GrillBot.Tests.App.Controllers
{
    [TestClass]
    public class AuditLogControllerTests
    {
        [TestMethod]
        public void RemoveItem_Success()
        {
            var discord = new DiscordSocketClient();
            var auditLogService = new Mock<AuditLogService>(new object[] { discord, null, null, null });
            auditLogService.Setup(o => o.RemoveItemAsync(It.IsAny<long>())).Returns(Task.FromResult(true));

            var controller = new AuditLogController(auditLogService.Object, null, null);
            var removeResult = controller.RemoveItemAsync(0).Result;

            Assert.IsInstanceOfType(removeResult, typeof(OkResult));
        }

        [TestMethod]
        public void RemoveItem_NotFound()
        {
            var discord = new DiscordSocketClient();
            var auditLogService = new Mock<AuditLogService>(new object[] { discord, null, null, null });
            auditLogService.Setup(o => o.RemoveItemAsync(It.IsAny<long>())).Returns(Task.FromResult(false));

            var controller = new AuditLogController(auditLogService.Object, null, null);
            var removeResult = controller.RemoveItemAsync(0).Result;

            Assert.IsInstanceOfType(removeResult, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public void GetAuditLogList()
        {
            using var container = TestHelper.DIHelpers.CreateContainer();
            var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));

            var controller = new AuditLogController(null, dbContext, null);
            var result = controller.GetAuditLogListAsync(new()).Result;

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public void GetFileContent_NotFound()
        {
            using var container = TestHelper.DIHelpers.CreateContainer();
            var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
            dbContext.AuditLogs.RemoveRange(dbContext.AuditLogs.ToList());
            dbContext.SaveChanges();

            var controller = new AuditLogController(null, dbContext, null);
            var result = controller.GetFileContentAsync(0, 0).Result;
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public void GetFileContent_NoFile()
        {
            using var container = TestHelper.DIHelpers.CreateContainer();
            var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
            dbContext.AuditLogs.RemoveRange(dbContext.AuditLogs.ToList());
            dbContext.AuditLogs.Add(new AuditLogItem()
            {
                Id = 1,
            });
            dbContext.SaveChanges();

            var controller = new AuditLogController(null, dbContext, null);
            var result = controller.GetFileContentAsync(1, 0).Result;
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public void GetFileContent_FileNotExists()
        {
            using var container = TestHelper.DIHelpers.CreateContainer();
            var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
            dbContext.AuditLogs.RemoveRange(dbContext.AuditLogs.ToList());
            dbContext.AuditLogFiles.RemoveRange(dbContext.AuditLogFiles.ToList());
            var item = new AuditLogItem() { Id = 1 };
            var currentLocation = Assembly.GetExecutingAssembly().Location;
            item.Files.Add(new AuditLogFileMeta()
            {
                Id = 1,
                Filename = Path.GetFileName(currentLocation)
            });
            dbContext.AuditLogs.Add(item);
            dbContext.SaveChanges();

            var configuration = TestHelper.ConfigHelpers.CreateConfiguration(0, new System.Collections.Generic.Dictionary<string, string>()
            {
                { "FileStorage:Audit:Location", Path.GetDirectoryName(currentLocation) }
            });

            var fileStorageFactory = new FileStorageFactory(configuration);

            var controller = new AuditLogController(null, dbContext, fileStorageFactory);
            var result = controller.GetFileContentAsync(1, 1).Result;
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public void GetFileContent_Success()
        {
            using var container = TestHelper.DIHelpers.CreateContainer();
            var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
            dbContext.AuditLogs.RemoveRange(dbContext.AuditLogs.ToList());
            dbContext.AuditLogFiles.RemoveRange(dbContext.AuditLogFiles.ToList());
            var item = new AuditLogItem() { Id = 1 };
            var currentLocation = Assembly.GetExecutingAssembly().Location;
            item.Files.Add(new AuditLogFileMeta()
            {
                Id = 1,
                Filename = Path.GetFileName(currentLocation)
            });
            dbContext.AuditLogs.Add(item);
            dbContext.SaveChanges();

            var currentDirectory = Path.GetDirectoryName(currentLocation);
            File.Copy(currentLocation, Path.Combine(currentDirectory, "DeletedAttachments", Path.GetFileName(currentLocation)), true);
            var configuration = TestHelper.ConfigHelpers.CreateConfiguration(0, new System.Collections.Generic.Dictionary<string, string>()
            {
                { "FileStorage:Audit:Location", currentDirectory }
            });

            var fileStorageFactory = new FileStorageFactory(configuration);

            var controller = new AuditLogController(null, dbContext, fileStorageFactory);
            var result = controller.GetFileContentAsync(1, 1).Result;
            Assert.IsInstanceOfType(result, typeof(FileContentResult));
            File.Delete(Path.Combine(currentDirectory, "DeletedAttachments", Path.GetFileName(currentLocation)));
        }

        [TestMethod]
        public void GetFileContent_RandomContentType()
        {
            var currentLocation = Assembly.GetExecutingAssembly().Location;
            var currentDirectory = Path.GetDirectoryName(currentLocation);
            var filename = Path.Combine(Path.Combine(currentDirectory, "DeletedAttachments"), "randomfile.jpg");

            try
            {
                File.WriteAllBytes(filename, new byte[] { 0, 0, 0, 0, 0, 0, 0 });

                using var container = TestHelper.DIHelpers.CreateContainer();
                var dbContext = (GrillBotContext)container.GetService(typeof(TestingGrillBotContext));
                dbContext.AuditLogs.RemoveRange(dbContext.AuditLogs.ToList());
                dbContext.AuditLogFiles.RemoveRange(dbContext.AuditLogFiles.ToList());
                var item = new AuditLogItem() { Id = 1 };
                item.Files.Add(new AuditLogFileMeta()
                {
                    Id = 1,
                    Filename = Path.GetFileName(filename)
                });
                dbContext.AuditLogs.Add(item);
                dbContext.SaveChanges();

                var configuration = TestHelper.ConfigHelpers.CreateConfiguration(0, new System.Collections.Generic.Dictionary<string, string>()
                {
                    { "FileStorage:Audit:Location", currentDirectory }
                });

                var fileStorageFactory = new FileStorageFactory(configuration);

                var controller = new AuditLogController(null, dbContext, fileStorageFactory);
                var result = controller.GetFileContentAsync(1, 1).Result;
                Assert.IsInstanceOfType(result, typeof(FileContentResult));
            }
            finally
            {
                File.Delete(filename);
            }
        }
    }
}
