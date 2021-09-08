using GrillBot.Data.Models.API.Common;
using GrillBot.Data.Models.API.Params;
using GrillBot.Database.Entity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Tests.Data.Models.API.Common
{
    [TestClass]
    public class PaginatedResponseTests
    {
        [TestMethod]
        public void Empty()
        {
            TestHelpers.CheckDefaultPropertyValues(new PaginatedResponse<object>());
        }

        [TestMethod]
        public void Create_Empty()
        {
            using var context = TestHelpers.CreateDbContext();
            var query = context.Guilds.AsQueryable();

            var result = PaginatedResponse<Guild>.CreateAsync(query, new PaginatedParams() { Page = 1, PageSize = 20 }, entity => entity).Result;
            Assert.AreEqual(0, result.TotalItemsCount);
        }

        [TestMethod]
        public void Create_Filled()
        {
            using var context = TestHelpers.CreateDbContext();
            context.Add(new Guild() { Name = "Name", Id = "1234" });
            context.SaveChanges();

            var query = context.Guilds.AsQueryable();
            var result = PaginatedResponse<Guild>.CreateAsync(query, new PaginatedParams() { Page = 1, PageSize = 20 }, entity => entity).Result;
            Assert.AreEqual(1, result.TotalItemsCount);
        }

        [TestMethod]
        public void CreateFromList()
        {
            var data = new List<Guild>() { new(), new(), new() };
            var request = new PaginatedParams() { Page = 1, PageSize = 1 };

            var response = PaginatedResponse<Guild>.Create(data, request);
            Assert.IsTrue(response.CanNext);
            Assert.AreEqual(3, response.TotalItemsCount);
        }

        [TestMethod]
        public void CreateEmpty()
        {
            var response = PaginatedResponse<Guild>.CreateEmpty(new PaginatedParams() { Page = 5 });

            Assert.IsFalse(response.CanNext);
        }
    }
}
