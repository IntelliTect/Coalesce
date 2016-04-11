using Coalesce.Domain.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coalesce.Domain.Repositories
{
    public static class DevTeamRepository
    {
        private static List<DevTeam> _items;



        public static IQueryable<DevTeam> Items { get
            {
                if (_items == null) Load();
                return _items.AsQueryable();
            }
        }


        public static void Load()
        {
            _items = new List<DevTeam>();
            _items.Add(new DevTeam() { DevTeamId = 1, Name = "Office" });
            _items.Add(new DevTeam() { DevTeamId = 2, Name = "Windows" });
            _items.Add(new DevTeam() { DevTeamId = 3, Name = "Visual Studio" });
            _items.Add(new DevTeam() { DevTeamId = 4, Name = "TFS" });
        }
    }
}
