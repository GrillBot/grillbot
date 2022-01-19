namespace GrillBot.App.Helpers
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
