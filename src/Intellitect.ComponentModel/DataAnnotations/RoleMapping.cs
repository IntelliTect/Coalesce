using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intellitect.ComponentModel.DataAnnotations
{
    public static class RoleMapping
    {
        // TODO: This should probably be made read only.
        private static ConcurrentDictionary<string, List<string>> _data = new ConcurrentDictionary<string, List<string>>();

        /// <summary>
        /// Adds a mapping. InternalRole is not case sensitive.
        /// </summary>
        /// <param name="internalRole"></param>
        /// <param name="externalRole"></param>
        public static void Add(string internalRole, string externalRole)
        {
            var list = _data.GetOrAdd(internalRole.ToUpper(), new List<string>());

            if (!list.Contains(externalRole)) list.Add(externalRole);

            return;
        }

        /// <summary>
        /// Adds a mapping. InternalRole is not case sensitive.
        /// </summary>
        /// <param name="internalRole"></param>
        /// <param name="externalRole"></param>
        public static void Remove(string internalRole, string externalRole)
        {
            var list = _data.GetOrAdd(internalRole.ToUpper(), new List<string>());

            if (list.Contains(externalRole)) list.Remove(externalRole);

            return;
        }

        /// <summary>
        /// Gets the external mapping from the internal mapping. If not found the internal mapping is returned.
        /// </summary>
        /// <param name="internalRole"></param>
        /// <returns></returns>
        public static IEnumerable<string> Map(string internalRole)
        {
            List<string> result;
            if (_data.TryGetValue(internalRole.ToUpper(), out result))
            {
                return result;
            };
            return new List<string>() { internalRole };
        }

        /// <summary>
        /// Gets a list of all the maps. Add keys are in uppercase.
        /// </summary>
        public static IEnumerable<KeyValuePair<string, List<string>>> AllMaps
        {
            get
            {
                return _data;
            }

        }
    }
}
