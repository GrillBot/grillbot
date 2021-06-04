using GrillBot.Data.Exceptions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GrillBot.Tests.Data.Exceptions
{
    [TestClass]
    public class NotFoundExceptionTests
    {
        [TestMethod]
        [ExpectedException(typeof(NotFoundException))]
        public void NotFoundException_Empty()
        {
            throw new NotFoundException();
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException))]
        public void NotFoundException_Message()
        {
            throw new NotFoundException("Hello");
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException))]
        public void NotFoundException_InnerException()
        {
            throw new NotFoundException("Hello", new Exception("Test"));
        }
    }
}
