using PT.PM.Common.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PT.PM.UstPreprocessing
{
    public static class ListenerVisitorUtils
    {
        public static IEnumerable<Type> GetAssemblyAstNodeTypes(params Type[] types)
        {
            return types.SelectMany(type => Assembly.GetAssembly(type).GetTypes())
                .Where(t => t.IsSubclassOf(typeof(UstNode)) && !t.IsAbstract);
        }
    }
}
