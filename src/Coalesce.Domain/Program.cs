using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coalesce.Domain
{
    // *** Workaround for dotnet ef ... won't currently work on class libraries ***
    // http://benjii.me/2016/06/entity-framework-core-migrations-for-class-library-projects/
    public class Program
    {
        public static void Main(string[] args) { }
    }
}
