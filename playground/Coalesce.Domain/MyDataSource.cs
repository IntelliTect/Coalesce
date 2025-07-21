using IntelliTect.Coalesce;
using System.Linq;
using IntelliTect.Coalesce.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Coalesce.Domain;

public class MyDataSource<T, TContext> : StandardDataSource<T, TContext>
    where T : class, new()
    where TContext : DbContext
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


public class MyBehaviors<T, TContext> : StandardBehaviors<T, TContext>
    where T : class, new()
    where TContext : DbContext
{
    public MyBehaviors(CrudContext<TContext> context) : base(context)
    {
    }

    public override ItemResult BeforeSave(SaveKind kind, T? oldItem, T item)
    {
        // Do nothing - just testing that our custom behaviors will get injected.
        return base.BeforeSave(kind, oldItem, item);
    }

    public override Task ExecuteDeleteAsync(T item)
    {
        return base.ExecuteDeleteAsync(item);
    }
}
