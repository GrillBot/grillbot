using System;

namespace GrillBot.Data.Helpers
{
    public static class CommonHelper
    {
        public static void SuppressException<TException>(Action action) where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException) { /* Ignored */ }
        }
    }
}
