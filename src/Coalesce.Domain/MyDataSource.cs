using IntelliTect.Coalesce;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace Coalesce.Domain
{
    public class MyDataSource<T, TContext> : StandardDataSource<T, TContext>
        where T : class, new()
        where TContext : AppDbContext
    {
        public MyDataSource(CrudContext<TContext> context) : base(context)
        {
        }

        public override IQueryable<T> GetQuery(IDataSourceParameters parameters)
        {
            //Console.WriteLine("sdfsdfds");
            //if (new T() is System.Collections.IEnumerable)
            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(typeof(T)))
            {
                //return base.GetQuery(parameters).Where("IsActive");
            }

            return base.GetQuery(parameters);
        }
    }
}
