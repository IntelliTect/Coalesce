using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Utilities
{
    public static class MvcHelper
    {
        private static readonly Dictionary<Type, List<Type>> LoadedClasses = new Dictionary<Type, List<Type>>();

        private static List<Type> GetSubClasses<T>()
        {
            if (!LoadedClasses.ContainsKey(typeof(T)))
            {
                lock (LoadedClasses)
                {
                    if (!LoadedClasses.ContainsKey(typeof(T)))
                    {
                        var list = new List<Type>();
                        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            list.AddRange(asm.GetTypes().Where(
                                type => type.IsSubclassOf(typeof(T))).ToList());
                        }
                        LoadedClasses.Add(typeof(T), list);
                    }
                }
            }
            return LoadedClasses[typeof(T)];
        }

        //public static List<string> GetControllerNames()
        //{
        //    List<string> controllerNames = new List<string>();
        //    GetSubClasses<Controller>().ForEach(
        //        type => controllerNames.Add(type.Name));
        //    return controllerNames;
        //}

        //public static List<string> GetApiControllerNames()
        //{
        //    List<string> controllerNames = new List<string>();
        //    GetSubClasses<ApiController>().ForEach(
        //        type => controllerNames.Add(type.Name));
        //    return controllerNames;
        //}
    }
}
