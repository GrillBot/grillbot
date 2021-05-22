using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GrillBot.App.Helpers
{
    static public class ReflectionHelper
    {
        static public IEnumerable<Type> GetAllEventHandlers()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(o => !o.IsAbstract && typeof(Infrastructure.Handler).IsAssignableFrom(o));
        }

        static public IEnumerable<Type> GetAllReactionEventHandlers()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(o => !o.IsAbstract && typeof(Infrastructure.ReactionEventHandler).IsAssignableFrom(o));
        }
    }
}
