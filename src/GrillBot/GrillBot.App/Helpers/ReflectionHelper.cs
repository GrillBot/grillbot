using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GrillBot.Data.Helpers
{
    static public class ReflectionHelper
    {
        private static Type[] AssemblyTypes { get; }

        static ReflectionHelper()
        {
            AssemblyTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(o => o.IsClass && !o.IsAbstract).ToArray();
        }

        static public IEnumerable<Type> GetAllInternalServices()
        {
            return AssemblyTypes.Where(o => !o.IsAbstract && typeof(Infrastructure.ServiceBase).IsAssignableFrom(o));
        }

        static public IEnumerable<Type> GetAllReactionEventHandlers()
        {
            return AssemblyTypes.Where(o => !o.IsAbstract && typeof(Infrastructure.ReactionEventHandler).IsAssignableFrom(o));
        }
    }
}
