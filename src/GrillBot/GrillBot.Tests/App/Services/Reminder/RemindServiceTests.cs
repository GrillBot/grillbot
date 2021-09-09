using Discord;
using GrillBot.App.Services.Reminder;
using GrillBot.Database.Services;
using GrillBot.Tests.TestHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.ComponentModel.DataAnnotations;

namespace GrillBot.Tests.App.Services.Reminder
{
    [TestClass]
    public class RemindServiceTests
    {
        private ServiceProvider CreateService(out RemindService service, int minimalTimeMinutes = 0)
        {
            service = null;
            var container = DIHelpers.CreateContainer();

            if (container.GetService<GrillBotContextFactory>() is not TestingGrillBotContextFactory dbFactory)
            {
                Assert.Fail("DbFactory není TestingGrillBotContextFactory.");
                return null;
            }

            var configuration = ConfigHelpers.CreateConfiguration(minimalTimeMinutes);
            service = new RemindService(null, dbFactory, configuration);
            return container;
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void CreateRemind_BeforeNow()
        {
            using var _ = CreateService(out var service);

            try
            {
                service.CreateRemindAsync(null, null, DateTime.MinValue, "", null).Wait();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is ValidationException vEx)
                {
                    Assert.AreEqual("Datum a čas upozornění musí být v budoucnosti.", vEx.Message);
                    throw vEx;
                }

                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void CreateRemind_MinimalTime()
        {
            using var _ = CreateService(out var service, 1);

            try
            {
                var at = DateTime.Now.AddSeconds(10);

                service.CreateRemindAsync(null, null, at, null, null).Wait();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is ValidationException vEx)
                {
                    Assert.AreEqual("Datum a čas upozornění musí být později, než 1 minuta", vEx.Message);
                    throw vEx;
                }

                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void CreateRemind_MinimalTime2()
        {
            using var _ = CreateService(out var service, 6);

            try
            {
                var at = DateTime.Now.AddSeconds(10);

                service.CreateRemindAsync(null, null, at, null, null).Wait();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is ValidationException vEx)
                {
                    Assert.AreEqual("Datum a čas upozornění musí být později, než 6 minut", vEx.Message);
                    throw vEx;
                }

                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void CreateRemind_MinimalTime3()
        {
            using var _ = CreateService(out var service, 2);

            try
            {
                var at = DateTime.Now.AddSeconds(10);

                service.CreateRemindAsync(null, null, at, null, null).Wait();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is ValidationException vEx)
                {
                    Assert.AreEqual("Datum a čas upozornění musí být později, než 2 minuty", vEx.Message);
                    throw vEx;
                }

                throw;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void CreateRemind_RequiredMessage()
        {
            using var _ = CreateService(out var service, 10);

            try
            {
                var at = DateTime.MaxValue;
                service.CreateRemindAsync(null, null, at, null, null).Wait();
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is ValidationException vEx)
                {
                    Assert.AreEqual("Text upozornění je povinný.", vEx.Message);
                    throw vEx;
                }

                throw;
            }
        }

        [TestMethod]
        public void CreateRemind_Success_SamePerson()
        {
            using var _ = CreateService(out var service, 10);

            var fromTo = new Mock<IUser>();
            fromTo.Setup(o => o.Id).Returns(12345);
            fromTo.Setup(o => o.Username).Returns("Username");
            fromTo.Setup(o => o.IsBot).Returns(false);

            var message = new Mock<IMessage>();
            message.Setup(o => o.Id).Returns(123456);

            service.CreateRemindAsync(fromTo.Object, fromTo.Object, DateTime.MaxValue, "Zpráva", message.Object).Wait();
        }

        [TestMethod]
        public void CreateRemind_Success_AnotherPerson()
        {
            using var _ = CreateService(out var service, 10);

            var from = new Mock<IUser>();
            from.Setup(o => o.Id).Returns(12345);
            from.Setup(o => o.Username).Returns("Username");
            from.Setup(o => o.IsBot).Returns(false);

            var to = new Mock<IUser>();
            to.Setup(o => o.Id).Returns(123425);
            to.Setup(o => o.Username).Returns("Username2");
            to.Setup(o => o.IsBot).Returns(false);

            var message = new Mock<IMessage>();
            message.Setup(o => o.Id).Returns(222123456);

            service.CreateRemindAsync(from.Object, to.Object, DateTime.MaxValue, "Zpráva", message.Object).Wait();
        }

        [TestMethod]
        public void GetRemindersCount()
        {
            var user = new Mock<IUser>();
            user.Setup(o => o.Id).Returns(123245);

            using var _ = CreateService(out var service);
            var result = service.GetRemindersCountAsync(user.Object).Result;

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void GetReminders()
        {
            var user = new Mock<IUser>();
            user.Setup(o => o.Id).Returns(12345111);

            using var _ = CreateService(out var service);
            var reminds = service.GetRemindersAsync(user.Object, 0).Result;

            Assert.AreEqual(0, reminds.Count);
        }

        [TestMethod]
        public void Copy_NotFound()
        {
            var user = new Mock<IUser>();
            user.Setup(o => o.Id).Returns(12345111);

            var message = new Mock<IMessage>();
            message.Setup(o => o.Id).Returns(2221123456);

            using var _ = CreateService(out var service);
            service.CopyAsync(message.Object, user.Object).Wait();
        }
    }
}
